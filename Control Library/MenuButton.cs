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
    public class MenuButton : ButtonBase
    {
        private SolidColorBrush NormalColor;
        private SolidColorBrush OverColor;
        private SolidColorBrush ClickColor;

        private Image IconImage;
        public ImageSource Icon
        {
            get { return IconImage.Source; }
            set { IconImage.Source = value; }
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
                //if (((SolidColorBrush)value) == ClickColor) TitleText.Foreground = Brushes.Black;
                //else TitleText.Foreground = Brushes.White;
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

        public MenuButton()
        {
            NormalColor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            OverColor = new SolidColorBrush(Color.FromArgb(63, 255, 255, 255));
            ClickColor = new SolidColorBrush(Color.FromArgb(127, 255, 255, 255));

            Grid tmpGrid = new Grid();
            ColumnDefinition cd;
            RowDefinition rd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            tmpGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(48f);
            tmpGrid.ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(4f);
            tmpGrid.ColumnDefinitions.Add(cd);

            rd = new RowDefinition();
            rd.Height = new GridLength(4f);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(36f);
            tmpGrid.RowDefinitions.Add(rd);

            rd = new RowDefinition();
            rd.Height = new GridLength(16f);
            tmpGrid.RowDefinitions.Add(rd);

            IconImage = new Image();
            IconImage.Width = 48;
            IconImage.Height = 36;
            Grid.SetColumn(IconImage, 1);
            Grid.SetRow(IconImage, 1);
            tmpGrid.Children.Add(IconImage);

            TitleText = new TextBlock();
            TitleText.FontFamily = new FontFamily("Sans Serif");
            TitleText.FontSize = 12f;
            TitleText.Foreground = new SolidColorBrush(Colors.White);
            TitleText.HorizontalAlignment = HorizontalAlignment.Center;
            TitleText.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(TitleText, 1);
            Grid.SetRow(TitleText, 2);
            tmpGrid.Children.Add(TitleText);

            HighlightBorder = new Border();
            HighlightBorder.Background = NormalColor;
            HighlightBorder.BorderThickness = new Thickness(0f);
            HighlightBorder.CornerRadius = new CornerRadius(4);
            Grid.SetColumn(HighlightBorder, 0);
            Grid.SetColumnSpan(HighlightBorder, 3);
            Grid.SetRow(HighlightBorder, 0);
            Grid.SetRowSpan(HighlightBorder, 3);
            tmpGrid.Children.Add(HighlightBorder);

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
