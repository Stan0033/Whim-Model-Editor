using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Whim_GEometry_Editor.Dialogs;

namespace Whim_GEometry_Editor.Misc
{
    public static class AppSettings
    {
        public static float NearDistance = 1f;
        public static float FarDistance = 10000f;
        public static float FieldOfView = 45f;
        public static float ZoomIncrement = 1f;
        public static float RotateSpeed = 1f;
        public static bool BackfaceCullingClockwise = true;
        public static bool BackfaceCullingEnabled = false;

        public static float[] BackgroundColor = [0, 0, 0];
        public static float[] Color_Vertex = [1, 0, 1];
        public static float[] Color_VertexSelected = [1, 1, 0];
        public static float[] NodeColor = [1, 1, 0.5f];
        public static float[] NodeColorSelected = [1, 0.5f, 0.5f];
        public static float[] EdgeColor = [1, 0, 0];
        public static float[] EdgeColorSelected = [1, 0, 0];
        public static float[] GridColor = [1, 1, 1];
        public static float[] TriangleColorSelected = [1, 0, 0];
        public static float[] NormalsColor = [0, 1, 0];
        public static float[] RiggingColor = [1, 0.5f, 0];
        private static float[] RiggingColorSelected = [201, 74, 64];
        public static float[] ExtentsColor = [0, 0, 1];
        public static float[] SkeletonColor = [0.5f, 0, 0.5f];
        public static bool EnableLighting = true;
        public static AntialiasingTechnique AA = AntialiasingTechnique.None;
        internal static float[] AmbientColor = [0.5f, 0.5f, 0.5f];
        internal static float[] DiffuseColor = [0.75294117647f, 0.75294117647f, 0.75294117647f];
        internal static float[] SpecularColor = [0, 0, 0];

        internal static float[] LightPostion = [0.0f, 0.0f, 1.0f, 1.0f];
        internal static float[] MaterialDiffuseColor = [1.0f, 0.0f, 0.0f, 1.0f];
        internal static float[] MaterialSpecularColor = [1.0f, 1.0f, 1.0f, 1.0f];
        internal static float Shininess = 8f;
        internal static int AutoBackup = 0;

        public static int Autosave = 0;

        internal static bool HistoryEnabled = false;
        internal static int HistoryLimit = 200;
        internal static PointType PointType = PointType.Square;
        internal static float PointSize = 1f;
        internal static float[] Color_VertexRigged = [73, 97, 186];
        internal static float[] Color_VertexRiggedSelected = [86, 105, 179];

        public static void SetDefaults()
        {
            NearDistance = 0.5f;
            FarDistance = 5000f;
            FieldOfView = 45f;
            ZoomIncrement = 1f;
            RotateSpeed = 1f;
            BackfaceCullingClockwise = true;
            BackfaceCullingEnabled = false;

            BackgroundColor = [0, 0, 0];
            Color_Vertex = [1, 0, 1];
            Color_VertexSelected = [1, 1, 0];
            NodeColor = [1, 1, 0.5f];
            NodeColorSelected = [1, 1, 0];
            EdgeColor = [1, 0, 0];
            EdgeColorSelected = [1, 0, 0];
            GridColor = [1, 1, 1];
            TriangleColorSelected = [1, 0, 0];
            NormalsColor = [0, 1, 0];
            RiggingColor = [1, 0.5f, 0];
            RiggingColorSelected = [201, 74, 64];
            ExtentsColor = [0, 0, 1];
            SkeletonColor = [0.5f, 0, 0.5f];
            EnableLighting = true;
            AA = AntialiasingTechnique.None;
            AmbientColor = [0.5f, 0.5f, 0.5f];
            DiffuseColor = [0.75294117647f, 0.75294117647f, 0.75294117647f];
            SpecularColor = [0, 0, 0];

            LightPostion = [0.0f, 0.0f, 1.0f, 1.0f];
            MaterialDiffuseColor = [1.0f, 0.0f, 0.0f, 1.0f];
            MaterialSpecularColor = [1.0f, 1.0f, 1.0f, 1.0f];
            Shininess = 50.0f;
            AutoBackup = 0;

            Autosave = 0;

            HistoryEnabled = false;
            HistoryLimit = 200;
            PointType = PointType.Square;
            PointSize = 1f;
        }
        public static void LoadSettings()
        {
            string path = Path.Combine(AppHelper.DataPath, "Settings.txt");
            try
            {
                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines)
                    {
                        string[] data = line.Split("=");
                        if (data.Length == 2)
                        {
                            switch (data[0])
                            {
                                case nameof(NearDistance):
                                    NearDistance = Converters.SafeFloat(data[1], 0.5f);
                                    break;
                                case nameof(FarDistance):
                                    FarDistance = Converters.SafeFloat(data[1], 5000f);
                                    break;
                                case nameof(FieldOfView):
                                    FieldOfView = Converters.SafeFloat(data[1], 45f);
                                    break;
                                case nameof(ZoomIncrement):
                                    ZoomIncrement = Converters.SafeFloat(data[1], 1f);
                                    break;
                                case nameof(RotateSpeed):
                                    RotateSpeed = Converters.SafeFloat(data[1], 1f);
                                    break;
                                case nameof(BackfaceCullingClockwise):
                                    BackfaceCullingClockwise = Converters.SafeBool(data[1]);
                                    break;
                                case nameof(BackfaceCullingEnabled):
                                    BackfaceCullingEnabled = Converters.SafeBool(data[1]);
                                    break;
                                case nameof(BackgroundColor):
                                    BackgroundColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(Color_Vertex):
                                    Color_Vertex = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(Color_VertexSelected):
                                    Color_VertexSelected = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(NodeColor):
                                    NodeColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(NodeColorSelected):
                                    NodeColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(EdgeColor):
                                    EdgeColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(EdgeColorSelected):
                                    EdgeColorSelected = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(GridColor):
                                    GridColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(TriangleColorSelected):
                                    TriangleColorSelected = Converters.SafeFloatArray(data[1]);
                                   
                                    break;
                                case nameof(NormalsColor):
                                    NormalsColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(RiggingColor):
                                    RiggingColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(RiggingColorSelected):
                                    RiggingColorSelected = Converters.SafeFloatArray(data[1]);
                                    break;
                                    
                                case nameof(ExtentsColor):
                                    ExtentsColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(SkeletonColor):
                                    SkeletonColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(AmbientColor):
                                    AmbientColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(Color_VertexRigged):
                                    Color_VertexRigged = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(Color_VertexRiggedSelected):
                                    Color_VertexRiggedSelected = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(DiffuseColor):
                                    DiffuseColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(SpecularColor):
                                    SpecularColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(LightPostion):
                                    LightPostion = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(MaterialDiffuseColor):
                                    MaterialDiffuseColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(MaterialSpecularColor):
                                    MaterialSpecularColor = Converters.SafeFloatArray(data[1]);
                                    break;
                                case nameof(EnableLighting):
                                    EnableLighting = Converters.SafeBool(data[1]);
                                    break;
                                case nameof(Shininess):
                                    Shininess = Converters.SafeFloat(data[1]);
                                    break;
                                
                                case nameof(AA):
                                    AA = (AntialiasingTechnique)Enum.Parse(typeof(AntialiasingTechnique), data[1]);
                                    break;
                                case nameof(Autosave):
                                    Autosave = Converters.SafeInt(data[1], 0); break;
                                case nameof(AutoBackup):
                                    AutoBackup = Converters.SafeInt(data[1], 0);
                                    break;
                                case nameof(HistoryEnabled):
                                    HistoryEnabled = Converters.SafeBool(data[1]); break;
                                case nameof(HistoryLimit):
                                    HistoryLimit = Converters.SafeInt(data[1], 0); break;
                                case nameof(PointSize):
                                    PointSize = Converters.SafeFloat(data[1], 1); break;
                                case nameof(PointType):
                                    PointType = data[1] == PointType.Square.ToString() ? PointType.Square : PointType.Triangle;
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error while reading the settings. File has invalid formatting.", "Can't read settings"); return;
            }
        }
        public static void SaveSettings()
        {
            string path = Path.Combine(AppHelper.DataPath, "Settings.txt");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{nameof(NearDistance)}={NearDistance}");
            sb.AppendLine($"{nameof(FarDistance)}={FarDistance}");
            sb.AppendLine($"{nameof(FieldOfView)}={FieldOfView}");
            sb.AppendLine($"{nameof(ZoomIncrement)}={ZoomIncrement}");
            sb.AppendLine($"{nameof(RotateSpeed)}={RotateSpeed}");
            sb.AppendLine($"{nameof(BackfaceCullingEnabled)}={BackfaceCullingEnabled}");
            sb.AppendLine($"{nameof(BackfaceCullingClockwise)}={BackfaceCullingClockwise}");
            sb.AppendLine($"{nameof(BackgroundColor)}={string.Join(",", BackgroundColor)}");
            sb.AppendLine($"{nameof(Color_Vertex)}={string.Join(",", Color_Vertex)}");
            sb.AppendLine($"{nameof(Color_VertexSelected)}={string.Join(",", Color_VertexSelected)}");
            sb.AppendLine($"{nameof(Color_VertexRigged)}={string.Join(",", Color_VertexRigged)}");
            sb.AppendLine($"{nameof(Color_VertexRiggedSelected)}={string.Join(",", Color_VertexRiggedSelected)}");
            sb.AppendLine($"{nameof(NodeColor)}={string.Join(",", NodeColor)}");
            sb.AppendLine($"{nameof(NodeColorSelected)}={string.Join(",", NodeColorSelected)}");
            sb.AppendLine($"{nameof(EdgeColor)}={string.Join(",", EdgeColor)}");
            sb.AppendLine($"{nameof(EdgeColorSelected)}={string.Join(",", EdgeColorSelected)}");
            sb.AppendLine($"{nameof(GridColor)}={string.Join(",", GridColor)}");
            sb.AppendLine($"{nameof(TriangleColorSelected)}={string.Join(",", TriangleColorSelected)}");
            sb.AppendLine($"{nameof(NormalsColor)}={string.Join(",", NormalsColor)}");
            sb.AppendLine($"{nameof(RiggingColor)}={string.Join(",", RiggingColor)}");
            sb.AppendLine($"{nameof(ExtentsColor)}={string.Join(",", ExtentsColor)}");
            sb.AppendLine($"{nameof(SkeletonColor)}={string.Join(",", SkeletonColor)}");

            sb.AppendLine($"{nameof(EnableLighting)}={EnableLighting}");
            sb.AppendLine($"{nameof(AA)}={AA}");
            sb.AppendLine($"{nameof(AA)}={AA}");
            sb.AppendLine($"{nameof(AmbientColor)}={string.Join(",", AmbientColor)}");
            sb.AppendLine($"{nameof(DiffuseColor)}={string.Join(",", DiffuseColor)}");
            sb.AppendLine($"{nameof(SpecularColor)}={string.Join(",", SpecularColor)}");
            sb.AppendLine($"{nameof(LightPostion)}={string.Join(",", LightPostion)}");
            sb.AppendLine($"{nameof(MaterialDiffuseColor)}={string.Join(",", MaterialDiffuseColor)}");
            sb.AppendLine($"{nameof(MaterialSpecularColor)}={string.Join(",", MaterialSpecularColor)}");
            sb.AppendLine($"{nameof(Shininess)}={Shininess}");
            sb.AppendLine($"{nameof(HistoryEnabled)}={HistoryEnabled}");
            sb.AppendLine($"{nameof(HistoryLimit)}={HistoryLimit}");
            sb.AppendLine($"{nameof(PointSize)}={PointSize}");
            sb.AppendLine($"{nameof(PointType)}={PointType}");

            File.WriteAllText(path, sb.ToString());
        }
    }
}
