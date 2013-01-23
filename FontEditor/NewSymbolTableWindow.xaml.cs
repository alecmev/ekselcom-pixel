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

namespace MyFa
{
    public partial class NewSymbolTableWindow : Window
    {
        public NewSymbolTableWindow()
        {
            InitializeComponent();
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NumberInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"\D")) e.Handled = true;
        }

        private void NumberLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tmp = (TextBox)sender;
            if (Regex.IsMatch(tmp.Text, @"\D")) tmp.Text = Regex.Replace(tmp.Text, @"\D", "");
            if (tmp.Text == "" || int.Parse(tmp.Text) == 0) tmp.Text = "1";
        }
    }
}
