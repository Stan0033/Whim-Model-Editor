using MDLLib;
using MDLLibs.Classes.Misc;
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
    /// Interaction logic for import_geoset.xaml
    /// </summary>
    public partial class import_geoset : Window
    {
        public int MaterialId;
        public string NodeName;
        List<w3Geoset> geosets;
        w3Model Model;
        public import_geoset(string importing, w3Model model, List<w3Geoset> whichGeosets)
        {
            InitializeComponent();
             
            ImportName.Text = importing;
            geosets = whichGeosets;
            Model = model;
            foreach (w3Material mat in model.Materials)
            {
                List_Materials.Items.Add(new ListBoxItem() { Content = mat.ID });
            }
            List_Materials.SelectedIndex = 0;
            int BoneCount = model.Nodes.Count(x => x.Data is Bone);
            
            if (BoneCount  == 0)
            {
                Check_Bone.IsChecked = true;
                Check_Bone.IsEnabled = false;
                List_Bones.IsEnabled = false;
            }
            else
            {
                foreach (w3Node node in model.Nodes)
                {
                    if (node.Data is Bone)
                    {
                        List_Bones.Items.Add(new ListBoxItem() { Content = node.Name });
                    }

                }
                
                List_Bones.SelectedIndex = 0;
            }

            
            if (model.Geosets.Count == 0) {
                Check_Geoset.IsChecked = false; 
                Check_Geoset.IsEnabled = false;
                List_Geosets.IsEnabled = false; 
            }
            else
            {
                foreach (w3Geoset geo in model.Geosets)
                {
                    List_Geosets.Items.Add(new ListBoxItem() { Content = geo.ID });
                }
            }
        }

        private void Complete(object sender, RoutedEventArgs e)
        {
           
            //-------------------------------------------------------
            //--- give ids
            //-------------------------------------------------------
            foreach (w3Geoset geo in geosets)
            {
                foreach (w3Vertex w3 in geo.Vertices)
                {
                    if (w3.Id == -1) { w3.Id = IDCounter.Next(); }
                }
                
            }
            //-------------------------------------------------------
            //--- give material
            //-------------------------------------------------------
            MaterialId = int.Parse((List_Materials.SelectedItem as ListBoxItem).Content.ToString());
            foreach (w3Geoset geo in geosets)
            {
                geo.Material_ID = MaterialId;
            }
                //-------------------------------------------------------
                //--- attach to a bone
                //-------------------------------------------------------
            if (Check_Bone.IsChecked == true)  
            {
               
                // new bone
                w3Node node = new w3Node();
                node.Data = new Bone();
                node.objectId = IDCounter.Next();
                node.parentId = -1;
                node.Name = $"ImportedGeosetBone_{node.objectId}";
                Model.Nodes.Add(node);
                foreach (w3Geoset geo in geosets)
                {
                   foreach (w3Vertex v in  geo.Vertices)
                    {
                        v.AttachedTo.Add(node.objectId);
                    }
                }
            }
            else // include in bone
            {
                NodeName = (List_Bones.SelectedItem as ListBoxItem).Content.ToString();

                w3Node selectedNone = Model.Nodes.First(x=>x.Name == NodeName);
                foreach (w3Geoset geo in geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        v.AttachedTo.Add(selectedNone.objectId);
                    }
                }
            }

            //-------------------------------------------------------
            //--- finalize geoset
            //-------------------------------------------------------
            foreach (w3Geoset geoset1 in geosets)
            {
                geoset1.RecalculateEdges();
                geoset1.Extent = Calculator3D.GetExtentFromVertexList(geoset1.Vertices);
                w3Geoset_Animation ga = new w3Geoset_Animation();
                ga.Geoset_ID = geoset1.ID;
                ga.Alpha.StaticValue = [1];
                ga.Alpha.isStatic = true;
                Model.Geoset_Animations.Add(ga);
                geoset1.RecalculateEdges();
            }
            //-------------------------------------------------------
            //--- part of geoset or new geoset
            //-------------------------------------------------------
            if (Check_Geoset.IsChecked == true)
            {
                // include in selected geoset
                int selectedGeoset = int.Parse((List_Geosets.SelectedItem as ListBoxItem).Content.ToString());
                w3Geoset geoset = Model.Geosets.First(x => x.ID == selectedGeoset);
                foreach (w3Geoset geo in geosets)
                {
                    geoset.Vertices.AddRange(geo.Vertices);
                    geoset.Triangles.AddRange(geo.Triangles);
                }
                geoset.RecalculateEdges();

            }
            else
            {
                foreach (var geoset in geosets)
                {
                    foreach (w3Vertex v in geoset.Vertices) { v.Id = IDCounter.Next(); }
                }
                Model.Geosets.AddRange(geosets);
                // include in model

            }

            DialogResult = true;

          
        }

        private void Checked_Bone(object sender, RoutedEventArgs e)
        {
            List_Bones.IsEnabled = Check_Bone.IsChecked == false;
        }

        private void Checked_Geoset(object sender, RoutedEventArgs e)
        {
            List_Geosets.IsEnabled = Check_Geoset.IsChecked == true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
