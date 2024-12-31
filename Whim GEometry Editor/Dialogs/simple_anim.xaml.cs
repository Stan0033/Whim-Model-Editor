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
    /// Interaction logic for simple_anim.xaml
    /// </summary>
    public partial class simple_anim : Window
    {
        float min;
        float max
            ;
        public float X =0;
        public float Y =0;
        public float Z =0;
        public float Times_;
        public simple_anim(float m, float mx)
        {
            InitializeComponent();
            min = m;
            max = mx;
        }
        private bool allInLimit()
        {
            if ((X > min &&  X < max) && (Y > min && Y < max) && (Z > min && Z < max)) { return true; }
            return false;
        }
        private void ok(object sender, RoutedEventArgs e)
        {
            bool parsedx = float.TryParse(x.Text, out float xx);
            if (parsedx) X = xx;
            bool parsedy = float.TryParse(y.Text, out float yy);
            if (parsedy) Y = yy;
            bool parsedz = float.TryParse(z.Text, out float zz);
            
            if (parsedz) Z = zz;
            bool parsedt = int.TryParse(times.Text, out int Times);
            if (parsedt) Times_ = Times;
            if (parsedx && parsedy && parsedz && parsedt)
            {
                if (allInLimit())
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show($"Allowed range {min} - {max}", "Invalid request");
                }
            }
         
        }
        }
    }

