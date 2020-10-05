using System;
using System.Threading;
using System.Linq;
using System.IO;

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

        public void Run(string DirPath)
        {

            cts = new CancellationTokenSource();

            string[] Files = Directory.GetFiles(DirPath);

            Thread[] threads = new Thread[Environment.ProcessorCount];

            int processed = -1;

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                threads[i] = new Thread(()=>
                {
                    int FileNum;

                    while(!cts.Token.IsCancellationRequested)
                    {

                        FileNum=Interlocked.Increment(ref processed);
                        if(FileNum >= Files.Count())
                            break;
                        else
                        {
                            object output = model.ProcessFile(Files[FileNum]);
                            OutputEvent?.Invoke(this, Files[FileNum], output);

                        }
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