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
using W3_Texture_Finder;
using War3Net.IO.Mpq;

namespace Whim_GEometry_Editor
{
    /// <summary>
    /// Interaction logic for Material_Manager.xaml
    /// </summary>
    public partial class Material_Manager : Window
    {
        w3Model Model;
        bool Initializing = true;
        bool Filling = false;
        public Material_Manager(w3Model model)
        {
            InitializeComponent();
            Model = model;
            FillMAterials();
            CreateWhiteTextureIfMissing();
            FillTextures();
            Initializing = false;
        }
        private w3Material GetSelectedMaterial()
        {
            if (MaterialsListBox.SelectedItem == null) { return new w3Material(); }
            int id = int.Parse((MaterialsListBox.SelectedItem as ListBoxItem).Content.ToString());
            return Model.Materials.First(x => x.ID == id);
        }
        private void FillMAterials()
        {
            foreach (w3Material mat in Model.Materials)
            {
                MaterialsListBox.Items.Add(new ListBoxItem() { Content = mat.ID.ToString() });
            }
        }
        private void CreateWhiteTextureIfMissing()
        {
            if (Model.Textures.Count == 0)
            {
                w3Texture tex = new w3Texture();
                tex.ID = IDCounter.Next();
                tex.Path = "Textures\\white.blp";
                Model.Textures.Add(tex);
            }
        }
        private void DelMaterial(object sender, RoutedEventArgs e)
        {

            if (MaterialsListBox.SelectedItem == null) { return; }
            if (Model.Materials.Count == 1 && Model.Geosets.Count > 1)
            {
                MessageBox.Show("Cannot delete the last material because there are still geosets", "Invalid requst"); return;
            }
            w3Material mat = GetSelectedMaterial();
            if (MaterialUsed(mat.ID))
            {
                MessageBoxResult result = MessageBox.Show("This material is being used. Are you sure you want to delete it? if yes, the first available material will be replaced.", "Material used", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ReplaceUsedMaterial(mat.ID);
                    Model.Materials.Remove(mat);
                    MaterialsListBox.Items.Remove(MaterialsListBox.SelectedItem);
                    LayersListBox.Items.Clear();

                }
            }
            else
            {
                Model.Materials.Remove(mat);
                MaterialsListBox.Items.Remove(MaterialsListBox.SelectedItem);
                LayersListBox.Items.Clear();
            }


        }
        private void ReplaceUsedMaterial(int id)
        {
            foreach (w3Geoset geo in Model.Geosets)
            {
                if (geo.Material_ID == id) { geo.Material_ID = Model.Materials[0].ID; }
            }
        }
        private bool MaterialUsed(int id)
        {
            foreach (w3Geoset geo in Model.Geosets)
            {
                if (geo.Material_ID == id) { return true; }
            }
            return false;
        }
        private void DelLayer(object sender, RoutedEventArgs e)
        {
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                if (LayersListBox.Items.Count == 1)
                {
                    MessageBox.Show("Cannot leave a material without layers!", "Invalid requst"); return;
                }
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                mat.Layers.Remove(lr);
                LayersListBox.Items.Remove(LayersListBox.SelectedItem);

            }
        }

        private void NewMAterial(object sender, RoutedEventArgs e)
        {
            
            w3Material mat = new w3Material(IDCounter.Next());
            w3Layer l = new w3Layer(IDCounter.Next());
            l.Two_Sided = true;
            mat.Layers.Add(l);
            if (Model.Textures.Count > 0)
            {
                l.Diffuse_Texure_ID.StaticValue = [Model.Textures[0].ID];
            }
            else { l.Diffuse_Texure_ID.StaticValue = [-1]; }
            Model.Materials.Add(mat);
            MaterialsListBox.Items.Add(new ListBoxItem() { Content = mat.ID });
        }

        private void ChangedLayerFilter(object sender, SelectionChangedEventArgs e)
        {
            if (Initializing || Filling) {   return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                int selected = FilterModeListBox.SelectedIndex;
                switch (selected)
                {
                    case 0: lr.Filter_Mode = FilterMode.None; break;
                    case 1: lr.Filter_Mode = FilterMode.Transparent; break;
                    case 2: lr.Filter_Mode = FilterMode.Transparent; break;
                    case 3: lr.Filter_Mode = FilterMode.Blend; break;
                    case 4: lr.Filter_Mode = FilterMode.Additive; break;
                    case 5: lr.Filter_Mode = FilterMode.AddAlpha; break;
                    case 6: lr.Filter_Mode = FilterMode.Modulate; break;
                }
            }


        }

        string GetSelectedItemString(ListBox list)
        {
            if (list.SelectedItem == null) { return ""; }
            return (list.SelectedItem as ListBoxItem).Content.ToString();
        }

        private void ChangesAnimatedTexture(object sender, SelectionChangedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                string selected = GetSelectedItemString(AnimatedTextureListBox);
                if (selected == "None")
                {
                    lr.Animated_Texture_ID = -1;
                }
                else
                {
                    int id = int.Parse(selected);
                    lr.Animated_Texture_ID = id;
                }
            }
        }

        private void ChangedUsedTexture(object sender, SelectionChangedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null
                && LayersListBox.SelectedItem != null
                && UsedTextureListBox.SelectedItem != null

                )
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                string selected = GetSelectedItemString(UsedTextureListBox);

                int id = 0;
                if (selected == "Team Color")
                {
                    id = Model.Textures.First(x => x.Replaceable_ID == 1).ID;
                    Preview.Source = MPQHelper.GetImageSource("ReplaceableTextures\\TeamColor\\TeamColor00.blp");
                }
                else if (selected == "Team Glow")
                {
                    id = Model.Textures.First(x => x.Replaceable_ID == 2).ID;
                    Preview.Source = MPQHelper.GetImageSource("ReplaceableTextures\\TeamGlow\\TeamGlow00.blp");
                }
                else
                {
                    id = Model.Textures.First(x => x.Path == selected).ID;
                    Preview.Source = MPQHelper.GetImageSource(selected);
                }

                lr.Diffuse_Texure_ID.StaticValue = [id];
                
            }
        }

        private void CheckedConstantColor(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                mat.Constant_Color = Check_CC.IsChecked == true;
            }
        }

        private void Checked_Sort(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) { return; }
            if (MaterialsListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                mat.Sort_Primitives_Far_Z = Check_Sort.IsChecked == true;
            }
        }

        private void Checked_Full(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                mat.Full_Resolution = Check_Full.IsChecked == true;
            }
        }

        private void SetPriorityPlane(object sender, TextChangedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            string text = PriorityPlaneInput.Text.Trim();
            w3Material mat = GetSelectedMaterial();
            if (text.Length == 0)
            {
                mat.Priority_Plane = 0;

            }
            else
            {
                
                bool parsed = int.TryParse(text, out int result);
                mat.Priority_Plane = result;

            }
        }


        private w3Layer GetSelectedLayer(w3Material mat)
        {
            int lid = int.Parse((LayersListBox.SelectedItem as ListBoxItem).Content.ToString());
            return mat.Layers.First(x => x.ID == lid);
        }

        private void UnshadedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.Unshaded = UnshadedCheckBox.IsChecked == true;
            }
        }

        private void UnfoggedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.Unfogged = UnfoggedCheckBox.IsChecked == true;
            }
        }

        private void TwoSidedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.Two_Sided = TwoSidedCheckBox.IsChecked == true;
            }
        }

        private void SphereEnvMapCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.Sphere_Environment_Map = SphereEnvMapCheckBox.IsChecked == true;
            }
        }

        private void NoDepthTestCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.No_Depth_Test = NoDepthTestCheckBox.IsChecked == true;
            }
        }

        private void NoDepthSetCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.No_Depth_Set = NoDepthSetCheckBox.IsChecked == true;
            }
        }

        private void SelectedMaterial(object sender, SelectionChangedEventArgs e)
        {
            Filling = true;
            if (MaterialsListBox.SelectedItem != null)
            {
                Filling = true;
                w3Material mat = GetSelectedMaterial();
                LayersListBox.Items.Clear();

                foreach (w3Layer l in mat.Layers)
                {
                    LayersListBox.Items.Add(new ListBoxItem() { Content = l.ID.ToString() });
                }
                LayersListBox.SelectedItem = LayersListBox.Items[0];

                Check_Full.IsChecked = mat.Full_Resolution;
                Check_CC.IsChecked = mat.Constant_Color;
                Check_Sort.IsChecked = mat.Sort_Primitives_Far_Z;
                 
                PriorityPlaneInput.Text = mat.Priority_Plane.ToString();
            }
            Filling = false;
        }

        private void SelectedLayer(object sender, SelectionChangedEventArgs e)
        {
            Filling = true;
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            {
               
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                
                UnshadedCheckBox.IsChecked = lr.Unshaded;
                UnfoggedCheckBox.IsChecked = lr.Unfogged;
                TwoSidedCheckBox.IsChecked = lr.Two_Sided;
                SphereEnvMapCheckBox.IsChecked = lr.Sphere_Environment_Map;
                NoDepthSetCheckBox.IsChecked = lr.No_Depth_Set;
                NoDepthTestCheckBox.IsChecked = lr.No_Depth_Test;
                switch (lr.Filter_Mode)
                {
                    case FilterMode.None: FilterModeListBox.SelectedIndex = 0; break;
                    case FilterMode.Transparent: FilterModeListBox.SelectedIndex = 1; break;
                    case FilterMode.Blend: FilterModeListBox.SelectedIndex = 2; break;
                    case FilterMode.Additive: FilterModeListBox.SelectedIndex = 3; break;
                    case FilterMode.AddAlpha: FilterModeListBox.SelectedIndex = 4; break;
                    case FilterMode.Modulate: FilterModeListBox.SelectedIndex = 5; break;
                }
                //animated tx
                if (lr.Animated_Texture_ID < 0) { AnimatedTextureListBox.SelectedIndex = 0; }

                else
                {
                    string id = lr.Animated_Texture_ID.ToString();
                    SelectListBoxItem(id, AnimatedTextureListBox);
                }
                // used texture
                Check_StaticTexture.IsChecked = lr.Diffuse_Texure_ID.isStatic;
                Button_TextureAnimate.IsEnabled = lr.Diffuse_Texure_ID.isStatic == false;
                if (lr.Diffuse_Texure_ID.isStatic)
                {
                    UsedTextureListBox.IsEnabled = true;
                    int texture_id = (int)lr.Diffuse_Texure_ID.StaticValue[0];
                    if (!Model.Textures.Any(x=>x.ID == texture_id)) { throw new Exception($"Invalid texture id {texture_id} when selecting layer"); }
                    string txPath = Model.Textures.First(x => x.ID == texture_id).Path;
                    SelectListBoxItem(txPath, UsedTextureListBox);
                }
                else
                {
                    UsedTextureListBox.IsEnabled = false;
                }

                //alpha
                AlphaStaticInput.IsEnabled = lr.Alpha.isStatic;
                ButtonAnimateAlpha.IsEnabled = lr.Alpha.isStatic == false;
                Check_Alpha.IsChecked = lr.Alpha.isStatic;
                if (lr.Alpha.isStatic) { AlphaStaticInput.Text = lr.Alpha.StaticValue[0].ToString(); }

                //  UsedTextureListBox
            }
            Filling = false;
        }
        private void SelectListBoxItem(string name, ListBox list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                string itemx = (list.Items[i] as ListBoxItem).Content.ToString();
                if (name == itemx)
                {
                    list.SelectedIndex = i; break;
                }
            }

        }
        private void FillTextures()
        {
            foreach (w3Texture tx in Model.Textures)
            {
                if (tx.Replaceable_ID == 1)
                {
                    UsedTextureListBox.Items.Add(new ListBoxItem() { Content = "Team Color" });
                }
                else if (tx.Replaceable_ID == 2)
                {
                    UsedTextureListBox.Items.Add(new ListBoxItem() { Content = "Team Glow" });
                }
                else if (tx.Replaceable_ID == 0)
                {
                    UsedTextureListBox.Items.Add(new ListBoxItem() { Content = tx.Path });
                }

            }
        }

        private void NewLayer(object sender, RoutedEventArgs e)
        {
            if (MaterialsListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer l = new w3Layer(IDCounter.Next());
                l.Diffuse_Texure_ID.isStatic = true;
                if (Model.Textures.Count > 0)
                {
                    l.Diffuse_Texure_ID.StaticValue = [Model.Textures[0].ID];
                }
                else { l.Diffuse_Texure_ID.StaticValue = [-1]; }
               
                mat.Layers.Add(l);
                LayersListBox.Items.Add(new ListBoxItem() { Content = l.ID.ToString() });
                
            }
        }

        private void ChecekdLayersAlpha(object sender, RoutedEventArgs e)
        {
            if (Initializing || Filling) {  return; }
            if (MaterialsListBox.SelectedItem != null && LayersListBox.SelectedItem != null)
            { 
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                lr.Alpha.isStatic = Check_Alpha.IsChecked == true;
                ButtonAnimateAlpha.IsEnabled = Check_Alpha.IsChecked == false;
                AlphaStaticInput.IsEnabled = Check_Alpha.IsChecked == true;
            }
        }

        private void EditAlphaDynamic(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                Transformation_window tw = new Transformation_window(Model, lr.Alpha, TransformationType.Alpha);
                tw.ShowDialog();
            }
           
        }

        private void EditTextureDynamic(object sender, RoutedEventArgs e)
        {
            if (LayersListBox.SelectedItem != null)
            {
                w3Material mat = GetSelectedMaterial();
                w3Layer lr = GetSelectedLayer(mat);
                Transformation_window tw = new Transformation_window(Model, lr.Diffuse_Texure_ID, TransformationType.Color);
                tw.ShowDialog();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
 
