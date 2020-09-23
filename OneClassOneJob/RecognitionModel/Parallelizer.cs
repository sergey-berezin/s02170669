using System;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace RecognitionModel
{

    public delegate void OutputHandler(object sender, params object[] args);
    public interface IProcess
    {
        object ProcessFile(object Obj);
    }

    public class Parallelizer
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        
        IProcess model;
        public event OutputHandler OutputEvent;

        public Parallelizer(IProcess model)
        {
            this.model = model;
            Console.CancelKeyPress += new ConsoleCancelEventHandler((s, args)=>{args.Cancel = true; cts.Cancel();});
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public void Run(string Path)
        {
            cts = new CancellationTokenSource();

            string[] Images = Directory.GetFiles(Path);
            foreach(string path in Images)
                queue.Enqueue(path);

            Thread[] threads = new Thread[Environment.ProcessorCount];

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i] = new Thread(()=> 
                {
                    
                    string ImgPath;
                    while(!queue.IsEmpty && !cts.Token.IsCancellationRequested)
                        if(queue.TryDequeue(out ImgPath))
                        {
                            object output = model.ProcessFile(ImgPath);
                            OutputEvent?.Invoke(this,ImgPath,output);

                        }

                });
                threads[i].Start();
            }
             for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i].Join();
            }

        }

    }
    
}