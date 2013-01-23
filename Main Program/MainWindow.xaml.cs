using System.Windows;
using MyFa;
using System.Diagnostics;

namespace MyFa
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FontClick(object sender, RoutedEventArgs e)
        {
            Process tmpProcess = new Process();
            tmpProcess.StartInfo.FileName = "FontEditor.exe";
            tmpProcess.Start();
            tmpProcess.WaitForExit();
        }

        private void MessageClick(object sender, RoutedEventArgs e)
        {
            Process tmpProcess = new Process();
            tmpProcess.StartInfo.FileName = "MessageEditor.exe";
            tmpProcess.Start();
            tmpProcess.WaitForExit();
        }

        private void UploadClick(object sender, RoutedEventArgs e)
        {
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("MyFaPixel © MyFaJoArCo 2009\n\nhttp://myfajoarco.lv/\nmyfajoarco@gmail.com\nSkype: l..cmapt..l");
        }
    }
}
