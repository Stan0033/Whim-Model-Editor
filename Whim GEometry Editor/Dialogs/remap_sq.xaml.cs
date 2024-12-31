using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for remap_sq.xaml
    /// </summary>
    public partial class remap_sq : Window
    {
        public int Duration;
        public remap_sq()
        {
            InitializeComponent();
        }

        private void set(object sender, RoutedEventArgs e)
        {
            if (input.Text.Length == 0) { MessageBox.Show("Invalid input"); return; }
            bool parsed = int.TryParse(input.Text, out int duration);
            if (parsed)
            {
                if (duration >= 50)
                {
                    DialogResult = true;
                }
                else
                {
                    Duration = duration;
                    MessageBox.Show("Duration cannot be elss than 50", "Invalid input"); return;
                }
            }
            else
            {
                MessageBox.Show("Invalid input"); return;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
