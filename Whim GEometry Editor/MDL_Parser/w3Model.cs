
using MDLLibs.Classes.Misc;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Numerics;
using System.Security;
using System.Text;
using System.Windows;
using Whim_GEometry_Editor.Misc;

namespace MDLLib
{
    public enum FilterMode
    {
        None,
        Additive,
        AddAlpha,
        Blend,
        Transparent,
        Modulate,
        Modulate2x,
        AlphaKey,
    }
    public enum InterpolationType
    {
        DontInterp,
        Linear,
        Hermite,
        Bezier
    }

    public enum TransformationType
    {
        Translation, // any positive or negative float
        Rotation, // any float between -360 and 360
        Scaling, // any positive integer
        Visibility, // only "True" or "False"
        Alpha, // any integer between 0 and 100
        Int, // any positive integer
        Float, // any positive float
        Color, // any integer between 0 and 255
        Undefined,
        ID,
    }
    public enum PlaneType
    {
        XY, // Circle in the XY plane (parallel to the ground if Z is vertical)
        XZ, // Circle in the XZ plane (front-facing view if Y is vertical)
        YZ  // Circle in the YZ plane (side view if X is horizontal)
    }

    public enum CollisionShapeType
    {
        Box, Sphere
    }
    public class w3Line
    {
        public Coordinate From;
        public Coordinate To;
    }
    
    internal static class MDLHelper
    {
        internal static Dictionary<string, string> NodeNameRef = new Dictionary<string, string>()
        {
            { "Bone", "Bone" },
            { "w3Attachment", "Attachment" },
            { "Collision_Shape", "CollisionShape" },
            { "Light_W3", "Light" },
            { "Particle_Emitter_1", "ParticleEmitter" },
            { "Particle_Emitter_2", "ParticleEmitter2" },
            { "Helper", "Helper" },
            { "Event_Object", "EventObject" },
            { "Ribbon_Emitter", "RibbonEmitter" },

        };

        internal static (List<List<int>> matrixgroups, List<int> vertexgroup) GetMatrixGroups(List<w3Vertex> vertices)
        {
            // List to store unique matrix groups
            List<List<int>> matrixgroups = new List<List<int>>();

            // List to store the index in matrixgroups corresponding to each vertex
            List<int> vertexgroup = new List<int>();

            // Iterate through each vertex
            foreach (w3Vertex vertex in vertices)
            {
                if (vertex.AttachedTo.Count == 0) { MessageBox.Show("Empty attachedto:" + vertex.Id.ToString()); }
                // Sort the bone indices to ensure consistent comparison
                List<int> sortedBones = vertex.AttachedTo.OrderBy(bone => bone).ToList();

                // Try to find a matrix group that matches the sorted bone list
                int groupIndex = matrixgroups.FindIndex(group => group.SequenceEqual(sortedBones));

                if (groupIndex == -1) // No match found, so this is a new matrix group
                {
                    // Add the new matrix group
                    matrixgroups.Add(new List<int>(sortedBones));
                    // Assign the new group index to this vertex
                    vertexgroup.Add(matrixgroups.Count - 1);
                }
                else
                {
                    // Vertex belongs to an existing matrix group
                    vertexgroup.Add(groupIndex);
                }
            }

            // Return both matrixgroups and vertexgroup as a tuple
            return (matrixgroups, vertexgroup);
        }


    }
    public class Extent
    {
        internal float Minimum_X { get; set; } = 0;
        internal float Minimum_Y { get; set; } = 0;
        internal float Minimum_Z { get; set; } = 0;
        internal float Maximum_X { get; set; } = 0;
        internal float Maximum_Y { get; set; } = 0;
        internal float Maximum_Z { get; set; } = 0;
        internal float Bounds_Radius { get; set; } = 0;
        internal Coordinate GetCentroid()
        {
            float centroidX = (Minimum_X + Maximum_X) / 2;
            float centroidY = (Minimum_Y + Maximum_Y) / 2;
            float centroidZ = (Minimum_Z + Maximum_Z) / 2;
            return new Coordinate
            {
                X = centroidX,
                Y = centroidY,
                Z = centroidZ
            };
        }
        internal string ToMDL()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"MinimumExtent {{ {Minimum_X:f6}, {Minimum_Y:f6}, {Minimum_Z:f6} }},");
            sb.AppendLine($"MaximumExtent {{ {Maximum_X}, {Maximum_Y}, {Maximum_Z:f6} }},");
            sb.AppendLine($"BoundsRadius {Bounds_Radius:f6},");
            return sb.ToString();
        }
        internal void CalculateBoundsRadius()
        {
            float deltaX = Maximum_X - Minimum_X;
            float deltaY = Maximum_Y - Minimum_Y;
            float deltaZ = Maximum_Z - Minimum_Z;
            float squaredDistance = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
            Bounds_Radius = (float)Math.Sqrt(squaredDistance) / 2;
        }
        internal Extent(float minimum_X, float minimum_Y, float minimum_Z,
                      float maximum_X, float maximum_Y, float maximum_Z,
                      float bounds_Radius)
        {
            Minimum_X = minimum_X;
            Minimum_Y = minimum_Y;
            Minimum_Z = minimum_Z;
            Maximum_X = maximum_X;
            Maximum_Y = maximum_Y;
            Maximum_Z = maximum_Z;
            Bounds_Radius = bounds_Radius;
        }
        internal Extent Clone()
        {
            return new Extent(Minimum_X, Minimum_Y, Minimum_Z, Maximum_X, Maximum_Y, Maximum_Z, Bounds_Radius);
        }
        internal Extent()
        {
            Minimum_X = 0;
            Minimum_Y = 0;
            Minimum_Z = 0;
            Maximum_X = 0;
            Maximum_Y = 0;
            Maximum_Z = 0;
            Bounds_Radius = 0;
        }
        internal void GetMaxFromList(List<float> list)
        {
            Maximum_X = list[0];
            Maximum_Y = list[1];
            Maximum_Z = list[2];
        }
        internal void GetMinFromList(List<float> list)
        {
            Minimum_X = list[0];
            Minimum_Y = list[1];
            Minimum_Z = list[2];
        }
        internal bool minimumExtentsAreZero()
        {
            if (Minimum_X == 0 && Minimum_Y == 0 && Minimum_Z == 0) { return true; }
            else { return false; }
        }
        internal bool maximumExtentsAreZero()
        {
            if (Maximum_X == 0 && Maximum_Y == 0 && Maximum_Z == 0) { return true; }
            else { return false; }
        }
        public override string ToString()
        {
            return $"Minimum:{Minimum_X:f6} {Minimum_Y:f6} {Minimum_Z:f6}\nMaximum: {Maximum_X:f6} {Maximum_Y:f6} {Maximum_Z:f6}\nBounds radius: {Bounds_Radius:f6}";
        }

        internal void MinFromFloatArray(float[] vector)
        {
            Minimum_X = vector[0];
            Minimum_Y = vector[1];
            Minimum_Z = vector[2];
        }

        internal void MaxFromFloatArray(float[] vector)
        {
            Maximum_X = vector[0];
            Maximum_Y = vector[1];
            Maximum_Z = vector[2];
        }

        internal void Change(float x, float y, float z)
        {
           Minimum_X+= x;
            Minimum_Y+= y;
            Minimum_Z+= z;
            Maximum_X+= x;
            Maximum_Y+= y;
            Maximum_Z+= z;
        }
    }
    static class IDCounter
    {
        static int index = 0;
        public static int Next() { return index++; }

        internal static void Reset()
        {
            index = 0;
        }
    }
    public class w3Model
    {
        public List<w3Line> CollisionShapeLines { get; set; } = new List<w3Line>();
        public List<w3Line> GeosetLines { get; set; } = new List<w3Line>();
        public void CalculateCollisionShapeEdges()
        {
            CollisionShapeLines.Clear();
            foreach (w3Node node in Nodes)
            {
                if (node.Data is Collision_Shape)
                {
                    Collision_Shape cs = (Collision_Shape)node.Data;

                    if (cs.Type == CollisionShapeType.Sphere)
                    {
                        float radius = cs.Extents.Bounds_Radius;
                        Coordinate center =  new Coordinate(cs.Extents.Minimum_X, cs.Extents.Minimum_Y, cs.Extents.Minimum_Z); // Assuming there's a center property

                        // Add lines to approximate the sphere (in XY, XZ, YZ planes)
                        AddCircleEdges(center, radius, PlaneType.XY);
                        AddCircleEdges(center, radius, PlaneType.XZ);
                        AddCircleEdges(center, radius, PlaneType.YZ);
                    }
                    else // CollisionShapeType.Box
                    {
                        float minx = cs.Extents.Minimum_X;
                        float miny = cs.Extents.Minimum_Y;
                        float minz = cs.Extents.Minimum_Z;
                        float maxx = cs.Extents.Maximum_X;
                        float maxy = cs.Extents.Maximum_Y;
                        float maxz = cs.Extents.Maximum_Z;

                        // Add the 12 edges for the box
                        AddBoundingBoxEdges(minx, miny, minz, maxx, maxy, maxz, CollisionShapeLines);
                    }
                }
            }
        }

        private void AddCircleEdges(Coordinate center, float radius, PlaneType plane)
        {
            int segments = 16; // Number of line segments to approximate the circle
            for (int i = 0; i < segments; i++)
            {
                // Use floats for angles and trigonometric functions
                float angle1 = (float)(i * (2 * Math.PI) / segments);
                float angle2 = (float)((i + 1) * (2 * Math.PI) / segments);

                Coordinate p1 = new Coordinate();
                Coordinate p2 = new Coordinate();

                if (plane == PlaneType.XY)
                {
                    p1 = new Coordinate(center.X + radius * (float)Math.Cos(angle1), center.Y + radius * (float)Math.Sin(angle1), center.Z);
                    p2 = new Coordinate(center.X + radius * (float)Math.Cos(angle2), center.Y + radius * (float)Math.Sin(angle2), center.Z);
                }
                else if (plane == PlaneType.XZ)
                {
                    p1 = new Coordinate(center.X + radius * (float)Math.Cos(angle1), center.Y, center.Z + radius * (float)Math.Sin(angle1));
                    p2 = new Coordinate(center.X + radius * (float)Math.Cos(angle2), center.Y, center.Z + radius * (float)Math.Sin(angle2));
                }
                else if (plane == PlaneType.YZ)
                {
                    p1 = new Coordinate(center.X, center.Y + radius * (float)Math.Cos(angle1), center.Z + radius * (float)Math.Sin(angle1));
                    p2 = new Coordinate(center.X, center.Y + radius * (float)Math.Cos(angle2), center.Z + radius * (float)Math.Sin(angle2));
                }

                CollisionShapeLines.Add(new w3Line { From = p1, To = p2 });
            }
        }

        private void AddBoundingBoxEdges(float minx, float miny, float minz, float maxx, float maxy, float maxz, List<w3Line> lines )
        {
            // Define the 8 corners of the box
            Coordinate[] corners = new Coordinate[]
            {
             new Coordinate(minx, miny, minz),
              new Coordinate(maxx, miny, minz),
              new Coordinate(minx, maxy, minz),
              new Coordinate(maxx, maxy, minz),
             new Coordinate(minx, miny, maxz),
             new Coordinate(maxx, miny, maxz),
             new Coordinate(minx, maxy, maxz),
             new Coordinate(maxx, maxy, maxz)
            };

            // Define the 12 edges (from -> to) connecting the corners
            int[,] edgeIndices = new int[,]
            {
        { 0, 1 }, { 0, 2 }, { 0, 4 },
        { 1, 3 }, { 1, 5 }, { 2, 3 },
        { 2, 6 }, { 3, 7 }, { 4, 5 },
        { 4, 6 }, { 5, 7 }, { 6, 7 }
            };

            // Add each edge to CollisionShapeEdges
            for (int i = 0; i < edgeIndices.GetLength(0); i++)
            {
                Coordinate from = corners[edgeIndices[i, 0]];
                Coordinate to = corners[edgeIndices[i, 1]];
                lines.Add(new w3Line { From = from, To = to });
            }
        }

        public void CalculateGeosetBoundingBoxes()
        {
            GeosetLines.Clear();
            foreach (w3Geoset geo in Geosets)
            {
                float minx = geo.Extent.Minimum_X;
                float miny = geo.Extent.Minimum_Y;
                float minz = geo.Extent.Minimum_Z;
                float maxx = geo.Extent.Maximum_X;
                float maxy = geo.Extent.Maximum_Y;
                float maxz = geo.Extent.Maximum_Z;

                // Add the 12 edges for the box
                AddBoundingBoxEdges(minx, miny, minz, maxx, maxy, maxz, GeosetLines);
            }
        }
        internal string ToMDL()
        {

            // StringBuilder sh = new StringBuilder();
            // foreach (w3Vertex v in Geosets[0].Vertices) { sh.AppendLine(string.Join(",", v.AttachedTo)); }
            // MessageBox.Show(sh.ToString());
            StringBuilder result = new();
            // add the model info
            
            result.AppendLine("//+-----------------------------------------------------------------------------");
            result.AppendLine($"//| {Name}.mdl");
            result.AppendLine($"//| Generated by {AppInfo.AppTitle} v{AppInfo.Version}");
            result.AppendLine($"//| {DateTime.Now.ToString()}");
            result.AppendLine("//+-----------------------------------------------------------------------------");
            result.AppendLine("Version {");
            result.AppendLine($"FormatVersion {800},");
            result.AppendLine("}");
            result.AppendLine($"Model \"{Name}\" {{");
            if (Geosets.Count > 0)
            {
                result.AppendLine($"\t NumGeosets {Geosets.Count},");
            }

            if (Geoset_Animations.Count > 0)
            {
                result.AppendLine($"\t NumGeosetAnims {Geoset_Animations.Count},");
            }
            if (Nodes.Count(x => x.Data is Helper) > 0)
            {
                result.AppendLine($"\t NumHelpers {Nodes.Count(x => x.Data is Helper)},");
            }
            if (Nodes.Count(x => x.Data is Bone) > 0)
            {
                result.AppendLine($"\t NumBones {Nodes.Count(x => x.Data is Bone)},");
            }
            if (Nodes.Count(x => x.Data is w3Light) > 0)
            {
                result.AppendLine($"\t NumLights {Nodes.Count(x => x.Data is w3Light)},");
            }
            if (Nodes.Count(x => x.Data is w3Attachment) > 0)
            {
                result.AppendLine($"\t NumAttachments {Nodes.Count(x => x.Data is w3Attachment)},");
            }
            if (Nodes.Count(x => x.Data is Particle_Emitter_1) > 0)
            {
                result.AppendLine($"\t NumParticleEmitters {Nodes.Count(x => x.Data is Particle_Emitter_1)},");
            }
            if (Nodes.Count(x => x.Data is Particle_Emitter_2) > 0)
            {
                result.AppendLine($"\t NumParticleEmitters2 {Nodes.Count(x => x.Data is Particle_Emitter_2)},");
            }
            if (Nodes.Count(x => x.Data is Ribbon_Emitter) > 0)
            {
                result.AppendLine($"\t NumRibbonEmitters {Nodes.Count(x => x.Data is Ribbon_Emitter)},");
            }
            result.AppendLine($"\t MinimumExtent {{{Extents.Minimum_X:f6}, {Extents.Minimum_Y:f6}, {Extents.Minimum_Z:f6} }},");
            result.AppendLine($"\t MaximumExtent {{{Extents.Maximum_X:f6}, {Extents.Maximum_Y:f6}, {Extents.Maximum_Z:f6} }},");
            result.AppendLine($"\t BoundsRadius {Extents.Bounds_Radius:f6},");
            result.AppendLine($"\t BlendTime {BlendTime},");
            result.AppendLine($"}}");
            // add the sequences
            if (Sequences.Count > 0)
            {
                result.AppendLine($"Sequences {Sequences.Count} {{");
                foreach (w3Sequence s in Sequences)
                {
                    result.AppendLine(s.ToMDL());
                }
                result.AppendLine("}");
            }
            // add global sequences
            if (Global_Sequences.Count > 0)
            {
                result.AppendLine($"GlobalSequences {Global_Sequences.Count}{{");
                foreach (w3Global_Sequence seq in Global_Sequences)
                {
                    result.AppendLine($"\tDuration {seq.Duration},");
                }
                result.AppendLine($"}}");
            }
            // add the textures
            if (Textures.Count > 0)
            {
                result.AppendLine($"Textures {Textures.Count} {{");
                foreach (w3Texture texture in Textures)
                {
                    result.AppendLine(texture.ToMDL());
                }
                result.AppendLine($"}}");
            }
            // add the materials
            if (Materials.Count > 0)
            {
                result.AppendLine($"Materials {Materials.Count} {{");
                foreach (w3Material mat in Materials)
                {
                    result.AppendLine(mat.ToMDL());// UNFHINISHED
                }
                result.AppendLine("}");
            }
            // add the texture animations
            if (Texture_Animations.Count > 0)
            {
                result.AppendLine($"TextureAnims {Materials.Count} {{");
                foreach (w3Texture_Animation ta in Texture_Animations)
                {
                    result.AppendLine(ta.toMDL());
                }
                result.AppendLine("}");
            }
            // add the geosets
            foreach (w3Geoset g in Geosets)
            {
                result.AppendLine(g.ToMDL()); // UNFHINISHED
            }
            // add the geoset animations
            
            {
                foreach (w3Geoset_Animation ga in Geoset_Animations)
                {
                    result.AppendLine(ga.ToMDL());
                }

                // add the nodes
                int index = -1;
                foreach (w3Node n in Nodes)
                {
                    index++;
                    result.AppendLine(n.ToMDL(index, Nodes, Geosets, Geoset_Animations));// UNFHINISHED
                }
                // add the pivot points
                result.AppendLine(CollectPivotPoints());
                // add the cameras
                if (Cameras.Count > 0)
                {
                    foreach (w3Camera cam in Cameras)
                    {
                        result.AppendLine($"\tCamera \"{cam.Name}\" {{");
                        result.AppendLine(cam.toMDL());
                        result.AppendLine($"}}");
                    }
                }









                return result.ToString();
            }
        }
        public   void CalculateExtents()
        {
           if (Geosets.Count == 0)
            {
                Extents = new Extent();
                foreach (w3Sequence seq in Sequences) { seq.Extent = new Extent(); }
                return;
            }
            RecalculateGeosetExtents(); //done - best of all vertex extents 
           
            RecalculateSequenceExtents();  // done -  best of all transformation extents for each sequence. 
            
            RecalculateModelExtents(); //done - best of all geoset extents
            FixCollisionShapeExtents();


        }
        private void FixCollisionShapeExtents()
        {
            foreach (w3Node n in Nodes)
            {
                if (n.Data is Collision_Shape)
                {
                    Collision_Shape c = (Collision_Shape)n.Data;
                    if (c.Type == CollisionShapeType.Sphere)
                    {
                        if (c.Extents.Bounds_Radius <= 0) { c.Extents.Bounds_Radius = 1; }
                    }
                    else
                    {
                        if (c.Extents.Minimum_X > c.Extents.Maximum_X)
                        {
                            c.Extents.Minimum_X = c.Extents.Maximum_X - 1;
                        }
                        if (c.Extents.Minimum_Y > c.Extents.Maximum_Y)
                        {
                            c.Extents.Minimum_Y = c.Extents.Maximum_Y - 1;
                        }
                        if (c.Extents.Minimum_Z > c.Extents.Maximum_Z)
                        {
                            c.Extents.Minimum_Z = c.Extents.Maximum_Z - 1;
                        }
                    }
                }
            }
        }
        private   void RecalculateGeosetExtents()
        {
            for (int i = 0; i <  Geosets.Count; i++)
            {
                 Geosets[i].Extent = CalculateGeosetExtent( Geosets[i].Vertices);
            }
        }
        private void RecalculateModelExtents()
        {
            // Initialize model extents
            Extent modelExtent = new Extent
            {
                Minimum_X = float.MaxValue,
                Minimum_Y = float.MaxValue,
                Minimum_Z = float.MaxValue,
                Maximum_X = float.MinValue,
                Maximum_Y = float.MinValue,
                Maximum_Z = float.MinValue,
                Bounds_Radius = 0 // Initialize the radius as 0
            };

            foreach (w3Geoset geoset in Geosets)
            {
                // Calculate the minimum extents
                modelExtent.Minimum_X = Math.Min(modelExtent.Minimum_X, geoset.Extent.Minimum_X);
                modelExtent.Minimum_Y = Math.Min(modelExtent.Minimum_Y, geoset.Extent.Minimum_Y);
                modelExtent.Minimum_Z = Math.Min(modelExtent.Minimum_Z, geoset.Extent.Minimum_Z);

                // Calculate the maximum extents
                modelExtent.Maximum_X = Math.Max(modelExtent.Maximum_X, geoset.Extent.Maximum_X);
                modelExtent.Maximum_Y = Math.Max(modelExtent.Maximum_Y, geoset.Extent.Maximum_Y);
                modelExtent.Maximum_Z = Math.Max(modelExtent.Maximum_Z, geoset.Extent.Maximum_Z);

                // Update the bounding radius to the largest one
                modelExtent.Bounds_Radius = Math.Max(modelExtent.Bounds_Radius, geoset.Extent.Bounds_Radius);
            }

            // Assign the calculated extents to the model
            Extents = modelExtent.Clone();
             
        }

        private Extent CalculateGeosetExtent(List<w3Vertex> vertices)
        {
            if (vertices == null || vertices.Count == 0)
                return null;
            Extent extent = new Extent
            {
                Minimum_X = vertices[0].Position.X,
                Minimum_Y = vertices[0].Position.Y,
                Minimum_Z = vertices[0].Position.Z,
                Maximum_X = vertices[0].Position.X,
                Maximum_Y = vertices[0].Position.Y,
                Maximum_Z = vertices[0].Position.Z
            };
            foreach (var vertex in vertices)
            {
                extent.Minimum_X = Math.Min(extent.Minimum_X, vertex.Position.X);
                extent.Minimum_Y = Math.Min(extent.Minimum_Y, vertex.Position.Y);
                extent.Minimum_Z = Math.Min(extent.Minimum_Z, vertex.Position.Z);
                extent.Maximum_X = Math.Max(extent.Maximum_X, vertex.Position.X);
                extent.Maximum_Y = Math.Max(extent.Maximum_Y, vertex.Position.Y);
                extent.Maximum_Z = Math.Max(extent.Maximum_Z, vertex.Position.Z);
            }
            // Calculate bounds radius
            float centerX = (extent.Maximum_X + extent.Maximum_X) / 2;
            float centerY = (extent.Maximum_Y + extent.Maximum_Y) / 2;
            float centerZ = (extent.Maximum_Z + extent.Maximum_Z) / 2;
            double maxDistance = 0;
            foreach (var vertex in vertices)
            {
                double distance = Math.Sqrt(Math.Pow(vertex.Position.X - centerX, 2) +
                                            Math.Pow(vertex.Position.Y - centerY, 2) +
                                            Math.Pow(vertex.Position.Z - centerZ, 2));
                maxDistance = Math.Max(maxDistance, distance);
            }
            extent.Bounds_Radius = (float)maxDistance;
            return extent;
        }


        private static Coordinate GetVertexAfterTranslation(Coordinate vPos, float x, float y, float z)
        {
            return new Coordinate(vPos.X + x, vPos.Y + y, vPos.Z + z);
        }

        private   void RecalculateSequenceExtents()
        {
            foreach (w3Sequence sequence in  Sequences)
            {
                List<Extent> Extents = new List<Extent>();
                foreach (w3Geoset geoset in  Geosets)
                {
                    Extent collectiveExtent = new Extent();
                    List<Coordinate> ModifiedCoords = new List<Coordinate>();
                    foreach (w3Vertex vertex in geoset.Vertices)
                    {
                        foreach (w3Node node in  Nodes)
                        {
                            //translation
                            foreach (w3Keyframe kf in node.Translation.Keyframes)
                            {
                                if (kf.Track >= sequence.From && kf.Track <= sequence.To)
                                {
                                    ModifiedCoords.Add(GetVertexAfterTranslation(vertex.Position, kf.Data[0], kf.Data[1], kf.Data[2]));
                                }
                            }
                            //rotation
                            foreach (w3Keyframe kf in node.Rotation.Keyframes)
                            {
                                if (kf.Track >= sequence.From && kf.Track <= sequence.To)
                                {
                                    Coordinate bonePos =  Nodes.Find(x => x.objectId == vertex.AttachedTo[0]).PivotPoint;
                                    ModifiedCoords.Add(GetVertexAfterRotating(vertex.Position, bonePos, kf.Data[0], kf.Data[1], kf.Data[2]));
                                }
                            }
                            //scaling
                            foreach (w3Keyframe kf in node.Scaling.Keyframes)
                            {
                                if (kf.Track >= sequence.From && kf.Track <= sequence.To)
                                {
                                    ModifiedCoords.Add(GetVertexAfterScaling(vertex.Position, kf.Data[0], kf.Data[1], kf.Data[2]));
                                }
                            }
                        }
                    }

                       
                    if (ModifiedCoords.Count == 0)
                    {
                        collectiveExtent = CalculateCollectiveExtent( Geosets.Select(x => x.Extent).ToList());

                    }
                    else
                    {
                        collectiveExtent = Calculator3D.CalculateCollectiveExtentFromCoordinates(ModifiedCoords);

                    }
                    collectiveExtent.CalculateBoundsRadius();
                    Extents.Add(collectiveExtent);
                }
                sequence.Extent = Calculator3D.CalculateCollectiveExtent(Extents);
                sequence.Extent.CalculateBoundsRadius();
            }
            FillEmptyExtents();
        }
         Coordinate GetVertexAfterRotating(Coordinate vPos, Coordinate BonePos, float x, float y, float z)
        {
            return Calculator3D.RotateAroundBone(vPos, BonePos, x, y, z);
        }
        Coordinate GetVertexAfterScaling(Coordinate vPos, float x, float y, float z)
        {
            return new Coordinate(
                vPos.X * (x / 100),
                vPos.Y * (y / 100),
                vPos.Z * (z / 100)

                );

        }
        Extent CalculateCollectiveExtent(List<Extent> extents)
        {
            if (extents == null || extents.Count == 0)
                throw new ArgumentException("Extents list cannot be null or empty.");
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            foreach (var extent in extents)
            {
                minX = Math.Min(minX, extent.Minimum_X);
                minY = Math.Min(minY, extent.Minimum_Y);
                minZ = Math.Min(minZ, extent.Minimum_Z);
                maxX = Math.Max(maxX, extent.Maximum_X);
                maxY = Math.Max(maxY, extent.Maximum_Y);
                maxZ = Math.Max(maxZ, extent.Maximum_Z);
            }
            return new Extent
            {
                Minimum_X = minX,
                Minimum_Y = minY,
                Minimum_Z = minZ,
                Maximum_X = maxX,
                Maximum_Y = maxY,
                Maximum_Z = maxZ
            };
        }
        void FillEmptyExtents()
        {
            foreach (w3Geoset geoset in ModelHelper.Current.Geosets)
            {
                geoset.SequenceExtents.Clear();
                foreach (w3Sequence sequence in ModelHelper.Current.Sequences)
                {
                    geoset.SequenceExtents.Add(sequence.Extent.Clone());
                }
            }
        }
        private void FixGlobalSequenceID(w3Transformation t)
        {
            if (t.Global_Sequence_ID > 0 &&
                   t.Global_Sequence_ID < Global_Sequences.Count)
            {
                t.Global_Sequence_ID = Global_Sequences[t.Global_Sequence_ID].ID;
            }
            else { t.Global_Sequence_ID = -1; }
        }
        private void CreateTextureIfNoteTexturesButMaterials()
        {
            if (Textures.Count == 0 && Materials.Count > 0)
            {
                w3Texture textures = new w3Texture(IDCounter.Next(),"Textures\\white.blp", false, false, 0);
                Textures.Add(textures);
            }
        }
        public void GiveIDs()
        {
             
            // sequence, camera = no id
            // leafs
            foreach (w3Global_Sequence s in Global_Sequences)  s.ID = IDCounter.Next();  //nodes, layer alpha, layer diffuse, tax3, ga alpha color, camera pos, camera target, cmera rotation
            foreach (w3Texture t in Textures)  t.ID = IDCounter.Next();
             //----------------------------------------
                    
           
            foreach (w3Texture_Animation texture_Animation in Texture_Animations)
            {
                texture_Animation.ID = IDCounter.Next();
                FixGlobalSequenceID(texture_Animation.Translation);
                FixGlobalSequenceID(texture_Animation.Scaling);
                FixGlobalSequenceID(texture_Animation.Rotation);

            }
           
            foreach (w3Material m in Materials)
            {
                m.ID = IDCounter.Next();

                foreach (w3Layer l in m.Layers)
                {
                    l.ID = IDCounter.Next();
                    FixGlobalSequenceID(l.Diffuse_Texure_ID);
                    FixGlobalSequenceID(l.Alpha);
                    
                    int TextureAnimIndex = l.Animated_Texture_ID;
                    // set texture id
                    if (l.Diffuse_Texure_ID.isStatic)
                    {
                        int rawTextureIndex = (int)l.Diffuse_Texure_ID.StaticValue[0];
                       
                        if (rawTextureIndex >= 0 && rawTextureIndex < Textures.Count)
                        {
                            l.Diffuse_Texure_ID.StaticValue = [Textures[rawTextureIndex].ID];
                        }
                        else { l.Diffuse_Texure_ID.StaticValue = [Textures[0].ID]; }
                    }
                    else
                    {
                        foreach (w3Keyframe keyframe in l.Diffuse_Texure_ID.Keyframes)
                        {
                            int RawKFTextureIndex = (int)keyframe.Data[0];
                            if (RawKFTextureIndex >= 0 && RawKFTextureIndex < Textures.Count)
                            {
                                keyframe.Data = [Textures[RawKFTextureIndex].ID];
                            }
                            else { keyframe.Data= [-1]; }
                        }
                    }
                    
                    // set animated texture
                    if (TextureAnimIndex >= 0 && TextureAnimIndex < Texture_Animations.Count)
                    {
                        l.Animated_Texture_ID = Texture_Animations[TextureAnimIndex].ID;
                    }
                    else { l.Animated_Texture_ID = -1; }


                }

            }
            // all nodes, and ids
            foreach (w3Node node in Nodes)
            {
                // if (node.parentId == node.objectId) { node.parentId = -1; }
                node.objectId = IDCounter.Next();
               
               
                if (node.Data is Event_Object)
                {
                    Event_Object event_Object = (Event_Object)node.Data;
                    int gid = event_Object.Global_sequence_ID;

                    if (gid > 0 && gid < Global_Sequences.Count)
                    {

                        event_Object.Global_sequence_ID = Global_Sequences[gid].ID;
                    }
                    else
                    {
                        event_Object.Global_sequence_ID = -1;
                    }
                }
                if (node.Data is w3Attachment)
                {
                    w3Attachment a = (w3Attachment)node.Data;
                    FixGlobalSequenceID(a.Visibility);

                }
                if (node.Data is w3Light)
                {
                    w3Light light = (w3Light)node.Data;
                    FixGlobalSequenceID(light.Visibility);
                    FixGlobalSequenceID(light.Ambient_Intensity);
                    FixGlobalSequenceID(light.Intensity);
                    FixGlobalSequenceID(light.Color);
                    FixGlobalSequenceID(light.Ambient_Color);
                    FixGlobalSequenceID(light.Attenuation_End);
                    FixGlobalSequenceID(light.Attenuation_Start);
                }
                if (node.Data is Particle_Emitter_1)
                {
                    Particle_Emitter_1 pe = (Particle_Emitter_1)node.Data;
                    FixGlobalSequenceID(pe.Visibility);
                    FixGlobalSequenceID(pe.Latitude);
                    FixGlobalSequenceID(pe.Longitude);
                    FixGlobalSequenceID(pe.Gravity);
                    FixGlobalSequenceID(pe.Life_Span);
                    FixGlobalSequenceID(pe.Initial_Velocity);
                    FixGlobalSequenceID(pe.Emission_Rate);
                }
                if (node.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 pe = (Particle_Emitter_2)node.Data;
                    int index = pe.Texture_ID;
                    if (index >= 0 && index < Textures.Count)
                    {
                        pe.Texture_ID = Textures[index].ID;
                    }
                    FixGlobalSequenceID(pe.Emission_Rate);
                    FixGlobalSequenceID(pe.Gravity);
                    FixGlobalSequenceID(pe.Width);
                    FixGlobalSequenceID(pe.Length);
                    FixGlobalSequenceID(pe.Latitude);
                    FixGlobalSequenceID(pe.Speed);
                    FixGlobalSequenceID(pe.Emission_Rate);
                    FixGlobalSequenceID(pe.Visibility);
                    FixGlobalSequenceID(pe.Variation);
                }
                if (node.Data is Ribbon_Emitter)
                {
                    Ribbon_Emitter re = (Ribbon_Emitter)node.Data;
                    FixGlobalSequenceID(re.Visibility);
                    FixGlobalSequenceID(re.Height_Below);
                    FixGlobalSequenceID(re.Height_Above);
                    FixGlobalSequenceID(re.Color);
                    FixGlobalSequenceID(re.Alpha);
                    FixGlobalSequenceID(re.Texture_Slot);
                    FixGlobalSequenceID(re.Texture_Slot);
                    int mr = re.Material_ID;
                    if (mr >= 0 && mr < Materials.Count)
                    {
                        re.Material_ID = Materials[mr].ID;
                    }
                    else
                    {
                        re.Material_ID = -1;
                    }
                }
                FixGlobalSequenceID(node.Translation);
                FixGlobalSequenceID(node.Rotation);
                FixGlobalSequenceID(node.Scaling);

            }
            // all parents of nodes
            foreach (w3Node node in Nodes)
            {
                if (node.parentId < -1 || node.parentId > Nodes.Count) { node.parentId = -1;  continue; } // invalid parent 

                if (node.parentId >= 0 && node.parentId < Nodes.Count)
                {
                    node.parentId = Nodes[node.parentId].objectId;
                }
            }
            foreach (w3Camera cam in Cameras)
            {
                FixGlobalSequenceID(cam.Rotation);
                FixGlobalSequenceID(cam.Target);
                FixGlobalSequenceID(cam.Position);
            }
            foreach (w3Geoset geo in Geosets)
            {
                geo.ID = IDCounter.Next();
                if (Materials.Count > geo.Material_ID) // check if valid
                {
                    geo.Material_ID = Materials[geo.Material_ID].ID;
                }
                else { geo.Material_ID = -1; }
                foreach (w3Vertex v in geo.Vertices)
                {
                    v.Id = IDCounter.Next();
                    for (int AttachIndex = 0; AttachIndex< v.AttachedTo.Count; AttachIndex++)
                    {
                        int NodeIndex =v.AttachedTo[AttachIndex] ;
                            v.AttachedTo[AttachIndex] = Nodes[NodeIndex].objectId;
                     
                      
                    }
                }
                foreach (w3Triangle triangle in geo.Triangles)
                {
                   
                    triangle.Vertex1 = geo.Vertices[triangle.Index1];
                    triangle.Vertex2 = geo.Vertices[triangle.Index2];
                    triangle.Vertex3 = geo.Vertices[triangle.Index3];
                }                

            }
            foreach (w3Geoset_Animation ga in Geoset_Animations)
            {
                ga.ID = IDCounter.Next();
                if (ga.Geoset_ID  >=0)
                {
                    ga.Geoset_ID = Geosets[ga.Geoset_ID].ID;
                }
                 
                else { ga.Geoset_ID = -1; }

                FixGlobalSequenceID(ga.Alpha);
                FixGlobalSequenceID(ga.Color);
            }
            // bone geoset and ga ids
            foreach (w3Node node in Nodes)
            {
                if (node.Data is Bone)
                {
                    Bone b = (Bone)node.Data;
                    int gaindex = b.Geoset_Animation_ID;
                    int gindex = b.Geoset_ID;
                    if (gaindex > 0 && gaindex < Geoset_Animations.Count)
                    {
                        b.Geoset_Animation_ID = Geoset_Animations[gaindex].ID;
                    }
                    if (gindex > 0 && gindex < Geosets.Count)
                    {
                        b.Geoset_ID = Geosets[gindex].ID;
                    }

                }
            }
        }
        #region Properties
        //------------------------------------------
        // properties
        //------------------------------------------
        internal string Name { get; set; } = string.Empty;
        internal string AnimationFile { get; set; } = string.Empty;
        internal int BlendTime { get; set; } = 150;
        internal int ModelVersion { get; set; } = 0;
        internal Extent Extents { get; set; } = new();
        internal List<Coordinate> PivotPoints = new();
        #endregion
        #region Constructors
        internal w3Model(string name, string animfile, int blendtime)
        {
            Name = name;
            AnimationFile = animfile;
            BlendTime = blendtime;
        }
        internal w3Model()
        {
        }
        #endregion
        #region Components
        //------------------------------------------
        // components
        //------------------------------------------
        internal List<w3Transformation> Transformations = new List<w3Transformation>(); // here hold references to all all transformations
        internal int NumHelpers;
        internal int NumBones;
        internal int NumLights;
        internal int NumAttachments;
        internal int NumEvents;
        internal int NumParticleEmitters;
        internal int NumParticleEmitters2;
        internal int NumRibbonEmitters;

        internal List<w3Camera> Cameras { get; set; } = new List<w3Camera>();
        internal List<w3Global_Sequence> Global_Sequences { get; set; } = new List<w3Global_Sequence>();
        internal List<w3Sequence> Sequences { get; set; } = new List<w3Sequence>();
        internal List<w3Material> Materials { get; set; } = new List<w3Material>();
        internal List<w3Geoset> Geosets { get; set; } = new();
        // contains pairs of pivot points, each assigned to a node in consequtive order
        internal List<w3Geoset_Animation> Geoset_Animations { get; set; } = new List<w3Geoset_Animation>();
        public List<w3Texture> Textures { get; set; } = new();
        internal List<w3Texture_Animation> Texture_Animations { get; set; } = new List<w3Texture_Animation>();
        internal List<w3Node> Nodes { get; set; } = new List<w3Node>();
        #endregion
        #region Functions
        //------------------------------------------
        // functions
        //------------------------------------------
        public void RefreshSequenceExtents()
        {
            foreach (w3Geoset geo in Geosets)
            {
                geo.SequenceExtents.Clear();
                foreach (w3Sequence seq in Sequences)
                {
                    geo.SequenceExtents.Add(seq.Extent.Clone());
                }
            }
        }
        internal bool GlobalSequenceIsUsed(int id)
        {
            foreach (w3Transformation t in Transformations)
            {
                if (t.Global_Sequence_ID == id) { return true; }
            }
            foreach (w3Node n in Nodes)
            {
                if (n.Data is Event_Object)
                {
                    Event_Object e = (Event_Object)n.Data;
                    if (e.Global_sequence_ID == id) { return true; }
                }
            }
            return false;
        }
        internal bool MaterialIsUed(int id)
        {
            return Geosets.Any(x => x.Material_ID == id);
        }
        internal bool TextureAnimIsUsed(int id)
        {
            foreach (w3Material m in Materials)
            {
                foreach (w3Layer l in m.Layers)
                {
                    if (l.Animated_Texture_ID == id) { return true; }
                }
            }
            return false;
        }
        internal bool TextureIsUsed(int id)
        {
            foreach (w3Material m in Materials)
            {
                foreach (w3Layer l in m.Layers)
                {
                    if (l.Diffuse_Texure_ID.isStatic && l.Diffuse_Texure_ID.StaticValue[0] == id) { return true; }
                    if (l.Diffuse_Texure_ID.isStatic == false && l.Diffuse_Texure_ID.Keyframes.Any(x => x.Data[0] == id)) { return true; }
                }
            }
            return false;
        }
        internal void RemoveTargetGlobalSequenceFromAllTransformations(int id)
        {
            foreach (w3Camera cam in Cameras)
            {
                if (cam.Position.isStatic == false && cam.Position.Global_Sequence_ID == id) { cam.Position.Global_Sequence_ID = -1; }
                if (cam.Position.isStatic == false && cam.Position.Global_Sequence_ID == id) { cam.Rotation.Global_Sequence_ID = -1; }
                if (cam.Target.isStatic == false && cam.Target.Global_Sequence_ID == id) { cam.Target.Global_Sequence_ID = -1; }
            }
            foreach (w3Geoset_Animation ga in Geoset_Animations)
            {
                if (ga.Alpha.isStatic == false && ga.Alpha.Global_Sequence_ID == id) { ga.Alpha.Global_Sequence_ID = -1; }
                if (ga.Color.isStatic == false && ga.Color.Global_Sequence_ID == id) { ga.Color.Global_Sequence_ID = -1; }
            }
            foreach (w3Texture_Animation ta in Texture_Animations)
            {
                if (ta.Scaling.isStatic == false && ta.Scaling.Global_Sequence_ID == id) { ta.Scaling.Global_Sequence_ID = -1; }
                if (ta.Translation.isStatic == false && ta.Translation.Global_Sequence_ID == id) { ta.Translation.Global_Sequence_ID = -1; }
                if (ta.Rotation.isStatic == false && ta.Rotation.Global_Sequence_ID == id) { ta.Rotation.Global_Sequence_ID = -1; }
            }
            foreach (w3Material mat in Materials)
            {
                foreach (w3Layer l in mat.Layers)
                {
                    if (l.Alpha.isStatic == false && l.Alpha.Global_Sequence_ID == id) { l.Alpha.Global_Sequence_ID = -1; }
                    if (l.Diffuse_Texure_ID.isStatic == false && l.Diffuse_Texure_ID.Global_Sequence_ID == id) { l.Diffuse_Texure_ID.Global_Sequence_ID = -1; }
                }
            }
            foreach (w3Node n in Nodes)
            {
                if (n.Scaling.isStatic == false && n.Scaling.Global_Sequence_ID == id) { n.Scaling.Global_Sequence_ID = -1; }
                if (n.Translation.isStatic == false && n.Translation.Global_Sequence_ID == id) { n.Translation.Global_Sequence_ID = -1; }
                if (n.Rotation.isStatic == false && n.Rotation.Global_Sequence_ID == id) { n.Rotation.Global_Sequence_ID = -1; }
                if (n.Data is w3Attachment)
                {
                    w3Attachment m = (w3Attachment)n.Data;
                    if (m.Visibility.isStatic == false && m.Visibility.Global_Sequence_ID == id) { m.Visibility.Global_Sequence_ID = -1; }
                }
                if (n.Data is w3Light)
                {
                    w3Light m = (w3Light)n.Data;
                    if (m.Visibility.isStatic == false && m.Visibility.Global_Sequence_ID == id) { m.Visibility.Global_Sequence_ID = -1; }
                    if (m.Color.isStatic == false && m.Color.Global_Sequence_ID == id) { m.Color.Global_Sequence_ID = -1; }
                    if (m.Attenuation_Start.isStatic == false && m.Attenuation_Start.Global_Sequence_ID == id) { m.Attenuation_Start.Global_Sequence_ID = -1; }
                    if (m.Attenuation_End.isStatic == false && m.Attenuation_End.Global_Sequence_ID == id) { m.Attenuation_End.Global_Sequence_ID = -1; }
                    if (m.Ambient_Color.isStatic == false && m.Ambient_Color.Global_Sequence_ID == id) { m.Ambient_Color.Global_Sequence_ID = -1; }
                    if (m.Ambient_Intensity.isStatic == false && m.Ambient_Intensity.Global_Sequence_ID == id) { m.Ambient_Intensity.Global_Sequence_ID = -1; }
                    if (m.Intensity.isStatic == false && m.Intensity.Global_Sequence_ID == id) { m.Intensity.Global_Sequence_ID = -1; }
                }
                if (n.Data is Particle_Emitter_1)
                {
                    Particle_Emitter_1 m = (Particle_Emitter_1)n.Data;
                    if (m.Visibility.isStatic == false && m.Visibility.Global_Sequence_ID == id) { m.Visibility.Global_Sequence_ID = -1; }
                    if (m.Gravity.isStatic == false && m.Gravity.Global_Sequence_ID == id) { m.Gravity.Global_Sequence_ID = -1; }
                    if (m.Initial_Velocity.isStatic == false && m.Initial_Velocity.Global_Sequence_ID == id) { m.Initial_Velocity.Global_Sequence_ID = -1; }
                    if (m.Latitude.isStatic == false && m.Latitude.Global_Sequence_ID == id) { m.Latitude.Global_Sequence_ID = -1; }
                    if (m.Longitude.isStatic == false && m.Longitude.Global_Sequence_ID == id) { m.Longitude.Global_Sequence_ID = -1; }
                    if (m.Life_Span.isStatic == false && m.Life_Span.Global_Sequence_ID == id) { m.Life_Span.Global_Sequence_ID = -1; }
                    if (m.Emission_Rate.isStatic == false && m.Emission_Rate.Global_Sequence_ID == id) { m.Emission_Rate.Global_Sequence_ID = -1; }
                }
                if (n.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 m = (Particle_Emitter_2)n.Data;
                    if (m.Visibility.isStatic == false && m.Visibility.Global_Sequence_ID == id) { m.Visibility.Global_Sequence_ID = -1; }
                    if (m.Gravity.isStatic == false && m.Gravity.Global_Sequence_ID == id) { m.Gravity.Global_Sequence_ID = -1; }
                    if (m.Speed.isStatic == false && m.Speed.Global_Sequence_ID == id) { m.Speed.Global_Sequence_ID = -1; }
                    if (m.Latitude.isStatic == false && m.Latitude.Global_Sequence_ID == id) { m.Latitude.Global_Sequence_ID = -1; }
                    if (m.Width.isStatic == false && m.Width.Global_Sequence_ID == id) { m.Width.Global_Sequence_ID = -1; }
                    if (m.Length.isStatic == false && m.Length.Global_Sequence_ID == id) { m.Length.Global_Sequence_ID = -1; }
                    if (m.Variation.isStatic == false && m.Variation.Global_Sequence_ID == id) { m.Variation.Global_Sequence_ID = -1; }
                    if (m.Emission_Rate.isStatic == false && m.Emission_Rate.Global_Sequence_ID == id) { m.Emission_Rate.Global_Sequence_ID = -1; }
                }
                if (n.Data is Ribbon_Emitter)
                {
                    Ribbon_Emitter m = (Ribbon_Emitter)n.Data;
                    if (m.Visibility.isStatic == false && m.Visibility.Global_Sequence_ID == id) { m.Visibility.Global_Sequence_ID = -1; }
                    if (m.Alpha.isStatic == false && m.Alpha.Global_Sequence_ID == id) { m.Alpha.Global_Sequence_ID = -1; }
                    if (m.Color.isStatic == false && m.Color.Global_Sequence_ID == id) { m.Color.Global_Sequence_ID = -1; }
                    if (m.Height_Above.isStatic == false && m.Height_Above.Global_Sequence_ID == id) { m.Height_Above.Global_Sequence_ID = -1; }
                    if (m.Height_Below.isStatic == false && m.Height_Below.Global_Sequence_ID == id) { m.Height_Below.Global_Sequence_ID = -1; }
                    if (m.Texture_Slot.isStatic == false && m.Texture_Slot.Global_Sequence_ID == id) { m.Texture_Slot.Global_Sequence_ID = -1; }
                }
            }
        }
        internal void RemoveTargetMaterialFromGeosets(int id)
        {
            foreach (w3Geoset geo in Geosets)
            {
                if (geo.Material_ID == id)
                {
                    geo.Material_ID = Materials.Count == 0 ? -1 : 0;
                }
            }
        }
        internal void RemoveTargetTextureFromLayers(int id)
        {
            foreach (w3Material m in Materials)
            {
                foreach (w3Layer layer in m.Layers)
                {
                    if (layer.Diffuse_Texure_ID.isStatic)
                    {
                        if (layer.Diffuse_Texure_ID.StaticValue[0] == id)
                        {
                            layer.Diffuse_Texure_ID.StaticValue = Textures.Count == 0 ? [-1] : [0];
                        }
                    }
                    else
                    {
                        foreach (w3Keyframe kf in layer.Diffuse_Texure_ID.Keyframes)
                        {
                            if (kf.Data[0] == id)
                            {
                                kf.Data = Textures.Count == 0 ? [-1] : [0];
                            }
                            if (kf.InTan[0] == id)
                            {
                                kf.InTan = Textures.Count == 0 ? [-1] : [0];
                            }
                            if (kf.OutTan[0] == id)
                            {
                                kf.OutTan = Textures.Count == 0 ? [-1] : [0];
                            }
                        }
                    }
                }
            }
        }
        internal void RemoveTextureAnimastionFromLayers(int id)
        {
            foreach (w3Material m in Materials)
            {
                foreach (w3Layer layer in m.Layers)
                {
                    if (layer.Animated_Texture_ID == id) { layer.Animated_Texture_ID = -1; }
                }
            }
        }
        internal void RemoveTargetGeoset(int id)
        {
            Geoset_Animations.RemoveAll(x => x.Geoset_ID == id);
        }
        internal void RemoveKeyframsInRange(int from, int to)
        {
            foreach (w3Transformation t in Transformations)
            {
                foreach (w3Keyframe k in t.Keyframes.ToList())
                {
                    if (k.Track >= from && k.Track <= to)
                    {
                        t.Keyframes.Remove(k);
                    }
                }
            }
        }
        internal void RefreshTransformationsList()
        {

            Transformations.Clear();
            foreach (w3Geoset_Animation ga in Geoset_Animations)
            {
                Transformations.Add(ga.Alpha); ga.Alpha.BelongsTo = $"Geoset animation {ga.ID}";
                Transformations.Add(ga.Color); ga.Color.BelongsTo = $"Geoset animation {ga.ID}";
            }
            foreach (w3Texture_Animation ta in Texture_Animations)
            {
                Transformations.Add(ta.Translation); ta.Translation.BelongsTo = $"Texture animation {ta.ID}";
                Transformations.Add(ta.Rotation); ta.Rotation.BelongsTo = $"Texture animation {ta.ID}";
                Transformations.Add(ta.Scaling); ta.Scaling.BelongsTo = $"Texture animation {ta.ID}";
            }
            foreach (w3Material m in Materials)
            {
                foreach (w3Layer l in m.Layers)
                {
                    Transformations.Add(l.Alpha); l.Alpha.BelongsTo = $"Material {m.ID}, layer {l.ID}";
                    Transformations.Add(l.Diffuse_Texure_ID); l.Diffuse_Texure_ID.BelongsTo = $"Material {m.ID}, layer {l.ID}";
                }
            }
            foreach (w3Camera c in Cameras)
            {
                Transformations.Add(c.Position); c.Position.BelongsTo = $"Camera {c.Name}";
                Transformations.Add(c.Target); c.Target.BelongsTo = $"Camera {c.Name}";
                Transformations.Add(c.Rotation); c.Rotation.BelongsTo = $"Camera {c.Name}";
            }
            foreach (w3Node n in Nodes)
            {
                Transformations.Add(n.Translation); n.Translation.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                Transformations.Add(n.Rotation); n.Rotation.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                Transformations.Add(n.Scaling); n.Scaling.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                if (n.Data is w3Attachment)
                {
                    w3Attachment a = (w3Attachment)n.Data;
                    Transformations.Add(a.Visibility); a.Visibility.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                }
                if (n.Data is w3Light)
                {
                    w3Light a = (w3Light)n.Data;
                    Transformations.Add(a.Intensity); a.Intensity.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Attenuation_End); a.Attenuation_End.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Attenuation_Start); a.Attenuation_Start.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Color); a.Color.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Ambient_Color); a.Ambient_Color.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Ambient_Intensity); a.Ambient_Intensity.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(a.Visibility); a.Visibility.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                }
                if (n.Data is Particle_Emitter_1)
                {
                    Particle_Emitter_1 p = (Particle_Emitter_1)n.Data;
                    Transformations.Add(p.Emission_Rate); p.Emission_Rate.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Life_Span); p.Life_Span.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Initial_Velocity); p.Initial_Velocity.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Gravity); p.Gravity.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Longitude); p.Longitude.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Latitude); p.Latitude.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Visibility); p.Visibility.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                }
                if (n.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 p = (Particle_Emitter_2)n.Data;
                    Transformations.Add(p.Visibility); p.Visibility.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Emission_Rate); p.Emission_Rate.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Speed); p.Speed.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Variation); p.Variation.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Latitude); p.Latitude.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Width); p.Width.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Length); p.Length.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Gravity); p.Gravity.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                }
                if (n.Data is Ribbon_Emitter)
                {
                    Ribbon_Emitter p = (Ribbon_Emitter)n.Data;
                    Transformations.Add(p.Color); p.Color.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Alpha); p.Alpha.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Visibility); p.Visibility.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Height_Below); p.Height_Below.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Height_Above); p.Height_Above.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                    Transformations.Add(p.Texture_Slot); p.Texture_Slot.BelongsTo = $"{n.Data.GetType().Name} \"{n.Name}\"";
                }
            }

        }
        private string CollectPivotPoints()
        {
            List<string> points = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (w3Node n in Nodes)
            {
                string x = n.PivotPoint.X % 1 == 0 ? n.PivotPoint.X.ToString("F0") : n.PivotPoint.X.ToString("F6");
                string y = n.PivotPoint.Y % 1 == 0 ? n.PivotPoint.Y.ToString("F0") : n.PivotPoint.Y.ToString("F6");
                string z = n.PivotPoint.Z % 1 == 0 ? n.PivotPoint.Z.ToString("F0") : n.PivotPoint.Z.ToString("F6");
                points.Add($"   {{ {x}, {y}, {z} }},");
            }
            if (points.Count > 0)
            {
                sb.AppendLine($"\nPivotPoints {points.Count} {{");
                foreach (string s in points)
                {
                    sb.AppendLine(s);
                }
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        internal void Clear()
        {
            Nodes.Clear();
            Textures.Clear();
            Global_Sequences.Clear();
            Sequences.Clear();
            Materials.Clear();
            Cameras.Clear();
            Geosets.Clear();
            Geoset_Animations.Clear();
        }
        internal void FinalizeComponents()
        {

            SetPivotPoints();
            TransferMatrixGroups();
            CreateTextureIfNoteTexturesButMaterials();
            GiveIDs();
            CalculateCollisionShapeEdges();
            CalculateGeosetBoundingBoxes();
            RefreshTransformationsList();
          
            ConvKeyframes();
            
        }

        private void ClampAllKeyframes() { foreach (w3Transformation tr in Transformations) tr.Clamp(); }
             
       public void ConvKeyframes()
        {
           
            ConvertKeyframes();
           
            ClampAllKeyframes();
        }

        private void ConvertKeyframes()
        {

            foreach (w3Transformation tr in Transformations)
            {
                if (tr.Type == TransformationType.Color)
                {
                    foreach (w3Keyframe kf in tr.Keyframes)
                    {
                        kf.Data = Converters.Warcraft3ColorToRGB(kf.Data);
                        kf.InTan = Converters.Warcraft3ColorToRGB(kf.InTan);
                        kf.OutTan = Converters.Warcraft3ColorToRGB(kf.OutTan);
                    }
                }
                if (tr.Type == TransformationType.Alpha)
                {
                    foreach (w3Keyframe kf in tr.Keyframes)
                    {
                        kf.Data = [kf.Data[0] * 100];
                    }
                }
                if (tr.Type == TransformationType.Scaling)
                {
                    foreach (w3Keyframe kf in tr.Keyframes)
                    {
                        kf.Data[0] = kf.Data[0] * 100;
                        kf.Data[1] = kf.Data[1] * 100;
                        kf.Data[2] = kf.Data[2] * 100;

                        kf.InTan[0] = kf.InTan[0] * 100;
                        kf.InTan[1] = kf.InTan[1] * 100;
                        kf.InTan[2] = kf.InTan[2] * 100;
                        kf.OutTan[0] = kf.OutTan[0] * 100;
                        kf.OutTan[1] = kf.OutTan[1] * 100;
                        kf.OutTan[2] = kf.OutTan[2] * 100;
                    }
                }
                if (tr.Type == TransformationType.Rotation)
                {
                    foreach (w3Keyframe kf in tr.Keyframes)
                    {
                        kf.Data = Calculator.Rotation_Quaternion_To_Euler(kf.Data);
                        kf.InTan = Calculator.Rotation_Quaternion_To_Euler(kf.InTan);
                        kf.OutTan = Calculator.Rotation_Quaternion_To_Euler(kf.OutTan);
                    }
                }

            }
            foreach (w3Transformation tr in Transformations)
            {
                if (tr.Type != TransformationType.Rotation)
                {
                    foreach (w3Keyframe k in tr.Keyframes)
                    {
                        if (k.InTan.Length == 4) { k.InTan = [0, 0, 0]; }
                        if (k.OutTan.Length == 4) { k.OutTan = [0, 0, 0]; }
                    }
                }
            }
        }

        
        private void TransferMatrixGroups()
        {
            foreach (w3Geoset geo in Geosets)
            {
                for (int vertexIndex = 0; vertexIndex< geo.VertexGroup.Count; vertexIndex++)
                {
                    int MatrixGroupIndex = geo.VertexGroup[vertexIndex];
                    geo.Vertices[vertexIndex].AttachedTo = geo.MatrixGroups[MatrixGroupIndex].ToList();
                }
                 
            }
            
        }

        internal void RefreshEdges()
        {
            foreach (w3Geoset geo in Geosets) { geo.RecalculateEdges(); }
        }








        internal void SetPivotPoints()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (i < PivotPoints.Count)
                {
                    Nodes[i].PivotPoint = PivotPoints[i];
                }
            }

        }
        internal void FixInvalidNodeRelationships()
        {
            foreach (w3Node node in Nodes)
            {
                // fix self referencing node
                if (node.parentId == node.objectId) { node.parentId = -1; }
                // fix < (-1) negative parent
                if (node.parentId < -1) { node.parentId = -1; }
                // fix invalid parent
                if (node.parentId > -1 && Nodes.Any(x => x.objectId == node.parentId) == false) { node.parentId = -1; }
                // fix mutually refereing node
                if (Nodes.Any(x => x.objectId == node.parentId)) // has parent
                {
                    w3Node parent = Nodes.First(x => x.objectId == node.parentId);
                    if (parent.parentId == node.objectId) { parent.parentId = -1; }
                }
            }
        }
        private void DeleteNonBoneIDs()
        {
            foreach (w3Geoset g in Geosets)
            {
                foreach (w3Vertex v in g.Vertices)
                {
                    foreach (int id in v.AttachedTo.ToList())
                    {
                        // if it doesnt exist
                        if (Nodes.Any(x => x.objectId == id) == false)
                        {
                            v.AttachedTo.Remove(id);
                        }

                        if (Nodes.Any(x => x.objectId == id) == true)
                        {
                            w3Node node = Nodes.First(x => x.objectId == id);
                            if (node.Data is Bone == false)
                            {
                                v.AttachedTo.Remove(id);
                            }

                        }
                    }
                }
            }


        }
        private void HandleFreeVertices()
        {

            int freeCount = 0;
            foreach (w3Geoset g in Geosets)
            {
                foreach (w3Vertex v in g.Vertices.ToList())
                {
                    if (v.AttachedTo.Count == 0)
                    {
                        freeCount++;
                      //  MessageBox.Show("");
                         
                    }
                }

            }
            if (freeCount > 0)
            {
                MessageBoxResult result = MessageBox.Show(
    $"{freeCount} Free vertices were found. Would you like to delete them? If NO, they will be attached to a dummy bone.",
    "Free vertices: action required",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes) // delete
                {
                    foreach (w3Geoset g in Geosets)
                    {
                        foreach (w3Vertex v in g.Vertices.ToList())
                        {
                            if (v.AttachedTo.Count == 0)
                            {
                                int vID = v.Id;
                                DeleteTrianglesUsingVertex(vID);
                                g.Vertices.Remove(v);
                            }
                        }
                    }
                }
                else if (result == MessageBoxResult.No) // attach to dummy bone
                {
                    AttachFreeVerticesToDummy();
                }


            }
        }
        private void AttachFreeVerticesToDummy()
        {
            int DummyID = GetDummyBone();
            foreach (w3Geoset g in Geosets)
            {
                foreach (w3Vertex v in g.Vertices.ToList())
                {
                    if (v.AttachedTo.Count == 0)
                    {
                        v.AttachedTo.Add(DummyID);
                    }
                }
            }
        }

        private int GetDummyBone()
        {
            int boneIndex = 0;

            foreach (w3Node node in Nodes)
            {
                if (node.Name == "Dummy_Bone")
                {
                    return Nodes.IndexOf(node);
                }
            }
            w3Node dummy = new w3Node();
            dummy.Name = "Dummy_Bone";
            dummy.objectId = IDCounter.Next();
            dummy.Data = new Bone();
            Nodes.Add(dummy);
            return dummy.objectId;
        }

        private void DeleteTrianglesUsingVertex(int id)
        {
            foreach (w3Geoset g in Geosets)
            {
                foreach (w3Triangle t in g.Triangles.ToList())
                {
                    if (t.Index1 == id || t.Index2 == id || t.Index3 == id)
                    {
                        g.Triangles.Remove(t);
                    }
                }
            }
        }
        private void CapitalizeAndEnumerateSequences()
        {
            // First loop: Capitalize all sequence names
            foreach (w3Sequence seq in Sequences)
            {
                if (seq.Name.Trim().Length == 0)
                {
                    seq.Name = "Unnamed";
                }
            }

            foreach (w3Sequence seq in Sequences)
            {
                seq.Name = Capitalize(seq.Name);
            }


            // Dictionary to track occurrences of each name
            Dictionary<string, int> nameCount = new Dictionary<string, int>();

            // Second loop: Enumerate sequences with identical names
            foreach (w3Sequence seq in Sequences)
            {
                string capitalizedName = seq.Name;

                // Check if the name has already been seen
                if (nameCount.ContainsKey(capitalizedName))
                {
                    // Increment the count for this name
                    nameCount[capitalizedName]++;
                    // Append the count to make the name distinct
                    seq.Name = $"{capitalizedName} {nameCount[capitalizedName]}";
                }
                else
                {
                    // If it's the first time this name is seen, initialize the count
                    nameCount[capitalizedName] = 1;
                    // No need to append a number for the first occurrence
                }
            }
        }

        private string Capitalize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Capitalize the first letter of every word
            return string.Join(" ", name.Split(' ').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }

        internal void Optimize()
        {
          FixInvalidNodeRelationships();
        DeleteNonBoneIDs();
            HandleFreeVertices();
            CapitalizeAndEnumerateSequences();
            FixOverlappingSequences();
            RearrangeSequences();
            
            RefreshGeosetAnimations();
            RenameIdenticalNodesAndCameras();
            DeleteDuplicateTracksAndRearrangeKeyframes();
            CalculateExtents();
             RecalculateGeosetAnimations();
            DeleteSequencesWithInvalidInterval();
            ClampTextureCoordinates();
           

        }
        private void ClampTextureCoordinates()
        {
            foreach (w3Geoset geo in Geosets)
            {
                foreach (w3Vertex vertex in geo.Vertices)
                {
                    vertex.Texture_Position.Clamp();
                }
            }
        }
       private void DeleteSequencesWithInvalidInterval()
        {
            foreach (w3Sequence seq in Sequences.ToList())
            {
                if (seq.To < 0 && seq.From < 0 || seq.From > seq.To || seq.From > 999999 || seq.To > 999999)
                {
                    MessageBox.Show($"Removed sequence \"{seq}\" because of invalid interval", "Report");
                    Sequences.Remove(seq);
                }
            }
        }
        private void DeleteDuplicateTracksAndRearrangeKeyframes()
        {
          RefreshTransformationsList();
            foreach (w3Transformation t in Transformations)
            {
                t.Keyframes
            .GroupBy(kf => kf.Track)
            .Select(g => g.Last())    // Keep the last keyframe for each track
            .OrderBy(kf => kf.Track)  // Sort by track in ascending order
            .ToList();
            }
        }

        private void RecalculateGeosetAnimations()
        {
            // find all geosets without geoset animation

            List<int> geosetsWithoutGA = new List<int>();
            foreach (w3Geoset geo in Geosets)
            {
                if (Geoset_Animations.Any(x=>x.Geoset_ID == geo.ID) == false)
                {
                    geosetsWithoutGA.Add(geo.ID);
                }
               
            }
            foreach (int id in geosetsWithoutGA)
            {
                w3Geoset_Animation ga = new w3Geoset_Animation();
                ga.ID = IDCounter.Next();
                ga.Geoset_ID = id;
                Geoset_Animations.Add(ga);
            }
            List<w3Geoset_Animation> distinct = Geoset_Animations.DistinctBy(x=>x.Geoset_ID).ToList();
            Geoset_Animations = distinct;


        }

        private void RenameIdenticalNodesAndCameras()
        {
            // Dictionary to keep track of name occurrences for nodes
            Dictionary<string, int> nodeNameCount = new Dictionary<string, int>();

            // Rename nodes
            foreach (w3Node node in Nodes)
            {
                // Check if the name already exists in the dictionary
                if (nodeNameCount.ContainsKey(node.Name))
                {
                    // Increment the count for this name
                    nodeNameCount[node.Name]++;
                    // Rename the node with the count
                    node.Name = $"{node.Name}_{nodeNameCount[node.Name]}";
                }
                else
                {
                    // First occurrence of this name, initialize count
                    nodeNameCount[node.Name] = 1;
                }
            }

            // Dictionary to keep track of name occurrences for cameras
            Dictionary<string, int> cameraNameCount = new Dictionary<string, int>();

            // Rename cameras
            foreach (w3Camera cam in Cameras)
            {
                // Check if the name already exists in the dictionary
                if (cameraNameCount.ContainsKey(cam.Name))
                {
                    // Increment the count for this name
                    cameraNameCount[cam.Name]++;
                    // Rename the camera with the count
                    cam.Name = $"{cam.Name}_{cameraNameCount[cam.Name]}";
                }
                else
                {
                    // First occurrence of this name, initialize count
                    cameraNameCount[cam.Name] = 1;
                }
            }
        }

        public void RefreshGeosetAnimations()
        { 
           foreach (w3Geoset geo in Geosets)
            {
                if (Geoset_Animations.Any(X=>X.Geoset_ID == geo.ID) == false)
                {
                    Geoset_Animations.Add(new w3Geoset_Animation(geo.ID));
                }
            }
        }
        private void FixOverlappingSequences()
        {
            // Phase 1: Find overlapping sequences and store them in a list
            List<w3Sequence> overlappingSequences = new List<w3Sequence>();

            foreach (w3Sequence seq1 in Sequences)
            {
                foreach (w3Sequence seq2 in Sequences)
                {
                    // Skip if they are the same sequence
                    if (seq1 == seq2) continue;

                    // Check for overlapping intervals
                    if (seq1.From < seq2.To && seq1.To > seq2.From)
                    {
                        // Add to the list if not already there
                        if (!overlappingSequences.Contains(seq1))
                            overlappingSequences.Add(seq1);

                        if (!overlappingSequences.Contains(seq2))
                            overlappingSequences.Add(seq2);
                    }
                }
            }

            // Phase 2: Adjust overlapping sequences by finding unused intervals
            foreach (w3Sequence overlappingSeq in overlappingSequences)
            {
                // Get the interval range of the current sequence
                int range = overlappingSeq.To - overlappingSeq.From;

                // Find the latest 'To' value used by any sequence
                int maxUsedTo = Sequences.Max(seq => seq.To);

                // Set the new interval starting after the last 'To'
                overlappingSeq.From = maxUsedTo + 1;
                overlappingSeq.To = overlappingSeq.From + range;

                // Optionally log the adjustment
                Console.WriteLine($"Adjusted sequence '{overlappingSeq.Name}' to new interval: {overlappingSeq.From} - {overlappingSeq.To}");
            }
        }

        private void RearrangeSequences()
        {
            Sequences = Sequences.OrderBy(x=>x.From).ToList();
        }
        private void RearrangeKeyframes()
        {
            foreach (w3Node node in Nodes)
            {
              node.Translation.Keyframes=  node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
              node.Rotation.Keyframes=  node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
             node.Scaling.Keyframes=   node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();
            }
            foreach (w3Geoset_Animation ga  in Geoset_Animations)
            {
                ga.Alpha.Keyframes = ga.Alpha.Keyframes.OrderBy(x => x.Track).ToList();
                ga.Color.Keyframes = ga.Alpha.Keyframes.OrderBy(x => x.Track).ToList();
            }
        }

        internal w3Model CloneAnimated()
        {
            w3Model model = new w3Model();
            model.Sequences = Sequences;
            model.Materials = Materials;
           model.Global_Sequences = Global_Sequences;
             foreach (w3Geoset geo in Geosets) {  model.Geosets.Add(geo.Clone()); }
            model.Geoset_Animations = Geoset_Animations;

            return model;

            throw new NotImplementedException();
        }
        #endregion
    }
    #region ModelTypes
    internal static class NodeType
    {
        internal static readonly string Bone = typeof(Bone).Name;
        internal static readonly string Attachment = typeof(w3Attachment).Name;
        internal static readonly string Collision_Shape = typeof(Collision_Shape).Name;
        internal static readonly string Emitter1 = typeof(Particle_Emitter_1).Name;
        internal static readonly string Emitter2 = typeof(Particle_Emitter_2).Name;
        internal static readonly string Event = typeof(Event_Object).Name;
        internal static readonly string Light = typeof(w3Light).Name;
        internal static readonly string Helper = typeof(Helper).Name;
        internal static readonly string Ribbon = typeof(Ribbon_Emitter).Name;
    }
    public class w3Node
    {// any of the nodes can be put here
        internal bool isSelected = false;

        internal object Data { get; set; } // whatever type of node it is
        internal int objectId { get; set; } = -1;
        internal int parentId { get; set; } = -1;
        internal Coordinate PivotPoint { get; set; } = new Coordinate(0, 0, 0);
        internal string Name { get; set; }
        internal w3Transformation Translation { get; set; } = new w3Transformation(true, [], TransformationType.Translation, -1, 0, new List<w3Keyframe>()) { isStatic = false};
        internal w3Transformation Rotation { get; set; } = new w3Transformation(true, [], TransformationType.Rotation, -1, 0, new List<w3Keyframe>()) { isStatic = false };
        internal w3Transformation Scaling { get; set; } = new w3Transformation(true, [], TransformationType.Scaling, -1, 0, new List<w3Keyframe>()) { isStatic = false };
        //tags
        internal bool Inherits_Translation = true;
        internal bool Inherits_Rotation = true;
        internal bool Inherits_Scaling = true;
        internal bool Billboarded = false;
        internal bool Billboarded_Lock_X = false;
        internal bool Billboarded_Lock_Y = false;
        internal bool Billboarded_Lock_Z = false;
        internal bool Camera_Anchored = false;
        // internal Node_Animation_Properties AnimationProperties { get; set; } // try in the future to implement this

        internal w3Node(string name, string type)
        {
            Name = name;
            if (type == NodeType.Bone) Data = new Bone();
            if (type == NodeType.Attachment) Data = new w3Attachment();
            if (type == NodeType.Collision_Shape) Data = new Collision_Shape();
            if (type == NodeType.Helper) Data = new Helper();
            if (type == NodeType.Emitter1) Data = new Particle_Emitter_1();
            if (type == NodeType.Emitter2) Data = new Particle_Emitter_2();
            if (type == NodeType.Ribbon) Data = new Ribbon_Emitter();
            if (type == NodeType.Event) Data = new Event_Object();
            if (type == NodeType.Light) Data = new w3Light();
            objectId = IDCounter.Next();
        }
        internal w3Node()
        {
        }
        private bool IsValidDataType(object data)
        {
            return
               nameof(data) == NodeType.Bone ||
                nameof(data) == NodeType.Attachment ||
                nameof(data) == NodeType.Event ||
               nameof(data) == NodeType.Helper ||
               nameof(data) == NodeType.Collision_Shape ||
               nameof(data) == NodeType.Emitter1 ||
               nameof(data) == NodeType.Emitter2 ||
               nameof(data) == NodeType.Ribbon ||
               nameof(data) == NodeType.Light;
        }

        internal string ToMDL(int index, List<w3Node> nodes, List<w3Geoset> geosets, List<w3Geoset_Animation> gas)
        {
            StringBuilder result = new();

            result.AppendLine($"{MDLHelper.NodeNameRef[Data.GetType().Name]} \"{Name}\" {{");
            //------------------------------------------------ ids
            result.AppendLine($"\tObjectId {index},");
            if (parentId >= 0)
            {
                int indexof = nodes.FindIndex(x => x.objectId == parentId);
                result.AppendLine($"\tParent {indexof},");
            }
            //----------------------------------------------------------

            if (Data is Bone)
            {
                Bone b = (Bone)Data;
                string gsid = b.Geoset_ID < 0 ? "Multiple" : geosets.FindIndex(x => x.ID == b.Geoset_ID).ToString();
                result.AppendLine($"\tGeosetId {gsid},");
                 string gsaid = b.Geoset_ID < 0 ? "None" : gas.FindIndex(x => x.ID == b.Geoset_Animation_ID).ToString();
                result.AppendLine($"\tGeosetAnimId {gsaid},");
            }
            if (Data is w3Attachment)
            {
                w3Attachment b = (w3Attachment)Data;
                result.AppendLine($"\tAttachmentID {index},");
                if (b.Path.Length > 0)
                {
                    result.AppendLine($"\tPath \"{b.Path}\",");
                }
                result.AppendLine(b.Visibility.Keyframes.Count > 0 ? b.Visibility.ToMDL() : string.Empty);
                //----------------------------------------------------------
            }
            if (Data is Event_Object)
            {
                Event_Object e = (Event_Object)Data;
                if (e.Tracks.Count > 0)
                {
                    result.AppendLine($"EventTrack {e.Tracks.Count} {{");
                    result.AppendLine(string.Join(",\n", e.Tracks) + ",\n");
                    result.AppendLine($"}}");
                }

            }
            if (Data is Collision_Shape)
            {
                Collision_Shape cs = (Collision_Shape)Data;


                if (cs.Type == CollisionShapeType.Sphere)
                {
                    result.AppendLine("Sphere,");
                    result.AppendLine("Vertices 1 {");
                    result.AppendLine($"{{ {cs.Extents.Minimum_X}, {cs.Extents.Minimum_Y}, {cs.Extents.Minimum_Z} }},");
                    result.AppendLine("}");
                    if (cs.Extents.Bounds_Radius > 0)
                    {
                        result.AppendLine($"BoundsRadius {cs.Extents.Bounds_Radius},");
                    }
                }
                if (cs.Type == CollisionShapeType.Box)
                {   
                    result.AppendLine("Box,");
                    result.AppendLine("Vertices 2 {");
                    result.AppendLine($"{{ {cs.Extents.Minimum_X}, {cs.Extents.Minimum_Y}, {cs.Extents.Minimum_Z} }},");
                    result.AppendLine($"{{ {cs.Extents.Maximum_X}, {cs.Extents.Maximum_Y}, {cs.Extents.Maximum_Z} }},");
                    result.AppendLine("}");
                    
                }
            }
            if (Data is Ribbon_Emitter)
            {
                Ribbon_Emitter r = (Ribbon_Emitter)Data;
                if (r.Color.isStatic) { result.AppendLine($"static Color {r.Color.StaticValue[0]:f0},"); }
                else { result.AppendLine(r.Color.ToMDL("Color")); }
                if (r.Alpha.isStatic) { result.AppendLine($"static Alpha {string.Join(", ", r.Color.StaticValue)},"); }
                else { result.AppendLine(r.Alpha.ToMDL("Alpha")); }
                if (r.Visibility.isStatic) { result.AppendLine($"static Visibility {r.Visibility.StaticValue},"); }
                else { result.AppendLine(r.Visibility.ToMDL("Visibility")); }
                if (r.Height_Above.isStatic) { result.AppendLine($"static HeightAbove {r.Height_Above.StaticValue},"); }
                else { result.AppendLine(r.Height_Above.ToMDL("HeightAbove")); }
                if (r.Height_Below.isStatic) { result.AppendLine($"static HeightBelow {r.Height_Below.StaticValue},"); }
                else { result.AppendLine(r.Height_Below.ToMDL("HeightBelow")); }
                if (r.Texture_Slot.isStatic) { result.AppendLine($"static TextureSlot {r.Texture_Slot.StaticValue},"); }
                else { result.AppendLine(r.Texture_Slot.ToMDL("TextureSlot")); }
                result.AppendLine($"EmissionRate {r.Emission_Rate},");
                result.AppendLine($"LifeSpan {r.Life_Span},");
                result.AppendLine($"Rows {r.Rows},");
                result.AppendLine($"Columns {r.Columns},");
                result.AppendLine($"MaterialID {r.Material_ID},");
            }
            if (Data is w3Light)
            {
                w3Light cs = (w3Light)Data;

                result.AppendLine(cs.Type.ToString() + ",");
                if (cs.Attenuation_Start.isStatic) { result.AppendLine($"static AttenuationStart {cs.Attenuation_Start.StaticValue[0]:f0},"); }
                else { result.AppendLine(cs.Attenuation_Start.ToMDL("AttenuationStart")); }
                if (cs.Attenuation_End.isStatic) { result.AppendLine($"static AttenuationEnd {cs.Attenuation_End.StaticValue[0]:f0},"); }
                else { result.AppendLine(cs.Attenuation_End.ToMDL("AttenuationEnd")); }
                if (cs.Color.isStatic) { result.AppendLine($"static Color {cs.Color.StaticValue},"); }
                else { result.AppendLine(cs.Color.ToMDL("Color")); }
                if (cs.Ambient_Color.isStatic) { result.AppendLine($"static AmbColor {cs.Ambient_Color.StaticValue},"); }
                else { result.AppendLine(cs.Ambient_Color.ToMDL("AmbColor")); }
                if (cs.Intensity.isStatic) { result.AppendLine($"static Intensity {cs.Intensity.StaticValue[0]:f0},"); }
                else { result.AppendLine(cs.Intensity.ToMDL("Intensity")); }
                if (cs.Ambient_Intensity.isStatic) { result.AppendLine($"static AmbIntensity {cs.Ambient_Intensity.StaticValue[0]:f0},"); }
                else { result.AppendLine(cs.Ambient_Intensity.ToMDL("AmbIntensity")); }
                if (cs.Visibility.isStatic) { result.AppendLine($"static Visibility {cs.Visibility.StaticValue[0]:f0},"); }
                else { result.AppendLine(cs.Visibility.ToMDL("Visibility")); }
            }

            if (Data is Particle_Emitter_1)
            {
                Particle_Emitter_1 pe = (Particle_Emitter_1)Data;

                if (pe.Emitter_Uses_MDL) result.AppendLine("EmitterUsesMDL,");
                if (pe.Emitter_Uses_TGA) result.AppendLine("EmitterUsesTGA,");
                if (pe.Emission_Rate.isStatic) { result.AppendLine($"static EmissionRate {pe.Emission_Rate.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Emission_Rate.ToMDL("EmissionRate")); }
                if (pe.Gravity.isStatic) { result.AppendLine($"static Gravity {pe.Gravity.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Gravity.ToMDL("Gravity")); }
                if (pe.Longitude.isStatic) { result.AppendLine($"static Longitude {pe.Longitude.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Longitude.ToMDL("Longitude")); }
                if (pe.Latitude.isStatic) { result.AppendLine($"static Latitude {pe.Latitude.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Latitude.ToMDL("Latitude")); }
                if (pe.Visibility.isStatic) { result.AppendLine($"static Visibility {pe.Visibility.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Visibility.ToMDL("Visibility")); }
                result.AppendLine("Particle {");
                if (pe.Life_Span.isStatic) { result.AppendLine($"static LifeSpan {pe.Life_Span.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Life_Span.ToMDL("LifeSpan")); }
                if (pe.Initial_Velocity.isStatic) { result.AppendLine($"static InitVelocity {pe.Initial_Velocity.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe.Initial_Velocity.ToMDL("InitVelocity")); }
                result.AppendLine($"Path \"{pe.Particle_Filename}\",");
                result.AppendLine("}");

            }
            if (Data is Particle_Emitter_2)
            {

                Particle_Emitter_2 pe2 = Data as Particle_Emitter_2;

                if (pe2.Filter_Mode != FilterMode.None)
                {
                    result.AppendLine(" " + pe2.Filter_Mode.ToString() + ",");
                }

                if (pe2.Speed.isStatic) { result.AppendLine($"static Speed {pe2.Speed.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Speed.ToMDL("Speed")); }
                if (pe2.Variation.isStatic) { result.AppendLine($"static Variation {pe2.Variation.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Variation.ToMDL("Variation")); }
                if (pe2.Latitude.isStatic) { result.AppendLine($"static Latitude {pe2.Latitude.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Latitude.ToMDL("Latitude")); }
                if (pe2.Gravity.isStatic) { result.AppendLine($"static Gravity {pe2.Gravity.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Gravity.ToMDL("Gravity")); }
                if (pe2.Emission_Rate.isStatic) { result.AppendLine($"static EmissionRate {pe2.Emission_Rate.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Emission_Rate.ToMDL("EmissionRate")); }
                if (pe2.Width.isStatic) { result.AppendLine($"static Width {pe2.Width.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Width.ToMDL("Width")); }
                if (pe2.Length.isStatic) { result.AppendLine($"static Length {pe2.Length.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Length.ToMDL("Length")); }
                if (pe2.Visibility.isStatic) { result.AppendLine($"static Visibility {pe2.Visibility.StaticValue[0]:f0},"); }
                else { result.AppendLine(pe2.Visibility.ToMDL("Visibility")); }
                result.AppendLine("SegmentColor {");
                result.AppendLine($"Color {{ {pe2.Color_Segment1[0]}, {pe2.Color_Segment1[1]}, {pe2.Color_Segment1[2]} }},");
                result.AppendLine($"Color {{ {pe2.Color_Segment2[0]}, {pe2.Color_Segment2[1]}, {pe2.Color_Segment2[2]} }},");
                result.AppendLine($"Color {{ {pe2.Color_Segment3[0]},{pe2.Color_Segment3[1]},{pe2.Color_Segment3[2]} }},");

                result.AppendLine("},");
                result.AppendLine($"ParticleScaling {{ {pe2.Scaling_Segment1}, {pe2.Scaling_Segment2}, {pe2.Scaling_Segment3} }},");
                 



                result.AppendLine($"Alpha {{ {pe2.ALpha_Segment1}, {pe2.ALpha_Segment2}, {pe2.ALpha_Segment3} }},");
                result.AppendLine($"LifeSpanUVAnim {{ {pe2.Head_Lifespan_Start}, {pe2.Head_Lifespan_End}, {pe2.Head_Lifespan_Repeat} }},");
                result.AppendLine($"DecayUVAnim {{ {pe2.Head_Decay_Start}, {pe2.Head_Decay_End}, {pe2.Head_Decay_Repeat} }},");
                result.AppendLine($"TailUVAnim {{ {pe2.Tail_Lifespan_Start}, {pe2.Tail_Lifespan_End}, {pe2.Tail_Lifespan_Repeat} }},");
                result.AppendLine($"TailDecayUVAnim {{ {pe2.Tail_Decay_Start}, {pe2.Tail_Decay_End}, {pe2.Tail_Decay_End} }},");
                result.AppendLine($"Rows {pe2.Rows},");
                result.AppendLine($"Columns {pe2.Columns},");
                result.AppendLine($"LifeSpan {pe2.Life_Span},");
                result.AppendLine($"TailLength {pe2.Tail_Length},");
                if (pe2.Sort_Primitives_Far_Z) result.AppendLine($"SortPrimsFarZ,");
                if (pe2.Line_Emitter) result.AppendLine($"LineEmitter,");
                if (pe2.Model_Space) result.AppendLine($"ModelSpace,");
                if (pe2.AlphaKey) result.AppendLine($"AlphaKey,");
                if (pe2.Unshaded) result.AppendLine($"Unshaded,");
                if (pe2.Unfogged) result.AppendLine($"Unfogged,");
                if (pe2.XY_Quad) result.AppendLine($"XYQuad,");
                if (pe2.Squirt) result.AppendLine($"Squirt,");
                result.AppendLine($"Time {pe2.Time},");
                int txid = ModelHelper.Current.Textures.FindIndex(c => c.ID == pe2.Texture_ID);
                result.AppendLine($"TextureID {txid},");
                if (pe2.Head && pe2.Tail) result.AppendLine($"Both,");
                else if (pe2.Head && pe2.Tail == false) result.AppendLine($"Head,");
                else if (pe2.Head == false && pe2.Tail == true) result.AppendLine($"Tail,");

            }
            if (Translation.Keyframes.Count > 0) result.AppendLine(Translation.ToMDL());
            if (Rotation.Keyframes.Count > 0) result.AppendLine(Rotation.ToMDL());
            if (Scaling.Keyframes.Count > 0) result.AppendLine(Scaling.ToMDL());
            result.AppendLine("}");
            return result.ToString();
        }

        internal w3Node Clone()
        {
            w3Node n = new w3Node();
            int newid = IDCounter.Next();
            n.objectId = newid;
            n.Name = Name + "_Duplicated_" + newid.ToString();
            n.PivotPoint = PivotPoint.Clone();
            n.Rotation = Rotation.Clone();
            n.Scaling = Scaling.Clone();
            n.Translation = Translation.Clone();
            n.Inherits_Translation = Inherits_Translation;
            n.Inherits_Rotation = Inherits_Rotation;
            n.Inherits_Scaling = Inherits_Scaling;
            n.Camera_Anchored = Camera_Anchored;
            n.Billboarded = Billboarded;
            n.Billboarded_Lock_X = Billboarded_Lock_X;
            n.Billboarded_Lock_Y = Billboarded_Lock_Y;
            n.Billboarded_Lock_Z = Billboarded_Lock_Z;
            if ( Data is Bone)
            {
                Bone dat = Data as Bone;
                Bone b = new Bone();
                b.Geoset_Animation_ID = dat.Geoset_Animation_ID;
                b.Geoset_ID = dat.Geoset_ID;
                n.Data = b;
            }
            if (Data is w3Attachment)
            {
                w3Attachment dat = Data as w3Attachment;
                w3Attachment b = new w3Attachment();
                b.Path = dat.Path;
                b.Visibility = dat.Visibility.Clone();
                n.Data = b;
            }
            if (Data is Helper) { n.Data = new Helper(); }
            if (Data is Event_Object)
            {

                Event_Object dat = Data as Event_Object;
                Event_Object b = new Event_Object();
               
                b.identifier = dat.Identifier;
                b.Global_sequence_ID = dat.Global_sequence_ID;
                b.Tracks = dat.Tracks.ToList();
                b.Data = dat.Data;
                b.Type = dat.Type;
                n.Data = b;


            }
            if (Data is Collision_Shape) {
                Collision_Shape dat = Data as Collision_Shape;
                Collision_Shape b = new Collision_Shape();
                b.Extents = dat.Extents.Clone();
                b.Type = dat.Type;
                n.Data = b;

            }
            if (Data is Particle_Emitter_1)
            {
                Particle_Emitter_1 dat = Data as Particle_Emitter_1;
                Particle_Emitter_1 b = new Particle_Emitter_1();
                b.Visibility = dat.Visibility.Clone();
                b.Latitude = dat.Latitude.Clone();
                b.Longitude = dat.Longitude.Clone();
                b.Initial_Velocity = dat.Initial_Velocity.Clone();
                b.Emission_Rate = dat.Emission_Rate.Clone();
                b.Emitter_Uses_MDL = dat.Emitter_Uses_MDL;
                b.Emitter_Uses_TGA = dat.Emitter_Uses_TGA;
                b.Gravity = dat.Gravity.Clone();
                b.Life_Span = dat.Life_Span.Clone();
                b.Particle_Filename = dat.Particle_Filename;
                n.Data = b;

            }
            if (Data is Ribbon_Emitter)
            {
                Ribbon_Emitter dat = Data as Ribbon_Emitter;
                Ribbon_Emitter b = new Ribbon_Emitter();

                b.Visibility = dat.Visibility.Clone();
                b.Alpha = dat.Alpha.Clone();
                b.Columns = dat.Columns;
                b.Rows = dat.Rows;
                b.Color = dat.Color.Clone();
                b.Material_ID = dat.Material_ID;
                b.Emission_Rate = dat.Emission_Rate;
                b.Gravity = dat.Gravity;
                b.Height_Above = dat.Height_Above.Clone();  
                b.Height_Below = dat.Height_Below.Clone();
                b.Life_Span = dat.Life_Span;
                b.Texture_Slot = dat.Texture_Slot.Clone();
                n.Data = b;

            }
            if (Data is w3Light)
            {
                w3Light dat = Data as w3Light;
                w3Light b = new w3Light();

                b.Attenuation_Start = dat.Attenuation_Start.Clone();
                b.Attenuation_End = dat.Attenuation_End.Clone();
                b.Ambient_Intensity = dat.Ambient_Intensity.Clone();    
                b.Ambient_Color = dat.Ambient_Color.Clone();
                b.Visibility = dat.Visibility.Clone();
                b.Color = b.Color.Clone();
                b.Type = dat.Type;
                n.Data = b;

            }
            if (Data is Particle_Emitter_2)
            {
                Particle_Emitter_2 dat = Data as Particle_Emitter_2;
                Particle_Emitter_2 b = new Particle_Emitter_2();
                b.Visibility = dat.Visibility.Clone();
                b.Width = dat.Width.Clone();
                b.Length = dat.Length.Clone();
                b.Latitude = dat.Latitude.Clone();
                b.Speed = dat.Speed.Clone();
                b.Variation = dat.Variation.Clone();
                b.Emission_Rate = dat.Emission_Rate.Clone();
                b.Gravity = dat.Gravity.Clone();    
                b.Life_Span = dat.Life_Span;
                b.Time = dat.Time;
                b.Rows = dat.Rows;
                b.Columns = dat.Columns;
                b.Tail_Length = dat.Tail_Length;
                b.Priority_Plane = dat.Priority_Plane;
                b.Texture_ID = dat.Texture_ID;
                b.Filter_Mode = dat.Filter_Mode;
                b.Unfogged = dat.Unfogged;
                b.Unshaded = dat.Unshaded;
                b.AlphaKey = dat.AlphaKey;
                b.Line_Emitter = dat.Line_Emitter;
                b.Sort_Primitives_Far_Z= dat.Sort_Primitives_Far_Z;
                    b.Model_Space = dat.Model_Space;
                b.XY_Quad = dat.XY_Quad;
                b.Squirt = dat.Squirt;
                b.Head = dat.Head;
                b.Tail  = dat.Tail;
                b.Color_Segment1 = dat.Color_Segment1;
                b.Color_Segment2 = dat.Color_Segment2;
                b.Color_Segment3 = dat. Color_Segment3;
                b.ALpha_Segment1 = dat.ALpha_Segment1;
                b.ALpha_Segment2    = dat.ALpha_Segment2;
                b.ALpha_Segment3 = dat.ALpha_Segment3;
                b.Scaling_Segment1 = dat.Scaling_Segment1;
                b.Scaling_Segment2 = dat.Scaling_Segment2;
                b.Scaling_Segment3 = dat.Scaling_Segment3;
                b.Head_Lifespan_Start = dat.Head_Lifespan_Start;
                b.Head_Lifespan_Repeat = dat.Head_Lifespan_Repeat;
                b.Head_Lifespan_End = dat.Head_Lifespan_End;
                b.Head_Decay_End = dat.Head_Decay_End;
                b.Head_Decay_Repeat = dat.Head_Decay_Repeat;
                b.Head_Decay_Repeat= dat.Head_Decay_Repeat;
                b.Tail_Lifespan_End= dat.Tail_Lifespan_End;
                b.Tail_Lifespan_Repeat= dat.Tail_Lifespan_Repeat;
                b.Tail_Lifespan_Start= dat.Tail_Lifespan_Start;
                b.Tail_Decay_End = dat.Tail_Decay_End;
                b.Tail_Decay_Repeat= dat.Tail_Decay_Repeat ;
                b.Tail_Decay_Start = dat.Tail_Decay_Start;




                n.Data = b;
            }
            return n;
             
        }

        internal void Change(float x, float y, float z)
        {
            PivotPoint.X += x;
            PivotPoint.Y += y;
            PivotPoint.Z += z;
        }
    }
    internal class w3Attachment
    {
        internal string Path { get; set; } = string.Empty;
        internal w3Transformation Visibility { get; set; } = new w3Transformation(TransformationType.Visibility);
        internal w3Attachment(string path, List<w3Keyframe> keyframes)
        {
            Path = path;
            Visibility.Keyframes = keyframes.ToList();
        }
        internal w3Attachment()
        {
        }
    }
    public class w3Global_Sequence
    {
        internal int Duration { get; set; } = 0;
        internal int ID { get; set; } = -1;
        internal w3Global_Sequence(int duration) { Duration = duration; }
        internal w3Global_Sequence(int duration, int id) { Duration = duration; ID = id; }
        internal w3Global_Sequence() { }
        public override string ToString()
        {
            return $"{ID}: {Duration}";
        }
        internal w3Global_Sequence Clone()
        {
            return new w3Global_Sequence(Duration, ID);
        }
        internal void Debug()
        {
            Clipboard.SetText($"Global sequence {ID}: Duration: {Duration}");
        }
    }
    internal class w3Camera
    {
        internal string Name { get; set; } = "";
        internal w3Transformation Target { get; set; } = new w3Transformation(TransformationType.Translation);
        internal w3Transformation Position { get; set; } = new w3Transformation(TransformationType.Translation);
        internal w3Transformation Rotation { get; set; } = new w3Transformation(TransformationType.Int);
        internal double Field_Of_View { get; set; } = 0;
        internal double Near_Distance { get; set; } = 0;
        internal double Far_Distance { get; set; } = 0;
        internal w3Camera(string name) { Name = name; }
        internal w3Camera() { }
        internal w3Camera Clone()
        {
            w3Camera cam = new w3Camera();
            cam.Far_Distance = Far_Distance;
            cam.Near_Distance = Near_Distance;
            cam.Field_Of_View = Field_Of_View;
            cam.Target = Target.Clone();
            cam.Position = Position.Clone();
            cam.Rotation = Rotation.Clone();
            cam.Name = Name;
            return cam;
        }

        internal void Debug()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Target:\n {Target.ToString()}");
            sb.AppendLine($"Position:\n {Target.ToString()}");
            if (Rotation.isStatic == false) { sb.AppendLine($"Rotation:\n {Target.StaticValue}"); }
            sb.AppendLine($"Field of view: {Field_Of_View}");
            sb.AppendLine($"Near distance: {Near_Distance}");
            sb.AppendLine($"Far distance: {Far_Distance}");
            Clipboard.SetText(sb.ToString());
        }

        internal string toMDL()
        {
            StringBuilder sb = new StringBuilder();
            string position = string.Join(", ", Position.StaticValue);
            sb.AppendLine($"\t\tPosition {{{position}}},");
            if (Position.isStatic == false) { sb.AppendLine(Position.ToMDL()); }
            if (Rotation.isStatic == false) { sb.AppendLine(Rotation.ToMDL()); }
            sb.AppendLine($"\tFieldOfView {Field_Of_View:f7},");
            sb.AppendLine($"\tFarClip {Far_Distance},");
            sb.AppendLine($"\tNearClip {Near_Distance},");
            sb.AppendLine($"\tTarget {{");
            string target = string.Join(", ", Target.StaticValue);
            sb.AppendLine($"\t\tPosition {{{target}}},");
            if (Target.isStatic == false) { sb.AppendLine(Target.ToMDL()); }
            sb.AppendLine($"}}");
            return sb.ToString();
        }
    }
    public class w3Sequence
    {
        internal string Name { get; set; } = string.Empty;
        internal int From { get; set; } = 0; // or Start
        internal int To { get; set; } = 0; // or End
        internal double Rarity { get; set; } = 0;
        internal double Move_Speed { get; set; } = 0;
        internal bool Looping { get; set; } = true;
        internal Extent? Extent { get; set; } = new Extent();
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Name: {Name}");
            stringBuilder.AppendLine($"From: {From}");
            stringBuilder.AppendLine($"To: {To}");
            stringBuilder.AppendLine($"Rarity: {Rarity}");
            stringBuilder.AppendLine($"Movespeed: {Move_Speed}");
            stringBuilder.AppendLine($"Looping: {Looping}");
            stringBuilder.AppendLine($"Extents: {Extent.ToString()}");
            return stringBuilder.ToString();
        }
        internal w3Sequence()
        {
            
        }
        internal w3Sequence(string name)
        {
             
            Name = name;
             
        }
        public int Interval { get { if (From >= To) { return 0; } return To - From; }  }
       
        internal w3Sequence(string name, int from, int to, double rarity, double move_Speed,
                   bool looping)
        {
            Name = name;
            From = from;
            To = to;
            Rarity = rarity;
            Move_Speed = move_Speed;
            Looping = looping;
            
        }

        internal w3Sequence Clone()
        {
            return new w3Sequence(Name, From, To, Rarity, Move_Speed, Looping);
        }
        internal void Debug()
        {
            Clipboard.SetText(ToString());
        }

        internal string ToMDL()
        {
            StringBuilder result = new();
            result.AppendLine($"\n\tAnim \"{Name}\" {{");
            result.AppendLine($"\t\tInterval {{ {From},{To} }},");
            if (Rarity > 0) { result.AppendLine($"\t\tRarity {Rarity},"); }
            if (Move_Speed > 0) { result.AppendLine($"\t\tMoveSpeed {Move_Speed},"); }
            if (!Looping) { result.AppendLine($"\t\tNonLooping,"); }
            if (!Extent.minimumExtentsAreZero()) { result.AppendLine($"\t\tMinimumExtent {{ {Extent.Minimum_X},{Extent.Minimum_Y},{Extent.Minimum_Z} }},"); }
            if (!Extent.maximumExtentsAreZero()) { result.AppendLine($"\t\tMaximumExtent {{ {Extent.Maximum_X},{Extent.Maximum_Y},{Extent.Maximum_Z} }},"); }
            if (Extent.Bounds_Radius > 0) { result.AppendLine($"\t\tBoundsRadius {Extent.Bounds_Radius},"); }
            result.AppendLine($"\t}}");
            return result.ToString();
        }
    }
    internal class w3Material
    {
        internal int ID { get; set; } = -1;// app-only. to be referenced easier without additional checks
        internal int Priority_Plane { get; set; }
        internal bool Sort_Primitives_Far_Z { get; set; }
        internal bool Constant_Color { get; set; }
        internal bool Full_Resolution { get; set; }
        internal List<w3Layer> Layers { get; set; }
        internal w3Material()
        {
            Layers = new List<w3Layer>();
        }
        internal w3Material(int id)
        {
            Layers = new List<w3Layer>();
            ID = id;
            Priority_Plane = 0;
            Sort_Primitives_Far_Z = false;
            Constant_Color = false;
            Full_Resolution = false;
        }
        internal w3Material(int priority_Plane, bool sort_Primitives_Far_Z, bool constant_Color,
                    bool full_Resolution, List<w3Layer> layers)
        {
            Priority_Plane = priority_Plane;
            Sort_Primitives_Far_Z = sort_Primitives_Far_Z;
            Constant_Color = constant_Color;
            Full_Resolution = full_Resolution;
            Layers = layers;
        }

        internal w3Material Clone()
        {
            w3Material m = new w3Material();
            m.Priority_Plane = Priority_Plane;
            m.Full_Resolution = Full_Resolution;
            m.Sort_Primitives_Far_Z = Sort_Primitives_Far_Z;
            m.ID = ID;
            m.Constant_Color = Constant_Color;
            foreach (w3Layer r in Layers)
            {
                m.Layers.Add(r.Clone());
            }
            return m;
        }
        internal void Debug(bool window)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine($"Layers: {Layers.Count}");
            b.AppendLine($"Priority plane: {Priority_Plane}");
            b.AppendLine($"Sort primitives far Z: {Sort_Primitives_Far_Z}");
            b.AppendLine($"Constant Color: {Constant_Color}");
            b.AppendLine($"Full Resolution: {Full_Resolution}");
            foreach (w3Layer r in Layers)
            {
                b.AppendLine($"Layer:");
                b.AppendLine($"\tUnshaded: {r.Unshaded}");
                b.AppendLine($"\tUnfogged: {r.Unfogged}");
                b.AppendLine($"\tTwo_Sided: {r.Two_Sided}");
                b.AppendLine($"\tSphere_Environment_Map: {r.Sphere_Environment_Map}");
                b.AppendLine($"\tNo_Depth_Test: {r.No_Depth_Test}");
                b.AppendLine($"\tNo_Depth_Set: {r.No_Depth_Set}");
                b.AppendLine($"\tFilter mode: {r.Filter_Mode.ToString()}");
                b.AppendLine($"\tAnimated texture id: {r.Animated_Texture_ID}");
                b.AppendLine($"\tAlpha: {r.Alpha.ToString()}");
                b.AppendLine($"\tTexture ID: {r.Diffuse_Texure_ID.ToString()}");
            }
            if (window)
            {
                MessageBox.Show(b.ToString());
            }
            else
            {
                Clipboard.SetText(b.ToString());
            }
        }

        internal string ToMDL()
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;
            sb.AppendLine("\tMaterial { ");
            if (Constant_Color) { sb.AppendLine("\t\tConstantColor,"); }
            if (Full_Resolution) { sb.AppendLine("\t\tFullResolution,"); }
            if (Sort_Primitives_Far_Z) { sb.AppendLine("\t\tSortPrimsFarZ,"); }
            if (Priority_Plane > 0) { sb.AppendLine($"\t\tPriorityPlane {Priority_Plane},"); }
            foreach (w3Layer r in Layers)
            {
                sb.AppendLine(r.ToMDL());
            }
            sb.AppendLine("\t}");
            return sb.ToString();
        }
    }
    internal class SkinWeight
    { //unused
        internal int ID { get; set; } = -1;
        internal List<int> Data { get; set; } = new List<int>(); 
        internal SkinWeight(params int[] values)
        {
            Data = values.ToList();
        }
        internal SkinWeight() { }
        // only in Reforged
    }
    public class IntListEqualityComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int> x, List<int> y)
        {
            if (x == null || y == null)
                return false;
            if (ReferenceEquals(x, y))
                return true;
            return x.SequenceEqual(y);
        }
        public int GetHashCode(List<int> obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (var i in obj)
                {
                    hash = hash * 23 + i.GetHashCode();
                }
                return hash;
            }
        }
    }
    public class w3Geoset
    {
        //----------------------------------------------
        // properties
        //----------------------------------------------
        
        internal string Name { get; set; } = string.Empty;
        internal int ID { get; set; } = -1;
        internal Bitmap? UsedBitmap { get; set; } = null; // if it's null then we use white color for the triangles of the geoset

        internal Texture UsedTexture;
       
        internal List<w3Vertex> Vertices { get; set; } = new List<w3Vertex>();
       
        internal List<w3Triangle> Triangles { get; set; } = new List<w3Triangle>();  
        internal List<Extent> SequenceExtents { get; set; } = new List<Extent>();
      
        internal Extent Extent { get; set; } = new Extent();
        internal int Material_ID { get; set; } = -1; // the material it uses 
        internal bool Unselectable { get; set; }
        internal int Selection_Group { get; set; }
        //-----------------------------------------------------------------------
        //--- Reforged
        //-----------------------------------------------------------------------
        //  internal int LevelofDetail { get; set; } // Reforged
        //  internal List<SkinWeight> SkinWeights; //Reforged
        // internal List<Tangent> Tangents { get; set; } // Reforged
        //------------------------------------------------
        //----------------------------------------------
        // app-only
        //----------------------------------------------
        // internal List<Mesh> Meshes; // a mesh is an in-app-only component that determines
        // which are the 3d objects in a geoset, to work with them in the editor
        internal List<w3Edge> Edges; // an edge is composed from 2 vertices
        internal List<int> VertexGroup = new List<int>();
        internal List<List<int>> MatrixGroups = new List<List<int>>();
        internal uint UsedTextureBindId;
        internal bool isSelected;

        internal void TransferMatrixGroups()
        {
            for (int i = 0; i < VertexGroup.Count; i++)
            {
                if (i < Vertices.Count)
                {
                    Vertices[i].AttachedTo = MatrixGroups[VertexGroup[i]];
                }

            }
            // throw new NotImplementedException();
        }

        //------------------------------------------------
     
        internal List<Coordinate> TemporaryFaces { get; set; } = new List<Coordinate>();
        public bool isVisible   = true;

        internal bool IsEmpty() { return Vertices.Count == 0; }
        //----------------------------------------------------------------------
        internal int CountEdges()
        {
            HashSet<string> edges = new HashSet<string>();
            foreach (var triangle in Triangles)
            {
                // Add edges to the hash set
                AddEdge(edges, triangle.Index1, triangle.Index2);
                AddEdge(edges, triangle.Index2, triangle.Index3);
                AddEdge(edges, triangle.Index3, triangle.Index1);
            }
            // The number of edges is the count of unique edges
            return edges.Count;
        }
        private void AddEdge(HashSet<string> edges, int vertex1, int vertex2)
        {
            // Ensure that the smaller vertex ID is always first to avoid duplicates
            string edge = vertex1 < vertex2 ? $"{vertex1}-{vertex2}" : $"{vertex2}-{vertex1}";
            edges.Add(edge);
        }
        internal w3Geoset Clone()
        {
            w3Geoset geo = new w3Geoset();
            geo.Extent = Extent.Clone();
            geo.UsedBitmap = UsedBitmap;
            geo.UsedTexture = UsedTexture;
            geo.Unselectable = Unselectable;
            geo.ID = IDCounter.Next();
            geo.Vertices = CopyVertices();
            geo.Triangles = CopyTriangles();
            return geo;
        }
        //----------------------------------------------------------------------
        internal List<w3Triangle> CopyTriangles()
        {
            List<w3Triangle> triangles = new List<w3Triangle>();
            foreach (w3Triangle tr in Triangles)
            {
                triangles.Add(tr.Clone());
            }
            return triangles;
        }
        internal List<w3Vertex> CopyVertices()
        {
            List<w3Vertex> vertices = new List<w3Vertex>();
            foreach (w3Vertex tr in Vertices)
            {
                vertices.Add(tr.Clone());
            }
            return vertices;
        }
        
        internal void MergeSimilarVertices()
        {
        //can possibly cause infinite loop but we will see
        Start:
            foreach (w3Vertex vertex1 in Vertices.ToList())
            {
                foreach (w3Vertex vertex2 in Vertices.ToList())
                {
                    if (vertex1.Id == vertex2.Id) { continue; }
                    if (Calculator3D.IsDistanceLessThan(vertex1.Position, vertex2.Position, 0.15f))
                    {
                        int id1 = vertex1.Id;
                        int id2 = vertex2.Id;
                        Vertices.Remove(vertex2);
                        SetNewVertexIDToTriangles(id2, id1);
                        goto Start;
                    }
                }
            }
            return;
        }
        private void SetNewVertexIDToTriangles(int oldId, int newId)
        {
            foreach (w3Triangle tr in Triangles)
            {
                if (tr.Index1 == oldId) { tr.Index1 = newId; }
                if (tr.Index2 == oldId) { tr.Index2 = newId; }
                if (tr.Index3 == oldId) { tr.Index3 = newId; }
            }
        }
        static bool CoordinatesAreIDentical(Coordinate first, Coordinate second)
        {
            return (int)first.X == (int)second.X &&
                   (int)first.Y == (int)second.Y &&
                   (int)first.Z == (int)second.Z;
        }
        static bool IsZeroAreaTriangle(double[] vertices)
        {
            // Extracting vertices
            double x1 = vertices[0];
            double y1 = vertices[1];
            double x2 = vertices[2];
            double y2 = vertices[3];
            double x3 = vertices[4];
            double y3 = vertices[5];
            // Calculating the area of the triangle
            double area = Math.Abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2);
            // If the area is very close to zero, consider it zero
            const double epsilon = 0.000001;
            return area < epsilon;
        }

        public void RecalculateEdges()
        {
            Edges.Clear();
            HashSet<w3Edge> uniqueEdges = new HashSet<w3Edge>(new EdgeComparer());

            foreach (w3Triangle tr in Triangles)
            {
                // Create edges from the triangle's vertices
                AddUniqueEdge(tr.Vertex1, tr.Vertex2, uniqueEdges);
                AddUniqueEdge(tr.Vertex2, tr.Vertex3, uniqueEdges);
                AddUniqueEdge(tr.Vertex3, tr.Vertex1, uniqueEdges);
            }

            // Add all unique edges to the Edges list
            Edges.AddRange(uniqueEdges);
        }

        private void AddUniqueEdge(w3Vertex v1, w3Vertex v2, HashSet<w3Edge> edgeSet)
        {
            w3Edge edge = new w3Edge(v1, v2);
            edgeSet.Add(edge); // HashSet ensures no duplicates
        }

        // Comparer to ensure edges are unique, regardless of vertex order
        public class EdgeComparer : IEqualityComparer<w3Edge>
        {
            public bool Equals(w3Edge e1, w3Edge e2)
            {
                // An edge is equal if it connects the same two vertices, regardless of order
                return (e1.Vertex1 == e2.Vertex1 && e1.Vertex2 == e2.Vertex2) ||
                       (e1.Vertex1 == e2.Vertex2 && e1.Vertex2 == e2.Vertex1);
            }

            public int GetHashCode(w3Edge edge)
            {
                // Generate a hash code that treats (v1, v2) the same as (v2, v1)
                int hash1 = edge.Vertex1.GetHashCode();
                int hash2 = edge.Vertex2.GetHashCode();
                return hash1 ^ hash2; // XOR to combine the two hashes
            }
        }


        internal void ResetFaceIndexes(int max)
        {
            foreach (w3Triangle triangle in Triangles)
            {
                triangle.Index1 -= max;
                triangle.Index2 -= max;
                triangle.Index3 -= max;
            }
        }
        

        internal string ToMDL()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Geoset {");
            //vertex positions
            sb.AppendLine($"\tVertices {Vertices.Count} {{");
            foreach (w3Vertex v in Vertices)
            {
                sb.AppendLine("\t\t{ " + v.Position.ToString() + " },");
            }
            sb.AppendLine("\t}");
            //normals
            sb.AppendLine($"\tNormals {Vertices.Count} {{");
            foreach (w3Vertex v in Vertices)
            {
                sb.AppendLine("\t\t{ " + v.Normal.ToString() + " },");
            }
            sb.AppendLine("\t}");
            //vertices
            sb.AppendLine($"\tTVertices {Vertices.Count} {{");
            foreach (w3Vertex v in Vertices)
            {
                sb.AppendLine("\t\t{ " + v.Texture_Position.ToString() + " },");
            }
            sb.AppendLine("\t}");
            //faces
            sb.AppendLine($"\tFaces 1 {Triangles.Count * 3} {{\n\t\tTriangles {{\n\t\t\t{{");
            List<string> trIndexes = new List<string>();
            foreach (w3Triangle tr in Triangles)
            {
                trIndexes.Add(Vertices.FindIndex(x => x == tr.Vertex1).ToString());
                trIndexes.Add(Vertices.FindIndex(x => x == tr.Vertex2).ToString());
                trIndexes.Add(Vertices.FindIndex(x => x == tr.Vertex3).ToString());
            }
            sb.Append(string.Join(",", trIndexes));
            sb.Append("},\n\t}\n}\n");
            // vertexgroup and matrix groups
            //------------------------------------------
            var (MatrixGroups, VertexGroup) = MDLHelper.GetMatrixGroups(Vertices);


            //vertexgroup
            sb.AppendLine("\tVertexGroup {");
            
            foreach (int vg in VertexGroup)
            {
                sb.AppendLine($"\t\t{vg},");
            }
            sb.AppendLine("\t}");
            int mgc = 0; foreach (var list in MatrixGroups) { mgc += list.Count; }
            sb.AppendLine($"\tGroups {MatrixGroups.Count} {mgc} {{");
            foreach (List<int> mg in MatrixGroups)
            {
                for (int i = 0; i < mg.Count; i++)
                {
                    mg[i] = ModelHelper.Current.Nodes.FindIndex(x => x.objectId == mg[i]);
                }
                sb.AppendLine($"\t\tMatrices {{ {string.Join(",", mg)} }},");

            }
            sb.AppendLine("\t}");
            //------------------------------------------
            // sequence extents
            sb.AppendLine(Extent.ToMDL());
            foreach (Extent sqe in SequenceExtents)
            {
                sb.AppendLine("\tAnim {");
                sb.AppendLine(sqe.ToMDL());
                sb.AppendLine("\t}");
            }
            sb.AppendLine($"MaterialID {ModelHelper.Current.Materials.FindIndex(x => x.ID == Material_ID)},");
            sb.AppendLine($"SelectionGroup {Selection_Group},");
            sb.AppendLine("}");
            //extents
            return sb.ToString();
        }

        internal string ToWhimGeoset()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{BinaryConverter.FloatToBinary(Vertices.Count)}");
            sb.Append($"{BinaryConverter.FloatToBinary(Triangles.Count)}");
            foreach (w3Vertex vertex in Vertices)
            {
                string x = BinaryConverter.FloatToBinary(vertex.Position.X);
                string y = BinaryConverter.FloatToBinary(vertex.Position.Y);
                string z = BinaryConverter.FloatToBinary(vertex.Position.Z);
                string xn = BinaryConverter.FloatToBinary(vertex.Normal.X);
                string yn = BinaryConverter.FloatToBinary(vertex.Normal.Y);
                string zn = BinaryConverter.FloatToBinary(vertex.Normal.Z);
                string u = BinaryConverter.FloatToBinary(vertex.Texture_Position.U);
                string v = BinaryConverter.FloatToBinary(vertex.Texture_Position.V);
               
                sb.Append($"{x}{y}{z}{xn}{yn}{zn}{u}{v}");
            }
            foreach (w3Triangle t in Triangles)
            {
                string first = BinaryConverter.FloatToBinary(Vertices.IndexOf(t.Vertex1));
                string second = BinaryConverter.FloatToBinary(Vertices.IndexOf(t.Vertex2));
                string third = BinaryConverter.FloatToBinary(Vertices.IndexOf(t.Vertex3));
                sb.Append($"{first}{second}{third}");
            }
            return sb.ToString();
           
        }

        internal void SetTexure(string v)
        {
            throw new NotImplementedException();
        }

        internal w3Geoset()
        {
            Extent = new Extent();
            SequenceExtents = new List<Extent>();
            Triangles = new List<w3Triangle>();
            // Meshes = new List<Mesh>();
            Edges = new List<w3Edge>();
            Vertices = new List<w3Vertex>();
            // SkinWeights = new List<SkinWeight>();
            // Tangents = new List<Tangent>();
            Name = "Unnamed";//reforged
            Unselectable = false;
            Selection_Group = 0;
            //LevelofDetail = 100;
            Material_ID = 0;
        }
    }
    public class w3Edge
    {
        public w3Vertex Vertex1;
        public w3Vertex Vertex2;
        public void SelectVertices(bool select = true)
        {
            Vertex1.isSelected = select;
            Vertex2.isSelected = select;
        }
        internal bool isSelected;

        internal w3Edge(w3Vertex one, w3Vertex two)
        {
            Vertex1 = one;
            Vertex2 = two;
        }
        public void SelectIf()  {  isSelected = Vertex1.isSelected || Vertex2.isSelected; }
    }
    public class w3Geoset_Animation
    {
        internal string Name { get; set; } = string.Empty;
        internal int ID { get; set; } = -1; // app-only. to be referenced easier without additional checks
        internal int Geoset_ID { get; set; } = -1;
        internal bool Use_Color { get; set; }
        internal bool Drop_Shadow { get; set; }
        internal w3Transformation Alpha { get; set; }
        internal w3Transformation Color { get; set; }
        //--------------------------------------------------
        internal w3Geoset_Animation(int gaID, int geoset_ID, bool use_Color, bool drop_Shadow,
                               w3Transformation alpha, w3Transformation color)
        {
            ID = gaID;
            Geoset_ID = geoset_ID;
            Use_Color = use_Color;
            Drop_Shadow = drop_Shadow;
            Alpha = alpha;
            Color = color;
        }
        internal w3Geoset_Animation()
        {
            Geoset_ID = -1;
            Use_Color = false;
            Drop_Shadow = false;
            Alpha = new w3Transformation(TransformationType.Alpha);
            Alpha.StaticValue = [1];
            Color = new w3Transformation(TransformationType.Color);
        }
        internal w3Geoset_Animation(int geoID)
        {
            Geoset_ID = geoID;
            ID = IDCounter.Next();
            Use_Color = false;
            Drop_Shadow = false;
            Alpha = new w3Transformation(TransformationType.Alpha);
            Alpha.StaticValue = [1];
            Color = new w3Transformation(TransformationType.Color);
        }

        internal w3Geoset_Animation Clone()
        {
            return new w3Geoset_Animation(ID, Geoset_ID, Use_Color, Drop_Shadow, Alpha.Clone(), Color.Clone());
        }

        internal string? ToMDL()
        {
           StringBuilder sb = new StringBuilder();
            sb.AppendLine("GeosetAnim {");
            sb.AppendLine($"\tGeosetId {ModelHelper.Current.Geosets.FindIndex(x=>x.ID == Geoset_ID)},");
            if (Drop_Shadow)
            {
                sb.AppendLine("\tDropShadow,");
            }
            sb.AppendLine(Alpha.ToMDL("Alpha"));
            if (Use_Color == true)
            {
                sb.AppendLine(Color.ToMDL("Color"));
            }
           
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
    internal class w3Layer
    {
        internal int ParentMaterialID { get; set; } = -1;
        internal int ID { get; set; } = -1; // app-only. to be referenced easier without additional checks
        internal w3Transformation Alpha { get; set; } = new w3Transformation(TransformationType.Alpha);
        internal w3Transformation Diffuse_Texure_ID { get; set; } = new w3Transformation(TransformationType.Int);
        //-----REFORGED----
        // internal Transformation Normal_Texure_ID { get; set; } = new Transformation(TransformationType.Int);
        //  internal Transformation ORM_Texure_ID { get; set; } = new Transformation(TransformationType.Int);
        //  internal Transformation Emissive_Texure_ID { get; set; } = new Transformation(TransformationType.Int);
        // internal Transformation Reflections_Texure_ID { get; set; } = new Transformation(TransformationType.Int);
        // internal Transformation Team_Color_Texure_ID { get; set; } = new Transformation(TransformationType.Int);
        //  internal Transformation Emissive_Gain { get; set; } = new Transformation(TransformationType.Double); //double 
        //   internal Transformation Fresnel_Color { get; set; } = new Transformation(TransformationType.Color); //color
        //  internal Transformation Fresnel_Opacity { get; set; } = new Transformation(TransformationType.Int); //percent
        //  internal Transformation Fresnel_Team_Color { get; set; } = new Transformation(TransformationType.Color); //double
        //--------------------------------------------
        internal int Animated_Texture_ID { get; set; } = -1;
        internal bool Unshaded { get; set; } = false;
        internal bool Unfogged { get; set; } = false;
        internal bool Two_Sided { get; set; } = false;
        internal bool Sphere_Environment_Map { get; set; } = false;
        internal bool No_Depth_Test { get; set; } = false;
        internal bool No_Depth_Set { get; set; } = false;
        internal FilterMode Filter_Mode { get; set; } = FilterMode.None;
        internal w3Layer(int id)
        {
            ID = id;
        }
        internal w3Layer(w3Transformation alpha, w3Transformation texture_ID,
                    int animated_Texture_ID,
                    bool unshaded, bool unfogger, bool two_Sided,
                    bool sphere_Environment_Map, bool no_Depth_Test,
                    bool sNo_Depth_Set, FilterMode filter_Mode)
        {
            Alpha = alpha;
            Diffuse_Texure_ID = texture_ID;
            Animated_Texture_ID = animated_Texture_ID;
            Unshaded = unshaded;
            Unfogged = unfogger;
            Two_Sided = two_Sided;
            Sphere_Environment_Map = sphere_Environment_Map;
            No_Depth_Test = no_Depth_Test;
            No_Depth_Set = sNo_Depth_Set;
            Filter_Mode = filter_Mode;
        }
        internal w3Layer()
        {
            Alpha = new w3Transformation(TransformationType.Alpha); ;
            Diffuse_Texure_ID = new w3Transformation(TransformationType.ID);
            //Normal_Texure_ID = new();
            //  ORM_Texure_ID = new();
            // Team_Color_Texure_ID = new();
            //   Emissive_Texure_ID = new();
            // Reflections_Texure_ID = new();
            // Fresnel_Color = new();
            // Fresnel_Opacity = new();
            //  Fresnel_Team_Color = new();
            Animated_Texture_ID = -1;
            Unshaded = false;
            Unfogged = false;
            Two_Sided = false;
            Sphere_Environment_Map = false;
            No_Depth_Test = false;
            No_Depth_Set = false;
            Filter_Mode = FilterMode.None;
        }


        internal w3Layer Clone()
        {
            return new w3Layer(
                Alpha.Clone(),
                Diffuse_Texure_ID.Clone(),
                Animated_Texture_ID,
                Unshaded,
                Unfogged,
                Two_Sided,
                Sphere_Environment_Map,
                No_Depth_Test,
                No_Depth_Set,
                Filter_Mode);
        }

        internal string ToMDL()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"\tLayer {{");
            result.AppendLine($"\t\tFilterMode {Filter_Mode},");
            if (Two_Sided) { result.AppendLine($"\t\tTwoSided,"); }
            if (Unshaded) { result.AppendLine($"\t\tUnshaded,"); }
            if (Unfogged) { result.AppendLine($"\t\tUnfogged,"); }
            if (Sphere_Environment_Map) { result.AppendLine($"\t\tSphereEnvMap,"); }
            if (No_Depth_Test) { result.AppendLine($"\t\tNoDepthTest,"); }
            if (No_Depth_Set) { result.AppendLine($"\t\tNoDepthSet,"); }
            if (Animated_Texture_ID >= 0) { result.AppendLine($"\t\tTVertexAnimId {FindAnimatiedTextureIndex(Animated_Texture_ID)}, "); }
            if (Diffuse_Texure_ID.isStatic)
            {
                int fID = (int)Diffuse_Texure_ID.StaticValue[0];
                int index = ModelHelper.Current.Textures.FindIndex(x => x.ID == fID);
                result.AppendLine($"\t\tstatic TextureID {index},");
            }
            else
            {
                result.AppendLine(Diffuse_Texure_ID.ToMDL());
            }
            if (Alpha.isStatic)
            {
                result.AppendLine($"\t\tstatic Alpha {Alpha.StaticValue[0] },");
            }
            else
            {
                result.AppendLine(Alpha.ToMDL());
            }
            result.AppendLine("}");
            return result.ToString();
        }

        private int FindAnimatiedTextureIndex(int id)
        {
            if (id < 0) { return -1; }
            for (int i = 0; i < ModelHelper.Current.Texture_Animations.Count; i++)
            {
                if (ModelHelper.Current.Texture_Animations[i].ID == id) { return i; }
            }
            return -1;
        }
    }
    public class w3Texture // does it have id or just path?
    { // also called bitmap
        internal int ID { get; set; } = -1;
        internal string Path { get; set; }
        internal bool Wrap_Height { get; set; }
        internal bool Wrap_Width { get; set; }
        internal int Replaceable_ID { get; set; } = -1;
        internal w3Texture(int id, string path, bool wrap_Height, bool wrap_Width, int replaceable_ID)
        {
            ID = id;
            Path = path;
            Wrap_Height = wrap_Height;
            Wrap_Width = wrap_Width;
            Replaceable_ID = replaceable_ID;
        }
        internal w3Texture()
        {
            Path = string.Empty;
            Wrap_Height = false;
            Wrap_Width = false;
            Replaceable_ID = 0;
        }

        internal w3Texture Clone()
        {
            return new w3Texture(ID, Path, Wrap_Height, Wrap_Width, Replaceable_ID);
        }
        internal void Set0ReplaceableIdIFEmpty()
        {
            if (Path.Length == 0 && Replaceable_ID < 0)
            {
                Replaceable_ID = 0;
            }
        }
        internal string ToListBoxItem()
        {
            if (Path.Length == 0) { return $"{ID}: Replaceable Texture {Replaceable_ID}"; }
            return $"{ID}: {Path}";
        }
        internal string TreeItemName()
        {
            if (Replaceable_ID == 0)
            {
                if (Path.Trim().Length == 0) { return ID.ToString(); }
                else
                {
                    return StringHelper.GetFilenameFromPath(Path);
                }
            }
            else
            {
                return $"RpID {Replaceable_ID}";
            }
            return "";
        }
        internal void Debug()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Texture {ID}");
            if (Replaceable_ID == 0)
            {
                sb.AppendLine($"Path: {Path}");
            }
            else
            {
                sb.AppendLine($"Replaceable ID: {Replaceable_ID}");
            }
           if (Wrap_Height) sb.AppendLine($"Wrap height: {Wrap_Height}");
           if (Wrap_Width) sb.AppendLine($"Wrap width: {Wrap_Width}");



            Clipboard.SetText(sb.ToString());
        }

        internal string ToMDL()
        {
            StringBuilder result = new();
            result.AppendLine($"\tBitmap {{");
            result.AppendLine($"\t\t\tImage \"{Path}\",");
            if (Replaceable_ID > 0) { result.AppendLine($"\t\tReplaceableId {Replaceable_ID},"); }
            if (Wrap_Width) { result.AppendLine($"WrapWidth,"); }
            if (Wrap_Height) { result.AppendLine($"WrapHeight,"); }
            result.AppendLine($"\t}}");
            return result.ToString();
        }
    }
    internal class w3Texture_Animation //also called TVerxtexAnim
    {
        internal string Name { get; set; } = string.Empty;
        internal int ID { get; set; } = -1;// app-only. to be referenced easier without additional checks
        internal w3Transformation Translation { get; set; } = new w3Transformation(TransformationType.Translation);
        internal w3Transformation Rotation { get; set; } = new w3Transformation(TransformationType.Rotation);
        internal w3Transformation Scaling { get; set; } = new w3Transformation(TransformationType.Scaling);
        internal w3Texture_Animation(w3Transformation translation, w3Transformation rotation, w3Transformation scaling)
        {
            Translation = translation;
            Rotation = rotation;
            Scaling = scaling;
        }
        internal w3Texture_Animation(int id, w3Transformation translation, w3Transformation rotation, w3Transformation scaling)
        {
            ID = id;
            Translation = translation;
            Rotation = rotation;
            Scaling = scaling;
        }
        internal w3Texture_Animation() { ID = -1; }
        internal w3Texture_Animation(int id)
        {
            ID = id;
            Translation = new w3Transformation(TransformationType.Translation);
            Rotation = new w3Transformation(TransformationType.Rotation);
            Scaling = new w3Transformation(TransformationType.Scaling);
        }

        internal w3Texture_Animation Clone()
        {
            return new w3Texture_Animation(ID, Translation.Clone(), Rotation.Clone(), Scaling.Clone());
        }
        internal void Debug()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Texture animation {ID}");
            sb.AppendLine($"Translation: {Translation.ToString()}");
            sb.AppendLine($"Rotation: {Rotation.ToString()}");
            sb.AppendLine($"Scaling: {Scaling.ToString()}");
            Clipboard.SetText(sb.ToString());
        }

        internal string toMDL()
        {
            StringBuilder result = new();
            result.AppendLine("TVertexAnim { ");
            if (Translation.isStatic == false) result.AppendLine(Translation.ToMDL());
            if (Rotation.isStatic == false) result.AppendLine(Translation.ToMDL());
            if (Scaling.isStatic == false) result.AppendLine(Translation.ToMDL());
            result.AppendLine(" }");
            return result.ToString();
        }
    }
    public class w3Keyframe
    {
        internal int Track;
        internal float[] Data;
        internal float[] InTan;
        internal float[] OutTan;
        internal w3Keyframe(int whichFrame, float[] data, float[] intan, float[] outtan)
        {
            Track = whichFrame;
            Data = data;
            InTan = intan;
            OutTan = outtan;
        }
        internal w3Keyframe(int whichFrame, float[] data)
        {
            Track = whichFrame;
            Data = data;
            
        }
        internal w3Keyframe( )
        { 
            
            InTan = [0,0,0,1];
            OutTan = [0,0,0,1]; 

        }
        internal void Reset()
        {
            Data = new float[3];
            InTan = new float[3];
            OutTan = new float[3];
        }
         
        internal w3Keyframe Clone()
        {
            return new w3Keyframe(Track, Data, InTan, OutTan); ;
        }

        internal string? ToFormattedCopy()
        {
            return Track.ToString() + " " + string.Join(" ", Data) + string.Join(" ", InTan) + string.Join(" ", OutTan);
        }
    }
    public class w3Transformation
    {
        internal int ID { get; set; } = -1;
        internal string BelongsTo { get; set; } = "Not set";
        internal bool isStatic { get; set; }
        internal float[] StaticValue = [0,0,0];
        public string GetStaticValue(int v)
        {
            switch (v)
            {
                case 1: return StaticValue[0].ToString();
                case 2: return $"{StaticValue[0]}, {StaticValue[1]}";
                case 3: return $"{StaticValue[0]}, {StaticValue[1]}, {StaticValue[2]}";
            }
            return string.Join(", ", StaticValue);
        }
        internal TransformationType Type { get; set; } = TransformationType.Undefined;
        internal int Global_Sequence_ID { get; set; } = -1;
        internal InterpolationType Interpolation_Type { get; set; } = InterpolationType.DontInterp;
        internal List<w3Keyframe> Keyframes { get; set; } = new List<w3Keyframe>();
        public bool? BoolValue {

            get

            {
                return StaticValue[0] == 1 ? true : false;
            }
        }

        //----------------------------------------------
        internal void ClearTransformation()
        {
            Keyframes.Clear();
            Global_Sequence_ID = -1;
            Interpolation_Type = 0;
        }
        internal void AddEmptyTrack(int frame)
        {
            float[] value = [];
            if (Type == TransformationType.Translation) { value = [0, 0, 0]; }
            if (Type == TransformationType.Rotation) { value = [0, 0, 0]; }
            if (Type == TransformationType.Scaling) { value = [1, 1, 1]; }
            if (Type == TransformationType.Int) { value = [0]; }
            if (Type == TransformationType.Float) { value = [0]; }
            if (Type == TransformationType.Color) { value = [1, 1, 1]; }
            if (Type == TransformationType.Visibility) { value = [1]; }
            if (Type == TransformationType.Alpha) { value = [0]; }
            Keyframes.Add(new w3Keyframe(frame, value, value, value));
        }
        //----------------------------------------------
        //constructors
        //----------------------------------------------
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (isStatic) { return string.Join(",", StaticValue); }
            else
            {
                sb.AppendLine("Type: " + Type.ToString());
                sb.AppendLine("Interpolation: " + Interpolation_Type.ToString());
                sb.AppendLine($"Global sequence id: {Global_Sequence_ID}");
                foreach (w3Keyframe kf in Keyframes)
                {
                    sb.AppendLine($"\t{kf.Track}: {string.Join(", ", kf.Data)}");
                    if ((int)Interpolation_Type > 1)
                    {
                        sb.AppendLine($"\t\tIntan: {string.Join(", ", kf.InTan)}");
                        sb.AppendLine($"\t\tOutTan: {string.Join(", ", kf.OutTan)}");
                    }
                }
            }
            return sb.ToString();
        }
        internal w3Transformation()
        {
            Type = TransformationType.Undefined;
            Global_Sequence_ID = -1;
            Interpolation_Type = 0;
            SetStaticValueBaseOnType(TransformationType.Translation); ;
            isStatic = true;
        }
        internal w3Transformation(
             bool is_static,
             float[] staticValue,
            TransformationType type,
            int global_Sequence_ID,
          int interpolation_Typeid,
            List<w3Keyframe> keyframes)
        {
            isStatic = is_static;
            if (is_static) { StaticValue = staticValue; }
            else { SetStaticValueBaseOnType(Type); Keyframes = keyframes; }
            Type = type;
            Global_Sequence_ID = global_Sequence_ID;
            Interpolation_Type = (InterpolationType)interpolation_Typeid;
        }
        internal w3Transformation(TransformationType type)
        {
            isStatic = true;
            Global_Sequence_ID = -1;
            Interpolation_Type = 0;
            SetStaticValueBaseOnType(type);
        }
        internal w3Transformation(
            TransformationType type,
            int global_Sequence_ID,
            int interpolation_Typeid
            )
        {
            SetStaticValueBaseOnType(type);
            Global_Sequence_ID = global_Sequence_ID;
            Interpolation_Type = (InterpolationType)interpolation_Typeid;
            Keyframes = new List<w3Keyframe>();
        }
        private void SetStaticValueBaseOnType(TransformationType type)
        {
            Type = type;
            switch (type)
            {
                case TransformationType.Translation:
                case TransformationType.Rotation:
                    StaticValue = [0, 0, 0];
                    break;
                case TransformationType.Scaling:
                    StaticValue = [1, 1, 1];
                    break;
                case TransformationType.Int:
                case TransformationType.Float:
                    StaticValue[0] = 0;
                    break;
                case TransformationType.Color:
                    StaticValue = [1, 1, 1];
                    break;
                case TransformationType.Visibility:
                    StaticValue = [1];
                    break;
                case TransformationType.Alpha:
                    StaticValue = [1];
                    break;
                default:
                    StaticValue = []; break;
            }
        }
    
        internal string GetKeyframesAsNative()
        {
            List<string> kf = new List<string>();
            foreach (w3Keyframe frame in Keyframes)
            {
                kf.Add($"{frame.Data}|{frame.InTan}|{frame.OutTan}");
            }
            return string.Join("_", kf);
        }
        private List<w3Keyframe> CloneKeyframes()
        {
            List<w3Keyframe> kf = new List<w3Keyframe>();
            foreach (w3Keyframe k in Keyframes)
            {
                kf.Add(k.Clone());
            }
            return kf;
        }
        internal w3Transformation Clone()
        {
            return new w3Transformation(isStatic, StaticValue, Type, Global_Sequence_ID,
               (int)Interpolation_Type, CloneKeyframes());
        }
        internal string ToTooltip()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Interpolation_Type.ToString());
            foreach (w3Keyframe k in Keyframes)
            {
                if ((int)Interpolation_Type > 1)
                {
                    sb.AppendLine(k.Track.ToString() + ": " + string.Join(",", k.Data) + ", " + string.Join(",", k.InTan) + ", " + string.Join(",", k.OutTan));
                    sb.AppendLine("  Intan:" + ": " + string.Join(",", k.InTan));
                    sb.AppendLine("  Outtan:" + ": " + string.Join(",", k.OutTan));
                }
                else
                {
                    sb.AppendLine(k.Track.ToString() + ": " + string.Join(",", k.Data));
                }
            }
            return sb.ToString();
        }
        internal void GiveDefaultStaticValue()
        {
            switch (Type)
            {
                case TransformationType.Translation:
                case TransformationType.Rotation:
                    StaticValue = [0, 0, 0]; break;
                case TransformationType.Scaling: StaticValue = [1, 1, 1]; break;
                case TransformationType.Int:
                case TransformationType.Float:
                case TransformationType.ID:
                    StaticValue[0] = -1; break;
                case TransformationType.Visibility:
                    StaticValue[0] = 1; break;
                case TransformationType.Alpha: StaticValue = [1]; break;
                case TransformationType.Color: StaticValue = [1, 1, 1]; break;
            }
        }

        internal string ToMDL(string Real_type = "")
        {
            
            StringBuilder result = new();
            if (isStatic)
            {
                if (StaticValue.Length == 1)
                {
                    return "static " + Real_type + " " + string.Join(", ", StaticValue) + ",";
                }
                else
                {
                    return "static " + Real_type + " {" + string.Join(", ", StaticValue) + "},";
                }
                
            }
            else
            {

             
            string type = Real_type == "" ? Type.ToString() : Real_type;
            
            string data = "";
            string intan = "";
            string outtan = "";
           
            result.AppendLine($"\t\t\t{type} {Keyframes.Count} {{");
            result.AppendLine($"\t\t\t\t{Interpolation_Type},");
                if (Global_Sequence_ID >= 0)
                {
                    int index = ModelHelper.Current.Global_Sequences.FindIndex(x => x.ID == Global_Sequence_ID); ;
                    result.AppendLine($"\t\t\t\tGlobalSeqId {index},");
                }
                foreach (w3Keyframe frame in Keyframes)
                {
                    switch (Type)
                    {
                        case TransformationType.Translation:
                            data = string.Join(", ", frame.Data);
                            intan = string.Join(", ", frame.InTan);
                            outtan = string.Join(", ", frame.OutTan);
                            break;
                        case TransformationType.Scaling:
                            data = Converters.ScalingPercentageToNormalizedString(frame.Data);  
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = Converters.ScalingPercentageToNormalizedString(frame.InTan);
                            outtan = Converters.ScalingPercentageToNormalizedString(frame.OutTan);
                            break;
                        case TransformationType.Rotation:
                            data = string.Join(", " ,Calculator.Rotation_Euler_To_Quaternion(frame.Data));
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = string.Join(", ", Calculator.Rotation_Euler_To_Quaternion(frame.Data));
                            outtan = string.Join(", ", Calculator.Rotation_Euler_To_Quaternion(frame.OutTan));
                            break;
                        case TransformationType.Int:
                            data = frame.Data[0].ToString();
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = frame.InTan[0].ToString();
                            outtan = frame.OutTan[0].ToString();
                            break;
                        case TransformationType.ID:
                            int index1 = ModelHelper.Current.Textures.FindIndex(x => x.ID == frame.Data[0]);
                            int index2 = ModelHelper.Current.Textures.FindIndex(x => x.ID == frame.InTan[0]);
                            int index3 = ModelHelper.Current.Textures.FindIndex(x => x.ID == frame.OutTan[0]);
                            data = index1.ToString();
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = index2.ToString();
                            outtan = index3.ToString();
                            break;
                        case TransformationType.Float:
                            data = frame.Data[0].ToString("F6");
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { intan = "0".ToString(); outtan = 0.ToString(); break; }
                            intan = frame.InTan[0].ToString("F6");
                            outtan = frame.OutTan[0].ToString("F6"); break;
                        case TransformationType.Color:
                            data = string.Join(", ", Converters.RGBToWarcraft3Color(frame.Data)); 
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = string.Join(", ", Converters.RGBToWarcraft3Color(frame.InTan));
                            outtan = string.Join(", ", Converters.RGBToWarcraft3Color(frame.OutTan));
                            break;
                        case TransformationType.Visibility:
                            data = frame.Data[0] >= 1 ? "1" : "0";
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = frame.InTan[0] >= 1 ? "1" : "0";
                            outtan = frame.OutTan[0] >= 1 ? "1" : "0"; break;
                        case TransformationType.Alpha:
                            data = (frame.Data[0]/100).ToString();
                            if (frame.InTan.Length == 0 || frame.OutTan.Length == 0) { break; }
                            intan = (frame.InTan[0]/100).ToString();
                            outtan = (frame.OutTan[0] /100).ToString();
                            break;
                    }
                    if (data.Contains(","))
                    {
                        result.AppendLine($"\t\t\t\t{frame.Track}: {{ {data} }},");
                    }
                    else
                    {
                        result.AppendLine($"\t\t\t\t{frame.Track}: {data},");
                    }

                    if ( (int)Interpolation_Type == 2 || (int)Interpolation_Type  == 3)
                    {
                        if (intan.Contains(","))
                        {
                            result.AppendLine($"\t\t\t\t\tInTan {{ {intan} }},");
                        }
                        else
                        {
                            result.AppendLine($"\t\t\t\t\tInTan {intan},");
                        }
                        if (outtan.Contains(","))
                        {
                            result.AppendLine($"\t\t\t\t\tOutTan {{ {outtan} }},");
                        }
                        else
                        {
                            result.AppendLine($"\t\t\t\t\tOutTan {outtan},");
                        }

                    }
                }
            }
            result.AppendLine($"\t\t\t}}");

            return result.ToString();
        }

    internal void Clamp()
    {
       foreach (w3Keyframe kf in Keyframes)
            {

                if (Type == TransformationType.Scaling)
                {

                    kf.Data[0] = Clamper.Positive(kf.Data[0]);
                    kf.Data[1] = Clamper.Positive(kf.Data[1]);
                    kf.Data[2] = Clamper.Positive(kf.Data[2]);
                    kf.InTan[0] = Clamper.Positive(kf.InTan[0]);
                    kf.InTan[0] = Clamper.Positive(kf.InTan[1]);
                    kf.InTan[0] = Clamper.Positive(kf.InTan[2]);
                    kf.OutTan[0] = Clamper.Positive(kf.OutTan[0]);
                    kf.OutTan[0] = Clamper.Positive(kf.OutTan[1]);
                    kf.OutTan[0] = Clamper.Positive(kf.OutTan[2]);


                }
                if (Type == TransformationType.Rotation)
                {

                    kf.Data[0] = Clamper.Degree(kf.Data[0]);
                    kf.Data[1] = Clamper.Degree(kf.Data[1]);
                    kf.Data[2] = Clamper.Degree(kf.Data[2]);
                    kf.InTan[0] = Clamper.Degree(kf.InTan[0]);
                    kf.InTan[1] = Clamper.Degree(kf.InTan[1]);
                    kf.InTan[2] = Clamper.Degree(kf.InTan[2]);
                    kf.OutTan[0] = Clamper.Degree(kf.OutTan[0]);
                    kf.OutTan[1] = Clamper.Degree(kf.OutTan[1]);
                    kf.OutTan[2] = Clamper.Degree(kf.OutTan[2]);
                }
                if (Type == TransformationType.Alpha)
                {

                    kf.Data[0] = Clamper.Percentage(kf.Data[0]);
                    
                    kf.InTan[0] = Clamper.Percentage(kf.InTan[0]);
                    
                    kf.OutTan[0] = Clamper.Percentage(kf.OutTan[0]);
                   
                }
                if (Type == TransformationType.Color)
                {

                    kf.Data[0] = Clamper.RGB(kf.Data[0]);
                    kf.Data[1] = Clamper.RGB(kf.Data[1]);
                    kf.Data[2] = Clamper.RGB(kf.Data[2]);
                    kf.InTan[0] = Clamper.RGB(kf.InTan[0]);
                    kf.InTan[1] = Clamper.RGB(kf.InTan[1]);
                    kf.InTan[2] = Clamper.RGB(kf.InTan[2]);
                    kf.OutTan[0] = Clamper.RGB(kf.OutTan[0]);
                    kf.OutTan[1] = Clamper.RGB(kf.OutTan[1]);
                    kf.OutTan[2] = Clamper.RGB(kf.OutTan[2]);
                }
                if (Type == TransformationType.ID)
                {

                    kf.Data[0] = Clamper.Positive(kf.Data[0]);

                    kf.InTan[0] = Clamper.Positive(kf.InTan[0]);

                    kf.OutTan[0] = Clamper.Positive(kf.OutTan[0]);

                }
                if (Type == TransformationType.Visibility)
                {

                    kf.Data[0] = Clamper.Visibility(kf.Data[0]);

                    kf.InTan[0] = Clamper.Visibility(kf.InTan[0]);

                    kf.OutTan[0] = Clamper.Visibility(kf.OutTan[0]);

                }
            }
        }
}
    //--------------------------------
    //--------------------------------
    //--------------------------------
    //--------------------------------

    public class Coordinate
    {
        internal float X, Y, Z;
         
        internal Coordinate(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public static explicit operator Vector3(Coordinate coord)
        {
            Vector3 vector = new Vector3();
            vector.X =coord. X;
            vector.Y =coord. Y;
            vector.Z =coord. Z;
            return vector;

        }
        public static explicit operator Vector2(Coordinate coord)
        {
            Vector2 vector = new Vector2();
            vector.X = coord.X;
            vector.Y = coord.Y;
          
            return vector;

        }
        internal Coordinate(double x, double y, double z)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }
        internal void Negate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }
        internal Coordinate()
        {
            X = 0; Y = 0; Z = 0;
        }
        internal Coordinate Clone()
        {
            return new Coordinate(X, Y, Z);
        }
        public override string ToString()
        {
            return $"{X:f6}, {Y:f6}, {Z:f6}";
        }
        internal bool Compare(Coordinate coordinate)
        {
            if (X == coordinate.X && Y == coordinate.Y && Z == coordinate.Z) { return true; }
            return false;
        }
        internal void SetTo(Coordinate firstCoordinate)
        {
            X = firstCoordinate.X; Y = firstCoordinate.Y; Z = firstCoordinate.Z;
        }
        internal void SetTo(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
        internal void GetFromDoubleArray(float[] doubles)
        {
            X = doubles[0];
            Y = doubles[1];
            Z = doubles[2];
        }
        public void ScaleFrom(Coordinate origin, float scale)
        {
            X = origin.X + (X - origin.X) * scale;
            Y = origin.Y + (Y - origin.Y) * scale;
            Z = origin.Z + (Z - origin.Z) * scale;
        }

        internal string ToStringBroken()
        {
            return $"\n{X:f6}\n{Y:f6}\n{Z:f6}";
        }

        internal float[] ToArray()
        {
            return [X,Y,Z];
        }

        internal void Floor()
        {
            X = (float)Math.Floor(X);
            Y = (float)Math.Floor(Y);
            Z = (float)Math.Floor(Z);
        }
        internal void Ceiling()
        {
            X = (float)Math.Ceiling(X);
            Y = (float)Math.Ceiling(Y);
            Z = (float)Math.Ceiling(Z);
        }

        internal void Null()
        {
            X = 0; Y=0; Z=0;
        }

        internal bool SameAs(Coordinate position)
        {
            return position.X == X && position.Y == Y && position.Z == Z;
        }

        internal void ChangeWith(float x, float y, float z)
        {
            X += x; Y += y; Z += z;
        }
        internal void ChangeWith(float[] i)
        {
            X += i[0]; Y += i[1]; Z += i[2];
        }

        internal void ScaleWith(float[] instruction)
        {
            X *= instruction[0] / 100;
            Y *= instruction[1] / 100; Z *= instruction[2] / 100;
        }
    }
    internal class Coordinate2D
    {
        internal float U { get; set; }
        internal float V { get; set; }
        internal Coordinate2D(float x, float y)
        {
            U = x;
            V = y;
        }
        internal Coordinate2D(double x, double y)
        {
            U = (float)x;
            V = (float)y;
        }
        internal Coordinate2D()
        {
            U = 0;
            V = 0;
        }
        internal Coordinate2D Clone()
        {
            return new Coordinate2D(U, V);
        }
        public override string ToString()
        {
            return $"{U}, {V}";
        }
        internal void SetTo(float v1, float v2)
        {
            U = v1;
            V = v2;
        }
        internal void SetTo(Coordinate2D coord)
        {
            U = coord.U; V = coord.V;
          
        }

        internal void Swap()
        {
            float v = V;
            float u = U;
            U = v;
            V = u;
        }

        internal void Clamp()
        {
            if (U < -10) U = -10;
            if (U >10) U = 10;
            if (V < -10) V = -10;
            if (V > 10) V = 10;
        }

        internal void SwapWith(Coordinate2D texture_Position)
        {
            Coordinate2D temp =  Clone();
            U = texture_Position.U;
            V = texture_Position.V;
            texture_Position.U = temp.U;
            texture_Position.V = temp.V;
            
        }

        internal void ClampDefault()
        {
            if (U < 0 || U > 1)
            {
                // Keep only the decimal part, even if U is negative
                U = U - (float)Math.Truncate(U);
            }
            // Values in [0, 1] remain unchanged

            if (V < 0 || V > 1)
            {
                // Keep only the decimal part, even if V is negative
                V = V - (float)Math.Truncate(V);
            }
            // Values in [0, 1] remain unchanged
        }
    }
    public class InsetConstaint
    {
        public w3Vertex one, two, tree;
        public InsetConstaint(w3Vertex first, w3Vertex second, w3Vertex trhid)
        {
            one = first; two = second; tree = trhid;
        }
       
    }
    //------------------ THE GEOSET ELEMENTS -------------------
    public class w3Vertex
    {
        internal bool isSelected = false;
        internal bool isSelectedUV = false;
        internal bool isRigged = false;
        private List<int> list;

        internal int Id { get; set; } = -1;
        
        internal Extent TransformationExtent { get; set; } = new Extent();
        internal Coordinate Position { get; set; } = new(); //index from that list in the geoset
        internal Coordinate Normal { get; set; } = new(); //index from that list in the geoset
        internal Coordinate2D Texture_Position { get; set; } = new Coordinate2D(); //index from that list in the geoset
        internal List<int> AttachedTo { get; set; } = new(); // a simplified version of attaching, that will later be converted to matrix groups and vertex groups
        internal w3Vertex()
        {
            Position = new Coordinate();
            Normal = new Coordinate();
            Texture_Position = new Coordinate2D();
        }
        public w3Vertex(int id, float x, float y, float z)
        {
            Id = id;
            Position = new Coordinate(x, y, z);
        }
        public w3Vertex(int id, float x, float y, float z, float normalx, float normaly, float normalz, float tu, float tv, int boneID)
        {
            Id = id;
            Position = new Coordinate(x, y, z);
            Normal = new Coordinate(normalx, normaly, normalz);
            Texture_Position = new Coordinate2D(tu, tv);
            AttachedTo = [boneID];
        }
        public w3Vertex(  float x, float y, float z  )
        {
            Id = IDCounter.Next();
            Position = new Coordinate(x, y, z);
            Normal = new Coordinate(x, y, z);
            Texture_Position = new Coordinate2D(0, 0);
            AttachedTo = [0];
        }
        internal w3Vertex Clone()
        {
            return new w3Vertex()
            {
                Position = Position.Clone(),
                Normal = Normal.Clone(),
                Id = IDCounter.Next(),
                Texture_Position = Texture_Position.Clone(),
                AttachedTo = AttachedTo.ToList(),
            };
        }
        internal w3Vertex Clone(int id)
        {
            return new w3Vertex()
            {
                Id = id,
                Position = Position.Clone(),
                Normal = Normal.Clone(),
                Texture_Position = Texture_Position.Clone(),
                AttachedTo = AttachedTo
            };
        }
        internal void SetPosition(float x, float y, float z)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
        }
        internal void SetPosition(Coordinate c)
        {
            Position.X = c.X;
            Position.Y = c.Y;
            Position.Z = c.Z;
        }

        internal bool SameAs(w3Vertex v)
        {
           return v.Position.X == Position.X && v.Position.Y == Position.Y && v.Position.Z == Position.Z;
        }

       

        internal w3Vertex(float x, float y, float z, float nx, float ny, float nz, float tx, float ty)
        {
            Position = new Coordinate(x, y, z);
            Normal = new Coordinate(nx, ny, nz);
            Texture_Position = new Coordinate2D(tx, ty);
        }
        internal w3Vertex(float x, float y, float z, float nx, float ny, float nz, float tx, float ty, List<int> attachTo)
        {
            Position = new Coordinate(x, y, z);
            Normal = new Coordinate(nx, ny, nz);
            Texture_Position = new Coordinate2D(tx, ty);
            AttachedTo.AddRange(attachTo);
        }

        public w3Vertex(int id, float x, float y, float z, List<int> list) : this(id, x, y, z)
        {
            this.list = list;
        }
    }
    /* internal class Tangent //reforged only 
     {
         internal double X { get; set; }
         internal double Y { get; set; }
         internal double Z { get; set; }
         internal int W { get; set; }
     } 
   internal class Mesh
     {
         internal List<int> Vertices { get; set; }
     }*/
    public class w3Triangle
    {
        internal w3Vertex Vertex1;
        internal w3Vertex Vertex2;
        internal w3Vertex Vertex3;
        internal int Index1;
        internal int Index2;
        internal int Index3;
        internal bool isSelected = false;
        public InsetConstaint InsetConstaint;
        public void SelectVertices(bool select = true)
        {
            Vertex1.isSelected = select;
            Vertex2.isSelected = select;
            Vertex3.isSelected = select;
        }
        internal w3Triangle(w3Vertex one, w3Vertex two, w3Vertex tri)
        {
            Vertex1 = one;
            Vertex2 = two;
            Vertex3 = tri;
           
        }
        public void SelectIf()
        {
            isSelected = Vertex1.isSelected || Vertex2.isSelected || Vertex3.isSelected;
        }
        internal w3Triangle() { }
        public override string ToString()
        {
            return $"{Vertex1.Id}, {Vertex2.Id}, {Vertex3.Id}";
        }
        internal w3Triangle Clone()
        {
            return new w3Triangle( Vertex1,Vertex2,Vertex3);
        }
        public List<Tuple<w3Vertex, w3Vertex>> GetEdges()
        {
            return new List<Tuple<w3Vertex, w3Vertex>>()
        {
            Tuple.Create(Vertex1, Vertex2),
            Tuple.Create(Vertex2, Vertex3),
            Tuple.Create(Vertex3, Vertex1)
        };
        }

        internal void InsetScale()
        {
            // Calculate the centroid (center) of the triangle
            float centerX = (Vertex1.Position.X + Vertex2.Position.X + Vertex3.Position.X) / 3;
            float centerY = (Vertex1.Position.Y + Vertex2.Position.Y + Vertex3.Position.Y) / 3;
            float centerZ = (Vertex1.Position.Z + Vertex2.Position.Z + Vertex3.Position.Z) / 3;

            // Scale each vertex towards the center by 10%
            Vertex1.Position.X = centerX + (Vertex1.Position.X - centerX) * 0.9f;
            Vertex1.Position.Y = centerY + (Vertex1.Position.Y - centerY) * 0.9f;
            Vertex1.Position.Z = centerZ + (Vertex1.Position.Z - centerZ) * 0.9f;

            Vertex2.Position.X = centerX + (Vertex2.Position.X - centerX) * 0.9f;
            Vertex2.Position.Y = centerY + (Vertex2.Position.Y - centerY) * 0.9f;
            Vertex2.Position.Z = centerZ + (Vertex2.Position.Z - centerZ) * 0.9f;

            Vertex3.Position.X = centerX + (Vertex3.Position.X - centerX) * 0.9f;
            Vertex3.Position.Y = centerY + (Vertex3.Position.Y - centerY) * 0.9f;
            Vertex3.Position.Z = centerZ + (Vertex3.Position.Z - centerZ) * 0.9f;
        }

    }
    internal class Ngon
    {
        internal List<w3Vertex> Vertices = new List<w3Vertex>();
    }
    internal class wQuad
    {
        internal int ID { get; set; } = -1;
        internal int Id1 { get; set; } = -1;
        internal int Id2 { get; set; } = -1;
        internal int Id3 { get; set; } = -1;
        internal int Id4 { get; set; } = -1;
        internal wQuad(int one, int two, int three, int four)
        {
            Id1 = one;
            Id2 = two;
            Id3 = three;
            Id4 = four;
        }
        internal wQuad() { }
        public override string ToString()
        {
            return $"{Id1}, {Id2}, {Id3} {Id4}";
        }
    }
    // now the nodes
    internal class Bone
    {
        internal int Geoset_ID { get; set; } = -1; // if it's -1 then it's (multiple)
        internal int Geoset_Animation_ID { get; set; } = -1;//if it's -1 then it's(none)
        internal Bone() { Geoset_ID = -1; Geoset_Animation_ID = -1; }
        internal Bone(int gid, int gaid)
        {
            Geoset_ID = gid;
            Geoset_Animation_ID = gaid;
        }
    }
    internal class Helper
    {
        // helper doesnt have properties
    }
    internal class Collision_Shape
    {
        internal CollisionShapeType Type { get; set; }
        internal Extent Extents { get; set; } // serves for both types
        internal Collision_Shape(CollisionShapeType type, Extent extents)
        {
            Type = type;
            Extents = extents;
        }
        internal Collision_Shape()
        {
            Type = CollisionShapeType.Box;
            Extents = new Extent();
        }
    }
    internal class Event_Object
    {
        internal List<int> Tracks { get; set; } = new List<int>();
        internal string Type { get; set; }
        internal string Data { get; set; } = string.Empty;
        internal char identifier = 'x';
        internal char Identifier
        {
            get
            {
                return identifier;
            }
            set
            {
                if (char.IsLetter(value))
                {
                    identifier = value;
                }
                else { identifier = 'x'; }
            }
        }
        internal int Global_sequence_ID { get; set; } = -1;
        internal Event_Object()
        {
        }
        internal Event_Object(List<int> tracks, string type, string data,
                            char identifier, int global_sequence_ID
                             )
        {
            Tracks = tracks;
            Type = type;
            Data = data;
            Identifier = identifier;
            Global_sequence_ID = global_sequence_ID;
        }
        internal string ToMDL()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\tEventTrack {Tracks.Count}{{");
            foreach (int i in Tracks)
            {
                sb.AppendLine($"\t\t{i},"); ;
            }
            sb.AppendLine("\t}"); ;
            return sb.ToString(); ;
        }
    }
    enum LightType
    {
        Omnidirectional, Directional, Ambient
    }
    internal class w3Light
    {
        internal w3Transformation Color { get; set; } = new w3Transformation(TransformationType.Color);
        internal w3Transformation Ambient_Color { get; set; } = new w3Transformation(TransformationType.Color);
        internal w3Transformation Ambient_Intensity { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Intensity { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Attenuation_Start { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Attenuation_End { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Visibility { get; set; } = new w3Transformation(TransformationType.Visibility);
        internal LightType Type { get; set; } = LightType.Omnidirectional;
        internal readonly string[] Types = ["Omnidirectional", "Directional", "Ambient"];
        internal w3Light(w3Transformation color, w3Transformation ambient_Color,
                   w3Transformation ambient_Intensity, w3Transformation intensity,
                   w3Transformation attenuation_Start, w3Transformation attenuation_End,
                   w3Transformation visibility, LightType type
                   )
        {
            Color = color;
            Ambient_Color = ambient_Color;
            Ambient_Intensity = ambient_Intensity;
            Intensity = intensity;
            Attenuation_Start = attenuation_Start;
            Attenuation_End = attenuation_End;
            Visibility = visibility;
            Type = type;
        }
        internal w3Light()
        {
            Color = new w3Transformation(TransformationType.Color);
            Ambient_Color = new w3Transformation(TransformationType.Color);
            Ambient_Intensity = new w3Transformation(TransformationType.Int);
            Intensity = new w3Transformation(TransformationType.Int);
            Attenuation_Start = new w3Transformation(TransformationType.Int);
            Attenuation_End = new w3Transformation(TransformationType.Int);
            Visibility = new w3Transformation(TransformationType.Visibility);
        }
    }
    internal class Particle_Emitter_1
    {
        internal w3Transformation Emission_Rate { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Life_Span { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Initial_Velocity { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Gravity { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Longitude { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Latitude { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Visibility { get; set; } = new w3Transformation(TransformationType.Visibility);
        internal string Particle_Filename { get; set; } = string.Empty;
        internal bool Emitter_Uses_MDL { get; set; } = false;
        internal bool Emitter_Uses_TGA { get; set; } = false;
        internal Particle_Emitter_1()
        {
        }
        internal Particle_Emitter_1(w3Transformation emission_Rate, w3Transformation life_Span,
                              w3Transformation initial_Velocity, w3Transformation gravity,
                              w3Transformation longtitude, w3Transformation latitude,
                              w3Transformation visibility, string particle_Filename,
                              bool emitter_Uses_MDL, bool emitter_Uses_TGA
                             )
        {
            Emission_Rate = emission_Rate;
            Life_Span = life_Span;
            Initial_Velocity = initial_Velocity;
            Gravity = gravity;
            Longitude = longtitude;
            Latitude = latitude;
            Visibility = visibility;
            Particle_Filename = particle_Filename;
            Emitter_Uses_MDL = emitter_Uses_MDL;
            Emitter_Uses_TGA = emitter_Uses_TGA;
        }
    }
    internal class Particle_Emitter_2
    {
        internal Particle_Emitter_2()
        {
        }
        internal w3Transformation Visibility { get; set; } = new w3Transformation(TransformationType.Visibility);
        internal w3Transformation Emission_Rate { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Speed { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Latitude { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Variation { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Width { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Length { get; set; } = new w3Transformation(TransformationType.Float);
        internal w3Transformation Gravity { get; set; } = new w3Transformation(TransformationType.Float);
        internal float[] Color_Segment1 { get; set; } = { 1, 1, 1 };
        internal float[] Color_Segment2 { get; set; } = { 1, 1, 1 };
        internal float[] Color_Segment3 { get; set; } = { 1, 1, 1 };
        internal int Texture_ID { get; set; } = -1;
        internal FilterMode Filter_Mode { get; set; } = FilterMode.None;
        //
        internal float ALpha_Segment1 { get; set; }
        internal float ALpha_Segment2 { get; set; }
        internal float ALpha_Segment3 { get; set; }
        internal float Scaling_Segment1 { get; set; }
        internal float Scaling_Segment2 { get; set; }
        internal float Scaling_Segment3 { get; set; }
        //----------------------------------
        internal float Head_Lifespan_Start { get; set; }
        internal float Head_Lifespan_End { get; set; }
        internal float Head_Lifespan_Repeat { get; set; }
        //----------------------------------
        internal float Head_Decay_Start { get; set; }
        internal float Head_Decay_End { get; set; }
        internal float Head_Decay_Repeat { get; set; }
        //----------------------------------
        internal float Tail_Lifespan_Start { get; set; }
        internal float Tail_Lifespan_End { get; set; }
        internal float Tail_Lifespan_Repeat { get; set; }
        //----------------------------------
        internal float Tail_Decay_Start { get; set; }
        internal float Tail_Decay_End { get; set; }
        internal float Tail_Decay_Repeat { get; set; }
        //misc
        internal int Rows { get; set; }
        internal int Columns { get; set; }
        internal float Life_Span { get; set; }
        internal float Tail_Length { get; set; }
        internal int Priority_Plane { get; set; }
        internal int Replaceable_ID { get; set; }
        internal float Time { get; set; }
        //tags
        internal bool Unshaded { get; set; } = false;
        internal bool Unfogged { get; set; } = false;
        internal bool Line_Emitter { get; set; } = false;
        internal bool Sort_Primitives_Far_Z { get; set; } = false;
        internal bool Model_Space { get; set; } = false;
        internal bool XY_Quad { get; set; } = false;
        internal bool Squirt { get; set; } = false;
        internal bool Head { get; set; } = false;
        internal bool Tail { get; set; } = false;
        internal bool AlphaKey { get; set; } = false;
        internal Particle_Emitter_2(
       w3Transformation visibility,
       w3Transformation emissionRate,
       w3Transformation speed,
       w3Transformation latitude,
       w3Transformation variation,
       w3Transformation width,
       w3Transformation length,
       w3Transformation gravity,
       float[] colorSegment1,
       float[] colorSegment2,
       float[] colorSegment3,
       int textureId,
       FilterMode filterMode,
       float alphaSegment1,
       float alphaSegment2,
       float alphaSegment3,
       float scalingSegment1,
       float scalingSegment2,
       float scalingSegment3,
       float headLifespanStart,
       float headLifespanEnd,
       float headLifespanRepeat,
       float headDecayStart,
       float headDecayEnd,
       float headDecayRepeat,
       float tailLifespanStart,
       float tailLifespanEnd,
       float tailLifespanRepeat,
       float tailDecayStart,
       float tailDecayEnd,
       float tailDecayRepeat,
       int rows,
       int columns,
       float lifeSpan,
       float tailLength,
       int priorityPlane,
       int replaceableId,
       float time,
       bool unshaded,
       bool unfogged,
       bool lineEmitter,
       bool sortPrimitivesFarZ,
       bool modelSpace,
       bool xqQuad,
       bool squirt,
       bool head,
       bool tail)
        {
            Visibility = visibility;
            Emission_Rate = emissionRate;
            Speed = speed;
            Latitude = latitude;
            Variation = variation;
            Width = width;
            Length = length;
            Gravity = gravity;
            Color_Segment1 = colorSegment1;
            Color_Segment2 = colorSegment2;
            Color_Segment3 = colorSegment3;
            Texture_ID = textureId;
            Filter_Mode = filterMode;
            ALpha_Segment1 = alphaSegment1;
            ALpha_Segment2 = alphaSegment2;
            ALpha_Segment3 = alphaSegment3;
            Scaling_Segment1 = scalingSegment1;
            Scaling_Segment2 = scalingSegment2;
            Scaling_Segment3 = scalingSegment3;
            Head_Lifespan_Start = headLifespanStart;
            Head_Lifespan_End = headLifespanEnd;
            Head_Lifespan_Repeat = headLifespanRepeat;
            Head_Decay_Start = headDecayStart;
            Head_Decay_End = headDecayEnd;
            Head_Decay_Repeat = headDecayRepeat;
            Tail_Lifespan_Start = tailLifespanStart;
            Tail_Lifespan_End = tailLifespanEnd;
            Tail_Lifespan_Repeat = tailLifespanRepeat;
            Tail_Decay_Start = tailDecayStart;
            Tail_Decay_End = tailDecayEnd;
            Tail_Decay_Repeat = tailDecayRepeat;
            Rows = rows;
            Columns = columns;
            Life_Span = lifeSpan;
            Tail_Length = tailLength;
            Priority_Plane = priorityPlane;
            Replaceable_ID = replaceableId;
            Time = time;
            Unshaded = unshaded;
            Unfogged = unfogged;
            Line_Emitter = lineEmitter;
            Sort_Primitives_Far_Z = sortPrimitivesFarZ;
            Model_Space = modelSpace;
            XY_Quad = xqQuad;
            Squirt = squirt;
            Head = head;
            Tail = tail;
        }
    }
    internal class Ribbon_Emitter
    {
        internal Ribbon_Emitter()
        {
        }
        internal w3Transformation Color { get; set; } = new w3Transformation(TransformationType.Color);
        internal w3Transformation Alpha { get; set; } = new w3Transformation(TransformationType.Alpha);
        internal w3Transformation Visibility { get; set; } = new w3Transformation(TransformationType.Visibility);
        internal w3Transformation Height_Above { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Height_Below { get; set; } = new w3Transformation(TransformationType.Int);
        internal w3Transformation Texture_Slot { get; set; } = new w3Transformation(TransformationType.Int);
        internal int Material_ID = -1;
       

        internal int Rows { get; set; } = 0;
        internal int Columns { get; set; } = 0;
        internal int Emission_Rate { get; set; } = 0;
        internal double Life_Span { get; set; } = 0;
        internal double Gravity { get; set; } = 0;
        internal Ribbon_Emitter(w3Transformation color, w3Transformation alpha, w3Transformation visibility,
                         w3Transformation height_Above, w3Transformation height_Below,
                         w3Transformation texture_Slot, int material_ID,
                         int rows, int columns, int emission_Rate, double life_Span, double gravity
                       )
        {
            Color = color;
            Alpha = alpha;
            Visibility = visibility;
            Height_Above = height_Above;
            Height_Below = height_Below;
            Texture_Slot = texture_Slot;
            Material_ID = material_ID;
            Rows = rows;
            Columns = columns;
            Emission_Rate = emission_Rate;
            Life_Span = life_Span;
            Gravity = gravity;
        }
    }

    #endregion
    #region TransformationObjectType
    static class TransformationObjectType
    {
        internal const string Emitter1 = "Emitter1";
        internal const string Emitter2 = "Emitter2";
        internal const string Emitter3 = "Emitter3";
        internal const string Light = "Light";
        internal const string Layer = "Layer";
        internal const string GeosetAnim = "GeosetAnim";
        internal const string TextureAnim = "TextureAnim";
        internal const string Camera = "Camera";
        internal const string Node = "Node";
        internal const string Position = "Position";
        internal const string Target = "Target";
        internal const string CameraRotation = "CameraRotation";
    }
    #endregion
    class ListComparer : IEqualityComparer<List<int>>
    {
        public bool Equals(List<int> x, List<int> y)
        {
            // Compare the two lists by checking if they are both non-null and contain the same elements
            if (x == null || y == null)
                return false;

            // Ensure that both lists have the same count and contain the same elements in the same order
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<int> obj)
        {
            // Generate a hash code by combining all elements in the list
            unchecked
            {
                int hash = 19;
                foreach (var item in obj)
                {
                    hash = hash * 31 + item.GetHashCode();
                }
                return hash;
            }
        }
    }
}
