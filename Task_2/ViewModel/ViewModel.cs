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
using System.Linq;
using System.Threading;

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

            ModelParallelizer.OutputEvent += Output;

            startCommand = new RelayCommand(_ => !running,
                                            async _ =>
                                            {
                                                running = true;
                                                string[] Files;

                                                string Path = uiServices.OpenPath();
                                                
                                                try
                                                {
                                                     Files = Directory.GetFiles(Path,"*.*").Where(s => s.EndsWith(".bmp") || s.EndsWith(".jpg") ||
                                                                                                       s.EndsWith(".png") || s.EndsWith(".jpeg")).ToArray();

                                                    // 0 images found after LINQ filtering
                                                    if (Files.Length == 0) throw new Exception("There is no images in the directory");
                                                }
                                                catch(ArgumentNullException)
                                                {
                                                    running = false;
                                                    return;
                                                }
                                                catch(Exception ex)
                                                {
                                                    uiServices.Message(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                    running = false;
                                                    return;
                                                }

                                                List<ImageRepresentation> Images = new List<ImageRepresentation>();

                                                await Task.Run(() =>
                                                {
                                                    float current_count = 0;
                                                    foreach (string FilePath in Files)
                                                    {
                                                        current_count++;
                                                        byte[] ByteImage = File.ReadAllBytes(FilePath);
                                                        Images.Add(new ImageRepresentation { ImageName = FilePath, Base64Image = Convert.ToBase64String(ByteImage) });

                                                        Progress = (int)(current_count / 100.0);
                                                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
                                                    }
                                                });

                                                Progress = 0;
                                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));

                                                if (!UseServer)
                                                {
                                                    if (Images != null)
                                                    {
                                                        results.Clear();
                                                        images.Clear();

                                                        await Task.Run(() => ModelParallelizer.Run(Images));

                                                    }
                                                }
                                                else
                                                try
                                                {
                                                    IndeterminedPBar = true;
                                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndeterminedPBar"));

                                                    HttpClient client = new HttpClient();
                                                    var s = JsonConvert.SerializeObject(Images);
                                                    var c = new StringContent(s);
                                                    c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                                                    cts = new CancellationTokenSource();

                                                    HttpResponseMessage result = await client.PutAsync("http://localhost:5000/images", c, cts.Token);

                                                    string content = await result.Content.ReadAsStringAsync();
                                                    List<ImageRepresentation> ResultImages = JsonConvert.DeserializeObject<List<ImageRepresentation>>(content);

                                                    results.Clear();
                                                    images.Clear();

                                                    await this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
                                                    {
                                                         results.AddResults(ResultImages);
                                                    }));

                                                }
                                                catch (HttpRequestException ex)
                                                {
                                                    uiServices.Message("Server is not available: "+ ex.Message, "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                                catch(TaskCanceledException)
                                                {
                                                     
                                                }
                                                finally
                                                {
                                                    IndeterminedPBar = false;
                                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndeterminedPBar"));
                                                }

                                                running = false;
                                            }
                                           );

            stopCommand = new RelayCommand(_ => running,
                                           _ =>
                                           {
                                               if(UseServer)
                                               {
                                                   cts.Cancel();
                                               }
                                               else
                                                   ModelParallelizer.Stop();
                                               running = false;
                                           }
                                           );
            clearCommand = new RelayCommand(_ => true,
                                           async _ =>
                                           {
                                               StatisticResults= new List<ImageRepresentation>();
                                               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatisticResults"));

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


        private bool running = false;
        private static CancellationTokenSource cts = new CancellationTokenSource();
        public bool UseServer { get; set; } = false;
        public int Progress { get; set; }
        public bool IndeterminedPBar { get; set; } = false;

        private static Model MnistRecognizer = new Model(ModelPath: Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName + "\\Task_1\\RecognitionModel\\mnist-8.onnx");

        private static Parallelizer<ImageRepresentation,ImageRepresentation> ModelParallelizer = new Parallelizer<ImageRepresentation,ImageRepresentation>(MnistRecognizer);

        private static ObservableResults results = new ObservableResults(GetDatabaseImages());

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

        public void Output(object sender, ImageRepresentation result)
        {

            this.uiServices.GetDispatcher().BeginInvoke(new Action(() =>
            {
                results.AddResult(result.Base64Image, result.ClassName, result.Prob.ToString());
            }));


        }

        public static List<ImageRepresentation> GetDatabaseImages()
        {
            try
            {
                HttpClient client = new HttpClient();
                string result = client.GetStringAsync("http://localhost:5000/images/all").Result;

                return JsonConvert.DeserializeObject<List<ImageRepresentation>>(result);

            }
            catch(Exception)
            {
                
            }

            return null;
        }

    }
}
