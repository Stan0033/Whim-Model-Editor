using MDLLib;
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
    /// Interaction logic for ToFitIn.xaml
    /// </summary>
    public partial class ToFitIn : Window
    {
        public Extent ex = new Extent();
        bool WindowInitialized = false;
        public ToFitIn()
        {
            InitializeComponent();
            WindowInitialized = true;
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            if (check_Box.IsChecked == true)
            {
                if (IsCorrectExtent(i_minx.Text, i_miny.Text, i_minz.Text, i_maxx.Text, i_maxy.Text, i_maxz.Text)) 
                { 
                    ex.Minimum_X = float.Parse(i_minx.Text);
                    ex.Minimum_Y = float.Parse(i_miny.Text);
                    ex.Minimum_Z = float.Parse(i_minz.Text);
                    ex.Maximum_X = float.Parse(i_maxx.Text);
                    ex.Maximum_Y = float.Parse(i_maxy.Text);
                    ex.Maximum_Z = float.Parse(i_maxz.Text);
                    
                    DialogResult = true;
                }
                 
            }
            else
            {
                try
                {
                    float bounds = float.Parse(i_bounds.Text);
                    DialogResult = true;
                }
                catch
                {
                    MessageBox.Show("Enter bounds radius", "Input not valid");
                }
               
            }
        }
        private bool IsCorrectExtent(string minx, string miny, string minz, string maxx, string maxy, string maxz)
        {
            // Try to parse each string as a float
            if (!float.TryParse(minx, out float minXValue) ||
                !float.TryParse(miny, out float minYValue) ||
                !float.TryParse(minz, out float minZValue) ||
                !float.TryParse(maxx, out float maxXValue) ||
                !float.TryParse(maxy, out float maxYValue) ||
                !float.TryParse(maxz, out float maxZValue))
            {
                MessageBox.Show("All values must be valid floats.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Check that min values are less than max values
            if (minXValue >= maxXValue || minYValue >= maxYValue || minZValue >= maxZValue)
            {
                MessageBox.Show("Min values must be less than Max values for each axis.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // All conditions passed
            return true;
        }

        private void UnlockBounds(object sender, RoutedEventArgs e)
        {
            if (!WindowInitialized) { return;}
            i_bounds.IsEnabled = true;
            Panel_Box.IsEnabled = false;
        }

        private void CheckBoxBounds(object sender, RoutedEventArgs e)
        {
            if (!WindowInitialized) { return; }
            i_bounds.IsEnabled = false;
            Panel_Box.IsEnabled = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
