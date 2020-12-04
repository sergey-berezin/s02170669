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
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows;
using ImageContracts;

namespace ViewModel
{
    public interface IUIServises
    {
        string OpenPath();

        Dispatcher GetDispatcher();
        void Message(string MessageString, string CaptionString, MessageBoxButton Button, MessageBoxImage Icon);

    }

    public class ViewModel : INotifyPropertyChanged
    {

        public ViewModel(IUIServises uiServices)
        {
            this.uiServices = uiServices;

            //ModelParallelizer.OutputEvent += Output;

            startCommand = new RelayCommand(_ => !Running,
                                            async _ =>
                                            {
                                                string Path = uiServices.OpenPath();
                                                //if (Path != null)
                                                //{
                                                //    results.Clear();
                                                //    images.Clear();

                                                //    Running = true;

                                                //    await Task.Run(() => ModelParallelizer.Run(Path));

                                                //    Running = false;
                                                //}



                                                string[] Files = Directory.GetFiles(Path);
                                                List<ImageRepresentation> Images = new List<ImageRepresentation>();
                                                foreach(string FilePath in Files)
                                                {
                                                    byte[] ByteImage = File.ReadAllBytes(FilePath);
                                                    Images.Add(new ImageRepresentation {ImageName=FilePath, Base64Image=Convert.ToBase64String(ByteImage) });
                                                    
                                                }

                                                try
                                                {
                                                    HttpClient client = new HttpClient();
                                                    var s = JsonConvert.SerializeObject(Images);
                                                    var c = new StringContent(s);
                                                    c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                                                    HttpResponseMessage result = await client.PutAsync("http://localhost:5000/images", c);

                                                    string content = await result.Content.ReadAsStringAsync();
                                                    List<ImageRepresentation> ResultImages = JsonConvert.DeserializeObject<List<ImageRepresentation>>(content);

                                                    results.Clear();
                                                    images.Clear();

                                                    await this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
                                                     {
                                                         results.AddResults(ResultImages);
                                                     }));




                                                }
                                                catch (System.Net.Http.HttpRequestException ex)
                                                {
                                                    uiServices.Message("Server is not available: "+ ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            }
                                           );

            stopCommand = new RelayCommand(_ => Running,
                                           _ =>
                                           {
                                               //ModelParallelizer.Stop();
                                               Running = false;
                                           }
                                           );
            clearCommand = new RelayCommand(_ => true,
                                           async _ =>
                                           {
                                               try
                                               {
                                                   HttpClient client = new HttpClient();
                                                   HttpResponseMessage result = await client.DeleteAsync("http://localhost:5000/images/");
                                                   string message = await result.Content.ReadAsStringAsync();
                                                   uiServices.Message(message, "Server response", MessageBoxButton.OK, MessageBoxImage.Information);

                                               }
                                               catch (System.Net.Http.HttpRequestException ex)
                                               {
                                                   uiServices.Message("Server is not available: " + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                               }

                                           }
                                           );
            statsCommand = new RelayCommand(_ => true,
                                          async _ =>
                                          {                                      
                                              try
                                              {
                                                    HttpClient client = new HttpClient();
                                                    string result = await client.GetStringAsync("http://localhost:5000/images/statistics");

                                                    StatisticResults = JsonConvert.DeserializeObject<List<ImageRepresentation>>(result);
                                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatisticResults"));

                                              }
                                              catch (System.Net.Http.HttpRequestException ex)
                                              {
                                                  uiServices.Message("Server is not available" + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                              }

                                          }
                                          );
        }


        private bool Running = false;

        private static Model MnistRecognizer = new Model(ModelPath: Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName + "\\Task_1\\RecognitionModel\\mnist-8.onnx");

        //private static Parallelizer<byte[],(string, float)> ModelParallelizer = new Parallelizer<byte[], (string,float)>(MnistRecognizer);

        private static ObservableResults results = new ObservableResults(GetDatabaseImagesAsync());

        private static Results selectedclass;

        private static ObservableCollection<Image> images = new ObservableCollection<Image>();

        public List<ImageRepresentation> StatisticResults { get; set; }


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

        //public void Output(object sender, byte[] image, (string,float) result)
        //{
        //    string ImagePath = input;
        //    (string class_name, float prob) = result;

        //    this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
        //    {
        //        results.AddResult(class_name, ImagePath, prob.ToString());
        //    }));
             

        //}

        public static List<ImageRepresentation> GetDatabaseImagesAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                string result = client.GetStringAsync("http://localhost:5000/images/all").Result;

                return JsonConvert.DeserializeObject<List<ImageRepresentation>>(result);

            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                //uiServices.Message("Server images can't be loaded" + ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

    }
}
