using MDLLib;
using MdxLib.Model;
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

namespace Whim_GEometry_Editor
{
    /// <summary>
    /// Interaction logic for Texture_Animation.xaml
    /// </summary>
    public partial class Texture_Animation_Editor : Window
    {
        w3Model Model;
        w3Texture_Animation CurrentTA;
        public Texture_Animation_Editor(w3Model model)
        {
            InitializeComponent();
             Model = model;
            FillTextureAnimations();
        }
        private void FillTextureAnimations()
        {
            foreach (w3Texture_Animation ta in Model.Texture_Animations)
            {
                List_GAs.Items.Add(new ListBoxItem() { Content = ta.ID.ToString()});
            }
        }

        private void NewTA(object sender, RoutedEventArgs e)
        {
            w3Texture_Animation ta = new w3Texture_Animation();
            ta.ID = IDCounter.Next();
            List_GAs.Items.Add(new ListBoxItem() { Content = ta.ID.ToString()});
            Model.Texture_Animations.Add(ta);
        }

        private void DelTA(object sender, RoutedEventArgs e)
        {
            if (List_GAs.SelectedItem != null)
            {
                int id = int.Parse((List_GAs.SelectedItem as ListBoxItem).Content.ToString());
                if (TAIsUsed(id))
                {
                    MessageBoxResult result = MessageBox.Show("This texture animation is used. Still delete?", "Texture animation used", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        NullDeletesTAs(id);
                        Model.Texture_Animations.RemoveAll(x => x.ID == id);
                        List_GAs.Items.Remove(List_GAs.SelectedItem);
                    }
                }
                else
                {
                    Model.Texture_Animations.RemoveAll(x => x.ID == id);
                    List_GAs.Items.Remove(List_GAs.SelectedItem);
                }
            }
        }
        private bool TAIsUsed(int id)
        {
            foreach (w3Material mat in Model.Materials)
            {
                foreach (w3Layer l in mat.Layers)
                {
                    if (l.Animated_Texture_ID == id) { return true; }
                }
                
            }
            return false;
        }
        private void NullDeletesTAs(int id)
        {
            foreach (w3Material mat in Model.Materials)
            {
                foreach (w3Layer l in mat.Layers)
                {
                    if (l.Animated_Texture_ID == id) { l.Animated_Texture_ID = -1; }
                }

            }
        }

        private void EditRotation(object sender, RoutedEventArgs e)
        {
            if (List_GAs.SelectedItem == null) { return; }
            
            Transformation_window tw = new Transformation_window(Model, CurrentTA.Rotation, TransformationType.Rotation);
            tw.ShowDialog();
        }
   

        private void EditSclaing(object sender, RoutedEventArgs e)
        {
            if (List_GAs.SelectedItem == null) { return; }
           
            Transformation_window tw = new Transformation_window(Model, CurrentTA.Scaling, TransformationType.Scaling);
            tw.ShowDialog();
        }
    

        private void EditTranslation(object sender, RoutedEventArgs e)
        {
            if (List_GAs.SelectedItem == null) { return; }
            
            Transformation_window tw = new Transformation_window(Model, CurrentTA.Translation, TransformationType.Translation);
            tw.ShowDialog();
        }
        private w3Texture_Animation GetSelectedTextureAnimation()
        {
            
            int id = int.Parse((List_GAs.SelectedItem as ListBoxItem).Content.ToString());
            return Model.Texture_Animations.First(x=>x.ID == id);
        }
        private void SelectedTA(object sender, SelectionChangedEventArgs e)
        {
            CurrentTA =   GetSelectedTextureAnimation();
            Check_TranslationAnimated.IsEnabled = true;
            Check_TranslationAnimated.IsChecked = CurrentTA.Translation.isStatic == false;
            Check_RotationAnimated.IsEnabled = true;
            Check_RotationAnimated.IsChecked = CurrentTA.Rotation.isStatic == false;
            Check_ScalingAnimated.IsEnabled = true;
            Check_ScalingAnimated.IsChecked= CurrentTA.Scaling.isStatic == false;
        }

        private void SEtTA_TR_Animated(object sender, RoutedEventArgs e)
        {
            ButtonEdit_TR.IsEnabled = Check_TranslationAnimated.IsChecked == true ;
            CurrentTA.Translation.isStatic = Check_TranslationAnimated.IsChecked == false ;
        }

        private void SEtTA_ROT_Animated(object sender, RoutedEventArgs e)
        {
            ButtonEdit_RO.IsEnabled = Check_RotationAnimated.IsChecked == true;
            CurrentTA.Rotation.isStatic = Check_RotationAnimated.IsChecked == false;
        }

        private void SEtTA_SC_Animated(object sender, RoutedEventArgs e)
        {
            ButtonEdit_SC.IsEnabled = Check_ScalingAnimated.IsChecked == true;
            CurrentTA.Scaling.isStatic = Check_ScalingAnimated.IsChecked == false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
