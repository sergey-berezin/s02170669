using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageContracts;
namespace ViewModel
{


    public class Image
    {
        private ImageSource file;
        public ImageSource File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        private string info;
        public string Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
            }
        }

        public Image(byte [] Img, string Info)
        {
            this.file = ByteToImage(Img);
            this.info = Info;
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource ImgSrc = biImg as ImageSource;

            return ImgSrc;
        }
    }

    public class Results : INotifyPropertyChanged
    {
        private string class_name;

        private int count;

        private ObservableCollection<Image> images;

        public string Class_Name
        {
            get
            {
                return class_name;
            }
            set
            {
                class_name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Class_Name"));

            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }

        public ObservableCollection<Image> Images
        {
            get
            {
                return images;
            }
            set
            {
                images = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Images"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Results(string Img, string ClassName, string Info)
        {
            this.class_name = ClassName;
            this.count = 1;
            this.images = new ObservableCollection<Image>
            {
                new Image(Convert.FromBase64String(Img), Info)
            };

        }

    }

    public class ObservableResults : ObservableCollection<Results>
    {
        public ObservableResults() { }
        public ObservableResults(List<ImageRepresentation> Images)
        {
            AddResults(Images);
        }

        public void AddResult(string Img, string ClassName, string Info)
        {

            foreach (var Result in base.Items)
            {
                if (Result.Class_Name.Equals(ClassName))
                {
                    base[base.IndexOf(Result)].Count += 1;
                    base[base.IndexOf(Result)].Images.Add(new Image(Convert.FromBase64String(Img), Info));

                    return;
                }
            }

            base.Add(new Results(Img, ClassName, Info));
            return;

        }

        public void AddResults(List<(string, string, float)> Results)
        {
            foreach ((string, string, float) Result in Results)
                AddResult(Result.Item1, Result.Item2, Result.Item3.ToString());
        }

        public void AddResults(List<ImageRepresentation> Results)
        {
            if (Results != null)
            {
                foreach (ImageRepresentation Result in Results)
                {
                    AddResult(Result.Base64Image, Result.ClassName, Result.Prob.ToString());
                }
            }
        }
    }
}
