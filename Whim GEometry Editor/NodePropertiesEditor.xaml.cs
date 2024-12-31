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
using Whim_GEometry_Editor;

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for Transformations_Editor.xaml
    /// </summary>
    public partial class NodePropertiesEditor : Window
    {
        w3Model w3Model;
        w3Node CurrentNode;
        TextBlock NodeBoxName;
        public NodePropertiesEditor(w3Model Model, w3Node WhichNoe, TextBlock NodeNamePlaceholder)
        {
            InitializeComponent();
            CurrentNode = WhichNoe;
            w3Model = Model;
            NodeBoxName = NodeNamePlaceholder;
            InputName.Text = WhichNoe.Name;
            ButtonTranslation.Content = $"Translation ({WhichNoe.Translation.Keyframes.Count})";
            ButtonRotation.Content = $"Rotation ({WhichNoe.Rotation.Keyframes.Count})";
            ButtonScaling.Content = $"Scaling ({WhichNoe.Scaling.Keyframes.Count})";
            CheckTags();
        }

        private void CheckTags()
        {
            if (CurrentNode != null)
            {
                BillboardedCheckBox.IsChecked = CurrentNode.Billboarded;
                BillboardedLockXCheckBox.IsChecked = CurrentNode.Billboarded_Lock_X;
                BillboardedLockYCheckBox.IsChecked = CurrentNode.Billboarded_Lock_Y;
                BillboardedLockZCheckBox.IsChecked = CurrentNode.Billboarded_Lock_Z;
                CameraAnchoredCheckBox.IsChecked = CurrentNode.Camera_Anchored;
                DontInheritTranslationCheckBox.IsChecked = !CurrentNode.Inherits_Translation;
                DontInheritRotationCheckBox.IsChecked = !CurrentNode.Inherits_Rotation;
                DontInheritScalingCheckBox.IsChecked = !CurrentNode.Inherits_Scaling;
            }
        }

        private void EditTranslation(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(w3Model, CurrentNode.Translation, TransformationType.Translation);
            tw.ShowDialog();
        }

        private void EditRotation(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(w3Model, CurrentNode.Rotation, TransformationType.Rotation);
            tw.ShowDialog();
        }

        private void EditScaling(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(w3Model, CurrentNode.Scaling, TransformationType.Scaling);
            tw.ShowDialog();
        }

        private void BillboardedLockX_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Billboarded_Lock_X = BillboardedLockXCheckBox.IsChecked == true;
        }

        private void Billboarded_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Billboarded = BillboardedCheckBox.IsChecked == true;
        }

        private void BillboardedLockY_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Billboarded_Lock_Y = BillboardedLockYCheckBox.IsChecked == true;
        }

        private void BillboardedLockZ_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Billboarded_Lock_Z = BillboardedLockZCheckBox.IsChecked == true;
        }

        private void DontInheritTranslation_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Inherits_Translation = DontInheritTranslationCheckBox.IsChecked == false;
        }

        private void DontInheritRotation_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Inherits_Rotation = DontInheritRotationCheckBox.IsChecked == false;
        }

        private void DontInheritScaling_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Inherits_Scaling = DontInheritScalingCheckBox.IsChecked == false;
        }

        private void CameraAnchored_Checked(object sender, RoutedEventArgs e)
        {
            CurrentNode.Camera_Anchored = CameraAnchoredCheckBox.IsChecked == false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }

        private void SetName(object sender, RoutedEventArgs e)
        {
            string name = InputName.Text.Trim();
            string currentName = CurrentNode.Name;
            if (currentName == name)
            {
                MessageBox.Show("Thew new name is the same as the current", "Invalid request"); return;
            }
            foreach (w3Node node in w3Model.Nodes)
            {
                if ( node.Name == currentName) { continue; }
                if ( node.Name ==  name) { MessageBox.Show("A node with such name already exists", "Invalid request"); return; }

            }
            CurrentNode.Name = name;
            NodeBoxName.Text = name;
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
