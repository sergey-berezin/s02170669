using System;
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

namespace RecognitionModel
{
    public delegate void OutputHandler(object sender, OutputHandlerEventArgs args);
    public class OutputHandlerEventArgs: EventArgs
    {
        public string Name{get; set;}
        public int Class{get; set;}
        public float Probability{get;set;}

        public OutputHandlerEventArgs(string Name, int Class, float Prob)
        {
            this.Name = Name;
            this.Class = Class;
            this.Probability = Prob;

        }
        public override string ToString()
        {
            return $"{Name} belongs to class {Class} with prob. - {Probability}";
        }

    }

    public class Model
    {

        string modelpath;
        string inputnodename;
        InferenceSession session;
        static CancellationTokenSource cts;
        public event OutputHandler OutputEvent;
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
        public Model(string ModelPath="mnist-8.onnx", string InputNodeName="Input3")
        {
            this.modelpath = ModelPath;
            this.inputnodename = InputNodeName;
            this.session = new InferenceSession(modelpath);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
        }
        static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            cts.Cancel();
        }
        public void Stop()
        {
            cts.Cancel();
        }

        public (int class_number, float probability) ProcessFile(object obj)
        {
            string ImgPath = obj as string;
            Image<Rgb24> image;

            try
            {
                image = Image.Load<Rgb24>(ImgPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.GetType()+" : "+ex.Message);
                return (-1,-1);
            }
            
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
            int maxIndex = softmax.ToList().IndexOf(maxValue);

            return (maxIndex, maxValue);
            
        }

        void ProcessDirectory(string ImgPath)
        {
            cts = new CancellationTokenSource();

            IEnumerable<string> Imgs = Directory.EnumerateFiles(ImgPath);
            using IEnumerator<string> ImgP = Imgs.GetEnumerator();

            Thread[] threads = new Thread[Environment.ProcessorCount];

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i] = new Thread(()=>
                {
                    string Path;

                    while(true)
                    {
                        lock(ImgP)
                        {
                            if(!ImgP.MoveNext() || cts.Token.IsCancellationRequested){
                                // Console.ForegroundColor = ConsoleColor.Red;
                                // Console.WriteLine("*********THE THREAD WAS STOPPED*********");
                                // Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                            else
                                Path=ImgP.Current;

                        }

                        (int class_num, float prob) = ProcessFile(Path);
                        OutputEvent?.Invoke(this, new OutputHandlerEventArgs(Path, class_num, prob));
                    }


                });
                threads[i].Start();
            }

             for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i].Join();
            }
            

        }
        public void Recognize(string ImgPath)
        {
            if(File.Exists(ImgPath))
            {
                // This path is a file
                ProcessFile(ImgPath);
            }
            else if(Directory.Exists(ImgPath))
            {
                // This path is a directory
                ProcessDirectory(ImgPath);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", ImgPath);
            }
        }

        public static void ConsolePrint(object sender, OutputHandlerEventArgs args)
        {
            Console.WriteLine(args);
        }


    }
    
}
