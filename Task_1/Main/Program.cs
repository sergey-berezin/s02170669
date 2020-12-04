using System;
using System.Diagnostics;
using RecognitionModel;
using System.Linq;
using SQL;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

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
        static async Task<int> Main(string[] args)
        {
            //Model MnistModel = new Model();

            //Stopwatch sw = new Stopwatch();

            //sw.Start();
            //Parallelizer<(string, float)> ModelParallelizer = new Parallelizer<(string, float)>(MnistModel);
            //ModelParallelizer.OutputEvent += ConsoleOutput;
            //ModelParallelizer.Run(args.FirstOrDefault() ?? "images");
            //sw.Stop();

            //Console.WriteLine($"Time:{sw.ElapsedMilliseconds}");

            //using var db = new LibraryContext();

            //foreach (var imageclass in db.ImageClasses.Include(a => a.Images).ThenInclude(a => a.ByteImage))
            //{
            //    Console.WriteLine($"ID:{imageclass.ImageClassId} NAME:{imageclass.ClassName} COUNT{imageclass.Count}");

            //    foreach (var img in imageclass.Images)
            //        Console.WriteLine($"-- id:{img.ImageInfoId} name:{img.ClassName} prob:{img.Prob} get:{img.NumOfRequests} path:{img.Path} hash:{img.ImageHash} Image:{img.ByteImage.Img}");

            //}
            ////MnistModel.ClearDataBase();
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync("http://localhost:5000/images/all");
            var allimages = JsonConvert.DeserializeObject<ImageInfo[]>(result);

            //var nb = new NewBook() { Title = "C", Pages = 400 };
            //var s = JsonConvert.SerializeObject(nb);
            //var c = new StringContent(s);
            //c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            //await client.PutAsync("http://localhost:5000/api/books", c);

            return 0;
        }
    }
}
