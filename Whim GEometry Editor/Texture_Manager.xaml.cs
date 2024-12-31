using MDLLib;
using MDLLibs.Classes.Misc;
using Microsoft.Win32;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using Whim_GEometry_Editor.Dialogs;

namespace Whim_Model_Editor
{
    /// <summary>
    /// Interaction logic for Texture_Manager.xaml
    /// </summary>
    public partial class Texture_Manager : Window
    {

        w3Model Model;

        public Texture_Manager(w3Model model)
        {
            InitializeComponent();
            Model = model;


            FillTexturesInModel();
            FillBrowserList();
        }
        private void FillTexturesInModel()
        {
            foreach (w3Texture t in Model.Textures)
            {
                switch (t.Replaceable_ID)
                {
                    case 0: List_Textures.Items.Add(new ListBoxItem() { Content = $"{t.ID}: " + t.Path }); break;
                    case 1: List_Textures.Items.Add(new ListBoxItem() { Content = $"{t.ID}: " + "Team Color" }); break;
                    case 2: List_Textures.Items.Add(new ListBoxItem() { Content = $"{t.ID}: " + "Team Glow" }); break;
                    default: break;
                }
            }
        }
        private void FillBrowserList()
        {
            foreach (string item in MPQHelper.Listfile_All)
            {
                Browser.Items.Add(new ListBoxItem() { Content = item });
            }
        }
        private void DelTexture(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {

                if (List_Textures.Items.Count == 1)
                {
                    MessageBox.Show("Not allowed to delete the only remaining texture"); return;
                }
                string selected = (List_Textures.SelectedItem as ListBoxItem).Content.ToString();
                int id = int.Parse(selected.Split(":")[0]);
                w3Texture t = Model.Textures.First(x => x.ID == id);

                if (TextureIsUsed(t.ID))
                {
                    MessageBoxResult result = MessageBox.Show("This texture is being used. Are you sure you want to delete it? if yes, the first available texture will be replaced on them.", "Texture used", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {

                        GivenewTextureToLayers(t.ID);
                        Model.Textures.Remove(t);
                        List_Textures.Items.Remove(List_Textures.SelectedItem);
                    }
                }


                else
                {
                    Model.Textures.Remove(t);
                    List_Textures.Items.Remove(List_Textures.SelectedItem);
                }


            }

        }
        private void GivenewTextureToLayers(int txid)
        {
            int newID = Model.Textures[0].ID;
            foreach (w3Material w3Material in Model.Materials)
            {
                foreach (w3Layer w3Layer in w3Material.Layers)
                {
                    if (w3Layer.Diffuse_Texure_ID.isStatic)
                    {
                        if (w3Layer.Diffuse_Texure_ID.StaticValue[0] == txid)
                        {
                            w3Layer.Diffuse_Texure_ID.StaticValue[0] = newID;
                        }
                    }
                    else
                    {
                        foreach (w3Keyframe k in w3Layer.Diffuse_Texure_ID.Keyframes)
                        {

                            if (k.Data[0] == txid) { k.Data[0] = newID; }
                        }
                    }
                }
            }
            foreach (w3Node w3Node in Model.Nodes)
            {
                if (w3Node.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 pe = (Particle_Emitter_2)w3Node.Data;
                    if (pe.Texture_ID == txid) { pe.Texture_ID = newID; }
                }
            }
        }
        private bool TextureIsUsed(int id)
        {
            foreach (w3Material w3Material in Model.Materials)
            {
                foreach (w3Layer w3Layer in w3Material.Layers)
                {
                    if (w3Layer.Diffuse_Texure_ID.isStatic)
                    {
                        if (w3Layer.Diffuse_Texure_ID.StaticValue[0] == id) { return true; }

                    }
                    else
                    {
                        foreach (w3Keyframe k in w3Layer.Diffuse_Texure_ID.Keyframes)
                        {
                            if (k.Data[0] == id) { return true; }
                        }
                    }
                }
            }
            foreach (w3Node w3Node in Model.Nodes)
            {
                if (w3Node.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 pe = (Particle_Emitter_2)w3Node.Data;
                    if (pe.Texture_ID == id) { return true; }
                }
            }
            return false;
        }

        private void Browse(object sender, RoutedEventArgs e)
        {

        }

        private void Search(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return; }
            string query = Searchbox.Text.Trim().ToLower();
            if (query.Length == 0)
            {
                Browser.Items.Clear();
                FillBrowserList();
            }
            else
            {
                Browser.Items.Clear();
                foreach (string item in MPQHelper.Listfile_All)
                {
                    if (item.ToLower().Contains(query))
                    {
                        Browser.Items.Add(new ListBoxItem() { Content = item });
                    }
                }
            }
        }

        private void finish(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItems.Count == 1) { DialogResult = true; }
        }

        private void AddTexture(object sender, RoutedEventArgs e)
        {
            if (Browser.SelectedItems.Count == 1)
            {
                string browsed = (Browser.SelectedItem as ListBoxItem).Content.ToString();
                if (Model.Textures.Any(x => x.Path == browsed) == false)
                {
                    w3Texture newtx = new w3Texture(IDCounter.Next(), browsed, false, false, 0);
                    Model.Textures.Add(newtx);
                    List_Textures.Items.Add(new ListBoxItem() { Content = $"{newtx.ID}: " + browsed });
                }
                else { MessageBox.Show("There is a texture with that path already"); }
            }
        }

        private void ShowBrowsed(object sender, SelectionChangedEventArgs e)
        {
            if (Browser.SelectedItem == null) { return; }
            string name = (Browser.SelectedItem as ListBoxItem).Content.ToString();
            Display.Source = MPQHelper.GetImageSource(name);
        }

        private void ShowTexture(object sender, SelectionChangedEventArgs e)
        {
            if (List_Textures.SelectedItem == null) { return; }
            w3Texture tex = GetSelectedTexture(List_Textures);
            if (tex.Replaceable_ID == 0)
            {
                Display.Source = MPQHelper.GetImageSource(tex.Path);
            }
            if (tex.Replaceable_ID == 1)
            {
                Display.Source = MPQHelper.GetImageSource("ReplaceableTextures\\TeamColor\\TeamColor00.blp");
            }
            if (tex.Replaceable_ID == 2)
            {
                Display.Source = MPQHelper.GetImageSource("ReplaceableTextures\\TeamGlow\\TeamGlow00.blp");
            }
            if (tex.Replaceable_ID > 2)
            {
                MessageBox.Show("Replaceable ID greater than 2 is not supported");
            }
        }

        private void CreateTeamColor(object sender, RoutedEventArgs e)
        {
            if (Model.Textures.Any(x => x.Replaceable_ID == 1)) { return; }
            w3Texture t = new w3Texture();
            t.ID = IDCounter.Next();
            t.Replaceable_ID = 1;
            Model.Textures.Add(t);
            List_Textures.Items.Add(new ListBoxItem() { Content = $"{t.ID}: " + "Team Color" });
        }

        private void CreateTeamGlow(object sender, RoutedEventArgs e)
        {
            if (Model.Textures.Any(x => x.Replaceable_ID == 2)) { return; }
            w3Texture t = new w3Texture();
            t.ID = IDCounter.Next();
            t.Replaceable_ID = 2;
            Model.Textures.Add(t);
            List_Textures.Items.Add(new ListBoxItem() { Content = $"{t.ID}: " + "Team Glow" });
        }

        private void EditTextureProperties(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TextureProperties tp = new TextureProperties(GetSelectedTexture(List_Textures));
                tp.ShowDialog();
            }
        }
        private w3Texture GetSelectedTexture(ListBox whichList)
        {
            string selected = (whichList.SelectedItem as ListBoxItem).Content.ToString();
            int id = int.Parse(selected.Split(":")[0]);
            return Model.Textures.First(X => X.ID == id);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }

        private void ExportTexture(object sender, RoutedEventArgs e)
        {
            if (Browser.SelectedItems.Count == 1)
            {
                string browsed = (Browser.SelectedItem as ListBoxItem).Content.ToString();
                // Create an instance of SaveFileDialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Files (*.png)|*.png|BLP Files (*.blp)|*.blp|All Files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Get the selected file path
                    string filePath = saveFileDialog.FileName;
                    if (System.IO.Path.GetExtension(filePath).ToLower() == ".blp")
                    {
                        MPQHelper.Export(browsed, filePath);
                    }
                    else
                    {
                        Bitmap bmp = MPQHelper.GetImage(filePath);
                        bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    }

                }

            }
        }

        private void CopyPath(object sender, RoutedEventArgs e)
        {
             if (Browser.SelectedItem != null)
            {
                Clipboard.SetText((Browser.SelectedItem as ListBoxItem).Content.ToString());
            }
        }

        private void AddTextureMaterial(object sender, RoutedEventArgs e)
        {
            if (Browser.SelectedItems.Count == 1)
            {
                string browsed = (Browser.SelectedItem as ListBoxItem).Content.ToString();
                if (Model.Textures.Any(x => x.Path == browsed) == false)
                {
                    w3Texture newtx = new w3Texture(IDCounter.Next(), browsed, false, false, 0);
                    w3Layer l = new w3Layer();
                    l.Diffuse_Texure_ID.StaticValue = [newtx.ID];
                    l.ID = IDCounter.Next();
                    w3Material mat = new w3Material();
                    mat.ID = IDCounter.Next();
                    mat.Layers.Add(l);

                    Model.Textures.Add(newtx);
                    Model.Materials.Add(mat);
                    List_Textures.Items.Add(new ListBoxItem() { Content = $"{newtx.ID}: " + browsed });
                }
                else { MessageBox.Show("There is a texture with that path already"); }
            }
        }
    }
}