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
    /// Interaction logic for quickClean.xaml
    /// </summary>
    public partial class quickClean : Window
    {
        w3Model model;
        public quickClean(w3Model Model)
        {
            InitializeComponent();
            this.model = Model;
        }

        private void Do(object sender, RoutedEventArgs e)
        {
            if (RemoveEmitter1sCheckBox.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is Particle_Emitter_1);
            if (RemoveEmitter2sCheckBox.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is Particle_Emitter_2);
            if (RemoveRibbonsCheckBox.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is Ribbon_Emitter);
            if (RemoveRibbonsCheckBox.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is w3Light);
            if (RemoveEventObjectsCheckBox.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is Event_Object);
            if (check_att.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is w3Attachment);
            if (check_cols.IsChecked == true) model.Nodes.RemoveAll(x => x.Data is Collision_Shape);
            if (RootNodesCheckBox.IsChecked == true) foreach (w3Node node in model.Nodes) { node.parentId = -1; }


            DialogResult = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
