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

namespace Whim_GEometry_Editor.Misc
{
    /// <summary>
    /// Interaction logic for CustomCamera.xaml
    /// </summary>
    public partial class CustomCamera : Window
    {
        public CustomCamera()
        {
            InitializeComponent();
        }

        private void Set(object sender, RoutedEventArgs e)
        {
          
            // Try to parse X, Y, and Z, and only update the valid ones
            if (int.TryParse(InputX.Text, out int xValue))
            {
                CameraControl.eyeX = xValue;
            }

            if (int.TryParse(InputY.Text, out int yValue))
            {
                CameraControl.eyeY = yValue;
            }

            if (int.TryParse(InputZ.Text, out int zValue))
            {
                CameraControl.eyeZ = zValue;
            }
            if (int.TryParse(InputCX.Text, out int cx)){
                CameraControl.CenterX = cx;
            }
            if (int.TryParse(InputCY.Text, out int cy)){
                CameraControl.CenterY = cy;
            }
            if (int.TryParse(InputCZ.Text, out int cz)){
                CameraControl.CenterZ = cz;
            }
            if (int.TryParse(InputUX.Text, out int ux)){
                CameraControl.UpX = ux;
            }
            if (int.TryParse(InputUY.Text, out int uy))
            {
                CameraControl.UpY = uy;
            }
            if (int.TryParse(InputUZ.Text, out int uz))
            {
                CameraControl.UpZ = uz;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void IncrementX(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX++;
            InputX.Text = CameraControl.eyeX.ToString();
        }

        private void DecrementX(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX--;
            InputX.Text = CameraControl.eyeX.ToString();
        }

        private void IncrementY(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeY++;
            InputY.Text = CameraControl.eyeY.ToString();
        }

        private void DecrementY(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeY--;
            InputY.Text = CameraControl.eyeY.ToString();
        }

        private void IncrementZ(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeZ++;
            InputZ.Text = CameraControl.eyeZ.ToString();
        }

        private void DecrementZ(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeZ--;
            InputZ.Text = CameraControl.eyeZ.ToString();
        }

        private void IncrementCX(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterX++;
            InputCX.Text = CameraControl.CenterX.ToString();
        }

        private void DecrementCX(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterX--;
            InputCX.Text = CameraControl.CenterX.ToString();
        }

        private void IncrementCY(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterY++;
            InputCY.Text = CameraControl.CenterY.ToString();
        }

        private void DecrementCY(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterY--;
            InputCY.Text = CameraControl.CenterY.ToString();
        }

        private void IncrementCZ(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterZ++;
            InputCZ.Text = CameraControl.CenterZ.ToString();
        }

        private void DecrementCZ(object sender, RoutedEventArgs e)
        {
            CameraControl.CenterZ--;
            InputCZ.Text = CameraControl.CenterZ.ToString();
        }

        private void IncrementUX(object sender, RoutedEventArgs e)
        {
            CameraControl.UpX++;
            InputUX.Text = CameraControl.UpX.ToString();
        }

        private void DecrementUX(object sender, RoutedEventArgs e)
        {
            CameraControl.UpX--;
            InputUX.Text = CameraControl.UpX.ToString();
        }

        private void IncrementUY(object sender, RoutedEventArgs e)
        {
            CameraControl.UpY++;
            InputUY.Text = CameraControl.UpY.ToString();
        }

        private void DecrementUY(object sender, RoutedEventArgs e)
        {
            CameraControl.UpY--;
            InputUY.Text = CameraControl.UpY.ToString();
        }

        private void IncrementUZ(object sender, RoutedEventArgs e)
        {
            CameraControl.UpZ++;
            InputUZ.Text = CameraControl.UpZ.ToString();
        }
      

        private void DecrementUZ(object sender, RoutedEventArgs e)
        {
            CameraControl.UpZ--;
            InputUZ.Text = CameraControl.UpZ.ToString();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
    }
    }
