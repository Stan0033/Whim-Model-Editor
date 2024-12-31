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
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Whim_GEometry_Editor.Dialogs
{
    /// <summary>
    /// Interaction logic for Editor_Ribbon.xaml
    /// </summary>
    public partial class Editor_Ribbon : Window
    {
        Ribbon_Emitter ribbon;
        w3Model Model;
        public Editor_Ribbon(w3Node node, w3Model model)
        {
            InitializeComponent();
            Model = model;
            ribbon = node.Data as Ribbon_Emitter;
            FillData();
        }
        private void FillData()
        {
             
            fillMaterial();
            InputGravity.Text = ribbon.Gravity.ToString();
            InputRow.Text = ribbon.Rows.ToString();
            InputColumn.Text = ribbon.Rows.ToString();
            InputEmission.Text = ribbon.Emission_Rate.ToString();   
            InputLife.Text = ribbon.Life_Span.ToString();
            BoxAlpha.Text = ribbon.Alpha.GetStaticValue(1);
            BoxHeightAbove.Text = ribbon.Height_Above.GetStaticValue(1);
            BoxHeightBelow.Text = ribbon.Height_Below.GetStaticValue(1);
            BoxTextureSlot.Text = ribbon.Texture_Slot.GetStaticValue(1);
            Check_Vis.IsChecked = ribbon.Visibility.BoolValue;
            ButtonEditStaticColor.Background = Converters.BrushFromRGB(ribbon.Color.StaticValue);
            Check_Visibility.IsChecked =  ribbon.Visibility.isStatic == false;
            Check_HeightAbove.IsChecked = ribbon.Height_Above.isStatic == false;
            Check_HeightBelow.IsChecked = ribbon.Height_Below.isStatic == false;
            Check_TextureSlot.IsChecked = !ribbon.Visibility.isStatic;
            Check_Color.IsChecked = !ribbon.Color.isStatic;
            Check_Alpha.IsChecked = !ribbon.Alpha.isStatic;

        }
        private void fillMaterial()
        {
            bool found = Model.Materials.Any(x => x.ID == ribbon.Material_ID);


            foreach (w3Material material in Model.Materials)
            {
                List_Materials.Items.Add(new ComboBoxItem() { Content = material.ID.ToString() });
            }
            if (found)
            {
                int index = Model.Materials.FindIndex(x => x.ID == ribbon.Material_ID);
                List_Materials.SelectedIndex = index;
            }
            else
            {
                if (List_Materials.Items.Count > 0)
                {
                    List_Materials.SelectedIndex = 0;
                }
            }
        }

        private void Checked_Alpha(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxAlpha, Check_Alpha, ButtonEditAlpha, ribbon.Alpha);

        }

        private void SetAlpha(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxAlpha.Text, out float value);
            if (parsed)
            {
                ribbon.Alpha.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                ribbon.Alpha.StaticValue = [min];
            }
        }

        private void Checked_Visibility(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(Check_Vis, Check_Visibility, ButtonEditVisibility, ribbon.Visibility);

        }

        private void SetAnimatedVisibility(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Visibility, TransformationType.Visibility);
            tw.ShowDialog();
        }

        private void Checked_Intensity(object sender, RoutedEventArgs e)
        {

        }

        private void Checked_Color(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(ButtonEditStaticColor, Check_Color, ButtonEDitColor, ribbon.Color);

        }

        private void Checked_AttStart(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxHeightAbove, Check_HeightAbove, ButtonEditHeightAbove, ribbon.Height_Above);

        }

      

        private void Checked_AmIntensity(object sender, RoutedEventArgs e)
        {

        }

        private void SetAnimatedAlpha(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Alpha, TransformationType.Alpha);
            tw.ShowDialog();
        }

        private void SetMaterial(object sender, SelectionChangedEventArgs e)
        {
            ribbon.Material_ID = int.Parse( (List_Materials.SelectedItem as ComboBoxItem).Content.ToString());
        }

        private void SetVisible(object sender, RoutedEventArgs e)
        {

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
                 

                ribbon.Color.StaticValue = rgb;
                
            }
        }

        private void SetIntensity(object sender, TextChangedEventArgs e)
        {

        }

        private void SetAttentuantStart(object sender, TextChangedEventArgs e)
        {

        }

        private void SetAttentuanEnd(object sender, TextChangedEventArgs e)
        {

        }

        private void SetAnimatedColor(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Visibility, TransformationType.Color);
            tw.ShowDialog();
        }

        private void SetAnimatedAttStart(object sender, RoutedEventArgs e)
        {

        }

        private void SetAnimatedAttEnd(object sender, RoutedEventArgs e)
        {

        }

        private void SetAnimatedIntensity(object sender, RoutedEventArgs e)
        {

        }

        private void SetHEightAbove(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxHeightAbove.Text, out float value);
            if (parsed)
            {
                ribbon.Height_Above.StaticValue = value >= min && value <= max ? [value] : [0];
            }
            else
            {
                ribbon.Height_Above.StaticValue = [min];
            }
        }

        private void SetHeightBelow(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxHeightBelow.Text, out float value);
            if (parsed)
            {
                ribbon.Height_Below.StaticValue = value >= min && value <= max ? [value] : [0];
            }
            else
            {
                ribbon.Height_Below.StaticValue = [min];
            }
        }

        private void SetTextureSlot(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxTextureSlot.Text, out float value);
            if (parsed)
            {
                ribbon.Texture_Slot.StaticValue = value >= min && value <= max ? [value] : [0];
            }
            else
            {
                ribbon.Texture_Slot.StaticValue = [min];
            }
        }

        private void SetRows(object sender, TextChangedEventArgs e)
        {
            int min = 1;
            int max = 100000000;
            bool parsed = int.TryParse(InputRow.Text, out int value);
            if (parsed)
            {
                ribbon.Rows = value >= min && value <= max ? value : min;
            }
            else
            {
                ribbon.Rows = min;
            }
        }

        private void SetColumns(object sender, TextChangedEventArgs e)
        {
            int min = 1;
            int max = 100000000;
            bool parsed = int.TryParse(InputColumn.Text, out int value);
            if (parsed)
            {
                ribbon.Columns = value >= min && value <= max ? value : min;
            }
            else
            {
                ribbon.Columns = min;
            }
        }

        private void SetEmissionRate(object sender, TextChangedEventArgs e)
        {
            int min = 0;
            int max = 100000000;
            bool parsed = int.TryParse(InputEmission.Text, out int value);
            if (parsed)
            {
                ribbon.Emission_Rate = value >= min && value <= max ? value : min;
            }
            else
            {
                ribbon.Emission_Rate = min;
            }
        }

        private void SetLifespan(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = int.TryParse(InputLife.Text, out int value);
            if (parsed)
            {
                ribbon.Life_Span = value >= min && value <= max ? value : min;
            }
            else
            {
                ribbon.Life_Span = min;
            }
        }

        private void SetGravity(object sender, TextChangedEventArgs e)
        {
            float min = -100000;
            float max = -min;
            bool parsed = int.TryParse(InputGravity.Text, out int value);
            if (parsed)
            {
                ribbon.Gravity = value >= min && value <= max ? value : min;
            }
            else
            {
                ribbon.Gravity = min;
            }
        }

        private void Checked_HeightBelow(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxHeightBelow, Check_HeightBelow, ButtonEditHeightBelow, ribbon.Height_Below);

        }

        private void Checked_TextureSlot(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxTextureSlot, Check_TextureSlot, ButtonTextureSlot, ribbon.Texture_Slot);

        }

        private void SetAnimatedHeightAbove(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Height_Above, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetAnimatedHeightBelow(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Height_Below, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetAnimatedTextureSlot(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, ribbon.Texture_Slot, TransformationType.Int);
            tw.ShowDialog();
        }
    }
}
