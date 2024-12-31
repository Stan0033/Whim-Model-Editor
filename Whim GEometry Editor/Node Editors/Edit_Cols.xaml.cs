using MDLLib;
 
using System.Windows;
 

namespace Whim_Model_Editor
{
   
    /// <summary>
    /// Interaction logic for edit_node.xaml
    /// </summary>
    public partial class edit_cols : Window
    {
        bool isCollision = false;
        Collision_Shape cs;
        public edit_cols(w3Node node)
        {
            InitializeComponent();
            Collision_Shape shape = node.Data as Collision_Shape;
            cs = shape;
            E_minx.Text = $"{shape.Extents.Minimum_X:f6}";
            E_miny.Text = $"{shape.Extents.Minimum_Y:f6}";
            E_minz.Text = $"{shape.Extents.Minimum_Z:f6}";
            E_maxx.Text = $"{shape.Extents.Maximum_X:f6}";
            E_maxy.Text = $"{shape.Extents.Maximum_Y:f6}";
            E_maxz.Text = $"{shape.Extents.Maximum_Z:f6}";
            E_b.Text = $"{shape.Extents.Bounds_Radius:f6}";

            if (shape.Type == CollisionShapeType.Box)
            {
                Check_box.IsChecked = true;
            }
            else
            { Check_Sphere.IsChecked = true;
                E_maxx.IsEnabled = false; E_maxy.IsEnabled = false; E_maxz.IsEnabled = false;
            }
        }
        private bool isNumber(string num)
        {
            bool iz = float.TryParse(num, out float val);
            return iz;
        }
        private void Set(object sender, RoutedEventArgs e)
        {
            
               
                    if (
              
               isNumber(E_minx.Text) &&
               isNumber(E_miny.Text) &&
               isNumber(E_minz.Text) &&
               isNumber(E_maxx.Text) &&
               isNumber(E_maxy.Text) &&
               isNumber(E_maxz.Text)  
                
               )
                    {
                        cs.Type = Check_box.IsChecked == true ? CollisionShapeType.Box : CollisionShapeType.Sphere;
                cs.Extents.Bounds_Radius = float.Parse(E_b.Text);
                cs.Extents.Minimum_X = float.Parse(E_minx.Text);
                cs.Extents.Minimum_Y = float.Parse(E_miny.Text);
                cs.Extents.Minimum_Z = float.Parse(E_minz.Text);
                cs.Extents.Maximum_X = float.Parse(E_maxx.Text);
                cs.Extents.Maximum_Y = float.Parse(E_maxy.Text);
                cs.Extents.Maximum_Z = float.Parse(E_maxz.Text);
                        DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("One or more invalid inputs.", "Invalid request");
                    }
                
                
            
             
        }

        private void CheckedBox(object sender, RoutedEventArgs e)
        {
            E_b.IsEnabled = false;
            E_maxx.IsEnabled = true;
            E_maxy.IsEnabled = true;
            E_maxz.IsEnabled = true;
        }

        private void Checked_Sphere(object sender, RoutedEventArgs e)
        {
            E_b.IsEnabled = true;
            E_maxx.IsEnabled = false;
            E_maxy.IsEnabled = false;
            E_maxz.IsEnabled = false;
        }
    }
}
