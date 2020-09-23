using System;
using System.Diagnostics;
using RecognitionModel;
using System.Threading;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;

namespace Main
{
    class Program
    {
        public static void ConsoleOutput(object sender, params object[] result)
        {
            string ImagePath = (string) result[0];
            (int clas, float prob) = ((int, float)) result[1];
            Console.WriteLine($"{ImagePath} belongs to class {clas} with prob. - {prob}");

        }
        static void Main(string[] args)
        {
            Model MnistModel = new Model();

            Stopwatch sw = new Stopwatch();

            sw.Start();
            Parallelizer ModelParallelizer = new Parallelizer(MnistModel);
            ModelParallelizer.OutputEvent+=ConsoleOutput;
            ModelParallelizer.Run(args.FirstOrDefault() ?? "images");
            sw.Stop();

            Console.WriteLine($"Time:{sw.ElapsedMilliseconds}");
        }
    }
}
