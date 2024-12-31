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
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for Create_Shape.xaml
    /// </summary>
    public partial class Create_Shape : Window
    {
        w3Model Model;
        public Create_Shape(int type, w3Model model)
        {
            InitializeComponent();
            Model = model;
            switch (type)
            {
                case 1: Check_Cube.IsChecked = true; break;
                case 2: Check_Sphere.IsChecked = true; break;
                case 3: Check_Cone.IsChecked = true; break;
                case 4: Check_Cyl.IsChecked = true; break;
            }
            FillMaterials();
            FillGeosets();
            FillBondes();
        }
        private void FillMaterials()
        {
            foreach (w3Material mat in Model.Materials)
            {
                lsit_material.Items.Add(new ComboBoxItem() { Content = mat.ID });
            }
            if (lsit_material.Items.Count > 0)
            {
                lsit_material.SelectedIndex = 0;
            }
        }
        private void FillGeosets()
        {
            if (Model.Geosets.Count == 0)
            {
                Check_Geoset.IsEnabled = false;
                list_Geosets.IsEnabled = false;
            }
            else
            {
                foreach (w3Geoset geo in Model.Geosets)
                {
                    list_Geosets.Items.Add(new ComboBoxItem() { Content = geo.ID });
                }
            }
            if (list_Geosets.Items.Count > 0)
            {
                list_Geosets.SelectedIndex = 0;
            }
        }
        private void FillBondes()
        {
            if (Model.Nodes.Any(x => x.Data is Bone) == false) { Check_Bone.IsEnabled = false; list_bones.IsEnabled = false; return; }
            foreach (w3Node node in Model.Nodes)
            {
                if (node.Data is Bone)
                {
                    list_bones.Items.Add(new ComboBoxItem() { Content = node.Name });
                }
                list_bones.SelectedIndex = 0;
            }
        }
        private void checkedCube(object sender, RoutedEventArgs e)
        {
            InputCuts.IsEnabled = true;
            InputSections.IsEnabled = false;
            InputSlices.IsEnabled = false;
        }

        private void checkedSphere(object sender, RoutedEventArgs e)
        {
            InputCuts.IsEnabled = false;
            InputSections.IsEnabled = true;
            InputSlices.IsEnabled = true;
        }

        private void checkedCone(object sender, RoutedEventArgs e)
        {
            InputCuts.IsEnabled = false;
            InputSections.IsEnabled = true;
            InputSlices.IsEnabled = false;
        }

        private void Checekdcyl(object sender, RoutedEventArgs e)
        {
            InputCuts.IsEnabled = false;
            InputSections.IsEnabled = true;
            InputSlices.IsEnabled = false;
        }
        private int GetSelectedBone()
        {
            if (Check_Bone.IsChecked == true)
            {
                // attach to bone
                string name = (list_bones.SelectedItem as ComboBoxItem).Content.ToString();
                int selectedBone = Model.Nodes.First(x => x.Name == name).objectId;
                return selectedBone;
            }
            else
            {
                w3Node node = new w3Node();
                node.objectId = IDCounter.Next();
                node.Data = new Bone();
                node.Name = $"GeneratedShapesBone_{node.objectId}";
                Model.Nodes.Add(node);
                return node.objectId;
            }
        }
        private void Create(object sender, RoutedEventArgs e)
        {
            int materialID = int.Parse((lsit_material.SelectedItem as ComboBoxItem).Content.ToString());



            w3Geoset CreatedShape = null;
            w3Geoset IncludeInWhichGeoset = null;
            if (Check_Geoset.IsChecked == true)
            {
                int selected = int.Parse((list_Geosets.SelectedItem as ComboBoxItem).Content.ToString());
                IncludeInWhichGeoset = Model.Geosets.First(x => x.ID == selected);
            }
            if (Check_Geoset.IsChecked == true)
            {

            }

            if (Check_Cube.IsChecked == true)
            {
                bool parse = int.TryParse(InputCuts.Text, out int cuts);


                if (parse == false) { MessageBox.Show("input is invalid", "Precaution"); return; }
                else
                {
                    if (cuts > 20) { MessageBox.Show("Cuts cannot be greater than 20", "Precaution"); return; }
                    CreatedShape = ShapeGenerator.GenerateCube(cuts);


                }
            }
            if (Check_Cone.IsChecked == true)
            {
                bool parse = int.TryParse(InputSections.Text, out int sections);
                if (parse == false) { MessageBox.Show("input is invalid", "Precaution"); return; }
                else
                {
                    if (sections < 3 || sections > 50) { MessageBox.Show("Sections must not be less than 3 and greater than 50", "Precaution"); return; }
                    CreatedShape = ShapeGenerator.GenerateCone(sections);
                }
            }

            if (Check_Cyl.IsChecked == true)
            {
                bool parse  = int.TryParse(InputSections.Text, out int sections);
                
                if (parse  == false  ) { MessageBox.Show("input is invalid"); return; }
                else
                {
                    if (sections < 3 || sections > 50) { MessageBox.Show("Sections must not be less than 3 and greater than 50", "Precaution"); return; }
                    CreatedShape = ShapeGenerator.GenerateCylinder(sections);
                }
            }
            if (Check_Sphere.IsChecked == true)
            {
                bool parse1 = int.TryParse(InputSections.Text, out int sections);
                bool parse2 = int.TryParse(InputSlices.Text, out int slices);
                if (parse1 == false || parse2 == false) { MessageBox.Show("input is invalid", "Precaution"); return; }
                else
                {
                    if (sections < 3 || sections > 50) { MessageBox.Show("Sections must not be less than 3 and greater than 50", "Precaution"); return; }
                    if (sections < 3 || sections > 50) { MessageBox.Show("Slices must not be less than 3 and greater than 50", "Precaution"); return; }
                }
                CreatedShape = ShapeGenerator.GenerateSphere(sections, slices);
            }
            if (CreatedShape != null)
            {
                //----------------------------------
                // calculate edges
                CreatedShape.RecalculateEdges();
                // give bone attachment
                int boneID = GetSelectedBone();
                foreach (w3Vertex v in CreatedShape.Vertices) { v.Id = IDCounter.Next(); v.AttachedTo.Add(boneID); }

                //----------------------------------
                CreatedShape.ID = IDCounter.Next();
                if (IncludeInWhichGeoset == null)
                {
                    // geoset anim
                    w3Geoset_Animation ga = new w3Geoset_Animation();
                    ga.Alpha.isStatic = true;
                    ga.Alpha.StaticValue = [1];
                    ga.Geoset_ID = CreatedShape.ID;
                    ga.ID = IDCounter.Next();
                    CreatedShape.Material_ID = materialID;
                    Model.Geoset_Animations.Add(ga);
                    Calculator3D.CenterVertices(CreatedShape.Vertices,0,0,0);
                    //----------------------------------
                    
                    CreatedShape.Extent = Calculator3D.CalculateCollectiveExtentFromCoordinates(CreatedShape.Vertices.Select(x => x.Position).ToList());
                    //----------------------------------
                    Model.Geosets.Add(CreatedShape);
                }
                else
                {
                    // include in geoset
                    Calculator3D.CenterVertices(CreatedShape.Vertices, 0, 0, 0);
                    IncludeInWhichGeoset.Vertices.AddRange(CreatedShape.Vertices);
                    IncludeInWhichGeoset.Triangles.AddRange(CreatedShape.Triangles);
                    IncludeInWhichGeoset.Extent = Calculator3D.CalculateCollectiveExtentFromCoordinates(IncludeInWhichGeoset.Vertices.Select(x => x.Position).ToList());

                }
               
            }
            DialogResult = true;
        }

        private void Checked_IncludeInGeoset(object sender, RoutedEventArgs e)
        {
             
            lsit_material.IsEnabled = Check_Geoset.IsChecked == false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
