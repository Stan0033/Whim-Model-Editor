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
    /// Interaction logic for AdjustAll.xaml
    /// </summary>
    public partial class AdjustAll : Window
    {
        public AdjustAll()
        {
            InitializeComponent();
        }
        public float X, Y, Z;
        private void ok(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(inputx.Text, out float x);
            bool parsed2 = float.TryParse(inputy.Text, out float y);
            bool parsed3 = float.TryParse(inputz.Text, out float z);
            if (parsed1 && parsed2 && parsed3)
            {
                X = x;
                Y = y;
                Z = z;
                DialogResult = true;


            }
        }
    }
}
