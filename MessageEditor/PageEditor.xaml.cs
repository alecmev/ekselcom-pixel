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
using MyFa.Properties;
using System.IO;

namespace MyFa
{
    public partial class PageEditor : Window
    {
        private PagePreview SelectedMessagePreview;
        public PageStorage Page;
        private int Indent = 1;
        private int Shift = 0;
        private int VShift = 0;

        private bool fontsFound;
        private bool autoCenter = false;

        new private bool Initialized = false;

        public PageEditor(PageStorage newPage, bool multi, Int32Rect clip, int messageNum, string messageName, int pageNum, int totalPages)
        {
            Page = newPage;

            InitializeComponent();

            if (!multi) MessageTime.IsEnabled = false;
            Title = "MyFaPixel - Page Editor - " + messageName + " №" + messageNum + " - page №" + (pageNum + 1) + " (total " + totalPages + " pages)";

            SelectedMessagePreview = new PagePreview(Settings.Default.DisplayWidth, Settings.Default.DisplayHeight, clip);
            SelectedMessagePreview.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            PagePreviewScroll.Content = SelectedMessagePreview;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Fonts";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            DirectoryInfo tmpDI = new DirectoryInfo(path);
            FileInfo[] tmpFI = tmpDI.GetFiles("*.mfpf");

            foreach (FileInfo tmpFont in tmpFI)
            {
                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = tmpFont.Name.Substring(0, tmpFont.Name.Length - 5);
                if (Page != null && Page.Strings[0].Font == (string)tmpItem.Content) MessageFont.SelectedItem = tmpItem.Content;
                MessageFont.Items.Add(tmpItem);
            }

            if (MessageFont.Items.Count == 0)
            {
                fontsFound = false;
                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = "no fonts found";
                MessageFont.Items.Add(tmpItem);
            }
            else fontsFound = true;

            if (MessageFont.SelectedItem == null) MessageFont.SelectedIndex = 0;

            if (Page != null)
            {
                if (multi) MessageTime.Text = Page.Time.ToString();
                MessageText.Text = Page.Strings[0].Text;
                Indent = Page.Strings[0].Indent;
                Shift = Page.Strings[0].Shift;
                VShift = Page.Strings[0].VShift;
            }

            Initialized = true;

            MessageChanged();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            DialogResult = fontsFound;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void MessageChanged()
        {
            if (fontsFound && Initialized)
            {
                if (autoCenter) Shift = -1;
                SelectedMessagePreview.Clear(false);
                int tmp = 0;
                Page = new PageStorage(MessageTime.IsEnabled && int.TryParse(MessageTime.Text, out tmp) ? tmp : 0, 1);
                Page.Strings[0] = new StringStorage("Page", SelectedMessagePreview.Clip.X, SelectedMessagePreview.Clip.Y, SelectedMessagePreview.Clip.Width, SelectedMessagePreview.Clip.Height, MessageText.Text, (string)((ComboBoxItem)MessageFont.SelectedItem).Content, Shift, VShift, Indent, false);
                Page = SelectedMessagePreview.Render(Page);
                if (SelectedMessagePreview.Fits)
                {
                    SaveButton.IsEnabled = true;
                    SaveButton.Title = "save";
                    SaveButton.Filter = Brushes.Green;
                }
                else
                {
                    SaveButton.IsEnabled = false;
                    SaveButton.Title = "TOO LONG TEXT";
                    SaveButton.Filter = Brushes.Yellow;
                }
                Shift = Page.Strings[0].Shift;
            }
        }

        private void MessageTextChanged(object sender, TextChangedEventArgs e)
        {
            MessageChanged();
        }

        private void MessageFontChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageChanged();
        }

        private void MessageTimeChanged(object sender, TextChangedEventArgs e)
        {
            int tmp = 0;
            if (!int.TryParse(MessageTime.Text, out tmp) || tmp < 0) MessageTime.Text = "0";
            MessageChanged();
        }

        private void ToolClick(object sender, RoutedEventArgs e)
        {
            SecondMenuButton tmpButton = (SecondMenuButton)sender;
            switch (tmpButton.Title)
            {
                case "←→":
                    ++Indent;
                    break;
                case "→←":
                    if (Indent > 0) --Indent;
                    break;
                case "↑":
                    --VShift;
                    break;
                case "↓":
                    ++VShift;
                    break;
                case "←":
                    if (Shift > 0) --Shift;
                    break;
                case "→":
                    ++Shift;
                    break;
                case "●":
                    autoCenter = !autoCenter;
                    ShiftCenter.Selected = !ShiftCenter.Selected;
                    break;
            }
            MessageChanged();
        }
    }
}
