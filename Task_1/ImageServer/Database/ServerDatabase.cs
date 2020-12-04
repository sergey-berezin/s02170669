using Microsoft.EntityFrameworkCore;
using SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecognitionModel;
using System.IO;
using ImageContracts;

namespace ImageServer.Database
{
    public interface IImageDB
    {
        List<ImageRepresentation> GetStatistics();
        List<ImageRepresentation> GetAllImages();
        List<ImageRepresentation> RecognizeImage(List<ImageRepresentation> Images);

        string ClearDatabase();

    }
    public class ServerDatabase : IImageDB
    {
        public List<ImageRepresentation> GetStatistics()
        {
            List<ImageRepresentation> ImageList = new List<ImageRepresentation>();
            using (var db = new LibraryContext())
            {
                foreach (var img in db.Images)
                {
                    ImageList.Add(new ImageRepresentation
                    {
                        ImageId = img.ImageInfoId,
                        ImageName = img.ImageName,
                        ClassName = img.ClassName,
                        NumOfRequests = img.NumOfRequests
                    });
                }
            }
            // Important! Return clean statictics without ByteImage 
            return ImageList;
        }

        public List<ImageRepresentation> GetAllImages()
        {
            List<ImageRepresentation> ImageList = new List<ImageRepresentation>();
            using (var db = new LibraryContext())
            {
                foreach (var img in db.Images.Include(a => a.ByteImage))
                {
                    ImageList.Add(new ImageRepresentation
                    {   
                        ImageId = img.ImageInfoId,
                        ImageName=img.ImageName,
                        ClassName=img.ClassName,
                        NumOfRequests=img.NumOfRequests,
                        ImageHash=img.ImageHash,
                        Prob=img.Prob,
                        Base64Image=Convert.ToBase64String(img.ByteImage.Img) 
                    });
                } 
            }
            // Important! Return all images including ByteImage arrays
            return ImageList;
        }
        public List<ImageRepresentation> RecognizeImage(List<ImageRepresentation> Images)
        {
            Model MnistModel = new Model(ModelPath: Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Task_1\\RecognitionModel\\mnist-8.onnx");

            Parallelizer<ImageRepresentation, ImageRepresentation> ModelParallelizer = new Parallelizer<ImageRepresentation, ImageRepresentation>(MnistModel, UseServer:true);
            List<ImageRepresentation> result = ModelParallelizer.Run(Images);
            foreach(var l in result)
                Console.WriteLine(l.ClassName, l.Prob);
            return result;
        }

        public string ClearDatabase()
        {
            try
            {
                using (var db = new LibraryContext())
                {
                    db.ImageClasses.RemoveRange(db.ImageClasses);
                    db.Images.RemoveRange(db.Images);
                    db.SaveChanges();
                    return "Database cleared";
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
