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
        static void Main(string[] args)
        {
            Model MnistModel = new Model();
            MnistModel.OutputEvent+=Model.ConsolePrint;

            Stopwatch sw1 = new Stopwatch();

            sw1.Start();
            MnistModel.Recognize(args.FirstOrDefault() ?? "images");
            sw1.Stop();

            Console.WriteLine($"Standart:{sw1.ElapsedMilliseconds}\n");
        }
    }
}
