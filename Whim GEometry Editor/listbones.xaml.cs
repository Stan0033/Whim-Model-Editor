using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for listbones.xaml
    /// </summary>
    public partial class listbones : Window
    {
        public listbones(List<w3Node> nodes, w3Node exclude)
        {
            InitializeComponent();
            foreach (w3Node node in nodes) 
            {

                // emitter1, emitter2, ribbon, light can be under any, but bone, helper, cols, eo must be under helper or bone
                if (node == exclude) { continue; }
                if (exclude.Data is Bone || 
                    exclude.Data is Helper ||
                    exclude.Data is Collision_Shape || 
                    exclude.Data is Event_Object ||
                    exclude.Data is w3Attachment
                    
                    )
                {
                    if (node.Data is Bone || node.Data is Helper)
                    {
                        Box.Items.Add(new ListBoxItem() { Content = node.Name });
                    }
                }
                else
                {
                    Box.Items.Add(new ListBoxItem() { Content = node.Name });
                }
                
              
               
            }
            if (Box.Items.Count > 0)
            {
                Box.SelectedIndex = 0;
            }
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            if (Box.SelectedItems.Count ==1)
            {
                DialogResult= true;
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
