using MDLLib;
using MDLLibs.Classes.Misc;
using MdxLib.Model;

using Microsoft.Win32;
using SharpGL;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;

using System.IO;

using System.Numerics;

using System.Text;
using System.Timers;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

using test_parser_mdl;
using W3_Texture_Finder;

using Whim_GEometry_Editor;
using Whim_GEometry_Editor.Dialogs;
using Whim_GEometry_Editor.MDL_Parser;
using Whim_GEometry_Editor.Misc;
using Whim_GEometry_Editor.Node_Editors;
using static test_parser_mdl.Parser_MDL;

using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Whim_Model_Editor
{
    public static class DisplayOptions
    {
        public static bool Axis = true;
        public static bool Grid = true;
        public static bool Shading = true;
        public static bool Normals = false;
        public static bool Edges = false;
        public static bool Extents = false;
        public static bool Rigging = false;
        public static bool Skeleton = false;
        public static bool Textures = true;

        public static bool Nodes = false;


        internal static bool CollishionShapes = false;
        internal static bool Wireframe = false;

        public static bool Vertices = false;
        public static bool Points = false;
        public static bool Triangles = true;
        internal static bool GridXZ = false;
        internal static bool GridYZ = false;
    }
    public enum SelectionMode
    {
        ClearAndSelect,
        AddSelect,
        RemoveSelect
    }
    public enum EditMode
    {
        Vertices,
        Edges,
        Triangles,
        Geosets,
        Normals,
        Sculpt,
        UV,
        Rigging
            ,
        Animator,
        Nodes
    }
    public enum AxisMode
    {
        X, Y, Z, U,
        Facing, None
    }
    public enum ModifyMode
    {
        None,
        Translate,
        Rotate,
        Scale,
        Inset,

        Extrude, // move 
        ExtrudeEach, // move 
        Extract,  // move 
        extractEach,// move 
        extracteachNewGeoset,  // move 
        ExtractGeoset, // move 
        RotateNormals,
        Extend, // move 
        Widen,
        Narrow,
        Bevel,
        Curve,
        DetachGeoset, // move 
        Expand,
        ExpandTriangles,
        ExpandEdges,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //-------------------------------------------------------
        //--app state
        //-------------------------------------------------------

        CMessageBox CBox;
        List<w3Line> SelectionExtentLines = new List<w3Line>();
        string SaveLocation = "";
        private bool Saved = true;
        w3Model CurrentModel = new w3Model();
        w3Model CurrentModelAnimated = new w3Model();
        List<w3Geoset> ModifiedGeometryForTrack = new List<w3Geoset>();
        string DefaultSaveLocation;
        OpenGL Scene_GL;
        System.Timers.Timer AdditionalTimer;
        int LineSpacing = 10;
        int GridSize = 100;
        public int CurrentSceneFrame = -1;
        public bool playingAnimation = false;
        public float ModifyAmount = 1;
        public Coordinate CurrentRotateCentroid = new Coordinate();
        public Coordinate Center = new Coordinate();

        System.Timers.Timer AutoSaveTimer = new System.Timers.Timer();
        System.Timers.Timer BackupTimer = new System.Timers.Timer();

        private float WorldSnapSpacing = 1;
        int CopiedFrame = -1;
        bool CutFrame_ = false;
        MDLX_Window MDLX_Window_INSTANCE = new MDLX_Window();
        BitmapSource UV_CurrentImage;
        //------------------------------------------------------
        //------------------------------------------------------
        bool SelectingSquare = false;

        int CurrentHistoryIndex = 0; //unfinished
        List<w3Geoset> SelectedGeosets = new List<w3Geoset>(); // vertices and normals
        List<w3Vertex> SelectedVertices = new List<w3Vertex>();
        List<w3Triangle> SelectedTriangles = new List<w3Triangle>(); // triangle and uv mapping
        List<w3Triangle> CurrentInsetCollection = new List<w3Triangle>(); // triangle and uv mapping
        List<w3Edge> SelectedEdges = new List<w3Edge>(); // triangle and uv mapping
        List<w3Triangle> TrianglesToExtract = new List<w3Triangle>();
        w3Node CurrentlySelectedNode = new w3Node();
        //------------------------------------------------------
        //------------------------------------------------------
        EditMode editMode_current = EditMode.Vertices;
        ModifyMode modifyMode_current = ModifyMode.None;
        AxisMode axisMode = AxisMode.X;
        UVWorkMode uVWorkMode = UVWorkMode.Move;
        SelectionMode selectionMode = SelectionMode.ClearAndSelect;

        Dictionary<string, BitmapImage> Icons;
        Dictionary<int, BitmapImage> Path_Texture; // store unique textures here to avoid browsing the MPQ - id, image
        Dictionary<int, BitmapImage> Geoset_Textures; // store unique textures here to avoid browsing th MPQ - geosetid, image

        ContextMenu AxisDetail = new ContextMenu();
        List<string> ColorPaths = new List<string>();
        ContextMenu MainMenu;
        CustomCamera Camera_Editor;
        public MainWindow()
        {
            InitializeComponent();
            InitializeEssentials();

        }
        public MainWindow(string filepath)
        {
            InitializeComponent();
            InitializeEssentials();

            if (filepath.Length > 0)
            {
                if (System.IO.Path.GetExtension(filepath).ToLower() == ".mdl" || System.IO.Path.GetExtension(filepath).ToLower() == ".mdx")
                {
                    OpenModel(filepath);

                }

            }
        }


        //-----------------------------------

        private void SwitchSelectButtonsGeometry()
        {
            ButtonSelectNextVertex.IsEnabled = editMode_current == EditMode.Vertices;
            ButtonSelectPrevVertex.IsEnabled = editMode_current == EditMode.Vertices;
            ButtonSelectNextTriangle.IsEnabled = editMode_current == EditMode.Triangles;
            ButtonSelectPrevTriangle.IsEnabled = editMode_current == EditMode.Triangles;
            ButtonSelectNextEdge.IsEnabled = editMode_current == EditMode.Edges;
            ButtonSelectPrevEdge.IsEnabled = editMode_current == EditMode.Edges;
        }
        private static class UVEditor_Values
        {
            public static int gridSize = 0;
            public static Bitmap Imageused;
            public static int ZoomAmount = 0; // from -10 to 10
            public static List<w3Triangle> SelectedTriangles = new List<w3Triangle>();
            public static Bitmap DisplayedImage;
            public static void SetBitmapToBlack()
            {

            }


        }
        private void RefreshUVTextureList(int selectedID = -1)
        {
            Combo_UVTextures.Items.Clear();
            Dictionary<int, string> paths = new Dictionary<int, string>();
            for (int i = 0; i < CurrentModel.Textures.Count; i++)
            {
                if (CurrentModel.Textures[i].Replaceable_ID == 0)
                {
                    paths.Add(i, CurrentModel.Textures[i].Path);
                }
            }
            if (paths.Count > 0)
            {
                foreach (var path in paths)
                {
                    Combo_UVTextures.Items.Add($"[{path.Key}] {path.Value} ");
                }
            }
            if (selectedID != -1)
            {
                string path = CurrentModel.Textures.First(x => x.ID == selectedID).Path; ;
                int index = paths.First(x => x.Value == path).Key;
                Combo_UVTextures.SelectedIndex = index;
            }

        }
        private void InitializeEssentials()
        {
            // constructor

            Filef.CreateFileAndDirectory(AppHelper.TempMDLLocation);
            CBox = new();
            MainMenu = Scene_Canvas_.ContextMenu;
            InitializeAxisDetailMenu();
            InitIcons();
            EventObjectHelper.Initialize();
            RefreshTitle();
            SetupMPQs();
            ModelHelper.Current = CurrentModel;
            Scene_GL = Scene_Viewport.OpenGL;
            DefaultSaveLocation = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultModel.mdl");
            Camera_Editor = new CustomCamera();
            AdditionalTimer = new();
            AdditionalTimer.Interval = 1000;
            AdditionalTimer.Elapsed += ElapsedAdditionalTimer;
            HAndleRecentFiles();
            AppSettings.LoadSettings();
            LightingCheckBox.IsChecked = AppSettings.EnableLighting;
            //timers
            //-----------------------------------

            BackupTimer.Enabled = false;
            AutoSaveTimer.Enabled = false;
            BackupTimer.Elapsed += AutoBackup;
            AutoSaveTimer.Elapsed += AutoSave;
            if (AppSettings.Autosave > 0)
            {

                AutoSaveTimer.Interval = AppSettings.Autosave * 60000;
                AutoSaveTimer.Enabled = true;
                AutoSaveTimer.Start();
            }
            if (AppSettings.AutoBackup > 0)
            {
                BackupTimer.Interval = AppSettings.AutoBackup * 60000;
                BackupTimer.Enabled = true;
                BackupTimer.Start();
            }
            if (AppSettings.HistoryLimit == 0)
            {
                ButtonUndo.IsEnabled = false;
                ButtonRedo.IsEnabled = false;
            }
        }

        private void ElapsedAdditionalTimer(object? sender, ElapsedEventArgs e)
        {
            AdditionalTimer.Stop();
            test(null, null);
        }

        private void AutoSave(object? sender, ElapsedEventArgs e)
        {
            CurrentModel.Name = InputModelName.Text.Trim();

            CurrentModel.CalculateExtents();
            File.WriteAllText(SaveLocation, CurrentModel.ToMDL());

            Saved = true;
            RefreshTitle();
        }

        private void AutoBackup(object? sender, ElapsedEventArgs e)
        {
            CurrentModel.Name = InputModelName.Text.Trim();
            string backupLocation = GetFilePathWithTime(SaveLocation);
            CurrentModel.CalculateExtents();
            File.WriteAllText(backupLocation, CurrentModel.ToMDL());
        }


        private void RefreshBitmaps()
        {


            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                int mid = geo.Material_ID;
                if (CurrentModel.Materials.Any(x => x.ID == mid))
                {
                    w3Material mat = CurrentModel.Materials.First(x => x.ID == mid);
                    if (mat.Layers.Count > 0)
                    {
                        if (mat.Layers[0].Diffuse_Texure_ID.isStatic)
                        {
                            int lid = (int)mat.Layers[0].Diffuse_Texure_ID.StaticValue[0];
                            if (CurrentModel.Textures.Any(x => x.ID == lid))
                            {
                                w3Texture tx = CurrentModel.Textures.First(x => x.ID == lid);

                                if (tx.Replaceable_ID == 0)
                                {
                                    if (tx.Path.Length == 0) { continue; }
                                    geo.UsedBitmap = MPQHelper.GetImage(tx.Path);
                                    geo.UsedTexture = new SharpGL.SceneGraph.Assets.Texture();
                                    geo.UsedTexture.Create(Scene_GL, geo.UsedBitmap);
                                }
                                else if (tx.Replaceable_ID == 1)
                                {
                                    string path = "ReplaceableTextures\\TeamColor\\TeamColor00.blp";
                                    geo.UsedBitmap = MPQHelper.GetImage(path);
                                    geo.UsedTexture = new SharpGL.SceneGraph.Assets.Texture();
                                    geo.UsedTexture.Create(Scene_GL, geo.UsedBitmap);
                                }


                                else if (tx.Replaceable_ID == 2)
                                {
                                    string path = "ReplaceableTextures\\TeamGlow\\TeamGlow00.blp";
                                    geo.UsedBitmap = MPQHelper.GetImage(path);
                                    geo.UsedTexture = new SharpGL.SceneGraph.Assets.Texture();
                                    geo.UsedTexture.Create(Scene_GL, geo.UsedBitmap);
                                }

                            }
                        }
                    }

                }
            }

            //  foreach (w3Geoset geo in CurrentModel.Geosets)  {  if (geo.UsedTexture != null) { geo.UsedTexture.Bind(Scene_GL); }  }



        }
        private void SetupMPQs()
        {
            MPQFinder.Find();
            MPQHelper.Initialize();
        }

        private void EnableContextMenu(bool enable)
        {

            if (enable)
            {
                Scene_Canvas_.ContextMenu = MainMenu;
            }
            else
            {
                Scene_Canvas_.ClearValue(Canvas.ContextMenuProperty);
            }

        }
        private static BitmapImage LoadImage(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new System.Uri(uri, System.UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }
        private void InitIcons()
        {
            Icons = new Dictionary<string, BitmapImage>()
        {
            {NodeType.Bone, LoadImage("pack://application:,,,/Images/bone.png") },
            {NodeType.Attachment, LoadImage("pack://application:,,,/Images/attach.png") },
            {NodeType.Collision_Shape, LoadImage("pack://application:,,,/Images/cols.png") },
            {NodeType.Helper, LoadImage("pack://application:,,,/Images/info.png") },
            {NodeType.Light, LoadImage("pack://application:,,,/Images/light.png") },
            {NodeType.Emitter1, LoadImage("pack://application:,,,/Images/emitter1.png") },
            {NodeType.Emitter2, LoadImage("pack://application:,,,/Images/emitter2.png") },
            {NodeType.Ribbon, LoadImage("pack://application:,,,/Images/emitter3.png") },
            {NodeType.Event, LoadImage("pack://application:,,,/Images/event.png") },

        };
        }
        private void InitColorPaths()
        {
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor00.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor01.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor02.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor03.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor04.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor05.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor06.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor07.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor08.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor09.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor10.blp");
            ColorPaths.Add("ReplaceableTextures\\TeamColor\\TeamColor11.blp");
            ColorPaths.Add("Textures\\white.blp");
        }
        private void RecalculateVisibleGeosets()
        {
            foreach (object item in List_Geosets.Items)
            {
                ListBoxItem i = item as ListBoxItem;
                CheckBox c = i.Content as CheckBox;
                bool C_hecked = c.IsChecked == true;
                string name = c.Content.ToString();
                CurrentModel.Geosets.First(x => x.Name == name).isVisible = C_hecked;

            }
        }
        private void InitializeAxisDetailMenu()
        {
            MenuItem x = new MenuItem();
            x.Header = "X";
            x.Click += SetAxisModeX;
            MenuItem y = new MenuItem();
            y.Header = "Y";
            y.Click += SetAxisModeY;
            MenuItem z = new MenuItem();
            z.Header = "Z";
            z.Click += SetAxisModeZ;
            MenuItem f = new MenuItem();
            f.Header = "Facing";
            f.Click += SetAxisModeZ;
            MenuItem u = new MenuItem();
            u.Header = "Uniform";
            u.Click += SetAxisModeU;
            AxisDetail.Items.Add(x);
            AxisDetail.Items.Add(y);
            AxisDetail.Items.Add(z);
            AxisDetail.Items.Add(f);

            AxisDetail.Items.Add(u);
        }

        private void SetAxisModeY(object sender, RoutedEventArgs e)
        {
            axisMode = AxisMode.Y; CurrentSceneInteraction = SceneInteractionState.Modify;
        }

        private void SetAxisModeU(object sender, RoutedEventArgs e)
        {
            axisMode = AxisMode.U; CurrentSceneInteraction = SceneInteractionState.Modify;
        }

        private void CallDetailsMenu(bool x, bool y, bool z, bool f, bool u)
        {
            System.Windows.Point mousePosition = Mouse.GetPosition(Application.Current.MainWindow);
            AxisDetail.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            AxisDetail.IsOpen = true;

            MenuItem itemX = AxisDetail.Items[0] as MenuItem;
            itemX.IsEnabled = x;
            MenuItem itemY = AxisDetail.Items[1] as MenuItem;
            itemY.IsEnabled = y;
            MenuItem itemZ = AxisDetail.Items[2] as MenuItem;
            itemZ.IsEnabled = z;
            MenuItem facing = AxisDetail.Items[3] as MenuItem;
            facing.IsEnabled = f;
            MenuItem itemUniform = AxisDetail.Items[4] as MenuItem;
            itemUniform.IsEnabled = u;
        }



        private void SetAxisModeZ(object sender, RoutedEventArgs e)
        {
            axisMode = AxisMode.Z; CurrentSceneInteraction = SceneInteractionState.Modify;
        }

        private void SetAxisModeX(object sender, RoutedEventArgs e)
        {
            axisMode = AxisMode.X;

            CurrentSceneInteraction = SceneInteractionState.Modify;

        }

        private void CallAxisDetailMenu()
        {

            // Get the current mouse position relative to the window
            System.Windows.Point mousePosition = Mouse.GetPosition(Application.Current.MainWindow);

            // Set the position where the ContextMenu will open
            AxisDetail.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            AxisDetail.PlacementTarget = Application.Current.MainWindow; // Set the target to the window or appropriate control
            AxisDetail.HorizontalOffset = mousePosition.X;
            AxisDetail.VerticalOffset = mousePosition.Y;

            // Open the menu
            AxisDetail.IsOpen = true;

        }


        private void SetModeNodes(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Visible;
            LabelNodes.Visibility = Visibility.Visible;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            editMode_current = EditMode.Nodes;
            Manual.Visibility = Visibility.Visible;
            Scene_Viewport.Height = ActualHeight;
            EnableContextMenu(true);
            CurrentSceneInteraction = SceneInteractionState.None;
            SetActive(Button_Bones);
            SetContextMenuState(EditMode.Nodes);
            Unrig();
            SetManualAvailability(true, false, false);
        }


        private void SetActive(Button b)
        {
            Button_Vertices.BorderBrush = Brushes.Black;
            Button_Sculpt.BorderBrush = Brushes.Black;
            Button_Edge.BorderBrush = Brushes.Black;
            Button_Triangles.BorderBrush = Brushes.Black;
            Button_Geosets.BorderBrush = Brushes.Black;
            Button_Geometry.BorderBrush = Brushes.Black;
            Button_Normals.BorderBrush = Brushes.Black;
            Button_Bones.BorderBrush = Brushes.Black;
            Button_UV.BorderBrush = Brushes.Black;
            Button_Rigging.BorderBrush = Brushes.Black;
            Button_Animator.BorderBrush = Brushes.Black;



            b.BorderBrush = Brushes.Blue;
        }
        private void RefreshNodesList()
        {
            List_Nodes.Items.Clear();

            // Create a HashSet to track visited nodes
            HashSet<int> visitedNodes = new HashSet<int>();

            foreach (w3Node node in CurrentModel.Nodes)
            {
                bool hasAnimations = node.Translation.Keyframes.Count > 0 || node.Scaling.Keyframes.Count > 0 || node.Rotation.Keyframes.Count > 0;
                if (node.parentId == -1)
                {
                    TreeViewItem treeViewItem = NewTreeItem(node.Name, node.Data.GetType().Name, hasAnimations);

                    // Start adding children with the visited nodes set
                    AddChildren(node.objectId, treeViewItem, visitedNodes);

                    List_Nodes.Items.Add(treeViewItem);
                }
            }
            LabelNodes.Text = $"Nodes {CurrentModel.Nodes.Count}";

        }

        private void AddChildren(int nodeID, TreeViewItem item, HashSet<int> visitedNodes)
        {
            // Add the current node ID to the visited set
            visitedNodes.Add(nodeID);

            foreach (w3Node node in CurrentModel.Nodes)
            {
                // Check if the node is a child and hasn't been visited
                if (node.parentId == nodeID && !visitedNodes.Contains(node.objectId))
                {
                    bool hasAnimations = node.Translation.Keyframes.Count > 0 || node.Scaling.Keyframes.Count > 0 || node.Rotation.Keyframes.Count > 0;

                    TreeViewItem childItem = NewTreeItem(node.Name, node.Data.GetType().Name, hasAnimations);
                    item.Items.Add(childItem);

                    // Recursively add children with the updated visitedNodes set
                    AddChildren(node.objectId, childItem, visitedNodes);
                }
            }
        }



        private void SetModeVertices(object sender, RoutedEventArgs e)
        {
            editMode_current = EditMode.Vertices;
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Visible;
            Label_Geosets.Visibility = Visibility.Visible;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            Manual.Visibility = Visibility.Visible;
            Field_ModifyAmount.Visibility = Visibility.Visible;
            Field_WorldMatrix.Visibility = Visibility.Visible;
            EnableDisableUndoRedo();
            Unrig();
            SetManualAvailability(true, true, true);
            SwitchSelectButtonsGeometry();
            CurrentSceneInteraction = SceneInteractionState.None;
             
            SetManualAvailability(true, true, true);

            EnableContextMenu(true);
            SetActive(Button_Vertices);
            SetContextMenuState(EditMode.Vertices);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle tr in geo.Triangles)
                {
                    tr.isSelected = false;
                }
                foreach (w3Edge edge in geo.Edges) { edge.isSelected = false; }
            }
        }



        private void SetModeEdges(object sender, RoutedEventArgs e)
        {
            editMode_current = EditMode.Edges;
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Visible;
            Label_Geosets.Visibility = Visibility.Visible;
            Manual.Visibility = Visibility.Visible;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Visible;
            Field_WorldMatrix.Visibility = Visibility.Visible;
            SetManualAvailability(true, true, true);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Edge tr in geo.Edges) tr.SelectIf();
            }
            EnableDisableUndoRedo();
            Unrig();
            SwitchSelectButtonsGeometry();
            SetActive(Button_Edge);
            EnableContextMenu(true);
            SetContextMenuState(EditMode.Edges);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Edge edge in geo.Edges)
                {
                    if (edge.Vertex1.isSelected || edge.Vertex2.isSelected) { edge.isSelected = true; }
                }
            }
            CurrentSceneInteraction = SceneInteractionState.None;
        }

        private void SetModeTriangles(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Visible;
            Label_Geosets.Visibility = Visibility.Visible;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            Manual.Visibility = Visibility.Visible;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            CurrentSceneInteraction = SceneInteractionState.None;
            editMode_current = EditMode.Triangles;
            Field_ModifyAmount.Visibility = Visibility.Visible;
            Field_WorldMatrix.Visibility = Visibility.Visible;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle tr in geo.Triangles) tr.SelectIf();
                foreach (w3Edge edge in geo.Edges) { edge.isSelected = false; }
            }
            EnableDisableUndoRedo();
            SetActive(Button_Triangles);
            EnableContextMenu(true);
            SetContextMenuState(EditMode.Triangles);
            Unrig();
            SwitchSelectButtonsGeometry();
            SetManualAvailability(true, true, true);
        }

        private void SetModeGeosets(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Visible;
            Label_Geosets.Visibility = Visibility.Visible;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            editMode_current = EditMode.Geosets;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Visible;
            Field_WorldMatrix.Visibility = Visibility.Visible;
            EnableDisableUndoRedo();
            SetManualAvailability(true, true, true);
            CurrentSceneInteraction = SceneInteractionState.None;
            Manual.Visibility = Visibility.Visible;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Vertices.Any(x => x.isSelected))
                {
                    if (geo.isVisible)
                    {
                        foreach (w3Triangle tr in geo.Triangles) { tr.isSelected = true; }
                    }
                }
            }
            EnableContextMenu(true);

            SetActive(Button_Geosets);
            SetContextMenuState(EditMode.Geosets);
            Unrig();
        }

        private void SetModeNormals(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Visible;
            Label_Geosets.Visibility = Visibility.Visible;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Collapsed;
            Field_WorldMatrix.Visibility = Visibility.Collapsed;
            editMode_current = EditMode.Normals;
            EnableContextMenu(true);
            CurrentSceneInteraction = SceneInteractionState.None;
            EnableDisableUndoRedo();
            SetActive(Button_Normals);
            Manual.Visibility = Visibility.Visible;
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            SetContextMenuState(EditMode.Normals);
            SetManualAvailability(false, true, false);



        }
        private ListBoxItem GetItem(ListBox box) { return box.SelectedItem as ListBoxItem; }
        private TreeViewItem GetITem(TreeView box) { return box.SelectedItem as TreeViewItem; }
        private void HideAllGeosets(object sender, RoutedEventArgs e)
        {
            foreach (object item in List_Geosets.Items)
            {
                ListBoxItem i = item as ListBoxItem;
                CheckBox c = i.Content as CheckBox;
                c.IsChecked = false;


            }

        }
        private void InvertGeosetsCheck(object sender, RoutedEventArgs e)
        {
            foreach (object item in List_Geosets.Items)
            {
                ListBoxItem i = item as ListBoxItem;
                CheckBox c = i.Content as CheckBox;
                c.IsChecked = !c.IsChecked;
            }

        }

        private void UnhideAllGeosets(object sender, RoutedEventArgs e)
        {
            foreach (object item in List_Geosets.Items)
            {
                ListBoxItem i = item as ListBoxItem;
                CheckBox c = i.Content as CheckBox;
                c.IsChecked = true;
            }

        }

        private void SetTextureWhite(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count < 1) { return; }
            List<w3Geoset> geosets = GetSelectedGeosets();
            CreateWhiteTexture();
            w3Material m = CreateWhiteMaterial();

            foreach (w3Geoset g in geosets)
            {
                g.Material_ID = m.ID;
                g.SetTexure("Textures\\white.blp");
            }




        }

        private List<w3Geoset> GetSelectedGeosets()
        {
            List<w3Geoset> geosets = new List<w3Geoset>();
            foreach (object selected in List_Geosets.SelectedItems)
            {
                ListBoxItem i = selected as ListBoxItem;
                CheckBox c = i.Content as CheckBox;
                string name = c.Content as string;
                int id = int.Parse(name);
                geosets.Add(CurrentModel.Geosets.First(x => x.ID == id));
            }
            return geosets;
        }

        private void CreateWhiteTexture()
        {
            bool white = CurrentModel.Textures.Any(x => x.Path == "Textures\\White.blp");
            if (!white)
            {
                w3Texture t = new w3Texture();
                t.Path = "Textures\\White.blp";
                t.ID = IDCounter.Next();
                CurrentModel.Textures.Add(t);
            }

        }
        private int GetWhiteID()
        {
            return CurrentModel.Textures.First(x => x.Path == "Textures\\White.blp").ID;
        }
        private w3Material CreateWhiteMaterial()
        {
            bool white = false;
            int wid = GetWhiteID();
            foreach (w3Material m in CurrentModel.Materials)
            {
                if (m.Layers[0].Diffuse_Texure_ID.StaticValue[0] == wid)
                {
                    return m;
                }
            }
            if (!white)
            {
                w3Material mat = new w3Material();
                w3Layer l = new w3Layer();
                l.ID = IDCounter.Next();
                l.Diffuse_Texure_ID.StaticValue = [wid];
                mat.Layers.Add(l);
                mat.ID = IDCounter.Next();
                CurrentModel.Materials.Add(mat);
                return mat;
            }
            throw new Exception("This should not be reached");
        }



        private void Merge(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count < 2)
            {
                MessageBox.Show("Select at least two geosets", "Invalid request");
                return;
            }

            List<w3Geoset> SelectedGeosets = GetSelectedGeosets();



            w3Geoset first = SelectedGeosets[0];
            first.isVisible = true;


            for (int i = 1; i < SelectedGeosets.Count; i++)
            {
                first.Vertices.AddRange(SelectedGeosets[i].Vertices);
                first.Triangles.AddRange(SelectedGeosets[i].Triangles);
                CurrentModel.Geosets.Remove(SelectedGeosets[i]);
            }



            RefreshGeosetList();
        }


        private void MergeSimilar(object sender, RoutedEventArgs e)
        {
            Dictionary<int, List<w3Geoset>> UseList = new Dictionary<int, List<w3Geoset>>();
            List<w3Geoset> newList = new List<w3Geoset>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                int mid = geo.Material_ID;
                if (UseList.ContainsKey(mid))
                {
                    UseList[mid].Add(geo);
                }
                else
                {
                    UseList.Add(mid, new List<w3Geoset>() { geo });
                }
            }
            if (UseList.Any(x => x.Value.Count > 1))
            {


                foreach (var item in UseList)
                {

                }
                CurrentModel.Geosets = newList;
                RefreshGeosetList();
            }
            else
            {
                MessageBox.Show("There aren't any mergeable geosets.");
            }

        }
        private void RefreshGeosetList()
        {
            List_Geosets.Items.Clear();
            foreach (w3Geoset g in CurrentModel.Geosets)
            {
                CheckBox c = new CheckBox();
                c.Content = g.ID.ToString();
                c.IsChecked = g.isVisible;
                c.Checked += ChechedGeoset;
                c.Unchecked += ChechedGeoset;
                c.ToolTip = $"Used Material ID: {g.Material_ID}";
                List_Geosets.Items.Add(new ListBoxItem() { Content = c });
            }
            Label_Geosets.Text = $"Geosets ({CurrentModel.Geosets.Count}):";
        }
        private void ChechedGeoset(object sender, RoutedEventArgs e)
        {
            int id = int.Parse((sender as CheckBox).Content.ToString());
            bool chked = (sender as CheckBox).IsChecked == true;
            w3Geoset geo = CurrentModel.Geosets.First(X => X.ID == id);
            geo.isVisible = chked;
            if (!chked)
            {

                foreach (w3Vertex v in geo.Vertices) v.isSelected = false;
                foreach (w3Triangle v in geo.Triangles) v.isSelected = false;
                foreach (w3Edge v in geo.Edges) v.isSelected = false;
            }

        }
        private void RenameGeoset(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count != 1) { return; }
            ListBoxItem item = GetItem(List_Geosets);
            CheckBox c = item.Content as CheckBox;
            string name = c.Content.ToString();

            UserInput i = new UserInput();
            i.Title = "New name";
            i.Box.Text = name;
            if (i.ShowDialog() == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Geosets.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is already a geoset with this name", "Changes not made");

                }
                else
                {
                    CurrentModel.Geosets.First(x => x.Name == name).Name = name2;
                    c.Content = name2;

                }

            }

        }
        private w3Geoset GetGeosetByID(int id)
        {
            return CurrentModel.Geosets.First(x => x.ID == id);
        }
        private int GetSelectedGeosetByItem(object o)
        {
            ListBoxItem i = o as ListBoxItem;
            CheckBox c = i.Content as CheckBox;
            string s = c.Content as string;
            return int.Parse(s);
        }
        private void DelGeoset(object sender, RoutedEventArgs e)
        {
            
            if (List_Geosets.SelectedItems.Count > 0)
            {
                
                foreach (object item in List_Geosets.SelectedItems)
                {
                    int id = GetSelectedGeosetByItem(item);
                    w3Geoset currentGeoset = GetGeosetByID(id);

                    CurrentModel.Geosets.Remove(currentGeoset);
                    CurrentModel.Geoset_Animations.RemoveAll(x => x.Geoset_ID == id);

                }
                var selectedItems = List_Geosets.SelectedItems.Cast<object>().ToList();
                foreach (object item in selectedItems)
                {
                    List_Geosets.Items.Remove(item);

                }
                CheckAskFreedBones();

            }
        }
        private void CheckAskFreedBones()
        {

            List<w3Node> NodesToRemove = new List<w3Node>();
            List<w3Node> NodesToConvert = new List<w3Node>();
            foreach (w3Node node in CurrentModel.Nodes.Where(x => x.Data is Bone))
            {
                {
                    if (NodeHasAttachedVertices(node.objectId) == false)
                    {
                        int parent = node.parentId;
                        if (parent == -1)
                        {
                            MessageBoxResult result = MessageBox.Show($"Bone \'{node.Name}\' is now free. Remove?", "A bone was freed", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            // Handle the result
                            if (result == MessageBoxResult.Yes)
                            {
                                NodesToRemove.Add(node);
                            }

                        }
                        else
                        {
                            if (CurrentModel.Nodes.Any(x => x.objectId == parent))
                            {
                                MessageBoxResult result = MessageBox.Show($"Bone \'{node.Name}\' is now free. Convert to helper?", "A parent bone was freed", MessageBoxButton.YesNo, MessageBoxImage.Question);

                                // Handle the result
                                if (result == MessageBoxResult.Yes)
                                {
                                    NodesToConvert.Add(node);
                                }
                            }
                        }
                    }
                }

            }
            if (NodesToRemove.Count > 0)
            {
                foreach (w3Node nodeToRemove in NodesToRemove)
                {
                    CurrentModel.Nodes.Remove(nodeToRemove);
                }


            }
            if (NodesToConvert.Count > 0)
            {
                foreach (w3Node node in NodesToConvert)
                {
                    node.Data = new Helper();
                }
            }
            if ((NodesToConvert.Count + NodesToRemove.Count) > 0) { RefreshNodesList(); }
        }

        private void DeleteNode(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            TreeViewItem selectedNode = (TreeViewItem)List_Nodes.SelectedItem;

            string name = selectedNode.Header.ToString();

            if (selectedNode != null)
            {
                // Check if the selected node has children
                if (selectedNode.Items.Count > 0)
                {
                    // Display message if node has children

                    MessageBox.Show("It is not allowed to delete a bone that has children. Relocate or delete the children first.", "Changes not made");

                }
                else
                {
                    // Get the parent node (if it exists)
                    TreeViewItem parentNode = selectedNode.Parent as TreeViewItem;
                    w3Node node = GetSelectedNode();
                    if (NodeHasAttachedVertices(node.objectId))
                    {
                        MessageBox.Show("There are attached vertices to this bone. Reattach them first", "Precaution"); return;
                    }
                    if (parentNode != null)
                    {
                        // Remove the node from its parent
                        parentNode.Items.Remove(selectedNode);

                        if (node.Data is Collision_Shape)
                        {
                            CurrentModel.Nodes.Remove(node);
                            CurrentModel.CalculateCollisionShapeEdges();
                            return;
                        }
                        CurrentModel.Nodes.Remove(node);
                    }
                    else
                    {
                        // If there is no parent, the node is a root node
                        List_Nodes.Items.Remove(selectedNode);
                        CurrentModel.Nodes.Remove(node);
                    }
                }
            }


        }
        private bool NodeHasAttachedVertices(int id)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.AttachedTo.Contains(id)) { return true; }
                }
            }
            return false;
        }
        private w3Node GetSelectedNode()
        {

            TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
            StackPanel p = item.Header as StackPanel;
            TextBlock t = p.Children[1] as TextBlock;
            string name = t.Text;
            return CurrentModel.Nodes.First(x => x.Name == name);
        }
        private void MoveBoneToRoot(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                w3Node bone = GetSelectedNode();
                if (bone.parentId != -1)
                {
                    bone.parentId = -1;
                    RefreshNodesList();
                }


            }
        }

        private void MoveBone(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Nodes.Count < 2) { MessageBox.Show("At least two nodes total must be present.", "Invalid request"); return; }

            if (List_Nodes.SelectedItem != null)
            {
                w3Node SelectedNode = GetSelectedNode();

                listbones b = new listbones(CurrentModel.Nodes, SelectedNode);
                if (b.ShowDialog() == true)
                {
                    if (b.Box.Items.Count > 0)
                    {
                        ListBoxItem SelectedTargetNodeItem = b.Box.SelectedItem as ListBoxItem;
                        string TargetNodeName = SelectedTargetNodeItem.Content.ToString();
                        w3Node TargetNode = CurrentModel.Nodes.First(x => x.Name == TargetNodeName);
                        SelectedNode.parentId = TargetNode.objectId;
                        RefreshNodesList();
                    }

                }
            }
        }





        private Bitmap GetImageFromTexturePath(string path)
        {
            return MPQHelper.GetImage(path);
            //unused... so far
        }
        private void RenameBone(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                TreeViewItem item = GetITem(List_Nodes);
                w3Node node = GetSelectedNode();
                string OriginalName = node.Name;
                UserInput i = new UserInput();
                i.Title = "Rename bone";
                i.Box.Text = OriginalName;
                if (i.ShowDialog() == true)
                {
                    string name2 = i.Box.Text.Trim();
                    if (CurrentModel.Nodes.Any(x => x.Name == name2))
                    {

                        MessageBox.Show("There is alrady a bone with this name", "Changes not made"); return;

                    }
                    else
                    {
                        node.Name = name2;
                        item.Header = name2;

                    }
                }
            }
        }
        private void RefreshTextureListForGeosets()
        {
            Item_GeosetTextureSet.Items.Clear();
            foreach (w3Material mat in CurrentModel.Materials)
            {
                MenuItem i = new MenuItem();
                if (mat.Layers.Count > 0)
                {
                    if (mat.Layers[0].Diffuse_Texure_ID.isStatic)
                    {
                        int tid = (int)mat.Layers[0].Diffuse_Texure_ID.StaticValue[0];
                        if (CurrentModel.Textures.Any(x => x.ID == tid))
                        {
                            w3Texture tx = CurrentModel.Textures.First(x => x.ID == tid);
                            if (tx.Replaceable_ID == 0)
                            {
                                i.Header = $"Material {mat.ID}: " + tx.Path;
                                i.Click += SetSelectedTextureToGeoset;
                                Item_GeosetTextureSet.Items.Add(i);
                            }
                            else if (tx.Replaceable_ID == 1)
                            {
                                i.Header = $"Material {mat.ID}: " + "Team Color";
                                i.Click += SetSelectedTextureToGeoset;
                                Item_GeosetTextureSet.Items.Add(i);
                            }
                            else if (tx.Replaceable_ID == 2)
                            {
                                i.Header = $"Material {mat.ID}: " + "Team Glow";
                                i.Click += SetSelectedTextureToGeoset;
                                Item_GeosetTextureSet.Items.Add(i);
                            }
                            else
                            {
                                throw new Exception($"Could not find texture with id {tid}");
                                continue;
                            }
                        }
                    }
                }

            }
            RefreshBitmaps();
        }
        private int GetMAterialBasedOnSelectedTexture(string texture)
        {
            switch (texture)
            {
                case "Team Color":

                    break;
                case "Team Glow": break;
                default: break;
            }
            return -1;
        }
        private void SetSelectedTextureToGeoset(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count == 0) { MessageBox.Show("Select at least one geoset", "Invalid request"); return; }
            string name = (sender as MenuItem).Header.ToString();
            int newID = int.Parse(name.Split(":")[0].Split(" ")[1]);

            foreach (object selected in List_Geosets.SelectedItems)
            {
                ListBoxItem item = selected as ListBoxItem;
                CheckBox c = item.Content as CheckBox;
                string cName = c.Content.ToString();
                int gid = int.Parse(cName);
                w3Geoset g = CurrentModel.Geosets.First(x => x.ID == gid);
                g.Material_ID = newID;
                item.ToolTip = $"Used Material ID: {newID}";
            }
            List<w3Geoset> geosets = GetSelectedGeosets();



            foreach (var geoset in geosets)
            {
                geoset.Material_ID = newID;
            }
            if (editMode_current == EditMode.Animator) RefreshFrame();
        }
        private void CenterNodeAtGeoset(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            if (CurrentModel.Geosets.Count == 0) { MessageBox.Show("No geosets were found", "Invalid request"); return; }
            w3Node SelectedNode = GetSelectedNode();
            List<string> geosets = CurrentModel.Geosets.Select(x => x.ID.ToString()).ToList();
            Selector s = new Selector(geosets);
            s.ShowDialog();
            if (s.DialogResult == true)
            {
                int selected = int.Parse((s.box.SelectedItem as ListBoxItem).Content.ToString());
                w3Geoset geo = CurrentModel.Geosets.First(x => x.ID == selected);
                Coordinate centroid = Calculator3D.CalculateCentroidFromVertices(geo.Vertices);
                SelectedNode.PivotPoint = centroid;
            }

            SetSaved(false);
        }

        private void EqualizeGeosets(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 1)
            {
                Extent firstExtent = Calculator3D.GetExtentFromVertexList(geosets[0].Vertices);
                for (int i = 1; i < geosets.Count; i++)
                {

                    Calculator3D.ScaleToFitIn(firstExtent, geosets[i].Vertices);
                }
            }
            else
            {
                MessageBox.Show("Must select 2 or more geosets", "Invalid request");
            }

        }
        private Coordinate3D GetCentroid(Extent extent)
        {
            // Calculate the centroid (center point) for each axis.
            float centerX = (extent.Maximum_X + extent.Minimum_X) / 2;
            float centerY = (extent.Maximum_Y + extent.Minimum_Y) / 2;
            float centerZ = (extent.Maximum_Z + extent.Minimum_Z) / 2;

            // Return the calculated centroid as a Coordinate3D object.
            return new Coordinate3D(centerX, centerY, centerZ);
        }
        private void SetScaling(w3Geoset geo, Extent targetExtent)
        {
            // First, get the current extent of the WhimGeoset.
            Extent currentExtent = getExtent(geo);

            // Calculate the size of the current and target extents.
            float currentWidth = currentExtent.Maximum_X - currentExtent.Minimum_X;
            float currentHeight = currentExtent.Maximum_Y - currentExtent.Minimum_Y;
            float currentDepth = currentExtent.Maximum_Z - currentExtent.Minimum_Z;

            float targetWidth = targetExtent.Maximum_X - targetExtent.Minimum_X;
            float targetHeight = targetExtent.Maximum_Y - targetExtent.Minimum_Y;
            float targetDepth = targetExtent.Maximum_Z - targetExtent.Minimum_Z;

            // Calculate the scaling factors for each axis.
            float scaleX = targetWidth / currentWidth;
            float scaleY = targetHeight / currentHeight;
            float scaleZ = targetDepth / currentDepth;

            // To maintain the aspect ratio, use the smallest scale factor across all axes.
            float uniformScale = Math.Min(Math.Min(scaleX, scaleY), scaleZ);

            // Calculate the center of the current and target extents.
            float currentCenterX = (currentExtent.Maximum_X + currentExtent.Minimum_X) / 2;
            float currentCenterY = (currentExtent.Maximum_Y + currentExtent.Minimum_Y) / 2;
            float currentCenterZ = (currentExtent.Maximum_Z + currentExtent.Minimum_Z) / 2;

            float targetCenterX = (targetExtent.Maximum_X + targetExtent.Minimum_X) / 2;
            float targetCenterY = (targetExtent.Maximum_Y + targetExtent.Minimum_Y) / 2;
            float targetCenterZ = (targetExtent.Maximum_Z + targetExtent.Minimum_Z) / 2;

            // Translate and scale each vertex in the geoset.
            foreach (w3Vertex vertex in geo.Vertices)
            {
                // First, translate the vertex to the origin (center it).
                vertex.Position.X -= currentCenterX;
                vertex.Position.Y -= currentCenterY;
                vertex.Position.Z -= currentCenterZ;

                // Scale the vertex uniformly.
                vertex.Position.X *= uniformScale;
                vertex.Position.Y *= uniformScale;
                vertex.Position.Z *= uniformScale;

                // Translate the vertex to the target center.
                vertex.Position.X += targetCenterX;
                vertex.Position.Y += targetCenterY;
                vertex.Position.Z += targetCenterZ;
            }
        }

        private Extent getExtent(w3Geoset geo)
        {
            // Initialize the Extent object with very large/small values to start comparisons.
            Extent result = new Extent
            {
                Maximum_X = float.MinValue,
                Maximum_Y = float.MinValue,
                Maximum_Z = float.MinValue,
                Minimum_X = float.MaxValue,
                Minimum_Y = float.MaxValue,
                Minimum_Z = float.MaxValue
            };

            // Loop through each vertex to update the min and max values
            foreach (w3Vertex vertex in geo.Vertices)
            {
                // Update max values
                if (vertex.Position.X > result.Maximum_X) result.Maximum_X = vertex.Position.X;
                if (vertex.Position.Y > result.Maximum_Y) result.Maximum_Y = vertex.Position.Y;
                if (vertex.Position.Z > result.Maximum_Z) result.Maximum_Z = vertex.Position.Z;

                // Update min values
                if (vertex.Position.X < result.Minimum_X) result.Minimum_X = vertex.Normal.X;
                if (vertex.Position.Y < result.Minimum_Y) result.Minimum_Y = vertex.Normal.Y;
                if (vertex.Position.Z < result.Minimum_Z) result.Minimum_Z = vertex.Normal.Z;
            }

            // Optionally, calculate the bounding radius from the center of the extent.
            // Center of the extent
            float centerX = (result.Maximum_X + result.Minimum_X) / 2;
            float centerY = (result.Maximum_Y + result.Minimum_Y) / 2;
            float centerZ = (result.Maximum_Z + result.Minimum_Z) / 2;

            // Calculate bounding radius (max distance from center to any corner of the extent)
            foreach (w3Vertex vertex in geo.Vertices)
            {
                float distanceSquared =
                    (vertex.Position.X - centerX) * (vertex.Position.X - centerX) +
                    (vertex.Position.Y - centerY) * (vertex.Position.Y - centerY) +
                    (vertex.Position.Z - centerZ) * (vertex.Position.Z - centerZ);

                result.Bounds_Radius = Math.Max(result.Bounds_Radius, (float)Math.Sqrt(distanceSquared));
            }

            return result;
        }
        bool ClickedEmpty = false;
        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            TreeView treeView = sender as TreeView;

            if (IsRightClickOnEmptySpace(treeView, e))
            {
                ClickedEmpty = true;
            }
            else
            {
                ClickedEmpty = false;
            }
        }
        private bool IsRightClickOnEmptySpace(TreeView treeView, MouseButtonEventArgs e)
        {
            // Get the element that was clicked
            DependencyObject clickedElement = e.OriginalSource as DependencyObject;

            // Walk up the visual tree to see if we hit a TreeViewItem
            while (clickedElement != null && !(clickedElement is TreeViewItem))
            {
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            // If clickedElement is null, or we didn't hit a TreeViewItem, it means the user clicked empty space
            return clickedElement == null;
        }

        private void SaveCurrentModel(object sender, RoutedEventArgs e)
        {

            if (SaveLocation == "")
            {
                Saveas(null, null);
            }
            else
            {
                CurrentModel.Name = InputModelName.Text.Trim(); ;
                CurrentModel.CalculateExtents();
                if (System.IO.Path.GetExtension(SaveLocation) == ".mdl")
                {
                    CurrentModel.CalculateExtents();
                    File.WriteAllText(SaveLocation, CurrentModel.ToMDL());

                    Saved = true;
                    RefreshTitle();
                }
                if (System.IO.Path.GetExtension(SaveLocation) == ".mdx")
                {
                    CurrentModel.CalculateExtents();
                    string tempLocation = AppHelper.TempMDLLocation;
                    File.WriteAllText(tempLocation, CurrentModel.ToMDL());
                    SaveMDX(tempLocation, SaveLocation);

                    Saved = true;
                    RefreshTitle();
                }
            }

        }


        private void Saveas(object sender, RoutedEventArgs e)
        {
            // Create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set properties for .whim files
            saveFileDialog.Filter = "MDL files (*.mdl)|*.mdl|MDX files (*.mdx)|*.mdx";
            saveFileDialog.Title = "Select a Save Location";
            saveFileDialog.DefaultExt = ".mdl";


            if (saveFileDialog.ShowDialog() == true)
            {
                CurrentModel.Name = InputModelName.Text.Trim();
                SaveLocation = saveFileDialog.FileName;
                if (System.IO.Path.GetExtension(SaveLocation) == ".mdl")
                {
                    CurrentModel.CalculateExtents();
                    File.WriteAllText(SaveLocation, CurrentModel.ToMDL());

                    Saved = true;
                    RefreshTitle();
                }
                if (System.IO.Path.GetExtension(SaveLocation) == ".mdx")
                {
                    CurrentModel.CalculateExtents();
                    string tempLocation = AppHelper.TempMDLLocation;
                    File.WriteAllText(tempLocation, CurrentModel.ToMDL());
                    SaveMDX(tempLocation, SaveLocation);

                    Saved = true;
                    RefreshTitle();
                }
            }

        }

        private void SaveMDX(string fromFile, string ToFile)
        {
            CModel Model = new();
            using (var Stream = new System.IO.FileStream(fromFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                //load

                var ModelFormat = new MdxLib.ModelFormats.CMdl();
                ModelFormat.Load(fromFile, Stream, Model);

            }
            //save
            using (var Stream = new System.IO.FileStream(ToFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var ModelFormat = new MdxLib.ModelFormats.CMdx();
                ModelFormat.Save(ToFile, Stream, Model);
            }
        }
        private void RefreshTitle()
        {
            string saved = Saved ? "Saved" : "Unsaved";
            Title = $"{AppInfo.AppTitle} v{AppInfo.Version}  - \"{SaveLocation}\" [{saved}]";
        }
        private void SetSaved(bool sav)
        {
            Saved = sav;
            string saved = sav ? "Saved" : "Unsaved";

            Title = $"{AppInfo.AppTitle} v{AppInfo.Version}  - \"{SaveLocation}\" [{saved}]";
        }
        private void SetVisibleMEnuItems(params MenuItem[] items)
        {

            mItem_Curve.Visibility = Visibility.Collapsed;
            mItem_Negate.Visibility = Visibility.Collapsed;
            mItem_CopyPos.Visibility = Visibility.Collapsed;
            mItem_PastePos.Visibility = Visibility.Collapsed;
            mItem_Arrange.Visibility = Visibility.Collapsed;
            mItem_ExpandShrink.Visibility = Visibility.Collapsed;
            mItem_CeilingVertices.Visibility = Visibility.Collapsed;
            mItem_FloorVertices.Visibility = Visibility.Collapsed;
            mItem_Split.Visibility = Visibility.Collapsed;
            mItem_DuplAA.Visibility = Visibility.Collapsed;
            mItem_Spread.Visibility = Visibility.Collapsed;
            mItem_SwapTwo.Visibility = Visibility.Collapsed;
            mItem_drawinTriangle.Visibility = Visibility.Collapsed;
            mItem_Pastefa.Visibility = Visibility.Collapsed;
            mItem_Copyfa.Visibility = Visibility.Collapsed;
            mItem_CreateShape.Visibility = Visibility.Collapsed;

            mItem_UV.Visibility = Visibility.Collapsed;



            mItem_Translate.Visibility = Visibility.Collapsed;
            mItem_Rotate.Visibility = Visibility.Collapsed;
            mItem_Aim.Visibility = Visibility.Collapsed;
            mItem_Snap.Visibility = Visibility.Collapsed;
            mItem_Scale.Visibility = Visibility.Collapsed;
            mItem_Scalefit.Visibility = Visibility.Collapsed;
            mItem_ExtrudeEach.Visibility = Visibility.Collapsed;
            mItem_ExtrudeTog.Visibility = Visibility.Collapsed;
            mItem_Extract.Visibility = Visibility.Collapsed;

            mItem_Extend.Visibility = Visibility.Collapsed;
            mItem_Widen.Visibility = Visibility.Collapsed;


            mItem_Detach.Visibility = Visibility.Collapsed;


            mItem_Inset.Visibility = Visibility.Collapsed;
            mItem_Inset2.Visibility = Visibility.Collapsed;

            mItem_Flatten.Visibility = Visibility.Collapsed;
            mItem_Bevel.Visibility = Visibility.Collapsed;
            mItem_Connect.Visibility = Visibility.Collapsed;
            mItem_CreateTriangle.Visibility = Visibility.Collapsed;
            mItem_Subdivide.Visibility = Visibility.Collapsed;
            mItem_Simplify.Visibility = Visibility.Collapsed;

            mItem_Weld.Visibility = Visibility.Collapsed;
            mItem_Center.Visibility = Visibility.Collapsed;
            mItem_Align.Visibility = Visibility.Collapsed;
            mItem_Collapse.Visibility = Visibility.Collapsed;
            mItem_Del.Visibility = Visibility.Collapsed;
            mItem_Mirror.Visibility = Visibility.Collapsed;
            mItem_MergeEdges.Visibility = Visibility.Collapsed;

            mItem_RecalcNormals.Visibility = Visibility.Collapsed;
            mItem_FlipNormals.Visibility = Visibility.Collapsed;
            mItem_NullifyNormals.Visibility = Visibility.Collapsed;
            mItem_RotateNormals.Visibility = Visibility.Collapsed;
            mItem_Cut.Visibility = Visibility.Collapsed;
            // mItem_Import.Visibility = Visibility.Collapsed;
            // mItem_Export.Visibility = Visibility.Collapsed;

            foreach (MenuItem item in items)
            {
                item.Visibility = Visibility.Visible;
            }
        }
        private void SetContextMenuState(EditMode mode)
        {
            switch (mode)
            {
                case EditMode.Vertices:
                    SetVisibleMEnuItems(
                        mItem_Translate, mItem_Snap, mItem_Scale, mItem_Rotate, mItem_Scalefit, //mItem_Flatten,
                        mItem_Connect, mItem_Weld, mItem_CreateTriangle, mItem_Center, mItem_Align, mItem_Collapse, mItem_Del
                        , mItem_Negate, mItem_CopyPos, mItem_PastePos, mItem_CeilingVertices, mItem_Arrange,
            mItem_FloorVertices, mItem_Split, mItem_DuplAA, mItem_Spread, mItem_SwapTwo, mItem_CreateShape,

            mItem_UV
                        );
                    break;
                case EditMode.Edges:

                    SetVisibleMEnuItems(mItem_Translate, mItem_Snap, mItem_Rotate, mItem_Cut, mItem_Scale, mItem_Widen, mItem_MergeEdges,
                       // mItem_Flatten,
                       mItem_ExpandShrink, mItem_CreateShape, mItem_Connect, mItem_Center, mItem_Align, mItem_Collapse, mItem_Del, mItem_Negate, mItem_CeilingVertices,
            mItem_FloorVertices);



                    break;
                case EditMode.Triangles:
                    SetVisibleMEnuItems(

                        mItem_Inset,
                        mItem_Inset2,
                        mItem_Snap,
                        mItem_CreateShape, mItem_ExpandShrink,
                        mItem_UV, mItem_drawinTriangle,
                    mItem_Pasteuv, mItem_Copyfa, mItem_Pastefa,



                    //   mItem_Curve,
                    mItem_Translate,

                    mItem_Mirror,
                    mItem_Aim,

                    mItem_Rotate,
                    mItem_Scale,
                    mItem_Scalefit,
                    mItem_ExtrudeEach
                    , mItem_Negate,
                    mItem_ExtrudeTog,
                    mItem_Extract,

                    mItem_Extend,

                    //mItem_Flatten,
                    mItem_Bevel,
                    mItem_Connect,

                    mItem_Subdivide,
                    mItem_Simplify,
                    mItem_Detach,

                    mItem_Center,
                    mItem_Align,
                    mItem_Collapse,
                    mItem_Del, mItem_CeilingVertices,
            mItem_FloorVertices
                    );
                    break;
                case EditMode.Geosets:
                    SetVisibleMEnuItems(
                        mItem_CreateShape,
                       mItem_Snap,
                    mItem_Translate,
                    mItem_Rotate,
                    mItem_Scale,
                      mItem_Negate,


                    mItem_Scalefit,



                    mItem_Center,
                    mItem_Align,

                    mItem_Del, mItem_CeilingVertices,
            mItem_FloorVertices);
                    break;
                case EditMode.Normals:
                    SetVisibleMEnuItems(mItem_RecalcNormals, mItem_FlipNormals, mItem_NullifyNormals, mItem_RotateNormals);

                    break;
                case EditMode.Sculpt:
                    SetVisibleMEnuItems();

                    break;

                case EditMode.Animator:

                    SetVisibleMEnuItems(
                       mItem_Translate, mItem_Scale, mItem_Rotate); break;
                case EditMode.Nodes:
                    SetVisibleMEnuItems(mItem_Translate); break;
            }
        }
        private void RefreshSequencesList()
        {
            ListSequences.Items.Clear();
            foreach (w3Sequence s in CurrentModel.Sequences)
            {
                ListBoxItem i = new ListBoxItem();
                string looping = s.Looping ? "Looping" : "Nonlooping";
                i.Content = $"{s.Name} [{s.From} - {s.To}] [{looping}]";
                ListSequences.Items.Add(i);
            }
            if (ListSequences.Items.Count > 0) { ListSequences.SelectedIndex = 0; }
            LabelSequences.Text = $"Sequences ({CurrentModel.Sequences.Count}):";


        }



        private void Subdivide(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                List<w3Triangle> newTriangles = new List<w3Triangle>();
                List<w3Triangle> oldTriangles = new List<w3Triangle>();

                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangle.isSelected)
                    {
                        oldTriangles.Add(triangle);

                        // Calculate the midpoint of the triangle (centroid)
                        w3Vertex midpoint = Calculator3D.GetMiddleAsNewVertex(triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);

                        // Add the midpoint to the geoset's vertices
                        geo.Vertices.Add(midpoint);

                        // Create three new triangles sharing the midpoint
                        w3Triangle newTriangle1 = new w3Triangle(triangle.Vertex1, triangle.Vertex2, midpoint);
                        w3Triangle newTriangle2 = new w3Triangle(triangle.Vertex2, triangle.Vertex3, midpoint);
                        w3Triangle newTriangle3 = new w3Triangle(triangle.Vertex3, triangle.Vertex1, midpoint);

                        // Optionally set selection state
                        newTriangle1.isSelected = true;
                        newTriangle2.isSelected = true;
                        newTriangle3.isSelected = true;

                        // Add new triangles to the list
                        newTriangles.Add(newTriangle1);
                        newTriangles.Add(newTriangle2);
                        newTriangles.Add(newTriangle3);
                    }
                }

                // Replace old triangles with new ones
                geo.Triangles.RemoveAll(t => oldTriangles.Contains(t));
                geo.Triangles.AddRange(newTriangles);

                // Recalculate edges
                geo.RecalculateEdges();
            }
        }



        private void Simplify(object sender, RoutedEventArgs e)
        {

            List<w3Triangle> triangles = GetSelectedTriangles();

            if (triangles.Count < 2)
            {
                MessageBox.Show("Select at least 2 triangles", "Invalid request"); return;
            }
            //"These 2 trinagles do not share a flat edge and cannot be reduced"
            // find all flat surfaces and then try to simplify them
            w3Geoset geo = TrianglesBelongToSameGeoset(triangles);

            if (geo != null)
            {

                if (Calculator3D.FlatCkecker.IsFlatSurface(triangles))
                {
                    Calculator3D.SimplifyTriangles(geo, triangles);
                }
                else
                {
                    MessageBox.Show("The selected triangles do not form a flat surface", "Invalid request"); return;
                }
            }
            else
            {
                MessageBox.Show("All selected triangles must belong to the same geoset", "Invalid request"); return;
            }
            ;

            //unfinished
        }

        private void SetWiden(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            if (EdgesShareVertices(SelectedEdges))
            {
                MessageBox.Show("All selected edges must not be connected to each other", "Invalid request"); return;
            }
            else
            {
                modifyMode_current = ModifyMode.Widen;

            }
        }

        private void SetTranslateMode(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.Translate;
            CallDetailsMenu(true, true, true, false, false);
        }

        private void SetScaleMode(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.Scale;
            CallDetailsMenu(true, true, true, false, true);
        }

        private void SetScaleExtrudeTogether(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.Extrude;
            CallDetailsMenu(true, true, true, true, false);
        }

        private void SetRotateNormals(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.RotateNormals;
            CallDetailsMenu(true, true, true, false, false);
        }
        private Coordinate CurrentRotationCentroid;
        private void SetRotateMode(object sender, RoutedEventArgs e)
        {

            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count < 1) { return; }
            modifyMode_current = ModifyMode.Rotate;
            CurrentRotationCentroid = Calculator3D.GetCentroid(vertices);
            CallDetailsMenu(true, true, true, false, false);
        }

        private void SetInset(object sender, RoutedEventArgs e)
        {
            SelectedTriangles = GetSelectedTriangles();
            List<w3Triangle> CreatedTriangles = new List<w3Triangle>();
            if (SelectedTriangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request"); return;
            }

            CurrentInsetCollection.Clear();

            foreach (w3Triangle triangle in SelectedTriangles)
            {
                w3Geoset geo = GetGeosetOfTriangle(triangle);
                w3Vertex v1 = triangle.Vertex1.Clone();
                w3Vertex v2 = triangle.Vertex1.Clone();
                w3Vertex v3 = triangle.Vertex1.Clone();
                w3Triangle new_triangle = new w3Triangle(v1, v2, v3);
                geo.Vertices.Add(v1);
                geo.Vertices.Add(v2);
                geo.Vertices.Add(v3);
                new_triangle.InsetScale();
                geo.Triangles.Add(new_triangle);
                CreatedTriangles.Add(new_triangle);
                geo.RecalculateEdges();

            }
            foreach (w3Triangle tr in SelectedTriangles) tr.isSelected = false;
            foreach (w3Triangle tr in CreatedTriangles) tr.isSelected = true;
            SelectedTriangles = CreatedTriangles;
            modifyMode_current = ModifyMode.Scale;
            axisMode = AxisMode.U;

            // then on mosue move we scale those vertices in their alignment position


        }

        private void SetFlattenZ(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> selected = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelected)
                    {
                        selected.Add(v);
                    }

                }

            }
            if (selected.Count > 1)
            {
                for (int i = 1; i < selected.Count; i++)
                {
                    selected[i].Position.Z = selected[0].Position.Z;
                }
            }
        }

        private void Detach(object sender, RoutedEventArgs e)
        {
            // Get selected triangles
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request");
                return;
            }

            // Ensure all triangles belong to the same geoset
            w3Geoset geo = TrianglesBelongToSameGeoset(triangles);

            // Check if all selected triangles belong to the same geoset
            if (geo == null)
            {
                MessageBox.Show("Can only detach from one geoset at a time", "Invalid request");
                return;
            }

            // Prevent detaching the entire geoset from itself
            if (geo.Triangles.Count == triangles.Count)
            {
                MessageBox.Show("Cannot detach the whole geoset from itself", "Invalid request");
                return;
            }

            // Proceed with detaching the selected vertices
            HashSet<w3Vertex> selectedVertices = new HashSet<w3Vertex>();
            foreach (w3Triangle triangle in triangles)
            {
                selectedVertices.Add(triangle.Vertex1);
                selectedVertices.Add(triangle.Vertex2);
                selectedVertices.Add(triangle.Vertex3);
            }

            // Find the shared vertices that are on the outer edge
            List<w3Vertex> edgeVertices = new List<w3Vertex>();
            foreach (w3Vertex vertex in selectedVertices)
            {
                int count = 0;
                foreach (w3Triangle tri in triangles)
                {
                    if (tri.Vertex1 == vertex || tri.Vertex2 == vertex || tri.Vertex3 == vertex)
                    {
                        count++;
                    }
                }
                // A shared vertex is on the outer edge if it is part of multiple triangles
                if (count == 1) // It's used by only one triangle
                {
                    edgeVertices.Add(vertex);
                }
                else if (count > 1)
                {
                    // Check if the vertex is shared and on the edge of the selection
                    int sharedCount = 0;
                    foreach (w3Triangle tri in triangles)
                    {
                        if ((tri.Vertex1 == vertex || tri.Vertex2 == vertex || tri.Vertex3 == vertex) && !selectedVertices.Contains(tri.Vertex1) &&
                            !selectedVertices.Contains(tri.Vertex2) && !selectedVertices.Contains(tri.Vertex3))
                        {
                            sharedCount++;
                        }
                    }
                    if (sharedCount == 1) // The vertex is shared with other triangles
                    {
                        edgeVertices.Add(vertex);
                    }
                }
            }

            // Clone and detach the edge vertices
            foreach (w3Vertex vertex in edgeVertices)
            {
                w3Vertex clonedVertex = vertex.Clone();
                // Add the cloned vertex to the geoset
                geo.Vertices.Add(clonedVertex);

                // Now update all affected triangles
                foreach (w3Triangle triangle in triangles)
                {
                    if (triangle.Vertex1 == vertex) triangle.Vertex1 = clonedVertex;
                    if (triangle.Vertex2 == vertex) triangle.Vertex2 = clonedVertex;
                    if (triangle.Vertex3 == vertex) triangle.Vertex3 = clonedVertex;
                }
            }

            // Refresh the geoset list or related data
            RefreshGeosetList();
        }


        public w3Geoset TrianglesBelongToSameGeoset(List<w3Triangle> triangles)
        {
            w3Geoset currentGeoset = null;

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                // Check if this geoset contains any of the triangles
                if (geo.Triangles.Any(tri => triangles.Contains(tri)))
                {
                    if (currentGeoset == null)
                    {
                        // First geoset that matches, set it as the current geoset
                        currentGeoset = geo;
                    }
                    else if (currentGeoset != geo)
                    {
                        // A triangle belongs to a different geoset
                        return null;
                    }
                }
            }

            // If all triangles belong to the same geoset, return it, otherwise null
            return currentGeoset;
        }

        private void CreateTriangle(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> selected = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelected)
                    {
                        selected.Add(v);
                    }

                }

            }
            if (selected.Count == 3)
            {
                if (!VerticesBelongToSameGeoset(selected[0], selected[1], selected[2])) { MessageBox.Show("These vertices do not belong to the same geoset", "Invalid request"); return; }
                w3Geoset owner = FindGeosetOfVertex(selected[0]);
                if (!VerticesCreateTriangle(selected[0], selected[1], selected[2], owner))
                {
                    owner.Triangles.Add(new w3Triangle(selected[0], selected[1], selected[2]));
                }
            }
            else if (selected.Count == 4)
            {
                if (!VerticesBelongToSameGeoset(selected[0], selected[1], selected[2])) { MessageBox.Show("These vertices do not belong to the same geoset", "Invalid request"); return; }
                w3Geoset owner = FindGeosetOfVertex(selected[0]);
                if (!VerticesCreateTriangle(selected[0], selected[1], selected[2], owner) && !VerticesCreateTriangle(selected[0], selected[3], selected[2], owner))
                {
                    owner.Triangles.Add(new w3Triangle(selected[0], selected[1], selected[2])); // First triangle
                    owner.Triangles.Add(new w3Triangle(selected[2], selected[3], selected[0])); // Second triangle
                }



            }
            else
            {
                MessageBox.Show("Select 3 or 4 vertices", "Invalid request"); return;
            }



        }
        private bool VerticesCreateTriangle(w3Vertex one, w3Vertex two, w3Vertex three, w3Geoset geo)
        {
            foreach (w3Triangle t in geo.Triangles)
            {
                // Check for all permutations of the vertices to ensure any order match
                bool match = (t.Vertex1 == one && t.Vertex2 == two && t.Vertex3 == three) ||
                             (t.Vertex1 == one && t.Vertex2 == three && t.Vertex3 == two) ||
                             (t.Vertex1 == two && t.Vertex2 == one && t.Vertex3 == three) ||
                             (t.Vertex1 == two && t.Vertex2 == three && t.Vertex3 == one) ||
                             (t.Vertex1 == three && t.Vertex2 == one && t.Vertex3 == two) ||
                             (t.Vertex1 == three && t.Vertex2 == two && t.Vertex3 == one);

                if (match)
                {
                    MessageBox.Show("These vertices already form a triangle", "Invalid request");
                    return true;
                }
            }
            return false;
        }

        private w3Geoset FindGeosetOfVertex(w3Vertex vertex)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Vertices.Contains(vertex))
                {
                    return geo;
                }
            }
            return null;
        }

        private bool VerticesBelongToSameGeoset(w3Vertex one, w3Vertex two, w3Vertex three)
        {

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Vertices.Contains(one))
                {
                    if (!geo.Vertices.Contains(two) || !geo.Vertices.Contains(three))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool VerticesBelongToSameGeoset(w3Vertex one, w3Vertex two, w3Vertex three, w3Vertex four)
        {

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Vertices.Contains(one))
                {
                    if (!geo.Vertices.Contains(two) || geo.Vertices.Contains(three) || geo.Vertices.Contains(four))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void CreateSphere(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("No materials", "Precaution"); return; }
            Create_Shape c = new Create_Shape(2, CurrentModel);

            c.ShowDialog();
            if (c.DialogResult == true) RefreshGeosetList(); RefreshTextureListForGeosets(); RefreshNodesList();

        }

        private void CreateCone(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("No materials", "Precaution"); return; }
            Create_Shape c = new Create_Shape(3, CurrentModel);
            c.ShowDialog();
            if (c.DialogResult == true) RefreshGeosetList(); RefreshTextureListForGeosets(); RefreshNodesList();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            //unfinished
            /**
             edges -  create 1 or two triangles if not already
            - faces - bridge if 2 surfaces

             */
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count != 2) { MessageBox.Show("Select two edges", "Invalid request"); return; }
                w3Geoset TargetGeoset = Edges2BelongToSameGeoset(edges[0], edges[1]);
                if (TargetGeoset == null) { return; }
                // if the edges share  vertex - 1 triangle
                w3Vertex joint = new w3Vertex();

                List<w3Vertex> VerticesOfEdges = Calculator3D.GetVerticesOfEdges(edges[0], edges[1]);
                if (Calculator3D.GeosetContainsTriangleWithTheseVertices(TargetGeoset, VerticesOfEdges))
                {
                    MessageBox.Show("These vertices already form a triangle", "Invalid request"); return;
                }
                if (VerticesOfEdges.Count == 3)
                {


                    w3Triangle new_triangle = new w3Triangle();
                    new_triangle.Vertex1 = VerticesOfEdges[0];
                    new_triangle.Vertex2 = VerticesOfEdges[1];
                    new_triangle.Vertex3 = VerticesOfEdges[2];

                    TargetGeoset.Triangles.Add(new_triangle);
                    TargetGeoset.RecalculateEdges();

                }
                if (VerticesOfEdges.Count == 4)
                {
                    List<w3Triangle> triangles = Calculator3D.CreateTwoTriangles(VerticesOfEdges);
                    TargetGeoset.Triangles.AddRange(triangles);

                }

            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                w3Geoset geoset = TrianglesBelongToSameGeoset(triangles);
                if (geoset == null)
                {
                    MessageBox.Show("The selected triangles don't belong to the same geoset", "Invalid request"); return;
                }
                else
                {
                    // unfinished
                }
            }

        }




        private w3Geoset Edges2BelongToSameGeoset(w3Edge one, w3Edge two)
        {
            bool found1 = false;
            bool found2 = false;
            w3Geoset? owner1 = new w3Geoset();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (found1 == false)
                {
                    if (geo.Edges.Contains(one))
                    {
                        owner1 = geo; found1 = true; break;
                    }
                }
            }
            if (found1 == false) { return null; }
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (found1 == false)
                {
                    if (geo.Edges.Contains(two))
                    {
                        found2 = true; break;
                    }
                }
            }
            if (found2) { return owner1; } else { MessageBox.Show("The selected edges do not belong to the same geoset", "Invalid request"); }
            return null;
        }

        private void SetNarrow(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            modifyMode_current = ModifyMode.Narrow;

        }

        private void SetFlattenY(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> selected = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelected)
                    {
                        selected.Add(v);
                    }

                }

            }
            if (selected.Count > 1)
            {
                for (int i = 1; i < selected.Count; i++)
                {
                    selected[i].Position.Y = selected[0].Position.Y;
                }
            }
        }

        private void SetFlattenX(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> selected = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelected)
                    {
                        selected.Add(v);
                    }

                }

            }
            if (selected.Count > 1)
            {
                for (int i = 1; i < selected.Count; i++)
                {
                    selected[i].Position.X = selected[0].Position.X;
                }
            }
        }


        private void SetExtractNAsewGEoset(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            List<w3Triangle> newtriangles = GetSelectedTriangles();

            if (triangles.Count > 0)
            {
                w3Geoset newGeoset = new w3Geoset();

                foreach (w3Triangle triangle in triangles)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        if (geo.Triangles.Contains(triangle))
                        {

                            w3Triangle newtriangle = new w3Triangle();
                            newtriangle.Vertex1 = GetExistingOrNewVertexForTriangle(newGeoset, triangle.Vertex1);
                            newtriangle.Vertex2 = GetExistingOrNewVertexForTriangle(newGeoset, triangle.Vertex2);
                            newtriangle.Vertex3 = GetExistingOrNewVertexForTriangle(newGeoset, triangle.Vertex3);

                            newGeoset.Triangles.Add(newtriangle);

                        }
                    }

                }
                foreach (w3Triangle triangle in triangles) triangle.isSelected = false;
                foreach (w3Triangle triangle in newtriangles) triangle.isSelected = true;
                newGeoset.ID = IDCounter.Next();
                CurrentModel.Geosets.Add(newGeoset);
                RefreshGeosetList();
                modifyMode_current = ModifyMode.Translate;
                CallDetailsMenu(true, true, true, false, false);



            }


        }

        private void SetExtract(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> InitiallySelectedTRiangles = GetSelectedTriangles();
            List<w3Triangle> newTriangles = new List<w3Triangle>(); // deselect the otehrs and select these
            if (InitiallySelectedTRiangles.Count > 0)
            {

                foreach (w3Triangle triangle in InitiallySelectedTRiangles)
                {
                    // clone each triangle in its geoset, with cloned vertices
                    // save each cloned triangles in a list of triangles, to translate them later
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        if (geo.Triangles.Contains(triangle))
                        {

                            w3Triangle newtriangle = new w3Triangle();

                            newtriangle.Vertex1 = GetExistingOrNewVertexForTriangle(geo, triangle.Vertex1);
                            newtriangle.Vertex2 = GetExistingOrNewVertexForTriangle(geo, triangle.Vertex2);
                            newtriangle.Vertex3 = GetExistingOrNewVertexForTriangle(geo, triangle.Vertex3);
                            geo.Triangles.Add(newtriangle);

                            newTriangles.Add(newtriangle);
                        }
                    }

                }
                
                foreach (w3Triangle triangle in newTriangles) triangle.isSelected = true;

                SelectVerticesOf(newTriangles);
                modifyMode_current = ModifyMode.Translate;
                CallDetailsMenu(true, true, true, false, false);



            }
        }

        private void SelectVerticesOf(List<w3Triangle> newTriangles)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle tr in geo.Triangles)
                {
                    if (newTriangles.Contains(tr) == false){
                        tr.Vertex1.isSelected = false;
                        tr.Vertex2.isSelected = false;
                        tr.Vertex3.isSelected = false;
                    }
                    else
                    {
                        tr.Vertex1.isSelected = true;
                        tr.Vertex2.isSelected = true;
                        tr.Vertex3.isSelected = true;
                    }
                }
                
            }
        }

        private w3Vertex GetExistingOrNewVertexForTriangle(w3Geoset geoset, w3Vertex inputVertex)
        {
            if (geoset.Vertices.Any(x => x.Position.SameAs(inputVertex.Position) && x.Id != inputVertex.Id))
            {
                return inputVertex;
            }
            else
            {
                w3Vertex newV = inputVertex.Clone(); ;
                geoset.Vertices.Add(newV);
                return newV;
            }
        }

        private void SetExtend(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (Calculator3D.CanBeExtended(triangles))
            {
                modifyMode_current = ModifyMode.Extend;

            }
            else
            {
                MessageBox.Show("The slected triangle islands have different facing angles. Extending not possible", "Invalid request"); return;
            }
            //unfinished - check if can be extended

        }

        private void SetBevel(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.Bevel;
            //unfinished
        }

        private void ScaleToFit(object sender, RoutedEventArgs e)
        {
            ToFitIn toFitIn = new ToFitIn();
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0)
            {
                MessageBox.Show("No vertices are seelcted", "Invalid request");
            }
            else
            {
                toFitIn.ShowDialog();
                if (toFitIn.DialogResult == true)
                {



                    Extent ex = toFitIn.ex;
                    Calculator3D.ScaleToFitIn(ex, vertices);


                }
            }



        }

        private void AverageNormals(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();

            // Dictionary to store the summed normals for each vertex
            Dictionary<int, Coordinate> summedNormals = new Dictionary<int, Coordinate>();

            // Initialize the dictionary with zero vectors for each vertex
            foreach (w3Vertex v in vertices)
            {
                summedNormals[v.Id] = new Coordinate(0, 0, 0);
            }

            // For each vertex, find all triangles that use the vertex
            foreach (w3Vertex v in vertices)
            {
                List<w3Triangle> triangles = GetTrianglesThatUseVertex(v);

                // Sum the face normals for each triangle that shares this vertex
                foreach (w3Triangle triangle in triangles)
                {
                    // Compute the face normal of the triangle
                    Coordinate faceNormal = CalculateFaceNormal(triangle.Vertex1.Position, triangle.Vertex2.Position, triangle.Vertex3.Position);

                    // Add the face normal to the sum for this vertex
                    summedNormals[v.Id] = AddCoordinates(summedNormals[v.Id], faceNormal);
                }

                // Normalize the summed normal to get the averaged normal
                v.Normal = Normalize(summedNormals[v.Id]);
            }
        }

        private List<w3Triangle> GetTrianglesThatUseVertex(w3Vertex v)
        {
            List<w3Triangle> list = new List<w3Triangle>();

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle t in geo.Triangles)
                {
                    if (t.Vertex1 == v || t.Vertex2 == v || t.Vertex3 == v)
                    {
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        private Coordinate CalculateFaceNormal(Coordinate v1, Coordinate v2, Coordinate v3)
        {
            // Calculate the edges
            Coordinate edge1 = SubtractCoordinates(v2, v1);
            Coordinate edge2 = SubtractCoordinates(v3, v1);

            // Compute the cross product of the two edges
            return Normalize(CrossProduct(edge1, edge2));
        }

        private Coordinate CrossProduct(Coordinate a, Coordinate b)
        {
            return new Coordinate(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        private Coordinate SubtractCoordinates(Coordinate a, Coordinate b)
        {
            return new Coordinate(
                a.X - b.X,
                a.Y - b.Y,
                a.Z - b.Z
            );
        }

        private Coordinate AddCoordinates(Coordinate a, Coordinate b)
        {
            return new Coordinate(
                a.X + b.X,
                a.Y + b.Y,
                a.Z + b.Z
            );
        }

        private Coordinate Normalize(Coordinate c)
        {
            float length = (float)Math.Sqrt(c.X * c.X + c.Y * c.Y + c.Z * c.Z);
            if (length == 0) return new Coordinate(0, 0, 0); // Handle zero-length vectors to avoid division by zero
            return new Coordinate(c.X / length, c.Y / length, c.Z / length);
        }

        private void NullNormals(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            foreach (w3Vertex vertex in vertices)
            {
                vertex.Normal.SetTo(vertex.Position);
            }

        }
        private List<w3Vertex> GetSelectedVertices()
        {
            List<w3Vertex> selected = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.isVisible)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        if (v.isSelected)
                        {
                            selected.Add(v);
                        }

                    }
                }
            }

            return selected;
        }

        private void Mirror(object sender, RoutedEventArgs e)
        {
            //unsinished
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0) { MessageBox.Show("Select at least one triangle"); return; }
            w3Geoset WhichGeoset = GetGeosetOfTriangles(triangles);
            if (WhichGeoset == null)
            {
                MessageBox.Show("All selected triangles must belong to the same geoset","Invalid Request"); return;
            }
            if (WhichGeoset.Triangles.Count < 2)
            {
                MessageBox.Show("No other triangles in the geoset", "Invalid request"); return;
            }
            if (triangles.Count == WhichGeoset.Triangles.Count)
            {
                MessageBox.Show("The selected triangles' count is tha same as all triangles in the geoset", "Invalid request"); return;
            }

            List<w3Vertex> vertices = GetVerticesOf(triangles);
            if (Calculator3D.IsFlatSurface(vertices))
            {
                if (Calculator3D.IsUninterruptedSurface(triangles))
                {

                    Calculator3D.MirrorGeometryInGeoset(WhichGeoset, triangles);
                }
                else
                {
                    MessageBox.Show("The selection is not an un-interrupted surface", "Invalid request"); return;
                }
            }
            else
            {
                MessageBox.Show("The selection is not a flat, un-interrupted surface", "Invalid request"); return;
            }

            // negate   vertex positions
            //unfinished
        }

        private List<w3Vertex> GetVerticesOfTriangles(List<w3Triangle> triangles)
        {
            throw new NotImplementedException();
        }

        private bool VerticesBelongToSameGeoset(List<w3Vertex> vertices, w3Geoset given)
        {
            w3Geoset? g = new w3Geoset();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in vertices)
                {
                    if (geo.Vertices.Contains(v))
                    {
                        if (g.Vertices.Count == 0)
                        {
                            g = geo; continue;
                        }
                        else
                        {
                            if (g != geo) { return false; }

                        }
                    }

                }
            }
            given = g;
            return true;
        }
        private void SetExtrudeEachMode(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.ExtrudeEach;
            CallDetailsMenu(true, true, true, true, false);
        }

        private void NegateNormals(object sender, RoutedEventArgs e)
        {

            List<w3Vertex> vertices = GetSelectedVertices();
            foreach (w3Vertex v in vertices)
            {
                v.Normal.Negate();
            }

        }

        private void EditAttachment(w3Node node)
        {
            edit_Attachment ea = new edit_Attachment(node, CurrentModel);
            ea.ShowDialog();

        }

        private void DetachAsNewGeoset(object sender, RoutedEventArgs e)
        {
            // Get selected triangles
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request");
                return;
            }

            // Ensure all triangles belong to the same geoset
            w3Geoset geo = TrianglesBelongToSameGeoset(triangles);
            if (geo != null)
            {
                // Create the new geoset
                w3Geoset newGeoset = new w3Geoset();

                // Move the selected triangles to the new geoset
                newGeoset.Triangles.AddRange(triangles);
                geo.Triangles.RemoveAll(x => triangles.Contains(x));

                // Create a map to track new vertices and avoid duplicates
                Dictionary<w3Vertex, w3Vertex> vertexMap = new Dictionary<w3Vertex, w3Vertex>();

                // Process each triangle in the new geoset
                foreach (w3Triangle t in triangles)
                {
                    // Clone vertices if not already cloned
                    if (!vertexMap.ContainsKey(t.Vertex1))
                        vertexMap[t.Vertex1] = t.Vertex1.Clone();
                    if (!vertexMap.ContainsKey(t.Vertex2))
                        vertexMap[t.Vertex2] = t.Vertex2.Clone();
                    if (!vertexMap.ContainsKey(t.Vertex3))
                        vertexMap[t.Vertex3] = t.Vertex3.Clone();

                    // Update the triangle to use the cloned vertices
                    t.Vertex1 = vertexMap[t.Vertex1];
                    t.Vertex2 = vertexMap[t.Vertex2];
                    t.Vertex3 = vertexMap[t.Vertex3];
                }

                // Add the unique cloned vertices to the new geoset
                foreach (var vertex in vertexMap.Values)
                {
                    newGeoset.Vertices.Add(vertex);
                }

                // Add the new geoset to the model
                CurrentModel.Geosets.Add(newGeoset);

                // Refresh UI or geoset-related data
                CurrentModel.RefreshGeosetAnimations();
                RefreshGeosetList();
            }
            else
            {
                MessageBox.Show("Can only detach from one geoset at a time", "Invalid request");
            }

        }

        private void DeleteSelectionInScene(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> VERTICES = GetSelectedVertices();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                geo.Vertices.RemoveAll(x => x.isSelected);
            }
            foreach (w3Geoset geoset in CurrentModel.Geosets)
            {
                geoset.Triangles.RemoveAll(x => VERTICES.Contains(x.Vertex1) || VERTICES.Contains(x.Vertex2) || VERTICES.Contains(x.Vertex3));
            }
            foreach (w3Geoset geoset in CurrentModel.Geosets)
            {
                geoset.Edges.RemoveAll(x => VERTICES.Contains(x.Vertex1) || VERTICES.Contains(x.Vertex2));
            }
            CurrentModel.Geosets.RemoveAll(x => x.Vertices.Count == 0); // remove empty geosets
            CurrentModel.Geosets.RemoveAll(x => x.Triangles.Count == 0); // remove empty geosets
            RefreshGeosetList();

        }


        private void CenterYE(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                if (CurrentlySelectedNode != null) CurrentlySelectedNode.PivotPoint.Y = 0;
            }

            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices != null && vertices.Count > 0)
                {
                    foreach (w3Vertex v in vertices)
                    {
                        v.Position.Y = 0; // Center each vertex individually on the Y-axis
                    }
                }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles != null && triangles.Count > 0)
                {
                    foreach (w3Triangle t in triangles)
                    {
                        // Calculate the centroid of the triangle
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2, t.Vertex3 });

                        // Calculate the offset to move the centroid to Y = 0
                        float offsetY = -centroid.Y;

                        // Shift each vertex by the offset on the Y-axis
                        t.Vertex1.Position.Y += offsetY;
                        t.Vertex2.Position.Y += offsetY;
                        t.Vertex3.Position.Y += offsetY;
                    }
                }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges != null && edges.Count > 0)
                {
                    foreach (w3Edge t in edges)
                    {
                        // Calculate the centroid (midpoint) of the edge
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2 });

                        // Calculate the offset to move the centroid to Y = 0
                        float offsetY = -centroid.Y;

                        // Shift each vertex by the offset on the Y-axis
                        t.Vertex1.Position.Y += offsetY;
                        t.Vertex2.Position.Y += offsetY;
                    }
                }
            }
        }


        private void AlignZ(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.Z = vertices[0].Position.Z; }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count > 1) { }
                for (int i = 1; i < edges.Count; i++) { Calculator3D.AlignEdgeTo(edges[i], edges[0], AxisMode.Z); }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count > 1) { }
                for (int i = 1; i < triangles.Count; i++) { Calculator3D.AlignTriangleTo(triangles[i], triangles[0], AxisMode.Z); }
            }
            if (editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.Z = vertices[0].Position.Z; }
            }
        }

        private void AlignY(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.Y = vertices[0].Position.Y; }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count > 1) { }
                for (int i = 1; i < edges.Count; i++) { Calculator3D.AlignEdgeTo(edges[i], edges[0], AxisMode.Y); }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count > 1) { }
                for (int i = 1; i < triangles.Count; i++) { Calculator3D.AlignTriangleTo(triangles[i], triangles[0], AxisMode.Y); }
            }
            if (editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.Y = vertices[0].Position.Y; }
            }
        }

        private void AlignX(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.X = vertices[0].Position.X; }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count > 1) { }
                for (int i = 1; i < edges.Count; i++) { Calculator3D.AlignEdgeTo(edges[i], edges[0], AxisMode.X); }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count > 1) { }
                for (int i = 1; i < triangles.Count; i++) { Calculator3D.AlignTriangleTo(triangles[i], triangles[0], AxisMode.X); }
            }
            if (editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 1) { }
                for (int i = 1; i < vertices.Count; i++) { vertices[i].Position.X = vertices[0].Position.X; }
            }

        }

        private void Cut2(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            int cutTime = 2;

            foreach (w3Edge edge in SelectedEdges)
            {
                w3Geoset geo = GetGeosetOfEdge(edge);
                Calculator3D.CutEdge(edge, geo, cutTime);
            }
        }
        private w3Geoset GetGeosetOfEdge(w3Edge edge)
        {
            return CurrentModel.Geosets.First(x => x.Edges.Contains(edge));
        }

        private void Cut3(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            int cutTime = 3;

            foreach (w3Edge edge in SelectedEdges)
            {
                w3Geoset geo = GetGeosetOfEdge(edge);
                Calculator3D.CutEdge(edge, geo, cutTime);
            }
        }

        private void Cut4(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            int cutTime = 4;

            foreach (w3Edge edge in SelectedEdges)
            {
                w3Geoset geo = GetGeosetOfEdge(edge);
                Calculator3D.CutEdge(edge, geo, cutTime);
            }
        }

        private void Cut5(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            int cutTime = 5;

            foreach (w3Edge edge in SelectedEdges)
            {
                w3Geoset geo = GetGeosetOfEdge(edge);
                Calculator3D.CutEdge(edge, geo, cutTime);
            }
        }

        private void Cut10(object sender, RoutedEventArgs e)
        {
            SelectedEdges = GetSelectedEdges();
            int cutTime = 10;

            foreach (w3Edge edge in SelectedEdges)
            {
                w3Geoset geo = GetGeosetOfEdge(edge);
                Calculator3D.CutEdge(edge, geo, cutTime);
            }
        }



        private void CreateCyl(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("No materials", "Precaution"); return; }
            Create_Shape c = new Create_Shape(4, CurrentModel);
            c.ShowDialog();
            if (c.DialogResult == true) RefreshGeosetList(); RefreshTextureListForGeosets(); RefreshNodesList();
        }

        private void CreateCube(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("No materials", "Precaution"); return; }
            Create_Shape c = new Create_Shape(1, CurrentModel);
            c.ShowDialog();
            if (c.DialogResult == true) RefreshGeosetList(); RefreshTextureListForGeosets(); RefreshNodesList();
        }


        private void CenterXE(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices != null && vertices.Count > 0) // Safety check
                {
                    foreach (w3Vertex v in vertices)
                    {
                        v.Position.X = 0; // Center each vertex individually on the X-axis
                    }
                }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles != null && triangles.Count > 0) // Safety check
                {
                    foreach (w3Triangle t in triangles)
                    {
                        // Calculate the centroid of the triangle
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2, t.Vertex3 });

                        // Calculate the offset to move the centroid to X = 0
                        float offsetX = -centroid.X;

                        // Shift each vertex by the offset on the X-axis
                        t.Vertex1.Position.X += offsetX;
                        t.Vertex2.Position.X += offsetX;
                        t.Vertex3.Position.X += offsetX;
                    }
                }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges != null && edges.Count > 0) // Safety check
                {
                    foreach (w3Edge t in edges)
                    {
                        // Calculate the centroid (midpoint) of the edge
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2 });

                        // Calculate the offset to move the centroid to X = 0
                        float offsetX = -centroid.X;

                        // Shift each vertex by the offset on the X-axis
                        t.Vertex1.Position.X += offsetX;
                        t.Vertex2.Position.X += offsetX;
                    }
                }
            }
        }

        private void CenterXT(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                if (CurrentlySelectedNode != null) CurrentlySelectedNode.PivotPoint.X = 0;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                Coordinate newCentroid = new Coordinate(0, centroid.Y, centroid.Z);
                Calculator3D.CenterVertices(vertices, newCentroid);
            }

        }

        private void CenterYT(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                if (CurrentlySelectedNode != null) CurrentlySelectedNode.PivotPoint.Y = 0;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                Coordinate newCentroid = new Coordinate(centroid.X, 0, centroid.Z);
                Calculator3D.CenterVertices(vertices, newCentroid);
            }
        }
        private void CenterZT(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                if (CurrentlySelectedNode != null) CurrentlySelectedNode.PivotPoint.Z = 0;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                Coordinate newCentroid = new Coordinate(centroid.X, centroid.Y, 0);
                Calculator3D.CenterVertices(vertices, newCentroid);
            }
        }
        private void CenterZE(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices != null && vertices.Count > 0)
                {
                    foreach (w3Vertex v in vertices)
                    {
                        v.Position.Z = 0; // Center each vertex individually on the Z-axis
                    }
                }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles != null && triangles.Count > 0)
                {
                    foreach (w3Triangle t in triangles)
                    {
                        // Calculate the centroid of the triangle
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2, t.Vertex3 });

                        // Calculate the offset to move the centroid to Z = 0
                        float offsetZ = -centroid.Z;

                        // Shift each vertex by the offset on the Z-axis
                        t.Vertex1.Position.Z += offsetZ;
                        t.Vertex2.Position.Z += offsetZ;
                        t.Vertex3.Position.Z += offsetZ;
                    }
                }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges != null && edges.Count > 0)
                {
                    foreach (w3Edge t in edges)
                    {
                        // Calculate the centroid (midpoint) of the edge
                        Coordinate centroid = Calculator3D.GetCentroid(new List<w3Vertex>() { t.Vertex1, t.Vertex2 });

                        // Calculate the offset to move the centroid to Z = 0
                        float offsetZ = -centroid.Z;

                        // Shift each vertex by the offset on the Z-axis
                        t.Vertex1.Position.Z += offsetZ;
                        t.Vertex2.Position.Z += offsetZ;
                    }
                }
            }
        }


        private void AlignDistance(object sender, RoutedEventArgs e)
        {
            SetDistanceDialog sdd = new SetDistanceDialog();
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count != 2)
                {
                    MessageBox.Show("Select exactly 2 vertices", "Invalid request");
                }
                else
                {
                    float distance = DistanceSetter.GetDistance(vertices[0], vertices[1]);
                    sdd.ShowDialog();
                    if (sdd.DialogResult == true)
                    {
                        float inputDistance = sdd.Distance;
                        bool set = sdd.Check_Set.IsChecked == true;
                        if (set)
                        {
                            DistanceSetter.SetDistance(vertices[0], vertices[2], inputDistance);
                        }
                        else
                        {
                            float newDistance = distance - inputDistance;
                            if (newDistance > 0)
                            {
                                DistanceSetter.SetDistance(vertices[0], vertices[2], newDistance);
                            }
                        }


                    }

                }
            }
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count != 2)
                {
                    MessageBox.Show("Select exactly 2 triangles", "Invalid request");
                }
                else
                {

                }
            }
            if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count != 2)
                {
                    MessageBox.Show("Select exactly 2 edges", "Invalid request");
                }
                else
                {

                }
            }

            //unfinished
        }

        private void Collapse(object sender, RoutedEventArgs e)
        {
            // Collapse selected vertices into a single one
            List<w3Vertex> selectedVertices = GetSelectedVertices();
            if (selectedVertices.Count < 2) // Require at least 2 vertices to collapse
            {
                MessageBox.Show("Select more than 1 vertex", "Invalid request");
                return;
            }
            List<w3Geoset> toDestroy = new List<w3Geoset>();
            foreach (w3Geoset geo in CurrentModel.Geosets.ToList())
            {
                if (geo != null)
                {
                    // Get the first selected vertex to keep
                    w3Vertex first = selectedVertices[0];

                    // Update triangles to use the first vertex instead of any of the other selected vertices
                    foreach (w3Triangle triangle in geo.Triangles.ToList())
                    {
                        if (selectedVertices.Contains(triangle.Vertex1))
                        {
                            triangle.Vertex1 = first;
                        }
                        if (selectedVertices.Contains(triangle.Vertex2))
                        {
                            triangle.Vertex2 = first;
                        }
                        if (selectedVertices.Contains(triangle.Vertex3))
                        {
                            triangle.Vertex3 = first;
                        }
                    }

                    // Remove the other selected vertices from the geo.Vertices list
                    foreach (w3Vertex vertex in selectedVertices.Skip(1).ToList())
                    {
                        geo.Vertices.Remove(vertex);
                    }

                    // Optionally, check for triangles with invalid references and remove them
                    geo.Triangles.RemoveAll(t => t.Vertex1 == null || t.Vertex2 == null || t.Vertex3 == null);

                    // Check if the geoset has no triangles left
                    if (geo.Triangles.Count == 0)
                    {

                        toDestroy.Add(geo);

                    }
                }
            }
            foreach (w3Geoset g in toDestroy)
            {
                MessageBox.Show($"Geoset {g.ID} was destroyed because its triangles were destroyed.", "Precaution");
                CurrentModel.Geosets.Remove(g);
            }

            CurrentModel.RefreshEdges();
            CurrentModel.RefreshSequenceExtents();
            CurrentModel.CalculateGeosetBoundingBoxes();

            RefreshGeosetList();
        }







        private void NewModel(object sender, RoutedEventArgs e)
        {
            if (Saved)
            {
                StartNewModel();

            }
            else
            {
                MessageBoxResult result = MessageBox.Show("The model is not saved. Save?", "Model not saved", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentModel(null, null);
                    StartNewModel();
                }
                else if (result == MessageBoxResult.No)
                {
                    StartNewModel();
                }
            }
        }

        private void StartNewModel()
        {
            CurrentModel = new w3Model();
            IDCounter.Reset();
            SetModeVertices(null, null);
            InputModelName.Text = CurrentModel.Name;
            List_Nodes.Items.Clear();
            List_Geosets.Items.Clear();
            List_Rigging_AttachedTo.Items.Clear();
            SaveLocation = DefaultSaveLocation;
            NewModelWindow w = new NewModelWindow(CurrentModel);
            w.ShowDialog();
            RefreshSequencesList();
            RefreshNodesList();
        }
        private void OpenModel(object sender, RoutedEventArgs e)
        {
            if (!Saved)
            {
                MessageBoxResult result = MessageBox.Show("The model is not saved. Save?", "Model not saved", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) { return; }
                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentModel(null, null);
                    OpenModelPrompt();
                }
                if (result == MessageBoxResult.No)
                {
                    OpenModelPrompt();
                }

            }
            else
            {
                OpenModelPrompt();
            }

        }
        private void OpenModel(string name)
        {

            if (!Saved)
            {
                MessageBoxResult result = MessageBox.Show("The model is not saved. Save?", "Model not saved", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) { return; }
                if (result == MessageBoxResult.Yes)
                {
                    SaveCurrentModel(null, null);
                    OpenModelPrompt(name);
                }
                if (result == MessageBoxResult.No)
                {
                    OpenModelPrompt(name);
                }
                if (result == MessageBoxResult.Cancel) { return; }
            }
            else
            {
                OpenModelPrompt(name);
            }

        }
        private void AddRecent(string file)
        {


            string path = System.IO.Path.Combine(AppHelper.DataPath, "Recents.txt");
            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(path))
                {
                    if (line == file) { return; } // if already exists skip remaining actions

                }

            }
            MenuItem item = new MenuItem();
            item.Cursor = Cursors.Hand;
            item.Header = file;
            item.Click += OpenRecent;

            ButtonOpenRecent.Items.Insert(ButtonOpenRecent.Items.Count - 1, item);

            File.AppendAllLines(path, new List<string>() { file });
        }
        private void ConvertMDXToMDLTemp(string filename)
        {
            CModel Model = new();
            string fromFile = filename;
            string ToFile = AppHelper.TempMDLLocation;
            using (var Stream = new System.IO.FileStream(fromFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                //load

                var ModelFormat = new MdxLib.ModelFormats.CMdx();
                ModelFormat.Load(fromFile, Stream, Model);

            }
            //save

            using (var Stream = new System.IO.FileStream(ToFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var ModelFormat = new MdxLib.ModelFormats.CMdl();
                ModelFormat.Save(ToFile, Stream, Model);
            }
        }
        private void OpenModelPrompt(string givenFilename = "")
        {
            string filename = givenFilename;
            if (filename == "") { filename = GetFile(); }
            if (filename == "") { return; }
            SaveLocation = filename;
            AddRecent(filename);
            if (System.IO.Path.GetExtension(filename).ToLower() == ".mdx")
            {
                ConvertMDXToMDLTemp(filename);
                filename = AppHelper.TempMDLLocation;
            }
            DrawPaused = true;
            List<Token> tokens = Parser_MDL.Tokenize(filename);
            List<TemporaryObject> temporaryObjects = Parser_MDL.SplitCollectObjects(tokens);
            CurrentModel = Parser_MDL.Parse(temporaryObjects);
            InputModelName.Text = CurrentModel.Name;
            ModelHelper.Current = CurrentModel;


            Saved = true;
            RefreshTitle();
           
            CurrentModel.FinalizeComponents();
            
            CurrentModel.Optimize();
            CurrentModel.RefreshEdges();

            // refresh lists:

            RefreshSequencesList();

            RefreshNodesList();
            RefreshGeosetList();

            List_Rigging_AttachedTo.Items.Clear();
            FixMissingLayersForMaterials();

            RefreshCameras();
            RefreshTextureListForGeosets();
            // RefreshUVTextureList();
            DrawPaused = false;
            // AdditionalTimer.Start();
            // MessageBox.Show(DisplayOptions.Textures.ToString());

        }

        private void HAndleRecentFiles()
        {

            string paths = System.IO.Path.Combine(AppHelper.DataPath, "Recents.txt");
            if (File.Exists(paths))
            {
                foreach (string line in File.ReadLines(paths))
                {
                    if (File.Exists(line))
                    {
                        MenuItem item = new MenuItem();
                        item.Click += OpenRecent;
                        item.Header = line;
                        item.Cursor = Cursors.Hand;
                        ButtonOpenRecent.Items.Insert(ButtonOpenRecent.Items.Count - 1, item);
                    }

                }
            }
        }

        private string GetFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MDL Files (*.mdl)|*.mdl|MDX Files (*.mdx)|*.mdx", // Filter for .mdl files
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
        private string GetImportedGeoset()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "WHIMGEOSET Files (*.whimgeoset)|*.whimgeoset", // Filter for .mdl files
                Title = "Open Whimgeoset File" // Title for the dialog
            };

            // Show the dialog and check if a file was selected
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;


            }
            return "";
        }
        private string GetOBJ()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "OBJ Files (*.obj)|*.obj", // Filter for .mdl files
                Title = "Import Wavefront OBJ File" // Title for the dialog
            };

            // Show the dialog and check if a file was selected
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;

            }
            return "";
        }
        private void FixMissingLayersForMaterials()
        {
            foreach (w3Material m in CurrentModel.Materials.ToList())
            {
                if (m.Layers.Count == 0)
                {
                    m.Layers.Add(CreateWhiteLayer());

                }


            }
        }

        private w3Layer CreateWhiteLayer()
        {
            w3Layer l = new w3Layer();
            l.Two_Sided = true;
            l.Diffuse_Texure_ID.isStatic = true;
            l.Diffuse_Texure_ID.StaticValue = [CurrentModel.Textures.First(x => x.Path == "Textures\\white.blp").ID];
            CreateWhiteTexture();
            return l;
        }

        private void NewBone(object sender, RoutedEventArgs e)
        {

            UserInput i = new UserInput();
            i.Title = "New bone: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is alrady a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new Bone();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Bone));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Bone));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
                SetSaved(false);
            }

        }

        private void NewHelper(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New helper: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is alrady a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();
                    node.Name = name2;
                    node.Data = new Helper();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Helper));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;
                        item.Items.Add(NewTreeItem(name2, NodeType.Helper));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
            }


        }


        private void SelectGEosetsBasedOnSceneSelection()
        {
            //unused
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelected) { geo.isSelected = true; break; }
                }
            }
            foreach (object item in List_Geosets.Items)
            {
                ListBoxItem c = item as ListBoxItem;
                c.IsSelected = false;
            }
            foreach (w3Geoset geo in CurrentModel.Geosets.Where(x => x.isSelected == true))
            {
                foreach (object item in List_Geosets.Items)
                {
                    ListBoxItem c = item as ListBoxItem;
                    CheckBox ch = c.Content as CheckBox;
                    if (ch.Content.ToString() == geo.ID.ToString())
                    {
                        c.IsSelected = true;
                    }
                }
            }
        }
        private void NewAttachment(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New attachment: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is already a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();
                    node.Name = name2;
                    node.Data = new w3Attachment();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Attachment));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;
                        item.Items.Add(NewTreeItem(name2, NodeType.Attachment));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
            }


        }

        private void NewCols(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New collision shape: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is already a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();
                    node.Name = name2;
                    node.Data = new Collision_Shape();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);
                    CurrentModel.CalculateCollisionShapeEdges();

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Collision_Shape));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;
                        item.Items.Add(NewTreeItem(name2, NodeType.Collision_Shape));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
            }



        }

        private TreeViewItem NewTreeItem(string name, string nodeType, bool hasChildren = false)
        {

            StackPanel panel = new StackPanel();
            panel.Height = 16;
            panel.Orientation = Orientation.Horizontal;
            Image img = new Image();
            img.Height = 16;
            img.Width = 16;

            img.Source = Icons[nodeType];
            TextBlock txt = new TextBlock();
            txt.Text = name;
            txt.Foreground = hasChildren ? Brushes.Green : Brushes.Black;
            txt.Margin = new Thickness(5, 0, 10, 0);
            panel.Children.Add(img);
            panel.Children.Add(txt);

            TreeViewItem builtTreeItem = new TreeViewItem();
            builtTreeItem.Header = panel;

            return builtTreeItem;
        }



        private float ParseFloat(TextBox t)
        {
            float value;
            if (float.TryParse(t.Text, out value))
            {
                return value;
            }
            else
            {
                t.Text = "0";
                return 0;
            }
        }



        private bool EdgesShareVertices(List<w3Edge> edges)
        {
            List<w3Vertex> met = new List<w3Vertex>();

            foreach (w3Edge edge in edges)
            {
                if (met.Contains(edge.Vertex1) && met.Contains(edge.Vertex2)) { return true; }
                else
                {
                    met.Add(edge.Vertex1);
                    met.Add(edge.Vertex2);
                }
            }
            met.Clear();
            return false;
        }
        private void Weld(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> selected = GetSelectedVertices();

            // Check if exactly 2 vertices are selected
            if (selected.Count != 2)
            {
                MessageBox.Show("Select 2 vertices", "Invalid request");
                return;
            }

            w3Geoset geo1 = CurrentModel.Geosets.First(x => x.Vertices.Contains(selected[0]));

            // Ensure both vertices belong to the same geoset
            if (!geo1.Vertices.Contains(selected[1]))
            {
                MessageBox.Show("These vertices do not belong to the same geoset", "Invalid request");
                return;
            }

            // Check if vertices share at least one triangle
            if (!VerticesShareATriangle(selected[0], selected[1], geo1))
            {
                MessageBox.Show("These vertices do not belong to the same triangle", "Invalid request");
                return;
            }

            // Weld the vertices by updating triangle references
            foreach (w3Triangle t in geo1.Triangles)
            {
                if (t.Vertex1 == selected[1]) t.Vertex1 = selected[0];
                if (t.Vertex2 == selected[1]) t.Vertex2 = selected[0];
                if (t.Vertex3 == selected[1]) t.Vertex3 = selected[0];
            }

            // Remove the second vertex from the geoset
            geo1.Vertices.Remove(selected[1]);
        }

        private bool VerticesShareATriangle(w3Vertex vertex1, w3Vertex vertex2, w3Geoset geo)
        {

            // Ensure that both vertices are in the geoset's vertex list
            if (geo.Vertices.Contains(vertex1) && geo.Vertices.Contains(vertex2))
            {
                // Iterate through all triangles in the geoset
                foreach (w3Triangle t in geo.Triangles)
                {
                    // Check if both vertices are part of the current triangle
                    if ((t.Vertex1 == vertex1 || t.Vertex2 == vertex1 || t.Vertex3 == vertex1) &&
                        (t.Vertex1 == vertex2 || t.Vertex2 == vertex2 || t.Vertex3 == vertex2))
                    {
                        // Return true if both vertices share this triangle
                        return true;
                    }
                }

            }
            // Return false if no shared triangle is found
            return false;
        }


        private List<w3Triangle> GetSelectedTriangles()
        {
            
            List<w3Triangle> selected = new List<w3Triangle>();
           
            foreach (w3Geoset g in CurrentModel.Geosets)
            {
                foreach (w3Triangle t in g.Triangles) { if (t.isSelected) { selected.Add(t); } }
            }
            return selected;
        }
        //-------------------------------------------------------------------
        private void AppendHistory()
        {
            if (AppSettings.HistoryEnabled == false) { return; }
            // unfinished - add to each function that changes the geometry
            switch (editMode_current)
            {
                case EditMode.Triangles:
                case EditMode.Vertices:
                case EditMode.Edges:
                case EditMode.Geosets:
                case EditMode.Sculpt:
                    break;
                case EditMode.UV:
                    break;
                case EditMode.Animator:
                    break;
                case EditMode.Rigging: break;
                case EditMode.Nodes:
                    break;
                case EditMode.Normals:
                    break;
            }


        }
        static class UndoRedo
        {



            public static int NodeIndex = 0;
            public static int AnimateNodeIndex = 0;
            public static int GeometryIndex = 0;
            public static List<NodeHistory> NodesHistory = new List<NodeHistory>();
            public static List<AnimatorHistory> AnimationHistories = new List<AnimatorHistory>();
            public static List<List<w3Geoset>> GeometryHistory = new();

            internal static void ClearAll()
            {
                NodesHistory.Clear();
                AnimationHistories.Clear();
                GeometryHistory.Clear();
            }
            private static void TrimList<T>(List<T> list, int index)
            {
                if (index >= 0 && index < list.Count)
                    list.RemoveRange(index + 1, list.Count - index - 1);
            }
        }
        private void ChangeHistory(bool undo)
        {
            if (!AppSettings.HistoryEnabled) { return; }

            int index = 0;
            switch (editMode_current)
            {
                case EditMode.Vertices:
                case EditMode.Triangles:
                case EditMode.Geosets:
                case EditMode.Edges:
                case EditMode.Sculpt:
                    index = undo ? UndoRedo.GeometryIndex - 1 : UndoRedo.GeometryIndex + 1;

                    break;
                case EditMode.Animator:
                    index = undo ? UndoRedo.AnimateNodeIndex - 1 : UndoRedo.AnimateNodeIndex + 1;
                    break;
                case EditMode.Nodes:
                    index = undo ? UndoRedo.NodeIndex - 1 : UndoRedo.NodeIndex + 1;
                    /*
                     change from:
                    manual
                    scene

                     */
                    break;
            }
        }
        private void undo(object sender, RoutedEventArgs e)
        {
            ChangeHistory(true);
            Saved = false; RefreshTitle();
        }

        private void redo(object sender, RoutedEventArgs e)
        {
            ChangeHistory(false);
            Saved = false; RefreshTitle();
        }

        //-------------------------------------------------------------------
        //------------------------------------------------------------------- 



        private void SaveasMDL(object sender, RoutedEventArgs e)
        {
            // Create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set properties for .whim files
            saveFileDialog.Filter = "MDL files (*.mdl)|*.mdl";
            saveFileDialog.Title = "Select a Save Location";
            saveFileDialog.DefaultExt = ".mdl";


            if (saveFileDialog.ShowDialog() == true)
            {
                CurrentModel.Name = InputModelName.Text.Trim();
                SaveLocation = saveFileDialog.FileName;
                CurrentModel.CalculateExtents();
                File.WriteAllText(SaveLocation, CurrentModel.ToMDL());
                RefreshTitle();
            }
        }
        private void DeleteSelectedInScene()
        {
            if (List_Nodes.IsFocused == true)
            {
                DeleteNode(null, null);
                return;
            }
            foreach (w3Geoset geo in CurrentModel.Geosets.ToList())
            {
                if (geo != null)
                {
                    geo.Vertices.RemoveAll(x => x.isSelected);
                    geo.Triangles.RemoveAll(x => !geo.Vertices.Contains(x.Vertex1) || !geo.Vertices.Contains(x.Vertex2) || !geo.Vertices.Contains(x.Vertex3));

                }

            }
            CurrentModel.Geosets.RemoveAll(x => x.Vertices.Count == 0 || x.Triangles.Count == 0);
            CurrentModel.RefreshEdges();
            CurrentModel.RefreshSequenceExtents();
            CurrentModel.CalculateGeosetBoundingBoxes();

            RefreshGeosetList();
        }
        private void SetCameraRotation(int eulerXChange, int eulerYChange, int eulerZChange)
        {
            // Update Euler angles
            eulerX += eulerXChange; // Forward-backward rotation
            eulerY += eulerYChange; // Left-right rotation
            eulerZ += eulerZChange; // Up-down rotation (if needed)

            // Ensure Euler angles remain within the 0 to 360 range
            eulerX = (eulerX + 360) % 360;
            eulerY = (eulerY + 360) % 360;
            eulerZ = (eulerZ + 360) % 360;


        }
        private void SetSelectionIcon()
        {
            if (selectionMode == SelectionMode.AddSelect)
            {
                Icon_Mouse1.BorderBrush = Brushes.Gray;
                Icon_Mouse2.BorderBrush = Brushes.GreenYellow;
                Icon_Mouse3.BorderBrush = Brushes.Gray;
            }
            else if (selectionMode == SelectionMode.ClearAndSelect)
            {
                Icon_Mouse1.BorderBrush = Brushes.GreenYellow;
                Icon_Mouse2.BorderBrush = Brushes.Gray;
                Icon_Mouse3.BorderBrush = Brushes.Gray;
            }
            else if (selectionMode == SelectionMode.RemoveSelect)
            {
                Icon_Mouse1.BorderBrush = Brushes.Gray;
                Icon_Mouse2.BorderBrush = Brushes.Gray;
                Icon_Mouse3.BorderBrush = Brushes.GreenYellow;
            }
        }

        private void GlobalHotkeyRelease(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.LeftShift)
            {
                selectionMode = SelectionMode.ClearAndSelect; SetSelectionIcon();
            }

        }

        private void GlobalHotkey(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Up)
            {

                CameraControl.eyeZ++;
                CameraControl.CenterZ++;

            }
            if (e.Key == Key.Down)
            {
                CameraControl.eyeZ--; CameraControl.CenterZ--;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A) { SelectAll(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O) { OpenModel(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S) { SaveCurrentModel(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.S) { Saveas(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z) { undo(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y) { redo(null, null); return; }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N) { NewModel(null, null); return; }
            if (e.Key == Key.V) SetModeVertices(null, null);
            if (e.Key == Key.G) SetModeGeosets(null, null);
            if (e.Key == Key.E) SetModeEdges(null, null);
            if (e.Key == Key.T) SetModeTriangles(null, null);
            if (e.Key == Key.N) SetModeNodes(null, null);
            if (e.Key == Key.U) SetModeUV(null, null);
            if (e.Key == Key.I) SetModeRigging(null, null);
            if (e.Key == Key.L) SetModeNormals(null, null);
            if (e.Key == Key.J) SetModeScaleHor(null, null);
            if (e.Key == Key.K) SetModeScaleVer(null, null);
            if (e.Key == Key.Delete)
            {
                if (editMode_current == EditMode.Nodes)
                {
                    DeleteNode(null,null);
                    return;
                }
               
                DeleteSelectedInScene();

            }
            if (e.Key == Key.S) ResetCamera(null, null);
            //uv
            if (e.Key == Key.Z) SetModeSelect(null, null);
            if (e.Key == Key.C) SetModeScale(null, null);
            if (e.Key == Key.R) SetModeRotate(null, null);
            if (e.Key == Key.M) SetModeMove(null, null);

            if (e.Key == Key.A) SetModeAnimator(null, null);
            if (e.Key == Key.P) SetModeSculpt(null, null);
            if (e.Key == Key.Space) SelectNone(null, null);

            if (e.Key == Key.LeftCtrl && selectionMode == SelectionMode.ClearAndSelect)
            {
                selectionMode = SelectionMode.AddSelect; SetSelectionIcon();
            }
            else if (e.Key == Key.LeftShift && selectionMode == SelectionMode.ClearAndSelect)
            {
                selectionMode = SelectionMode.RemoveSelect; SetSelectionIcon();
            }


        }
        private string GetNodeName()
        {
            if (List_Nodes.SelectedItem == null)
            {
                return "";
            }
            TreeViewItem i = GetITem(List_Nodes);
            StackPanel s = i.Header as StackPanel;
            TextBlock t = s.Children[1] as TextBlock;
            return t.Text;

        }

        private void RenameNode(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            string name = GetNodeName();
            TreeViewItem selected = GetITem(List_Nodes);
            StackPanel s = selected.Header as StackPanel;
            TextBlock NodeNameHolder = s.Children[1] as TextBlock;
            //---------------------------------------------------
            w3Node node = GetSelectedNode();
            UserInput i = new UserInput();
            i.Box.Text = node.Name;
            i.ShowDialog();
            if (i.DialogResult == true)
            {
                string newname = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == newname))
                {
                    MessageBox.Show("There is a node with this name already", "Changes not made");
                }
                else
                {
                    node.Name = newname;
                    NodeNameHolder.Text = newname;
                }
            }



        }

        private void SetModeUV(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            Manual.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Collapsed;
            Field_WorldMatrix.Visibility = Visibility.Collapsed;

            editMode_current = EditMode.UV;
            UVEditor.Visibility = Visibility.Visible;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            CurrentSceneInteraction = SceneInteractionState.None;
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            Manual.Visibility = Visibility.Collapsed;
            SetActive(Button_UV);
            EnableContextMenu(false); 
            Unrig();
            SetManualAvailability(false, false, false);
        }

        private void Resized(object sender, SizeChangedEventArgs e)
        {

            double h = ActualHeight;
            SidePanel.Height = h - 20;
            Scene_Canvas_.Height = h - 20;
            Scene_Frame.Height = h - 20;
            Scene_Viewport.Height = h - 20;

            Row_ViewPort.Height = new GridLength(h - 285);

            ResizeUVCanvasElements();
            SetSceneGrid(null, null);
            DrawGrid(UVCanvas_Grid, GetGridInput(Input_UVMapperGridSize));

            double DistanceBetweenTimelineAndSequences = Calculator.GetDistanceBetweenControls(Timeline_Viewer, ListSequences);
            Timeline_Viewer.Width = 300 + (DistanceBetweenTimelineAndSequences - 10);
            if (Timeline.Width < Timeline_Viewer.Width)
            {
                Timeline.Width = Timeline_Viewer.Width;
            }

        }

        private void SetModeRigging(object sender, RoutedEventArgs e)
        {

            
            editMode_current = EditMode.Rigging;


            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Collapsed;
            Field_WorldMatrix.Visibility = Visibility.Collapsed;
            CurrentSceneInteraction = SceneInteractionState.None;
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Visible;
            Manual.Visibility = Visibility.Collapsed;
            SetActive(sender as Button);
            EnableContextMenu(false);
            SetManualAvailability(false, false, false);
            List_Rigging_Bones.Items.Clear();
            foreach (w3Node n in CurrentModel.Nodes)
            {
                if (n.Data is Bone)
                {
                    List_Rigging_Bones.Items.Add(new ListBoxItem() { Content = n.Name });
                }
            }

        }
        private void UpdateRiggingData(bool selected = true)
        {
            if (editMode_current == EditMode.Rigging)
            {
                if (selected == false)
                {
                    List_Rigging_AttachedTo.Items.Clear();

                    return;
                }
                SelectedVertices = GetSelectedVertices();
                if (SelectedVertices.Count == 0)
                {
                    List_Rigging_AttachedTo.Items.Clear();
                }
                else
                {
                    bool same = true;
                    foreach (w3Vertex v in SelectedVertices) { v.AttachedTo.OrderBy(x => x).ToList(); }
                    for (int i = 1; i < SelectedVertices.Count; i++)
                    {
                        if (SelectedVertices[i].AttachedTo.SequenceEqual(SelectedVertices[0].AttachedTo) == false) { same = false; break; }
                    }
                    if (same)
                    {
                        Icon_RiggingBad.Visibility = Visibility.Collapsed;
                        Icon_RiggingOK.Visibility = Visibility.Visible;
                        Button_Detach.IsEnabled = true;
                        Button_AddAttach.IsEnabled = true;
                        List_Rigging_AttachedTo.Items.Clear();
                        foreach (int id in SelectedVertices[0].AttachedTo)
                        {
                            List_Rigging_AttachedTo.Items.Add(new ListBoxItem() { Content = CurrentModel.Nodes.First(x => x.objectId == id).Name, Foreground = Brushes.Black });
                        }


                    }
                    else
                    {
                        Icon_RiggingBad.Visibility = Visibility.Visible;
                        Icon_RiggingOK.Visibility = Visibility.Collapsed;
                        Button_Detach.IsEnabled = false;
                        Button_AddAttach.IsEnabled = false;
                        List_Rigging_AttachedTo.Items.Clear();
                    }
                }

            }

        }
        private void SetModeSelect(object sender, RoutedEventArgs e)
        {
            // uVWorkMode = UVWorkMode.Select;
            // Button_Select.BorderBrush = Brushes.Blue;
            Button_Rotate.BorderBrush = Brushes.Black;
            Button_Move.BorderBrush = Brushes.Black;
            Button_Scale.BorderBrush = Brushes.Black;
            Button_ScaleHor.BorderBrush = Brushes.Black;
            Button_ScaleVer.BorderBrush = Brushes.Black;
        }

        private void SetModeMove(object sender, RoutedEventArgs e)
        {
            uVWorkMode = UVWorkMode.Move;
            //Button_Select.BorderBrush = Brushes.Black;
            Button_Rotate.BorderBrush = Brushes.Black;
            Button_Move.BorderBrush = Brushes.Blue;
            Button_Scale.BorderBrush = Brushes.Black;
            Button_ScaleHor.BorderBrush = Brushes.Black;
            Button_ScaleVer.BorderBrush = Brushes.Black;
            UVCanvas_Overlay.Cursor = Cursors.Hand;
        }

        private void SetModeRotate(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            CurrentRotateCentroid = Calculator3D.GetCentroid(vertices);
            uVWorkMode = UVWorkMode.Rotate;
            //Button_Select.BorderBrush = Brushes.Black;
            Button_Rotate.BorderBrush = Brushes.Blue;
            Button_Move.BorderBrush = Brushes.Black;
            Button_Scale.BorderBrush = Brushes.Black;
            Button_ScaleHor.BorderBrush = Brushes.Black;
            Button_ScaleVer.BorderBrush = Brushes.Black;
            UVCanvas_Overlay.Cursor = Cursors.Arrow;
        }

        private void SetModeScale(object sender, RoutedEventArgs e)
        {
            uVWorkMode = UVWorkMode.Resize;
            
            Button_Rotate.BorderBrush = Brushes.Black;
            Button_Move.BorderBrush = Brushes.Black;
            Button_Scale.BorderBrush = Brushes.Blue;
            Button_ScaleHor.BorderBrush = Brushes.Black;
            Button_ScaleVer.BorderBrush = Brushes.Black;
            UVCanvas_Overlay.Cursor = Cursors.SizeAll;
        }



        private void Loadedd(object sender, RoutedEventArgs e)
        {
            SetModeVertices(null, null);
            Radio_Centroid.IsChecked = true;
            SetSaved(false);


        }
        private void EnableDisableUndoRedo()
        {
            switch (editMode_current)
            {
                case EditMode.Normals:
                case EditMode.Rigging:
                case EditMode.UV:
                    Panel_UndoRedo.IsEnabled = false;
                    break;
                default:
                    Panel_UndoRedo.IsEnabled = true; break;
            }
        }

        private void SaveCopy(object sender, RoutedEventArgs e)
        {
            // Create a new SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Set properties for .whim files
            saveFileDialog.Filter = "MDL files (*.mdl)|*.mdl";  // Restrict to .whim files
            saveFileDialog.Title = "Select a Save Location";        // Dialog title
            saveFileDialog.DefaultExt = ".mdl";                    // Default extension


            if (saveFileDialog.ShowDialog() == true)
            {

                string location = saveFileDialog.FileName;
                CurrentModel.CalculateExtents();
                File.WriteAllText(CurrentModel.ToMDL(), location);




            }
        }
        private Coordinate GetCustomPivotPoint()
        {
            Coordinate c = new Coordinate();
            bool parsed1 = float.TryParse(InputCustomX.Text, out float x);
            bool parsed2 = float.TryParse(InputCustomY.Text, out float y);
            bool parsed3 = float.TryParse(InputCustomZ.Text, out float z);
            if (parsed1) { c.X = x; }
            if (parsed2) { c.Y = y; }
            if (parsed3) { c.Z = z; }
            return c;
        }
        private void AxisCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Axis = AxisCheckBox.IsChecked == true;
        }

        private void GridCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Grid = GridCheckBox.IsChecked == true;
        }

        private void EdgesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Edges = EdgesCheckBox.IsChecked == true;
        }

        private void NodesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Nodes = NodesCheckBox.IsChecked == true;
        }

        private void CshapeExtentsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Extents = CshapeExtentsCheckBox.IsChecked == true;
        }

        private void RiggingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Rigging = RiggingCheckBox.IsChecked == true;
        }



        private void ShadingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Shading = ShadingCheckBox.IsChecked == true;
        }

        private void NormalsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Normals = NormalsCheckBox.IsChecked == true;
        }

        private void TexturesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Textures = TexturedCheckBox.IsChecked == true;
        }

        private void SurfaceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TrianglesCheckBox.IsChecked = true;
        }

        private void Skeleton_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Skeleton = SkeletonCheckBox.IsChecked == true;
        }
        private w3Geoset GetSelectedGeoset()
        {

            ListBoxItem item = List_Geosets.SelectedItem as ListBoxItem;
            CheckBox c = item.Content as CheckBox;
            string name = c.Content.ToString();
            int id = int.Parse(name);
            return CurrentModel.Geosets.First(x => x.ID == id);
        }
        private void CreateColShapeForGeoset(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count < 1) { return; }
            List<w3Geoset> geosets = GetSelectedGeosets();
            foreach (w3Geoset geoset in geosets)
            {
                Extent ex = Calculator3D.GetExtentFromVertexList(geoset.Vertices);
                Collision_Shape cs = new Collision_Shape();
                cs.Extents = ex;
                w3Node nod = new w3Node();
                nod.Data = cs;

                nod.Name = $"GeneratedCols_{geoset.ID}";

                CurrentModel.Nodes.Add(nod);

                List_Nodes.Items.Add(NewTreeItem(nod.Name, NodeType.Collision_Shape));

            }

            CurrentModel.CalculateCollisionShapeEdges();
        }

        private void SetTexture(object sender, RoutedEventArgs e)
        {
            Texture_Manager t = new Texture_Manager(CurrentModel);
            if (t.DialogResult == true)
            {
                string selected = (t.List_Textures.SelectedItem as ListBoxItem).Content.ToString();
                
                List<w3Geoset> geosets = GetSelectedGeosets();
                int txId = CurrentModel.Textures.First(x => x.Path == selected).ID;
                int mId = FindFirstMAterialIdOfTextureId(txId);
                foreach (w3Geoset geoset in geosets)
                {
                    geoset.Material_ID = mId;
                    geoset.SetTexure(selected);
                }

                GiveWhiteTextureToGeosetsMissing();
                RefreshTextureListForGeosets();
               
            }
        }
        private void GiveWhiteTextureToGeosetsMissing()
        {
            CreateWhiteTexture();
            w3Material m = CreateWhiteMaterial();
            foreach (w3Geoset geoset in CurrentModel.Geosets)
            {
                if (CurrentModel.Materials.Any(x => x.ID == geoset.Material_ID) == false)
                {
                    geoset.Material_ID = m.ID;
                }
            }


        }
        private int FindFirstMAterialIdOfTextureId(int mid)
        {
            foreach (w3Material m in CurrentModel.Materials)
            {
                if (m.Layers[0].Diffuse_Texure_ID.StaticValue[0] == mid)
                {
                    return m.ID;
                }
            }
            w3Material nMat = new w3Material();
            nMat.ID = IDCounter.Next();
            w3Layer l = new w3Layer();
            l.ID = IDCounter.Next();
            l.Diffuse_Texure_ID.StaticValue = [mid];
            nMat.Layers.Add(l);
            CurrentModel.Materials.Add(nMat);
            return nMat.ID;

        }
        enum UVWorkMode { Move, Rotate, Resize,
            ResizeHor,
            ResizeVer
        }


        private void SelectedNode(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            CurrentlySelectedNode = GetSelectedNode();
            foreach (w3Node node1 in CurrentModel.Nodes)
            {
                node1.isSelected = false;
            }
            CurrentlySelectedNode.isSelected = true;

            if (editMode_current == EditMode.Nodes)
            {
                if (List_Nodes.SelectedItem != null)
                {

                    inputX.Text = CurrentlySelectedNode.PivotPoint.X.ToString("F6");
                    inputY.Text = CurrentlySelectedNode.PivotPoint.Y.ToString("F6");
                    inputZ.Text = CurrentlySelectedNode.PivotPoint.Z.ToString("F6");
                    foreach (w3Node nd in CurrentModel.Nodes)
                    {
                        nd.isSelected = false;

                    }
                    CurrentlySelectedNode.isSelected = true;
                }

                if (editMode_current == EditMode.Animator)
                {

                    inputX.Text = CurrentlySelectedNode.PivotPoint.X.ToString("F6");
                    inputY.Text = CurrentlySelectedNode.PivotPoint.Y.ToString("F6");
                    inputZ.Text = CurrentlySelectedNode.PivotPoint.Z.ToString("F6");
                    bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                    if (parsedTrack)
                    {
                        if (CurrentlySelectedNode.Translation.Keyframes.Any(x => x.Track == track))
                        {
                            w3Keyframe k = CurrentlySelectedNode.Translation.Keyframes.First(x => x.Track == track);
                            inputX.Text = k.Data[0].ToString("F6");
                            inputY.Text = k.Data[1].ToString("F6");
                            inputZ.Text = k.Data[2].ToString("F6");

                        }
                        else
                        {
                            inputX.Text = "0";
                            inputY.Text = "0";
                            inputZ.Text = "0";
                        }
                        if (CurrentlySelectedNode.Rotation.Keyframes.Any(x => x.Track == track))
                        {
                            w3Keyframe k = CurrentlySelectedNode.Rotation.Keyframes.First(x => x.Track == track);
                            InputXs.Text = k.Data[0].ToString("F6");
                            InputYs.Text = k.Data[1].ToString("F6");
                            InputZs.Text = k.Data[2].ToString("F6");

                        }
                        else
                        {
                            InputXs.Text = "0";
                            InputYs.Text = "0";
                            InputZs.Text = "0";
                        }
                        if (CurrentlySelectedNode.Scaling.Keyframes.Any(x => x.Track == track))
                        {
                            w3Keyframe k = CurrentlySelectedNode.Scaling.Keyframes.First(x => x.Track == track);
                            InputXs.Text = k.Data[0].ToString("F6");
                            InputYs.Text = k.Data[1].ToString("F6");
                            InputZs.Text = k.Data[2].ToString("F6");

                        }
                        else
                        {
                            InputXs.Text = "100";
                            InputYs.Text = "100";
                            InputZs.Text = "100";
                        }
                    }
                }
            }

        }

        private System.Windows.Point _startPoint;
        private bool _isSelecting;
        private SelectionRect _selectionRect;
        private void ClickedUVMapper(object sender, MouseButtonEventArgs e)
        {




        }

        private void ReleaedUVMapper(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;




            DrawGrid(UVCanvas_Grid, GetGridInput(Input_UVMapperGridSize));
        }

        private void DrawSelectionRectangle()
        {
            // Calculate the rectangle's width and height
            float x1 = _selectionRect.TopLeft;
            float x2 = _selectionRect.TopRight;
            float y1 = _selectionRect.BottomLeft;
            float y2 = _selectionRect.BottomRight;

            // Create a new Rectangle
            UVCanvas_SelectionRect = new System.Windows.Shapes.Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };

            // Set the rectangle's dimensions and position
            double left = Math.Min(x1, x2);
            double top = Math.Min(y1, y2);
            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);

            UVCanvas_SelectionRect.Width = width;
            UVCanvas_SelectionRect.Height = height;

            // Position the rectangle on the canvas
            Canvas.SetLeft(UVCanvas_SelectionRect, left);
            Canvas.SetTop(UVCanvas_SelectionRect, top);


        }


        private void UVMapperMove(object sender, MouseEventArgs e)
        {

        }

        private void ShowInfo(object sender, RoutedEventArgs e)
        {
            CurrentModel.RefreshTransformationsList();
            string spaces = "                ";
            List<string> list =
           new List<string>() {
                $"Geosets: {CurrentModel.Geosets.Count}",
                $"Geoset animations: {CurrentModel.Geoset_Animations.Count}{spaces}",
                $"Textures: {CurrentModel.Textures.Count}{spaces}",
                $"Texture Animations: {CurrentModel.Texture_Animations.Count}{spaces}",
                $"Materials: {CurrentModel.Materials.Count}{spaces}",
                  $"Materials' layers: {CurrentModel.Materials.Sum(x=>x.Layers.Count)}{spaces}",
                $"Sequences: {CurrentModel.Sequences.Count}{spaces}",
                $"Global Sequences: {CurrentModel.Global_Sequences.Count}{spaces}   ",
                $"Global Sequences total duration: {CurrentModel.Global_Sequences.Sum(x=>x.Duration)}{spaces}   ",
                $"Cameras: {CurrentModel.Cameras.Count}{spaces}",
                $"Attachments: {CurrentModel.Nodes.Count(x => x.Data is w3Attachment)}{spaces}",
                $"Nodes: {CurrentModel.Nodes.Count}{spaces}",
                $"Bones: {CurrentModel.Nodes.Count(x => x.Data is Bone)}{spaces}",
                $"Collision Shapes: {CurrentModel.Nodes.Count(x => x.Data is Collision_Shape)}{spaces}",
                $"Helpers: {CurrentModel.Nodes.Count(x => x.Data is Helper)}{spaces}",
                $"Lights: {CurrentModel.Nodes.Count(x => x.Data is Light)}{spaces}",
                $"Emitters 1: {CurrentModel.Nodes.Count(x => x.Data is Particle_Emitter_1)}{spaces}",
                $"Emitters 2: {CurrentModel.Nodes.Count(x => x.Data is Particle_Emitter_2)}{spaces}",
                $"Ribbon Emitters: {CurrentModel.Nodes.Count(x => x.Data is Ribbon_Emitter)}{spaces}",
                $"Event Objects: {countEventObjectTracks()}{spaces}",
                $"Total Event Object Tracks: {CurrentModel.Nodes.Count(x => x.Data is Event_Object)}{spaces}",
                $"Vertices: {CurrentModel.Geosets.SelectMany(g => g.Vertices).Count()}{spaces}",
                $"Triangles: {CurrentModel.Geosets.SelectMany(g => g.Triangles).Count()}{spaces}",
                $"Edges: {CurrentModel.Geosets.Sum(g => g.CountEdges())}{spaces}",
                $"Keyframes: {CalcKeyframes()}{spaces}",
                $"Total animation time (sec): {CalculateTotalAnimationTime()}{spaces}",
                $"Total dynamic transformations: {CountDynamicTransformations()}{spaces}",
                $"Total particle count: {GetParticleCount()}{spaces}",
                $"Highest emission (particle) rate count: {GetHighestEmissionRate()}{spaces}",
                };
            int countEventObjectTracks()
            {
                int count = 0;
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (node.Data is Event_Object)
                    {
                        Event_Object e = (Event_Object)node.Data;
                        count += e.Tracks.Count;


                    }
                }
                return count;
            }
            int GetHighestEmissionRate()
            {

                int count = 0;
                foreach (w3Node nod in CurrentModel.Nodes)
                {
                    if (nod.Data is Particle_Emitter_1)
                    {
                        Particle_Emitter_1 pe = nod.Data as Particle_Emitter_1;
                        if (pe.Emission_Rate.isStatic) { count = Math.Max(count, (int)pe.Emission_Rate.StaticValue[0]); }
                        else
                        {

                            foreach (w3Keyframe k in pe.Emission_Rate.Keyframes) { count = Math.Max(count, (int)k.Data[0]); }

                        }
                    }
                    if (nod.Data is Particle_Emitter_2)
                    {
                        Particle_Emitter_2 pe = nod.Data as Particle_Emitter_2;
                        if (pe.Emission_Rate.isStatic) { count = Math.Max(count, (int)pe.Emission_Rate.StaticValue[0]); }
                        else
                        {

                            foreach (w3Keyframe k in pe.Emission_Rate.Keyframes) { count = Math.Max(count, (int)k.Data[0]); }

                        }
                    }
                    if (nod.Data is Ribbon_Emitter)
                    {
                        Ribbon_Emitter pe = nod.Data as Ribbon_Emitter;
                        count = Math.Max(count, pe.Emission_Rate);
                    }
                }
                return count;
            }
            int GetParticleCount()
            {
                int count = 0;
                foreach (w3Node nod in CurrentModel.Nodes)
                {
                    if (nod.Data is Particle_Emitter_1)
                    {
                        Particle_Emitter_1 pe = nod.Data as Particle_Emitter_1;
                        if (pe.Emission_Rate.isStatic) { count += (int)pe.Emission_Rate.StaticValue[0]; }
                        else
                        {
                            int temp = 0;
                            foreach (w3Keyframe k in pe.Emission_Rate.Keyframes) { temp = Math.Max(temp, (int)k.Data[0]); }
                            count += temp;
                        }
                    }
                    if (nod.Data is Particle_Emitter_2)
                    {
                        Particle_Emitter_2 pe = nod.Data as Particle_Emitter_2;
                        if (pe.Emission_Rate.isStatic) { count += (int)pe.Emission_Rate.StaticValue[0]; }
                        else
                        {
                            int temp = 0;
                            foreach (w3Keyframe k in pe.Emission_Rate.Keyframes) { temp = Math.Max(temp, (int)k.Data[0]); }
                            count += temp;
                        }
                    }
                    if (nod.Data is Ribbon_Emitter)
                    {
                        Ribbon_Emitter pe = nod.Data as Ribbon_Emitter;
                        count += pe.Emission_Rate;
                    }
                }
                return count;
            }
            int CountDynamicTransformations()
            {
                int count = 0;
                foreach (w3Geoset_Animation gs in CurrentModel.Geoset_Animations)
                {
                    if (gs.Alpha.isStatic == false) { count++; }
                    if (gs.Color.isStatic == false) { count++; }
                }
                foreach (w3Material m in CurrentModel.Materials)
                {
                    foreach (w3Layer l in m.Layers)
                    {
                        if (l.Alpha.isStatic != false) { count++; }
                        if (l.Diffuse_Texure_ID.isStatic != false) { count++; }
                    }

                }
                foreach (w3Texture_Animation ta in CurrentModel.Texture_Animations)
                {
                    if (ta.Translation.isStatic == false) { count++; }
                    if (ta.Rotation.isStatic == false) { count++; }
                    if (ta.Scaling.isStatic == false) { count++; }
                }
                foreach (w3Camera cam in CurrentModel.Cameras)
                {
                    if (cam.Position.isStatic == false) { count++; }
                    if (cam.Target.isStatic == false) { count++; }
                    if (cam.Rotation.isStatic == false) { count++; }
                }
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    count++;
                    if (node.Data is w3Attachment)
                    {
                        count++;

                    }
                    if (node.Data is w3Light)
                    {
                        w3Light light = (w3Light)node.Data;
                        if (light.Attenuation_Start.isStatic == false) { count++; }
                        if (light.Attenuation_End.isStatic == false) { count++; }
                        if (light.Visibility.isStatic == false) { count++; }
                        if (light.Intensity.isStatic == false) { count++; }
                        if (light.Ambient_Intensity.isStatic == false) { count++; }
                        if (light.Color.isStatic == false) { count++; }
                        if (light.Ambient_Color.isStatic == false) { count++; }
                        count++;

                    }
                    if (node.Data is Particle_Emitter_1)
                    {
                        Particle_Emitter_1 pe = (Particle_Emitter_1)node.Data;
                        if (pe.Visibility.isStatic == false) { count++; }
                        if (pe.Initial_Velocity.isStatic == false) { count++; }
                        if (pe.Latitude.isStatic == false) { count++; }
                        if (pe.Longitude.isStatic == false) { count++; }
                        if (pe.Emission_Rate.isStatic == false) { count++; }
                        if (pe.Gravity.isStatic == false) { count++; }
                        if (pe.Life_Span.isStatic == false) { count++; }
                        count++;

                    }
                    if (node.Data is Particle_Emitter_2)
                    {
                        Particle_Emitter_2 pe = (Particle_Emitter_2)node.Data;
                        if (pe.Gravity.isStatic == false) { count++; }
                        if (pe.Width.isStatic == false) { count++; }
                        if (pe.Length.isStatic == false) { count++; }
                        if (pe.Visibility.isStatic == false) { count++; }
                        if (pe.Latitude.isStatic == false) { count++; }
                        if (pe.Emission_Rate.isStatic == false) { count++; }
                        if (pe.Speed.isStatic == false) { count++; }
                        if (pe.Variation.isStatic == false) { count++; }
                        count++;

                    }
                    if (node.Data is Ribbon_Emitter)
                    {
                        Ribbon_Emitter re = (Ribbon_Emitter)node.Data;
                        if (re.Visibility.isStatic == false) { count++; }
                        if (re.Height_Below.isStatic == false) { count++; }
                        if (re.Height_Above.isStatic == false) { count++; }
                        if (re.Color.isStatic == false)
                        {
                            count++;
                            if (re.Texture_Slot.isStatic == false) { count++; }
                            count++;

                        }
                    }

                }
                return count;
            }
            int CalculateTotalAnimationTime()
            {
                int result = 0;
                foreach (w3Sequence s in CurrentModel.Sequences)
                {
                    result += s.To - s.From;
                }
                return result;
            }
            double collectTotalAnimationTime()
            {
                int collected = 0;
                foreach (w3Sequence s in CurrentModel.Sequences)
                {
                    collected += s.To - s.From;
                }
                return collected / 1000;
            }
            string message = string.Join("\n", list);
            CBox.Show(message, "Model Information", 580);
            // MessageBox.Show();
        }
        internal int CalcKeyframes()
        {
            int r = 0;
            foreach (w3Transformation t in CurrentModel.Transformations)
            {
                r += t.Keyframes.Count;
            }
            return r;
        }

        private void SetCurve(object sender, RoutedEventArgs e)
        {
            modifyMode_current = ModifyMode.Curve;
            // unfinished

        }




        private void MergeEdges(object sender, RoutedEventArgs e)
        {


            List<w3Edge> edges = GetSelectedEdges();
            if (edges.Count != 2)
            {
                MessageBox.Show("Select two edges", "Invalid request"); return;
            }
            w3Geoset geo1 = GetGeosetOfEdge(edges[0]);
            w3Geoset geo2 = GetGeosetOfEdge(edges[1]);
            if (geo1 != geo2)
            {
                MessageBox.Show("The selected edges must belong to the same geoset", "Invalid request"); return;
            }
            w3Vertex mid = GetJointVertexOfEdge(edges[0], edges[1]);
            if (mid == null)
            {
                MessageBox.Show("No mid", "Invalid request"); return;
            }
            if (edges[0].Vertex1 != mid)
            {
                foreach (w3Triangle tr in geo1.Triangles)
                {
                    if (tr.Vertex1 == edges[0].Vertex1)
                    {
                        tr.Vertex1 = mid;
                    }
                    if (tr.Vertex2 == edges[0].Vertex1)
                    {
                        tr.Vertex2 = mid;
                    }
                    if (tr.Vertex3 == edges[0].Vertex1)
                    {
                        tr.Vertex3 = mid;
                    }
                }
                geo1.Vertices.Remove(edges[0].Vertex1);
                return;
            }
            if (edges[1].Vertex1 != mid)
            {
                foreach (w3Triangle tr in geo1.Triangles)
                {
                    if (tr.Vertex1 == edges[1].Vertex1)
                    {
                        tr.Vertex1 = mid;
                    }

                    if (tr.Vertex2 == edges[1].Vertex1)
                    {
                        tr.Vertex2 = mid;
                    }
                    if (tr.Vertex3 == edges[1].Vertex1)
                    {
                        tr.Vertex3 = mid;
                    }
                }
                geo1.Vertices.Remove(edges[1].Vertex1);
                return;
            }

        }
        private w3Vertex GetJointVertexOfEdge(w3Edge edge1, w3Edge edge2)
        {
            if (edge1.Vertex1 == edge2.Vertex1) { return edge1.Vertex1; }
            if (edge1.Vertex1 == edge2.Vertex2) { return edge1.Vertex1; }
            if (edge1.Vertex2 == edge2.Vertex1) { return edge1.Vertex2; }

            return null;
        }
        private List<w3Edge> GetSelectedEdges()
        {
            List<w3Edge> list = new List<w3Edge>();

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Edge edge in geo.Edges)
                {
                    if (edge.isSelected == true) { list.Add(edge); }
                }
            }
            return list;
        }

        private void Negate(object sender, RoutedEventArgs e)
        {

            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { MessageBox.Show("Select vertices", "Invalid request"); return; }
            foreach (w3Vertex v in vertices) { v.Position.Negate(); v.Normal.Negate(); }
            SetSaved(false);
        }

        private void ExportOBJ(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Geosets.Count == 0)
            {
                MessageBox.Show("The model doesn't contain geosets", "Invalid request");
                return;
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                // Set properties for .whim files
                saveFileDialog.Filter = "OBJ files (*.obj)|*.obj";
                saveFileDialog.Title = "Select a Save Location";
                saveFileDialog.DefaultExt = ".obj";


                if (saveFileDialog.ShowDialog() == true)
                {

                    SaveLocation = saveFileDialog.FileName;
                    Parser_OBJ.Save(CurrentModel, SaveLocation);
                }
            }
        }
        private bool HasLessThan30kVertices()
        {
            int count = CurrentModel.Geosets.Sum(g => g.Vertices.Count);
            if (count >= 28900) MessageBox.Show("This program is not made to support more than 30,000 vertices.", "Program limitation");

            return count >= 28900;
        }

        public string Int16ToString(Int16 value)
        {
            // Convert the integer to its byte representation
            byte[] bytes = BitConverter.GetBytes(value);

            // Convert each byte to its corresponding character
            return $"{(char)bytes[0]}{(char)bytes[1]}";
        }

        public Int16 StringToInt16(string str)
        {
            if (str.Length != 2)
                throw new ArgumentException("Input string must be exactly 2 characters long.");

            // Create a byte array to hold the byte representation
            byte[] bytes = new byte[2];
            bytes[0] = (byte)str[0]; // Convert first character to byte
            bytes[1] = (byte)str[1]; // Convert second character to byte

            // Convert the byte array back to an Int16
            return BitConverter.ToInt16(bytes, 0);
        }

        public static string FloatToByteString(float value)
        {
            // Convert the float to bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Convert each byte to its corresponding character representation
            char[] chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                chars[i] = (char)bytes[i];
            }

            // Return the string representation of the characters
            return new string(chars);
        }
        private float StringToFloat(string charRepresentation)
        {
            byte[] bytes = new byte[4];
            for (int i = 0; i < charRepresentation.Length; i++)
            {
                bytes[i] = (byte)charRepresentation[i];
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        private void DetachBones(object sender, RoutedEventArgs e)
        {
            if (List_Rigging_AttachedTo.SelectedItem == null) { MessageBox.Show("Select an item in the rigging list", "Invalid request"); return; }
            if (List_Rigging_AttachedTo.Items.Count == 0) return;
            if (List_Rigging_AttachedTo.Items.Count == 1) { MessageBox.Show("Cannot leave free vertices", "Precaution"); return; }
            string name = (List_Rigging_AttachedTo.SelectedItem as ListBoxItem).Content.ToString();
            int id = CurrentModel.Nodes.First(x => x.Name == name).objectId;


            if (SelectedVertices.Count == 0) { MessageBox.Show("No vertices are selected", "Invalid request"); return; }
            foreach (w3Vertex v in SelectedVertices)
            {
                if (v.AttachedTo.Contains(id))
                {
                    v.AttachedTo.Remove(id);
                }


            }
            List_Rigging_AttachedTo.Items.Remove(List_Rigging_AttachedTo.SelectedItem);


        }
        private List<w3Node> GetSelectedNodes()
        {
            return CurrentModel.Nodes.Where(x => x.isSelected).ToList();

        }
        private void ClearAndAttachBones(object sender, RoutedEventArgs e)
        {

            if (List_Rigging_Bones.SelectedItem == null) { MessageBox.Show("Select a bone from the rigging bone list", "Incomplete request"); return; }
            if (SelectedVertices.Count == 0) { MessageBox.Show("No vertices are selected", "Incomplete request"); return; }
            w3Node node = GetSelectedRiggingBone();

            foreach (w3Vertex v in SelectedVertices)
            {
                v.AttachedTo.Clear();
                v.AttachedTo.Add(node.objectId);

            }
            Icon_RiggingBad.Visibility = Visibility.Collapsed;
            Icon_RiggingOK.Visibility = Visibility.Visible;
            Button_Detach.IsEnabled = true;
            Button_AddAttach.IsEnabled = true;
            List_Rigging_AttachedTo.Items.Clear();
            List_Rigging_AttachedTo.Items.Add(new ListBoxItem() { Content = node.Name, Foreground = Brushes.Black });
        }

        private w3Node GetSelectedRiggingBone()
        {
            ListBoxItem item = List_Rigging_Bones.SelectedItem as ListBoxItem;
            string content = item.Content.ToString();

            return CurrentModel.Nodes.First(x => x.Name == content);
        }
        private void AddAttachBone(object sender, RoutedEventArgs e)
        {
            if (List_Rigging_AttachedTo.Items.Count == 4)
            {
                MessageBox.Show("No more than 4 attached bones per vertex!", "Precaution"); return;
            }

            if (List_Rigging_Bones.SelectedItem == null) { MessageBox.Show("Select a bone from the rigging bone list", "Incomplete request"); return; }
            if (SelectedVertices.Count == 0) { MessageBox.Show("No vertices are selected", "Incomplete request"); return; }
            w3Node node = GetSelectedRiggingBone();

            List<w3Vertex> list = GetSelectedVertices();

            foreach (w3Vertex v in list)
            {
                if (!v.AttachedTo.Contains(node.objectId))
                {
                    v.AttachedTo.Add(node.objectId);
                }

            }
            List_Rigging_AttachedTo.Items.Add(new ListBoxItem() { Content = node.Name, Background = Brushes.Black });
        }
        private void RefreshRigginList(w3Vertex vertex)
        {
            List_Rigging_AttachedTo.Items.Clear();
            foreach (int id in vertex.AttachedTo)
            {
                List_Rigging_AttachedTo.Items.Add(new ListBoxItem() { Content = GetNodeNameByID(id) });
            }
        }
        private string GetNodeNameByID(int id)
        {
            return CurrentModel.Nodes.First(x => x.objectId == id).Name;
        }
        bool AllVerticesAreAttachedToSameNodes(List<w3Vertex> list)
        {
            List<int> first = list[0].AttachedTo;
            first = first.OrderBy(x => x).ToList();
            for (int i = 1; i < list.Count; i++)
            {
                list[i].AttachedTo = list[i].AttachedTo.OrderBy(x => x).ToList();
                if (first.SequenceEqual(list[i].AttachedTo) == false) { return false; }
            }
            return true;
        }
        private void SetModeAnimator(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Visible;
            LabelNodes.Visibility = Visibility.Visible;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            CurrentSceneInteraction = SceneInteractionState.None;
            Field_ModifyAmount.Visibility = Visibility.Visible;
            Field_WorldMatrix.Visibility = Visibility.Collapsed;
            editMode_current = EditMode.Animator;
            Manual.Visibility = Visibility.Visible;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Visible;
            EnableContextMenu(true);

            SetActive(Button_Animator);
            SetContextMenuState(EditMode.Animator);
            SetManualAvailability(true, true, true);
            RefreshFrame();

        }


        private void ChangedSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider s = (Slider)sender;
            int tick = (int)s.Value;
            // Slider_Sequence.ToolTip = tick.ToString();
        }



        private void DeleteSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
            string name = selected.Content.ToString().Split(" [")[0];
            ListSequences.Items.Remove(ListSequences.SelectedItem);
            ClearAnimationsForDeletedSEquence(name);
            CurrentModel.Sequences.RemoveAll(x => x.Name == name);
            if (ListSequences.Items.Count == 0)
            {
                //  Slider_Sequence.Minimum = 0;
                //Slider_Sequence.Maximum = 0;
                InputCurrentTrack.Text = "-1";
                CurrentSceneFrame = -1;
                RefreshFrame();
            }
            else
            {
                ListSequences.SelectedIndex = 0;
                w3Sequence sequence = CurrentModel.Sequences[0];
                InputCurrentTrack.Text = sequence.From.ToString();
                CurrentSceneFrame = sequence.From;
                FillTimeline(sequence);
            }
            RefreshFrame();
            SetSaved(false);
            LabelSequences.Text = $"Sequences ({CurrentModel.Sequences.Count}):";
        }
        private void FillTimeline(w3Sequence sequence)
        {

            if (sequence == null) { return; }
            Timeline.Children.Clear();
            for (int i = sequence.From; i <= sequence.To; i++)
            {
                System.Windows.Media.Brush br = GetTrackColor(i);
                if (br == Brushes.Gray) { continue; }
                Button b = new Button();
                b.Width = 10;
                b.Height = Timeline.Height;
                b.Background = br;
                b.ToolTip = i.ToString();
                b.Click += ClickedTrackInTimeline;
                b.MouseRightButtonDown += ClickedEDitInTimeline;
                Timeline.Children.Add(b);
            }
            double minimumWidth = 300;
            double newWith = Timeline.Children.Count * 10;
            if (minimumWidth < newWith) { minimumWidth = newWith; }
            Timeline.Width = minimumWidth;
        }

        private void ClickedEDitInTimeline(object sender, MouseButtonEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Precaution"); return;
            }
            Button b = sender as Button;
            int track = int.Parse(b.ToolTip.ToString());
            w3Node node = GetSelectedNode();
            if (node != null)
            {
                edit_keyframe kf = new edit_keyframe(node, track);
                kf.ShowDialog();
                if (kf.DialogResult == true)
                {
                    w3Sequence s = GetSelectedSequence();
                    FillTimeline(s);
                }
            }
        }

        private void ClickedTrackInTimeline(object sender, EventArgs e)
        {
            Button b = sender as Button;
            int track = int.Parse(b.ToolTip.ToString());
            InputCurrentTrack.Text = track.ToString();
            CurrentSceneFrame = track;
            RefreshFrame();
        }
        private System.Windows.Media.Brush GetTrackColor(int i)
        {
            bool translation = false;
            bool rotation = false;
            bool scaling = false;
            foreach (w3Node node in CurrentModel.Nodes)
            {
                if (node.Translation.Keyframes.Any(x => x.Track == i)) { translation = true; }
                if (node.Rotation.Keyframes.Any(x => x.Track == i)) { rotation = true; }
                if (node.Scaling.Keyframes.Any(x => x.Track == i)) { scaling = true; }
            }
            if (translation && !rotation & !scaling) { return Brushes.Green; }
            if (!translation && rotation & !scaling) { return Brushes.Yellow; }
            if (!translation && !rotation & scaling) { return Brushes.Red; }
            if (translation && rotation & !scaling) { return Brushes.GreenYellow; }
            if (translation && rotation & scaling) { return Brushes.White; }
            if (translation && !rotation & scaling) { return Brushes.LightGoldenrodYellow; }
            if (!translation && rotation & scaling) { return Brushes.Orange; }

            return Brushes.Gray;
        }
        private void ClearAnimationsForDeletedSEquence(string name)
        {
            w3Sequence seq = CurrentModel.Sequences.First(x => x.Name == name);
            int from = seq.From;
            int to = seq.To;
            foreach (w3Node node in CurrentModel.Nodes)
            {
                node.Translation.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                node.Scaling.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                node.Rotation.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
            }
            foreach (w3Geoset_Animation ga in CurrentModel.Geoset_Animations)
            {
                if (ga.Alpha.isStatic == false) ga.Alpha.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                if (ga.Color.isStatic == false) ga.Color.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);

            }
            foreach (w3Camera cam in CurrentModel.Cameras)
            {
                if (cam.Position.isStatic == false) cam.Position.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                if (cam.Rotation.isStatic == false) cam.Rotation.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                if (cam.Target.isStatic == false) cam.Target.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
            }
            foreach (w3Texture_Animation ta in CurrentModel.Texture_Animations)
            {
                if (ta.Translation.isStatic == false) ta.Translation.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                if (ta.Rotation.isStatic == false) ta.Rotation.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                if (ta.Scaling.isStatic == false) ta.Scaling.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
            }
            foreach (w3Material m in CurrentModel.Materials)
            {
                foreach (w3Layer l in m.Layers)
                {
                    if (l.Alpha.isStatic == false) l.Alpha.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                    if (l.Diffuse_Texure_ID.isStatic == false) l.Diffuse_Texure_ID.Keyframes.RemoveAll(x => x.Track >= from && x.Track <= to);
                }
            }
            /*

            attachment visibility
            pe1
            pe2
            re
            light

             */
        }

        private void EditSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
            string name = selected.Content.ToString().Split(" [")[0];
            w3Sequence found = CurrentModel.Sequences.First(x => x.Name == name);

            int OriginalFrom = found.From;
            int OriginalTo = found.To;
            sequenceWindow s = new sequenceWindow(CurrentModel.Sequences, true);
            s.Editing = true;
            s.OriginalName = found.Name;
            s.text_name.Text = found.Name;
            s.text_range.Text = $"{found.From} - {found.To}";
            s.Check_looping.IsChecked = found.Looping;
            s.ShowDialog();
            if (s.DialogResult == true)
            {
                if (s.check_rescaleK.IsChecked == true)
                {
                    int newInterval = s.AcceptedSequence.To - s.AcceptedSequence.From;

                    if (GetKeyframesCountForSequence(found) > newInterval)
                    {
                        MessageBox.Show("Cannot shrink the sequence, because the keyframes for its animations are more than the new count of tracks.", "Invalid request");

                        return;
                    }

                }
                string looping = s.AcceptedSequence.Looping ? "Looping" : "Nonlooping";
                string newItemName = $"{s.AcceptedSequence.Name} [{s.AcceptedSequence.From} - {s.AcceptedSequence.To}] [{looping}]";
                selected.Content = newItemName;
                found.Name = s.AcceptedSequence.Name;
                found.From = s.AcceptedSequence.From;
                found.To = s.AcceptedSequence.To;
                found.Looping = s.AcceptedSequence.Looping;


                if (s.check_rescaleK.IsChecked == true)
                {
                    CurrentModel.RefreshTransformationsList();
                    List<w3Keyframe> keyframes = CurrentModel.Transformations
     .SelectMany(x => x.Keyframes)
     .Where(y => y.Track >= OriginalFrom && y.Track <= OriginalTo)
     .ToList();

                    Calculator3D.RescaleSequenceKeyframes(OriginalFrom, OriginalTo, found.From, found.To, keyframes);
                }
                SelectedSequence(null, null);
                SetSaved(false);
            }
        }

        private int GetKeyframesCountForSequence(w3Sequence sequence)
        {
            List<int> usedKeyframes = new List<int>();
            CurrentModel.RefreshTransformationsList();
            foreach (w3Transformation transformation in CurrentModel.Transformations)
            {
                foreach (w3Keyframe keyframe in transformation.Keyframes)
                {
                    if (keyframe.Track <= sequence.From && keyframe.Track <= sequence.To)
                    {
                        if (usedKeyframes.Contains(keyframe.Track) == false)
                        {
                            usedKeyframes.Add(keyframe.Track);
                        }
                    }
                }
            }
            return usedKeyframes.Count;
        }


        private void ResetTrackAll(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    foreach (w3Keyframe k in node.Translation.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [0, 0, 0];
                    }
                    foreach (w3Keyframe k in node.Rotation.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [0, 0, 0];
                    }
                    foreach (w3Keyframe k in node.Scaling.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [100, 100, 100];
                    }

                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ResetTrackTrans(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    foreach (w3Keyframe k in node.Translation.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [0, 0, 0];
                    }


                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ResetTrackRot(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {

                    foreach (w3Keyframe k in node.Rotation.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [0, 0, 0];
                    }


                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ResetrackScale(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {

                    foreach (w3Keyframe k in node.Scaling.Keyframes)
                    {
                        if (k.Track == Track)
                            k.Data = [100, 100, 100];
                    }

                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearTrackAll(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Translation.Keyframes.RemoveAll(x => x.Track == Track);
                }
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Rotation.Keyframes.RemoveAll(x => x.Track == Track);
                }
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Scaling.Keyframes.RemoveAll(x => x.Track == Track);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearTrackTrans(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Translation.Keyframes.RemoveAll(x => x.Track == Track);
                }
            }
            FillTimeline(GetSelectedSequence());
        }

        private void ClearTrackRot(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Rotation.Keyframes.RemoveAll(x => x.Track == Track);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearTrackScale(object sender, RoutedEventArgs e)
        {
            int Track = 0;
            bool parsed = int.TryParse(InputCurrentTrack.Text, out Track);
            if (parsed)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.Scaling.Keyframes.RemoveAll(x => x.Track == Track);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearSequenceAll(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
                string name = selected.Content.ToString().Split(" [")[0];
                w3Sequence seq = CurrentModel.Sequences.First(x => x.Name == name);
                foreach (w3Node n in CurrentModel.Nodes)
                {
                    n.Translation.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                    n.Rotation.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                    n.Scaling.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearSequenceTrans(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
                string name = selected.Content.ToString().Split(" [")[0];
                w3Sequence seq = CurrentModel.Sequences.First(x => x.Name == name);
                foreach (w3Node n in CurrentModel.Nodes)
                {
                    n.Translation.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearSequenceRot(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
                string name = selected.Content.ToString().Split(" [")[0];
                w3Sequence seq = CurrentModel.Sequences.First(x => x.Name == name);
                foreach (w3Node n in CurrentModel.Nodes)
                {
                    n.Rotation.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ClearSequenceScale(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                ListBoxItem selected = ListSequences.SelectedItem as ListBoxItem;
                string name = selected.Content.ToString().Split(" [")[0];
                w3Sequence seq = CurrentModel.Sequences.First(x => x.Name == name);
                foreach (w3Node n in CurrentModel.Nodes)
                {
                    n.Scaling.Keyframes.RemoveAll(x => x.Track >= seq.From && x.Track <= seq.To);
                }
                FillTimeline(GetSelectedSequence());
            }
        }

        private void ShowAnimatorOptionss(object sender, RoutedEventArgs e)
        {
            AnimatorOptions.IsOpen = true;
        }

        private void EditGeosetAnimation(object sender, RoutedEventArgs e)
        {
            geosetanims eg = new geosetanims(CurrentModel);
            eg.ShowDialog();
            SetSaved(false);
        }

        private void Edittrs(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            w3Node n = GetSelectedNode();
            TextBlock currentTextBlock = GetSelectedNodeName();

            NodePropertiesEditor transformations_Editor = new NodePropertiesEditor(CurrentModel, n, currentTextBlock);
            transformations_Editor.ShowDialog();
            RefreshFrame();
        }
        private TextBlock GetSelectedNodeName()
        {
            TreeViewItem i = List_Nodes.SelectedItem as TreeViewItem;

            StackPanel p = i.Header as StackPanel;
            TextBlock b = p.Children[1] as TextBlock;
            return b;
        }
        private void EditNode_Data(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) return;
            w3Node node = GetSelectedNode();
            // MessageBox.Show(node.Data.GetType().Name);
            switch (node.Data)
            {
                case Bone:
                    bone_Data_editor editbn = new bone_Data_editor(node, CurrentModel);
                    editbn.ShowDialog();
                    break;
                case w3Attachment:
                    EditAttachment(node);
                    break;
                case w3Light:
                    Editor_Light el = new Editor_Light(node, CurrentModel);
                    el.ShowDialog();
                    break;
                case Particle_Emitter_1:
                    Editor_Emitter1 em_1 = new Editor_Emitter1(node, CurrentModel);
                    em_1.ShowDialog();
                    break;
                case Particle_Emitter_2:
                    editor_emitter2 em_2 = new editor_emitter2(CurrentModel, node);
                    em_2.ShowDialog();
                    break;
                case Ribbon_Emitter:
                    Editor_Ribbon rib = new Editor_Ribbon(node, CurrentModel);
                    rib.ShowDialog();
                    break;
                case Event_Object:
                    EditEventObject ee = new EditEventObject(node, CurrentModel);
                    ee.ShowDialog();
                    if (ee.DialogResult == true)
                    {
                        TreeViewItem treeViewItem = List_Nodes.SelectedItem as TreeViewItem;
                        treeViewItem.Header = node.Name;
                    }
                    break;
                case Collision_Shape:
                    edit_cols eg = new edit_cols(node);
                    eg.ShowDialog();
                    if (eg.DialogResult == true) { CurrentModel.CalculateCollisionShapeEdges(); }
                    break;
                default: return;
            }
            //unfinished
        }

        private void EditGlobalSequences(object sender, RoutedEventArgs e)
        {
            editgs egs = new editgs(CurrentModel);
            egs.ShowDialog();
            SetSaved(false);
        }

        private void ManageTextures(object sender, RoutedEventArgs e)
        {

            Texture_Manager t = new Texture_Manager(CurrentModel);
            t.ShowDialog();
            RefreshBitmaps();
            RefreshTextureListForGeosets();
            RefreshUVTextureList();
            SetSaved(false);
        }

        private void CreateCamFromView(object sender, RoutedEventArgs e)
        {
            w3Camera cam = new w3Camera();
            cam.Name = "Camera0" + IDCounter.Next().ToString();
            cam.Position.isStatic = true;
            cam.Position.StaticValue = [CameraControl.eyeX, CameraControl.eyeY, CameraControl.eyeZ];
            cam.Target.isStatic = true;
            cam.Target.StaticValue = [CameraControl.CenterX, CameraControl.CenterY, CameraControl.CenterZ];

            CurrentModel.Cameras.Add(cam);
            CameraList.Items.Add(new ListBoxItem() { Content = cam.Name });
        }

        private void DelCam(object sender, RoutedEventArgs e)
        {
            if (CameraList.SelectedItem == null) { return; }
            string name = (CameraList.SelectedItem as ListBoxItem).Content.ToString();
            CurrentModel.Cameras.RemoveAll(x => x.Name == name);
            CameraList.Items.Remove(CameraList.SelectedItem);
        }
        private void RefreshCameras()
        {
            CameraList.Items.Clear();
            foreach (w3Camera cam in CurrentModel.Cameras)
            {
                CameraList.Items.Add(new ListBoxItem() { Content = cam.Name });
            }
        }

        public class SelectionRect
        {
            public float TopLeft { get; set; }
            public float TopRight { get; set; }
            public float BottomLeft { get; set; }
            public float BottomRight { get; set; }
        }


        private void SelectUnselectedItem(object sender, MouseButtonEventArgs e)
        {
            // Cast the sender to ListBox
            ListBox listBox = sender as ListBox;

            if (listBox == null) return; // Ensure sender is a ListBox

            // Find the ListBoxItem under the mouse
            DependencyObject obj = e.OriginalSource as DependencyObject;
            while (obj != null && !(obj is ListBoxItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            // If we found a ListBoxItem, handle the selection
            if (obj is ListBoxItem item)
            {
                // Get the item from the ListBox
                var clickedItem = listBox.ItemContainerGenerator.ItemFromContainer(item);

                // If the item is not already selected, select it
                if (!listBox.SelectedItems.Contains(clickedItem))
                {
                    // Add to the selection without deselecting others
                    listBox.SelectedItems.Add(clickedItem);
                }

                // Optionally, focus the item
                item.Focus();

                // Mark the event as handled to prevent default behavior (deselection)
                e.Handled = true;
            }
        }

        private int GetGridInput(TextBox tex)
        {
            bool parsed = int.TryParse(tex.Text, out int vlaue);
            if (parsed) { return vlaue; } else { return 0; }
        }

        private void ChangedUVGrid(object sender, TextChangedEventArgs e)
        {
            //  MessageBox.Show($"{UVCanvas_Grid.ActualHeight} x {UVCanvas_Grid.ActualWidth}");
            DrawGrid(UVCanvas_Grid, GetGridInput(Input_UVMapperGridSize));
        }

        // Method to draw grid inside a Canvas

        private void ResizeUVCanvasElements()
        {

        }
        private void SetModeSculpt(object sender, RoutedEventArgs e)
        {
            List_Nodes.Visibility = Visibility.Collapsed;
            LabelNodes.Visibility = Visibility.Collapsed;
            List_Geosets.Visibility = Visibility.Collapsed;
            Label_Geosets.Visibility = Visibility.Collapsed;
            Panel_Rigging.Visibility = Visibility.Collapsed;
            Field_ModifyAmount.Visibility = Visibility.Collapsed;
            Field_WorldMatrix.Visibility = Visibility.Collapsed;
            editMode_current = EditMode.Sculpt;
            Manual.Visibility = Visibility.Collapsed;
            UVEditor.Visibility = Visibility.Collapsed;
            AnimatorPanel.Visibility = Visibility.Collapsed;
            EnableContextMenu(false);
            SetManualAvailability(false, false, false);
            SetActive(Button_Sculpt);
            SetContextMenuState(EditMode.Sculpt);
        }

        private void About_app(object sender, RoutedEventArgs e)
        {
            About_w about = new About_w();
            about.ShowDialog();
        }

        private void SetExtractEach(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            List<w3Triangle> newtriangles = new();

            if (triangles.Count > 0)
            {

                foreach (w3Triangle triangle in triangles)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        if (geo.Triangles.Contains(triangle))
                        {
                            w3Vertex v1 = triangle.Vertex1.Clone();
                            w3Vertex v2 = triangle.Vertex2.Clone();
                            w3Vertex v3 = triangle.Vertex3.Clone();
                            w3Triangle newtriangle = triangle.Clone();
                            newtriangle.Vertex1 = v1;
                            newtriangle.Vertex2 = v2;
                            newtriangle.Vertex3 = v3;
                            geo.Triangles.Add(newtriangle);
                            geo.Vertices.Add(v1);
                            geo.Vertices.Add(v2);
                            geo.Vertices.Add(v3);
                            newtriangles.Add(newtriangle);
                        }
                    }

                }
                foreach (w3Triangle triangle in triangles) triangle.isSelected = false;
                foreach (w3Triangle triangle in newtriangles) triangle.isSelected = true;
                modifyMode_current = ModifyMode.Translate;
                CallDetailsMenu(true, true, true, true, false);



            }


        }

        private void SetExtractEachAsNewGEoset(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            List<w3Geoset> newGeosets = new List<w3Geoset>();
            if (triangles.Count > 0)
            {


                foreach (w3Triangle triangle in triangles)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        if (geo.Triangles.Contains(triangle))
                        {
                            w3Geoset newGeoset = new w3Geoset();
                            w3Vertex v1 = triangle.Vertex1.Clone();
                            w3Vertex v2 = triangle.Vertex2.Clone();
                            w3Vertex v3 = triangle.Vertex3.Clone();
                            w3Triangle newtriangle = new w3Triangle();

                            newtriangle.Vertex1 = v1;
                            newtriangle.Vertex2 = v2;
                            newtriangle.Vertex3 = v3;
                            newGeoset.Triangles.Add(newtriangle);
                            newGeoset.Vertices.Add(v1);
                            newGeoset.Vertices.Add(v2);
                            newGeoset.Vertices.Add(v3);
                            newGeoset.ID = IDCounter.Next();
                            newGeoset.Material_ID = geo.Material_ID;
                            newGeosets.Add(newGeoset);
                            CurrentModel.Geosets.Add(newGeoset);
                        }
                    }

                }
                foreach (w3Triangle t in triangles) t.isSelected = false;
                foreach (w3Geoset geo in newGeosets)
                {
                    geo.Triangles[0].isSelected = true;
                }

                RefreshGeosetList();
                modifyMode_current = ModifyMode.Translate;
                CallDetailsMenu(true, true, true, false, false);



            }

        }


        // Function to draw a simple cube


        float CurrentZAngle = 0;
        private float GetAngle(bool plus)
        {
            if (plus)
            {
                CurrentZAngle = CurrentZAngle == 360 ? 0 : CurrentZAngle + 1; ; return CurrentZAngle;
            }
            CurrentZAngle = CurrentZAngle == 0 ? 360 : CurrentZAngle - 1; return CurrentZAngle;
        }
        private void AdjustCamera(OpenGL gl)
        {
            gl.Perspective(AppSettings.FieldOfView, (double)Width / Height, AppSettings.NearDistance, AppSettings.FarDistance);

            gl.LookAt(
           CameraControl.eyeX,
           CameraControl.eyeY,
           CameraControl.eyeZ,
           CameraControl.CenterX,
           CameraControl.CenterY,
           CameraControl.CenterZ,
           CameraControl.UpX,
           CameraControl.UpY,
           CameraControl.UpZ);
        }
        private void DrawScene(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            if (DrawPaused) { return; }
            OpenGL gl = args.OpenGL;
            w3Model Model = playingAnimation ? CurrentModelAnimated : CurrentModel;
            // Clear the color and depth buffer
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            // background color
            Scene_GL.ClearColor(AppSettings.BackgroundColor[0], AppSettings.BackgroundColor[1], AppSettings.BackgroundColor[2], 1.0f);
            // Set up the projection and model view matrices
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            // pespective
            AdjustCamera(gl);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            //culling
            Renderer.HandleCulling(gl);
            // anti anti aliasing
            Renderer.HandleAntiAliasing(gl);
            gl.Enable(OpenGL.GL_LINE_SMOOTH);  // Enable line smoothing
            gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);  // Use the nicest option for smoothing

            //----------------------------------------------------
            //independent rendering
            //----------------------------------------------------
            if (DisplayOptions.Axis) { Renderer.RenderAxis(gl, GridSize); }
            if (DisplayOptions.Extents) { Renderer.RenderExtents(gl, CurrentModel); }
            if (DisplayOptions.CollishionShapes) { Renderer.RenderCollisionShapes(gl, CurrentModel); }

            if (DisplayOptions.Rigging) { Renderer.RenderRigging(gl, CurrentModel); } // attachments
            if (DisplayOptions.Skeleton) { Renderer.RenderSkeleton(gl, CurrentModel); }
            if (DisplayOptions.Grid) { Renderer.RenderGrid(gl, GridSize, LineSpacing); }
            if (DisplayOptions.GridYZ) { Renderer.RenderYZGrid(gl, GridSize, LineSpacing); }
            if (DisplayOptions.GridXZ) { Renderer.RenderXZGrid(gl, GridSize, LineSpacing); }
            if (DisplayOptions.Extents) { Renderer.RenderExtents(gl, CurrentModel); }
            if (editMode_current == EditMode.Normals)
            {
                Renderer.RenderNormals(gl, Model);
            }

            else
            {
                if (DisplayOptions.Normals)
                {
                    if (DisplayOptions.Normals) Renderer.RenderNormals(gl, Model);
                }
            }
            //-----------------------------------------------------------------------------
            //----------------------------------------------------
            // rendering dependant on edit mode
            //----------------------------------------------------
            if (DisplayOptions.Textures)
            {
                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            }
            if (editMode_current == EditMode.UV)
            {
                Renderer.RenderTriangles(gl, CurrentModel, DisplayOptions.Textures);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);

            }
            else if (editMode_current == EditMode.Vertices)
            {

                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);
                Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);

            }
            else if (editMode_current == EditMode.Rigging)
            {
                Renderer.RenderVertices(gl, Model, null, false, false);

                Renderer.RenderNodes(gl, Model);
                if (DisplayOptions.Normals) Renderer.RenderNormals(gl, Model);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);

                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
            }
            else if (editMode_current == EditMode.Nodes)
            {
                Renderer.RenderNodes(gl, Model);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);

                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
            }
            else if (editMode_current == EditMode.Geosets)
            {
                if (DisplayOptions.Normals) Renderer.RenderNormals(gl, Model);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);


            }
            else if (editMode_current == EditMode.Edges)
            {

                Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Normals) { Renderer.RenderNormals(gl, Model); }
                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);
            }
            else if (editMode_current == EditMode.Triangles)
            {
                Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);

                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);


                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);

            }

            else if (editMode_current == EditMode.Animator)
            {
                Renderer.RenderNodes(gl, Model);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model, ModifiedGeometryForTrack);

                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures, ModifiedGeometryForTrack);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, ModifiedGeometryForTrack, playingAnimation, editMode_current == EditMode.Animator);

                //unfinished
            }
            else if (editMode_current == EditMode.Sculpt)
            {
                Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Normals) { Renderer.RenderNormals(gl, Model); }
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);

                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                if (DisplayOptions.Vertices) Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);
            }
            else if (editMode_current == EditMode.Normals)
            {
                Renderer.RenderNormals(gl, Model);


                if (DisplayOptions.Triangles) Renderer.RenderTriangles(gl, Model, DisplayOptions.Textures);
                Renderer.RenderVertices(gl, Model, null, playingAnimation, editMode_current == EditMode.Animator);
                if (DisplayOptions.Edges) Renderer.RenderEdges(gl, Model);
                if (DisplayOptions.Nodes) Renderer.RenderNodes(gl, Model);

            }
            Renderer.HandleLighting(gl);

            Renderer.HandeShading(gl);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            // Flush the OpenGL buffer to apply all the drawing commands
            gl.Flush();
        }

        private uint LoadTexture(OpenGL gl, Bitmap bitmap)
        {
            // Texture loading logic goes here, returning a texture ID
            // You may use OpenGL.GL.GenTextures and other OpenGL functions to upload the bitmap as a texture
            return 0; // Placeholder, actual texture ID should be returned
        }




        private void SetCameraLeft(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX = 0;
            CameraControl.eyeY = -180;
            CameraControl.eyeZ = 60;

            CameraControl.UpX = 0;
            CameraControl.UpY = 0;
            CameraControl.UpZ = 90;
        }


        private void SetCameraRight(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX = 0;
            CameraControl.eyeY = 180;
            CameraControl.eyeZ = 60;

            CameraControl.UpX = 0;
            CameraControl.UpY = 0;
            CameraControl.UpZ = 90;
        }


        private void SetCameraTop(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX = 10;
            CameraControl.eyeZ = 180;
            CameraControl.eyeY = 0;
            CameraControl.UpX = 0;
            CameraControl.UpY = 0;
            CameraControl.UpZ = 90;
        }

        private void SetCameraBottom(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX = 10;

            CameraControl.eyeY = 0;
            CameraControl.eyeZ = -180;
            CameraControl.UpX = 0;
            CameraControl.UpY = 0;
            CameraControl.UpZ = 90;
        }

        private void SetCameraFront(object sender, RoutedEventArgs e)
        {
            CameraControl.Reset();
        }

        private void SetCameraBack(object sender, RoutedEventArgs e)
        {
            CameraControl.eyeX = -180;
            CameraControl.eyeY = 0;
            CameraControl.eyeZ = 60;

            CameraControl.UpX = 0;
            CameraControl.UpY = 0;
            CameraControl.UpZ = 90;
        }

        private void ResetCamera(object sender, RoutedEventArgs e)
        {
            CameraControl.Reset();

        }

        private void colsChecked(object sender, RoutedEventArgs e)
        {

            DisplayOptions.CollishionShapes = CshapeExtentsCheckBox.IsChecked == true;
        }
        private void WireframeChecked(object sender, RoutedEventArgs e)
        {

            TrianglesCheckBox.IsChecked = false;
            EdgesCheckBox.IsChecked = true;

        }

        private void ManageTextureAnims(object sender, RoutedEventArgs e)
        {
            Texture_Animation_Editor ta = new Texture_Animation_Editor(CurrentModel);
            ta.ShowDialog();
            SetSaved(false);
        }

        private void ManageMaterials(object sender, RoutedEventArgs e)
        {
            Material_Manager mm = new Material_Manager(CurrentModel);
            mm.ShowDialog();
            RefreshBitmaps();
            RefreshTextureListForGeosets();
            RefreshUVTextureList();
            SetSaved(false);
        }


        public void DrawGrid(Canvas canvas, int cellSize)
        {

            if (cellSize == 0)
            {
                canvas.Children.Clear(); return;
            }

            // Clear existing lines
            canvas.Children.Clear();

            // Check if cellSize is valid
            if (cellSize <= 0) return;

            // Get the canvas dimensions
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            // Create a pen for the grid lines
            System.Windows.Media.Brush lineBrush = Brushes.Green;
            double lineThickness = 1.0;

            // Draw vertical lines
            for (int x = 0; x <= width; x += cellSize)
            {
                Line verticalLine = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = lineBrush,
                    StrokeThickness = lineThickness
                };
                canvas.Children.Add(verticalLine);
            }

            // Draw horizontal lines
            for (int y = 0; y <= height; y += cellSize)
            {
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = lineBrush,
                    StrokeThickness = lineThickness
                };
                canvas.Children.Add(horizontalLine);
            }
            //MessageBox.Show(canvas.Children.Count.ToString());
        }

        private void SetSceneGrid(object sender, TextChangedEventArgs e)
        {
            DrawGrid(Scene_Canvas_, GetGridInput(Input_SceneGrid));




        }



        enum SceneInteractionState
        {
            None,
            DrawRectangle,
            RotatingView,
            Modify
        }

        private SceneInteractionState CurrentSceneInteraction = SceneInteractionState.None;
        private Point selectionStart; // Starting point for selection rectangle
        private System.Windows.Shapes.Rectangle selectionRectangle; // Fully qualified path to avoid ambiguity
        private bool isRotating = false; // Flag for camera rotation

        private double rotationSpeed = 1000f; // Speed of rotation
        private double rotationAngleX = 0f; // Rotation angle for X-axis
        private double rotationAngleY = 0f; // Rotation angle for Y-axis
        private double rotationAngleZ = 0f; // Rotation angle for Y-axis




        private void Scene_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (CurrentSceneInteraction == SceneInteractionState.Modify && (e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed))
            {
                CurrentSceneInteraction = SceneInteractionState.None;
                return;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                // Toggle RotatingView mode when middle mouse button is pressed
                if (CurrentSceneInteraction == SceneInteractionState.RotatingView)
                {
                    // Turn off rotation mode
                    CurrentSceneInteraction = SceneInteractionState.None;
                    Scene_Canvas_.Cursor = Cursors.Arrow; ;

                }
                else
                {
                    // Turn on rotation mode
                    CurrentSceneInteraction = SceneInteractionState.RotatingView;
                    Scene_Canvas_.Cursor = Cursors.ScrollAll; ;


                    isRotating = true;
                    selectionStart = e.GetPosition(Scene_Canvas_); // Capture the starting point for rotation
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrentSceneInteraction == SceneInteractionState.RotatingView) { return; }
                // Toggle DrawRectangle mode when left mouse button is pressed
                if (CurrentSceneInteraction == SceneInteractionState.DrawRectangle)
                {
                    CurrentSceneInteraction = SceneInteractionState.None;
                    if (selectionRectangle != null)
                    {
                        Scene_Canvas_.Children.Remove(selectionRectangle);
                        selectionRectangle = null;
                    }
                }
                else
                {
                    CurrentSceneInteraction = SceneInteractionState.DrawRectangle;
                    selectionStart = e.GetPosition(Scene_Canvas_);


                    selectionRectangle = new System.Windows.Shapes.Rectangle
                    {
                        Stroke = Brushes.Blue,
                        StrokeThickness = 1,
                        Fill = Brushes.Transparent
                    };
                    Canvas.SetLeft(selectionRectangle, selectionStart.X);
                    Canvas.SetTop(selectionRectangle, selectionStart.Y);
                    double width = selectionRectangle.Width;
                    double height = selectionRectangle.Height;


                    Scene_Canvas_.Children.Add(selectionRectangle);
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (CurrentSceneInteraction == SceneInteractionState.RotatingView) { return; }
            }


        }


        private float minimumZoom = 5;
        private float distance = CameraControl.eyeX; // Camera's initial distance from the center

        private void Scene_Zoom(object sender, MouseWheelEventArgs e)
        {
            // Compute the direction vector from the eye to the center (0, 0, 0)
            Vector3 direction = new Vector3(CameraControl.eyeX, CameraControl.eyeY, CameraControl.eyeZ);

            // Normalize the direction vector
            direction = Vector3.Normalize(direction);

            // Adjust the zoom amount based on the mouse wheel movement
            if (e.Delta > 0) // Zoom in
            {
                if (distance <= minimumZoom) { return; }
                distance -= AppSettings.ZoomIncrement; // Decrease distance (zoom in)
            }
            else // Zoom out
            {
                distance += AppSettings.ZoomIncrement; // Increase distance (zoom out)
            }

            // Update the camera's position based on the new distance
            CameraControl.eyeX = direction.X * distance;
            CameraControl.eyeY = direction.Y * distance;
            CameraControl.eyeZ = direction.Z * distance;

            CameraControl.EndEyeX = CameraControl.eyeX;
            SetCameraControllerValues();
        }

        private float eulerX = 10; //forward-backward
        private float eulerY = 0; // left-right
        private float eulerZ = 10; // bottom-top
        private double LastClickPositionX = 0;
        private double LastClickPositionY = 0;
        enum CameraRotate
        {
            Left, Right, None
        }
        private CameraRotate cameraRotate = CameraRotate.None;
        bool ReverseRotationLogic = false;
        private bool RefreshBitmaps_ = true;
        private void Scene_MouseMove(object sender, MouseEventArgs e)
        {
            if (RefreshBitmaps_) { RefreshBitmaps_ = false; RefreshBitmaps(); }
            
            // Get the current mouse position
            var currentMousePos = e.GetPosition(Scene_Canvas_);
            double x = Math.Min(selectionStart.X, currentMousePos.X);
            double y = Math.Min(selectionStart.Y, currentMousePos.Y);
            if (CurrentSceneInteraction == SceneInteractionState.Modify)
            {
                switch (modifyMode_current)
                {
                    case ModifyMode.Translate:
                        if (editMode_current == EditMode.Nodes)
                        {
                            if (CurrentlySelectedNode != null)
                            {
                                if (axisMode == AxisMode.X)
                                {

                                    if (currentMousePos.X > LastClickPositionX) CurrentlySelectedNode.PivotPoint.X += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) CurrentlySelectedNode.PivotPoint.Y -= ModifyAmount;
                                    LastClickPositionX = currentMousePos.X;
                                }
                                else if (axisMode == AxisMode.Y)
                                {

                                    if (currentMousePos.X > LastClickPositionX) CurrentlySelectedNode.PivotPoint.Y += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) CurrentlySelectedNode.PivotPoint.Y -= ModifyAmount;
                                    LastClickPositionX = currentMousePos.X;
                                }
                                else if (axisMode == AxisMode.Z)
                                {

                                    if (currentMousePos.X > LastClickPositionX) CurrentlySelectedNode.PivotPoint.Z += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) CurrentlySelectedNode.PivotPoint.Z -= ModifyAmount;
                                    LastClickPositionX = currentMousePos.X;
                                }
                            }
                        }
                        else if (editMode_current == EditMode.Animator)
                        {
                            int track = CurrentSceneFrame;
                            if (track < 0) { return; }
                            if (CurrentlySelectedNode.Translation.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = CurrentlySelectedNode.Translation.Keyframes.First(x => x.Track == track);

                                if (axisMode == AxisMode.X)
                                {

                                    if (currentMousePos.X > LastClickPositionX) k.Data[0] += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[0] -= ModifyAmount;
                                }
                                else if (axisMode == AxisMode.Y)
                                {

                                    if (currentMousePos.X > LastClickPositionX) k.Data[1] += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[1] -= ModifyAmount;
                                }
                                else if (axisMode == AxisMode.Z)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[2] += ModifyAmount;
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[2] -= ModifyAmount;

                                }
                                LastClickPositionX = currentMousePos.X;
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe(track, CurrentlySelectedNode.PivotPoint.ToArray());
                                CurrentlySelectedNode.Translation.Keyframes.Add(k);
                                LastClickPositionX = currentMousePos.X;
                            }
                        }
                        else
                        {
                            if (axisMode == AxisMode.X)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.X += ModifyAmount;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.X -= ModifyAmount;
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Y)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Y += ModifyAmount;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Y -= ModifyAmount;
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Z)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Z += ModifyAmount;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Z -= ModifyAmount;
                                LastClickPositionX = currentMousePos.X;
                            }
                        }


                        break;
                    case ModifyMode.Rotate:
                        if (editMode_current == EditMode.Animator)
                        {
                            int track = CurrentSceneFrame;
                            if (track < 0) { return; }
                            if (CurrentlySelectedNode.Rotation.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = CurrentlySelectedNode.Rotation.Keyframes.First(x => x.Track == track);

                                if (axisMode == AxisMode.X)
                                {

                                    if (currentMousePos.X > LastClickPositionX) k.Data[0] = Calculator.SafeRotation(k.Data[0], true);
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[0] = Calculator.SafeRotation(k.Data[0], false);
                                }
                                else if (axisMode == AxisMode.Y)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[1] = Calculator.SafeRotation(k.Data[1], true);
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[1] = Calculator.SafeRotation(k.Data[1], false);
                                }
                                else if (axisMode == AxisMode.Z)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[2] = Calculator.SafeRotation(k.Data[2], true);
                                    else if (currentMousePos.X < LastClickPositionX) k.Data[2] = Calculator.SafeRotation(k.Data[2], false);

                                }
                                LastClickPositionX = currentMousePos.X;
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe(track, [0, 0, 0]);
                                CurrentlySelectedNode.Rotation.Keyframes.Add(k);
                                LastClickPositionX = currentMousePos.X;
                            }
                        }
                        else
                        {
                            if (axisMode == AxisMode.X)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                //CurrentRotateCentroid = Calculator3D.GetCentroid(vertices);
                                if (currentMousePos.X > LastClickPositionX) RotateVertices(vertices, axisMode, true, CurrentRotationCentroid, ModifyAmount);
                                else if (currentMousePos.X < LastClickPositionX) RotateVertices(vertices, axisMode, false, CurrentRotationCentroid, ModifyAmount);
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Y)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                //  CurrentRotateCentroid = Calculator3D.GetCentroid(vertices);
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) RotateVertices(vertices, axisMode, true, CurrentRotationCentroid, ModifyAmount);
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) RotateVertices(vertices, axisMode, false, CurrentRotationCentroid, ModifyAmount);
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Z)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                //  CurrentRotateCentroid = Calculator3D.GetCentroid(vertices);
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) RotateVertices(vertices, axisMode, true, CurrentRotationCentroid, ModifyAmount);
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) RotateVertices(vertices, axisMode, false, CurrentRotationCentroid, ModifyAmount);
                                LastClickPositionX = currentMousePos.X;
                            }
                        }


                        break;
                    case ModifyMode.ExpandEdges:
                        if (currentMousePos.X > LastClickPositionX) Calculator3D.ExpandEdges(SelectedEdges, true, ModifyAmount);
                        else if (currentMousePos.X < LastClickPositionX) Calculator3D.ExpandEdges(SelectedEdges, false, ModifyAmount);
                        break;
                    case ModifyMode.ExpandTriangles:
                        if (currentMousePos.X > LastClickPositionX) Calculator3D.ExpandTriangles(SelectedTriangles, true, ModifyAmount);
                        else if (currentMousePos.X < LastClickPositionX) Calculator3D.ExpandTriangles(SelectedTriangles, false, ModifyAmount);
                        break;
                    case ModifyMode.Scale:
                        if (editMode_current == EditMode.Animator)
                        {
                            int track = CurrentSceneFrame;
                            if (track < 0) { return; }
                            if (CurrentlySelectedNode.Scaling.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = CurrentlySelectedNode.Rotation.Keyframes.First(x => x.Track == track);

                                if (axisMode == AxisMode.X)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[0] *= 1 + ModifyAmount / 100;
                                    else if (currentMousePos.X < LastClickPositionX)
                                    {
                                        float newAmount = k.Data[0] *= 1 - ModifyAmount / 100;
                                        k.Data[0] = newAmount <= 0 ? 0 : k.Data[0];
                                    }

                                    LastClickPositionX = currentMousePos.X;
                                }
                                else if (axisMode == AxisMode.Y)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[1] *= 1 + ModifyAmount / 100;
                                    else if (currentMousePos.X < LastClickPositionX)
                                    {
                                        float newAmount = k.Data[1] *= 1 - ModifyAmount / 100;
                                        k.Data[1] = newAmount <= 0 ? 0 : k.Data[0];
                                    }

                                    LastClickPositionX = currentMousePos.X;
                                }
                                else if (axisMode == AxisMode.Z)
                                {
                                    if (currentMousePos.X > LastClickPositionX) k.Data[2] *= 1 + ModifyAmount / 100;
                                    else if (currentMousePos.X < LastClickPositionX)
                                    {
                                        float newAmount = k.Data[2] *= 1 - ModifyAmount / 100;
                                        k.Data[2] = newAmount <= 0 ? 0 : k.Data[2];
                                    }

                                    LastClickPositionX = currentMousePos.X;
                                }
                                else if (axisMode == AxisMode.U)
                                {
                                    if (currentMousePos.X > LastClickPositionX)
                                    {
                                        k.Data[0] *= 1 + ModifyAmount / 100;
                                        k.Data[1] *= 1 + ModifyAmount / 100;
                                        k.Data[2] *= 1 + ModifyAmount / 100;
                                    }
                                    else if (currentMousePos.X < LastClickPositionX)
                                    {
                                        float newAmount = k.Data[0] *= 1 - ModifyAmount / 100;
                                        k.Data[0] = newAmount <= 0 ? 0 : k.Data[0];
                                        newAmount = k.Data[1] *= 1 - ModifyAmount / 100;
                                        k.Data[1] = newAmount <= 0 ? 0 : k.Data[1];
                                        newAmount = k.Data[2] *= 1 - ModifyAmount / 100;
                                        k.Data[2] = newAmount <= 0 ? 0 : k.Data[2];
                                    }

                                    LastClickPositionX = currentMousePos.X;
                                }
                                LastClickPositionX = currentMousePos.X;
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe(track, [100, 100, 100]);
                                CurrentlySelectedNode.Scaling.Keyframes.Add(k);
                                LastClickPositionX = currentMousePos.X;
                            }


                        }
                        else
                        {
                            if (axisMode == AxisMode.X)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.X *= 1 + ModifyAmount / 100;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.X *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Y)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Y *= 1 + ModifyAmount / 100;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Y *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.Z)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Z *= 1 + ModifyAmount / 100;
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices) v.Position.Z *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                LastClickPositionX = currentMousePos.X;
                            }
                            else if (axisMode == AxisMode.U)
                            {
                                List<w3Vertex> vertices = GetSelectedVertices();
                                if (currentMousePos.X > LastClickPositionX) foreach (w3Vertex v in vertices)
                                    {
                                        v.Position.X *= 1 + ModifyAmount / 100;
                                        v.Position.Y *= 1 + ModifyAmount / 100;
                                        v.Position.Z *= 1 + ModifyAmount / 100;
                                    }
                                else if (currentMousePos.X < LastClickPositionX) foreach (w3Vertex v in vertices)
                                    {
                                        v.Position.X *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                        v.Position.Y *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                        v.Position.Z *= 1 - ModifyAmount / 100 <= 0 ? 1 : 1 - ModifyAmount / 100; ;
                                    }
                                LastClickPositionX = currentMousePos.X;
                            }
                        }


                        break;

                    case ModifyMode.Widen:

                        if (currentMousePos.X > LastClickPositionX)
                        {
                            foreach (w3Edge edge in SelectedEdges)
                            {
                                Calculator3D.SetDistanceBetweenPoints(edge.Vertex1, edge.Vertex2, ModifyAmount, true);
                            }

                        }
                        else if (currentMousePos.X < LastClickPositionX)
                        {
                            foreach (w3Edge edge in SelectedEdges)
                            {
                                Calculator3D.SetDistanceBetweenPoints(edge.Vertex1, edge.Vertex2, ModifyAmount, false);
                            }
                        }

                        LastClickPositionX = currentMousePos.X;
                        break;
                    case ModifyMode.RotateNormals: break;
                    case ModifyMode.Bevel: break;
                    case ModifyMode.Curve: break;
                    case ModifyMode.DetachGeoset: break;
                    case ModifyMode.Extend: break;
                    case ModifyMode.Extrude: break;
                    case ModifyMode.Extract:
                    case ModifyMode.ExtractGeoset:


                        foreach (w3Triangle t in TrianglesToExtract)
                        {
                            if (currentMousePos.X > LastClickPositionX)
                            {
                                if (axisMode == AxisMode.X) t.Vertex1.Position.X += ModifyAmount;
                                if (axisMode == AxisMode.Y) t.Vertex1.Position.Y += ModifyAmount;
                                if (axisMode == AxisMode.Z) t.Vertex1.Position.Z += ModifyAmount;
                                if (axisMode == AxisMode.Facing) { }
                            }
                            else if (currentMousePos.X < LastClickPositionX)
                            {
                                if (axisMode == AxisMode.X) t.Vertex1.Position.X -= ModifyAmount;
                                if (axisMode == AxisMode.Y) t.Vertex1.Position.Y -= ModifyAmount;
                                if (axisMode == AxisMode.Z) t.Vertex1.Position.Z -= ModifyAmount;

                            }
                            LastClickPositionX = currentMousePos.X;
                        }

                        break;
                    case ModifyMode.extracteachNewGeoset:
                    case ModifyMode.extractEach:

                        foreach (w3Triangle t in TrianglesToExtract)
                        {
                            if (currentMousePos.X > LastClickPositionX)
                            {
                                if (axisMode == AxisMode.X) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Y) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Z) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Facing) { }
                            }
                            else if (currentMousePos.X < LastClickPositionX)
                            {
                                if (axisMode == AxisMode.X) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Y) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Z) Calculator3D.IncrementCentroidOfTriangle(t.Vertex1, t.Vertex2, t.Vertex3, axisMode, currentMousePos.X > LastClickPositionX, ModifyAmount);
                                if (axisMode == AxisMode.Facing) MoveInFacingDirection(t); // only for each
                            }
                            LastClickPositionX = currentMousePos.X;
                        }
                        break;
                    case ModifyMode.Inset:

                        foreach (w3Triangle triangle in CurrentInsetCollection)
                        {
                            if (currentMousePos.X > LastClickPositionX) Calculator3D.InsetTriangle(triangle, true);
                            else if (currentMousePos.X < LastClickPositionX) Calculator3D.InsetTriangle(triangle, false);
                            LastClickPositionX = currentMousePos.X;

                        }

                        break;


                }
            }

            if (CurrentSceneInteraction == SceneInteractionState.DrawRectangle && e.LeftButton == MouseButtonState.Pressed)
            {
                if (editMode_current == EditMode.Geosets || editMode_current == EditMode.Sculpt) { return; }

                double width = Math.Abs(currentMousePos.X - selectionStart.X);
                double height = Math.Abs(currentMousePos.Y - selectionStart.Y);
                selectionRectangle.Width = width;
                selectionRectangle.Height = height;

                Canvas.SetLeft(selectionRectangle, x);
                Canvas.SetTop(selectionRectangle, y);
            }
            else if (CurrentSceneInteraction == SceneInteractionState.RotatingView && isRotating)
            {
                float angle = 0.5f;
                Coordinate center = new Coordinate(CameraControl.CenterX, CameraControl.CenterY, CameraControl.CenterZ);
                if (currentMousePos.Y > LastClickPositionY)
                {
                    CameraControl.RotateDown(center, angle);

                }
                else
                {
                    CameraControl.RotateUp(center, angle);
                }

                // Mouse moved to the right
                if (currentMousePos.X > LastClickPositionX)
                {


                    CameraControl.RotateLeft(center, angle);

                }
                // Mouse moved to the left
                else if (currentMousePos.X < LastClickPositionX)
                {
                    CameraControl.RotateRight(center, angle);
                }

                LastClickPositionX = currentMousePos.X;
                LastClickPositionY = currentMousePos.Y;


            }





        }
        private void MoveInFacingDirection(w3Triangle triangle)
        {
            FacingAngle angle = Calculator3D.GetTriangleFacingDirection(triangle);
            Calculator3D.MoveInFacingDirection(triangle, angle, ModifyAmount);
        }
        private void FillCentroidInRaw(Coordinate centroid)
        {
            inputX.Text = centroid.X.ToString();
            inputY.Text = centroid.Y.ToString();
            inputZ.Text = centroid.Z.ToString();
            inputXr.Text = "0";
            inputYr.Text = "0";
            inputZr.Text = "0";
            InputXs.Text = "100";
            InputYs.Text = "100";
            InputZs.Text = "100";
        }
        private void RotateVertices(List<w3Vertex> vertices, AxisMode axis, bool positive, Coordinate centroid, float amount)
        {
            if (vertices.Count <= 1) { return; } // Single vertex can't be rotated
            if (amount <= 0) { return; } // No rotation needed for non-positive amounts

            // Convert amount in degrees to radians
            float angle = (positive ? 1 : -1) * MathF.PI / 180 * amount;

            foreach (w3Vertex v in vertices)
            {
                // Translate the vertex to rotate around the centroid
                float x = v.Position.X - centroid.X;
                float y = v.Position.Y - centroid.Y;
                float z = v.Position.Z - centroid.Z;

                float newX = x, newY = y, newZ = z;

                // Apply the appropriate rotation matrix depending on the axis
                if (axis == AxisMode.X)
                {
                    newY = y * MathF.Cos(angle) - z * MathF.Sin(angle);
                    newZ = y * MathF.Sin(angle) + z * MathF.Cos(angle);
                }
                else if (axis == AxisMode.Y)
                {
                    newX = x * MathF.Cos(angle) + z * MathF.Sin(angle);
                    newZ = -x * MathF.Sin(angle) + z * MathF.Cos(angle);
                }
                else if (axis == AxisMode.Z)
                {
                    newX = x * MathF.Cos(angle) - y * MathF.Sin(angle);
                    newY = x * MathF.Sin(angle) + y * MathF.Cos(angle);
                }

                // Translate the vertex back to its original position, relative to the centroid
                v.Position = new Coordinate
                {
                    X = newX + centroid.X,
                    Y = newY + centroid.Y,
                    Z = newZ + centroid.Z
                };
            }
        }


        private void RotateVerticesFixed(List<w3Vertex> vertices, AxisMode axis, float angle, Coordinate aroundWhichCentroid)
        {
            // Angle will always be between -360 and 360

            if (vertices.Count <= 1) { return; } // single vertex can't be rotated

            // Convert angle to radians for trigonometric functions
            float radians = angle * (float)Math.PI / 180.0f;

            foreach (w3Vertex v in vertices)
            {
                // Translate the vertex to rotate around the origin (centroid)
                float x = v.Position.X - aroundWhichCentroid.X;
                float y = v.Position.Y - aroundWhichCentroid.Y;
                float z = v.Position.Z - aroundWhichCentroid.Z;

                float newX = x, newY = y, newZ = z;

                // Apply the appropriate rotation matrix depending on the axis
                if (axis == AxisMode.X)
                {
                    // Rotation around X-axis (y-z plane)
                    newY = (float)(y * Math.Cos(radians) - z * Math.Sin(radians));
                    newZ = (float)(y * Math.Sin(radians) + z * Math.Cos(radians));
                }
                else if (axis == AxisMode.Y)
                {
                    // Rotation around Y-axis (x-z plane)
                    newX = (float)(x * Math.Cos(radians) + z * Math.Sin(radians));
                    newZ = (float)(-x * Math.Sin(radians) + z * Math.Cos(radians));
                }
                else if (axis == AxisMode.Z)
                {
                    // Rotation around Z-axis (x-y plane)
                    newX = (float)(x * Math.Cos(radians) - y * Math.Sin(radians));
                    newY = (float)(x * Math.Sin(radians) + y * Math.Cos(radians));
                }

                // Translate the vertex back to its original position
                v.Position = new Coordinate
                {
                    X = newX + aroundWhichCentroid.X,
                    Y = newY + aroundWhichCentroid.Y,
                    Z = newZ + aroundWhichCentroid.Z
                };
            }
        }

        private void Scene_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentSceneInteraction == SceneInteractionState.Modify)
            {
                CurrentSceneInteraction = SceneInteractionState.None;
                AppendHistory();

                return;
            }
            if (editMode_current == EditMode.Geosets || editMode_current == EditMode.Sculpt) { return; }
            if (CurrentSceneInteraction == SceneInteractionState.DrawRectangle)
            {
                // Finalize the selection rectangle
                var currentMousePos = e.GetPosition(Scene_Canvas_);
                double width = Math.Abs(currentMousePos.X - selectionStart.X);
                double height = Math.Abs(currentMousePos.Y - selectionStart.Y);
                selectionRectangle.Width = width;
                selectionRectangle.Height = height;

                // Switch back to None after completing the rectangle drawing
                CurrentSceneInteraction = SceneInteractionState.None;
                if (selectionRectangle != null)
                {
                    Scene_Canvas_.Children.Remove(selectionRectangle);
                    selectionRectangle = null;
                }


                RectData canvasSelectionRectangleCoordinates = GetRectangleCoordinates(selectionStart.X, selectionStart.Y, height);
                if (Check_LockSelection.IsChecked == true) { return; }
                Extent SelectionExtent = GetSelectionExtent(canvasSelectionRectangleCoordinates, Scene_GL);

                CreateCuboidFromExtent(SelectionExtent);

                SetSelectionBasedOnExtent(SelectionExtent);// not tested

                UpdateRiggingData();
                UpdateSelectionData();
                UpdateSelectionRawData();


            }

        }
        private void UpdateSelectionRawData()
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            Coordinate cuboid = Calculator3D.GetCentroid(vertices);
            inputX.Text = cuboid.X.ToString();
            inputY.Text = cuboid.Y.ToString();
            inputZ.Text = cuboid.Z.ToString();
            InputXs.Text = "100";
            InputYs.Text = "100";
            InputZs.Text = "100";
            inputXr.Text = "0";
            inputYr.Text = "0";
            inputZr.Text = "0";
        }

        private void CreateCuboidFromExtent(Extent selectionExtent)
        {
            return; //test
            w3Geoset geoset = new w3Geoset();

            // Define the eight vertices of the cuboid
            w3Vertex[] vertices = new w3Vertex[8];
            vertices[0] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Minimum_X, Y = selectionExtent.Minimum_Y, Z = selectionExtent.Minimum_Z } }; // Bottom-left-front
            vertices[1] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Maximum_X, Y = selectionExtent.Minimum_Y, Z = selectionExtent.Minimum_Z } }; // Bottom-right-front
            vertices[2] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Maximum_X, Y = selectionExtent.Maximum_Y, Z = selectionExtent.Minimum_Z } }; // Top-right-front
            vertices[3] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Minimum_X, Y = selectionExtent.Maximum_Y, Z = selectionExtent.Minimum_Z } }; // Top-left-front
            vertices[4] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Minimum_X, Y = selectionExtent.Minimum_Y, Z = selectionExtent.Maximum_Z } }; // Bottom-left-back
            vertices[5] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Maximum_X, Y = selectionExtent.Minimum_Y, Z = selectionExtent.Maximum_Z } }; // Bottom-right-back
            vertices[6] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Maximum_X, Y = selectionExtent.Maximum_Y, Z = selectionExtent.Maximum_Z } }; // Top-right-back
            vertices[7] = new w3Vertex { Position = new Coordinate { X = selectionExtent.Minimum_X, Y = selectionExtent.Maximum_Y, Z = selectionExtent.Maximum_Z } }; // Top-left-back

            // Add vertices to the geoset
            foreach (var vertex in vertices)
            {
                geoset.Vertices.Add(vertex);
            }

            // Define the triangles (faces) of the cuboid
            // Each face consists of two triangles
            w3Triangle[] triangles = new w3Triangle[12];

            // Front face
            triangles[0] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[1], Vertex3 = vertices[2] };
            triangles[1] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[2], Vertex3 = vertices[3] };

            // Back face
            triangles[2] = new w3Triangle { Vertex1 = vertices[4], Vertex2 = vertices[5], Vertex3 = vertices[6] };
            triangles[3] = new w3Triangle { Vertex1 = vertices[4], Vertex2 = vertices[6], Vertex3 = vertices[7] };

            // Left face
            triangles[4] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[3], Vertex3 = vertices[7] };
            triangles[5] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[7], Vertex3 = vertices[4] };

            // Right face
            triangles[6] = new w3Triangle { Vertex1 = vertices[1], Vertex2 = vertices[5], Vertex3 = vertices[6] };
            triangles[7] = new w3Triangle { Vertex1 = vertices[1], Vertex2 = vertices[6], Vertex3 = vertices[2] };

            // Top face
            triangles[8] = new w3Triangle { Vertex1 = vertices[3], Vertex2 = vertices[2], Vertex3 = vertices[6] };
            triangles[9] = new w3Triangle { Vertex1 = vertices[3], Vertex2 = vertices[6], Vertex3 = vertices[7] };

            // Bottom face
            triangles[10] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[1], Vertex3 = vertices[5] };
            triangles[11] = new w3Triangle { Vertex1 = vertices[0], Vertex2 = vertices[5], Vertex3 = vertices[4] };

            // Add triangles to the geoset
            foreach (var triangle in triangles)
            {
                geoset.Triangles.Add(triangle);
            }

            // Optionally, set the geoset's properties here
            // e.g., setting the material, etc.

            // Now you can add the geoset to your model or scene
            foreach (w3Vertex v in geoset.Vertices) v.AttachedTo.Add(CurrentModel.Nodes[0].objectId);
            geoset.Material_ID = CurrentModel.Materials[0].ID;
            geoset.ID = IDCounter.Next();
            CurrentModel.Geosets.Add(geoset);
            RefreshGeosetList();
        }


        public Extent GetSelectionExtent(RectData canvasSelectionRectangleCoordinates, OpenGL gl)
        {
            // Get canvas dimensions
            double canvasWidth = Scene_Canvas_.ActualWidth;
            double canvasHeight = Scene_Canvas_.ActualHeight;

            // Initialize the selection extent
            Extent selectionExtent = new Extent();

            // Calculate the aspect ratio
            float aspectRatio = (float)canvasWidth / (float)canvasHeight;


            float nearPlaneDistance = AppSettings.NearDistance; // Adjust as needed
            float farPlaneDistance = AppSettings.FarDistance; // Adjust as needed
            float fieldOfViewY = AppSettings.FieldOfView;


            // Define camera parameters

            float top = nearPlaneDistance * (float)Math.Tan(Math.PI * fieldOfViewY / 360);
            float right = top * aspectRatio;

            // Create frustum corners in camera space
            Vector3[] frustumCorners = new Vector3[8];

            // Near plane corners
            frustumCorners[0] = new Vector3(-right, -top, -nearPlaneDistance);
            frustumCorners[1] = new Vector3(right, -top, -nearPlaneDistance);
            frustumCorners[2] = new Vector3(right, top, -nearPlaneDistance);
            frustumCorners[3] = new Vector3(-right, top, -nearPlaneDistance);

            // Far plane corners
            frustumCorners[4] = new Vector3(-right, -top, -farPlaneDistance);
            frustumCorners[5] = new Vector3(right, -top, -farPlaneDistance);
            frustumCorners[6] = new Vector3(right, top, -farPlaneDistance);
            frustumCorners[7] = new Vector3(-right, top, -farPlaneDistance);

            // Transform frustum corners to world coordinates using camera parameters
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                Vector3 worldPosition = TransformToWorldCoordinates(frustumCorners[i]);

                // Update the selection extent
                selectionExtent.Minimum_X = Math.Min(selectionExtent.Minimum_X, worldPosition.X);
                selectionExtent.Minimum_Y = Math.Min(selectionExtent.Minimum_Y, worldPosition.Y);
                selectionExtent.Minimum_Z = Math.Min(selectionExtent.Minimum_Z, worldPosition.Z);
                selectionExtent.Maximum_X = Math.Max(selectionExtent.Maximum_X, worldPosition.X);
                selectionExtent.Maximum_Y = Math.Max(selectionExtent.Maximum_Y, worldPosition.Y);
                selectionExtent.Maximum_Z = Math.Max(selectionExtent.Maximum_Z, worldPosition.Z);
            }

            // Now compare the canvas selection rectangle with the frustum extent
            // Calculate normalized device coordinates (NDC)
            float left = (float)(canvasSelectionRectangleCoordinates.TopLeft / canvasWidth * 2 - 1);
            float rightNDC = (float)(canvasSelectionRectangleCoordinates.TopRight / canvasWidth * 2 - 1);
            float bottom = (float)(canvasSelectionRectangleCoordinates.BottomLeft / canvasHeight * 2 - 1);
            float topNDC = (float)(canvasSelectionRectangleCoordinates.TopRight / canvasHeight * 2 - 1);

            // Calculate the selected extent based on the NDC
            Extent finalSelectionExtent = new Extent()
            {
                Minimum_X = Math.Max(selectionExtent.Minimum_X, left * (selectionExtent.Maximum_X - selectionExtent.Minimum_X) / 2 + (selectionExtent.Maximum_X + selectionExtent.Minimum_X) / 2),
                Minimum_Y = Math.Max(selectionExtent.Minimum_Y, bottom * (selectionExtent.Maximum_Y - selectionExtent.Minimum_Y) / 2 + (selectionExtent.Maximum_Y + selectionExtent.Minimum_Y) / 2),
                Maximum_X = Math.Min(selectionExtent.Maximum_X, rightNDC * (selectionExtent.Maximum_X - selectionExtent.Minimum_X) / 2 + (selectionExtent.Maximum_X + selectionExtent.Minimum_X) / 2),
                Maximum_Y = Math.Min(selectionExtent.Maximum_Y, topNDC * (selectionExtent.Maximum_Y - selectionExtent.Minimum_Y) / 2 + (selectionExtent.Maximum_Y + selectionExtent.Minimum_Y) / 2)
            };

            // Return the final selection extent
            return finalSelectionExtent;
        }

        // Transform from view space to world space using the camera's parameters
        private Vector3 TransformToWorldCoordinates(Vector3 viewSpacePoint)
        {
            // Apply camera transformation here based on CameraControl properties
            // Create the transformation matrix based on camera position and orientation
            float[] translationMatrix = new float[16];

            // Assuming eye position is (eyeX, eyeY, eyeZ)
            // Note: You may need to include rotation transformation here
            translationMatrix[12] = -CameraControl.eyeX; // translation in X
            translationMatrix[13] = -CameraControl.eyeY; // translation in Y
            translationMatrix[14] = -CameraControl.eyeZ; // translation in Z

            // Assuming no rotation for this example (you should adjust for rotation)
            float x = viewSpacePoint.X + translationMatrix[12];
            float y = viewSpacePoint.Y + translationMatrix[13];
            float z = viewSpacePoint.Z + translationMatrix[14];

            return new Vector3(x, y, z);
        }


        private RectData GetRectangleCoordinates(double from, double to, double height)
        {
            // Create a new instance of RectData
            var rectData = new RectData();

            // Set the coordinates for the rectangle
            rectData.TopLeft = from;      // X-coordinate for the top left corner
            rectData.TopRight = to;       // X-coordinate for the top right corner
            rectData.BottomLeft = from + height;  // Y-coordinate for the bottom left corner
            rectData.BottomRight = to + height;   // Y-coordinate for the bottom right corner

            return rectData;
        }



        private void SetGridSize(object sender, TextChangedEventArgs e)
        {
            GridSize = 10;
            bool parsed = int.TryParse(Input_GridSize.Text, out int newGridSize);
            if (parsed)
            {
                if (newGridSize > 0 && newGridSize <= 10000)
                {
                    GridSize = newGridSize;
                }

            }
        }

        private void SetLineSpacing(object sender, TextChangedEventArgs e)
        {
            LineSpacing = 1;
            bool parsed = int.TryParse(InputLineSpacing.Text, out int newLineSpacing);
            if (parsed)
            {
                LineSpacing = newLineSpacing;
            }
        }

        private void SetCameraControllerValues()
        {
            Camera_Editor.InputX.Text = CameraControl.eyeX.ToString();
            Camera_Editor.InputY.Text = CameraControl.eyeY.ToString();
            Camera_Editor.InputZ.Text = CameraControl.eyeZ.ToString();
            Camera_Editor.InputCX.Text = CameraControl.CenterX.ToString();
            Camera_Editor.InputCY.Text = CameraControl.CenterY.ToString();
            Camera_Editor.InputCZ.Text = CameraControl.CenterZ.ToString();
            Camera_Editor.InputUX.Text = CameraControl.UpX.ToString();
            Camera_Editor.InputUY.Text = CameraControl.UpY.ToString();
            Camera_Editor.InputUZ.Text = CameraControl.UpZ.ToString();
        }
        private void SetCameraCustom(object sender, RoutedEventArgs e)
        {
            Camera_Editor.Show();
            SetCameraControllerValues();
        }

        private void ClosingApp(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Saved)
            {
                e.Cancel = true;
                if (!Saved)
                {
                    MessageBoxResult result = MessageBox.Show("The model is not saved. Save?", "Model not saved", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCurrentModel(null, null);



                        if (SaveLocation == "")
                        {
                            Saveas(null, null);
                        }
                        else
                        {

                            File.WriteAllText(SaveLocation, CurrentModel.ToMDL());
                            Environment.Exit(0);
                        }
                    }
                    if (result == MessageBoxResult.No)

                    {
                        Environment.Exit(0);
                    }
                    if (result == MessageBoxResult.Cancel) { return; }
                }

            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void VerticesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Vertices = VerticesCheckBox.IsChecked == true;
        }

        private void GeosetExtentsChecked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Extents = GeosetExtentsCheckBox.IsChecked == true;
        }

        private void PointsChecked(object sender, RoutedEventArgs e)
        {
            TrianglesCheckBox.IsChecked = false;
            VerticesCheckBox.IsChecked = true;
            EdgesCheckBox.IsChecked = false;
        }

        private void OpenRecent(object sender, RoutedEventArgs e)
        {

            MenuItem item = sender as MenuItem;
            string name = item.Header.ToString();
            if (name == "Open Recent") { return; }

            OpenModel(name);
        }

        private void CallOptimizer(object sender, RoutedEventArgs e)
        {
            Optimizer ops = new Optimizer(CurrentModel);
            ops.ShowDialog();
            if (ops.DialogResult == true)
            {
                RefreshGeosetList();
                RefreshNodesList();

                RefreshSequencesList();


            }
        }

        private void CallErrorChecker(object sender, RoutedEventArgs e)
        {
            ErrorChecker checker = new ErrorChecker(CurrentModel);
            checker.ShowDialog();
        }

        private void CreateTCBG(object sender, RoutedEventArgs e)
        {
            w3Geoset geo = new w3Geoset();
            w3Vertex v1 = new w3Vertex();
            v1.Position = new Coordinate(-578.092f, -333.512f, 580.334f);
            v1.Normal = new Coordinate(0.765471f, -0.575163f, 0.288514f);
            v1.Texture_Position = new Coordinate2D(0, 0);
            w3Vertex v2 = new w3Vertex();
            v2.Position = new Coordinate(-451.113f, -566.069f, -246.582f);
            v1.Normal = new Coordinate(0.765471f, -0.575163f, 0.288514f);
            v1.Texture_Position = new Coordinate2D(0, 1);
            w3Vertex v3 = new w3Vertex();
            v2.Position = new Coordinate(38.0481f, 676.973f, 381.341f);
            v1.Normal = new Coordinate(0.765471f, -0.575163f, 0.288514f);
            v1.Texture_Position = new Coordinate2D(1, 0);
            w3Vertex v4 = new w3Vertex();
            v2.Position = new Coordinate(165.027f, 444.415f, -445.574f);
            v1.Normal = new Coordinate(0.765471f, -0.575163f, 0.288514f);
            v1.Texture_Position = new Coordinate2D(1, 1);
            w3Triangle t1 = new w3Triangle(v1, v2, v3);
            w3Triangle t2 = new w3Triangle(v4, v3, v2);
            geo.Vertices.Add(v1);
            geo.Vertices.Add(v2);
            geo.Vertices.Add(v3);
            geo.Vertices.Add(v4);
            geo.Triangles.Add(t1);
            geo.Triangles.Add(t2);
            geo.ID = IDCounter.Next();
            geo.Material_ID = GetHeroGlowTexture();
            w3Node node = new w3Node();
            node.Data = new Bone();
            node.objectId = IDCounter.Next();
            node.Name = $"TeamBackgroundPlane_{node.objectId}";
            foreach (w3Vertex v in geo.Vertices) { v.AttachedTo.Add(node.objectId); }
            CurrentModel.Nodes.Add(node);

            // add material
            // attach to bone
            geo.RecalculateEdges();
            CurrentModel.Geosets.Add(geo);
            RefreshNodesList();
            RefreshGeosetList();
        }

        private int GetHeroGlowTexture()
        {
            if (CurrentModel.Textures.Any(x => x.Replaceable_ID == 2))
            {
                int id = CurrentModel.Textures.First(x => x.Replaceable_ID == 2).ID;

                foreach (w3Material mat in CurrentModel.Materials)
                {
                    if (mat.Layers[0].Diffuse_Texure_ID.isStatic && mat.Layers[0].Diffuse_Texure_ID.StaticValue[0] == id)
                    {
                        return mat.ID;
                    }
                }
                // didnt find
                w3Material mat2 = new w3Material();
                mat2.ID = IDCounter.Next();
                w3Layer l = new w3Layer();
                l.ID = IDCounter.Next();
                l.Diffuse_Texure_ID.isStatic = true;
                l.Diffuse_Texure_ID.StaticValue = [id];
                mat2.Layers.Add(l);
                CurrentModel.Materials.Add(mat2);
                return mat2.ID;
            }
            else
            {
                w3Texture tex = new w3Texture();
                tex.Replaceable_ID = 2;
                tex.ID = IDCounter.Next();
                CurrentModel.Textures.Add(tex);
                w3Material mat2 = new w3Material();
                mat2.ID = IDCounter.Next();
                w3Layer l = new w3Layer();
                l.ID = IDCounter.Next();
                l.Diffuse_Texure_ID.isStatic = true;
                l.Diffuse_Texure_ID.StaticValue = [tex.ID];
                mat2.Layers.Add(l);
                CurrentModel.Materials.Add(mat2);
                return mat2.ID;
            }
        }

        private void CreateHEROAURA(object sender, RoutedEventArgs e)
        {
            w3Geoset geo = new w3Geoset();
            // Assuming Coordinate is a class that accepts three doubles (X, Y, Z)
            // and Coordinate2D is a class that accepts two doubles (X, Y)

            // Assuming geo is an object that has a Vertices list
            w3Vertex v1 = new w3Vertex();
            v1.Position = new Coordinate(-139.807, -130.78, 9.01961);
            v1.Normal = new Coordinate(0, 0, 1);
            v1.Texture_Position = new Coordinate2D(0.000499785, 0.000499547);
            geo.Vertices.Add(v1);

            w3Vertex v2 = new w3Vertex();
            v2.Position = new Coordinate(121.012, -130.78, 9.01961);
            v2.Normal = new Coordinate(0, 0, 1);
            v2.Texture_Position = new Coordinate2D(0.000499547, 0.9995);
            geo.Vertices.Add(v2);

            w3Vertex v3 = new w3Vertex();
            v3.Position = new Coordinate(-139.807, 130.039, 9.01961);
            v3.Normal = new Coordinate(0, 0, 1);
            v3.Texture_Position = new Coordinate2D(0.9995, 0.000499785);
            geo.Vertices.Add(v3);

            w3Vertex v4 = new w3Vertex();
            v4.Position = new Coordinate(121.012, 130.039, 9.01961);
            v4.Normal = new Coordinate(0, 0, 1);
            v4.Texture_Position = new Coordinate2D(0.9995, 0.9995);
            geo.Vertices.Add(v4);

            w3Vertex v5 = new w3Vertex();
            v5.Position = new Coordinate(-133.307, -124.28, 7.706);
            v5.Normal = new Coordinate(0, 0, 1);
            v5.Texture_Position = new Coordinate2D(0.000499785, 0.000499547);
            geo.Vertices.Add(v5);

            w3Vertex v6 = new w3Vertex();
            v6.Position = new Coordinate(114.512, -124.28, 7.706);
            v6.Normal = new Coordinate(0, 0, 1);
            v6.Texture_Position = new Coordinate2D(0.000499547, 0.9995);
            geo.Vertices.Add(v6);

            w3Vertex v7 = new w3Vertex();
            v7.Position = new Coordinate(-133.307, 123.539, 7.706);
            v7.Normal = new Coordinate(0, 0, 1);
            v7.Texture_Position = new Coordinate2D(0.9995, 0.000499785);
            geo.Vertices.Add(v7);

            w3Vertex v8 = new w3Vertex();
            v8.Position = new Coordinate(114.512, 123.539, 7.706);
            v8.Normal = new Coordinate(0, 0, 1);
            v8.Texture_Position = new Coordinate2D(0.9995, 0.9995);
            geo.Vertices.Add(v8);

            w3Vertex v9 = new w3Vertex();
            v9.Position = new Coordinate(-148.935, -139.908, 17.3682);
            v9.Normal = new Coordinate(0, 0, 1);
            v9.Texture_Position = new Coordinate2D(0.000499785, 0.000499547);
            geo.Vertices.Add(v9);

            w3Vertex v10 = new w3Vertex();
            v10.Position = new Coordinate(130.141, -139.908, 17.3682);
            v10.Normal = new Coordinate(0, 0, 1);
            v10.Texture_Position = new Coordinate2D(0.000499547, 0.9995);
            geo.Vertices.Add(v10);

            w3Vertex v11 = new w3Vertex();
            v11.Position = new Coordinate(-148.935, 139.167, 17.3682);
            v11.Normal = new Coordinate(0, 0, 1);
            v11.Texture_Position = new Coordinate2D(0.9995, 0.000499785);
            geo.Vertices.Add(v11);

            w3Vertex v12 = new w3Vertex();
            v12.Position = new Coordinate(130.141, 139.167, 17.3682);
            v12.Normal = new Coordinate(0, 0, 1);
            v12.Texture_Position = new Coordinate2D(0.9995, 0.9995);
            geo.Vertices.Add(v12);

            w3Vertex v13 = new w3Vertex();
            v13.Position = new Coordinate(-113.53, -104.503, 28.0158);
            v13.Normal = new Coordinate(0, 0, 1);
            v13.Texture_Position = new Coordinate2D(0.000499785, 0.000499547);
            geo.Vertices.Add(v13);

            w3Vertex v14 = new w3Vertex();
            v14.Position = new Coordinate(94.7354, -104.503, 28.0158);
            v14.Normal = new Coordinate(0, 0, 1);
            v14.Texture_Position = new Coordinate2D(0.000499547, 0.9995);
            geo.Vertices.Add(v14);

            w3Vertex v15 = new w3Vertex();
            v15.Position = new Coordinate(-113.53, 103.762, 28.0158);
            v15.Normal = new Coordinate(0, 0, 1);
            v15.Texture_Position = new Coordinate2D(0.9995, 0.000499785);
            geo.Vertices.Add(v15);

            w3Vertex v16 = new w3Vertex();
            v16.Position = new Coordinate(94.7354, 103.762, 28.0158);
            v16.Normal = new Coordinate(0, 0, 1);
            v16.Texture_Position = new Coordinate2D(0.9995, 0.9995);
            geo.Vertices.Add(v16);
            geo.ID = IDCounter.Next();
            geo.Triangles.Add(new w3Triangle(v1, v2, v3));
            geo.Triangles.Add(new w3Triangle(v4, v3, v2));
            geo.Triangles.Add(new w3Triangle(v5, v6, v7));
            geo.Triangles.Add(new w3Triangle(v8, v7, v6));
            geo.Triangles.Add(new w3Triangle(v9, v10, v11));
            geo.Triangles.Add(new w3Triangle(v12, v11, v10));
            geo.Triangles.Add(new w3Triangle(v13, v14, v15));
            geo.Triangles.Add(new w3Triangle(v16, v15, v14));
            geo.Material_ID = GetHeroGlowTexture();
            w3Node node = new w3Node();
            node.Data = new Bone();
            node.objectId = IDCounter.Next();
            node.Name = $"HeroGlow_{node.objectId}";
            foreach (w3Vertex v in geo.Vertices) { v.AttachedTo.Add(node.objectId); }
            CurrentModel.Nodes.Add(node);

            // add material
            // attach to bone
            geo.RecalculateEdges();
            CurrentModel.Geosets.Add(geo);
            RefreshNodesList();
            RefreshGeosetList();
        }
        private void SelectGeometryBasedOnSelectionExtent(Extent extent)
        {
            if (Check_LockSelection.IsChecked == true) { return; }
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.isVisible == true)
                {
                    if (editMode_current == EditMode.Vertices)
                    {
                        foreach (w3Vertex v in geo.Vertices)
                        {
                            if (CoordinateWithinExtents(v.Position, extent))
                            {
                                if (selectionMode == SelectionMode.RemoveSelect)
                                {
                                    v.isSelected = false;

                                }
                                else
                                {
                                    v.isSelected = true;
                                }
                            }
                            else
                            {
                                if (selectionMode == SelectionMode.ClearAndSelect)
                                {
                                    v.isSelected = false;
                                }

                            }

                        }
                    }
                    if (editMode_current == EditMode.Triangles)
                    {

                    }
                    if (editMode_current == EditMode.Edges)
                    {

                    }
                    if (editMode_current == EditMode.Normals)
                    {

                    }
                }
            }
        }
        private bool CoordinateWithinExtents(Coordinate coord, Extent extent)
        {
            return coord.X >= extent.Minimum_X && coord.X <= extent.Maximum_X &&
                   coord.Y >= extent.Minimum_Y && coord.Y <= extent.Maximum_Y &&
                   coord.Z >= extent.Minimum_Z && coord.Z <= extent.Maximum_Z;
        }

        private void TrianglesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DisplayOptions.Triangles = TrianglesCheckBox.IsChecked == true;
        }







        private void ExportGeoset(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();

            if (geosets.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (w3Geoset geoset in geosets)
                {
                    sb.AppendLine(geoset.ToWhimGeoset());
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();

                // Set properties for .whim files
                saveFileDialog.Filter = "WHIMGEOSET files (*.whimgeoset)|*.whimgeoset";  // Restrict to .whim files
                saveFileDialog.Title = "Select a Save Location";        // Dialog title
                saveFileDialog.DefaultExt = ".whimgeoset";                    // Default extension


                if (saveFileDialog.ShowDialog() == true)
                {

                    SaveLocation = saveFileDialog.FileName;
                    File.WriteAllText(SaveLocation, sb.ToString());
                }

            }
        }

        private void ImportGeoset(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0)
            {
                MessageBox.Show("There are no materials present to give an imported geoset", "Precaution"); return;
            }

            string path = GetImportedGeoset();
            if (path != null & path.Length > 0)
            {

                List<w3Geoset> importedGEosets = WhimGeosetImporter.Work(File.ReadAllText(path));


                import_geoset ig = new import_geoset(path, CurrentModel, importedGEosets);
                ig.ShowDialog();
                if (ig.DialogResult == true)
                {
                    CurrentModel.RefreshSequenceExtents();
                    CurrentModel.CalculateGeosetBoundingBoxes();
                    CurrentModel.RefreshEdges();
                    RefreshGeosetList();
                    RefreshNodesList();
                    RefreshTextureListForGeosets();

                }
            }


        }

        private void Exitapp(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void AimAtTriange(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            else
            {
                InputCoordinate c = new InputCoordinate();
                c.ShowDialog();
                if (c.DialogResult == true)
                {
                    Calculator3D.AimTriangleAtCoordinate(triangles[0], c.Coordinate);
                }
            }

        }

        private void ImportOBJ(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0)
            {
                MessageBox.Show("There are no materials", "Precaution"); return;
            }
            else
            {
                string file = GetOBJ();
                if (file != "")
                {

                    w3Geoset geo = Parser_OBJ.Parse(file);
                    import_geoset ig = new import_geoset(file, CurrentModel, new List<w3Geoset>() { geo });
                    ig.ShowDialog();
                    if (ig.DialogResult == true)
                    {
                        CurrentModel.RefreshSequenceExtents();
                        CurrentModel.CalculateGeosetBoundingBoxes();
                        CurrentModel.RefreshEdges();
                        RefreshGeosetList();
                        RefreshNodesList();
                    }
                }
            }
        }

        private void Explain(object sender, RoutedEventArgs e)
        {
            Explanation explanation = new Explanation();
            explanation.ShowDialog();
        }

        private void CopyFrame(object sender, RoutedEventArgs e)
        {
            bool parsed = int.TryParse(InputCurrentTrack.Text, out int track);
            if (!parsed) { MessageBox.Show("input track not an integer", "Invalid request"); return; }
            if (track == -1) { MessageBox.Show("No track selected", "Invalid request"); return; }
            CopiedFrame = track;
            CutFrame_ = false;

        }
        private void PasteFrame(bool transaltion, bool rotation, bool scaling)
        {

            if (CopiedFrame <0) { MessageBox.Show("Nothing was copied", "Invalid request"); return; }
            bool parsed = int.TryParse(InputCurrentTrack.Text, out int targetTrack);
            if (!parsed) { MessageBox.Show("input track not an integer", "Invalid request"); return; }
            if (CopiedFrame == targetTrack) { MessageBox.Show("Copeid frame and current frame are the same", "Invalid request"); return; }
            if (Check_OverwritePastedFrame.IsChecked == true)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (node.Data is Bone)
                    {
                        node.Translation.Keyframes.RemoveAll(x => x.Track == CopiedFrame);
                        node.Rotation.Keyframes.RemoveAll(x => x.Track == CopiedFrame);
                        node.Scaling.Keyframes.RemoveAll(x => x.Track == CopiedFrame);
                    }
                }
            }
            foreach (w3Node node in CurrentModel.Nodes)
            {
                if (node.Data is Bone)
                {
                    if (transaltion)
                    {
                        if (node.Translation.Keyframes.Any(x => x.Track == CopiedFrame))
                        {
                            // node has copied frame
                            w3Keyframe copied = node.Translation.Keyframes.First(x => x.Track == CopiedFrame);

                            if (node.Translation.Keyframes.Any(x => x.Track == targetTrack))
                            {
                                // if we have an existing keyframe with the track, just change the data
                                w3Keyframe target = node.Translation.Keyframes.First(x => x.Track == targetTrack);
                                target.Data = copied.Data.ToArray();
                                if (CutFrame_)
                                {
                                    node.Translation.Keyframes.Remove(copied);
                                }
                            }
                            else
                            {
                                // we have to create new keyframe
                                w3Keyframe newKeyfreame = new w3Keyframe();
                                newKeyfreame.Track = targetTrack;
                                newKeyfreame.Data = copied.Data.ToArray();
                                node.Translation.Keyframes.Add(newKeyfreame);
                                node.Translation.Keyframes = node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
                            }
                        }
                    }
                    if (rotation)
                    {
                        if (node.Rotation.Keyframes.Any(x => x.Track == CopiedFrame))
                        {
                            // node has copied frame
                            w3Keyframe copied = node.Rotation.Keyframes.First(x => x.Track == CopiedFrame);

                            if (node.Rotation.Keyframes.Any(x => x.Track == targetTrack))
                            {
                                // if we have an existing keyframe with the track, just change the data
                                w3Keyframe target = node.Rotation.Keyframes.First(x => x.Track == targetTrack);
                                target.Data = copied.Data.ToArray();
                                if (CutFrame_)
                                {
                                    node.Translation.Keyframes.Remove(copied);
                                }
                            }
                            else
                            {
                                // we have to create new keyframe
                                w3Keyframe newKeyfreame = new w3Keyframe();
                                newKeyfreame.Track = targetTrack;
                                newKeyfreame.Data = copied.Data.ToArray();
                                node.Rotation.Keyframes.Add(newKeyfreame);
                                node.Rotation.Keyframes = node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
                            }
                        }
                    }
                    if (scaling)
                    {
                        if (node.Scaling.Keyframes.Any(x => x.Track == CopiedFrame))
                        {
                            // node has copied frame
                            w3Keyframe copied = node.Scaling.Keyframes.First(x => x.Track == CopiedFrame);

                            if (node.Scaling.Keyframes.Any(x => x.Track == targetTrack))
                            {
                                // if we have an existing keyframe with the track, just change the data
                                w3Keyframe target = node.Scaling.Keyframes.First(x => x.Track == targetTrack);
                                target.Data = copied.Data.ToArray();
                                if (CutFrame_)
                                {
                                    node.Translation.Keyframes.Remove(copied);
                                }
                            }
                            else
                            {
                                // we have to create new keyframe
                                w3Keyframe newKeyfreame = new w3Keyframe();
                                newKeyfreame.Track = targetTrack;
                                newKeyfreame.Data = copied.Data.ToArray();
                                node.Scaling.Keyframes.Add(newKeyfreame);
                                node.Scaling.Keyframes = node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();
                            }
                        }
                    }


                    EnteredTrackInAnimator(null, null);
                }
            }
            CopiedFrame = -1; // after we completed a copy, prevent multiple pasting
            CutFrame_ = false;
        }
        private void PasteFrameAll(object sender, RoutedEventArgs e)
        {
            PasteFrame(true, true, true);
        }
        private w3Sequence GetSelectedSequence()
        {
            if (ListSequences.SelectedItem == null) { return null; }
            string name = (ListSequences.SelectedItem as ListBoxItem).Content.ToString();
            name = name.Split(" [")[0];
            return CurrentModel.Sequences.First(x => x.Name == name);
        }
        private void ResizeSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
            w3Sequence sequence = GetSelectedSequence();
            ResizeSequence resizeSequence = new ResizeSequence(sequence, CurrentModel);
            resizeSequence.ShowDialog();
            RefreshSequencesList();

        }

        private void ExplainApp(object sender, RoutedEventArgs e)
        {
            gettingstarted g = new gettingstarted();
            g.ShowDialog();
        }

        private void HotkeysInfo(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(AppHelper.DataPath, "Hotkeys.txt");
            if (File.Exists(path))
            {
                TextReaderWindow tx = new TextReaderWindow(path, "Hotkeys");
                tx.ShowDialog();
            }
        }

        private void callSettings(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();


        }

        private void TakeScreenshot(object sender, RoutedEventArgs e)
        {
            // Create a RenderTargetBitmap for the control
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                (int)Scene_Frame.ActualWidth,
                (int)Scene_Frame.ActualHeight,
                96d,  // DPI X
                96d,  // DPI Y
                PixelFormats.Pbgra32);

            // Render the control to the bitmap
            renderTargetBitmap.Render(Scene_Frame);

            // Define the file path with a timestamp
            string folderPath = System.IO.Path.Combine(AppHelper.appPath, $"Screenshots");
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }
            string path = System.IO.Path.Combine(folderPath, $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            // Save the bitmap to a file
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(fileStream);
            }
            //open folder
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = folderPath,
                UseShellExecute = true
            });
        }


        private void PutOnGround(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { return; }
            if (vertices.Count == 1) { vertices[0].Position.Z = 0; return; }
            float lowestZ = 0;
            foreach (w3Vertex vertex in vertices)
            {
                lowestZ = Math.Min(lowestZ, vertex.Position.Z);
            }
            if (lowestZ == 0) { return; } //is on ground
            foreach (w3Vertex vertex in vertices)
            {
                if (lowestZ != 0)
                {
                    vertex.Position.Z -= lowestZ;
                    vertex.Normal.Z -= lowestZ;
                }
                else
                {
                    vertex.Position.Z += lowestZ;
                    vertex.Normal.Z += lowestZ;
                }
            }
        }

        private void OpenInFolder(object sender, RoutedEventArgs e)
        {
            if (SaveLocation != "" && File.Exists(SaveLocation))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = System.IO.Path.GetDirectoryName(SaveLocation),
                    UseShellExecute = true
                });
            }
        }

        private void Draw3DShape(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("There are no materials", "Precaution"); return; }
            DrawWindow dr = new DrawWindow(CurrentModel);
            dr.ShowDialog();
            if (dr.DialogResult == true)
            {
                RefreshGeosetList();
            }
        }

        private void EditGeosetProperties(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count != 1) { return; }
            w3Geoset geo = GetSelectedGeoset();
            EditGeoset_Propertoes ed = new EditGeoset_Propertoes(geo);
            ed.ShowDialog();
        }

        private void GetChangeLog(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Join(AppHelper.DataPath, "Changelog.txt");
            if (File.Exists(path))
            {
                TextReaderWindow tx = new TextReaderWindow(path, "Changelog");
                tx.ShowDialog();
            }
            else
            {
                MessageBox.Show("Changelog.txt is missing");
            }
        }

        private void ClickedPasteFrame(object sender, RoutedEventArgs e)
        {
            ButtonPasteFrame.ContextMenu.IsOpen = true;
        }

        private void PasteTranslation(object sender, RoutedEventArgs e)
        {
            PasteFrame(true, false, false);
        }

        private void PasteScaling(object sender, RoutedEventArgs e)
        {
            PasteFrame(false, true, false);
        }

        private void PAsteRotation(object sender, RoutedEventArgs e)
        {
            PasteFrame(false, false, true);
        }

        private void ReattachVertices(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            w3Node node = GetSelectedNode();
            if (node.Data is Bone == false)
            {
                MessageBox.Show("Only bones have attached vertices", "Invalid request"); return;
            }
            if (CurrentModel.Nodes.Count(x => x.Data is Bone) < 2)
            {
                MessageBox.Show("There aren't other bones available", "Invalid request"); return;
            }
            Selector s = new Selector(CurrentModel.Nodes.Where(n => n.Data is Bone && n.Name != node.Name).Select(x => x.Name).ToList());
            s.ShowDialog();
            if (s.DialogResult == true)
            {
                string selected = (s.box.SelectedItem as ListBoxItem).Content.ToString();
                w3Node selectedNode = CurrentModel.Nodes.First(x => x.Name == selected);
                int id = selectedNode.objectId;

                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        if (v.AttachedTo.Contains(node.objectId))
                        {
                            v.AttachedTo.Remove(node.objectId); v.AttachedTo.Add(id);
                        }

                    }
                }
            }
        }

        private async void GetaTtachInfo(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                w3Node node = GetSelectedNode();
                if (node.Data is Bone == false)
                {
                    MessageBox.Show("This node is not a bone", "Invalid request"); return;
                }

                Dictionary<int, int> map = new();
                Dictionary<int, int> geoCount = new();
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    int vertices = 0;
                    int max = geo.Vertices.Count;
                    foreach (w3Vertex v in geo.Vertices)
                    {

                        if (v.AttachedTo.Contains(node.objectId))
                        {
                            vertices++;
                        }

                    }
                    if (vertices > 0)
                    {
                        map.Add(geo.ID, vertices);
                        geoCount.Add(geo.ID, max);
                    }
                }
                StringBuilder sb = new StringBuilder();
                foreach (var item in map)
                {
                    sb.AppendLine($"Geoset {item.Key}: {item.Value}/{geoCount[item.Key]} vertices");
                }
                if (map.Count == 0)
                {
                    MessageBox.Show("Nothing is attached to this bone", "Report"); return;

                }
                else
                {
                    MessageBox.Show(sb.ToString(), "What is attached to this bone");
                }

            }
        }
        public static class MathHelper
        {
            public const float Pi = (float)Math.PI;
            public const float TwoPi = Pi * 2;
            public const float HalfPi = Pi / 2;

            // Convert degrees to radians
            public static float ToRadians(float degrees)
            {
                return degrees * (Pi / 180.0f);
            }

            // Convert radians to degrees
            public static float ToDegrees(float radians)
            {
                return radians * (180.0f / Pi);
            }

            // Clamp a value between a minimum and maximum
            public static float Clamp(float value, float min, float max)
            {
                return Math.Max(min, Math.Min(max, value));
            }
        }

        private void LightingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AppSettings.EnableLighting = LightingCheckBox.IsChecked == true;



        }
        private bool CoordinateInExtent(Coordinate c, Extent ex)
        {
            return c.X >= ex.Minimum_X && c.X <= ex.Maximum_X &&
                   c.Y >= ex.Minimum_Y && c.Y <= ex.Maximum_Y &&
                   c.Z >= ex.Minimum_Z && c.Z <= ex.Maximum_Z;
        }

        private void SetSelectionBasedOnExtent(Extent ex)
        {
            if (selectionMode == SelectionMode.ClearAndSelect)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    if (geo.isVisible == false) { continue; }
                    foreach (w3Vertex V in geo.Vertices)
                    {
                        V.isSelected = false;
                    }
                    foreach (w3Edge V in geo.Edges)
                    {
                        V.isSelected = false;
                    }
                    foreach (w3Triangle t in geo.Triangles)
                    {
                        t.isSelected = false;
                    }
                }
                SelectedVertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(SelectedVertices);
                FillCentroidInRaw(centroid);
                return;
            }


            if (selectionMode == SelectionMode.RemoveSelect)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    if (geo.isVisible == false) { continue; }
                    foreach (w3Vertex V in geo.Vertices)
                    {
                        if (V.isSelected) V.isSelected = false;
                    }

                }
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {

                    foreach (w3Edge V in geo.Edges)
                    {
                        if (!V.Vertex1.isSelected && !V.Vertex2.isSelected) V.isSelected = false;

                    }
                    foreach (w3Triangle t in geo.Triangles)
                    {
                        if (!t.Vertex2.isSelected && !t.Vertex2.isSelected && !t.Vertex3.isSelected) t.isSelected = false;
                    }
                }
                SelectedVertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(SelectedVertices);
                FillCentroidInRaw(centroid);
            }
            else
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    if (geo.isVisible == false) { continue; }
                    foreach (w3Vertex V in geo.Vertices)
                    {
                        V.isSelected = false;
                    }

                }
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {

                    foreach (w3Edge V in geo.Edges)
                    {
                        if (V.Vertex1.isSelected) V.isSelected = true;
                        if (V.Vertex2.isSelected) V.isSelected = true;
                    }
                    foreach (w3Triangle t in geo.Triangles)
                    {
                        if (t.Vertex1.isSelected) t.isSelected = true;
                        if (t.Vertex2.isSelected) t.isSelected = true;
                        if (t.Vertex3.isSelected) t.isSelected = true;
                    }
                }
                SelectedVertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(SelectedVertices);
                FillCentroidInRaw(centroid);
            }
            if (editMode_current == EditMode.UV)
            {
                if (SelectedVertices.Count > 0)
                {
                    w3Geoset geoset = VerticesBelongToSameGeoset(SelectedVertices);
                    if (geoset != null)
                    {
                        UV_CurrentImage = GetTextureFromMateialID(geoset.ID);
                        RefreshUvMapping();
                    }
                    else
                    {
                        MessageBox.Show("Not all selected vertices belong to the same geoset.", "Invalid request"); return;
                    }
                }
            }
        }
        private w3Geoset VerticesBelongToSameGeoset(List<w3Vertex> vertices)
        {
            w3Geoset firstMet = null;

            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (firstMet == null)
                    {
                        if (vertices.Contains(v))
                        {
                            firstMet = geo; continue;
                        }
                    }
                    else
                    {
                        if (vertices.Contains(v))
                        {
                            if (firstMet == geo) { continue; }
                            else { return null; }
                        }
                    }
                }

            }

            return firstMet;
        }



        private BitmapSource GetTextureFromMateialID(int id)
        {
            throw new NotImplementedException();
        }

        private Coordinate ConvertToWorldCoordinates(float screenX, float screenY, float depth)
        {
            // Get the camera's position and direction
            float[] cameraPos = { CameraControl.eyeX, CameraControl.eyeY, CameraControl.eyeZ };
            float[] cameraCenter = { CameraControl.CenterX, CameraControl.CenterY, CameraControl.CenterZ };
            float[] cameraUp = { CameraControl.UpX, CameraControl.UpY, CameraControl.UpZ };

            // Transform the screen coordinates into normalized device coordinates
            float normX = (2.0f * screenX) / (float)Scene_Viewport.ActualWidth - 1.0f;
            float normY = 1.0f - (2.0f * screenY) / (float)Scene_Viewport.ActualHeight; // Invert Y axis

            // You will need to project this into your 3D space
            // Here you would apply the inverse projection and view matrices to convert these coords into world space

            // Placeholder logic to generate 3D coordinates based on the normalized device coordinates
            // (This needs to be replaced with actual math based on your projection matrix and camera)
            float worldX = normX * depth; // Example transformation
            float worldY = normY * depth; // Example transformation
            float worldZ = depth; // Example depth

            return new Coordinate(worldX, worldY, worldZ);
        }

        private void FPS_Checked(object sender, RoutedEventArgs e)
        {
            //  Scene_Viewport.DrawFPS = FPS.IsChecked == true; ;
        }

        private void SelectedSequence(object sender, SelectionChangedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            w3Sequence seq = GetSelectedSequence();
            // Slider_Sequence.Minimum = seq.From;
            // Slider_Sequence.Maximum = seq.To;
            // Slider_Sequence.Value = seq.From;
            InputCurrentTrack.Text = seq.From.ToString();
            CurrentSceneFrame = seq.From;
            FillTimeline(seq);
            if (editMode_current == EditMode.Animator) { RefreshFrame(); }
        }



        private void ChangedTrackInAnimator(object sender, TextChangedEventArgs e)
        {

        }
        private void SelectSEquenceInListFromName(string name)
        {

            for (int i = 0; i < ListSequences.Items.Count; i++)
            {
                ListBoxItem item = ListSequences.Items[i] as ListBoxItem;
                if (item.Content.ToString().Split(" [")[0] == name) { ListSequences.SelectedIndex = i; break; }

            }
            // MessageBox.Show(found.ToString());
        }

        private void EnteredTrackInAnimator(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool parsed = int.TryParse(InputCurrentTrack.Text, out int track);
                if (parsed)
                {
                    if (track < 0)
                    {
                        MessageBox.Show("Input integer cannot be negative", "Precaution"); return;
                    }
                    if (CurrentModel.Sequences.Any(sequence => track >= sequence.From && track <= sequence.To))
                    { // track exists

                        CurrentSceneFrame = track;

                        w3Sequence s = CurrentModel.Sequences.First(sequence => track >= sequence.From && track <= sequence.To);
                        SelectSEquenceInListFromName(s.Name);

                        GenerateModifiedGeometryForTrack(track);
                    }
                    else
                    {

                        MessageBox.Show("This track doesn't exist in any sequence", "Precaution");
                        InputCurrentTrack.Text = CurrentSceneFrame.ToString(); return;
                    }
                }
                else
                {
                    MessageBox.Show("Input not an integer", "Precaution");
                    InputCurrentTrack.Text = CurrentSceneFrame.ToString(); return;
                }

            }
        }
        private void RefreshFrame()
        {
            if (CurrentSceneFrame == -1) { Timeline.Children.Clear(); return; }
            bool parsed = int.TryParse(InputCurrentTrack.Text, out int track);
            if (parsed)
            {
                if (CurrentModel.Sequences.Any(sequence => track >= sequence.From && track <= sequence.To))
                {

                    CurrentSceneFrame = track;

                    w3Sequence s = CurrentModel.Sequences.First(sequence => track >= sequence.From && track <= sequence.To);
                    SelectSEquenceInListFromName(s.Name);
                    GenerateModifiedGeometryForTrack(track);
                }
                else
                {
                    CurrentSceneFrame = -1;
                    InputCurrentTrack.Text = "-1";
                }
            }
            else
            {
                CurrentSceneFrame = -1;
                InputCurrentTrack.Text = "-1";
            }

        }
        private void GenerateModifiedGeometryForTrack(int track)
        {
            ModifiedGeometryForTrack = new List<w3Geoset>();
            List<w3Geoset> geosets = new List<w3Geoset>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                ModifiedGeometryForTrack.Add(geo.Clone());

            }
            foreach (w3Geoset geo in ModifiedGeometryForTrack)
            {
                foreach (w3Vertex vertex in geo.Vertices)
                {


                    int firstBone = vertex.AttachedTo[0];
                    w3Node node = CurrentModel.Nodes.First(x => x.objectId == firstBone);
                    if (node.Scaling.Keyframes.Any(x => x.Track == CurrentSceneFrame))
                    {
                        float[] instruction = node.Scaling.Keyframes.First(x => x.Track == CurrentSceneFrame).Data;
                        vertex.Position.X *= instruction[0];
                        vertex.Position.Y *= instruction[1];
                        vertex.Position.Z *= instruction[2];
                    }
                    if (node.Rotation.Keyframes.Any(x => x.Track == CurrentSceneFrame))
                    {
                        float[] instruction = node.Rotation.Keyframes.First(x => x.Track == CurrentSceneFrame).Data;
                        vertex.Position = Calculator3D.RotateVertexAroundBone(vertex.Position, instruction, node.PivotPoint);
                    }

                    if (node.Translation.Keyframes.Any(x => x.Track == CurrentSceneFrame))
                    {
                        float[] instruction = node.Translation.Keyframes.First(x => x.Track == CurrentSceneFrame).Data;
                        vertex.Position.X += instruction[0];
                        vertex.Position.Y += instruction[1];
                        vertex.Position.Z += instruction[2];

                    }


                }
            }

            // ModifiedGeometryForTrack = 

        }

        private void PlayAnimation(object sender, RoutedEventArgs e)
        {
            playingAnimation = !playingAnimation;

            TopMenu.IsEnabled = playingAnimation == false;
            SidePanel.IsEnabled = playingAnimation == false;
            InputCurrentTrack.IsEnabled = playingAnimation == false;
            ListSequences.IsEnabled = playingAnimation == false;
            ButtonSequenceOptions.IsEnabled = playingAnimation == false;
            ButtonCopyFrame.IsEnabled = playingAnimation == false;
            ButtonCutFrame.IsEnabled = playingAnimation == false;
            ButtonPasteFrame.IsEnabled = playingAnimation == false;
            ButtonEditGS.IsEnabled = playingAnimation == false;
            ButtonDelSequenc.IsEnabled = playingAnimation == false;
            ButtonEditSequence.IsEnabled = playingAnimation == false;
            ButtonNewSeq.IsEnabled = playingAnimation == false;
            CurrentModelAnimated = CurrentModel.CloneAnimated();
        }

        private void EditGeosetVisibilities(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count == 0) { return; }
            if (List_Geosets.SelectedItems.Count > 1) { MessageBox.Show("Can edit one geoset's visibility at a time", "Invalid request"); return; }
            w3Geoset current = GetSelectedGeoset();

            edit_geo_vis eg = new edit_geo_vis(CurrentModel, current);
            eg.ShowDialog();
        }

        private void ClearRecents(object sender, RoutedEventArgs e)
        {
            if (ButtonOpenRecent.Items.Count > 1)
            {
                while (ButtonOpenRecent.Items.Count > 1)
                {
                    ButtonOpenRecent.Items.RemoveAt(0);
                }
                string path = System.IO.Path.Combine(AppHelper.DataPath, "Recents.txt");
                File.Delete(path);
            }
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {

            SelectedVertices.Clear();
            SelectedVertices = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.isVisible == false) { continue; }
                foreach (w3Vertex v in geo.Vertices)
                {
                    v.isSelected = true;
                    SelectedVertices.Add(v);
                }

                if (editMode_current == EditMode.Triangles) foreach (w3Triangle v in geo.Triangles) v.isSelected = true;

                else if (editMode_current == EditMode.Edges) foreach (w3Edge v in geo.Edges) v.isSelected = true;


            }




            Coordinate centroiud = Calculator3D.GetCentroid(SelectedVertices);
            FillCentroidInRaw(centroiud);
            UpdateSelectionData();
            UpdateSelectionRawData();
            UpdateRiggingData();
        }

        private void SelectNone(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices) v.isSelected = false;
                foreach (w3Triangle v in geo.Triangles) v.isSelected = false;
                foreach (w3Edge v in geo.Edges) v.isSelected = false;
            }
            UpdateRiggingData(false);
        }

        private void SelectInvert(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (editMode_current == EditMode.Triangles) foreach (w3Triangle v in geo.Triangles) v.isSelected = !v.isSelected;
                if (editMode_current == EditMode.Vertices) foreach (w3Vertex v in geo.Vertices) v.isSelected = !v.isSelected;
                if (editMode_current == EditMode.Edges) foreach (w3Edge v in geo.Edges) v.isSelected = !v.isSelected;

            }
        }

        private void SelectSameMat(object sender, RoutedEventArgs e)
        {

        }

        private void SelectDifMAt(object sender, RoutedEventArgs e)
        {

        }

        private void Selectadj(object sender, RoutedEventArgs e)
        {
            switch (editMode_current)
            {
                case EditMode.Vertices:
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Triangle t in geo.Triangles)
                        {
                            if (t.Vertex1.isSelected) { t.Vertex2.isSelected = true; t.Vertex3.isSelected = true; continue; }
                            if (t.Vertex2.isSelected) { t.Vertex1.isSelected = true; t.Vertex3.isSelected = true; continue; }
                            if (t.Vertex3.isSelected) { t.Vertex2.isSelected = true; t.Vertex1.isSelected = true; continue; }
                        }
                    }
                    break;
                case EditMode.Triangles:
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Triangle t in geo.Triangles)
                        {
                            SelectAdjascentTrianglesBasedOnVertex(geo, t.Vertex1);
                            SelectAdjascentTrianglesBasedOnVertex(geo, t.Vertex2);
                            SelectAdjascentTrianglesBasedOnVertex(geo, t.Vertex3);
                        }
                    }
                    break;
                case EditMode.Edges:

                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Edge t in geo.Edges)
                        {
                            SelectAdjascentEdgesBasedOnVertex(geo, t.Vertex1);
                            SelectAdjascentEdgesBasedOnVertex(geo, t.Vertex2);

                        }
                    }
                    break;
            }

        }
        private void SelectAdjascentEdgesBasedOnVertex(w3Geoset geo, w3Vertex which)
        {
            foreach (w3Edge e in geo.Edges)
            {
                if (e.Vertex1 == which) e.isSelected = true;
                if (e.Vertex2 == which) e.isSelected = true;
            }
        }
        private void SelectAdjascentTrianglesBasedOnVertex(w3Geoset geo, w3Vertex which)
        {
            foreach (w3Triangle t in geo.Triangles)
            {
                if (t.Vertex1 == which) t.isSelected = true;
                if (t.Vertex2 == which) t.isSelected = true;
                if (t.Vertex3 == which) t.isSelected = true;
            }
        }
        private string GetFilePathWithTime(string filename)
        {
            // Get the current date and time
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Get the file extension
            string extension = System.IO.Path.GetExtension(filename);

            // Get the filename without extension
            string fileWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);

            // Get the directory path of the original file (if provided with path)
            string directory = System.IO.Path.GetDirectoryName(filename);

            // Form the new filename with the timestamp appended
            string newFilename = $"{fileWithoutExtension}_{timestamp}{extension}";

            // Combine directory (if any) and new filename
            if (!string.IsNullOrEmpty(directory))
            {
                return System.IO.Path.Combine(directory, newFilename);
            }
            else
            {
                return newFilename;
            }
        }

        private void SaveBackup(object sender, RoutedEventArgs e)
        {
            if (SaveLocation != "" && File.Exists(SaveLocation))
            {
                CurrentModel.Name = InputModelName.Text.Trim();
                string backupLocation = GetFilePathWithTime(SaveLocation);
                CurrentModel.CalculateExtents();
                File.WriteAllText(backupLocation, CurrentModel.ToMDL());
            }
        }


        private void ClearHistory(object sender, RoutedEventArgs e)
        {

            UndoRedo.ClearAll();
        }

        private void CopyCentroid(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (List_Geosets.SelectedItems.Count > 0)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();
                foreach (w3Geoset g in geosets)
                {

                    sb.AppendLine($"Geoset {g.ID}: {Calculator3D.CalculateCentroidFromVertices(g.Vertices).ToString()}");
                }

            }
            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void NegateX(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { MessageBox.Show("Select vertices", "Invalid request"); return; }
            foreach (w3Vertex v in vertices) { v.Position.X = -v.Position.X; v.Normal.X = -v.Normal.X; }
            SetSaved(false);
        }

        private void NegateY(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { MessageBox.Show("Select vertices", "Invalid request"); return; }
            foreach (w3Vertex v in vertices) { v.Position.Y = -v.Position.Y; v.Normal.Y = -v.Normal.Y; }
            SetSaved(false);
        }

        private void NegateZ(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { MessageBox.Show("Select vertices", "Invalid request"); return; }
            foreach (w3Vertex v in vertices) { v.Position.Z = -v.Position.Z; v.Normal.Z = -v.Normal.Z; }
            SetSaved(false);
        }

        private void ModifiedModifyAmount(object sender, TextChangedEventArgs e)
        {
            string txt = InputModifyAmount.Text;
            bool parsed = float.TryParse(txt, out float amount);
            if (parsed)
            {
                if (amount < 0.000005)
                {
                    ModifyAmount = 1; return;
                }
                ModifyAmount = amount;
            }
            else
            {
                ModifyAmount = 1;


            }
        }
        private List<w3Vertex> GetSelectedVerticesOf(List<w3Edge>edges)
        {
            List<w3Vertex> list = new();
            foreach (w3Edge edge in edges)
            {
                if (list.Contains(edge.Vertex1) == false) list.Add(edge.Vertex1);
                if (list.Contains(edge.Vertex2) == false) list.Add(edge.Vertex2);
            }
            return list;
        }
        private List<w3Vertex> GetVerticesOf(List<w3Triangle> triangles)
        {
            List<w3Vertex> list = new();
            foreach (w3Triangle triangle in triangles)
            {
                if (list.Contains(triangle.Vertex1) == false) list.Add(triangle.Vertex1);
                if (list.Contains(triangle.Vertex2) == false) list.Add(triangle.Vertex2);
                if (list.Contains(triangle.Vertex3) == false) list.Add(triangle.Vertex3);
            }
            return list;
        }
        //-----------------------------------------------------------------
        //----- set manual position
        //-----------------------------------------------------------------
        private void SetX(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool ParsedValue = float.TryParse(inputX.Text, out float Value);


                if (editMode_current == EditMode.Animator)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                        if (parsedTrack && ParsedValue)
                        {
                            if (node.Translation.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = node.Translation.Keyframes.First(x => x.Track == track);
                                k.Data[0] = Value;
                                RefreshFrame();
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [Value, 0, 0];
                                node.Translation.Keyframes.Add(k);
                                node.Translation.Keyframes = node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
                                RefreshFrame();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }
                }
                else if (editMode_current == EditMode.Vertices  )
                {
                    List<w3Vertex> vertices = GetSelectedVertices();
                    if (vertices.Count == 1) vertices[0].Position.X = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.X = Value;
                        Calculator3D.CenterVertices(vertices, Value, 0, 0);

                    }
                }
                else if ( editMode_current == EditMode.Triangles  )
                {
                    List<w3Triangle> trianges = GetSelectedTriangles();
                    List<w3Vertex> vertices = GetVerticesOf(trianges);
                    if (vertices.Count == 1) vertices[0].Position.X = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.X = Value;
                        Calculator3D.CenterVertices(vertices, Value, 0, 0);

                    }
                }
                else if ( editMode_current == EditMode.Edges )
                {
                    List<w3Edge> edges = GetSelectedEdges();
                    List<w3Vertex> vertices = GetSelectedVerticesOf(edges);
                    if (vertices.Count == 1) vertices[0].Position.X = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.X = Value;
                        Calculator3D.CenterVertices(vertices, Value, 0, 0);

                    }
                }
                else if ( editMode_current == EditMode.Geosets)
                {
                    List<w3Geoset> geosets = GetSelectedGeosets();
                    if (geosets.Count > 0)
                    {
                        foreach (w3Geoset geo in geosets)
                        {
                            Coordinate centroid = Calculator3D.GetCentroid(geo.Vertices);
                            centroid.X = Value;
                            Calculator3D.CenterVertices(geo.Vertices, Value, 0, 0);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select at least one geoset", "Invalid request"); return;
                    }
                     
                }
                else if (editMode_current == EditMode.Nodes)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        if (ParsedValue) { node.PivotPoint.X = Value; }
                        else { MessageBox.Show("Invalid input", "Invalid request"); return; }

                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }


                }

            }
        }





        private void SetY(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool ParsedValue = float.TryParse(inputY.Text, out float Value);


                if (editMode_current == EditMode.Animator)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                        if (parsedTrack && ParsedValue)
                        {
                            if (node.Translation.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = node.Translation.Keyframes.First(x => x.Track == track);
                                k.Data[1] = Value;
                                RefreshFrame();
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [0, Value, 0];
                                node.Translation.Keyframes.Add(k);
                                node.Translation.Keyframes = node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
                                RefreshFrame();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }
                }
                else if (editMode_current == EditMode.Vertices)
                {
                    List<w3Vertex> vertices = GetSelectedVertices();
                    if (vertices.Count == 1) vertices[0].Position.Y = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.X = Value;
                        Calculator3D.CenterVertices(vertices, 0, Value, 0);

                    }
                }
                else if (editMode_current == EditMode.Triangles)
                {
                    List<w3Triangle> trianges = GetSelectedTriangles();
                    List<w3Vertex> vertices = GetVerticesOf(trianges);
                    if (vertices.Count == 1) vertices[0].Position.Y = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.Y = Value;
                        Calculator3D.CenterVertices(vertices, 0, Value, 0);

                    }
                }
                else if (editMode_current == EditMode.Edges)
                {
                    List<w3Edge> edges = GetSelectedEdges();
                    List<w3Vertex> vertices = GetSelectedVerticesOf(edges);
                    if (vertices.Count == 1) vertices[0].Position.X = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.Y = Value;
                        Calculator3D.CenterVertices(vertices, 0, Value, 0);

                    }
                }
                else if (editMode_current == EditMode.Geosets)
                {
                    List<w3Geoset> geosets = GetSelectedGeosets();
                    if (geosets.Count > 0)
                    {
                        foreach (w3Geoset geo in geosets)
                        {
                            Coordinate centroid = Calculator3D.GetCentroid(geo.Vertices);
                            centroid.Y = Value;
                            Calculator3D.CenterVertices(geo.Vertices, 0, Value, 0);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select at least one geoset", "Invalid request"); return;
                    }

                }
                else if (editMode_current == EditMode.Nodes)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        if (ParsedValue) { node.PivotPoint.Y = Value; }
                        else { MessageBox.Show("Invalid input", "Invalid request"); return; }

                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }


                }

            }
        }


     
        private void SetZ(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                bool ParsedValue = float.TryParse(inputZ.Text, out float Value);


                if (editMode_current == EditMode.Animator)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                        if (parsedTrack && ParsedValue)
                        {
                            if (node.Translation.Keyframes.Any(x => x.Track == track))
                            {
                                w3Keyframe k = node.Translation.Keyframes.First(x => x.Track == track);
                                k.Data[2] = Value;
                            }
                            else
                            {
                                w3Keyframe k = new w3Keyframe();
                                k.Track = track;
                                k.Data = [0, 0, Value];
                                node.Translation.Keyframes.Add(k);
                                node.Translation.Keyframes = node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
                                RefreshFrame();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }
                }
                else if (editMode_current == EditMode.Vertices)
                {
                    List<w3Vertex> vertices = GetSelectedVertices();
                    if (vertices.Count == 1) vertices[0].Position.Z = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.Z = Value;
                        Calculator3D.CenterVertices(vertices, 0, 0, Value);

                    }
                }
                else if (editMode_current == EditMode.Triangles)
                {
                    List<w3Triangle> trianges = GetSelectedTriangles();
                    List<w3Vertex> vertices = GetVerticesOf(trianges);
                    if (vertices.Count == 1) vertices[0].Position.Y = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.Z = Value;
                        Calculator3D.CenterVertices(vertices, 0, 0, Value);

                    }
                }
                else if (editMode_current == EditMode.Edges)
                {
                    List<w3Edge> edges = GetSelectedEdges();
                    List<w3Vertex> vertices = GetSelectedVerticesOf(edges);
                    if (vertices.Count == 1) vertices[0].Position.X = Value;
                    else if (vertices.Count > 1)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(vertices);
                        centroid.Y = Value;
                        Calculator3D.CenterVertices(vertices, 0,0 , Value);

                    }
                }
                else if (editMode_current == EditMode.Geosets)
                {
                    List<w3Geoset> geosets = GetSelectedGeosets();
                    if (geosets.Count > 0)
                    {
                        foreach (w3Geoset geo in geosets)
                        {
                            Coordinate centroid = Calculator3D.GetCentroid(geo.Vertices);
                            centroid.Y = Value;
                            Calculator3D.CenterVertices(geo.Vertices, 0,0 , Value);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Select at least one geoset", "Invalid request"); return;
                    }

                }
                else if (editMode_current == EditMode.Nodes)
                {

                    if (List_Nodes.SelectedItem != null)
                    {
                        w3Node node = GetSelectedNode();
                        if (ParsedValue) { node.PivotPoint.Z = Value; }
                        else { MessageBox.Show("Invalid input", "Invalid request"); return; }

                    }
                    else
                    {
                        MessageBox.Show("Select a node", "Invalid request"); return;
                    }


                }

            }
        }
        private void SetXYZ(object sender, RoutedEventArgs e)
        {
            bool ParsedValue1 = float.TryParse(inputX.Text, out float Value1);
            bool ParsedValue2 = float.TryParse(inputY.Text, out float Value2);
            bool ParsedValue3 = float.TryParse(inputZ.Text, out float Value3);

            if (!ParsedValue1 || !ParsedValue2 || !ParsedValue3)
            { MessageBox.Show("Invalid input", "Invalid request"); return; }

            if (editMode_current == EditMode.Animator)
            {

                if (List_Nodes.SelectedItem != null)
                {
                    w3Node node = GetSelectedNode();
                    bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                    if (parsedTrack )
                    {
                        if (node.Translation.Keyframes.Any(x => x.Track == track))
                        {
                            w3Keyframe k = node.Translation.Keyframes.First(x => x.Track == track);
                            k.Data  = [Value1, Value2, Value3];
                             
                        }
                        else
                        {
                            w3Keyframe k = new w3Keyframe();
                            k.Track = track;
                            k.Data = [Value1, Value2, Value3];
                            node.Translation.Keyframes.Add(k);
                            node.Translation.Keyframes = node.Translation.Keyframes.OrderBy(x => x.Track).ToList();
                            RefreshFrame();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Select a node", "Invalid request"); return;
                }
            }
            else if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) vertices[0].Position.SetTo(Value1, Value2, Value3);
                else if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    centroid.SetTo(Value1, Value2, Value3);
                    Calculator3D.CenterVertices(vertices, Value1, Value2, Value3);

                }
            }
            else if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> trianges = GetSelectedTriangles();
                List<w3Vertex> vertices = GetVerticesOf(trianges);
                if (vertices.Count == 1) vertices[0].Position.SetTo(Value1, Value2, Value3);
                else if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    centroid.SetTo(Value1, Value2, Value3);
                    Calculator3D.CenterVertices(vertices, Value1, Value2, Value3);

                }
            }
            else if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                List<w3Vertex> vertices = GetSelectedVerticesOf(edges);
                if (vertices.Count == 1) vertices[0].Position.SetTo(Value1, Value2, Value3);
                else if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    centroid.SetTo(Value1, Value2, Value3);
                    Calculator3D.CenterVertices(vertices, Value1, Value2, Value3);

                }
            }
            else if (editMode_current == EditMode.Geosets)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();
                if (geosets.Count > 0)
                {
                    foreach (w3Geoset geo in geosets)
                    {
                        Coordinate centroid = Calculator3D.GetCentroid(geo.Vertices);
                        centroid.SetTo(Value1, Value2, Value3);
                        Calculator3D.CenterVertices(geo.Vertices, Value1, Value2, Value3);
                    }
                }
                else
                {
                    MessageBox.Show("Select at least one geoset", "Invalid request"); return;
                }

            }
            else if (editMode_current == EditMode.Nodes)
            {

                if (List_Nodes.SelectedItem != null)
                {
                    w3Node node = GetSelectedNode();
                     node.PivotPoint.SetTo(Value1, Value2, Value3); 
                     

                }
                else
                {
                    MessageBox.Show("Select a node", "Invalid request"); return;
                }


            }


        }
        private void SetManualAvailability(bool translate, bool rotate, bool scale)
        {
            Expander_Translation.IsEnabled = translate;
            Expander_Rotation.IsEnabled = rotate; 
            Expander_Scaling.IsEnabled = scale;
        }
        private List<w3Vertex> GetVerticesOfSelectedGeoset()
        {
            List<w3Vertex> vertices = new List<w3Vertex>();
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {
                foreach (w3Geoset geo in geosets) { vertices.AddRange(geo.Vertices); }
            }
            return vertices;
        }
        private Vector3 GetRadians()
        {
            float min = -6.2832f;
            float max = 6.2832f;
            Vector3 def = new Vector3(0, 0, 0);
            bool parsed1 = float.TryParse(inputXr.Text, out float x);
            bool parsed2 = float.TryParse(inputXr.Text, out float y);
            bool parsed3 = float.TryParse(inputXr.Text, out float z);
            if (parsed1)
            {
                if (x > min && x < max)
                {
                    def.X = x;
                }
                if (y > min && x < max)
                {
                    def.Y = y;
                }
                if (z > min && z < max)
                {
                    def.Z = z;
                }

            }
            return
                     def;
        }
        private float GetRadianOutput(float number)
        {
            float limit = (float)Math.PI;
            if (number > 2 * limit) { return limit * 2; }
            if (number < -(2 * limit)) { return -(limit * 2); }
            return number;
        }
        private void SetXr(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return; }
            bool parsed = float.TryParse(inputXr.Text, out float value);
            if (!parsed) { MessageBox.Show("not a number", "Invalid request"); return; }
            if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Triangles || editMode_current == EditMode.Triangles || editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = new List<w3Vertex>();
                if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();  
                 
                if (vertices.Count > 1)
                {

                    Coordinate centroid = Radio_Centroid.IsChecked == true ? Calculator3D.GetCentroid(vertices) : GetCustomCoordinate();
                    if (Radio_Degrees.IsChecked == true)
                    {
                        if (value >= -360 && value <= 360)
                        {

                            RotateVerticesFixed(vertices, AxisMode.X, value, centroid);
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return;
                        }
                    }
                    else
                    {
                        value = Calculator.RadiansToDegrees(GetRadianOutput(value));
                        RotateVerticesFixed(vertices, AxisMode.X, value, centroid);
                    }


                }
            }
            if (editMode_current == EditMode.Normals)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (value >= -360 && value <= 360) RotateNormalFixed(vertices, AxisMode.X, value);
                else { MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return; }


            }
            if (editMode_current == EditMode.Animator)
            {
                bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                if (parsedTrack)
                {
                    if (track > 0)
                    {
                        if (value >= -360 && value <= 360)
                        {
                            if (List_Nodes.SelectedItem != null)
                            {
                                w3Node node = GetSelectedNode();
                                if (node.Rotation.Keyframes.Any(x => x.Track == track))
                                {
                                    w3Keyframe k = node.Rotation.Keyframes.First(x => x.Track == track);
                                    k.Data[0] = value;
                                    RefreshFrame();
                                }
                                else
                                {
                                    w3Keyframe k = new w3Keyframe();
                                    k.Track = track;
                                    k.Data = [value, 0, 0];
                                    node.Rotation.Keyframes.Add(k);
                                    node.Rotation.Keyframes = node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
                                    RefreshFrame();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Select a node", "Precaution"); return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Track cannot be negative", "Invalid request"); return;
                    }
                }
                else { MessageBox.Show("Invalid input for track", "Invalid request"); return; }
            }
            inputXr.Text = "0";
        }

        private void RotateNormalFixed(List<w3Vertex> vertices, AxisMode axis, float rotation)
        {
            // Convert rotation to radians
            float radians = rotation * (float)Math.PI / 180.0f;

            foreach (w3Vertex v in vertices)
            {
                // Extract current normal values
                float x = v.Normal.X;
                float y = v.Normal.Y;
                float z = v.Normal.Z;

                // Create a rotation matrix depending on the axis
                if (axis == AxisMode.X)
                {
                    // Rotation around the X-axis (affects Y and Z)
                    float newY = (float)(y * Math.Cos(radians) - z * Math.Sin(radians));
                    float newZ = (float)(y * Math.Sin(radians) + z * Math.Cos(radians));
                    v.Normal = new Coordinate { X = x, Y = newY, Z = newZ };  // X stays the same
                }
                else if (axis == AxisMode.Y)
                {
                    // Rotation around the Y-axis (affects X and Z)
                    float newX = (float)(x * Math.Cos(radians) + z * Math.Sin(radians));
                    float newZ = (float)(-x * Math.Sin(radians) + z * Math.Cos(radians));
                    v.Normal = new Coordinate { X = newX, Y = y, Z = newZ };  // Y stays the same
                }
                else if (axis == AxisMode.Z)
                {
                    // Rotation around the Z-axis (affects X and Y)
                    float newX = (float)(x * Math.Cos(radians) - y * Math.Sin(radians));
                    float newY = (float)(x * Math.Sin(radians) + y * Math.Cos(radians));
                    v.Normal = new Coordinate { X = newX, Y = newY, Z = z };  // Z stays the same
                }
            }
        }


        private void SetYr(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return; }
            bool parsed = float.TryParse(inputYr.Text, out float rotation);
            if (!parsed) { MessageBox.Show("not a number", "Invalid request"); return; }
            if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Triangles || editMode_current == EditMode.Triangles || editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = new List<w3Vertex>();
                if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Radio_Centroid.IsChecked == true ? Calculator3D.GetCentroid(vertices) : GetCustomCoordinate();
                    if (Radio_Degrees.IsChecked == true)
                    {
                        if (rotation >= -360 && rotation <= 360)
                        {

                            RotateVerticesFixed(vertices, AxisMode.Y, rotation, centroid);
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return;
                        }
                    }
                    else
                    {
                        rotation = Calculator.RadiansToDegrees(GetRadianOutput(rotation));
                        RotateVerticesFixed(vertices, AxisMode.Y, rotation, centroid);
                    }



                }
            }
            if (editMode_current == EditMode.Normals)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (rotation >= -360 && rotation <= 360) RotateNormalFixed(vertices, AxisMode.Y, rotation);
                else { MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return; }


            }
            if (editMode_current == EditMode.Animator)
            {
                bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                if (parsedTrack)
                {
                    if (track > 0)
                    {
                        if (rotation >= -360 && rotation <= 360)
                        {
                            if (List_Nodes.SelectedItem != null)
                            {
                                w3Node node = GetSelectedNode();
                                if (node.Rotation.Keyframes.Any(x => x.Track == track))
                                {
                                    w3Keyframe k = node.Rotation.Keyframes.First(x => x.Track == track);
                                    k.Data[1] = rotation;
                                    RefreshFrame();
                                }
                                else
                                {
                                    w3Keyframe k = new w3Keyframe();
                                    k.Track = track;
                                    k.Data = [0, rotation, 0];
                                    node.Rotation.Keyframes.Add(k);
                                    node.Rotation.Keyframes = node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
                                    RefreshFrame();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Select a node", "Precaution"); return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Track cannot be negative", "Invalid request"); return;
                    }
                }
                else { MessageBox.Show("Invalid input for track", "Invalid request"); return; }
            }
            inputYr.Text = "0";
        }

        private void SetZr(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return; }
            bool parsed = float.TryParse(inputZr.Text, out float rotation);
            if (!parsed) { MessageBox.Show("not a number", "Invalid request"); return; }
            if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Triangles || editMode_current == EditMode.Triangles || editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = new List<w3Vertex>();
                if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Radio_Centroid.IsChecked == true ? Calculator3D.GetCentroid(vertices) : GetCustomCoordinate();
                    if (Radio_Degrees.IsChecked == true)
                    {
                        if (rotation >= -360 && rotation <= 360)
                        {

                            RotateVerticesFixed(vertices, AxisMode.Y, rotation, centroid);
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return;
                        }
                    }
                    else
                    {
                        rotation = Calculator.RadiansToDegrees(GetRadianOutput(rotation));
                        RotateVerticesFixed(vertices, AxisMode.Z, rotation, centroid);
                    }


                }
            }
            if (editMode_current == EditMode.Normals)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (rotation >= -360 && rotation <= 360) RotateNormalFixed(vertices, AxisMode.Z, rotation);
                else { MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return; }


            }
            if (editMode_current == EditMode.Animator)
            {
                bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                if (parsedTrack)
                {
                    if (track > 0)
                    {
                        if (rotation >= -360 && rotation <= 360)
                        {
                            if (List_Nodes.SelectedItem != null)
                            {
                                w3Node node = GetSelectedNode();
                                if (node.Rotation.Keyframes.Any(x => x.Track == track))
                                {
                                    w3Keyframe k = node.Rotation.Keyframes.First(x => x.Track == track);
                                    k.Data[2] = rotation;
                                    RefreshFrame();
                                }
                                else
                                {
                                    w3Keyframe k = new w3Keyframe();
                                    k.Track = track;
                                    k.Data = [0, 0, rotation];
                                    node.Rotation.Keyframes.Add(k);
                                    node.Rotation.Keyframes = node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
                                    RefreshFrame();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Select a node", "Precaution"); return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Track cannot be negative", "Invalid request"); return;
                    }
                }
                else { MessageBox.Show("Invalid input for track", "Invalid request"); return; }
            }
            inputZr.Text = "0";
        }

        private void SetXs(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                if (editMode_current == EditMode.Animator)
                {
                    bool parsed = float.TryParse(InputXs.Text, out float result);
                    bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                    w3Node node = GetSelectedNode();
                    if (node != null && parsedTrack && parsed)
                    {
                        if (node.Scaling.Keyframes.Any(x => x.Track == track))
                        {
                            node.Scaling.Keyframes.First(x => x.Track == track).Data[0] = result;
                        }
                        else
                        {
                            w3Keyframe k = new w3Keyframe(); k.Track = track;
                            k.Data = [result, 1, 1];
                            k.OutTan = [1, 1, 1];
                            k.InTan = [1, 1, 1];
                            node.Scaling.Keyframes.Add(k);
                            node.Scaling.Keyframes = node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();

                        }

                    }

                }
                if (editMode_current == EditMode.Rigging || editMode_current == EditMode.Nodes) { }// nodes dont scale 
                if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Geosets || editMode_current == EditMode.Triangles || editMode_current == EditMode.Edges)
                {

                    bool parsed = float.TryParse(InputXs.Text, out float result);
                    if (parsed)
                    {
                        List<w3Vertex> vertices = new List<w3Vertex>();
                        if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                        if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                        if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                        if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                        if (vertices.Count > 1)
                        {
                            if (Radio_Centroid.IsChecked == true)
                            {

                                foreach (w3Vertex v in vertices) v.Position.X *= (result / 100);
                            }
                            else
                            {
                                Coordinate custom = GetCustomCoordinate();
                                Calculator3D.ScaleRelativeToCoordinate(vertices, custom, true, false, false, (int)result);
                            }
                        }
                    }

                }
                InputXs.Text = "100";



            }
        }
        private Coordinate GetCustomCoordinate()
        {
            Coordinate coordinate = new();
            bool bx = float.TryParse(InputXs.Text, out float x);
            bool by = float.TryParse(InputYs.Text, out float y);
            bool bz = float.TryParse(InputZs.Text, out float z);
            if (bx) coordinate.X = x;
            if (by) coordinate.Y = y;
            if (bz) coordinate.Z = z;
            return coordinate;
        }

        private void SetYs(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                if (editMode_current == EditMode.Animator)
                {
                    bool parsed = float.TryParse(InputYs.Text, out float result);
                    bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                    w3Node node = GetSelectedNode();
                    if (node != null && parsedTrack && parsed)
                    {
                        if (node.Scaling.Keyframes.Any(x => x.Track == track))
                        {
                            node.Scaling.Keyframes.First(x => x.Track == track).Data[1] = result;
                        }
                        else
                        {
                            w3Keyframe k = new w3Keyframe(); k.Track = track;
                            k.Data = [1, result, 1];
                            k.OutTan = [1, 1, 1];
                            k.InTan = [1, 1, 1];
                            node.Scaling.Keyframes.Add(k);
                            node.Scaling.Keyframes = node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();

                        }

                    }

                }
                if (editMode_current == EditMode.Rigging || editMode_current == EditMode.Nodes) { }// nodes dont scale 
                if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Geosets || editMode_current == EditMode.Triangles || editMode_current == EditMode.Edges)
                {
                    bool parsed = float.TryParse(InputYs.Text, out float result);
                    if (parsed)
                    {
                        List<w3Vertex> vertices = new List<w3Vertex>();
                        if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                        if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                        if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                        if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                        if (vertices.Count > 1)
                        {


                            if (Radio_Centroid.IsChecked == true)
                            {

                                foreach (w3Vertex v in vertices) v.Position.Y *= (result / 100);
                            }
                            else
                            {
                                Coordinate custom = GetCustomCoordinate();
                                Calculator3D.ScaleRelativeToCoordinate(vertices, custom, false, true, false, (int)result);
                            }
                        }
                    }

                }
                InputYs.Text = "100";
            }
        }

        private void SetZs(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {


                if (editMode_current == EditMode.Animator)
                {
                    bool parsed = float.TryParse(InputZs.Text, out float result);
                    bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                    w3Node node = GetSelectedNode();
                    if (node != null && parsedTrack && parsed)
                    {
                        if (node.Scaling.Keyframes.Any(x => x.Track == track))
                        {
                            node.Scaling.Keyframes.First(x => x.Track == track).Data[2] = result;
                        }
                        else
                        {
                            w3Keyframe k = new w3Keyframe(); k.Track = track;
                            k.Data = [1, 1, result];
                            k.OutTan = [1, 1, 1];
                            k.InTan = [1, 1, 1];
                            node.Scaling.Keyframes.Add(k);
                            node.Scaling.Keyframes = node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();

                        }

                    }

                }
                if (editMode_current == EditMode.Rigging || editMode_current == EditMode.Nodes) { }// nodes dont scale 
                if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Geosets || editMode_current == EditMode.Triangles || editMode_current == EditMode.Edges)
                {
                    bool parsed = float.TryParse(InputZs.Text, out float result);
                    if (parsed)
                    {
                        List<w3Vertex> vertices = new List<w3Vertex>();
                        if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                        if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                        if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                        if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                        if (vertices.Count > 1)
                        {


                            if (vertices.Count > 1)
                            {


                                if (Radio_Centroid.IsChecked == true)
                                {

                                    foreach (w3Vertex v in vertices) v.Position.Z *= (result / 100);
                                }
                                else
                                {
                                    Coordinate custom = GetCustomCoordinate();
                                    Calculator3D.ScaleRelativeToCoordinate(vertices, custom, false, false, true, (int)result);
                                }
                            }

                        }
                    }

                }
                InputXs.Text = "100";
            }
        }

        private void SetXYZr(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(inputXr.Text, out float rotationX);
            bool parsed2 = float.TryParse(inputYr.Text, out float rotationY);
            bool parsed3 = float.TryParse(inputZr.Text, out float rotationZ);
            if (!parsed1 || parsed2 || parsed3) { MessageBox.Show("not a number", "Invalid request"); return; }
            if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Triangles || editMode_current == EditMode.Triangles || editMode_current == EditMode.Geosets)
            {
                List<w3Vertex> vertices = new List<w3Vertex>();
                if (editMode_current == EditMode.Vertices) vertices = GetSelectedVertices();
                if (editMode_current == EditMode.Triangles) vertices = GetVerticesOf(GetSelectedTriangles());
                if (editMode_current == EditMode.Edges) vertices = GetSelectedVerticesOf(GetSelectedEdges());
                if (editMode_current == EditMode.Geosets) vertices = GetVerticesOfSelectedGeoset();
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Radio_Centroid.IsChecked == true ? Calculator3D.GetCentroid(vertices) : GetCustomCoordinate();
                    if (Radio_Degrees.IsChecked == true)
                    {
                        if ((rotationX >= -360 && rotationX <= 360) && (rotationY >= -360 && rotationY <= 360) && (rotationZ >= -360 && rotationZ <= 360))
                        {


                            RotateVerticesFixed(vertices, AxisMode.Y, rotationX, centroid);
                            RotateVerticesFixed(vertices, AxisMode.Y, rotationY, centroid);
                            RotateVerticesFixed(vertices, AxisMode.Y, rotationZ, centroid);
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return;
                        }

                    }
                    else
                    {
                        rotationX = Calculator.RadiansToDegrees(GetRadianOutput(rotationX));
                        rotationY = Calculator.RadiansToDegrees(GetRadianOutput(rotationY));
                        rotationZ = Calculator.RadiansToDegrees(GetRadianOutput(rotationZ));
                        RotateVerticesFixed(vertices, AxisMode.X, rotationX, centroid);
                        RotateVerticesFixed(vertices, AxisMode.Y, rotationY, centroid);
                        RotateVerticesFixed(vertices, AxisMode.Z, rotationZ, centroid);
                    }





                }
            }
            if (editMode_current == EditMode.Normals)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if ((rotationX >= -360 && rotationX <= 360) && (rotationY >= -360 && rotationY <= 360) && (rotationZ >= -360 && rotationZ <= 360))
                {
                    RotateNormalFixed(vertices, AxisMode.X, rotationX);
                    RotateNormalFixed(vertices, AxisMode.Y, rotationY);
                    RotateNormalFixed(vertices, AxisMode.Z, rotationZ);

                }
                else { MessageBox.Show("Rotation must be between -360 and 360", "Invalid request"); return; }


            }
            if (editMode_current == EditMode.Animator)
            {
                bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                if (parsedTrack)
                {
                    if (track > 0)
                    {
                        if ((rotationX >= -360 && rotationX <= 360) && (rotationY >= -360 && rotationY <= 360) && (rotationZ >= -360 && rotationZ <= 360))
                        {
                            if (List_Nodes.SelectedItem != null)
                            {
                                w3Node node = GetSelectedNode();
                                if (node.Rotation.Keyframes.Any(x => x.Track == track))
                                {
                                    w3Keyframe k = node.Rotation.Keyframes.First(x => x.Track == track);
                                    k.Data = [rotationX, rotationY, rotationZ];
                                }
                                else
                                {
                                    w3Keyframe k = new w3Keyframe();
                                    k.Track = track;
                                    k.Data = [rotationX, rotationY, rotationZ];
                                    node.Rotation.Keyframes.Add(k);
                                    node.Rotation.Keyframes = node.Rotation.Keyframes.OrderBy(x => x.Track).ToList();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Select a node", "Precaution"); return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Rotation must be between -360 and 360", "Invalid request");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Track cannot be negative", "Invalid request"); return;
                    }
                }
                else { MessageBox.Show("Invalid input for track", "Invalid request"); return; }
            }
            inputXr.Text = "0";
            inputYr.Text = "0";
            inputZr.Text = "0";


        }

        private void SetXYZs(object sender, RoutedEventArgs e)
        {



            if (editMode_current == EditMode.Animator)
            {
                bool parsed1 = float.TryParse(InputXs.Text, out float result1);
                bool parsed2 = float.TryParse(InputYs.Text, out float result2);
                bool parsed3 = float.TryParse(InputZs.Text, out float result3);
                bool parsedTrack = int.TryParse(InputCurrentTrack.Text, out int track);
                w3Node node = GetSelectedNode();
                if (node != null && parsedTrack && parsed1 && parsed2 && parsed3)
                {
                    if (node.Scaling.Keyframes.Any(x => x.Track == track))
                    {
                        node.Scaling.Keyframes.First(x => x.Track == track).Data[0] = result1;
                        node.Scaling.Keyframes.First(x => x.Track == track).Data[1] = result2;
                        node.Scaling.Keyframes.First(x => x.Track == track).Data[2] = result3;
                    }
                    else
                    {
                        w3Keyframe k = new w3Keyframe(); k.Track = track;
                        k.Data = [result1, result2, result3];
                        k.OutTan = [1, 1, 1];
                        k.InTan = [1, 1, 1];
                        node.Scaling.Keyframes.Add(k);
                        node.Scaling.Keyframes = node.Scaling.Keyframes.OrderBy(x => x.Track).ToList();

                    }

                }

            }
            if (editMode_current == EditMode.Rigging || editMode_current == EditMode.Nodes) { }// nodes dont scale 
            if (editMode_current == EditMode.Vertices || editMode_current == EditMode.Geosets || editMode_current == EditMode.Triangles || editMode_current == EditMode.Edges)
            {
                bool parsed1 = float.TryParse(InputXs.Text, out float result1);
                bool parsed2 = float.TryParse(InputYs.Text, out float result2);
                bool parsed3 = float.TryParse(InputZs.Text, out float result3);
                if (parsed1 && parsed2 && parsed3)
                {
                    List<w3Vertex> vertices = GetSelectedVertices();
                    if (vertices.Count > 1)
                    {
                        if (Radio_Centroid.IsChecked == true)
                        {

                            foreach (w3Vertex v in vertices) v.Position.X *= (result1 / 100);
                            foreach (w3Vertex v in vertices) v.Position.Y *= (result2 / 100);
                            foreach (w3Vertex v in vertices) v.Position.Z *= (result3 / 100);
                        }
                        else
                        {
                            Coordinate custom = GetCustomCoordinate();
                            Calculator3D.ScaleRelativeToCoordinate(vertices, custom, true, true, true, result1, result2, result3);
                        }

                    }
                }

            }
            InputXs.Text = "100";
            InputYs.Text = "100";
            InputZs.Text = "100";
        }
        private void UpdateSelectionData()
        {
            int countVertices = 0;
            int countTriangles = 0;
            int countEdges = 0;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex vertex in geo.Vertices) { countVertices += vertex.isSelected ? 1 : 0; }
                foreach (w3Triangle tr in geo.Triangles) { countTriangles += tr.isSelected ? 1 : 0; }
                foreach (w3Edge e in geo.Edges) { countEdges += e.isSelected ? 1 : 0; }
            }
            List<w3Vertex> vertices = GetSelectedVertices();
            Data_SelVertices.Text = "Vertices: " + countVertices.ToString();
            Data_SelTriangles.Text = "Triangles: " + countTriangles.ToString();
            Data_SelEdges.Text = "Edges: " + countEdges.ToString();
            Data_SelCentroid.Text = "Centroid: " + Calculator3D.GetCentroid(vertices).ToStringBroken(); ;
        }
        private void SelectAllGeosetVertices(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {
                foreach (w3Geoset g in geosets)
                {
                    if (editMode_current == EditMode.Vertices)
                    {
                        foreach (w3Vertex v in g.Vertices) { v.isSelected = true; }
                    }

                    if (editMode_current == EditMode.Triangles)
                    {
                        foreach (w3Triangle v in g.Triangles) { v.isSelected = true; v.Vertex1.isSelected = true; v.Vertex2.isSelected = true; v.Vertex3.isSelected = true; }
                    }
                    if (editMode_current == EditMode.Edges)
                    {
                        foreach (w3Edge v in g.Edges) { v.isSelected = true; v.Vertex1.isSelected = true; v.Vertex2.isSelected = true; }
                    }
                }
            }
            UpdateSelectionData();
        }

        private void QuickClean(object sender, RoutedEventArgs e)
        {
            quickClean qc = new quickClean(CurrentModel);
            qc.ShowDialog();
            if (qc.DialogResult == true) { RefreshNodesList(); }
        }

        private void ImportAnimations(object sender, RoutedEventArgs e)
        {
            import_animations ia = new import_animations(CurrentModel);
            ia.ShowDialog();
            if (ia.DialogResult == true)
            {
                if (ia.SequencesAdded > 0) { RefreshSequencesList(); }
            }
        }

        private void ClearSequencesAll(object sender, RoutedEventArgs e)
        {
            foreach (w3Node node in CurrentModel.Nodes)
            {
                node.Translation.Keyframes.Clear();
                node.Rotation.Keyframes.Clear();
                node.Scaling.Keyframes.Clear();
            }
            FillTimeline(GetSelectedSequence());
        }

        private void ClearSequencesTrans(object sender, RoutedEventArgs e)
        {
            foreach (w3Node node in CurrentModel.Nodes)
            {
                node.Translation.Keyframes.Clear();

            }
            FillTimeline(GetSelectedSequence());
        }

        private void ClearSequencesRot(object sender, RoutedEventArgs e)
        {
            foreach (w3Node node in CurrentModel.Nodes)
            {

                node.Rotation.Keyframes.Clear();

            }
        }

        private void ClearSequencesScale(object sender, RoutedEventArgs e)
        {
            foreach (w3Node node in CurrentModel.Nodes)
            {

                node.Scaling.Keyframes.Clear();
            }
            FillTimeline(GetSelectedSequence());
        }

        private void removeAllSequences(object sender, RoutedEventArgs e)
        {
            CurrentModel.Sequences.Clear();
            foreach (w3Node node in CurrentModel.Nodes)
            {
                node.Translation.Keyframes.Clear();
                node.Rotation.Keyframes.Clear();
                node.Scaling.Keyframes.Clear();
            }
            ListSequences.Items.Clear(); FillTimeline(GetSelectedSequence());
        }

        private void removeAllGlobalSequences(object sender, RoutedEventArgs e)
        {
            foreach (w3Transformation t in CurrentModel.Transformations) t.Global_Sequence_ID = -1;
            CurrentModel.Global_Sequences.Clear();
        }

        private void ViewCamera(object sender, RoutedEventArgs e)
        {
            if (CameraList.SelectedItem == null) { return; }
            string name = (CameraList.SelectedItem as ListBoxItem).Content.ToString();
            w3Camera cam = CurrentModel.Cameras.First(x => x.Name == name);
            CameraControl.eyeX = cam.Position.StaticValue[0];
            CameraControl.eyeY = cam.Position.StaticValue[1];
            CameraControl.eyeZ = cam.Position.StaticValue[2];
            CameraControl.CenterX = cam.Target.StaticValue[0];
            CameraControl.CenterY = cam.Target.StaticValue[1];
            CameraControl.CenterZ = cam.Target.StaticValue[2];
        }
        private Extent GetExtrudedExtent(Coordinate[] corners, float depth)
        {
            // Initialize min and max values with the first corner
            float minx = corners[0].X;
            float miny = corners[0].Y;
            float minz = corners[0].Z;
            float maxx = corners[0].X;
            float maxy = corners[0].Y;
            float maxz = corners[0].Z;

            // Iterate through the corners and extrude them along their direction
            for (int i = 0; i < corners.Length; i++)
            {
                Coordinate corner = corners[i];

                // Calculate the direction vector from the origin (0,0,0) to the corner
                Vector3 direction = new Vector3(corner.X, corner.Y, corner.Z);
                Vector3 extrudedCorner = direction + Vector3.Normalize(direction) * depth;

                // Update min and max values based on extruded corners
                if (extrudedCorner.X < minx) minx = extrudedCorner.X;
                if (extrudedCorner.Y < miny) miny = extrudedCorner.Y;
                if (extrudedCorner.Z < minz) minz = extrudedCorner.Z;

                if (extrudedCorner.X > maxx) maxx = extrudedCorner.X;
                if (extrudedCorner.Y > maxy) maxy = extrudedCorner.Y;
                if (extrudedCorner.Z > maxz) maxz = extrudedCorner.Z;
            }

            // Return the extent with the min/max values
            return new Extent(minx, miny, minz, maxx, maxy, maxz, 0);
        }


        public Coordinate[] GetCameraCorners()
        {
            // Coordinate topleft = MousePicke
            float witdth = (float)Scene_Canvas_.ActualWidth;
            float height = (float)Scene_Canvas_.ActualHeight;
            Vector2 pos1 = new Vector2(0, 0);
            Vector2 pos2 = new Vector2(witdth, 0);
            Vector2 pos3 = new Vector2(0, height);
            Vector2 pos4 = new Vector2(witdth, height);

            MousePicker picker1 = new MousePicker(Scene_GL, witdth, height, pos1);
            MousePicker picker2 = new MousePicker(Scene_GL, witdth, height, pos2);
            MousePicker picker3 = new MousePicker(Scene_GL, witdth, height, pos4);
            MousePicker picker4 = new MousePicker(Scene_GL, witdth, height, pos4);

            return new Coordinate[]
            {
                new Coordinate(picker1.WorldRay.X, picker1.WorldRay.Y, picker1.WorldRay.Z),
                new Coordinate(picker2.WorldRay.X, picker2.WorldRay.Y, picker2.WorldRay.Z),
                new Coordinate(picker3.WorldRay.X, picker3.WorldRay.Y, picker3.WorldRay.Z),
                new Coordinate(picker4.WorldRay.X, picker4.WorldRay.Y, picker4.WorldRay.Z),
            };
        }



        private void SelectSight(object sender, RoutedEventArgs e)
        {
            Extent ex = GetExtrudedExtent(GetCameraCorners(), 40);
            CurrentModel.Geosets.Add(Renderer.CreateCubeFromExtent(ex));
            SelectGeometryIfInsideExtent(ex);
        }
        private void SelectGeometryIfInsideExtent(Extent ex)
        {
            foreach (w3Geoset g in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in g.Vertices)
                {
                    v.isSelected = CoordinateWithinExtents(v.Position, ex);
                }
            }

        }

        private void ModifySnappungGrid(object sender, TextChangedEventArgs e)
        {
            bool parsed = float.TryParse(InputSnapSystem.Text, out float spacing);
            SnapUtility.WorldSnapSpacing = parsed ? spacing : 1;
        }

        private void SnapXp(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapXPlus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapXPlus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapXPlus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }


        }

        private void SnapYp(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapYPlus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapYPlus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapYPlus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }

        }

        private void SnapZp(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapZPlus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapZPlus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapZPlus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }

        }

        private void SnapXn(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapXMinus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapXMinus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapXMinus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }

        }

        private void SnapYn(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapYMinus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapYMinus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapYMinus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }

        }

        private void SnapZn(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapZMinus(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapZMinus(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapZMinus(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }
            SetSaved(false);
        }

        private void SnapToNearest(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                CurrentlySelectedNode.PivotPoint = SnapUtility.SnapToNearest(CurrentlySelectedNode.PivotPoint);
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count == 1) { vertices[0].Position = SnapUtility.SnapToNearest(vertices[0].Position); }
                if (vertices.Count > 1)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    Coordinate newPos = SnapUtility.SnapToNearest(centroid);
                    Calculator3D.CenterVertices(vertices, newPos.X, newPos.Y, newPos.Z);
                }
            }
            SetSaved(false);
        }

        private void RemoveAllGeosets(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure?", "Remove all geosets", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppendHistory();
                CurrentModel.Geosets.Clear();
                CurrentModel.Geoset_Animations.Clear();
                RefreshGeosetList();

            }
            SetSaved(false);
        }

        private void CreatePlane(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Materials.Count == 0) { MessageBox.Show("No materials", "Precaution"); return; }
            CreatePlane c = new CreatePlane(CurrentModel);

            c.ShowDialog();
            if (c.DialogResult == true) RefreshGeosetList(); RefreshTextureListForGeosets(); RefreshNodesList(); SetSaved(false);

        }

        private void DoReport(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("If you'd like to report a bug, suggest improving exisiting feature, or suggest a new feature, message me on Discord <stan0033>\n\n It's preferable that when you report you should include a screenshot and exact description or the bug, and explanation what you did before it popped up.", "Feedback");
        }

        private void ReattachAllVerticesTo(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count == 0) { return; }
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (CurrentModel.Nodes.Count(x => x.Data is Bone) > 1)
            {
                List<string> names = CurrentModel.Nodes.Where(x => x.Data is Bone).Select(x => x.Name).ToList();
                Selector s = new Selector(names);
                s.ShowDialog();
                if (s.DialogResult == true)
                {

                    string selected = s.Selected;

                    int id = CurrentModel.Nodes.First(x => x.Name == selected).objectId;
                    foreach (w3Geoset g in geosets)
                    {
                        foreach (w3Vertex v in g.Vertices) { v.AttachedTo.Clear(); v.AttachedTo.Add(id); }
                    }
                    SetSaved(false);
                }
            }
            else { MessageBox.Show("At least 2 bones must be present", "Precaution"); }

        }

        private void CallAttachInfoGeosets(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 0)
            {
                List<w3Geoset> geosts = GetSelectedGeosets();
                AttachInfoGeosets ai = new AttachInfoGeosets(geosts, CurrentModel.Nodes);
                ai.ShowDialog();
            }
        }

        private void AlignGEosetsByX(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 1)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();

                // Get the Z-coordinate of the first geoset's centroid
                Coordinate centroid1 = Calculator3D.GetCentroid(geosets[0].Vertices);

                for (int i = 1; i < geosets.Count; i++)
                {
                    // Get the centroid of the current geoset
                    Coordinate centroid2 = Calculator3D.GetCentroid(geosets[i].Vertices);

                    // Calculate the Z-offset to align with the first geoset's Z
                    float zOffset = centroid1.X - centroid2.X;

                    // Move all vertices by this Z-offset to align centroids
                    Calculator3D.ShiftVerticesBy(geosets[i].Vertices, zOffset, AxisMode.X);
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select 2 or more geosets", "Invalid request");
            }
        }

        private void AlignGEosetsByY(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 1)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();

                // Get the Z-coordinate of the first geoset's centroid
                Coordinate centroid1 = Calculator3D.GetCentroid(geosets[0].Vertices);

                for (int i = 1; i < geosets.Count; i++)
                {
                    // Get the centroid of the current geoset
                    Coordinate centroid2 = Calculator3D.GetCentroid(geosets[i].Vertices);

                    // Calculate the Z-offset to align with the first geoset's Z
                    float zOffset = centroid1.Y - centroid2.Y;

                    // Move all vertices by this Z-offset to align centroids
                    Calculator3D.ShiftVerticesBy(geosets[i].Vertices, zOffset, AxisMode.Y);
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select 2 or more geosets", "Invalid request");
            }
        }

        private void AlignGeosetsByZ(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 1)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();

                // Get the Z-coordinate of the first geoset's centroid
                Coordinate centroid1 = Calculator3D.GetCentroid(geosets[0].Vertices);

                for (int i = 1; i < geosets.Count; i++)
                {
                    // Get the centroid of the current geoset
                    Coordinate centroid2 = Calculator3D.GetCentroid(geosets[i].Vertices);

                    // Calculate the Z-offset to align with the first geoset's Z
                    float zOffset = centroid1.Z - centroid2.Z;

                    // Move all vertices by this Z-offset to align centroids
                    Calculator3D.ShiftVerticesBy(geosets[i].Vertices, zOffset, AxisMode.Z);
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select 2 or more geosets", "Invalid request");
            }
        }

        private void CutFrame(object sender, RoutedEventArgs e)
        {
            bool parsed = int.TryParse(InputCurrentTrack.Text, out int track);
            if (!parsed) { MessageBox.Show("input track not an integer", "Invalid request"); return; }
            CopiedFrame = track;
            CutFrame_ = true;
        }

        private void NegateUs(object sender, RoutedEventArgs e)
        {
            foreach (w3Triangle t in SelectedTriangles)
            {
                if (t.Vertex1.isSelectedUV) { t.Vertex1.Texture_Position.U = -t.Vertex1.Texture_Position.U; }
                if (t.Vertex2.isSelectedUV) { t.Vertex2.Texture_Position.U = -t.Vertex2.Texture_Position.U; }
                if (t.Vertex3.isSelectedUV) { t.Vertex3.Texture_Position.U = -t.Vertex3.Texture_Position.U; }
            }

            RefreshUvMapping();
            SetSaved(false);
        }

        private void RefreshUvMapping()
        {
            // re-apply texture
            // re-draw all vertices and edges of the selected triangles
            // clear any selection rectangle
            // apply grid
            throw new NotImplementedException();
        }

        private void NegateVs(object sender, RoutedEventArgs e)
        {
            foreach (w3Triangle t in SelectedTriangles)
            {
                if (t.Vertex1.isSelectedUV) { t.Vertex1.Texture_Position.V = -t.Vertex1.Texture_Position.V; }
                if (t.Vertex2.isSelectedUV) { t.Vertex2.Texture_Position.V = -t.Vertex2.Texture_Position.V; }
                if (t.Vertex3.isSelectedUV) { t.Vertex3.Texture_Position.V = -t.Vertex3.Texture_Position.V; }
            }

            RefreshUvMapping();
            SetSaved(false);

        }

        private void SwapUVs(object sender, RoutedEventArgs e)
        {
            foreach (w3Triangle t in SelectedTriangles)
            {
                if (t.Vertex1.isSelectedUV) { t.Vertex1.Texture_Position.Swap(); }
                if (t.Vertex2.isSelectedUV) { t.Vertex2.Texture_Position.Swap(); }
                if (t.Vertex3.isSelectedUV) { t.Vertex3.Texture_Position.Swap(); }
            }
            SetSaved(false);
        }

        private void CollapseUV(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices =GetSelectedUVVertices();
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position = new Coordinate2D();
            }
                RefreshUvMapping();
                SetSaved(false);

            }

       
  
    private void AlignUVsHorizontally(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = SelectedTriangles
     .SelectMany(x => new[] { x.Vertex1, x.Vertex2, x.Vertex3 })
     .ToList();

            List<w3Vertex> vertices_uv = vertices.Where(x => x.isSelectedUV).ToList();
            if (vertices_uv.Count > 1)
            {
                w3Vertex one = vertices_uv[0];
                for (int i = 1; i < vertices_uv.Count; i++)
                {
                    vertices_uv[i].Texture_Position.U = one.Texture_Position.U;
                }
                RefreshUvMapping();
                SetSaved(false);
            }


        }


        private void AlignUVsVertically(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = SelectedTriangles
     .SelectMany(x => new[] { x.Vertex1, x.Vertex2, x.Vertex3 })
     .ToList();

            List<w3Vertex> vertices_uv = vertices.Where(x => x.isSelectedUV).ToList();
            if (vertices_uv.Count > 1)
            {
                w3Vertex one = vertices_uv[0];
                for (int i = 1; i < vertices_uv.Count; i++)
                {
                    vertices_uv[i].Texture_Position.V = one.Texture_Position.V;
                }
                RefreshUvMapping();
                SetSaved(false);
            }
        }

        private void SnapUV_Up(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = SelectedTriangles
   .SelectMany(x => new[] { x.Vertex1, x.Vertex2, x.Vertex3 })
   .ToList();

            List<w3Vertex> vertices_uv = vertices.Where(x => x.isSelectedUV).ToList();
            if (vertices_uv.Count > 1)
            {
                w3Vertex one = vertices_uv[0];
                for (int i = 1; i < vertices_uv.Count; i++)
                {
                    vertices_uv[i].Texture_Position.U = one.Texture_Position.U;
                }
                RefreshUvMapping();
                SetSaved(false);
            }
        }
        private void SnapUV_Um(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void SnapUV_Vp(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void SnapUV_Vm(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void SnapUV_Nearest(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void ShowUVSnapMenu(object sender, RoutedEventArgs e)
        {
           
        }

        private void ProjectUV(object sender, RoutedEventArgs e)
        {
            //unfinished
            RefreshUvMapping();
            SetSaved(false);
        }

        private void MDLX_call(object sender, RoutedEventArgs e)
        {

            MDLX_Window_INSTANCE.Show();
        }

        private void RegisterExtensions(object sender, RoutedEventArgs e)
        {
            ExtensionHelper.Register();

        }

        private void call_PTM(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(AppHelper.appPath, "Tools\\CPTM.exe");
            if (File.Exists(path))
            {
                Process.Start(path);


            }
        }

        private void call_LSC(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(AppHelper.appPath, "Tools\\LSM\\Loading Screen Maker.exe");
            if (File.Exists(path))
            {
                Process.Start(path);


            }
        }

        private void RemapSequences(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Sequences.Count == 0)
            {
                MessageBox.Show("No sequences", "Invalid request"); return;
            }
            remap_sq rs = new remap_sq();
            rs.ShowDialog();

            if (rs.DialogResult == true)
            {
                CurrentModel.RefreshTransformationsList();
                int dur = rs.Duration;
                int from = 0;
                int to = dur;
                foreach (w3Sequence seq in CurrentModel.Sequences)
                {
                    int oldFrom = seq.From;
                    int oldTo = seq.To;
                    int oldLength = oldTo - oldFrom;
                    int newLength = to - from;

                    // Set the new sequence interval
                    seq.From = from;
                    seq.To = to;

                    // Update the keyframes to account for the sequence resizing

                    foreach (w3Transformation t in CurrentModel.Transformations)
                    {
                        foreach (w3Keyframe k in t.Keyframes)
                        {
                            // Check if the keyframe track is within the old interval
                            if (k.Track >= oldFrom && k.Track <= oldTo)
                            {
                                // Calculate the proportional position of the keyframe within the old interval
                                float proportion = (float)(k.Track - oldFrom) / oldLength;

                                // Update the keyframe's track based on the new interval
                                k.Track = (int)(from + proportion * newLength);
                            }
                        }
                    }
                    from = dur + 1;
                    to = from + dur;
                }
                RefreshSequencesList();
                SetSaved(false);
            }
        }

        private void CopyForKeyframes(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (w3Sequence seq in CurrentModel.Sequences)
            {
                sb.AppendLine($"{seq.From}: {seq.Name} - (From)");
                sb.AppendLine($"{seq.To}: {seq.Name} - (To)");
            }
            Clipboard.SetText(sb.ToString());
        }

        private void MergeSequence(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Sequences.Count < 2) { MessageBox.Show("There must be at least 2 sequences present", "Invalid request"); return; }
            SequencesSelector s = new SequencesSelector("Merge two or more sequences", CurrentModel.Sequences);
            s.ShowDialog();
            if (s.DialogResult == true)
            {
                List<w3Sequence> selected = new List<w3Sequence>();
                for (int i = 0; i < CurrentModel.Sequences.Count; i++)
                {
                    if (s.indexes.Contains(i)) { selected.Add(CurrentModel.Sequences[i]); }
                }
                selected.OrderBy(x => x.From);
                CurrentModel.RefreshTransformationsList();

                Calculator3D.MergeSequences(selected, CurrentModel.Sequences, s.NewName, CurrentModel.Transformations);
                SelectedSequence(null, null);
                SetSaved(false);
            }
        }

        private void SplitSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            w3Sequence sequence = GetSelectedSequence();
            SplitSEquenceDialog sd = new SplitSEquenceDialog(sequence, CurrentModel);
            sd.ShowDialog();
            if (sd.DialogResult == true)
            {
                RefreshSequencesList();
                SetSaved(false);
            }
        }

        private void TileLoopSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            w3Sequence sequence = GetSelectedSequence();
            if (sequence == null) { return; }
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;
                if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
                w3Sequence seq = GetSelectedSequence();
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr)
                    {
                        KeyframesModifier.TileLoopLocal(node.Translation.Keyframes, seq.From, seq.To);
                    }
                    if (ro)
                    {
                        KeyframesModifier.TileLoopLocal(node.Rotation.Keyframes, seq.From, seq.To);
                    }
                    if (sc)
                    {
                        KeyframesModifier.TileLoopLocal(node.Scaling.Keyframes, seq.From, seq.To);
                    }

                }
                SetSaved(false);
            }
        }

        private void FillGaps(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            w3Sequence sequence = GetSelectedSequence();
            if (sequence == null) { return; }
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;
                if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
                w3Sequence seq = GetSelectedSequence();
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr)
                    {
                        KeyframesModifier.FillGapsLocal(node.Translation.Keyframes, seq.From, seq.To);
                    }
                    if (ro)
                    {
                        KeyframesModifier.FillGapsLocal(node.Rotation.Keyframes, seq.From, seq.To);
                    }
                    if (sc)
                    {
                        KeyframesModifier.FillGapsLocal(node.Scaling.Keyframes, seq.From, seq.To);
                    }

                }
                SetSaved(false);
            }


        }

        private void Quantize(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem == null) { return; }
            w3Sequence sequence = GetSelectedSequence();
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;
                if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
                w3Sequence seq = GetSelectedSequence();
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr)
                    {
                        KeyframesModifier.QuantizeLocal(node.Translation.Keyframes, seq.From, seq.To);
                    }
                    if (ro)
                    {
                        KeyframesModifier.QuantizeLocal(node.Rotation.Keyframes, seq.From, seq.To);
                    }
                    if (sc)
                    {
                        KeyframesModifier.QuantizeLocal(node.Scaling.Keyframes, seq.From, seq.To);
                    }

                }
                SetSaved(false);
            }


        }

        private void MirrorSequenceOne(object sender, RoutedEventArgs e)
        {
            MirrorSequence m = new MirrorSequence();
            m.ShowDialog();
            if (m.DialogResult == true)
            {
                if (m.Radio_1.IsChecked == true)
                {

                }
                if (m.Radio_2.IsChecked == true)
                {

                }
                if (m.Radio_3.IsChecked == true)
                {

                }
                if (m.Check_1.IsChecked == true)
                {

                }
                if (m.Check_2.IsChecked == true)
                {

                }
                if (m.Check_3.IsChecked == true)
                {

                }
            }
            SetSaved(false);
        }


        private void ReverseSequenceOrder(object sender, RoutedEventArgs e)
        {
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;
                if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
                w3Sequence seq = GetSelectedSequence();
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr) { KeyframesModifier.ReverseLocalOrder(node.Translation.Keyframes, seq.From, seq.To); }
                    if (ro) { KeyframesModifier.ReverseLocalOrder(node.Rotation.Keyframes, seq.From, seq.To); }
                    if (sc) { KeyframesModifier.ReverseLocalOrder(node.Scaling.Keyframes, seq.From, seq.To); }

                }
                SelectedSequence(null, null);
                SetSaved(false);
            }
        }

        private void ReverseSequenceOrderAll(object sender, RoutedEventArgs e)
        {

            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;

                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr) { node.Translation.Keyframes.Reverse(); }
                    if (ro) { node.Rotation.Keyframes.Reverse(); }
                    if (sc) { node.Scaling.Keyframes.Reverse(); }

                }
                SelectedSequence(null, null);
                SetSaved(false);
            }
        }

        private void QuantizeAllSequences(object sender, RoutedEventArgs e)
        {

            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;

                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr) { KeyframesModifier.Quantize(node.Translation.Keyframes); }
                    if (ro) { KeyframesModifier.Quantize(node.Rotation.Keyframes); }
                    if (sc) { KeyframesModifier.Quantize(node.Scaling.Keyframes); }

                }
                FillTimeline(GetSelectedSequence());
                SetSaved(false);
            }
        }

        private void TileLoopSequenceAll(object sender, RoutedEventArgs e)
        {
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;

                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr) { KeyframesModifier.TileLoop(node.Translation.Keyframes); }
                    if (ro) { KeyframesModifier.TileLoop(node.Rotation.Keyframes); }
                    if (sc) { KeyframesModifier.TileLoop(node.Scaling.Keyframes); }

                }
                FillTimeline(GetSelectedSequence());
                SetSaved(false);
            }
        }

        private void FillGapsAll(object sender, RoutedEventArgs e)
        {
            SelectTransformations st = new SelectTransformations();
            st.ShowDialog();
            if (st.DialogResult == true)
            {
                bool tr = st.Check_1.IsChecked == true;
                bool ro = st.Check_2.IsChecked == true;
                bool sc = st.Check_3.IsChecked == true;

                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (tr) { KeyframesModifier.FillGaps(node.Translation.Keyframes); }
                    if (ro) { KeyframesModifier.FillGaps(node.Rotation.Keyframes); }
                    if (sc) { KeyframesModifier.FillGaps(node.Scaling.Keyframes); }

                }
                FillTimeline(GetSelectedSequence());
                SetSaved(false);
            }
        }

        private void MirrorSequenceAll(object sender, RoutedEventArgs e)
        {
            FillTimeline(GetSelectedSequence());
        }

        private void question1(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            CurrentModel.RefreshTransformationsList();

            foreach (w3Sequence seq in CurrentModel.Sequences)
            {
                bool empty = true;
                foreach (w3Transformation t in CurrentModel.Transformations)
                {

                    foreach (w3Keyframe k in t.Keyframes)
                    {
                        if (k.Track >= seq.From && k.Track <= seq.To) { empty = false; break; }
                    }
                }
                if (empty)
                {
                    sb.AppendLine(seq.Name);
                }
            }



            if (sb.Length > 0)
            {
                string result = "These sequences are not animated:\n" + sb.ToString();
                CBox.Show(result);
            }
            else
            {
                MessageBox.Show("No un-animated sequences", "Report");
            }
        }

        private void question2(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (w3Node node in CurrentModel.Nodes)
            {
                if (node.Data is Bone)
                {
                    if (
                        node.Translation.Keyframes.Count == 0 &&
                        node.Rotation.Keyframes.Count == 0 &&
                        node.Scaling.Keyframes.Count == 0



                        ) { sb.AppendLine(node.Name); }
                }
            }
            if (sb.Length > 0)
            {
                string result = "These bones are not animated:\n" + sb.ToString();
                CBox.Show(result);
            }
            else
            {
                MessageBox.Show("No un-animated bones", "Report");
            }

        }

        private void question3(object sender, RoutedEventArgs e)
        {
            var unusedIntervals = IntervalFinder.FindUnusedIntervals(CurrentModel.Sequences);
            string message = string.Join(Environment.NewLine, unusedIntervals);

            CBox.Show(message);
        }
        private w3Node? CopiedNodeForTransformations;
        private void CopyNodeTransformations(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                CopiedNodeForTransformations = GetSelectedNode();
            }
            else
            {
                MessageBox.Show("Select a node", "Invalid request");
            }
        }

        private void PasteNodeTranslation(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                if (CopiedNodeForTransformations == null)
                {
                    MessageBox.Show("Nothing was copied"); return;
                }
                else
                {
                    if (CurrentModel.Nodes.Contains(CopiedNodeForTransformations))
                    {
                        w3Node targetNode = GetSelectedNode();
                        targetNode.Translation = CopiedNodeForTransformations.Translation.Clone();
                        CopiedNodeForTransformations = null;
                        SetSaved(false);
                    }
                    else
                    {
                        MessageBox.Show("The copied node no longer exists", "Invalid request");
                    }
                }


            }
            else
            {
                MessageBox.Show("Select a node", "Invalid request");
            }
        }

        private void PasteNodeRotations(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                if (CopiedNodeForTransformations == null)
                {
                    MessageBox.Show("Nothing was copied"); return;
                }
                else
                {
                    w3Node targetNode = GetSelectedNode();
                    targetNode.Rotation = CopiedNodeForTransformations.Rotation.Clone();
                    CopiedNodeForTransformations = null;
                    SetSaved(false);
                }


            }
        }

        private void PasteNodeScaling(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                if (CopiedNodeForTransformations == null)
                {
                    MessageBox.Show("Nothing was copied"); return;
                }
                else
                {
                    w3Node targetNode = GetSelectedNode();
                    targetNode.Scaling = CopiedNodeForTransformations.Scaling.Clone();
                    CopiedNodeForTransformations = null;
                    SetSaved(false);
                }


            }
        }

        private void PasteNodeAll(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                if (CopiedNodeForTransformations == null)
                {
                    MessageBox.Show("Nothing was copied"); return;
                }
                else
                {
                    w3Node targetNode = GetSelectedNode();
                    targetNode.Translation = CopiedNodeForTransformations.Translation.Clone();
                    targetNode.Rotation = CopiedNodeForTransformations.Rotation.Clone();
                    targetNode.Scaling = CopiedNodeForTransformations.Scaling.Clone();
                    CopiedNodeForTransformations = null;
                    SetSaved(false);
                }


            }

        }

        private void Alphabetize(object sender, RoutedEventArgs e)
        {
            CurrentModel.Sequences.OrderBy(x => x.Name);
            RefreshSequencesList();
            SetSaved(false);
            // unfinished - rearrange keyframes to fit alphabetization
        }

        private void CloseGapsSequences(object sender, RoutedEventArgs e)
        {
            //unfinished

            FillTimeline(GetSelectedSequence());
            SetSaved(false);
        }
        private bool RangeOverlaps(int from, int to)
        {
            foreach (w3Sequence s in CurrentModel.Sequences)
            {
                if (from <= s.From && to <= s.To) { return true; }
            }
            return false;
        }

        private void DuplicateSequence(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                w3Sequence selected = GetSelectedSequence();

                if (selected != null)
                {
                    DuplicateSequence i = new DuplicateSequence();

                    i.Title = $"Duplicate sequence: {selected.Name}";
                    i.ShowDialog();
                    if (i.DialogResult == true)
                    {
                        string GivenName = i.InputName.Text.Trim();

                        if (GivenName.Length > 0)
                        {
                            GivenName = StringHelper.CapitalizeName(GivenName);
                            if (CurrentModel.Sequences.Any(x => x.Name.ToLower() == GivenName.ToLower()))
                            {
                                MessageBox.Show("A sequence with this name already exists", "Invalid request"); return;
                            }
                            else
                            {
                                int track = i.GivenValue;
                                int to = track + selected.Interval; ;
                                if (RangeOverlaps(track, to))
                                {
                                    MessageBox.Show("Cannot duplicate the sequence because its new interval overlaps with existsing sequences", "Invalid request"); return;

                                }
                                else
                                {
                                    w3Sequence duplicated = new w3Sequence(GivenName, track, to, 0, 0, selected.Looping);
                                    //unfinished
                                    CurrentModel.Sequences.Add(duplicated);
                                    RefreshSequencesList();
                                    SetSaved(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Empty name", "Invalid request");
                    }
                }
            }
        }
        private w3Vertex CopiedVertexPosition;
        private w3Triangle? CopiedUVTriangle;
        private w3Vertex? CopiedUVVertex;
        private void CopyPosition(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count != 1)
            {
                MessageBox.Show("Must select a single vertex", "Invalid request"); return;
            }
            CopiedVertexPosition = vertices[0];
        }

        private void PastePosition(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count != 1)
            {
                MessageBox.Show("Must select a single vertex", "Invalid request"); return;
            }
            if (CopiedVertexPosition == null)
            {
                MessageBox.Show("Nothing was copied", "Invalid request"); return;
            }
            vertices[0].Position.SetTo(CopiedVertexPosition.Position);
            SetSaved(false);

        }

        private void CopyUV(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count != 1)
                {
                    MessageBox.Show("Select a single vertex", "Invalid request");
                }
                else
                {
                    CopiedUVVertex = vertices[0];
                }
            }
            else if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count != 1)
                {
                    MessageBox.Show("Must select a single triangle", "Invalid request"); return;
                }

                CopiedUVTriangle = triangles[0];
            }

        }

        private void PasteUV(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count != 1)
                {
                    MessageBox.Show("Select a single vertex", "Invalid request");
                }
                else
                {
                    if (CopiedUVVertex == null) { MessageBox.Show("The copied vertex no longer exists", "Invalid request"); return; }
                    vertices[0].Texture_Position.SetTo(CopiedUVVertex.Texture_Position);
                    SetSaved(false);
                }
            }
            else if (editMode_current == EditMode.Triangles)
            {

                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count != 1)
                {
                    MessageBox.Show("Must select a single triangle", "Invalid request"); return;
                }
                if (CopiedUVTriangle == null)
                {
                    MessageBox.Show("Nothing was copied", "Invalid request"); return;
                }
                triangles[0].Vertex1.Texture_Position.SetTo(CopiedUVTriangle.Vertex1.Texture_Position);
                triangles[1].Vertex1.Texture_Position.SetTo(CopiedUVTriangle.Vertex2.Texture_Position);
                triangles[2].Vertex1.Texture_Position.SetTo(CopiedUVTriangle.Vertex3.Texture_Position);
                SetSaved(false);
            }

        }

        private void SwaptwoUVs(object sender, RoutedEventArgs e)
        {
            if (SelectedTriangles.Count == 2)
            {
                SelectedTriangles[0].Vertex1.Texture_Position.SwapWith(SelectedTriangles[1].Vertex1.Texture_Position);
                SelectedTriangles[0].Vertex2.Texture_Position.SwapWith(SelectedTriangles[1].Vertex2.Texture_Position);
                SelectedTriangles[0].Vertex3.Texture_Position.SwapWith(SelectedTriangles[1].Vertex3.Texture_Position);
                RefreshUvMapping();
                SetSaved(false);
            }
        }

        private void PanToGeoset(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count != 1)
            {
                MessageBox.Show("Must select a single geoset", "Invalid request");
            }
            else
            {
                w3Geoset geo = GetSelectedGeoset();
                Coordinate centroid = Calculator3D.GetCentroid(geo.Vertices);
                CameraControl.CenterX = centroid.X;
                CameraControl.CenterY = centroid.Y;
                CameraControl.CenterZ = centroid.Z;
                CameraControl.eyeX = centroid.X + 50;
                CameraControl.eyeY = centroid.Y + 50;
                CameraControl.eyeZ = centroid.Z + 50;
            }
        }

        private void GridCheckBox_CheckedYZ(object sender, RoutedEventArgs e)
        {
            DisplayOptions.GridYZ = GridCheckBoxYZ.IsChecked == true;
        }

        private void GridCheckBox_CheckedXZ(object sender, RoutedEventArgs e)
        {
            DisplayOptions.GridXZ = GridCheckBoxXZ.IsChecked == true;
        }

        private void OpenNodeProperies(object sender, MouseButtonEventArgs e)
        {
            Edittrs(null, null);
        }

        private w3Node copiedPPNode;
        private void CopyPivotPoint(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            copiedPPNode = GetSelectedNode();
        }

        private void PastePivotPoint(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            if (copiedPPNode == null)
            {
                MessageBox.Show("Nothing was copied", "Invalid request"); return;
            }
            if (CurrentModel.Nodes.Contains(copiedPPNode))
            {
                w3Node selected = GetSelectedNode();
                selected.PivotPoint.SetTo(copiedPPNode.PivotPoint);
                SetSaved(false);
            }
            else
            {
                MessageBox.Show("The copied node no longer exists", "Invalid request"); return;
            }
        }

        private void DuplicateNode(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            w3Node selected = GetSelectedNode();
            w3Node duplicated = selected.Clone();
            CurrentModel.Nodes.Add(duplicated);
            RefreshNodesList();
            SetSaved(false);
        }

        private void SnapUV_Mid(object sender, RoutedEventArgs e)
        {

        }

        private void FloorVertices(object sender, RoutedEventArgs e)
        {

            foreach (w3Vertex v in GetSelectedVertices()) v.Position.Floor(); SetSaved(false);
        }

        private void CeilingVertices(object sender, RoutedEventArgs e)
        {
            foreach (w3Vertex v in GetSelectedVertices()) v.Position.Ceiling(); SetSaved(false);
        }

        private void DetachEach(object sender, RoutedEventArgs e)
        {
            // Get selected triangles
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request");
                return;
            }

            // Ensure all triangles belong to the same geoset
            w3Geoset geo = TrianglesBelongToSameGeoset(triangles);

            if (geo.Triangles.Count == triangles.Count)
            {
                MessageBox.Show("Cannot detach the whole geoset from itself", "Invalid request");
                return;
            }
            if (geo != null)
            {
                // Track how many triangles use each vertex
                Dictionary<w3Vertex, int> vertexUsage = new Dictionary<w3Vertex, int>();
                foreach (w3Triangle tri in geo.Triangles)
                {
                    if (!vertexUsage.ContainsKey(tri.Vertex1)) vertexUsage[tri.Vertex1] = 0;
                    if (!vertexUsage.ContainsKey(tri.Vertex2)) vertexUsage[tri.Vertex2] = 0;
                    if (!vertexUsage.ContainsKey(tri.Vertex3)) vertexUsage[tri.Vertex3] = 0;

                    vertexUsage[tri.Vertex1]++;
                    vertexUsage[tri.Vertex2]++;
                    vertexUsage[tri.Vertex3]++;
                }

                // Detach vertices for each triangle
                foreach (w3Triangle triangle in triangles)
                {
                    // Create copies of vertices only if they are shared
                    if (vertexUsage[triangle.Vertex1] > 1)
                    {
                        w3Vertex newVertex1 = triangle.Vertex1.Clone();
                        geo.Vertices.Add(newVertex1);
                        triangle.Vertex1 = newVertex1;
                        vertexUsage[triangle.Vertex1]--;
                    }

                    if (vertexUsage[triangle.Vertex2] > 1)
                    {
                        w3Vertex newVertex2 = triangle.Vertex2.Clone();
                        geo.Vertices.Add(newVertex2);
                        triangle.Vertex2 = newVertex2;
                        vertexUsage[triangle.Vertex2]--;
                    }

                    if (vertexUsage[triangle.Vertex3] > 1)
                    {
                        w3Vertex newVertex3 = triangle.Vertex3.Clone();
                        geo.Vertices.Add(newVertex3);
                        triangle.Vertex3 = newVertex3;
                        vertexUsage[triangle.Vertex3]--;
                    }
                }

                SetSaved(false);

            }
            else
            {
                MessageBox.Show("Can only detach from one geoset at a time", "Invalid request");
            }
        }


        private void DetachEachAsNewGeoset(object sender, RoutedEventArgs e)
        {
            // Get selected triangles
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request");
                return;
            }

            // Ensure all triangles belong to the same geoset
            w3Geoset geo = TrianglesBelongToSameGeoset(triangles);
            if (geo == null)
            {
                MessageBox.Show("Can only detach from one geoset at a time", "Invalid request");
                return;
            }

            // Create a new geoset for each selected triangle
            foreach (w3Triangle triangle in triangles)
            {
                // Create a new geoset
                w3Geoset newGeoset = new w3Geoset();

                // Create a mapping to ensure vertices are cloned correctly
                Dictionary<w3Vertex, w3Vertex> vertexMap = new Dictionary<w3Vertex, w3Vertex>();

                // Clone vertices only if they haven't been cloned already
                if (!vertexMap.ContainsKey(triangle.Vertex1))
                    vertexMap[triangle.Vertex1] = triangle.Vertex1.Clone();
                if (!vertexMap.ContainsKey(triangle.Vertex2))
                    vertexMap[triangle.Vertex2] = triangle.Vertex2.Clone();
                if (!vertexMap.ContainsKey(triangle.Vertex3))
                    vertexMap[triangle.Vertex3] = triangle.Vertex3.Clone();

                // Create a new triangle with the cloned vertices
                w3Triangle newTriangle = new w3Triangle
                {
                    Vertex1 = vertexMap[triangle.Vertex1],
                    Vertex2 = vertexMap[triangle.Vertex2],
                    Vertex3 = vertexMap[triangle.Vertex3]
                };

                // Add the cloned vertices to the new geoset
                foreach (var vertex in vertexMap.Values)
                {
                    newGeoset.Vertices.Add(vertex);
                }

                // Add the new triangle to the new geoset
                newGeoset.Triangles.Add(newTriangle);

                // Add the new geoset to the model
                CurrentModel.Geosets.Add(newGeoset);

                // Remove the triangle from the original geoset
                geo.Triangles.Remove(triangle);
            }

            // Refresh UI or geoset-related data
            RefreshGeosetList();
            SetSaved(false);
            CurrentModel.RefreshGeosetAnimations();
        }

        private void SplitVertex(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count ==0) { MessageBox.Show("Select at least 1 vertex", "Invalid request"); return; }
            foreach (w3Vertex v in vertices)
            {
                w3Geoset geoset = GetGEosetOfVertex(v);
                if (geoset.Triangles.Count(x => x.Vertex1 == v || x.Vertex2 == v || x.Vertex3 == v) > 1)
                {
                    foreach (w3Triangle tr in geoset.Triangles)
                    {
                        if (tr.Vertex1 == v)
                        {
                            w3Vertex newV = v.Clone();
                            tr.Vertex1 = newV;
                            geoset.Vertices.Add(newV);
                        }
                        if (tr.Vertex2 == v)
                        {
                            w3Vertex newV = v.Clone();
                            tr.Vertex2 = newV;
                            geoset.Vertices.Add(newV);
                        }
                        if (tr.Vertex3 == v)
                        {
                            w3Vertex newV = v.Clone();
                            tr.Vertex3 = newV;
                            geoset.Vertices.Add(newV);
                        }
                    }
                    geoset.Vertices.Remove(v);
                }

            }
            SetSaved(false);
        }
        private w3Geoset GetGEosetOfVertex(w3Vertex v)
        {
            return CurrentModel.Geosets.First(x => x.Vertices.Contains(v));
        }

        private void DuplicateAlongAxis(object sender, RoutedEventArgs e)
        {
            DuplicateAlongAxis_Window dw = new DuplicateAlongAxis_Window();
            if (dw.DialogResult == true)
            {
                List<w3Vertex> vertices = new List<w3Vertex>();
                float distance = dw.Distance;
                int method = dw.method;
                int axis = dw.axis;
                int copies = dw.Copies;
                float currentDistance = 0;
                foreach (w3Vertex v in vertices)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        if (geo.Vertices.Contains(v))
                        {
                            if (method == 1)
                            {
                                for (int i = 0; i < copies; i++)
                                {
                                    currentDistance += distance;
                                    w3Vertex newVertex = v.Clone();
                                    if (axis == 0) v.Position.X += currentDistance;
                                    if (axis == 1) v.Position.X -= currentDistance;
                                    if (axis == 2) v.Position.Y += currentDistance;
                                    if (axis == 4) v.Position.Z += currentDistance;
                                    if (axis == 5) v.Position.Z -= currentDistance;



                                    geo.Vertices.Add(newVertex);
                                }
                            }
                            else
                            {

                            }

                        }
                        break;
                    }
                }

            }
        }

        private void CenterNodeAtSelection(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                w3Node node = GetSelectedNode();
                List<w3Vertex> vertices = GetSelectedVertices();
                if (vertices.Count > 0)
                {
                    Coordinate centroid = Calculator3D.GetCentroid(vertices);
                    node.PivotPoint.SetTo(centroid);
                }
                SetSaved(false);
            }

        }

        private void SpreadVertices(object sender, RoutedEventArgs e)
        {
            // Define a threshold for determining if vertices are "too close"
            const float overlapThreshold = 0.001f; // Adjust this as needed
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count <= 1)
            {
                MessageBox.Show("Select two or more vertices", "Invalid Request");
                return;
            }

            // Check if all selected vertices are overlapping
            bool allOverlapping = true;
            w3Vertex referenceVertex = vertices[0];

            foreach (w3Vertex vertex in vertices)
            {
                if (Calculator3D.DistanceBetween(referenceVertex, vertex) > overlapThreshold)
                {
                    allOverlapping = false;
                    break;
                }
            }

            if (allOverlapping)
            {

                // Spread vertices
                UserInput i = new UserInput();
                i.Title = "Enter value";
                i.ShowDialog();
                if (i.DialogResult == true)
                {

                    bool parsed = float.TryParse(i.Box.Text, out float val);
                    float spreadDistance = parsed ? val : 0.001f;
                    float angleIncrement = 360.0f / vertices.Count;
                    float currentAngle = 0.0f;

                    foreach (w3Vertex vertex in vertices)
                    {
                        float offsetX = (float)Math.Cos(currentAngle * Math.PI / 180) * spreadDistance;
                        float offsetY = (float)Math.Sin(currentAngle * Math.PI / 180) * spreadDistance;

                        vertex.Position.X += offsetX;
                        vertex.Position.Y += offsetY;

                        currentAngle += angleIncrement;
                    }
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Not all of the selected vertices are overlapping", "Invalid Request");
            }
        }

        // Helper method to calculate the distance between two positions
        private float GetDistance(Vector3 position1, Vector3 position2)
        {
            return (float)Math.Sqrt(Math.Pow(position1.X - position2.X, 2) +
                                    Math.Pow(position1.Y - position2.Y, 2) +
                                    Math.Pow(position1.Z - position2.Z, 2));
        }

        private void NullPivotPoint(object sender, RoutedEventArgs e)
        {
            GetSelectedNode().PivotPoint.Null();
            SetSaved(false);
        }

        private void NullPivotPoints(object sender, RoutedEventArgs e)
        {
            CurrentModel.Nodes.ForEach(x => x.PivotPoint.Null());
            SetSaved(false);
        }

        private void DuplicaetePivotPointToFirstChildren(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            w3Node selected = GetSelectedNode();
            foreach (var item in CurrentModel.Nodes)
            {
                if (item.parentId == selected.objectId)
                {
                    item.PivotPoint.SetTo(selected.PivotPoint);
                }
            }
            SetSaved(false);
        }

        private void DuplicaetePivotPointToAllChildren(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { return; }
            w3Node selected = GetSelectedNode();
            DuplicatePivotPointRecursively(selected);
            SetSaved(false);
        }

        private void DuplicatePivotPointRecursively(w3Node parent)
        {
            foreach (var child in CurrentModel.Nodes.Where(node => node.parentId == parent.objectId))
            {
                // Copy the pivot point to the child
                child.PivotPoint.SetTo(parent.PivotPoint);

                // Recursively process the child's children
                DuplicatePivotPointRecursively(child);
                SetSaved(false);
            }
        }

        private void FitUV(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 2)
            {
                MessageBox.Show("Select two triangles that form a quad", "Invalid request"); return;
            }
            w3Geoset geoset = GetGeosetOfTriangles(triangles);
            if (geoset != null)
            {
                if (Calculator3D.TrianglesAreQuad(triangles[0], triangles[1]))
                {
                    Calculator3D.FitUV(triangles[0], triangles[1]);
                    SetSaved(false);
                }
                else
                {
                    MessageBox.Show("These triangles do not form a quad", "Invalid request"); return;
                }
            }
            else
            {
                MessageBox.Show("These triangles don't belong to the same geoset", "Invalid request"); return;
            }
        }
        private w3Geoset GetGeosetOfTriangle(w3Triangle tr)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Triangles.Contains(tr)) { return geo; }
            }
            return null;
        }

        private w3Geoset GetGeosetOfTriangles(List<w3Triangle> triangles)
        {
            w3Geoset cgeo = null;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo.Triangles.Contains(triangles[0]))
                {
                    cgeo = geo; break;
                }
            }
            if (cgeo == null) { return null; }
            if (cgeo != null && triangles.Count == 1) { return cgeo; }
            for (int i = 1; i < triangles.Count; i++)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    if (geo.Triangles.Contains(triangles[i]) && cgeo != geo) { return null; }
                }
            }

            return cgeo;
        }

        private void RotateUV90(object sender, RoutedEventArgs e)
        {
            // Get the selected vertices whose UVs we want to rotate
            List<w3Vertex> vertices = GetSelectedVertices();

            // Step 1: Calculate the centroid of the UV coordinates
            float centroidU = 0;
            float centroidV = 0;

            foreach (w3Vertex v in vertices)
            {
                centroidU += v.Texture_Position.U;
                centroidV += v.Texture_Position.V;
            }

            // Calculate the average to find the centroid
            centroidU /= vertices.Count;
            centroidV /= vertices.Count;

            // Step 2: Rotate each vertex's UV around the centroid by 90 degrees (clockwise)
            foreach (w3Vertex v in vertices)
            {
                float currentU = v.Texture_Position.U;
                float currentV = v.Texture_Position.V;

                // Applying the 90-degree clockwise rotation
                // The 90 degree clockwise rotation around the centroid is performed like this:
                v.Texture_Position.U = centroidU - (centroidV - currentV);
                v.Texture_Position.V = centroidV + (centroidU - currentU);
            }
            SetSaved(false);
        }


        private void RotateUV90m(object sender, RoutedEventArgs e)
        {
            // Get the selected vertices whose UVs we want to rotate
            List<w3Vertex> vertices = GetSelectedVertices();

            // Step 1: Calculate the centroid of the UV coordinates
            float centroidU = 0;
            float centroidV = 0;

            foreach (w3Vertex v in vertices)
            {
                centroidU += v.Texture_Position.U;
                centroidV += v.Texture_Position.V;
            }

            // Calculate the average to find the centroid
            centroidU /= vertices.Count;
            centroidV /= vertices.Count;

            // Step 2: Rotate each vertex's UV around the centroid by -90 degrees (counterclockwise)
            foreach (w3Vertex v in vertices)
            {
                float currentU = v.Texture_Position.U;
                float currentV = v.Texture_Position.V;

                // Applying the 90-degree counterclockwise rotation
                // The -90 degree rotation around the centroid is performed like this:
                v.Texture_Position.U = centroidU + (centroidV - currentV);
                v.Texture_Position.V = centroidV - (centroidU - currentU);
            }
            SetSaved(false);
        }

        private void FlipUs(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { return; }
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position.U = -v.Texture_Position.U;
            }
            SetSaved(false);
        }

        private void FlipVs(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { return; }
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position.V = -v.Texture_Position.V;
            }
            SetSaved(false);
        }

        private void SwapUsandVs(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0) { return; }
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position.Swap();
            }
            SetSaved(false);
        }

        private void CloseModel(object sender, RoutedEventArgs e)
        {
            CurrentModel = new();

            ClearHistory(null, null);
            RefreshNodesList();
            RefreshGeosetList();
            RefreshSequencesList();
            List_Rigging_AttachedTo.Items.Clear();
            List_Rigging_Bones.Items.Clear();
            ModifiedGeometryForTrack.Clear();
            // Slider_Sequence.Minimum = 0; Slider_Sequence.Maximum = 0;
            Timeline.Children.Clear();
            InputCurrentTrack.Text = "-1";

            SaveLocation = "";
            SetSaved(false);

        }

        private void EnableDisableCustomPP(object sender, RoutedEventArgs e)
        {
            Panel_CustomPP.IsEnabled = Radio_Custom.IsChecked == true;
        }
        private Vector3 CopiedTranslations = new(0, 0, 0);
        private Vector3 CopiedRotations = new(0, 0, 0);
        private Vector3 CopiedScalings = new(0, 0, 0);
        private void CopyTranslations(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(inputX.Text, out float x);
            bool parsed2 = float.TryParse(inputY.Text, out float y);
            bool parsed3 = float.TryParse(inputZ.Text, out float z);
            CopiedTranslations = new(0, 0, 0);
            if (parsed1) CopiedTranslations.X = x;
            if (parsed2) CopiedTranslations.Y = y;
            if (parsed3) CopiedTranslations.Z = z;

        }

        private void PasteTranskations(object sender, RoutedEventArgs e)
        {
            inputX.Text = CopiedTranslations.X.ToString();
            inputY.Text = CopiedTranslations.Y.ToString();
            inputZ.Text = CopiedTranslations.Z.ToString();
        }

        private void CopyRotations(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(inputXr.Text, out float x);
            bool parsed2 = float.TryParse(inputYr.Text, out float y);
            bool parsed3 = float.TryParse(inputZr.Text, out float z);
            CopiedRotations = new(0, 0, 0);
            if (parsed1) CopiedRotations.X = x;
            if (parsed2) CopiedRotations.Y = y;
            if (parsed3) CopiedRotations.Z = z;
        }

        private void PasteRotations(object sender, RoutedEventArgs e)
        {
            inputXr.Text = CopiedRotations.X.ToString();
            inputYr.Text = CopiedRotations.Y.ToString();
            inputZr.Text = CopiedRotations.Z.ToString();
        }

        private void copyScalings(object sender, RoutedEventArgs e)
        {
            bool parsed1 = float.TryParse(InputXs.Text, out float x);
            bool parsed2 = float.TryParse(InputYs.Text, out float y);
            bool parsed3 = float.TryParse(InputZs.Text, out float z);
            CopiedScalings = new(0, 0, 0);
            if (parsed1) CopiedScalings.X = x;
            if (parsed2) CopiedScalings.Y = y;
            if (parsed3) CopiedScalings.Z = z;
        }

        private void pasteScalings(object sender, RoutedEventArgs e)
        {
            InputXs.Text = CopiedScalings.X.ToString();
            InputYs.Text = CopiedScalings.Y.ToString();
            InputZs.Text = CopiedScalings.Z.ToString();
        }

        private void NewSequence(object sender, MouseButtonEventArgs e)
        {

            if (e.RightButton == MouseButtonState.Pressed)
            {
                MultipleSequence ms = new(CurrentModel.Sequences);
                ms.ShowDialog();
                if (ms.DialogResult == true) RefreshSequencesList(); SetSaved(false);
                return;
            }




        }

        private void SWapTwoSElectedVerticesUV(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count != 2)
            {
                MessageBox.Show("Select two vertices", "Invalid requst"); return;
            }
            Coordinate2D cd = vertices[0].Texture_Position.Clone();
            vertices[0].Texture_Position.SetTo(vertices[1].Texture_Position);
            vertices[1].Texture_Position.SetTo(cd);
        }

        private void NewSequenceSingle(object sender, RoutedEventArgs e)
        {
            sequenceWindow s = new sequenceWindow(CurrentModel.Sequences);
            s.ShowDialog();
            if (s.DialogResult == true)
            {
                string looping = s.AcceptedSequence.Looping ? "Looping" : "Nonlooping";
                ListSequences.Items.Add(new ListBoxItem() { Content = $"{s.AcceptedSequence.Name} [{s.AcceptedSequence.From} - {s.AcceptedSequence.To}] [{looping}]" });
                CurrentModel.Sequences.Add(s.AcceptedSequence);
                CurrentModel.Sequences = CurrentModel.Sequences.OrderBy(x => x.From).ToList();

                OrderSEquencesByFrom();
                SetSaved(false);
            }
        }

        private void SaveGEosetsAsNewModel(object sender, RoutedEventArgs e)
        {
            // Get the selected geosets
            List<w3Geoset> geos = GetSelectedGeosets();
            if (geos.Count == 0)
            {
                MessageBox.Show("No geosets are selected", "Invalid Request");
                return;
            }

            // Prompt the user to select a file to save as .mdl
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Warcraft 3 Model File (*.mdl)|*.mdl",
                Title = "Save As New Model"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string selectedFilepath = saveFileDialog.FileName;

                // Create a new model
                w3Model model = new w3Model();

                // Add the selected geosets
                model.Geosets.AddRange(geos);

                // Add all materials used by the selected geosets
                var usedMaterials = CurrentModel.Materials
                    .Where(material => geos.Any(geo => geo.Material_ID == material.ID))
                    .ToList();
                model.Materials.AddRange(usedMaterials);

                // Add all textures used by the layers in the selected materials
                var usedTextures = CurrentModel.Textures
                    .Where(texture => usedMaterials
                        .SelectMany(material => material.Layers)
                        .Any(layer => layer.Diffuse_Texure_ID.StaticValue[0] == texture.ID))
                    .ToList();
                model.Textures.AddRange(usedTextures);

                // Save the model to the selected file
                try
                {
                    File.WriteAllText(selectedFilepath, model.ToMDL());

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save the model. Error: {ex.Message}", "Error");
                }
            }
        }

        private void BooleanCombine(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void BooleanSubtract1(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void BooleanSubtract2(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void BooleanAdd(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void BooleanExclude(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void BooleanSlice(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void DrawInTriangle(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            w3Geoset geoset = GetGeosetOfTriangles(triangles);
            //unfinished
            drawintriangle_window dw = new drawintriangle_window(triangles[0], geoset);
            dw.ShowDialog();
            if (dw.DialogResult == true)
            {
                geoset.RecalculateEdges();
                SetSaved(false);
            }

        }


        private void NewLight(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New light: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is alrady a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new w3Light();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Light));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Light));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }

                SetSaved(false);
            }

        }

        private void NewEmitter1(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New emitter1: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is alrady a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new Particle_Emitter_1();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Emitter1));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Emitter1));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }

                SetSaved(false);
            }
        }

        private void NewEmitter2(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New light: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is already a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new Particle_Emitter_2();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Emitter2));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Emitter2));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
                SetSaved(false);
            }
        }

        private void NewRibbon(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New ribbon: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is alrady a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new Ribbon_Emitter();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Ribbon));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Ribbon));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
                SetSaved(false);
            }
        }

        private void NewEvent(object sender, RoutedEventArgs e)
        {
            UserInput i = new UserInput();
            i.Title = "New light: name";
            i.ShowDialog();


            if (i.DialogResult == true)
            {
                string name2 = i.Box.Text.Trim();
                if (CurrentModel.Nodes.Any(x => x.Name == name2))
                {
                    MessageBox.Show("There is already a node with this name", "Changes not made"); return;

                }
                else
                {
                    w3Node node = new w3Node();

                    node.Name = name2;
                    node.Data = new Event_Object();
                    node.objectId = IDCounter.Next();
                    CurrentModel.Nodes.Add(node);

                    if (ClickedEmpty)
                    {
                        node.parentId = -1;
                        List_Nodes.Items.Add(NewTreeItem(name2, NodeType.Event));



                    }
                    else
                    {
                        TreeViewItem item = List_Nodes.SelectedItem as TreeViewItem;
                        StackPanel s = item.Header as StackPanel;
                        TextBlock t = s.Children[1] as TextBlock;
                        string ClickedName = t.Text;

                        item.Items.Add(NewTreeItem(name2, NodeType.Event));
                        int parent = CurrentModel.Nodes.First(x => x.Name == ClickedName).objectId;
                        node.parentId = parent;
                    }
                }
                SetSaved(false);
            }
        }

        private void LoadGeosetsFromModel(object sender, RoutedEventArgs e)
        {

            string filename = GetFile();
            if (filename.Length  == 0) { return; }
            if (System.IO.Path.GetExtension(filename).ToLower() == ".mdx")
            {
                ConvertMDXToMDLTemp(filename);
                filename = AppHelper.TempMDLLocation;
            }
            w3Model TemporaryModel = new w3Model();
            List<Token> tokens = Parser_MDL.Tokenize(filename);
            List<TemporaryObject> temporaryObjects = Parser_MDL.SplitCollectObjects(tokens);
            TemporaryModel = Parser_MDL.Parse(temporaryObjects);



            TemporaryModel.FinalizeComponents();
            if (CurrentModel.Textures.Count > 0 && CurrentModel.Materials.Count > 0)
            {
                foreach (w3Geoset geo in TemporaryModel.Geosets)
                {
                    geo.Material_ID = CurrentModel.Materials[0].ID;

                }
            }
            CurrentModel.Geosets.AddRange(TemporaryModel.Geosets);
            CurrentModel.RefreshGeosetAnimations();
            CurrentModel.RefreshEdges();
            RefreshGeosetList();
            TemporaryModel = null;
        }

        private void SimpleFloat(object sender, RoutedEventArgs e)
        {
            // change
            // unfinished
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            if (ListSequences.SelectedItem == null)
            {
                MessageBox.Show("Select a sequence", "Invalid request"); return;
            }
            w3Node node = GetSelectedNode();
            w3Sequence sequence = GetSelectedSequence();
            if (node != null)
            {

            }
        }

        private void SimpleRotate(object sender, RoutedEventArgs e)
        {
            // change
            // unfinished
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            if (ListSequences.SelectedItem == null)
            {
                MessageBox.Show("Select a sequence", "Invalid request"); return;
            }
            w3Node node = GetSelectedNode();
            w3Sequence sequence = GetSelectedSequence();
            if (node != null)
            {
                simple_anim sa = new simple_anim(float.MinValue, float.MaxValue);
                sa.ShowDialog();
                if (sa.DialogResult == true)
                {
                    SetSaved(false);
                }
            }
        }

        private void SimplePulsate(object sender, RoutedEventArgs e)
        {
            // change
            // unfinished
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("Select a node", "Invalid request"); return;
            }
            if (ListSequences.SelectedItem == null)
            {
                MessageBox.Show("Select a sequence", "Invalid request"); return;
            }
            w3Node node = GetSelectedNode();
            w3Sequence sequence = GetSelectedSequence();
            if (node != null)
            {
                SetSaved(false);
            }
        }
        private w3Triangle CopriedFacingTriangle;

        private void CopyFacingAngle(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            else
            {
                CopriedFacingTriangle = triangles[0];
            }
        }

        private void PasteFacingAngle(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            else
            {
                if (CopriedFacingTriangle == triangles[0])
                {
                    MessageBox.Show("Copied is same as pasted", "Invalid request"); return;
                }
                Calculator3D.PasteFacingAngle(CopriedFacingTriangle, triangles[0]);
                SetSaved(false);
            }
        }

        private void mItem_Aim_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GrowSelection(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex vertex in geo.Vertices)
                {
                    if (vertex.isSelected)
                    {
                        w3Vertex closest =
                              geo.Vertices
        .Where(v => v != vertex)
        .OrderBy(v => Math.Sqrt(
            Math.Pow(v.Position.X - vertex.Position.X, 2) +
            Math.Pow(v.Position.Y - vertex.Position.Y, 2) +
            Math.Pow(v.Position.Z - vertex.Position.Z, 2)))
        .FirstOrDefault();
                        if (closest != null) { closest.isSelected = true; }
                    }
                }
            }
        }

        private void ShrinkSelection(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {

                // Calculate center point
                var center = new Coordinate
                {
                    X = geo.Vertices.Average(v => v.Position.X),
                    Y = geo.Vertices.Average(v => v.Position.Y),
                    Z = geo.Vertices.Average(v => v.Position.Z)
                };

                // Find maximum distance from center
                float maxDistance = geo.Vertices.Max(v =>
                    MathF.Sqrt(
                        MathF.Pow(v.Position.X - center.X, 2) +
                        MathF.Pow(v.Position.Y - center.Y, 2) +
                        MathF.Pow(v.Position.Z - center.Z, 2)
                    ));

                // Unselect vertices at the maximum distance
                foreach (var vertex in geo.Vertices)
                {
                    float distance = MathF.Sqrt(
                        MathF.Pow(vertex.Position.X - center.X, 2) +
                        MathF.Pow(vertex.Position.Y - center.Y, 2) +
                        MathF.Pow(vertex.Position.Z - center.Z, 2)
                    );

                    if (distance == maxDistance)
                        vertex.isSelected = false;
                }
            }
        }

        private void CreateSkeleton(object sender, RoutedEventArgs e)
        {

            Create_Skeleton cs = new Create_Skeleton(CurrentModel); cs.ShowDialog();
            if (cs.DialogResult == true)
            {

                RefreshNodesList();
                SetSaved(false);
            }

        }

        private void CenterXYZT(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Nodes)
            {
                if (CurrentlySelectedNode != null) CurrentlySelectedNode.PivotPoint.Z = 0;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                Coordinate newCentroid = new Coordinate(0, 0, 0);
                Calculator3D.CenterVertices(vertices, newCentroid);
                SetSaved(false);
            }
        }

        private void SelectSimilarSizes(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> selectedTriangles = GetSelectedTriangles();
                if (selectedTriangles.Count > 0)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Triangle t in geo.Triangles)
                        {
                            if (selectedTriangles.Contains(t) == false)
                            {
                                foreach (w3Triangle selectedTriangle in selectedTriangles)
                                {
                                    if (Calculator3D.TrianglesHaveSameArea(selectedTriangle, t))
                                    {
                                        t.isSelected = true; break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Select at least one triangle", "Invalid request"); return;
                }
            }
        }

        private void SelectSimilarFacing(object sender, RoutedEventArgs e)
        {
            if (editMode_current != EditMode.Triangles) { return; }
            List<w3Triangle> selectedTriangles = GetSelectedTriangles();
            if (selectedTriangles.Count > 0)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Triangle t in geo.Triangles)
                    {
                        if (selectedTriangles.Contains(t) == false)
                        {
                            foreach (w3Triangle selectedTriangle in selectedTriangles)
                            {
                                if (Calculator3D.TrianglesHaveSimilarFacingAngle(selectedTriangle, t))
                                {
                                    t.isSelected = true; break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Select at least one triangle", "Invalid request"); return;
            }
        }

        private void SelectDifferentFacing(object sender, RoutedEventArgs e)
        {
            if (editMode_current != EditMode.Triangles) { return; }
            List<w3Triangle> selectedTriangles = GetSelectedTriangles();
            if (selectedTriangles.Count > 0)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Triangle t in geo.Triangles)
                    {
                        if (selectedTriangles.Contains(t) == false)
                        {
                            foreach (w3Triangle selectedTriangle in selectedTriangles)
                            {
                                if (!Calculator3D.TrianglesHaveSimilarFacingAngle(selectedTriangle, t))
                                {
                                    t.isSelected = true; break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Select at least one triangle", "Invalid request"); return;
            }
        }

        private void SelectDifferentSizes(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> selectedTriangles = GetSelectedTriangles();
                if (selectedTriangles.Count > 0)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Triangle t in geo.Triangles)
                        {
                            if (selectedTriangles.Contains(t) == false)
                            {
                                foreach (w3Triangle selectedTriangle in selectedTriangles)
                                {
                                    if (!Calculator3D.TrianglesHaveSameArea(selectedTriangle, t))
                                    {
                                        t.isSelected = true; break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Select at least one triangle", "Invalid request"); return;
                }
            }
        }

        private void SelectaGeosetsWithSimilarMaterial(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void SelectVerticesAttachedToBone(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null) { MessageBox.Show("Select a node", "Invalid request"); return; }
            w3Node node = GetSelectedNode();
            if (node.Data is Bone)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        if (v.AttachedTo.Contains(node.objectId))
                        {
                            v.isSelected = true;
                            if (editMode_current == EditMode.Triangles)
                            {
                                foreach (w3Triangle triangle in geo.Triangles)
                                {
                                    if (triangle.Vertex1 == v || triangle.Vertex2 == v || triangle.Vertex3 == v) { triangle.isSelected = true; }
                                }
                            }
                            if (editMode_current == EditMode.Edges)
                            {
                                foreach (w3Edge edge in geo.Edges)
                                {
                                    if (edge.Vertex1 == v || edge.Vertex2 == v) { edge.isSelected = true; }
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("This is not a bone", "Invalid request"); return;
            }


        }
        private List<int> RiggingCopy;
        private void CopyRigging(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count == 0)
            {
                MessageBox.Show("Select vertices", "Invalid request"); return;
            }
            List<int> first;
            foreach (w3Vertex v in vertices)
            {
                v.AttachedTo.OrderBy(x => x);
            }
            first = vertices[0].AttachedTo;
            if (vertices.Count == 1)
            {
                RiggingCopy = first;
            }
            else
            {
                bool same = true;
                for (int i = 1; i < vertices.Count; i++)
                {

                    if (first.SequenceEqual(vertices[i].AttachedTo) == false)
                    {
                        same = false; break;
                    }

                }
                if (same)
                {
                    RiggingCopy = first;
                }
            }
        }


        private void ClearPasteRigging(object sender, RoutedEventArgs e)
        {
            if (RiggingCopy == null)
            {
                MessageBox.Show("Nothing was copied", "Invalid request"); return;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();

                foreach (w3Vertex v in vertices)
                {
                    v.AttachedTo.Clear();
                    v.AttachedTo.AddRange(RiggingCopy);
                }
                RefreshRiggingListWith(RiggingCopy);
                SetSaved(false);
            }
        }
        private void RefreshRiggingListWith(List<int> list)
        {
            List_Rigging_AttachedTo.Items.Clear();
            foreach (int i in list)
            {
                List_Rigging_AttachedTo.Items.Add(new ListBoxItem() { Content = i.ToString() });
            }

        }
        private void AddPasteRigging(object sender, RoutedEventArgs e)
        {
            if (List_Rigging_AttachedTo.Items.Count == 4)
            {
                MessageBox.Show("No more than 4 attached bones per vertex!", "Precaution"); return;
            }
            if (RiggingCopy == null)
            {
                MessageBox.Show("Nothing was copied", "Invalid request"); return;
            }
            else
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                foreach (w3Vertex v in vertices)
                {
                    v.AttachedTo.Clear();
                    foreach (int el in RiggingCopy)
                    {
                        if (v.AttachedTo.Contains(el) == false) { v.AttachedTo.Add(el); }
                    }

                }
                RefreshRiggingListWith(vertices[0].AttachedTo);
                SetSaved(false);
            }
        }

        private void SelectNodeInViewport(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem != null)
            {
                w3Node snode = GetSelectedNode();
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    node.isSelected = false;

                }
                snode.isSelected = true;
            }
        }

        private void SelectGeosetsInViewport(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 0)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();
                foreach (w3Geoset geo in geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        v.isSelected = true;
                        if (editMode_current == EditMode.Edges)
                        {
                            foreach (w3Edge edge in geo.Edges)
                            {
                                if (edge.Vertex1 == v || edge.Vertex2 == v)
                                {
                                    edge.isSelected = true;
                                }
                            }
                        }

                        if (editMode_current == EditMode.Triangles)
                        {
                            foreach (w3Triangle triangle in geo.Triangles)
                            {
                                if (triangle.Vertex1 == v || triangle.Vertex3 == v || triangle.Vertex3 == v)
                                {
                                    triangle.isSelected = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OrderSEquencesByFrom()
        {
            CurrentModel.Sequences.OrderBy(x => x.From);
            RefreshSequencesList();
            SetSaved(false);
        }

        private void OrderGeosetsByV_A(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderBy(x => x.Vertices.Count);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsByV_D(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderByDescending(x => x.Vertices.Count);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsByT_A(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderBy(x => x.Triangles.Count);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsByT_D(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderByDescending(x => x.Triangles.Count);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsByid_A(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderBy(x => x.ID);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsByid_D(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderByDescending(x => x.ID);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsBymid_A(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderBy(x => x.Material_ID);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void OrderGeosetsBymid_D(object sender, RoutedEventArgs e)
        {
            CurrentModel.Geosets.OrderByDescending(x => x.Material_ID);
            RefreshGeosetList();
            SetSaved(false);
        }

        private void S_Next_G_V(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {

                foreach (w3Geoset geoset in geosets)
                {
                    if (geoset.Vertices.Count < 2) { continue; }
                    if (geoset.Vertices.Any(x => x.isSelected))
                    {
                        int index = geoset.Vertices.FindIndex(x => x.isSelected);
                        geoset.Vertices[index].isSelected = false;
                        if (index == geoset.Vertices.Count - 1)
                        {
                            index = 0;
                        }
                        else { index++; }

                        geoset.Vertices[index].isSelected = true;
                    }


                    else
                    {
                        geoset.Vertices[0].isSelected = true;
                    }
                }
                SetSaved(false);
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void S_Prev_G_V(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {

                foreach (w3Geoset geoset in geosets)
                {
                    if (geoset.Vertices.Count < 2) { continue; }
                    if (geoset.Vertices.Any(x => x.isSelected))
                    {
                        int index = geoset.Vertices.FindIndex(x => x.isSelected);
                        geoset.Vertices[index].isSelected = false;
                        if (index == 0)
                        {
                            index = geoset.Vertices.Count - 1;
                        }
                        else { index--; }

                        geoset.Vertices[index].isSelected = true;
                    }


                    else
                    {
                        geoset.Vertices[0].isSelected = true;
                    }
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void S_Next_G_T(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {

                foreach (w3Geoset geoset in geosets)
                {
                    foreach (w3Vertex v in geoset.Vertices) { v.isSelected = false; }
                    if (geoset.Triangles.Count < 2) { continue; }
                    if (geoset.Triangles.Any(x => x.isSelected))
                    {
                        int index = geoset.Triangles.FindIndex(x => x.isSelected);
                        geoset.Triangles[index].isSelected = false;
                        geoset.Triangles[0].SelectVertices(false);
                        if (index == geoset.Triangles.Count - 1)
                        {
                            index = 0;
                        }
                        else { index++; }

                        geoset.Triangles[index].isSelected = true;
                        geoset.Triangles[0].SelectVertices();
                    }


                    else
                    {
                        geoset.Triangles[0].isSelected = true;
                        geoset.Triangles[0].SelectVertices();
                    }
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void S_Prev_G_T(object sender, RoutedEventArgs e)
        {

            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {
                foreach (w3Geoset geoset in geosets)
                {
                    if (geoset.Triangles.Count < 2) { continue; }
                    if (geoset.Triangles.Any(x => x.isSelected))
                    {
                        foreach (w3Vertex v in geoset.Vertices) { v.isSelected = false; }
                        int index = geoset.Triangles.FindIndex(x => x.isSelected);
                        geoset.Triangles[index].isSelected = false;
                        geoset.Triangles[index].SelectVertices(false);
                        if (index == 0)
                        {
                            index = geoset.Triangles.Count - 1;
                        }
                        else { index--; }

                        geoset.Triangles[index].isSelected = true;
                        geoset.Triangles[index].SelectVertices();

                    }
                    else
                    {
                        geoset.Triangles[0].isSelected = true;
                        geoset.Triangles[0].SelectVertices();
                    }
                    SetSaved(false);

                }
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void S_Next_G_E(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {

                foreach (w3Geoset geoset in geosets)
                {
                    foreach (w3Vertex v in geoset.Vertices) { v.isSelected = false; }
                    if (geoset.Edges.Count < 2) { continue; }
                    if (geoset.Edges.Any(x => x.isSelected))
                    {

                        int index = geoset.Edges.FindIndex(x => x.isSelected);
                        geoset.Edges[index].isSelected = false;
                        geoset.Edges[index].SelectVertices(false);
                        if (index == geoset.Edges.Count - 1)
                        {
                            index = 0;
                        }
                        else { index++; }

                        geoset.Edges[index].isSelected = true;
                        geoset.Edges[index].SelectVertices();
                    }


                    else
                    {
                        geoset.Edges[0].isSelected = true;
                        geoset.Edges[0].SelectVertices();
                    }
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void S_Prev_G_E(object sender, RoutedEventArgs e)
        {
            List<w3Geoset> geosets = GetSelectedGeosets();
            if (geosets.Count > 0)
            {

                foreach (w3Geoset geoset in geosets)
                {
                    foreach (w3Vertex v in geoset.Vertices) { v.isSelected = false; }
                    if (geoset.Edges.Count < 2) { continue; }
                    if (geoset.Edges.Any(x => x.isSelected))
                    {
                        int index = geoset.Edges.FindIndex(x => x.isSelected);
                        geoset.Edges[index].isSelected = false;
                        geoset.Edges[index].SelectVertices(false);
                        if (index == 0)
                        {
                            index = geoset.Edges.Count - 1;
                        }
                        else { index--; }

                        geoset.Edges[index].isSelected = true;
                        geoset.Edges[index].SelectVertices();
                    }


                    else
                    {
                        geoset.Edges[0].isSelected = true;
                        geoset.Edges[0].Vertex1.isSelected = true;
                        geoset.Edges[0].Vertex2.isSelected = true;
                    }
                    SetSaved(false);
                }
            }
            else
            {
                MessageBox.Show("Select at least 1 geoset", "Invalid request");
            }
        }

        private void SplitBoneByTriangles(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode();
                if (node.Data is Bone == false)
                {
                    MessageBox.Show("select a bone", "Invalid request"); return;
                }
                else
                {

                    List<w3Triangle> isolated_triangles = GetIsolatedTriangles(node.objectId);
                    if (isolated_triangles.Count <= 1) { MessageBox.Show("Not enough isolated triangles", "Invalid request"); return; }
                    foreach (w3Triangle triangle in isolated_triangles)
                    {
                        w3Node new_node = new w3Node();
                        new_node.parentId = node.parentId;
                        new_node.objectId = IDCounter.Next();
                        triangle.Vertex1.AttachedTo.Remove(node.objectId);
                        triangle.Vertex1.AttachedTo.Add(new_node.objectId);
                        triangle.Vertex2.AttachedTo.Remove(node.objectId);
                        triangle.Vertex2.AttachedTo.Add(new_node.objectId);
                        triangle.Vertex3.AttachedTo.Remove(node.objectId);
                        triangle.Vertex3.AttachedTo.Add(new_node.objectId);
                        CurrentModel.Nodes.Add(new_node);
                    }
                    CurrentModel.Nodes.Remove(node);
                    RefreshNodesList();
                    SetSaved(false);
                }
            }
        }

        private List<w3Triangle> GetIsolatedTriangles(int boneID)
        {
            List<w3Triangle> isolatedTriangles = new List<w3Triangle>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                // Create a dictionary to count vertex occurrences
                Dictionary<w3Vertex, int> vertexUsage = new Dictionary<w3Vertex, int>();

                // Count vertex usage across all triangles
                foreach (var triangle in geo.Triangles)
                {
                    foreach (var vertex in new[] { triangle.Vertex1, triangle.Vertex2, triangle.Vertex3 })
                    {
                        if (!vertexUsage.ContainsKey(vertex))
                            vertexUsage[vertex] = 0;
                        vertexUsage[vertex]++;
                    }
                }

                // Find triangles where all vertices are used only once
                var isolated = geo.Triangles.Where(t =>
                    vertexUsage[t.Vertex1] == 1 &&
                    vertexUsage[t.Vertex2] == 1 &&
                    vertexUsage[t.Vertex3] == 1 &&
                    t.Vertex1.AttachedTo.Contains(boneID) &&
                    t.Vertex2.AttachedTo.Contains(boneID) &&
                    t.Vertex2.AttachedTo.Contains(boneID)

                    );


                isolatedTriangles.AddRange(isolated);
                SetSaved(false);
            }
            return isolatedTriangles;
        }


        private void NegateNodePPX(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode(); node.PivotPoint.X = -node.PivotPoint.X;
            }
        }
        private void NegateNodePPY(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode(); node.PivotPoint.Y = -node.PivotPoint.Y;
            }
        }

        private void NegateNodePPZ(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode(); node.PivotPoint.Z = -node.PivotPoint.Z;
                SetSaved(false);
            }
        }

        private void AlphabetizeAllNodes(object sender, RoutedEventArgs e)
        {
            CurrentModel.Nodes.OrderBy(x => x.Name);
            RefreshNodesList();
            SetSaved(false);
        }

        private void AlphabetizeAllChildren(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode();

                CurrentModel.Nodes.Where(x => x.parentId == node.objectId).OrderBy(x => x.Name);
                SetSaved(false);
            }
        }

        private void MergeBones(object sender, RoutedEventArgs e)
        {
            if (List_Nodes.SelectedItem == null)
            {
                MessageBox.Show("select a bone", "Invalid request"); return;
            }
            else
            {
                w3Node node = GetSelectedNode();
                if (node.Data is Bone == false) { MessageBox.Show("select a bone", "Invalid request"); return; }
                if (CurrentModel.Nodes.Count(x => x.Data is Bone) < 2)
                {
                    MessageBox.Show("At least 2 bones must be present", "Invalid request"); return;
                }
                List<w3Node> nodes = CurrentModel.Nodes.Where(x => x.Data is Bone && x.parentId == node.parentId && x.Name != node.Name).ToList();
                List<string> names = nodes.Select(x => x.Name).ToList();
                if (nodes.Count == 0)
                {
                    MessageBox.Show("No similar bones", "Invalid request"); return;
                }

                Selector s = new Selector(names);
                if (s.DialogResult == true)
                {
                    string selected = (s.box.SelectedItem as ListBoxItem).Content.ToString();
                    w3Node selectedNode = nodes.First(x => x.Name == selected);
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Vertex v in geo.Vertices)
                        {
                            foreach (int id in v.AttachedTo)
                            {
                                if (id == selectedNode.objectId)
                                {
                                    if (v.AttachedTo.Contains(node.objectId) == false)
                                    {
                                        v.AttachedTo.Add(node.objectId);
                                    }
                                    v.AttachedTo.Remove(selectedNode.objectId);
                                }
                            }
                        }
                    }
                    SetSaved(false);
                }
            }
        }

        private void C045_23(object sender, RoutedEventArgs e)
        {
            foreach (w3Geoset_Animation ga in CurrentModel.Geoset_Animations)
            {
                foreach (w3Sequence seq in CurrentModel.Sequences)
                {
                    if (!ga.Alpha.Keyframes.Any(x => x.Track == seq.From))
                    {
                        w3Keyframe kf = new w3Keyframe();
                        kf.Track = seq.From;
                        kf.Data = [100];
                    }
                }
                ga.Alpha.Keyframes.OrderBy(x => x.Track);
            }
        }
        private void Unrig()
        {
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    v.isRigged = false;
                }
            }
        }
        private void SelectedBoneInRigging(object sender, SelectionChangedEventArgs e)
        {
            if (List_Rigging_Bones.SelectedItem != null)
            {
                w3Node Selected = GetSelectedRiggingBone();
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        v.isRigged = v.AttachedTo.Contains(Selected.objectId);
                    }
                }
            }
            else
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Vertex v in geo.Vertices)
                    {
                        v.isRigged = false;
                    }
                }
            }
        }
        public float X, Y, Z;
        private bool DrawPaused = false;

        private void SelectNextOverlappingTraingle(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            bool break_ = false;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangle != triangles[0])
                    {
                        if (OverlapCalculator.AreTrianglesOverlapping(triangles[0], triangle))
                        {
                            triangles[0].isSelected = false;
                            triangle.isSelected = true;
                            break_ = true; break;
                        }
                    }

                }
            }

        }

        private void SelectAllBiggerTriangles(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangles.Contains(triangle)) { continue; }
                    float area = Calculator3D.GetTriangleArea(triangle);
                    if (area >= TriangleArea) { triangle.isSelected = true; }
                }
            }
        }

        private void SelectAllSmallerTriangles(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangles.Contains(triangle)) { continue; }
                    float area = Calculator3D.GetTriangleArea(triangle);
                    if (area < TriangleArea) { triangle.isSelected = true; }
                }
            }
        }

        private void SelectAllBiggerTriangles2(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            w3Geoset owner = GetGeosetOfTriangle(triangles[0]);
            foreach (w3Triangle triangle in owner.Triangles)
            {
                if (triangles.Contains(triangle)) { continue; }
                float area = Calculator3D.GetTriangleArea(triangle);
                if (area >= TriangleArea) { triangle.isSelected = true; }
            }
        }

        private void SelectAllSmallerTriangles2(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            w3Geoset owner = GetGeosetOfTriangle(triangles[0]);
            foreach (w3Triangle triangle in owner.Triangles)
            {
                if (triangles.Contains(triangle)) { continue; }
                float area = Calculator3D.GetTriangleArea(triangle);
                if (area < TriangleArea) { triangle.isSelected = true; }
            }
        }

        private void SelectAllBiggerTriangles3(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            w3Geoset owner = GetGeosetOfTriangle(triangles[0]);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo == owner) { continue; }
                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangles.Contains(triangle)) { continue; }
                    float area = Calculator3D.GetTriangleArea(triangle);
                    if (area >= TriangleArea) { triangle.isSelected = true; }
                }
            }
        }

        private void SelectAllSmallerTriangles3(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count != 1)
            {
                MessageBox.Show("Select a single triangle", "Invalid request"); return;
            }
            float TriangleArea = Calculator3D.GetTriangleArea(triangles[0]);
            w3Geoset owner = GetGeosetOfTriangle(triangles[0]);
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                if (geo == owner) { continue; }
                foreach (w3Triangle triangle in geo.Triangles)
                {
                    if (triangles.Contains(triangle)) { continue; }
                    float area = Calculator3D.GetTriangleArea(triangle);
                    if (area < TriangleArea) { triangle.isSelected = true; }
                }
            }
        }

        private void SelectNextOverlappingVertex(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count != 1)
            {
                MessageBox.Show("Select a single vertex", "Invalid request"); return;
            }
            bool br = false;
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v != vertices[0] && v.SameAs(vertices[0]))
                    {
                        vertices[0].isSelected = false;
                        v.isSelected = true; br = true;
                        break;
                    }
                }
                if (br) break;
            }
        }

        private void AssignToGeoset(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request"); return;
            }
            if (CurrentModel.Geosets.Count <= 1)
            {
                MessageBox.Show("At least 2 geosets must be present", "Invalid request"); return;
            }
            List<string> ids = CurrentModel.Geosets.Select(x => x.ID.ToString()).ToList();
            Selector s = new Selector(ids);
            s.Title = "Assign to geoset";
            w3Geoset target = getGeosetFromID(int.Parse(s.Selected));
            if (triangles.Count == 1)
            {
                w3Geoset geo = GetGeosetOfTriangle(triangles[0]);


                if (geo == target) { MessageBox.Show("Owner and target geoset cannot be the same", "Invalid request"); return; }

                w3Vertex v1 = triangles[0].Vertex1.Clone();
                w3Vertex v2 = triangles[0].Vertex2.Clone();
                w3Vertex v3 = triangles[0].Vertex3.Clone();
                w3Triangle tr = new w3Triangle();
                target.Triangles.Remove(triangles[0]);
                tr.Vertex1 = v1; tr.Vertex2 = v2; tr.Vertex3 = v3;
                target.Vertices.Add(v1);
                target.Vertices.Add(v2);
                target.Vertices.Add(v3);
                target.Triangles.Add(tr);

                return;
            }

            if (triangles.Count > 1)
            {
                Dictionary<w3Geoset, List<w3Triangle>> split = getParentsOfTriangles(triangles);
                foreach (var item in split)
                {
                    foreach (w3Triangle triangle in item.Key.Triangles)
                    {
                        if (triangles.Contains(triangle))
                        {
                            w3Geoset parent = item.Key;
                            w3Vertex v1 = triangle.Vertex1.Clone();
                            w3Vertex v2 = triangle.Vertex2.Clone();
                            w3Vertex v3 = triangle.Vertex3.Clone();
                            w3Triangle tr = new w3Triangle();
                            parent.Triangles.Remove(triangle);
                            tr.Vertex1 = v1; tr.Vertex2 = v2; tr.Vertex3 = v3;
                            parent.Vertices.Add(v1);
                            parent.Vertices.Add(v2);
                            parent.Vertices.Add(v3);
                            parent.Triangles.Add(tr);
                        }
                    }
                }

            }

        }
        private Dictionary<w3Geoset, List<w3Triangle>> getParentsOfTriangles(List<w3Triangle> list)
        {
            Dictionary<w3Geoset, List<w3Triangle>> split = new();
            foreach (w3Triangle triangle in list)
            {
                foreach (w3Geoset geo in CurrentModel.Geosets)
                {
                    foreach (w3Triangle currentTriangle in geo.Triangles)
                    {
                        if (currentTriangle == triangle)
                        {
                            if (split.ContainsKey(geo))
                            {
                                split[geo].Add(currentTriangle);
                            }
                            else
                            {
                                split.Add(geo, new List<w3Triangle>() { currentTriangle });
                            }
                        }
                    }
                }
            }


            return split;
        }
        private w3Geoset getGeosetFromID(int v)
        {
            return CurrentModel.Geosets.First(X => X.ID == v);
        }

        private void ExtractAssignToAG(object sender, RoutedEventArgs e)
        {
            List<w3Triangle> triangles = GetSelectedTriangles();
            if (triangles.Count == 0)
            {
                MessageBox.Show("Select at least one triangle", "Invalid request"); return;
            }
            if (CurrentModel.Geosets.Count <= 1)
            {
                MessageBox.Show("At least 2 geosets must be present", "Invalid request"); return;
            }
            List<string> ids = CurrentModel.Geosets.Select(x => x.ID.ToString()).ToList();
            Selector s = new Selector(ids);
            s.Title = "Assign to geoset";
            w3Geoset target = getGeosetFromID(int.Parse(s.Selected));
            if (triangles.Count == 1)
            {
                w3Geoset geo = GetGeosetOfTriangle(triangles[0]);


                if (geo == target) { MessageBox.Show("Owner and target geoset cannot be the same", "Invalid request"); return; }

                w3Vertex v1 = triangles[0].Vertex1.Clone();
                w3Vertex v2 = triangles[0].Vertex2.Clone();
                w3Vertex v3 = triangles[0].Vertex3.Clone();
                w3Triangle tr = new w3Triangle();

                tr.Vertex1 = v1; tr.Vertex2 = v2; tr.Vertex3 = v3;
                target.Vertices.Add(v1);
                target.Vertices.Add(v2);
                target.Vertices.Add(v3);
                target.Triangles.Add(tr);

                return;
            }

            if (triangles.Count > 1)
            {
                Dictionary<w3Geoset, List<w3Triangle>> split = getParentsOfTriangles(triangles);
                foreach (var item in split)
                {
                    foreach (w3Triangle triangle in item.Key.Triangles)
                    {
                        if (triangles.Contains(triangle))
                        {
                            w3Geoset parent = item.Key;
                            w3Vertex v1 = triangle.Vertex1.Clone();
                            w3Vertex v2 = triangle.Vertex2.Clone();
                            w3Vertex v3 = triangle.Vertex3.Clone();
                            w3Triangle tr = new w3Triangle();

                            tr.Vertex1 = v1; tr.Vertex2 = v2; tr.Vertex3 = v3;
                            parent.Vertices.Add(v1);
                            parent.Vertices.Add(v2);
                            parent.Vertices.Add(v3);
                            parent.Triangles.Add(tr);
                        }
                    }
                }

            }
        }

        private void CreatePlaneTexture(object sender, RoutedEventArgs e)
        {

            List<string> list = CurrentModel.Textures.Select(x => x.Path).ToList();
            if (list.Count == 0) { MessageBox.Show("No textures", "Invalid request"); return; }
            Selector s = new Selector(list);
            s.ShowDialog();
            if (s.DialogResult == true)
            {

                int index = s.box.SelectedIndex;
                if (CurrentModel.Textures[index].Replaceable_ID != 0)
                {
                    MessageBox.Show("No plane for replaceable textures!", "Invalid request"); return;
                }
                w3Node node = new w3Node();

                node.Data = new Bone();
                node.objectId = IDCounter.Next();
                node.Name = $"PlaneHolder_{node.objectId}";
                node.Billboarded = true;
                w3Material m = new w3Material(IDCounter.Next());
                w3Layer l = new w3Layer(IDCounter.Next());
                l.Diffuse_Texure_ID.StaticValue = [CurrentModel.Textures[index].ID];
                l.Two_Sided = true;
                m.Layers.Add(l);

                w3Geoset plane = Calculator3D.CreateVerticalPlane(MPQHelper.GetImage(CurrentModel.Textures[index].Path));
                foreach (w3Vertex v in plane.Vertices) { v.AttachedTo.Add(node.objectId); }
                plane.Material_ID = m.ID;
                CurrentModel.Materials.Add(m);
                CurrentModel.Geosets.Add(plane);
                RefreshGeosetList();
                CurrentModel.Nodes.Add(node);
                RefreshNodesList();
            }
        }

        private void SetExpandShrink(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Triangles)
            {
                List<w3Triangle> triangles = GetSelectedTriangles();
                if (triangles.Count != 2)
                {
                    MessageBox.Show("Select 2 triangles", "Invalid requset"); return;
                }
                if (Calculator3D.TrianglesAreConnected(triangles[0], triangles[1]))
                {
                    MessageBox.Show("The two selected triangles must not be connected", "Invalid requset"); return;
                }
                modifyMode_current = ModifyMode.ExpandTriangles;
                SelectedTriangles = triangles;
            }
            else if (editMode_current == EditMode.Edges)
            {
                List<w3Edge> edges = GetSelectedEdges();
                if (edges.Count != 2)
                {
                    MessageBox.Show("Select 2 edges", "Invalid requset"); return;
                }
                if (Calculator3D.EdgesAreConnected(edges[0], edges[1]))
                {
                    MessageBox.Show("The two selected edges must not be connected", "Invalid requset"); return;
                }
                modifyMode_current = ModifyMode.ExpandEdges;
                SelectedEdges = edges;

            }
            else
            {
                return;
            }


        }

        private void SelectedUVTexture(object sender, SelectionChangedEventArgs e)
        {
            int selected = Combo_UVTextures.SelectedIndex;
            UVEditor_Values.DisplayedImage = MPQHelper.GetImage(CurrentModel.Textures[selected].Path);
            RefreshUVCanvas(selected);
        }
        private void RefreshUVCanvas(int selectedIndex = -1)
        {
            //unfinished
            SetUVTexture();
            RefreshUvMapping();
            DrawGrid(UVCanvas_Grid, GetGridInput(Input_UVMapperGridSize));
        }
        private void SetUVTexture()
        {
            Bitmap texture = UVEditor_Values.DisplayedImage;
            if (texture == null)
                return; // Exit if no texture is available

            // Set how many times to tile the texture (20x20)
            int tileCount = 20;

            // Calculate the total size of the tiled texture
            int totalWidth = texture.Width * tileCount;  // Full width for 20 tiles horizontally
            int totalHeight = texture.Height * tileCount; // Full height for 20 tiles vertically

            // Create a new Bitmap to hold the entire tiled image
            Bitmap tiledTexture = new Bitmap(totalWidth, totalHeight);

            // Use Graphics to draw the texture multiple times to fill the canvas
            using (Graphics g = Graphics.FromImage(tiledTexture))
            {
                g.Clear(System.Drawing.Color.Transparent);  // Clear with transparency, or use any background color

                // Draw the texture in a 20x20 grid without any scaling
                for (int x = 0; x < tileCount; x++)
                {
                    for (int y = 0; y < tileCount; y++)
                    {
                        // Draw the texture at its full resolution in the appropriate position
                        g.DrawImage(texture, x * texture.Width, y * texture.Height, texture.Width, texture.Height);
                    }
                }
            }

            // Convert the tiled texture to a BitmapSource for WPF Image control
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                tiledTexture.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            // Set the tiled texture to the UVCanvas_Texture Image control
            UVCanvas_Texture.Source = bitmapSource;

            // Clean up the temporary GDI+ resources
            tiledTexture.Dispose();
        }


        private void ReleasedUVMapper(object sender, MouseButtonEventArgs e)
        {
            //unfinished
        }

        private void RefreshManualTranslation(object sender, RoutedEventArgs e)
        {
            if (editMode_current == EditMode.Vertices)
            {
                List<w3Vertex> vertices = GetSelectedVertices();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                inputX.Text = centroid.X.ToString(); ;
                inputY.Text = centroid.Y.ToString(); ;
                inputZ.Text = centroid.Z.ToString(); ;
            }
            else if (editMode_current == EditMode.Triangles)
            {
                List<w3Vertex> vertices = GetSelectedTriangles()
             .SelectMany(x => new[] { x.Vertex1, x.Vertex2, x.Vertex3 })
              .ToList();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                inputX.Text = centroid.X.ToString(); ;
                inputY.Text = centroid.Y.ToString(); ;
                inputZ.Text = centroid.Z.ToString(); ;
            }
            else if (editMode_current == EditMode.Edges)
            {
                List<w3Vertex> vertices = GetSelectedEdges()
           .SelectMany(x => new[] { x.Vertex1, x.Vertex2 })
            .ToList();
                Coordinate centroid = Calculator3D.GetCentroid(vertices);
                inputX.Text = centroid.X.ToString(); ;
                inputY.Text = centroid.Y.ToString(); ;
                inputZ.Text = centroid.Z.ToString(); ;
            }
        }



        private void ReleasedSlider(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Slider s = (Slider)sender;
            int tick = (int)s.Value;
            InputCurrentTrack.Text = tick.ToString();
            RefreshFrame();
        }
        private int FindKeyframeInRange(w3Sequence sequene, int currentTrack, bool Next)
        {
            if (sequene.Interval <= 1) { return currentTrack; }


            if (Next)
            {
                int from = currentTrack + 1;
                int to = sequene.To;
                for (int i = from; i <= to; i++)
                {
                    foreach (w3Node node in CurrentModel.Nodes)
                    {
                        if (
                            node.Translation.Keyframes.Any(x => x.Track == i) ||
                            node.Rotation.Keyframes.Any(x => x.Track == i) ||
                            node.Scaling.Keyframes.Any(x => x.Track == i))
                        { return i; }
                    }

                }
            }
            else
            {
                int to = currentTrack - 1;
                int from = sequene.From;
                for (int i = to; i >= from; i--)
                {
                    foreach (w3Node node in CurrentModel.Nodes)
                    {
                        if (
                            node.Translation.Keyframes.Any(x => x.Track == i) ||
                            node.Rotation.Keyframes.Any(x => x.Track == i) ||
                            node.Scaling.Keyframes.Any(x => x.Track == i))
                        { return i; }
                    }

                }

            }
            return currentTrack;



        }


        private void NextAnimatedTrack(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {

                w3Sequence sequence = GetSelectedSequence();

                if (CurrentSceneFrame == sequence.To) { return; }
                int compare = CurrentSceneFrame;
                int find = FindKeyframeInRange(sequence, CurrentSceneFrame, true);

                if (compare != find)
                {
                    CurrentSceneFrame = find;
                    InputCurrentTrack.Text = CurrentSceneFrame.ToString();
                    RefreshFrame();

                }

            }



        }

        private void PrevAnimatedTrack(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                w3Sequence sequence = GetSelectedSequence();

                if (CurrentSceneFrame == sequence.From) { return; }
                int compare = CurrentSceneFrame;
                int find = FindKeyframeInRange(sequence, CurrentSceneFrame, false);
                if (compare != find)
                {
                    CurrentSceneFrame = find;
                    InputCurrentTrack.Text = CurrentSceneFrame.ToString();
                    RefreshFrame();

                }
            }
        }

        private void MakeAsFirstPrevius(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                w3Sequence sequence = GetSelectedSequence();
                if (sequence != null)
                {
                    if (CurrentSceneFrame > sequence.From && CurrentSceneFrame <= sequence.To)
                    {
                        List<int> list = GetAnimatedFramesInRange(sequence.From, CurrentSceneFrame);
                        if (list.Count != 0)
                        {
                            CopiedFrame = list.Last();
                            PasteFrame(true, true, true);
                            FillTimeline(sequence);
                        }
                    }
                }
            }
        }
        private List<int> GetAnimatedFramesInRange(int from, int to)
        {
            List<int> list = new List<int>();
            for (int i = from; i <= to; i++)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (
                        node.Translation.Keyframes.Any(x => x.Track == i) ||
                        node.Rotation.Keyframes.Any(x => x.Track == i) ||
                        node.Scaling.Keyframes.Any(x => x.Track == i))


                    { list.Add(i); }
                }
            }

            return list;
        }
        private void MakeAsFirstNext(object sender, RoutedEventArgs e)
        {
            if (ListSequences.SelectedItem != null)
            {
                w3Sequence sequence = GetSelectedSequence();
                if (sequence != null)
                {
                    if (CurrentSceneFrame < sequence.To && CurrentSceneFrame >= sequence.To)
                    {
                        List<int> list = GetAnimatedFramesInRange(CurrentSceneFrame, sequence.To);
                        if (list.Count != 0)
                        {
                            CopiedFrame = list.First();
                            PasteFrame(true, true, true);
                            FillTimeline(sequence);
                        }
                    }
                }
                SetSaved(false);
            }

        }

        private void MissingKeyframes1(object sender, RoutedEventArgs e)
        {
            foreach (w3Sequence sequence in CurrentModel.Sequences)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (node.Translation.Keyframes.Any(x => x.Track >= sequence.From && x.Track <= sequence.To))
                    {
                        if (node.Translation.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = node.Translation.Keyframes[0].Clone();
                            kf.Track = sequence.From;
                            node.Translation.Keyframes.Add(kf);
                        }
                        if (node.Translation.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = node.Translation.Keyframes.Last().Clone();
                            kf.Track = sequence.To;
                            node.Translation.Keyframes.Add(kf);
                        }
                        if (node.Rotation.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = node.Rotation.Keyframes[0].Clone();
                            kf.Track = sequence.From;
                            node.Rotation.Keyframes.Add(kf);
                        }
                        if (node.Rotation.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = node.Rotation.Keyframes.Last().Clone();
                            kf.Track = sequence.To;
                            node.Rotation.Keyframes.Add(kf);
                        }
                        if (node.Scaling.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = node.Scaling.Keyframes[0].Clone();
                            kf.Track = sequence.From;
                            node.Scaling.Keyframes.Add(kf);
                        }
                        if (node.Scaling.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = node.Scaling.Keyframes.Last().Clone();
                            kf.Track = sequence.To;
                            node.Scaling.Keyframes.Add(kf);
                        }
                    }
                }
            }
            FillTimeline(GetSelectedSequence());
            SetSaved(false);
        }

        private void MissingKeyframes2(object sender, RoutedEventArgs e)
        {
            foreach (w3Sequence sequence in CurrentModel.Sequences)
            {
                foreach (w3Node node in CurrentModel.Nodes)
                {
                    if (node.Translation.Keyframes.Any(x => x.Track >= sequence.From && x.Track <= sequence.To))
                    {
                        if (node.Translation.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.From, [0, 0, 0], [0, 0, 0], [0, 0, 0]);

                            node.Translation.Keyframes.Add(kf);
                        }
                        if (node.Translation.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.To, [0, 0, 0], [0, 0, 0], [0, 0, 0]);

                            node.Translation.Keyframes.Add(kf);
                        }
                        if (node.Rotation.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.From, [0, 0, 0], [0, 0, 0], [0, 0, 0]);

                            node.Rotation.Keyframes.Add(kf);
                        }
                        if (node.Rotation.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.To, [0, 0, 0], [0, 0, 0], [0, 0, 0]);

                            node.Rotation.Keyframes.Add(kf);
                        }
                        if (node.Scaling.Keyframes.Any(x => x.Track == sequence.From) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.From, [100, 100, 100], [100, 100, 100], [100, 100, 100]);

                            node.Scaling.Keyframes.Add(kf);
                        }
                        if (node.Scaling.Keyframes.Any(x => x.Track == sequence.To) == false)
                        {
                            w3Keyframe kf = new w3Keyframe(sequence.To, [100, 100, 100], [100, 100, 100], [100, 100, 100]);

                            node.Scaling.Keyframes.Add(kf);
                        }
                    }
                }
            }
            FillTimeline(GetSelectedSequence());
            SetSaved(false);
        }

        private void ClampUVsTo_0_1(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();
            if (vertices.Count < 1)
            {
                MessageBox.Show("Select at least one vertex", "Invalid requst"); return;
            }
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position.ClampDefault();
            }
        }

        private void SwapSequenceNames(object sender, RoutedEventArgs e)
        {
            if (CurrentModel.Sequences.Count < 2) { MessageBox.Show("At least 2 sequences must be present", "Invalid request"); return; }
            if (ListSequences.SelectedItem == null) { MessageBox.Show("Select a sequence", "Invalid request"); return; }
            w3Sequence sequence = GetSelectedSequence();
            Selector ss = new Selector(CurrentModel.Sequences.Select(x => x.Name).ToList());
            ss.ShowDialog();
            if (ss.DialogResult == true)
            {
                w3Sequence second = GetSequenceFromName(ss.Selected);
                if (second == null) { MessageBox.Show("The second sequence doesn't exist", "Invalid request"); return; }
                if (sequence.Name == second.Name) { MessageBox.Show("The selected and the target sequences must be different", "Error"); return; }
                string temp = sequence.Name;
                sequence.Name = second.Name;
                second.Name = temp;
                RefreshSequencesList();
            }
        }
        private w3Sequence GetSequenceFromName(string name)
        {
            if (CurrentModel.Sequences.Any(x => x.Name == name))
            {
                return CurrentModel.Sequences.First(x => x.Name == name);
            }
            return null;
        }

        private void test(object sender, RoutedEventArgs e)
        {
            RefreshBitmaps();
        }

        private void OpenUVMore(object sender, RoutedEventArgs e)
        {
            ButtonUVMore.ContextMenu.IsOpen = true;
        }

        private void HitEnterUV_U(object sender, MouseButtonEventArgs e)
        {

        }

        private void HitEnterUV_V(object sender, KeyEventArgs e)
        {

        }

        private void CreateBuildingAttachmentPoints(object sender, RoutedEventArgs e)
        {
            List<string> names = new List<string>();
            names.Add("Sprite First Ref");
            names.Add("Sprite Second Ref");
            names.Add("Sprite Third Ref");
            names.Add("Sprite Fourth Ref");
            names.Add("Sprite RallyPoint Ref");
            foreach (string name in names)
            {
                if (CurrentModel.Nodes.Any(x=>x.Name.ToLower().Trim() == name.ToLower()) == false)
                { 
                    w3Node node = new w3Node();
                    node.Name = name;
                    node.objectId = IDCounter.Next();
                    node.parentId = -1;
                    node.Data = new w3Attachment();
                    CurrentModel.Nodes.Add(node);
                   List_Nodes.Items.Add(   NewTreeItem(name, NodeType.Attachment));
                }
            }
        }

        private void SetModeScaleHor(object sender, RoutedEventArgs e)
        {
            uVWorkMode = UVWorkMode.ResizeHor;

            Button_Rotate.BorderBrush = Brushes.Black;
            Button_Move.BorderBrush = Brushes.Black;
            Button_Scale.BorderBrush = Brushes.Black;
            Button_ScaleHor.BorderBrush = Brushes.Blue;
            Button_ScaleVer.BorderBrush = Brushes.Black;
            UVCanvas_Overlay.Cursor = Cursors.SizeWE;
        }

        private void SetModeScaleVer(object sender, RoutedEventArgs e)
        {
            uVWorkMode = UVWorkMode.ResizeVer;

            Button_Rotate.BorderBrush = Brushes.Black;
            Button_Move.BorderBrush = Brushes.Black;
            Button_Scale.BorderBrush = Brushes.Black;
            Button_ScaleHor.BorderBrush = Brushes.Black;
            Button_ScaleVer.BorderBrush = Brushes.Blue;
            UVCanvas_Overlay.Cursor = Cursors.SizeNS;
        }

        private void FitUVFromEditor(object sender, RoutedEventArgs e)
        {
            //unfinished
        }

        private void AttachAllSelectedGeosetsTo(object sender, RoutedEventArgs e)
        {
            if (List_Geosets.SelectedItems.Count > 0)
            {
                List<w3Geoset> geosets = GetSelectedGeosets();
                List<string> nodeNames = CurrentModel.Nodes.Where(y=>y.Data is Bone).Select(x => x.Name).ToList();
                if (nodeNames.Count == 0) { MessageBox.Show("No bones present","Invalid request" );  return; }
                Selector sc = new Selector(nodeNames);
                sc.ShowDialog();
                if (sc.DialogResult == true)
                {
                    string name = sc.Selected;
                    w3Node node = CurrentModel.Nodes.First(x => x.Name == name);
                    int id = node.objectId;
                    foreach (w3Geoset geo in geosets)
                    {
                        foreach (w3Vertex v in geo.Vertices)
                        {
                            v.AttachedTo = new List<int>() { id };
                        }
                    }
                }
            }
        }

        private void CenterUV(object sender, RoutedEventArgs e)
        {
            // unfinished
        }

        private void ClampUVsTo_0_1_fromEditor(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedUVVertices();
            if (vertices.Count < 1)
            {
                MessageBox.Show("Select at least one vertex", "Invalid requst"); return;
            }
            foreach (w3Vertex v in vertices)
            {
                v.Texture_Position.ClampDefault();
            }
        }

        private List<w3Vertex> GetSelectedUVVertices()
        {
            List<w3Vertex> list = new List<w3Vertex>();
            foreach (w3Geoset geo in CurrentModel.Geosets)
            {
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.isSelectedUV) { list.Add(v); }
                }
            }
            return list;
        }

        private void KeyOnListGeosets(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DelGeoset(null, null); return;
            }
        }

        private void ArrangeVertices(object sender, RoutedEventArgs e)
        {
            List<w3Vertex> vertices = GetSelectedVertices();

            if (vertices.Count < 3) { MessageBox.Show("Select at lesat 3 vertices", "Invalid request"); return; }
            ArrangeVertices_Window window = new ArrangeVertices_Window();
            window.ShowDialog();
            if (window.DialogResult == true)
            {
                Calculator3D.ArrangeVertices(vertices, window.Angle, window.Distance); SetSaved(false);
            }

        }

        private void AdjustAll_(object sender, RoutedEventArgs e)
        {
            AdjustAll ao = new AdjustAll();
            ao.ShowDialog();
            if (ao.DialogResult == true)
            {
                float x = ao.X;
                float y = ao.Y;
                float z = ao.Z;
                if (ao.check_c.IsChecked == true)
                {
                    foreach (w3Node node in CurrentModel.Nodes)
                    {
                        node.Change(x, y, z);
                    }
                }
                if (ao.check_g.IsChecked == true)
                {
                    foreach (w3Geoset geo in CurrentModel.Geosets)
                    {
                        foreach (w3Vertex v in geo.Vertices)
                        {
                            v.Position.ChangeWith(x, y, z);
                        }
                    }
                }
                if (ao.check_n.IsChecked == true)
                {
                    foreach (w3Node node in CurrentModel.Nodes)
                    {
                        if (node.Data is Collision_Shape)
                        {
                            Collision_Shape cs = (Collision_Shape)node.Data;
                            if (cs.Type == CollisionShapeType.Box)
                            {
                                cs.Extents.Change(x, y, z);
                            }
                        }

                    }
                }
                SetSaved(false);

            }
        }


    }

}