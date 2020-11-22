﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Collections.Concurrent;
using SQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RecognitionModel
{
    public class Model: IProcess
    {

        string modelpath;
        string inputnodename;
        InferenceSession session;

        public string ModelPath
        {
            get
            {
                return modelpath;
            }
            set
            {
                modelpath=value;
            }
        }
        public string InputNodeName
        {
            get
            {
                return inputnodename;
            }
            set
            {
                inputnodename=value;
            }
        }
        public Model(string ModelPath="mnist-8.onnx", string InputNodeName="Input3")
        {
            this.modelpath = ModelPath;
            this.inputnodename = InputNodeName;
            this.session = new InferenceSession(modelpath);
        }

        public object ProcessFile(object obj)
        {
            string ImgPath = obj as string;

            byte[] ByteImage = File.ReadAllBytes(ImgPath);
            string hash = Hash.GetHash(ByteImage);

            using (var db = new LibraryContext())
            {

                foreach (var imageclass in db.ImageClasses.Include(a => a.Images))
                {
                    foreach (var img in imageclass.Images)
                        if (Hash.VerifyHash(hash, img.ImageHash))
                        {
                            db.Entry(img).Reference(a => a.ByteImage).Load();
                            if (Hash.ByteArrayCompare(ByteImage, img.ByteImage.Img))
                            {
                                img.NumOfRequests += 1;
                                SaveDataBaseConcurrent(db, null);
                                
                                return (imageclass.ClassName, img.Prob);
                            }
                        }
                }

            }
            using Image<Rgb24> image =  Image.Load<Rgb24>(ByteImage);

            const int TargetWidth = 28;
            const int TargetHeight = 28;

            // Resize image to the 28 x 28
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(TargetWidth, TargetHeight),
                    Mode = ResizeMode.Crop
                });
                x.Grayscale();
            });

            // Create tensor of shape (batch-size, channels, height, width) and normalize the image
            var input = new DenseTensor<float>(new[] { 1, 1, TargetHeight, TargetWidth });
            for (int y = 0; y < TargetHeight; y++)         
                for (int x = 0; x < TargetWidth; x++)
                    input[0, 0, y, x] = image[x,y].R / 255f;

            // Create the inputs to the model
            var inputs = new List<NamedOnnxValue>  
            { 
                NamedOnnxValue.CreateFromTensor(inputnodename, input) 
            };

            // Run NNet  
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            // Softmax calculation
            var output = results.First().AsEnumerable<float>().ToArray();
            var sum = output.Sum(x => (float)Math.Exp(x));
            var softmax = output.Select(x => (float)Math.Exp(x) / sum);

            float maxValue = softmax.Max();
            string maxClass = softmax.ToList().IndexOf(maxValue).ToString();

            using (var db = new LibraryContext())
            {
                var NewImage = new ImageInfo() { ClassName = maxClass, Prob = maxValue, Path = ImgPath, NumOfRequests = 0, ImageHash =hash, ByteImage = new ImageFile { Img = ByteImage } };
                NewImage.ImageClasses = new List<ImageClass>();

                var ClassNum = db.ImageClasses.Where(a => a.ClassName == maxClass).FirstOrDefault();

                if (ClassNum != null)
                {
                    NewImage.ImageClasses.Add(ClassNum);
                    ClassNum.Images = new List<ImageInfo>();
                    ClassNum.Images.Add(NewImage);

                    db.Add(NewImage);
                }
                else
                {
                    var NewClass = new ImageClass() { ClassName = maxClass};

                    NewImage.ImageClasses.Add(NewClass);

                    NewClass.Images = new List<ImageInfo>();
                    NewClass.Images.Add(NewImage);

                    db.Add(NewClass);
                    db.Add(NewImage);

                }
                db.SaveChanges();
            }

            return (maxClass, maxValue);
            
        }

        public void ClearDataBase()
        {
            using (var db = new LibraryContext())
            {
                db.ImageClasses.RemoveRange(db.ImageClasses.AsEnumerable());
                db.Images.RemoveRange(db.Images.AsEnumerable());
                db.SaveChanges();
            }
        }

        public List<ImageInfo> GetModelStatistics()
        {
            List <ImageInfo> ImageList = new List<ImageInfo>();
            using (var db = new LibraryContext())
            {
                foreach(var img in db.Images)
                {
                    ImageList.Add(img);
                }
            }
            return ImageList;
        }

        static private void SaveDataBaseConcurrent(LibraryContext db, IReadOnlyList<EntityEntry> entries)
        {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry.Entity is ImageInfo)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();

                        proposedValues["NumOfRequests"] = (int)databaseValues["NumOfRequests"] + 1;

                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
            }
            try
            {
                db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException exc)
            {
                SaveDataBaseConcurrent(db, exc.Entries);
            }
        }

    }
    
}
