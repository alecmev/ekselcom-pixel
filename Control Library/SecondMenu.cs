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
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace MyFa
{
    public class SecondMenu : Grid
    {
        private TextBlock TitleText;
        public String Title
        {
            get { return TitleText.Text; }
            set { TitleText.Text = value; }
        }

        private ObservableCollection<SecondMenuButton> buttons;
        public ObservableCollection<SecondMenuButton> Buttons
        {
            get { return buttons; }
            set { buttons = value; }
        }

        public SecondMenu()
        {
            ColumnDefinition cd;

            cd = new ColumnDefinition();
            cd.Width = new GridLength(16f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = new GridLength(16f);
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            ColumnDefinitions.Add(cd);

            cd = new ColumnDefinition();
            ColumnDefinitions.Add(cd);

            Border tmpBorder;

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Color.FromArgb(159, 0, 0, 0));
            tmpBorder.BorderThickness = new Thickness(0f);
            Grid.SetColumn(tmpBorder, 0);
            Grid.SetColumnSpan(tmpBorder, 3);
            Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            tmpBorder.BorderThickness = new Thickness(0f, 0f, 1f, 1f);
            Grid.SetColumn(tmpBorder, 0);
            Grid.SetColumnSpan(tmpBorder, 3);
            Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(63, 255, 255, 255));
            tmpBorder.BorderThickness = new Thickness(0f, 1f, 0f, 0f);
            Grid.SetColumn(tmpBorder, 0);
            Grid.SetColumnSpan(tmpBorder, 3);
            Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            tmpBorder.BorderThickness = new Thickness(0f);
            Grid.SetColumn(tmpBorder, 4);
            Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(31, 0, 0, 0));
            tmpBorder.BorderThickness = new Thickness(0f, 0f, 0f, 1f);
            Grid.SetColumn(tmpBorder, 4);
            Children.Add(tmpBorder);

            tmpBorder = new Border();
            tmpBorder.Background = new SolidColorBrush(Colors.Transparent);
            tmpBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(63, 255, 255, 255));
            tmpBorder.BorderThickness = new Thickness(1f, 1f, 0f, 0f);
            Grid.SetColumn(tmpBorder, 4);
            Children.Add(tmpBorder);

            TitleText = new TextBlock();
            TitleText.FontFamily = new FontFamily("Courier New");
            TitleText.FontSize = 14f;
            TitleText.Foreground = new SolidColorBrush(Colors.White);
            TitleText.HorizontalAlignment = HorizontalAlignment.Center;
            TitleText.VerticalAlignment = VerticalAlignment.Center;
            TitleText.MaxHeight = 18f;
            Grid.SetColumn(TitleText, 1);
            Children.Add(TitleText);
            
            ItemsControl tmpItemsControl;

            tmpItemsControl = new ItemsControl();
            Buttons = new ObservableCollection<SecondMenuButton>();
            tmpItemsControl.ItemsSource = Buttons;
            tmpItemsControl.ItemsPanel = (ItemsPanelTemplate)XamlReader.Parse(@"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><StackPanel Orientation='Horizontal' /></ItemsPanelTemplate>");
            Grid.SetColumn(tmpItemsControl, 3);
            Children.Add(tmpItemsControl);
        }
    }
}
