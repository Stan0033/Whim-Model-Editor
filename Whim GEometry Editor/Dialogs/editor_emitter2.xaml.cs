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
    /// Interaction logic for editor_emitter2.xaml
    /// </summary>
    public partial class editor_emitter2 : Window
    {
        w3Model Model;
        Particle_Emitter_2 emitter;
        public editor_emitter2(w3Model model, w3Node node)
        {
            InitializeComponent();
            emitter = node.Data as Particle_Emitter_2;
            Model = model;
            SetData();
        }
        private void SetData()
        {
            Check_Visibility.IsChecked = emitter.Visibility.isStatic;
            Check_AnimatedVisibility.IsChecked = emitter.Visibility.isStatic == false;
            BoxEmissionRate.Text = emitter.Emission_Rate.StaticValue[0].ToString();
            Check_AnimatedEmissionRate.IsChecked = emitter.Emission_Rate.isStatic == false;
            BoxSpeed.Text = emitter.Speed.StaticValue[0].ToString();
            Check_AnimatedSpeed.IsChecked = emitter.Speed.isStatic == false;
            BoxVariation.Text = emitter.Variation.StaticValue[0].ToString();
            Check_AnimatedVariation.IsChecked = emitter.Variation.isStatic == false;
            BoxLatitude.Text = emitter.Latitude.StaticValue[0].ToString();
            Check_AnimatedLatitude.IsChecked = emitter.Latitude.isStatic == false;
            BoxWidth.Text = emitter.Width.StaticValue[0].ToString();
            Check_AnimatedWidth.IsChecked = emitter.Width.isStatic == false;
            BoxLEngth.Text = emitter.Length.StaticValue[0].ToString();
            Check_AnimatedLength.IsChecked = emitter.Length.isStatic == false;
            BoxGravity.Text = emitter.Gravity.StaticValue[0].ToString();
            Check_Gravity.IsChecked = emitter.Gravity.isStatic == false;
            BoxRows.Text = emitter.Rows.ToString();
            BoxColumns.Text = emitter.Columns.ToString();
            BoxLifespan.Text = emitter.Life_Span.ToString();
            BoxPriorityPlane.Text = emitter.Priority_Plane.ToString();
            BoxReplaceAbleID.Text = emitter.Replaceable_ID.ToString();
            BoxTime.Text = emitter.Time.ToString();
            BoxTailLength.Text = emitter.Tail_Length.ToString();
            foreach (w3Texture tex in Model.Textures)
            {
                Combo_Textures.Items.Add(new ComboBoxItem() { Content = tex.Path });
            }
            Combo_Textures.SelectedIndex = Model.Textures.FindIndex(x=>x.ID == emitter.Texture_ID);
            PopulateComboBoxWithEnum<FilterMode>(Combo_FilterMode);
            SelectEnumValueInComboBox<FilterMode>(Combo_FilterMode, emitter.Filter_Mode);
            Check_Unshaded.IsChecked = emitter.Unshaded;
            Check_Unfogged.IsChecked = emitter.Unfogged;
            Check_XYQuad.IsChecked = emitter.XY_Quad;
            Check_LineEmitter.IsChecked = emitter.Line_Emitter;
            Check_Sort.IsChecked = emitter.Sort_Primitives_Far_Z;
            Check_ModelSpace.IsChecked = emitter.Model_Space;
            Check_AlphaKey.IsChecked = emitter.AlphaKey;
            Check_Squirt.IsChecked = emitter.Squirt;
            Check_Head.IsChecked = emitter.Head;
            Check_Tail.IsChecked = emitter.Tail;
            ButtonAlpha.Text = ByteToPercent((int)emitter.ALpha_Segment1).ToString();
            ButtonAlpha2.Text = ByteToPercent((int)emitter.ALpha_Segment2).ToString();
            ButtonAlpha3.Text = ByteToPercent((int)emitter.ALpha_Segment3).ToString();
            BoxScaling1.Text = emitter.Scaling_Segment1.ToString();
            BoxScaling2.Text = emitter.Scaling_Segment2.ToString();
            BoxScaling3.Text = emitter.Scaling_Segment3.ToString();
            BoxHeadLifespanStart.Text = emitter.Head_Lifespan_Start.ToString();
            BoxHeadLifespanEnd.Text = emitter.Head_Lifespan_End.ToString();
            BoxHeadLifespanRepeat.Text = emitter.Head_Lifespan_Repeat.ToString();
            BoxHeadDecayStart.Text = emitter.Head_Decay_Start.ToString();
                BoxHeadDecayEnd.Text = emitter.Head_Decay_End.ToString();
                BoxHeadDecayRepeat.Text = emitter.Head_Decay_Repeat.ToString();
            BoxTailLifespanStart.Text = emitter.Tail_Lifespan_Start.ToString();
                BoxTailLifespanEnd.Text = emitter.Tail_Lifespan_End.ToString() ;
                BoxTailLifespanREpeat.Text = emitter.Tail_Lifespan_Repeat.ToString() ;
                BoxTailDecayStart.Text = emitter.Tail_Decay_Start.ToString();
            BoxTailDecayEnd.Text = emitter.Tail_Decay_End.ToString();
            BoxTailDecayREpeat.Text = emitter.Tail_Decay_Repeat.ToString();
            ButtonColor1.Background = Converters.BrushFromRGB(emitter.Color_Segment1);
            ButtonColor2.Background = Converters.BrushFromRGB(emitter.Color_Segment2);
            ButtonColor3.Background = Converters.BrushFromRGB(emitter.Color_Segment3);
            
        }
        public static void PopulateComboBoxWithEnum<T>(ComboBox comboBox) where T : Enum
        {
            comboBox.ItemsSource = Enum.GetValues(typeof(T));
        }
        public static void SelectEnumValueInComboBox<T>(ComboBox comboBox, T enumValue) where T : Enum
        {
            comboBox.SelectedItem = enumValue;
        }

        private void Checked_Color(object sender, RoutedEventArgs e)
        {

        }

        private void SetAnimatedVisibility(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Visibility, TransformationType.Visibility);
            tw.ShowDialog();
        }

        private void SetVisibility(object sender, RoutedEventArgs e)
        {

        }

        private void Checked_AnimatedEmissionRate(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxEmissionRate, Check_AnimatedEmissionRate, ButtonEmissionRate, emitter.Emission_Rate);

        }

        private void SetAnimatedEmissionRate(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Emission_Rate, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetSpeed(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxSpeed.Text, out float value);
            if (parsed)
            {
                emitter.Speed.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Speed.StaticValue = [min];
            }
        }

        private void Checked_AnimatedSpeed(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxSpeed, Check_AnimatedSpeed, ButtonEditSpeed, emitter.Speed);

        }

        private void SetAnimatedSpeed(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Speed, TransformationType.Float);
            tw.ShowDialog();
        }

        private void Checked_AnimatedVariation(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxVariation, Check_AnimatedVariation, ButtonAnimatedVariation, emitter.Variation);

        }

        private void SetAnimatedVar(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Variation, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetLAtitude(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxLatitude.Text, out float value);
            if (parsed)
            {
                emitter.Latitude.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Latitude.StaticValue = [min];
            }
        }

        private void Checked_AnimatedLatitude(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxLatitude, Check_AnimatedLatitude, ButtonAnimatedLatitude, emitter.Latitude);

        }

        private void SetAnimatedLatitude(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Latitude, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetWith(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxWidth.Text, out float value);
            if (parsed)
            {
                emitter.Width.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Width.StaticValue = [min];
            }
        }

        private void Checked_AnimatedWidth(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxWidth, Check_AnimatedWidth, ButtonAnimatedWidth, emitter.Width);

        }

        private void SetAnimatedWidth(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Width, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetAnimatedLength(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Length, TransformationType.Float);
            tw.ShowDialog();
        }

        private void Checked_Gravity(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxGravity, Check_Gravity, ButtonAnimateGravity, emitter.Gravity);

        }

        private void SetAnimatedGravity(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Gravity, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SelectedTexture(object sender, SelectionChangedEventArgs e)
        {
            emitter.Texture_ID = Model.Textures[Combo_Textures.SelectedIndex].ID;
        }

        private void SelectedFilterMode(object sender, SelectionChangedEventArgs e)
        {
            emitter.Filter_Mode = (FilterMode)Combo_FilterMode.SelectedIndex;

        }

        private void Checked_Unshaded(object sender, RoutedEventArgs e)
        {
            emitter.Unshaded = Check_Unshaded.IsChecked == true;
        }

        private void Checked_Unfogged(object sender, RoutedEventArgs e)
        {
            emitter.Unfogged = Check_Unfogged.IsChecked == true;
        }

        private void Checked_AlphaKey(object sender, RoutedEventArgs e)
        {
            emitter.AlphaKey = Check_AlphaKey.IsChecked == true;
        }

        private void Checked_LineEmitter(object sender, RoutedEventArgs e)
        {
            emitter.Line_Emitter = Check_LineEmitter.IsChecked == true;
        }

        private void Checked_Sort(object sender, RoutedEventArgs e)
        {
            emitter.Sort_Primitives_Far_Z = Check_Sort.IsChecked == true;
        }

        private void Checked_ModelSpace(object sender, RoutedEventArgs e)
        {
            emitter.Model_Space = Check_ModelSpace.IsChecked == true;
        }

        private void Checked_XYQuad(object sender, RoutedEventArgs e)
        {
            emitter.XY_Quad = Check_XYQuad.IsChecked == true;
        }

        private void Checked_Squirt(object sender, RoutedEventArgs e)
        {
            emitter.Squirt = Check_Squirt.IsChecked == true;
        }

        private void Checked_Head(object sender, RoutedEventArgs e)
        {
            emitter.Head = Check_Head.IsChecked == true;
            BoxHeadLifespanStart.IsEnabled = emitter.Head;
            BoxHeadLifespanEnd.IsEnabled = emitter.Head;
            BoxHeadLifespanRepeat.IsEnabled = emitter.Head;
            BoxHeadDecayStart.IsEnabled = emitter.Head;
            BoxHeadDecayEnd.IsEnabled = emitter.Head;
            BoxHeadDecayRepeat.IsEnabled = emitter.Head;
        }

        private void Checked_Tail(object sender, RoutedEventArgs e)
        {
            emitter.Tail = Check_Tail.IsChecked == true;
            BoxTailLifespanStart.IsEnabled = emitter.Tail;
            BoxTailLifespanEnd.IsEnabled = emitter.Tail;
            BoxTailLifespanREpeat.IsEnabled = emitter.Tail;
            BoxTailDecayStart.IsEnabled = emitter.Tail;
            BoxTailDecayEnd.IsEnabled = emitter.Tail;
            BoxTailDecayREpeat.IsEnabled = emitter.Tail;
            BoxTailLength.IsEnabled = emitter.Tail;
        }

        private void SetColor1(object sender, RoutedEventArgs e)
        {
            var initialColor = Converters.BrushToColor(ButtonColor1.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ButtonColor1.Background = Converters.ColorToBrush(dialog.Color);
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);


                emitter.Color_Segment1 = rgb;

            }
        }

        private void SetAlpha1(object sender, TextChangedEventArgs e)
        {
            int min = 0;
            int max = 100;
            bool parsed = int.TryParse(ButtonAlpha.Text, out int value);
            if (parsed)
            {
                emitter.ALpha_Segment1 = value >= 0 && value <= max ? PercentToByte(value) : min;
            }
            else
            {
                emitter.ALpha_Segment1 = min;
            }
        }
        public int PercentToByte(float percent)
        {
            return (int)(percent * 255 / 100);
        }

        public float ByteToPercent(int byteValue)
        {
            return byteValue * 100f / 255;
        }

        private void SetScaling1(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxScaling1.Text, out float value);
            if (parsed)
            {
                emitter.Scaling_Segment1 = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Scaling_Segment1 = min;
            }

        }

        private void SetColor2(object sender, RoutedEventArgs e)
        {
            var initialColor = Converters.BrushToColor(ButtonColor2.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ButtonColor2.Background = Converters.ColorToBrush(dialog.Color);
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);


                emitter.Color_Segment2 = rgb;

            }
        }

        private void SetAlpha2(object sender, TextChangedEventArgs e)
        {
            int min = 0;
            int max = 100;
            bool parsed = int.TryParse(ButtonAlpha2.Text, out int value);
            if (parsed)
            {
                emitter.ALpha_Segment2 = value >= 0 && value <= max ? PercentToByte(value) : min;
            }
            else
            {
                emitter.ALpha_Segment2 = min;
            }
        }

        private void SetScaling2(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxScaling2.Text, out float value);
            if (parsed)
            {
                emitter.Scaling_Segment2 = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Scaling_Segment2 = min;
            }
        }

        private void SetColor3(object sender, RoutedEventArgs e)
        {
            var initialColor = Converters.BrushToColor(ButtonColor3.Background);
            var dialog = new ColorPickerDialog(initialColor);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ButtonColor3.Background = Converters.ColorToBrush(dialog.Color);
                float[] rgb = Converters.ColorToFloatArray(dialog.Color);


                emitter.Color_Segment3 = rgb;

            }
        }

        private void SetAlpha3(object sender, TextChangedEventArgs e)
        {
            int min = 0;
            int max = 100;
            bool parsed = int.TryParse(ButtonAlpha3.Text, out int value);
            if (parsed)
            {
                emitter.ALpha_Segment3= value >= 0 && value <= max ? PercentToByte(value) : min;
            }
            else
            {
                emitter.ALpha_Segment3 = min;
            }
        }

        private void SetScaling3(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxScaling3.Text, out float value);
            if (parsed)
            {
                emitter.Scaling_Segment3 = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Scaling_Segment3 = min;
            }
        }

        private void SetRows(object sender, TextChangedEventArgs e)
        {
            int min = 1;
            int max = 100000000;
            bool parsed = int.TryParse(BoxRows.Text, out int value);
            if (parsed)
            {
                emitter.Rows = value >= 0 && value <= max ? value : 1;
            }
            else
            {
                emitter.Rows = min;
            }
        }

        private void SetColumn(object sender, TextChangedEventArgs e)
        {
            int min = 1;
            int max = 100000000;
            bool parsed = int.TryParse(BoxColumns.Text, out int value);
            if (parsed)
            {
                emitter.Columns = value >= 0 && value <= max ? value : 1;
            }
            else
            {
                emitter.Columns = min;
            }
        }

        private void SetLifespan(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxLifespan.Text, out float value);
            if (parsed)
            {
                emitter.Life_Span = value >= 0 && value <= max ? value : 1;
            }
            else
            {
                emitter.Life_Span = min;
            }
        }

        private void SetPP(object sender, TextChangedEventArgs e)
        {
            int min = 0;
            int max = 100000000;
            bool parsed = int.TryParse(BoxPriorityPlane.Text, out int value);
            if (parsed)
            {
                emitter.Priority_Plane = value >= 0 && value <= max ? value : 1;
            }
            else
            {
                emitter.Time = min;
            }
        }

        private void SetRID(object sender, TextChangedEventArgs e)
        {

        }
      

        private void SetTime(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 1;
            bool parsed = float.TryParse(BoxTime.Text, out float value);
            if (parsed)
            {
                emitter.Time = value >= 0 && value <= max ? value : 1;
            }
            else
            {
                emitter.Time = min;
            }
        }

        private void SetTailLen(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailLength.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Length = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Length = min;
            }
        }

        private void SetHeadLifespanStart(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadLifespanStart.Text, out float value);
            if (parsed)
            {
                emitter.Head_Lifespan_Start = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Lifespan_Start = min;
            }
        }

        private void SetHeadLifespanEnd(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadLifespanEnd.Text, out float value);
            if (parsed)
            {
                emitter.Head_Lifespan_End = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Lifespan_End = min;
            }
        }

        private void SetHeadLifespanRepeat(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadLifespanRepeat.Text, out float value);
            if (parsed)
            {
                emitter.Head_Lifespan_Repeat = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Lifespan_Repeat = min;
            }
        }

        private void SetHeadDecayStart(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadDecayStart.Text, out float value);
            if (parsed)
            {
                emitter.Head_Decay_Start = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Decay_Start = min;
            }
        }

        private void SetHeadDecayEnd(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadDecayEnd.Text, out float value);
            if (parsed)
            {
                emitter.Head_Decay_End = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Decay_End = min;
            }
        }

        private void SetHeadDecayRepeat(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxHeadDecayRepeat.Text, out float value);
            if (parsed)
            {
                emitter.Head_Decay_Repeat = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Head_Decay_Repeat = min;
            }
        }

        private void SetTailLifespanStart(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailLifespanStart.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Lifespan_Start = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Lifespan_Start = min;
            }
        }

        private void SetTailLifespanEnd(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailLifespanEnd.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Lifespan_End = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Lifespan_End = min;
            }
        }

        private void SetTailLifespanRepeat(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailLifespanREpeat.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Lifespan_Repeat = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Lifespan_Repeat = min;
            }
        }

        private void SetTailDecayStart(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailDecayStart.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Decay_Start = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Decay_Start = min;
            }
        }

        private void SetTailDecayEnd(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailDecayEnd.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Decay_End = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Decay_End = min;
            }
        }

        private void SetTailDecayRepeat(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000;
            bool parsed = float.TryParse(BoxTailDecayREpeat.Text, out float value);
            if (parsed)
            {
                emitter.Tail_Decay_Repeat = value >= 0 && value <= max ? value : min;
            }
            else
            {
                emitter.Tail_Decay_Repeat = min;
            }
        }

        private void Checked_AnimatedLength(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxLEngth, Check_AnimatedLength, ButtonAnimatedLength, emitter.Length);

        }

        private void SetEmissionRate(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxEmissionRate.Text, out float value);
            if (parsed)
            {
                emitter.Emission_Rate.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Emission_Rate.StaticValue = [min];
            }
        }

        private void SetGRavity(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxGravity.Text, out float value);
            if (parsed)
            {
                emitter.Gravity.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Gravity.StaticValue = [min];
            }
        }

        private void SetLength(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxLEngth.Text, out float value);
            if (parsed)
            {
                emitter.Length.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Length.StaticValue = [min];
            }
        }

        private void SetVariation(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxLatitude.Text, out float value);
            if (parsed)
            {
                emitter.Latitude.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Latitude.StaticValue = [min];
            }
        }

        private void Checked_Visibility(object sender, RoutedEventArgs e)
        {
            emitter.Visibility.StaticValue = Check_Visibility.IsChecked == true ? [1] : [0];
        }

        private void Checked_AnimatedVisibility(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(Check_Visibility, Check_AnimatedVisibility,ButtonEditVisibility, emitter.Visibility);
        }
    }
}
