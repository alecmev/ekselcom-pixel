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
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyFa
{
    public class PagePreview : Grid
    {
        private const double EDGE = 8f;

        private Border BackgroundBorder;
        private Border HighlightBorder;
        private Image DisplayImage;

        private bool?[,] States;
        private int DisplayWidth;
        private int DisplayHeight;
        new public Int32Rect Clip;
        public bool Fits = true;

        private SolidColorBrush OnBrush;
        private SolidColorBrush OffBrush;
        private SolidColorBrush DisabledBrush;

        private EllipseGeometry[,] OnEllipses;
        private EllipseGeometry[,] OffEllipses;
        private EllipseGeometry[,] DisabledEllipses;

        private string FontName = null;
        private Collection<SymbolStorage> FontSymbols;

        public PagePreview(int newWidth, int newHeight, Int32Rect newClip)
        {
            States = new bool?[newWidth, newHeight];
            DisplayWidth = newWidth;
            DisplayHeight = newHeight;

            Clip = newClip;
            if (Clip.X >= DisplayWidth) Clip.X = DisplayWidth - 1;
            if (Clip.Y >= DisplayHeight) Clip.Y = DisplayHeight - 1;
            if (Clip.X + Clip.Width > DisplayWidth) Clip.Width = DisplayWidth - Clip.X - 1;
            if (Clip.Y + Clip.Height > DisplayHeight) Clip.Height = DisplayHeight - Clip.Y - 1;

            OnBrush = new SolidColorBrush(Color.FromArgb(255, 255, 128, 0));
            OffBrush = new SolidColorBrush(Color.FromArgb(255, 15, 15, 15));
            DisabledBrush = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));

            OnEllipses = new EllipseGeometry[newWidth, newHeight];
            OffEllipses = new EllipseGeometry[newWidth, newHeight];
            DisabledEllipses = new EllipseGeometry[newWidth, newHeight];

            ColumnDefinition cd;
            RowDefinition rd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(32f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(32f);
            ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(8f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(4f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(4f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(8f);
            RowDefinitions.Add(rd);

            BackgroundBorder = new Border();
            BackgroundBorder.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
            BackgroundBorder.CornerRadius = new CornerRadius(2f);
            Grid.SetColumn(BackgroundBorder, 0);
            Grid.SetColumnSpan(BackgroundBorder, 5);
            Grid.SetRow(BackgroundBorder, 0);
            Grid.SetRowSpan(BackgroundBorder, 5);
            Children.Add(BackgroundBorder);

            HighlightBorder = new Border();
            HighlightBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            HighlightBorder.CornerRadius = new CornerRadius(4f);
            Grid.SetColumn(HighlightBorder, 1);
            Grid.SetColumnSpan(HighlightBorder, 3);
            Grid.SetRow(HighlightBorder, 1);
            Grid.SetRowSpan(HighlightBorder, 3);
            Children.Add(HighlightBorder);

            DisplayImage = new Image();
            DisplayImage.Width = (double)newWidth * EDGE;
            DisplayImage.Height = (double)newHeight * EDGE;
            Grid.SetColumn(DisplayImage, 2);
            Grid.SetRow(DisplayImage, 2);
            Children.Add(DisplayImage);

            /*GeometryDrawing tmpEllipse;

            tmpEllipse = new GeometryDrawing(new SolidColorBrush(Color.FromArgb(255, 255, 192, 0)), null, new EllipseGeometry(new Rect(0, 0, EDGE, EDGE)));
            OnBrush = new DrawingBrush(tmpEllipse);
            OnBrush.Viewport = new Rect(0, 0, EDGE, EDGE);
            OnBrush.TileMode = TileMode.None;
            OnBrush.Freeze();

            tmpEllipse = new GeometryDrawing(new SolidColorBrush(Color.FromArgb(255, 15, 15, 15)), null, new EllipseGeometry(new Rect(0, 0, EDGE, EDGE)));
            OffBrush = new DrawingBrush(tmpEllipse);
            OffBrush.Viewport = new Rect(0, 0, EDGE, EDGE);
            OffBrush.TileMode = TileMode.None;
            OffBrush.Freeze();

            tmpEllipse = new GeometryDrawing(new SolidColorBrush(Color.FromArgb(255, 127, 127, 127)), null, new EllipseGeometry(new Rect(0, 0, EDGE, EDGE)));
            DisabledBrush = new DrawingBrush(tmpEllipse);
            DisabledBrush.Viewport = new Rect(0, 0, EDGE, EDGE);
            DisabledBrush.TileMode = TileMode.None;
            DisabledBrush.Freeze();*/

            //GeometryDrawing tmpBackground = new GeometryDrawing(new SolidColorBrush(Color.FromArgb(255, 255, 192, 0)), null, new RectangleGeometry(new Rect(0, 0, EDGE, EDGE)));
            //DrawingGroup tmpDrawing = new DrawingGroup();
            //tmpDrawing.Children.Add(tmpBackground);
            //tmpDrawing.Children.Add(tmpEllipse);

            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    States[x, y] = null;
                    OnEllipses[x, y] = new EllipseGeometry(new Rect((double)x * EDGE + 0.5f, (double)y * EDGE + 0.5f, EDGE - 1f, EDGE - 1f));
                    OffEllipses[x, y] = new EllipseGeometry(new Rect((double)x * EDGE + 0.5f, (double)y * EDGE + 0.5f, EDGE - 1f, EDGE - 1f));
                    DisabledEllipses[x, y] = new EllipseGeometry(new Rect((double)x * EDGE + 0.5f, (double)y * EDGE + 0.5f, EDGE - 1f, EDGE - 1f));
                }
            }
        }

        public PageStorage Render(PageStorage page)
        {
            if (page != null)
            {
                for (int i = 0; i < page.Strings.Count; ++i)
                {
                    StringStorage str = page.Strings[i];
                    String path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Fonts\" + str.Font + @".mfpf";

                    if (File.Exists(path))
                    {
                        if (str.Font != FontName)
                        {
                            Stream stream = File.Open(path, FileMode.Open);
                            BinaryFormatter bformatter = new BinaryFormatter();
                            FontSymbols = (Collection<SymbolStorage>)bformatter.Deserialize(stream);
                            stream.Close();
                            FontName = str.Font;
                        }

                        byte[] symbols = System.Text.Encoding.GetEncoding(FontSymbols[0].Ascii).GetBytes(str.Text);
                        int margin = -1, tmpleft = (str.Shift > 0 ? str.Shift : 0) + str.X, left;

                        for (int j = 0; j < symbols.Length; ++j)
                        {
                            left = tmpleft;
                            margin = -1;
                            for (int x = 0; x < FontSymbols[symbols[j]].Width; ++x)
                            {
                                for (int y = 0; y < FontSymbols[symbols[j]].Height; ++y)
                                {
                                    if (FontSymbols[symbols[j]].Map[y][x])
                                    {
                                        if (margin < 0) margin = x;
                                        if (tmpleft < x - margin + left) tmpleft = left + x - margin;
                                        TurnOn(tmpleft, y + str.Y + str.VShift);
                                    }
                                }
                            }
                            tmpleft += (str.Indent + 1);
                        }

                        tmpleft -= (str.Indent + 1);
                        Fits = (tmpleft < Clip.X + Clip.Width);

                        if (str.Shift < 0)
                        {
                            if (tmpleft + 1 < str.X + str.Width)
                            {
                                page.Strings[i].Shift = (int)((double)(str.Width - (tmpleft - str.X)) / 2f);
                                for (int x = str.X + str.Width - 1; x >= str.X; --x)
                                {
                                    for (int y = str.Y; y < str.Y + str.Height; ++y)
                                    {
                                        if (States[x, y] == true)
                                        {
                                            TurnOff(x, y);
                                            TurnOn(x + page.Strings[i].Shift, y);
                                        }
                                    }
                                }
                            }
                            else page.Strings[i].Shift = 0;
                        }

                        Redraw();
                    }
                    else MessageBox.Show("FONT DOES NOT EXIST");
                }
                return page;
            }
            return null;
        }

        private void Redraw()
        {
            //long test = DateTime.Now.Ticks;

            DrawingGroup tmpDrawing = new DrawingGroup();
            GeometryGroup onGroup = new GeometryGroup();
            GeometryGroup offGroup = new GeometryGroup();
            GeometryGroup disabledGroup = new GeometryGroup();

            for (int x = 0; x < DisplayWidth; ++x)
            {
                for (int y = 0; y < DisplayHeight; ++y)
                {
                    if (States[x, y] == true) onGroup.Children.Add(OnEllipses[x, y]);
                    else if (States[x, y] == false) offGroup.Children.Add(OffEllipses[x, y]);
                    else disabledGroup.Children.Add(DisabledEllipses[x, y]);
                    /*if (States[x, y] == true) onGroup.Children.Add(new EllipseGeometry(new Rect((double)x * EDGE, (double)y * EDGE, EDGE, EDGE)));
                    else if (States[x, y] == false) offGroup.Children.Add(new EllipseGeometry(new Rect((double)x * EDGE, (double)y * EDGE, EDGE, EDGE)));
                    else disabledGroup.Children.Add(new EllipseGeometry(new Rect((double)x * EDGE, (double)y * EDGE, EDGE, EDGE)));*/
                }
            }

            tmpDrawing.Children.Add(new GeometryDrawing(OnBrush, null, onGroup));
            tmpDrawing.Children.Add(new GeometryDrawing(OffBrush, null, offGroup));
            tmpDrawing.Children.Add(new GeometryDrawing(DisabledBrush, null, disabledGroup));
            DisplayImage.Source = new DrawingImage(tmpDrawing);

            //test = DateTime.Now.Ticks - test;
            //MessageBox.Show(test.ToString());
        }

        private void TurnOn(int X, int Y)
        {
            if (X >= Clip.X && X < Clip.X + Clip.Width && Y >= Clip.Y && Y < Clip.Y + Clip.Height) States[X, Y] = true;
        }

        private void TurnOff(int X, int Y)
        {
            if (X >= Clip.X && X < Clip.X + Clip.Width && Y >= Clip.Y && Y < Clip.Y + Clip.Height) States[X, Y] = false;
        }

        public void Clear(bool redraw)
        {
            for (int x = 0; x < DisplayWidth; ++x) for (int y = 0; y < DisplayHeight; ++y) TurnOff(x, y);
            if (redraw) Redraw();
        }
    }
}

/*using System;
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
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyFa
{
    public class PagePreview : Grid
    {
        private Border BackgroundBorder;
        private Border HighlightBorder;
        private EllipseGeometry[,] Points;
        private bool?[,] States;
        new public Int32Rect Clip;

        private System.Windows.Shapes.Path OnPath;
        private GeometryGroup On
        {
            get { return (GeometryGroup)OnPath.Data; }
            set { OnPath.Data = value; }
        }

        private System.Windows.Shapes.Path OffPath;
        private GeometryGroup Off
        {
            get { return (GeometryGroup)OffPath.Data; }
            set { OffPath.Data = value; }
        }

        private System.Windows.Shapes.Path DisabledPath;
        private GeometryGroup Disabled
        {
            get { return (GeometryGroup)DisabledPath.Data; }
            set { DisabledPath.Data = value; }
        }

        public PagePreview(int newWidth, int newHeight, Int32Rect newClip)
        {
            Points = new EllipseGeometry[newWidth, newHeight];
            States = new bool?[newWidth, newHeight];
            Clip = newClip;

            ColumnDefinition cd;
            RowDefinition rd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(32f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(32f);
            ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(8f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(4f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(4f);
            RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(8f);
            RowDefinitions.Add(rd);

            BackgroundBorder = new Border();
            BackgroundBorder.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
            BackgroundBorder.CornerRadius = new CornerRadius(2f);
            Grid.SetColumn(BackgroundBorder, 0);
            Grid.SetColumnSpan(BackgroundBorder, 5);
            Grid.SetRow(BackgroundBorder, 0);
            Grid.SetRowSpan(BackgroundBorder, 5);
            Children.Add(BackgroundBorder);

            HighlightBorder = new Border();
            HighlightBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            HighlightBorder.CornerRadius = new CornerRadius(4f);
            Grid.SetColumn(HighlightBorder, 1);
            Grid.SetColumnSpan(HighlightBorder, 3);
            Grid.SetRow(HighlightBorder, 1);
            Grid.SetRowSpan(HighlightBorder, 3);
            Children.Add(HighlightBorder);

            OnPath = new System.Windows.Shapes.Path();
            OnPath.Width = newWidth;
            OnPath.Height = newHeight;
            OnPath.LayoutTransform = new ScaleTransform(8f, 8f);
            OnPath.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 192, 0));
            On = new GeometryGroup();
            Grid.SetColumn(OnPath, 2);
            Grid.SetRow(OnPath, 2);
            Children.Add(OnPath);

            OffPath = new System.Windows.Shapes.Path();
            OffPath.Width = newWidth;
            OffPath.Height = newHeight;
            OffPath.LayoutTransform = new ScaleTransform(8f, 8f);
            OffPath.Fill = new SolidColorBrush(Color.FromArgb(255, 15, 15, 15));
            Off = new GeometryGroup();
            Grid.SetColumn(OffPath, 2);
            Grid.SetRow(OffPath, 2);
            Children.Add(OffPath);

            DisabledPath = new System.Windows.Shapes.Path();
            DisabledPath.Width = newWidth;
            DisabledPath.Height = newHeight;
            DisabledPath.LayoutTransform = new ScaleTransform(8f, 8f);
            DisabledPath.Fill = new SolidColorBrush(Color.FromArgb(255, 127, 127, 127));
            Disabled = new GeometryGroup();
            Grid.SetColumn(DisabledPath, 2);
            Grid.SetRow(DisabledPath, 2);
            Children.Add(DisabledPath);

            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {
                    Points[x, y] = new EllipseGeometry();
                    Points[x, y].RadiusX = 0.375f;
                    Points[x, y].RadiusY = 0.375f;
                    Points[x, y].Center = new Point((double)x + 0.5f, (double)y + 0.5f);
                    States[x, y] = null;
                    Disabled.Children.Add(Points[x, y]);
                    TurnOff(x, y);
                }
            }
        }

        public PageStorage Render(PageStorage page)
        {
            if (page != null)
            {
                for (int i = 0; i < page.Strings.Count; ++i)
                {
                    StringStorage str = page.Strings[i];
                    String path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFaPixel\Fonts\" + str.Font + @".mfpf";

                    if (File.Exists(path))
                    {
                        Stream stream = File.Open(path, FileMode.Open);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        Collection<SymbolStorage> font = (Collection<SymbolStorage>)bformatter.Deserialize(stream);
                        stream.Close();

                        byte[] symbols = System.Text.Encoding.GetEncoding(font[0].Ascii).GetBytes(str.Text);
                        int margin = -1, tmpleft = (str.Shift > 0 ? str.Shift : 0) + str.X, left;

                        for (int j = 0; j < symbols.Length; ++j)
                        {
                            left = tmpleft;
                            margin = -1;
                            for (int x = 0; x < font[symbols[j]].Width; ++x)
                            {
                                for (int y = 0; y < font[symbols[j]].Height; ++y)
                                {
                                    if (font[symbols[j]].Map[y][x])
                                    {
                                        if (margin < 0) margin = x;
                                        if (tmpleft < x - margin + left) tmpleft = left + x - margin;
                                        TurnOn(tmpleft, y + str.Y + str.VShift);
                                    }
                                }
                            }
                            tmpleft += (str.Indent + 1);
                        }

                        tmpleft -= (str.Indent + 1);

                        if (str.Shift < 0)
                        {
                            if (tmpleft + 1 < str.X + str.Width)
                            {
                                page.Strings[i].Shift = (int)((double)(str.Width - (tmpleft - str.X)) / 2f);
                                for (int x = str.X + str.Width - 1; x >= str.X; --x)
                                {
                                    for (int y = str.Y; y < str.Y + str.Height; ++y)
                                    {
                                        if (States[x, y] == true)
                                        {
                                            TurnOff(x, y);
                                            TurnOn(x + page.Strings[i].Shift, y);
                                        }
                                    }
                                }
                            }
                            else page.Strings[i].Shift = 0;
                        }
                    }
                    else MessageBox.Show("FONT DOES NOT EXISTS");
                }
                return page;
            }
            return null;
        }

        private void TurnOn(int X, int Y)
        {
            if (X < Points.GetLength(0) && Y < Points.GetLength(1) && X > -1 && Y > -1 && X >= Clip.X && X < Clip.X + Clip.Width && Y >= Clip.Y && Y < Clip.Y + Clip.Height && States[X, Y] != true)
            {
                if (States[X, Y] == false) Off.Children.Remove(Points[X, Y]);
                else Disabled.Children.Remove(Points[X, Y]);
                On.Children.Add(Points[X, Y]);
                States[X, Y] = true;
            }
        }

        private void TurnOff(int X, int Y)
        {
            if (X < Points.GetLength(0) && Y < Points.GetLength(1) && X > -1 && Y > -1 && X >= Clip.X && X < Clip.X + Clip.Width && Y >= Clip.Y && Y < Clip.Y + Clip.Height && States[X, Y] != false)
            {
                if (States[X, Y] == true) On.Children.Remove(Points[X, Y]);
                else Disabled.Children.Remove(Points[X, Y]);
                Off.Children.Add(Points[X, Y]);
                States[X, Y] = false;
            }
        }

        public void Clear()
        {
            int tmpWidth = Points.GetLength(0);
            int tmpHeight = Points.GetLength(1);

            for (int x = 0; x < tmpWidth; ++x)
            {
                for (int y = 0; y < tmpHeight; ++y)
                {
                    TurnOff(x, y);
                }
            }
        }
    }
}*/
