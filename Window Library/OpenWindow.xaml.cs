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

namespace MyFa
{
    public partial class OpenWindow : Window
    {
        public string Path;

        public OpenWindow(string type)
        {
            InitializeComponent();

            Title = "MyFaPixel - Open " + type;
            SelectText.Text = "select " + type.ToLower();

            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\" + type + "s";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            DirectoryInfo tmpDI = new DirectoryInfo(Path);
            FileInfo[] tmpFI = tmpDI.GetFiles(type == "Font" ? "*.mfpf" : "*.mfpm");

            foreach (FileInfo tmpFile in tmpFI)
            {
                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = tmpFile.Name;
                SelectBox.Items.Add(tmpItem);
            }

            if (SelectBox.Items.Count == 0)
            {
                OpenButton.IsEnabled = false;
                MessageBox.Show("No any " + type.ToLower() + " found! Put them in " + Path + "!");
            }
            else SelectBox.SelectedItem = SelectBox.Items[0];
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            Path += @"\" + (string)((ComboBoxItem)SelectBox.SelectedItem).Content;
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
