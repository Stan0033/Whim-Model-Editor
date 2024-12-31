using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
using static test_parser_mdl.Parser_MDL;
using test_parser_mdl;
using Microsoft.Win32;
using System.IO;

namespace Whim_GEometry_Editor
{
    /// <summary>
    /// Interaction logic for Optimizer.xaml
    /// </summary>
    public partial class Optimizer : Window
    {
        public w3Model Model_;
        public w3Model TemporaryModel_Placeholder;
        bool finalize = true;
        public Optimizer(w3Model model)
        {
            InitializeComponent();
            Model_ = model;

        }

        private void Optimize(object sender, RoutedEventArgs e)
        {
            Model_.RefreshTransformationsList();
            if (Check_Isolated.IsChecked == true) RemoveIsolatedTriangles();
            if (Check_ZeroArea.IsChecked == true) Remove0AreaTriangles();
            if (Check_MergeVerts.IsChecked == true)
            {
                bool parseed = float.TryParse(PrecisionInput.Text, out float precision);
                if ( parseed && precision <= 1 && precision > 0)
                {
                    MergeIdenticalVertices(precision);
                }
                
            }
            if (Check_unusedtx.IsChecked == true) RemoveUnusedTextures();
            if (Check_unusedmats.IsChecked == true) RemoveUnusedMaterials();
            if (Check_unusedgs.IsChecked == true) RemoveUnusedGlobalSequences();
            if (Check_unusedtxanim.IsChecked == true) RemoveUnusedTextureAnims();
            if (Check_sequences.IsChecked == true) RemoveUnAnimatedSequences();
            if (Check_unusedevents.IsChecked == true) RemoveUnusedEventObjects();
            
            if (Check_helpers.IsChecked == true) RemoveFreeHelpers();
            if (Check_bones.IsChecked == true) RemoveUnAttachedBones();
            if (Check_unusedkf.IsChecked == true) RemoveUnusedKeyframes();
            if (Check_liner.IsChecked == true) LinearizeAnimations();
            if (Check_vis.IsChecked == true) SetInterpolationNoneForVisibilities();
            if (Check_invisGAs.IsChecked == true) SetStaticAlphasTo100();
            if (Check_freebones.IsChecked == true) ConvertFreeBonesToHelpers();
            if (Check_RemoveDanglingVertices.IsChecked == true) { RemoveDangingVertices(); }
            if (finalize)
            {
                DialogResult = true;
            }
        }

        private void RemoveDangingVertices()
        {
            foreach (w3Geoset geo in Model_.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices.ToList())
                {
                    bool found = false;
                    foreach (w3Triangle tr in geo.Triangles)
                    {
                        if (tr.Vertex1 == v) { found = true; break; }
                        if (tr.Vertex2 == v) { found = true; break; }
                        if (tr.Vertex3 == v) { found = true; break; }
                    }
                    if (!found) { geo.Vertices.Remove(v); }
                }
            }
        }
        private void SetStaticAlphasTo100()
        {
            foreach (w3Geoset_Animation ga in Model_.Geoset_Animations)
            {
                if (ga.Alpha.isStatic && ga.Alpha.StaticValue[0] < 1) { ga.Alpha.StaticValue[0] = 1; }
            }
        }

        private void RemoveIsolatedTriangles()
        {
            foreach (w3Geoset geoset in Model_.Geosets)
            {
                foreach (w3Triangle t in geoset.Triangles.ToList())
                {
                    if (IsISolated(geoset, t))
                    {
                        geoset.Triangles.Remove(t);
                    }
                }
            }
        }
        private bool IsISolated(w3Geoset geoset, w3Triangle triangle)
        {
            int vertexId1 = triangle.Vertex1.Id;
            int vertexId2 = triangle.Vertex2.Id;
            int vertexId3 = triangle.Vertex3.Id;

            bool sharedVertex1 = false;
            bool sharedVertex2 = false;
            bool sharedVertex3 = false;

            // Iterate through all triangles in the geoset
            foreach (w3Triangle other in geoset.Triangles)
            {
                // Skip the current triangle
                if (other == triangle) continue;

                // Check if the vertices are shared by other triangles
                if (other.Vertex1.Id == vertexId1 || other.Vertex2.Id == vertexId1 || other.Vertex3.Id == vertexId1)
                {
                    sharedVertex1 = true;
                }
                if (other.Vertex1.Id == vertexId2 || other.Vertex2.Id == vertexId2 || other.Vertex3.Id == vertexId2)
                {
                    sharedVertex2 = true;
                }
                if (other.Vertex1.Id == vertexId3 || other.Vertex2.Id == vertexId3 || other.Vertex3.Id == vertexId3)
                {
                    sharedVertex3 = true;
                }

                // If all vertices are shared, the triangle is not isolated
                if (sharedVertex1 && sharedVertex2 && sharedVertex3)
                {
                    return false;
                }
            }

            // If none of the vertices are shared, the triangle is isolated
            return !(sharedVertex1 || sharedVertex2 || sharedVertex3);
        }


        private void Remove0AreaTriangles()
        {
            foreach (w3Geoset geoset in Model_.Geosets)
            {
                foreach (w3Triangle t in geoset.Triangles.ToList())
                {
                    if (Is0Area(t.Vertex1.Position, t.Vertex2.Position, t.Vertex3.Position)) { geoset.Triangles.Remove(t); }
                }
            }
        }
        bool Is0Area(Coordinate one, Coordinate two, Coordinate three)
        {
            // Create two vectors from the triangle's vertices
            Vector3 v1 = new Vector3(two.X - one.X, two.Y - one.Y, two.Z - one.Z);
            Vector3 v2 = new Vector3(three.X - one.X, three.Y - one.Y, three.Z - one.Z);

            // Compute the cross product of the two vectors
            Vector3 crossProduct = Vector3.Cross(v1, v2);

            // If the magnitude of the cross product is close to zero, the triangle has no area
            return crossProduct.Length() < 1e-6;
        }

        private void MergeIdenticalVertices(float precision)
        {
            // Dictionary to hold vertices to be merged
            Dictionary<w3Vertex, List<w3Vertex>> verticesToMerge = new Dictionary<w3Vertex, List<w3Vertex>>();

            foreach (w3Geoset g in Model_.Geosets)
            {
                for (int i = 0; i < g.Vertices.Count; i++)
                {
                    w3Vertex v1 = g.Vertices[i];

                    // Check for similar vertices and group them in the dictionary
                    for (int j = i + 1; j < g.Vertices.Count; j++)
                    {
                        w3Vertex v2 = g.Vertices[j];

                        // Calculate the squared distance between the two vertices
                        float distanceSquared = (v1.Position.X - v2.Position.X) * (v1.Position.X - v2.Position.X) +
                                                (v1.Position.Y - v2.Position.Y) * (v1.Position.Y - v2.Position.Y) +
                                                (v1.Position.Z - v2.Position.Z) * (v1.Position.Z - v2.Position.Z);

                        // If distance is within the precision threshold
                        if (distanceSquared <= precision * precision)
                        {
                            // Add to the dictionary if not already added
                            if (!verticesToMerge.ContainsKey(v1))
                            {
                                verticesToMerge[v1] = new List<w3Vertex>();
                            }
                            verticesToMerge[v1].Add(v2);
                        }
                    }
                }

                // Second pass: Go through the dictionary and merge the vertices
                foreach (var kvp in verticesToMerge)
                {
                    w3Vertex masterVertex = kvp.Key;
                    List<w3Vertex> verticesToBeMerged = kvp.Value;

                    foreach (w3Vertex v in verticesToBeMerged)
                    {
                        MergeTargetVertices(g, masterVertex, v);
                    }
                }
            }

            Model_.RefreshEdges(); // Refresh edges since the triangles could change
        }
        private void MergeTargetVertices(w3Geoset InWhichGeoset, w3Vertex one, w3Vertex two)
        {
            one.Position.SetTo(two.Position);
            one.Normal.SetTo(two.Normal);
            one.Texture_Position.SetTo(two.Texture_Position);
            foreach (w3Triangle t in InWhichGeoset.Triangles)
            {
                if (t.Vertex1 == two) t.Vertex1 = one;
                if (t.Vertex2 == two) t.Vertex2 = one;
                if (t.Vertex3 == two) t.Vertex3 = one;
            }
           InWhichGeoset.Vertices.Remove(two);
        }
         
        private void RemoveUnusedTextures()
        {
            foreach (w3Texture t in Model_.Textures.ToList())
            {

                foreach (w3Material m in Model_.Materials)
                {
                    bool used = false;
                    foreach (w3Layer l in m.Layers)
                    {
                        if (l.Diffuse_Texure_ID.isStatic)
                        {
                            if (l.Diffuse_Texure_ID.StaticValue[0] == t.ID)
                            {
                                used = true; break;
                            }

                            if (used) break;
                        }
                        else
                        {
                            foreach (w3Keyframe k in l.Diffuse_Texure_ID.Keyframes)
                            {
                                if (k.Data[0] == t.ID) { used = true; break; }
                                if (used) break;
                            }
                            if (used) break;
                        }
                        if (used) break;

                    }
                    if (!used) Model_.Textures.Remove(t);
                }

            }
        }
        private void RemoveUnusedMaterials()
        {
            foreach (w3Material m in Model_.Materials.ToList())
            {
                if (Model_.Geosets.Any(x => x.Material_ID == m.ID) == false)
                {
                    Model_.Materials.Remove(m);
                }
            }
        }
        private void RemoveUnusedTextureAnims()
        {
            foreach (w3Texture_Animation ta in Model_.Texture_Animations.ToList())
            {
                foreach (w3Material m in Model_.Materials.ToList())
                {

                    if (m.Layers.Any(x => x.Animated_Texture_ID == m.ID) == false)
                    {
                        Model_.Texture_Animations.Remove(ta);
                    }
                }
            }

        }
        private void RemoveUnusedGlobalSequences()
        {
            foreach (w3Global_Sequence gs in Model_.Global_Sequences.ToList())
            {
                if (Model_.Transformations.Any(x => x.Global_Sequence_ID == gs.ID) == false)
                {
                    Model_.Global_Sequences.Remove(gs);
                }
            }
        }
        private void RemoveFreeHelpers()
        {
            foreach (w3Node node in Model_.Nodes.ToList())
            {
                if (node.Data is Helper)
                {
                    if (Model_.Nodes.Any(parentNode => parentNode.parentId == node.objectId) == false)
                    {
                        Model_.Nodes.Remove(node);
                    }
                }
            }
        }
        private void RemoveUnAttachedBones()
        {
            List<w3Node> unused = new List<w3Node> ();
            foreach (w3Node node in Model_.Nodes)
            {
                if (node.Data is Bone)
                {
                    bool used = false;
                    foreach (w3Geoset g in Model_.Geosets)
                    {
                        foreach (w3Vertex w3Vertex in g.Vertices)
                        {
                            foreach (int id in w3Vertex.AttachedTo)
                            {
                                if (id == node.objectId)
                                { used = true; break; }
                            }
                            if (used) break;
                        }
                        if (used) break;
                    }
                    if (!used) { unused.Add(node); }
                }
            }
            foreach (w3Node node in unused)
            {
                Model_.Nodes.Remove(node);
            }
        }
        private void ConvertFreeBonesToHelpers()
        {
            foreach (w3Node node in Model_.Nodes )
            {
                if (node.Data is Bone)
                {
                    if (Model_.Nodes.Any(x => x.parentId == node.objectId) == true)
                    {
                        if (BoneHasAttachees(node.objectId) == false)
                        {
                            node.Data = new Helper();
                        }
                    }
                }
            }
        }
        private bool BoneHasAttachees(int id)
        {
            foreach (w3Geoset geoset in Model_.Geosets)
            {
                foreach (w3Vertex v in geoset.Vertices)
                {
                    foreach (int i in v.AttachedTo)
                    {
                        if (i == id) { return true; }
                    }

                   


                }
            }
            return false;
        }
        private void RemoveUnusedKeyframes()
        {
            
            foreach (w3Transformation t in Model_.Transformations)
            {
               if (t.isStatic == false)
                {
                    foreach (w3Keyframe k in t.Keyframes.ToList())
                    {
                        if (Model_.Sequences.Any(x=> k.Track >= x.From && k.Track <= k.Track) == false)
                        {
                            t.Keyframes.Remove(k);
                        }
                    }
                }
            }
        }
        private void SetInterpolationNoneForVisibilities()
        {
            //visibility at attachment, pe1,pe2,re
            foreach (w3Node node in Model_.Nodes)
            {
                if (node.Data is Ribbon_Emitter)
                {
                    Ribbon_Emitter re = (Ribbon_Emitter)node.Data;
                    if (re.Visibility.isStatic == false)
                    {
                        if (re.Visibility.Interpolation_Type != InterpolationType.DontInterp)
                        {
                            re.Visibility.Interpolation_Type = InterpolationType.DontInterp;
                        }
                    }
                }
                if (node.Data is Particle_Emitter_1)
                {
                    Particle_Emitter_1 re = (Particle_Emitter_1)node.Data;
                    if (re.Visibility.isStatic == false)
                    {
                        if (re.Visibility.Interpolation_Type != InterpolationType.DontInterp)
                        {
                            re.Visibility.Interpolation_Type = InterpolationType.DontInterp;
                        }
                    }
                }
                if (node.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 re = (Particle_Emitter_2)node.Data;
                    if (re.Visibility.isStatic == false)
                    {
                        if (re.Visibility.Interpolation_Type != InterpolationType.DontInterp)
                        {
                            re.Visibility.Interpolation_Type = InterpolationType.DontInterp;
                        }
                    }
                }
            }
        }
        private void LinearizeAnimations()
        {
            foreach (w3Transformation t in Model_.Transformations)
            {
                t.Interpolation_Type = InterpolationType.Linear;
            }
        }

        private void RemoveUnAnimatedSequences()
        {
            foreach (w3Sequence sequence in Model_.Sequences.ToList())
            {
                bool met = false;
                foreach (w3Transformation t in Model_.Transformations)
                {
                    foreach (w3Keyframe k in t.Keyframes)
                    {
                        if (k.Track >= sequence.From && k.Track <= sequence.To) { met = true; break; }
                    }
                    if (met) break;
                }
                if (!met) { Model_.Sequences.Remove(sequence); }
            }
        }
        private void RemoveUnusedEventObjects()
        {
            foreach (w3Node node in Model_.Nodes.ToList())
            {
                
                if (node.Data is Event_Object)
                {
                    Event_Object event_Object = (Event_Object)node.Data;
                    
                    foreach (int track in event_Object.Tracks)
                    {
                        if (Model_.Sequences.Any(x=> track >= x.From && track <= x.To) == false)
                        {
                            Model_.Nodes.Remove(node);
                        }
                    }
                }
            }
        }
        private string GetFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MDL Files (*.mdl)|*.mdl", // Filter for .mdl files
                Title = "Open MDL File" // Title for the dialog
            };

            // Show the dialog and check if a file was selected
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;

            }
            return "";
        }

        private void OptimizeAndCloseTarget(object sender, RoutedEventArgs e)
        {
            string filename = "";
            if (filename == "") { filename = GetFile(); }
            if (filename == "") { return; }
             w3Model TemporaryModel = new w3Model();
            List<Token> tokens = Parser_MDL.Tokenize(filename);
            List<TemporaryObject> temporaryObjects = Parser_MDL.SplitCollectObjects(tokens);
            TemporaryModel = Parser_MDL.Parse(temporaryObjects);

            TemporaryModel.FinalizeComponents();
            TemporaryModel.Optimize();
            TemporaryModel_Placeholder = Model_;
            Model_ = TemporaryModel;
            finalize = false;
            Optimize(null,null);
            File.WriteAllText(filename, TemporaryModel.ToMDL());
            finalize = true;
            Model_ = TemporaryModel_Placeholder;
            TemporaryModel_Placeholder = new w3Model()
                ;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
}
