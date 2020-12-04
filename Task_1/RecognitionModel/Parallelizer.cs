using System;
using System.Threading;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace RecognitionModel
{

    public delegate void OutputHandler<In,Out>(object sender, In input, Out result);
    public delegate void OutputHandler<Out>(object sender, Out result);
    public interface IProcess <In,Out>
    {
        Out ProcessFile(In Input);

    }

    public class Parallelizer <In, Out>
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        
        IProcess<In,Out> model;
        public event OutputHandler<Out> OutputEvent;
        bool useServer;
        public bool UseServer
        {
            get
            {
                return useServer;
            }
            set
            {
                useServer = value;
            }
        }
        public Parallelizer(IProcess<In,Out> Model, bool UseServer=false)
        {
            this.model = Model;
            this.useServer = UseServer;
        }

        public void Stop()
        {
            cts.Cancel();
        }

        public List<Out> Run(List<In> Files)
        {
            ConcurrentQueue<Out> queue = new ConcurrentQueue<Out>();
            int NumOfThreads = Environment.ProcessorCount;

            cts = new CancellationTokenSource();

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
                            if(!UseServer)
                                OutputEvent?.Invoke(this, output);
                            queue.Enqueue(output);

                        }
                    }


                });
                threads[i].Start();
            }

             for (int i = 0; i < NumOfThreads; i++)
             {
                threads[i].Join();
             }

            return queue.ToList();

        }

    }
    
}