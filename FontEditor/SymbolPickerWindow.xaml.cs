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
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace MyFa
{
    public partial class SymbolPickerWindow : Window
    {
        private SymbolStorage symbol;
        public SymbolStorage Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public SymbolPickerWindow(SymbolStorage newSymbol)
        {
            InitializeComponent();

            Symbol = new SymbolStorage(newSymbol);
            Board.Width = newSymbol.Width;
            Board.Height = newSymbol.Height;

            ListBoxItem lbiTmp;

            foreach (System.Drawing.FontFamily tmpFontFamily in System.Drawing.FontFamily.Families)
            {
                lbiTmp = new ListBoxItem();
                lbiTmp.Content = tmpFontFamily.Name;
                lbiTmp.Tag = tmpFontFamily;
                FontFamilyList.Items.Add(lbiTmp);
            }

            double[] fontSizes = {
             5.0,   5.5,   6.0,   6.5,   7.0,   7.5,   8.0,   8.5,   9.0,   9.5,
             10.0,  11.0,  12.0,  13.0,  14.0,  15.0,  16.0,  17.0,  18.0,  19.0,
             20.0,  22.0,  24.0,  26.0,  28.0,  30.0,  32.0,  34.0,  36.0,  38.0,
             40.0,  44.0,  48.0,  52.0,  56.0,  60.0,  64.0,  68.0,  72.0,  76.0,
             80.0,  88.0,  96.0, 104.0, 112.0,  120.0, 128.0, 136.0, 144.0 };

            for (int i = 0; i < fontSizes.Length; ++i)
            {
                lbiTmp = new ListBoxItem();
                lbiTmp.Content = fontSizes[i].ToString("F");
                lbiTmp.Tag = fontSizes[i];
                FontSizeList.Items.Add(lbiTmp);
            }

            FontFamilyList.SelectedItem = FontFamilyList.Items[0];
            FontSizeList.SelectedItem = FontSizeList.Items[0];

            if (Symbol.Ascii > 0) TheSymbol.Text = (System.Text.Encoding.GetEncoding(Symbol.Ascii).GetChars(new byte[] { Symbol.Code }))[0].ToString();
            else TheSymbol.Text = "A";

            UpdatePreview(this, null);
            UpdateBoard();
        }

        public SymbolPickerWindow(SymbolStorage newSymbol, SymbolPickerWindow source)
        {
            InitializeComponent();

            Symbol = new SymbolStorage(newSymbol);
            Board.Width = newSymbol.Width;
            Board.Height = newSymbol.Height;

            ListBoxItem lbiTmp;

            foreach (System.Drawing.FontFamily tmpFontFamily in System.Drawing.FontFamily.Families)
            {
                lbiTmp = new ListBoxItem();
                lbiTmp.Content = tmpFontFamily.Name;
                lbiTmp.Tag = tmpFontFamily;
                FontFamilyList.Items.Add(lbiTmp);
                if (tmpFontFamily.Name == (string)((ListBoxItem)source.FontFamilyList.SelectedItem).Content) FontFamilyList.SelectedItem = lbiTmp;
            }

            double[] fontSizes = {
             5.0,   5.5,   6.0,   6.5,   7.0,   7.5,   8.0,   8.5,   9.0,   9.5,
             10.0,  11.0,  12.0,  13.0,  14.0,  15.0,  16.0,  17.0,  18.0,  19.0,
             20.0,  22.0,  24.0,  26.0,  28.0,  30.0,  32.0,  34.0,  36.0,  38.0,
             40.0,  44.0,  48.0,  52.0,  56.0,  60.0,  64.0,  68.0,  72.0,  76.0,
             80.0,  88.0,  96.0, 104.0, 112.0,  120.0, 128.0, 136.0, 144.0 };

            for (int i = 0; i < fontSizes.Length; ++i)
            {
                lbiTmp = new ListBoxItem();
                lbiTmp.Content = fontSizes[i].ToString("F");
                lbiTmp.Tag = fontSizes[i];
                FontSizeList.Items.Add(lbiTmp);
            }

            FontFamilyList.SelectedItem = source.FontFamilyList.SelectedItem;
            FontSizeList.SelectedItem = source.FontSizeList.SelectedItem;

            FontStyleBold.Selected = source.FontStyleBold.Selected;
            FontStyleItalic.Selected = source.FontStyleItalic.Selected;
            FontStyleUnderline.Selected = source.FontStyleUnderline.Selected;
            FontStyleStrikeout.Selected = source.FontStyleStrikeout.Selected;

            SymbolHAlignLeft.Selected = source.SymbolHAlignLeft.Selected;
            SymbolHAlignCenter.Selected = source.SymbolHAlignCenter.Selected;
            SymbolHAlignRight.Selected = source.SymbolHAlignRight.Selected;

            SymbolVAlignTop.Selected = source.SymbolVAlignTop.Selected;
            SymbolVAlignCenter.Selected = source.SymbolVAlignCenter.Selected;
            SymbolVAlignBottom.Selected = source.SymbolVAlignBottom.Selected;

            FontSizeLeft.Text = source.FontSizeLeft.Text;
            FontSizeRight.Text = source.FontSizeRight.Text;

            if (Symbol.Ascii > 0) TheSymbol.Text = (System.Text.Encoding.GetEncoding(Symbol.Ascii).GetChars(new byte[] { Symbol.Code }))[0].ToString();
            else TheSymbol.Text = "A";

            UpdatePreview(this, null);
            UpdateBoard();
        }

        public void UpdateBoard()
        {
            if (!IsInitialized) return;
            try
            {
                System.Drawing.Bitmap tmpBitmap = new System.Drawing.Bitmap((int)(Board.Width * 4), (int)(Board.Height * 4));
                System.Drawing.Graphics tmpGraphics = System.Drawing.Graphics.FromImage(tmpBitmap);

                System.Drawing.FontFamily tmpFontFamily = (System.Drawing.FontFamily)((ListBoxItem)FontFamilyList.SelectedItem).Tag;
                System.Drawing.FontStyle tmpFontStyle =
                    (FontStyleBold.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Bold) ?
                        System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular) |
                    (FontStyleItalic.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Italic) ?
                        System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular) |
                    (FontStyleUnderline.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Underline) ?
                        System.Drawing.FontStyle.Underline : System.Drawing.FontStyle.Regular) |
                    (FontStyleStrikeout.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Strikeout) ?
                        System.Drawing.FontStyle.Strikeout : System.Drawing.FontStyle.Regular);
                float tmpFontSize = 0f;
                if (!float.TryParse(FontSizeLeft.Text + "." + FontSizeRight.Text, out tmpFontSize)) float.TryParse(FontSizeLeft.Text + "," + FontSizeRight.Text, out tmpFontSize);
                if (tmpFontSize == 0f) return;
                System.Drawing.Font tmpFont;
                try { tmpFont = new System.Drawing.Font(tmpFontFamily, tmpFontSize, tmpFontStyle); }
                catch
                {
                    try
                    {
                        tmpFontStyle |= (!FontStyleBold.Selected ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);
                        tmpFont = new System.Drawing.Font(tmpFontFamily, tmpFontSize, tmpFontStyle);
                    }
                    catch { return; }
                }

                Symbol.Font = tmpFontFamily.Name;
                Symbol.Size = tmpFontSize;

                tmpGraphics.Clear(System.Drawing.Color.White);
                tmpGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                tmpGraphics.DrawString(TheSymbol.Text, tmpFont, new System.Drawing.SolidBrush(System.Drawing.Color.Black), new System.Drawing.PointF(0, 0));
                tmpGraphics.Save();

                int theMostLeft = tmpBitmap.Width, theMostTop = tmpBitmap.Height, theMostRight = -1, theMostBottom = -1;
                float[,] brightness = new float[tmpBitmap.Width, tmpBitmap.Height];

                for (int y = 0; y < tmpBitmap.Height; ++y)
                {
                    for (int x = 0; x < tmpBitmap.Width; ++x)
                    {
                        brightness[x, y] = tmpBitmap.GetPixel(x, y).GetBrightness();
                        if (brightness[x, y] <= 0.7)
                        {
                            if (x < theMostLeft) theMostLeft = x;
                            if (theMostTop == tmpBitmap.Height) theMostTop = y;
                            if (x > theMostRight) theMostRight = x;
                            if (y > theMostBottom) theMostBottom = y;
                        }
                    }
                }

                int tmpWidth = theMostRight - theMostLeft, tmpHeight = theMostBottom - theMostTop, marginX = 0, marginY = 0;

                if (SymbolHAlignLeft.Selected) marginX = theMostLeft;
                else if (SymbolHAlignCenter.Selected) marginX = theMostLeft - ((int)Board.Width - tmpWidth) / 2;
                else marginX = theMostLeft - ((int)Board.Width - tmpWidth) + 1;

                if (SymbolVAlignTop.Selected) marginY = theMostTop;
                else if (SymbolVAlignCenter.Selected) marginY = theMostTop - ((int)Board.Height - tmpHeight) / 2;
                else marginY = theMostTop - ((int)Board.Height - tmpHeight) + 1;

                Rectangle tmpRect;
                Board.Children.Clear();

                for (int y = 0; y < (int)Board.Height; ++y)
                {
                    for (int x = 0; x < (int)Board.Width; ++x)
                    {
                        if (x + marginX >= tmpBitmap.Width || y + marginY >= tmpBitmap.Height || x + marginX < 0 || y + marginY < 0 || brightness[x + marginX, y + marginY] > 0.7) Symbol.Map[y][x] = false;
                        else
                        {
                            Symbol.Map[y][x] = true;
                            tmpRect = new Rectangle();
                            tmpRect.Width = 1;
                            tmpRect.Height = 1;
                            Canvas.SetLeft(tmpRect, x);
                            Canvas.SetTop(tmpRect, y);
                            tmpRect.Fill = Brushes.Black;
                            Board.Children.Add(tmpRect);
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        public void UpdatePreview(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((ListBoxItem)FontFamilyList.SelectedItem == null) FontFamilyList.SelectedItem = FontFamilyList.Items[0];

                TheSymbol.FontFamily = new FontFamily(((ListBoxItem)FontFamilyList.SelectedItem).Content.ToString());
                TheSymbol.FontWeight = FontStyleBold.Selected ? FontWeights.Bold : FontWeights.Regular;
                TheSymbol.FontStyle = FontStyleItalic.Selected ? FontStyles.Italic : FontStyles.Normal;
                TheSymbol.TextDecorations = new TextDecorationCollection();

                if (FontStyleUnderline.Selected) TheSymbol.TextDecorations.Add(TextDecorations.Underline);
                if (FontStyleStrikeout.Selected) TheSymbol.TextDecorations.Add(TextDecorations.Strikethrough);

                if (SymbolHAlignLeft.Selected) TheSymbol.HorizontalContentAlignment = HorizontalAlignment.Left;
                else if (SymbolHAlignCenter.Selected) TheSymbol.HorizontalContentAlignment = HorizontalAlignment.Center;
                else TheSymbol.HorizontalContentAlignment = HorizontalAlignment.Right;

                if (SymbolVAlignTop.Selected) TheSymbol.VerticalContentAlignment = VerticalAlignment.Top;
                else if (SymbolVAlignCenter.Selected) TheSymbol.VerticalContentAlignment = VerticalAlignment.Center;
                else TheSymbol.VerticalContentAlignment = VerticalAlignment.Bottom;
            }
            catch { MessageBox.Show("ERROR", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void FontFamilyChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview(sender, null);
            UpdateBoard();
        }

        private void FontSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview(sender, null);
            String tmp = ((ListBoxItem)FontSizeList.SelectedItem).Content.ToString();
            Int32 index = tmp.IndexOf(".");
            if (index > -1)
            {
                FontSizeLeft.Text = tmp.Substring(0, index);
                FontSizeRight.Text = tmp.Substring(index + 1);
            }
            else
            {
                index = tmp.IndexOf(",");
                if (index > -1)
                {
                    FontSizeLeft.Text = tmp.Substring(0, index);
                    FontSizeRight.Text = tmp.Substring(index + 1);
                }
            }
            UpdateBoard();
        }

        private void FontSizeInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"\D")) e.Handled = true;
        }

        private void FontSizeCheck(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(FontSizeLeft.Text, @"\D")) FontSizeLeft.Text = Regex.Replace(FontSizeLeft.Text, @"\D", "");
            if (Regex.IsMatch(FontSizeRight.Text, @"\D")) FontSizeRight.Text = Regex.Replace(FontSizeRight.Text, @"\D", "");
            if (FontSizeLeft.Text.Length == 0) FontSizeLeft.Text = "0";
            else FontSizeLeft.Text = int.Parse(FontSizeLeft.Text).ToString();
            if (FontSizeRight.Text.Length == 1) FontSizeRight.Text += "0";
            else if (FontSizeRight.Text.Length == 0) FontSizeRight.Text = "00";
            UpdateBoard();
        }

        private void PreviewBorderMouseEnter(object sender, MouseEventArgs e)
        {
            PreviewBorder.Background = new SolidColorBrush(Color.FromArgb(95, 191, 143, 0));
        }

        private void PreviewBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviewBorder.Background = new SolidColorBrush(Color.FromArgb(127, 191, 143, 0));
        }

        private void PreviewBorderMouseUp(object sender, MouseButtonEventArgs e)
        {
            PreviewBorder.Background = new SolidColorBrush(Color.FromArgb(95, 191, 143, 0));
            UpdateBoard();
        }

        private void PreviewBorderMouseLeave(object sender, MouseEventArgs e)
        {
            PreviewBorder.Background = new SolidColorBrush(Color.FromArgb(63, 191, 143, 0));
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            UpdateBoard();
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FontStyleClick(object sender, RoutedEventArgs e)
        {
            ((SecondMenuButton)sender).Selected = !((SecondMenuButton)sender).Selected;
            UpdatePreview(sender, null);
            UpdateBoard();
        }

        private void SymbolHAlignClick(object sender, RoutedEventArgs e)
        {
            SymbolHAlignLeft.Selected = false;
            SymbolHAlignCenter.Selected = false;
            SymbolHAlignRight.Selected = false;
            ((SecondMenuButton)sender).Selected = true;
            UpdatePreview(sender, null);
            UpdateBoard();
        }

        private void SymbolVAlignClick(object sender, RoutedEventArgs e)
        {
            SymbolVAlignTop.Selected = false;
            SymbolVAlignCenter.Selected = false;
            SymbolVAlignBottom.Selected = false;
            ((SecondMenuButton)sender).Selected = true;
            UpdatePreview(sender, null);
            UpdateBoard();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            BoardScale.ScaleX = (PreviewBorder.ActualWidth - 8) / Board.Width;
            BoardScale.ScaleY = (PreviewBorder.ActualHeight - 8) / Board.Height;
            if (BoardScale.ScaleX > BoardScale.ScaleY) BoardScale.ScaleX = BoardScale.ScaleY;
            else BoardScale.ScaleY = BoardScale.ScaleX;
        }

        private void TheSymbolChanged(object sender, TextChangedEventArgs e)
        {
            if (TheSymbol.Text.Length > 1) TheSymbol.Text = TheSymbol.Text.Substring(1, 1);
            else if (TheSymbol.Text.Length < 1) TheSymbol.Text = "A";
            TheSymbol.SelectionStart = 1;
            UpdateBoard();
        }

        private void TheSymbolInput(object sender, RoutedEventArgs e)
        {
            if (TheSymbol.SelectionStart != 1) TheSymbol.SelectionStart = 1;
        }
    }
}