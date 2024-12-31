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
    /// Interaction logic for InputCoordinate.xaml
    /// </summary>
    public partial class InputCoordinate : Window
    {
        public Coordinate Coordinate;
        public InputCoordinate()
        {
            InitializeComponent();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(XInput.Text, out float x);
            bool parsed2 = float.TryParse(YInput.Text, out float y);
            bool parsed3 = float.TryParse(ZInput.Text, out float z);
            if (parsed1 && parsed2 && parsed3)
            {
                Coordinate = new Coordinate(x,y,z);
                DialogResult = true;
            }
        }
    }
}
