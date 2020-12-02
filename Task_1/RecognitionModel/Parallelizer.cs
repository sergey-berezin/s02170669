using System;
using System.Threading;
using System.Linq;
using System.IO;

namespace RecognitionModel
{

    public delegate void OutputHandler<Out>(object sender, string input, Out result);
    public interface IProcess <In,Out>
    {
        Out ProcessFile(In Input);

    }

    public class Parallelizer <Out>
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        
        IProcess<string,Out> model;
        public event OutputHandler<Out> OutputEvent;

        public Parallelizer(IProcess<string,Out> model)
        {
            this.model = model;
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public void Run(string Path)
        {
            int NumOfThreads = Environment.ProcessorCount;

            cts = new CancellationTokenSource();

            string[] Files = Directory.GetFiles(Path);

            Thread[] threads = new Thread[NumOfThreads];

            int processed = -1;

            for (int i = 0; i < NumOfThreads; i++)
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
                            Out output = model.ProcessFile(Files[FileNum]);
                            OutputEvent?.Invoke(this, Files[FileNum], output);

                        }
                    }


                });
                threads[i].Start();
            }

             for (int i = 0; i < NumOfThreads; i++)
            {
                threads[i].Join();
            }

        }

    }
    
}