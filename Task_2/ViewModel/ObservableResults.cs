using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ViewModel
{


    public class Image
    {
        private string path;
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
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

        public Image(string path, string info)
        {
            this.path = path;
            this.info = info;
        }
    }
    public class Results: INotifyPropertyChanged
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

        public Results(string Class_Name, string ImagePath, string ImageInfo)
        {
            this.class_name = Class_Name;
            this.count = 1;
            this.images = new ObservableCollection<Image>
            {
                new Image(ImagePath, ImageInfo)
            };

        }

    }

    public class ObservableResults: ObservableCollection<Results>
    {
        public void Add_Result(string Class_Name, string ImagePath, string ImageInfo)
        {
            
            foreach(var Result in base.Items)
            {
                if(Result.Class_Name.Equals(Class_Name))
                {
                    base[base.IndexOf(Result)].Count += 1;
                    base[base.IndexOf(Result)].Images.Add(new Image(ImagePath, ImageInfo));

                    return;
                }
            }

            base.Add(new Results(Class_Name, ImagePath, ImageInfo));
            return;

        }
    }
}
