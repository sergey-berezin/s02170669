using System;
using System.Diagnostics;
using RecognitionModel;
using System.Linq;
using SQL;
using Microsoft.EntityFrameworkCore;

namespace Main
{
    class Program
    {
        public static void ConsoleOutput(object sender, string input, (string, float) result)
        {
            string ImagePath = input;
            (string clas, float prob) = result;
            Console.WriteLine($"{ImagePath} belongs to class {clas} with prob. - {prob}");

        }
        static void Main(string[] args)
        {
            Model MnistModel = new Model();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            Parallelizer<(string,float)> ModelParallelizer = new Parallelizer<(string, float)>(MnistModel);
            ModelParallelizer.OutputEvent += ConsoleOutput;
            ModelParallelizer.Run(args.FirstOrDefault() ?? "images");
            sw.Stop();

            Console.WriteLine($"Time:{sw.ElapsedMilliseconds}");

            //using var db = new LibraryContext();

            //foreach (var imageclass in db.ImageClasses.Include(a => a.Images).ThenInclude(a => a.ByteImage))
            //{
            //    Console.WriteLine($"ID:{imageclass.ImageClassId} NAME:{imageclass.ClassName} COUNT{imageclass.Count}");

            //    foreach (var img in imageclass.Images)
            //        Console.WriteLine($"-- id:{img.ImageInfoId} name:{img.ClassName} prob:{img.Prob} get:{img.NumOfRequests} path:{img.Path} hash:{img.ImageHash} Image:{img.ByteImage.Img}");

            //}
            ////MnistModel.ClearDataBase();
        }
    }
}
