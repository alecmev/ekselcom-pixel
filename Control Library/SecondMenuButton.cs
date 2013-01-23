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
    public class SecondMenuButton : ButtonBase
    {
        private SolidColorBrush NormalColor;
        private SolidColorBrush OverColor;
        private SolidColorBrush ClickColor;

        private ColumnDefinition CenterColumn;
        private bool autoWidth = true;
        public bool AutoWidth
        {
            get { return autoWidth; }
            set
            {
                autoWidth = value;
                if (value) CenterColumn.Width = GridLength.Auto;
                else CenterColumn.Width = new GridLength(1f, GridUnitType.Star);
            }
        }

        private TextBlock TitleText;
        public String Title
        {
            get { return TitleText.Text; }
            set { TitleText.Text = value; }
        }

        private Border HighlightBorder;
        public Brush Highlight
        {
            get { return HighlightBorder.Background; }
            set
            {
                HighlightBorder.Background = value;
                if (((SolidColorBrush)value) == ClickColor) TitleText.Foreground = Brushes.Black;
                else TitleText.Foreground = Brushes.White;
            }
        }
        
        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (value) Highlight = ClickColor;
                else if (!IsMouseOver) Highlight = NormalColor;
                else if (Mouse.LeftButton != MouseButtonState.Pressed) Highlight = OverColor;
            }
        }

        private Border FilterBorder;
        public Brush Filter
        {
            get { return FilterBorder.Background; }
            set
            {
                FilterBorder.Background = value;
                if (value != Brushes.Transparent)
                {
                    NormalColor = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
                    OverColor = new SolidColorBrush(Color.FromArgb(127, 31, 31, 31));
                    ClickColor = new SolidColorBrush(Color.FromArgb(127, 63, 63, 63));
                }
                else
                {
                    NormalColor = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
                    OverColor = new SolidColorBrush(Color.FromArgb(127, 0, 102, 153));
                    ClickColor = new SolidColorBrush(Color.FromArgb(127, 0, 153, 204));
                }
            }
        }

        public SecondMenuButton()
        {
            NormalColor = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            OverColor = new SolidColorBrush(Color.FromArgb(127, 0, 102, 153));
            ClickColor = new SolidColorBrush(Color.FromArgb(127, 0, 153, 204));

            Grid tmpGrid = new Grid();
            ColumnDefinition cd;
            RowDefinition rd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(8f);
            tmpGrid.ColumnDefinitions.Add(cd);

            CenterColumn = new ColumnDefinition();
            CenterColumn.Width = GridLength.Auto;
            tmpGrid.ColumnDefinitions.Add(CenterColumn);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(8f);
            tmpGrid.ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(1f);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(1f, GridUnitType.Star);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(1f);
            tmpGrid.RowDefinitions.Add(rd);

            HighlightBorder = new Border();
            HighlightBorder.Background = NormalColor;
            HighlightBorder.BorderThickness = new Thickness(0f);
            Grid.SetColumn(HighlightBorder, 0);
            Grid.SetColumnSpan(HighlightBorder, 3);
            Grid.SetRow(HighlightBorder, 0);
            Grid.SetRowSpan(HighlightBorder, 3);
            tmpGrid.Children.Add(HighlightBorder);

            FilterBorder = new Border();
            Filter = Brushes.Transparent;
            FilterBorder.Opacity = 0.25;
            FilterBorder.BorderThickness = new Thickness(0f);
            Grid.SetColumn(FilterBorder, 0);
            Grid.SetColumnSpan(FilterBorder, 3);
            Grid.SetRow(FilterBorder, 0);
            Grid.SetRowSpan(FilterBorder, 3);
            tmpGrid.Children.Add(FilterBorder);

            Border tmpBorder;

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            tmpBorder.BorderThickness = new Thickness(0f, 0f, 1f, 1f);
            Grid.SetColumn(tmpBorder, 0);
            Grid.SetColumnSpan(tmpBorder, 3);
            Grid.SetRow(tmpBorder, 0);
            Grid.SetRowSpan(tmpBorder, 3);
            tmpGrid.Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(63, 255, 255, 255));
            tmpBorder.BorderThickness = new Thickness(1f, 1f, 0f, 0f);
            Grid.SetColumn(tmpBorder, 0);
            Grid.SetColumnSpan(tmpBorder, 3);
            Grid.SetRow(tmpBorder, 0);
            Grid.SetRowSpan(tmpBorder, 3);
            tmpGrid.Children.Add(tmpBorder);

            TitleText = new TextBlock();
            TitleText.FontFamily = new FontFamily("Sans Serif");
            TitleText.FontSize = 12f;
            TitleText.Foreground = new SolidColorBrush(Colors.White);
            TitleText.HorizontalAlignment = HorizontalAlignment.Center;
            TitleText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(TitleText, 1);
            Grid.SetRow(TitleText, 1);
            tmpGrid.Children.Add(TitleText);

            AddChild(tmpGrid);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (!selected) Highlight = OverColor;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!selected) Highlight = ClickColor;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (!selected) Highlight = OverColor;
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!selected) Highlight = NormalColor;
            base.OnMouseLeave(e);
        }
    }
}
