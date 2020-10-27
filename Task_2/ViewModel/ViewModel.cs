using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;

namespace ViewModel
{
    public interface IUIServises
    {
        string OpenPath();

        Dispatcher GetDispatcher();

        //List<object> SelectedItems();

        //string OpenPath();

        //bool WantToSave();

    }
    
    public class ViewModel: INotifyPropertyChanged
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
        }


        private bool Running = false;

        private static Recognizer MnistRecognizer = new Recognizer(ModelPath:Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName+"\\Model\\mnist-8.onnx");

        private static Parallelizer ModelParallelizer = new Parallelizer(MnistRecognizer);

        private static ObservableResults results = new ObservableResults();

        private static Results selectedclass = new Results(null, null, null);

        private static ObservableCollection<Image> images = new ObservableCollection<Image>();


        public Results SelectedClass
        {
            get
            {
                return selectedclass;
            }

            set
            {
                selectedclass = value;
                if(selectedclass != null)
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




        private IUIServises uiServices;

        private readonly ICommand stopCommand;
        public ICommand StopCommand { get { return stopCommand; } }

        private readonly ICommand startCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand StartCommand { get { return startCommand; } }

        public void Output(object sender, params object[] result)
        {
            string ImagePath = (string)result[0];
            (int class_name, float prob) = ((int, float))result[1];

            this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
            {
                results.Add_Result(class_name.ToString(), ImagePath, prob.ToString());
            }));


        }

    }
}
