using Ookii.Dialogs.Wpf;
using System.Windows;
using System.Windows.Threading;
using ViewModel;

namespace View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "Image Classifier. Pyatov Vladislav, 401 group.";
            DataContext = new ViewModel.ViewModel(new MyAppUIServices(this.Dispatcher));

        }
    }

    public class MyAppUIServices : IUIServises
    {
        private Dispatcher dispatcher;
        public MyAppUIServices(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }
        public string OpenPath()
        {
            // Ookii.Dialogs.Wpf NuGet package required
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true;

            if ((bool)dialog.ShowDialog())
                return dialog.SelectedPath;
            else
                return null;
        }

        public Dispatcher GetDispatcher()
        {
            return this.dispatcher;
        }

        public void Message(string MessageString, string CaptionString, MessageBoxButton Button, MessageBoxImage Icon)
        {
            MessageBox.Show(MessageString, CaptionString, Button, Icon);
        }
    }
}
