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

namespace Model
{
    public class Recognizer : IProcess
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
                modelpath = value;
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
                inputnodename = value;
            }
        }
        public Recognizer(string ModelPath = "mnist-8.onnx", string InputNodeName = "Input3")
        {
            this.modelpath = ModelPath;
            this.inputnodename = InputNodeName;
            this.session = new InferenceSession(modelpath);
        }

        public object ProcessFile(object obj)
        {
            string ImgPath = obj as string;
            Image<Rgb24> image;

            try
            {
                image = Image.Load<Rgb24>(ImgPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType() + " : " + ex.Message);
                return null;
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
                    input[0, 0, y, x] = image[x, y].R / 255f;

            // Create the inputs to the model
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputnodename, input)
            };

            // Run NNet  
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            // Softmax calculation
            var output = results.First().AsEnumerable<float>().ToArray();
            var sum = output.Sum(x => (float)Math.Exp(x));
            var softmax = output.Select(x => (float)Math.Exp(x) / sum);

            float maxValue = softmax.Max();
            int maxIndex = softmax.ToList().IndexOf(maxValue);

            return (maxIndex, maxValue);

        }

    }

}
