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

namespace MyFa
{
    public class SymbolTableButton : ButtonBase
    {
        private SolidColorBrush NormalColor;
        private SolidColorBrush OverColor;
        private SolidColorBrush ClickColor;

        private Canvas Board;
        private Border HighlightBorder;

        private TextBlock TitleText;
        public String Title
        {
            get { return TitleText.Text; }
            set { TitleText.Text = value; }
        }

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (value) HighlightBorder.Background = ClickColor;
                else if (!IsMouseOver) HighlightBorder.Background = NormalColor;
                else if (Mouse.LeftButton != MouseButtonState.Pressed) HighlightBorder.Background = OverColor;
            }
        }

        private SymbolStorage symbol;
        public SymbolStorage Symbol
        {
            get { return symbol; }
            set { symbol = value; UpdateBoard(); }
        }

        public SymbolTableButton(SymbolStorage newSymbol)
        {
            NormalColor = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            OverColor = new SolidColorBrush(Color.FromArgb(63, 0, 0, 0));
            ClickColor = new SolidColorBrush(Color.FromArgb(127, 191, 143, 0));

            Grid tmpGrid = new Grid();
            ColumnDefinition cd;
            RowDefinition rd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(2f);
            tmpGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(24f);
            tmpGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(2f);
            tmpGrid.ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(12f);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(24f);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(2f);
            tmpGrid.RowDefinitions.Add(rd);

            HighlightBorder = new Border();
            HighlightBorder.Background = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            HighlightBorder.CornerRadius = new CornerRadius(2f);
            Grid.SetColumn(HighlightBorder, 0);
            Grid.SetColumnSpan(HighlightBorder, 3);
            Grid.SetRow(HighlightBorder, 0);
            Grid.SetRowSpan(HighlightBorder, 5);
            tmpGrid.Children.Add(HighlightBorder);

            TitleText = new TextBlock();
            TitleText.FontFamily = new FontFamily("Sans Serif");
            TitleText.Foreground = new SolidColorBrush(Colors.Black);
            TitleText.FontSize = 10f;
            TitleText.HorizontalAlignment = HorizontalAlignment.Center;
            TitleText.VerticalAlignment = VerticalAlignment.Center;
            TitleText.MaxWidth = 24f;
            TitleText.MaxHeight = 12f;
            Grid.SetColumn(TitleText, 1);
            Grid.SetRow(TitleText, 0);
            tmpGrid.Children.Add(TitleText);

            Board = new Canvas();
            Board.Background = new SolidColorBrush(Colors.White);
            Board.LayoutTransform = new ScaleTransform();
            Board.Width = newSymbol.Width;
            Board.Height = newSymbol.Height;
            if (newSymbol.Width > newSymbol.Height)
            {
                ((ScaleTransform)Board.LayoutTransform).ScaleX = 24d / (Double)newSymbol.Width;
                ((ScaleTransform)Board.LayoutTransform).ScaleY = ((ScaleTransform)Board.LayoutTransform).ScaleX;
            }
            else
            {
                ((ScaleTransform)Board.LayoutTransform).ScaleY = 24d / (Double)newSymbol.Height;
                ((ScaleTransform)Board.LayoutTransform).ScaleX = ((ScaleTransform)Board.LayoutTransform).ScaleY;
            }
            Grid.SetColumn(Board, 1);
            Grid.SetRow(Board, 1);
            tmpGrid.Children.Add(Board);

            AddChild(tmpGrid);

            Symbol = newSymbol;

            MouseEnter += new MouseEventHandler(UserControl_MouseEnter);
            MouseLeave += new MouseEventHandler(UserControl_MouseLeave);
            MouseLeftButtonDown += new MouseButtonEventHandler(UserControl_MouseLeftButtonDown);
            MouseLeftButtonUp += new MouseButtonEventHandler(UserControl_MouseLeftButtonUp);
            LayoutUpdated += new EventHandler(UserControl_LayoutUpdated);
        }

        private void DrawPixel(int X, int Y)
        {
            Rectangle tmp = new Rectangle();
            tmp.Width = 1;
            tmp.Height = 1;
            tmp.Fill = Brushes.Black;
            Canvas.SetLeft(tmp, X);
            Canvas.SetTop(tmp, Y);
            Board.Children.Insert(0, tmp);
        }

        public void UpdateBoard()
        {
            Board.Children.Clear();

            if (Symbol.Map[0][0]) DrawPixel(0, 0);

            for (int i = 1; i < Symbol.Width; ++i)
            {
                if (Symbol.Map[0][i]) DrawPixel(i, 0);
            }

            for (int i = 1; i < Symbol.Height; ++i)
            {
                for (int j = 0; j < Symbol.Width; ++j)
                {
                    if (Symbol.Map[i][j]) DrawPixel(j, i);
                }
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!selected) HighlightBorder.Background = OverColor;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!selected) HighlightBorder.Background = NormalColor;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!selected) HighlightBorder.Background = ClickColor;
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!selected) HighlightBorder.Background = OverColor;
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (IsMouseOver) UserControl_MouseEnter(this, null);
            else UserControl_MouseLeave(this, null);
        }
    }
}
