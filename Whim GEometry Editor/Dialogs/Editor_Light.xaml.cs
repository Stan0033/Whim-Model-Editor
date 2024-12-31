using Dsafa.WpfColorPicker;
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

    public static class DialogAnimateChecker
    {
        public static void SwitchAnimated(TextBox input, CheckBox check, Button animate, w3Transformation transformation)
        {
            bool animated = check.IsChecked == true;
            input.IsEnabled = !animated;
            animate.IsEnabled = animated;
            transformation.isStatic = !animated;

        }
        public static void SwitchAnimated(Button input, CheckBox check, Button animate, w3Transformation transformation)
        {
            bool animated = check.IsChecked == true;
            input.IsEnabled = !animated;
            animate.IsEnabled = animated;
            transformation.isStatic = !animated;

        }
        public static void SwitchAnimated(CheckBox input, CheckBox check, Button animate, w3Transformation transformation)
        {
            bool animated = check.IsChecked == true;
            input.IsEnabled = !animated;
            animate.IsEnabled = animated;
            transformation.isStatic = !animated;

        }
    }
    /// <summary>
    /// Interaction logic for Editor_Light.xaml
    /// </summary>
    public partial class Editor_Light : Window
    {
        w3Light light;
        w3Model Model;
        public Editor_Light(w3Node node, w3Model model)
        {
            InitializeComponent();
            light = (w3Light)node.Data;
            FillData();
            Model = model;
        }
        private void FillData()
        {
            //type
            List_Type.SelectedIndex = (int)light.Type;
            //visibility
             
            Check_Visibility.IsChecked = light.Visibility.isStatic == false;
            
            if (light.Visibility.isStatic) {Check_Vis.IsChecked = light.Visibility.StaticValue[0] == 1; }
            //color

            Check_Color.IsChecked = light.Color.isStatic == false;
             ButtonEditStaticColor.Background = Converters.BrushFromRGB(light.Color.StaticValue);
            //amb color

            Check_AmColor.IsChecked = light.Ambient_Color.isStatic == false;
             ButtonEditStaticAmColor.Background = Converters.BrushFromRGB(light.Ambient_Color.StaticValue);
            // intensity
            Check_Intensity.IsChecked = light.Intensity.isStatic == false;
            BoxIntensity.Text = light.Intensity.StaticValue[0].ToString();
            // amb intensity
            Check_Intensity.IsChecked = light.Intensity.isStatic == false;
            BoxAmIntensity.Text = light.Ambient_Intensity.StaticValue[0].ToString();
            // att start
            Check_AttStart.IsChecked = light.Attenuation_Start.isStatic == false;
            BoxAttStart.Text = light.Attenuation_Start.StaticValue[0].ToString();
            // att start
            Check_AttEnd.IsChecked = light.Attenuation_Start.isStatic == false;
            BoxAttEnd.Text = light.Attenuation_Start.StaticValue[0].ToString();
            // visibility
            Check_Visibility.IsChecked = light.Visibility.isStatic == false;
            Check_Vis.IsChecked = light.Visibility.StaticValue[0] == 1 ? true : false;
        }

        private void Checked_AmIntensity(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxAmIntensity, Check_AmIntensity, ButtonEditAmIntensity, light.Ambient_Intensity);
        }

        private void SetAnimatedAmIntensity(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Ambient_Intensity, TransformationType.Float);
            tw.ShowDialog();

        }

        private void SetAmIntensity(object sender, TextChangedEventArgs e)
        {
            bool parsed = float.TryParse(BoxAmIntensity.Text, out float value);
            light.Ambient_Intensity.StaticValue = parsed ? [value] : [0];
        }

        private void SetType(object sender, SelectionChangedEventArgs e)
        {
            light.Type = (LightType)List_Type.SelectedIndex;
        }

        private void Checked_Intensity(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxIntensity, Check_Intensity, ButtonEditIntensity, light.Intensity);
        }

        private void SetAnimatedAttEnd(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Attenuation_End, TransformationType.Float); tw.ShowDialog();
        }

        private void SetAnimatedAmColor(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Ambient_Color, TransformationType.Color); tw.ShowDialog();
        }

        private void SetAnimatedVisibility(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Visibility, TransformationType.Visibility);

            tw.ShowDialog();
        }

        private void Checked_Color(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(ButtonEDitColor, Check_Color, ButtonEDitColor, light.Color);
        }
      
        private void Checked_AttEnd(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxAttEnd, Check_AttEnd, ButtonEditAttEnd, light.Attenuation_End);

        }

        private void Checked_AmColor(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(ButtonEDitAmColor, Check_AmColor, ButtonEDitAmColor, light.Ambient_Color);
        }

        private void SetVisible(object sender, RoutedEventArgs e)
        {
            light.Visibility.StaticValue = Check_Vis.IsChecked ==true? [1] : [0];
        }

        private void SetAnimatedColor(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Color, TransformationType.Color); tw.ShowDialog();
        }

        private void SetAnimatedAttStart(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Color, TransformationType.Float); tw.ShowDialog();
        }

        private void Checked_Visibility(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(Check_Vis, Check_Visibility, ButtonEditVisibility, light.Visibility);
        }

        private void SetColor(object sender, RoutedEventArgs e)
        {
            var initialColor = Converters.BrushToColor(ButtonEditStaticColor.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ButtonEditStaticColor.Background = Converters.ColorToBrush(dialog.Color);
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);


                light.Color.StaticValue = rgb;

            }
        }

        private void SetAttentuantStart(object sender, TextChangedEventArgs e)
        {
            bool parsed = float.TryParse(BoxAttStart.Text, out float value);
            if (parsed)
            {
                float clamped = value < 80 ? 80 : value;
                clamped = value > 199? 199 : clamped;
                light.Attenuation_Start.StaticValue = [clamped];
            }
            else
            {
                light.Attenuation_Start.StaticValue = [80];
            }
            
        }

        private void SetIntensity(object sender, TextChangedEventArgs e)
        {
            bool parsed = float.TryParse(BoxIntensity.Text, out float value);
            light.Intensity.StaticValue = parsed ? [value] : [value <0 ? 0 : value];
        }

        private void SetAttentuanEnd(object sender, TextChangedEventArgs e)
        {
            bool parsed = float.TryParse(BoxAttEnd.Text, out float value);
            if (parsed)
            {
                float clamped = value > 200? 200 : value;
                clamped = value < light.Attenuation_Start.StaticValue[0] ? light.Attenuation_Start.StaticValue[0] + 1 : clamped;
            }
            else
            {
                light.Attenuation_End.StaticValue = [light.Attenuation_Start.StaticValue[0] +1];
            }
            light.Attenuation_End.StaticValue = parsed ? [value] : [0];
        }

        private void SetAnimatedIntensity(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, light.Intensity, TransformationType.Float);

            tw.ShowDialog();
        }

        private void Checked_AttStart(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxAttStart, Check_AttStart, ButtonEditAttStart, light.Attenuation_Start);
        }

        private void SetColorAmbient(object sender, RoutedEventArgs e)
        {
            var initialColor = Converters.BrushToColor(ButtonEditStaticAmColor.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ButtonEditStaticAmColor.Background = Converters.ColorToBrush(dialog.Color);
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);


                light.Ambient_Color.StaticValue = rgb;

            }
        }
    }
}
