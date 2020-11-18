using System;
using System.Threading.Tasks;
using RecognitionModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
                                                    string[] Files = Directory.GetFiles(path);
                                                    await Task.Run(() => ModelParallelizer.Run(Files));

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
                                           async _ =>
                                           {
                                               await Task.Run(() =>
                                               {
                                                   using (var db = new LibraryContext())
                                                   {
                                                       db.ImageClasses.RemoveRange(db.ImageClasses.AsEnumerable());
                                                       db.Images.RemoveRange(db.Images.AsEnumerable());
                                                       db.SaveChanges();

                                                   }
                                               }
                                               );
                                           }
                                           );
                                           
        }


        private bool Running = false;

        private static Model MnistRecognizer = new Model(ModelPath: Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Task_1\\RecognitionModel\\mnist-8.onnx");

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




        private IUIServises uiServices;

        private readonly ICommand stopCommand;
        public ICommand StopCommand { get { return stopCommand; } }
        private readonly ICommand startCommand;
        public ICommand StartCommand { get { return startCommand; } }

        private readonly ICommand clearCommand;
        public ICommand ClearCommand { get { return clearCommand; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Output(object sender, params object[] result)
        {
            string ImagePath = (string)result[0];
            (int class_name, float prob) = ((int, float))result[1];

            using (var db = new LibraryContext())
            {
                var NewImage = new ImageInfo() { ClassName = class_name.ToString(), Prob = prob, Path = ImagePath, ByteImage = new ImageFile { Img = File.ReadAllBytes(ImagePath) } };
                NewImage.ImageClasses = new List<ImageClass>();

                var ClassNum = db.ImageClasses.Where(a => a.ClassName == class_name.ToString()).FirstOrDefault();

                if (ClassNum != null)
                {
                    NewImage.ImageClasses.Add(ClassNum);
                    ClassNum.Images = new List<ImageInfo>();
                    ClassNum.Images.Add(NewImage);
                    ClassNum.Count += 1;

                    db.Add(NewImage);
                    db.SaveChanges();
                }
                else
                {
                    var NewClass = new ImageClass() { ClassName = class_name.ToString(), Count = 1 };

                    NewImage.ImageClasses.Add(NewClass);

                    NewClass.Images = new List<ImageInfo>();
                    NewClass.Images.Add(NewImage);

                    db.Add(NewClass);
                    db.Add(NewImage);

                }

                db.SaveChanges();
            }

            this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
            {
                results.Add_Result(class_name.ToString(), ImagePath, prob.ToString());
            }));


        }

    }
}
