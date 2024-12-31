using System;
using System.Collections.Generic;
using System.Linq;
 
using System.Windows;
 
using System.Windows.Input;
 

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class UserInput : Window
    {
        public UserInput()
        {
            InitializeComponent();
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            if (Box.Text.Trim().Length > 0)
            {
                DialogResult = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
            if (e.Key == Key.Enter) { Accept(null,null); }
        }
    }
}
