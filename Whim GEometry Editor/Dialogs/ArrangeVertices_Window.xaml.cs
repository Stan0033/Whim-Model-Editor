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
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for ArrangeVertices_Window.xaml
    /// </summary>
    public partial class ArrangeVertices_Window : Window
    {
        public CustomAngle Angle = new(0,0,0);
        public float Distance;
        public ArrangeVertices_Window()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            bool dis = float.TryParse(DistanceInput.Text, out float distance);
            bool has1 = float.TryParse(XInput.Text, out float x);
            bool has2 = float.TryParse(YInput.Text, out float y);
            bool has3 = float.TryParse(ZInput.Text, out float z);
            if (dis && has1 && has2 && has3)
            {
                if (AllInFrame(x,y,z) && distance > 0.1f)
                {
                    Angle = new CustomAngle(x, y, z);
                    Distance = distance;
                    DialogResult = true;
                }
            }
        }
        private bool AllInFrame(float x, float y, float z)
        {
            return x >= 0 && x <= 360 && y >=0 && y <= 360 && z >=0 && z<=360;
        }
        private void setx(object sender, RoutedEventArgs e)
        {
            XInput.Text = "90";
            YInput.Text = "0";
            ZInput.Text = "0";
        }

        private void sety(object sender, RoutedEventArgs e)
        {
            XInput.Text = "0";
            YInput.Text = "90";
            ZInput.Text = "0";
        }

        private void setz(object sender, RoutedEventArgs e)
        {
            XInput.Text = "0";
            YInput.Text = "0";
            ZInput.Text = "90";
        }
    }
}
