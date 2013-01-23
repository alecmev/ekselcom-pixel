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
    public partial class MessageOpenWindow : Window
    {
        public string Path;
        public int Type;
        private string postfix;

        public MessageOpenWindow()
        {
            InitializeComponent();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            Path += @"\" + (string)((ComboBoxItem)SelectFile.SelectedItem).Content;
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TypeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (SelectType.SelectedIndex)
            {
                case 0:
                    Type = 0;
                    postfix = ".mfpm";
                    break;
                case 1:
                    Type = 1;
                    postfix = ".bin";
                    break;
            }

            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Message Collections";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            DirectoryInfo tmpDI = new DirectoryInfo(Path);
            FileInfo[] tmpFI = tmpDI.GetFiles("*" + postfix);
            SelectFile.Items.Clear();

            foreach (FileInfo tmpFile in tmpFI)
            {
                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = tmpFile.Name;
                SelectFile.Items.Add(tmpItem);
            }

            if (SelectFile.Items.Count == 0)
            {
                OpenButton.IsEnabled = false;
                MessageBox.Show("No any message collection in this format found! Put them in " + Path + "!");
            }
            else
            {
                OpenButton.IsEnabled = true;
                SelectFile.SelectedIndex = 0;
            }
        }
    }
}
