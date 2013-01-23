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
    public partial class MessageSaveWindow : Window
    {
        public string Path;
        public int Type;
        private string postfix;
        private bool exists = false;

        public MessageSaveWindow(string newFile)
        {
            InitializeComponent();

            if (newFile != "") NameText.Text = newFile;
            else NameText.Text = "untitled" + DateTime.Now.ToString("MMddyyyyHHmmss");
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            NameChanged(null, null);
            TypeChanged(null, null);
            NameText.Text = NameText.Text.Trim();
            if (NameText.Text != "")
            {
                if (!exists || (MessageBox.Show("File with this name and type exists already. Continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
                {
                    Path += @"\" + NameText.Text + postfix;
                    DialogResult = true;
                }
            }
            else NameText.Text = "untitled" + DateTime.Now.ToString("MMddyyyyHHmmss");
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InfoChanged()
        {
            if (NameText != null && SelectType != null)
            {
                switch (SelectType.SelectedIndex)
                {
                    case 1:
                        Type = 1;
                        postfix = ".bin";
                        break;
                    default:
                        Type = 0;
                        postfix = ".mfpm";
                        break;
                }

                Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Message Collections";
                if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

                DirectoryInfo tmpDI = new DirectoryInfo(Path);
                if (Regex.IsMatch(NameText.Text, @"\W")) NameText.Text = Regex.Replace(NameText.Text, @"\W", "");
                NameText.Text = NameText.Text.Trim();
                if (NameText.Text != "")
                {
                    FileInfo[] tmpFI = tmpDI.GetFiles(NameText.Text + postfix);

                    if (tmpFI.Length == 0) NameText.Background = Brushes.LightGreen;
                    else NameText.Background = Brushes.Gold;

                    exists = (tmpFI.Length > 0);
                }
                else NameText.Background = Brushes.Gold;
            }
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            InfoChanged();
        }

        private void TypeChanged(object sender, SelectionChangedEventArgs e)
        {
            InfoChanged();
        }
    }
}
