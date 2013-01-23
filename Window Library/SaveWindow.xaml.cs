using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace MyFa
{
    public partial class SaveWindow : Window
    {
        private string Type;

        public string Path;

        public SaveWindow(string type)
        {
            InitializeComponent();

            Title = "MyFaPixel - Save " + type;
            NameText.Text = "name of " + type.ToLower();
            Type = type;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\" + Type + "s";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            DirectoryInfo tmpDI = new DirectoryInfo(Path);

            Regex.Replace(NameBox.Text, @"\W", "");
            FileInfo[] tmpFI = tmpDI.GetFiles(NameBox.Text + (Type == "Font" ? ".mfpf" : ".mfpm"));

            if ((tmpFI.Length > 0 && (MessageBox.Show("File with this name exists already. Continue?", "File overwrite", MessageBoxButton.YesNo) == MessageBoxResult.Yes)) || tmpFI.Length == 0)
            {
                Path += @"\" + NameBox.Text + (Type == "Font" ? ".mfpf" : ".mfpm");
                DialogResult = true;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
