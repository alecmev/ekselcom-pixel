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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;

namespace MyFa
{
    public partial class FontEditor : Window
    {
        private Collection<SymbolTableButton> symbolButtons;
        public Collection<SymbolTableButton> SymbolButtons
        {
            get { return symbolButtons; }
            set { symbolButtons = value; }
        }

        private SymbolEditWindow sew;
        private NewSymbolTableWindow nstw;
        private SymbolPickerWindow spw;

        private bool AllInitialized = false;

        private string SaveFile = "";
        private bool isSaved = true;
        public bool IsSaved
        {
            get { return isSaved; }
            set
            {
                isSaved = value;
                Title = "MyFaPixel - Font Editor - " + ((SaveFile != "") ? (SaveFile.Substring(SaveFile.LastIndexOf(@"\") + 1) + (value ? "" : " *")) : "untitled *");
            }
        }

        public FontEditor()
        {
            InitializeComponent();
            spw = new SymbolPickerWindow(new SymbolStorage(1, 1, 65, 1250, "Arial", 8f));
            spw.Close();

            Closing += new System.ComponentModel.CancelEventHandler(FontEditorClosing);
        }

        void FontEditorClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSaved)
            {
                MessageBoxResult result = MessageBox.Show("Current font is not saved. Do you want to save?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel) e.Cancel = true;
                if (result == MessageBoxResult.Yes)
                {
                    SaveClick(null, null);
                    if (!IsSaved) e.Cancel = true;
                }
            }
        }

        public void CreateSymbolTable()
        {
            if (!AllInitialized)
            {
                ColumnDefinition cd;
                RowDefinition rd;

                for (int i = 0; i < 16; ++i)
                {
                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(28f);
                    SymbolTable.ColumnDefinitions.Add(cd);

                    if (i < 15)
                    {
                        cd = new ColumnDefinition();
                        cd.Width = new GridLength(4f);
                        SymbolTable.ColumnDefinitions.Add(cd);
                    }

                    rd = new RowDefinition();
                    rd.Height = new GridLength(38f);
                    SymbolTable.RowDefinitions.Add(rd);

                    if (i < 15)
                    {
                        rd = new RowDefinition();
                        rd.Height = new GridLength(4f);
                        SymbolTable.RowDefinitions.Add(rd);
                    }
                }

                AllInitialized = true;
            }
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            if (IsSaved ? true : (MessageBox.Show("Current font is not saved. Continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                nstw = new NewSymbolTableWindow();
                nstw.Owner = this;
                if (nstw.ShowDialog() == true)
                {
                    int tmpWidth, tmpHeight;
                    if (!int.TryParse(nstw.SymbolWidth.Text, out tmpWidth) || !int.TryParse(nstw.SymbolHeight.Text, out tmpHeight)) return;

                    CreateSymbolTable();

                    SymbolTable.Children.Clear();
                    SymbolButtons = new Collection<SymbolTableButton>();

                    SymbolTableButton stb;

                    for (int i = 0; i < 16; ++i)
                    {
                        stb = new SymbolTableButton(new SymbolStorage(tmpWidth, tmpHeight, (byte)(i * 16 + i), 0, "Arial", 8f));
                        stb.Title = "0x" + i.ToString("X") + i.ToString("X");
                        stb.Click += new RoutedEventHandler(SymbolClicked);
                        Grid.SetColumn(stb, i * 2);
                        Grid.SetRow(stb, i * 2);
                        SymbolTable.Children.Add(stb);
                        SymbolButtons.Add(stb);

                        for (int j = i + 1; j < 16; ++j)
                        {
                            stb = new SymbolTableButton(new SymbolStorage(tmpWidth, tmpHeight, (byte)(i * 16 + j), 0, "Arial", 8f));
                            stb.Title = "0x" + i.ToString("X") + j.ToString("X");
                            stb.Click += new RoutedEventHandler(SymbolClicked);
                            Grid.SetColumn(stb, j * 2);
                            Grid.SetRow(stb, i * 2);
                            SymbolTable.Children.Add(stb);
                            SymbolButtons.Add(stb);

                            stb = new SymbolTableButton(new SymbolStorage(tmpWidth, tmpHeight, (byte)(j * 16 + i), 0, "Arial", 8f));
                            stb.Title = "0x" + j.ToString("X") + i.ToString("X");
                            stb.Click += new RoutedEventHandler(SymbolClicked);
                            Grid.SetColumn(stb, i * 2);
                            Grid.SetRow(stb, j * 2);
                            SymbolTable.Children.Add(stb);
                            SymbolButtons.Add(stb);
                        }
                    }

                    SymbolButtons = new Collection<SymbolTableButton>(SymbolButtons.OrderBy(tmp => tmp.Title).ToList<SymbolTableButton>());

                    if (((string)((ComboBoxItem)nstw.LoadASCII.SelectedItem).Content) != "leave empty")
                    {
                        int tmpAscii = int.Parse((string)((ComboBoxItem)nstw.LoadASCII.SelectedItem).Content);
                        spw = new SymbolPickerWindow(new SymbolStorage(tmpWidth, tmpHeight, 65, tmpAscii, "Arial", 8f), spw);
                        spw.Owner = this;
                        if (spw.ShowDialog() == true)
                        {
                            byte[] codes = new byte[256];
                            for (int i = 0; i < 256; ++i) codes[i] = (byte)i;

                            char[] ASCII = System.Text.Encoding.GetEncoding(tmpAscii).GetChars(codes);

                            System.Drawing.Bitmap tmpBitmap = new System.Drawing.Bitmap((int)(spw.Board.Width * 4), (int)(spw.Board.Height * 4));
                            System.Drawing.Graphics tmpGraphics = System.Drawing.Graphics.FromImage(tmpBitmap);
                            tmpGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            System.Drawing.FontFamily tmpFontFamily = (System.Drawing.FontFamily)((ListBoxItem)spw.FontFamilyList.SelectedItem).Tag;
                            System.Drawing.FontStyle tmpFontStyle =
                                (spw.FontStyleBold.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Bold) ?
                                    System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular) |
                                (spw.FontStyleItalic.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Italic) ?
                                    System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular) |
                                (spw.FontStyleUnderline.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Underline) ?
                                    System.Drawing.FontStyle.Underline : System.Drawing.FontStyle.Regular) |
                                (spw.FontStyleStrikeout.Selected && tmpFontFamily.IsStyleAvailable(System.Drawing.FontStyle.Strikeout) ?
                                    System.Drawing.FontStyle.Strikeout : System.Drawing.FontStyle.Regular);
                            float tmpFontSize;
                            if (!float.TryParse(spw.FontSizeLeft.Text + "." + spw.FontSizeRight.Text, out tmpFontSize)) float.TryParse(spw.FontSizeLeft.Text + "," + spw.FontSizeRight.Text, out tmpFontSize);
                            System.Drawing.Font tmpFont = new System.Drawing.Font(tmpFontFamily, tmpFontSize, tmpFontStyle);

                            int theMostLeft, theMostTop, theMostRight, theMostBottom;
                            int tmpWidth2, tmpHeight2, marginX, marginY;
                            System.Drawing.SolidBrush tmpBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                            System.Drawing.PointF tmpPoint = new System.Drawing.PointF(0, 0);
                            float[,] brightness = new float[tmpBitmap.Width, tmpBitmap.Height];

                            for (int i = 0; i < 256; ++i)
                            {
                                try
                                {
                                    SymbolButtons[i].Symbol.Ascii = tmpAscii;
                                    SymbolButtons[i].Symbol.Font = tmpFontFamily.Name;
                                    SymbolButtons[i].Symbol.Size = tmpFontSize;

                                    tmpGraphics.Clear(System.Drawing.Color.White);
                                    tmpGraphics.DrawString(ASCII[i].ToString(), tmpFont, tmpBrush, tmpPoint);
                                    tmpGraphics.Save();

                                    theMostLeft = tmpBitmap.Width;
                                    theMostTop = tmpBitmap.Height;
                                    theMostRight = -1;
                                    theMostBottom = -1;

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

                                    tmpWidth2 = theMostRight - theMostLeft;
                                    tmpHeight2 = theMostBottom - theMostTop;

                                    if (spw.SymbolHAlignLeft.Selected) marginX = theMostLeft;
                                    else if (spw.SymbolHAlignCenter.Selected) marginX = theMostLeft - ((int)spw.Board.Width - tmpWidth2) / 2;
                                    else marginX = theMostLeft - ((int)spw.Board.Width - tmpWidth2) + 1;

                                    if (spw.SymbolVAlignTop.Selected) marginY = theMostTop;
                                    else if (spw.SymbolVAlignCenter.Selected) marginY = theMostTop - ((int)spw.Board.Height - tmpHeight2) / 2;
                                    else marginY = theMostTop - ((int)spw.Board.Height - tmpHeight2) + 1;

                                    for (int y = 0; y < (int)spw.Board.Height; ++y)
                                    {
                                        for (int x = 0; x < (int)spw.Board.Width; ++x)
                                        {
                                            if (marginX == tmpBitmap.Width || marginY == tmpBitmap.Height || x + marginX < 0 || y + marginY < 0 || brightness[x + marginX, y + marginY] > 0.7) SymbolButtons[i].Symbol.Map[y][x] = false;
                                            else SymbolButtons[i].Symbol.Map[y][x] = true;
                                        }
                                    }
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error); }
                                SymbolButtons[i].UpdateBoard();
                            }
                        }
                        spw.Close();
                    }
                }

                SaveFile = "";
                IsSaved = false;
            }
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            if (IsSaved ? true : (MessageBox.Show("Current font is not saved. Continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes))
            {
                FontOpenWindow fow = new FontOpenWindow();
                fow.Owner = this;

                if (fow.ShowDialog() == true)
                {
                    if (fow.Type == 0)
                    {
                        CreateSymbolTable();

                        Stream stream = File.Open(fow.Path, FileMode.Open);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        Collection<SymbolStorage> tmp = (Collection<SymbolStorage>)bformatter.Deserialize(stream);
                        stream.Close();

                        SymbolTable.Children.Clear();
                        SymbolButtons = new Collection<SymbolTableButton>();

                        for (int i = 0; i < tmp.Count; ++i)
                        {
                            SymbolButtons.Add(new SymbolTableButton(tmp[i]));
                            SymbolButtons.Last().Click += new RoutedEventHandler(SymbolClicked);
                            SymbolButtons.Last().Title = "0x" + (i / 16).ToString("X") + (i % 16).ToString("X");
                            Grid.SetColumn(SymbolButtons.Last(), (i % 16) * 2);
                            Grid.SetRow(SymbolButtons.Last(), (i / 16) * 2);
                            SymbolTable.Children.Add(SymbolButtons.Last());
                        }
                    }

                    SaveFile = fow.Path;
                    IsSaved = true;
                }
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (AllInitialized)
            {
                if (!IsSaved)
                {
                    if (SaveFile != "")
                    {
                        Collection<SymbolStorage> tmp = new Collection<SymbolStorage>();
                        for (int i = 0; i < SymbolButtons.Count; ++i) tmp.Add(SymbolButtons[i].Symbol);
                        Stream stream = File.Open(SaveFile, FileMode.Create);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, tmp);
                        stream.Close();

                        IsSaved = true;
                    }
                    else SaveAsClick(sender, e);
                }
            }
        }

        private void SaveAsClick(object sender, RoutedEventArgs e)
        {
            if (AllInitialized)
            {
                FontSaveWindow fsw = new FontSaveWindow((SaveFile != "") ? SaveFile.Substring(SaveFile.LastIndexOf(@"\") + 1, SaveFile.Length - SaveFile.LastIndexOf(@"\") - 6) : "");
                fsw.Owner = this;

                if (fsw.ShowDialog() == true)
                {
                    if (fsw.Type == 0)
                    {
                        Collection<SymbolStorage> tmp = new Collection<SymbolStorage>();
                        for (int i = 0; i < SymbolButtons.Count; ++i) tmp.Add(SymbolButtons[i].Symbol);
                        Stream stream = File.Open(fsw.Path, FileMode.Create);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, tmp);
                        stream.Close();

                        SaveFile = fsw.Path;
                        IsSaved = true;
                    }
                }
            }
        }

        private void SymbolClicked(object sender, RoutedEventArgs e)
        {
            sew = new SymbolEditWindow(((SymbolTableButton)sender).Symbol, spw);
            sew.Owner = this;
            if (sew.ShowDialog() == true)
            {
                ((SymbolTableButton)sender).Symbol = sew.Symbol;
                IsSaved = false;
            }
            spw = sew.spw;
            sew.Close();
            ((SymbolTableButton)sender).UpdateLayout();
        }
    }
}
