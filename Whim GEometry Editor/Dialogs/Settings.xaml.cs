using Dsafa.WpfColorPicker;
using MDLLibs.Classes.Misc;
 
using System.Windows;
using System.Windows.Controls;
 
using System.Windows.Input;
using System.Windows.Media;
 
using Whim_GEometry_Editor.Misc;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
   
    enum PointType
    {
        Square
            ,Triangle
    }
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            if (AppSettings.BackfaceCullingEnabled)
            {
                if (AppSettings.BackfaceCullingClockwise) Check_Culling1.IsChecked = true; else Check_Culling2.IsChecked = true;
            }
            else
            {
                Check_Culling0.IsChecked = true;
            }
            InputNearClip.Text = AppSettings.NearDistance.ToString();
            InputFarClip.Text = AppSettings.FarDistance.ToString();
            InputFieldOfView.Text = AppSettings.FieldOfView.ToString();
            InputZoomIncrement.Text = AppSettings.ZoomIncrement.ToString();
            InputRotateIncrement.Text = AppSettings.RotateSpeed.ToString();
            FillAntiAliasingTCombobox();
            
            if (Check_Culling0.IsChecked == true) { AppSettings.BackfaceCullingEnabled = false; }
            if (Check_Culling1.IsChecked == true) { AppSettings.BackfaceCullingClockwise = true; }
            if (Check_Culling2.IsChecked == true) { AppSettings.BackfaceCullingClockwise = false; }
            ButtonVertexColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.Color_Vertex);
            ButtonVertexColorS.Background = Converters.BrushFromNormalizedRGB(AppSettings.Color_VertexSelected);
            ButtonTriangleS.Background = Converters.BrushFromNormalizedRGB(AppSettings.TriangleColorSelected);
            
            ButtonNodeColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.NodeColor);
            ButtonNodeColorS.Background = Converters.BrushFromNormalizedRGB(AppSettings.NodeColorSelected);
            ButtonEdgeColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.EdgeColor);
            ButtonEdgeColorSelected.Background = Converters.BrushFromNormalizedRGB(AppSettings.EdgeColorSelected);
            ButtonGridColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.GridColor);
            ButtonNormalColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.NormalsColor);
            ButtonRiggingColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.RiggingColor);
            ButtonExtentsColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.ExtentsColor);
            ButtonSkeletonColor.Background = Converters.BrushFromNormalizedRGB(AppSettings.SkeletonColor);
            ButtonAmbient.Background = Converters.BrushFromNormalizedRGB(AppSettings.AmbientColor);
            ButtonDiffuse.Background = Converters.BrushFromNormalizedRGB(AppSettings.DiffuseColor);
            ButtonSpectacular.Background = Converters.BrushFromNormalizedRGB(AppSettings.SpecularColor);
            ButtonMaterialDiffuse.Background = Converters.BrushFromNormalizedRGB(AppSettings.MaterialDiffuseColor);
            ButtonMaterialSpecular.Background = Converters.BrushFromNormalizedRGB(AppSettings.MaterialSpecularColor);
            ButtonBackground.Background = Converters.BrushFromNormalizedRGB(AppSettings.BackgroundColor);
            ButtonVertexColorR.Background = Converters.BrushFromNormalizedRGB(AppSettings.Color_VertexRigged);
            ButtonVertexColorRS.Background = Converters.BrushFromNormalizedRGB(AppSettings.Color_VertexRiggedSelected);
            LightPos1.Text = AppSettings.LightPostion[0].ToString();
            LightPos2.Text = AppSettings.LightPostion[1].ToString();
            LightPos3.Text = AppSettings.LightPostion[2].ToString();
            LightPos4.Text = AppSettings.LightPostion[3].ToString();
            InputSpectacularPower.Text = AppSettings.Shininess.ToString();
            input_Save.Text = AppSettings.Autosave.ToString();
            input_SaveBackup.Text = AppSettings.AutoBackup.ToString();

            Check_PointSquare.IsChecked = AppSettings.PointType == PointType.Square;
            Check_PointTriangle.IsChecked = AppSettings.PointType == PointType.Triangle;
            InputPointSize.Text = AppSettings.PointSize.ToString();
            InputHistoryLimit.Text = AppSettings.HistoryLimit.ToString();
            Check_HistoryEnabled.IsChecked = AppSettings.HistoryEnabled;

        }
        private void FillAntiAliasingTCombobox()
        {
            // Clear any existing items in the ComboBox
            ComboAA.Items.Clear();

            // Loop through the AntialiasingTechnique enum values
            foreach (AntialiasingTechnique aaTech in Enum.GetValues(typeof(AntialiasingTechnique)))
            {
                // Add each enum value as a ComboBox item
                ComboAA.Items.Add(new ComboBoxItem() { Content = aaTech.ToString() });
            }

            // Set the currently selected item based on AppSettings.AA
            foreach (ComboBoxItem item in ComboAA.Items)
            {
                if (item.Content.ToString() == AppSettings.AA.ToString())
                {
                    ComboAA.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            //camera
            AppSettings.NearDistance = Converters.SafeFloat(InputNearClip.Text, 0.5f);
            AppSettings.FarDistance = Converters.SafeFloat(InputFarClip.Text, 5000f);
            AppSettings.FieldOfView = Converters.SafeFloat(InputFieldOfView.Text, 45f);
            AppSettings.ZoomIncrement = Converters.SafeFloat(InputZoomIncrement.Text, 1);
            AppSettings.RotateSpeed = Converters.SafeFloat(InputRotateIncrement.Text, 1);
            //lighting
             
            AppSettings.AmbientColor = Converters.BrushToNormalizedRGB(ButtonAmbient.Background);
            AppSettings.DiffuseColor = Converters.BrushToNormalizedRGB(ButtonDiffuse.Background);
            AppSettings.SpecularColor = Converters.BrushToNormalizedRGB(ButtonSpectacular.Background);
            AppSettings.Shininess = Converters.SafeFloat(InputSpectacularPower.Text, 50f);
            AppSettings.MaterialDiffuseColor = Converters.BrushToNormalizedRGB(ButtonMaterialDiffuse.Background);
            AppSettings.MaterialSpecularColor = Converters.BrushToNormalizedRGB(ButtonMaterialSpecular.Background);
            AppSettings.LightPostion = 
                [
                Converters.SafeFloat(LightPos1.Text, 0),
                Converters.SafeFloat(LightPos2.Text, 0),
                Converters.SafeFloat(LightPos3.Text, 1),
                Converters.SafeFloat(LightPos4.Text, 1),
                ];
            //rendering
            AppSettings.Color_Vertex = Converters.BrushToNormalizedRGB(ButtonVertexColor.Background);
            AppSettings.Color_VertexSelected = Converters.BrushToNormalizedRGB(ButtonVertexColorS.Background);
            AppSettings.Color_VertexRigged= Converters.BrushToNormalizedRGB(ButtonVertexColorR.Background);
            AppSettings.Color_VertexRiggedSelected= Converters.BrushToNormalizedRGB(ButtonVertexColorRS.Background);
            AppSettings.TriangleColorSelected = Converters.BrushToNormalizedRGB(ButtonTriangleS.Background);
            AppSettings.TriangleColorSelected = Converters.BrushToNormalizedRGB(ButtonTriangleS.Background);
            AppSettings.NodeColor = Converters.BrushToNormalizedRGB(ButtonNodeColor.Background);
            AppSettings.NodeColorSelected = Converters.BrushToNormalizedRGB(ButtonNodeColorS.Background);
            AppSettings.EdgeColor = Converters.BrushToNormalizedRGB(ButtonEdgeColor.Background);
            AppSettings.EdgeColorSelected = Converters.BrushToNormalizedRGB(ButtonEdgeColorSelected.Background);
            
            AppSettings.GridColor = Converters.BrushToNormalizedRGB(ButtonGridColor.Background);
            AppSettings.NormalsColor = Converters.BrushToNormalizedRGB(ButtonNormalColor.Background);
            AppSettings.RiggingColor = Converters.BrushToNormalizedRGB(ButtonRiggingColor.Background);
            AppSettings.ExtentsColor = Converters.BrushToNormalizedRGB(ButtonExtentsColor.Background);
            AppSettings.SkeletonColor = Converters.BrushToNormalizedRGB(ButtonSkeletonColor.Background);
            AppSettings.BackgroundColor = Converters.BrushToNormalizedRGB(ButtonBackground.Background);
            AppSettings.Autosave = Converters.SafeInt(input_Save.Text,0);
            AppSettings.AutoBackup = Converters.SafeInt(input_SaveBackup.Text,0);



            //history
            AppSettings.HistoryEnabled = Check_HistoryEnabled.IsEnabled == true;

            AppSettings.HistoryLimit = Converters.SafeInt(InputHistoryLimit.Text, 0);
            if (AppSettings.HistoryEnabled == false) { AppSettings.HistoryLimit = 0; }

            //point
            AppSettings.PointType = Check_PointSquare.IsChecked == true ? PointType.Square : PointType.Triangle;
            float pointSize = Converters.SafeFloat(InputPointSize.Text);
            if (pointSize >= 0 && pointSize <= 1)
            {
                AppSettings.PointSize = pointSize;
            }
            else
            {
                AppSettings.PointSize = 1;
            }
           



            AppSettings.SaveSettings();
            DialogResult = true;
        }
        private Brush SetColor(Button button)
        {

            Color initialColor = Converters.BrushToColor(button.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return Converters.ColorToBrush(dialog.Color);

                 

            }
            else
            {
                return button.Background;
            }
        }
        private void SetVertexColor(object sender, RoutedEventArgs e)
        {
            ButtonVertexColor.Background =  SetColor(ButtonVertexColor);
        }

        private void SetVertexColorSelected(object sender, RoutedEventArgs e)
        {
            ButtonVertexColorS.Background = SetColor(ButtonVertexColorS);
        }

        private void SetTriangleSelectedColor(object sender, RoutedEventArgs e)
        {
            ButtonTriangleS.Background = SetColor(ButtonTriangleS);

        }

        private void SetNodeColor(object sender, RoutedEventArgs e)
        {
            ButtonNodeColor.Background = SetColor(ButtonNodeColor);

        }

        private void SetNodeColorS(object sender, RoutedEventArgs e)
        {
            ButtonNodeColorS.Background = SetColor(ButtonNodeColorS);
        }

        private void SetEdgeColor(object sender, RoutedEventArgs e)
        {
            ButtonEdgeColor.Background = SetColor(ButtonEdgeColor);
        }

        private void SetEdgeColorS(object sender, RoutedEventArgs e)
        {
            ButtonEdgeColorSelected.Background = SetColor(ButtonEdgeColorSelected);
        }

        private void SetGridColor(object sender, RoutedEventArgs e)
        {
            ButtonGridColor.Background = SetColor(ButtonGridColor);
        }

        private void SetNormalColor(object sender, RoutedEventArgs e)
        {
            ButtonNormalColor.Background = SetColor(ButtonNormalColor);
        }

        private void SetExtentsColor(object sender, RoutedEventArgs e)
        {
            ButtonExtentsColor.Background = SetColor(ButtonExtentsColor);
        }

        private void SetSkeletonColor(object sender, RoutedEventArgs e)
        {
            ButtonSkeletonColor.Background = SetColor(ButtonSkeletonColor);
        }

        private void SetAmbientColor(object sender, RoutedEventArgs e)
        {
            ButtonAmbient.Background = SetColor(ButtonAmbient);
        }

        private void SetDiffuseColor(object sender, RoutedEventArgs e)
        {
            ButtonDiffuse.Background = SetColor(ButtonDiffuse);
        }

        private void SetSpectacularColor(object sender, RoutedEventArgs e)
        {
            ButtonSpectacular.Background = SetColor(ButtonSpectacular);
        }

        private void SetMaterialDiffuse(object sender, RoutedEventArgs e)
        {
            ButtonMaterialDiffuse.Background = SetColor(ButtonMaterialDiffuse);
        }

        private void SetRiggingColor(object sender, RoutedEventArgs e)
        {
            ButtonRiggingColor.Background = SetColor(ButtonRiggingColor);
        }

        private void ResetDefaults(object sender, RoutedEventArgs e)
        {
            AppSettings.SetDefaults();
            AppSettings.SaveSettings();
            DialogResult = true;
        }

        private void SetBackground(object sender, RoutedEventArgs e)
        {
            ButtonBackground.Background = SetColor(ButtonBackground);

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }

        private void SetVertexColorRigged(object sender, RoutedEventArgs e)
        {
            ButtonVertexColorR.Background = SetColor(ButtonVertexColorR);
        }

        private void SetVertexColorRiggedSelected(object sender, RoutedEventArgs e)
        {
            ButtonVertexColorRS.Background = SetColor(ButtonVertexColorRS);
        }
    }
}
