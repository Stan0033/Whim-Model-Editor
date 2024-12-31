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
    /// Interaction logic for DuplicateAlongAxis_Window.xaml
    /// </summary>
    public partial class DuplicateAlongAxis_Window : Window
    {
        public float Distance;
        public int axis;
        public int method = 1;
        public int Copies;
        public DuplicateAlongAxis_Window()
        {
            InitializeComponent();
        }

        private void ok(object sender, RoutedEventArgs e)
        {
            bool parsed = float.TryParse(InputDistance.Text, out float distance);
            bool parsed1 = int.TryParse(InputCopies.Text, out int copies);
            if (parsed && parsed1)
            {
                if ( distance <= 0) { return; }
                if ( copies <= 0 && copies > 50) { return; }
                {
                    
                }
                axis = List_Axis.SelectedIndex;
                method = radio1.IsChecked == true ? 0 : 1;
                Distance = distance;
               
                Copies = copies;
                DialogResult = true;
            }
        }
    }
}
