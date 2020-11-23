using System;
using System.Threading.Tasks;
using RecognitionModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using SQL;
using System.Collections.Generic;

namespace ViewModel
{
    public interface IUIServises
    {
        string OpenPath();

        Dispatcher GetDispatcher();

    }

    public class ViewModel : INotifyPropertyChanged
    {

        public ViewModel(IUIServises uiServices)
        {
            this.uiServices = uiServices;

            ModelParallelizer.OutputEvent += Output;

            startCommand = new RelayCommand(_ => !Running,
                                            async _ =>
                                            {
                                                string path = uiServices.OpenPath();
                                                if (path != null)
                                                {
                                                    results.Clear();
                                                    images.Clear();

                                                    Running = true;

                                                    await Task.Run(() => ModelParallelizer.Run(path));

                                                    Running = false;
                                                }
                                            }
                                           );

            stopCommand = new RelayCommand(_ => Running,
                                           _ =>
                                           {
                                               ModelParallelizer.Stop();
                                               Running = false;
                                           }
                                           );
            clearCommand = new RelayCommand(_ => true,
                                           _ =>
                                           {
                                               MnistRecognizer.ClearDataBase();
                                           }
                                           );
            statsCommand = new RelayCommand(_ => true,
                                          async _ =>
                                          {
                                              await Task.Run(() => StatisticResults = MnistRecognizer.GetModelStatistics());
                                              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatisticResults"));
                                          }
                                          );
        }


        private bool Running = false;

        private static Model MnistRecognizer = new Model(ModelPath: Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName + "\\Task_1\\RecognitionModel\\mnist-8.onnx");

        private static Parallelizer ModelParallelizer = new Parallelizer(MnistRecognizer);

        private static ObservableResults results = new ObservableResults();

        private static Results selectedclass = new Results(null, null, null);

        private static ObservableCollection<Image> images = new ObservableCollection<Image>();

        public List<ImageInfo> StatisticResults { get; set; }


        public Results SelectedClass
        {
            get
            {
                return selectedclass;
            }

            set
            {
                selectedclass = value;
                if (selectedclass != null)
                    SelectedImages = selectedclass.Images;
            }
        }

        public ObservableResults Results
        {
            get
            {
                return results;
            }
        }
        public ObservableCollection<Image> SelectedImages
        {
            get
            {
                return images;
            }
            set
            {
                images = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedImages"));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private IUIServises uiServices;

        private readonly ICommand stopCommand;
        public ICommand StopCommand { get { return stopCommand; } }

        private readonly ICommand startCommand;
        public ICommand StartCommand { get { return startCommand; } }

        private readonly ICommand clearCommand;
        public ICommand ClearCommand { get { return clearCommand; } }

        private readonly ICommand statsCommand;
        public ICommand StatsCommand { get { return statsCommand; } }

        public void Output(object sender, params object[] result)
        {
            string ImagePath = (string)result[0];
            (string class_name, float prob) = ((string, float))result[1];

            this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
            {
                results.Add_Result(class_name, ImagePath, prob.ToString());
            }));


        }

    }
}
