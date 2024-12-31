using MDLLib;
using SharpGL.SceneGraph.Transformations;
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
    /// Interaction logic for edit_Attachment.xaml
    /// </summary>
    public partial class edit_Attachment : Window
    {
        w3Node Att;
        w3Attachment w3Attachment;
        w3Model model;
        public edit_Attachment(w3Node node, w3Model model_)
        {
            InitializeComponent();
            Att = node;
            w3Attachment = node.Data as w3Attachment;
          
            model = model_;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }

        private void set(object sender, RoutedEventArgs e)
        {
            string path = InputName.Text.Trim();
            w3Attachment.Path = path;
        }

        private void editvisiblity(object sender, RoutedEventArgs e)
        {
           Transformation_window tw = new Transformation_window(model, w3Attachment.Visibility, TransformationType.Visibility );
        }
    }
}
