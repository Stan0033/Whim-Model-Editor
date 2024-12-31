using MDLLib;
using SharpGL.SceneGraph.ParticleSystems;
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
    /// Interaction logic for Editor_Emitter1.xaml
    /// </summary>
    public partial class Editor_Emitter1 : Window
    {
        w3Model Model;
        Particle_Emitter_1 emitter;
        public Editor_Emitter1( w3Node node, w3Model model)
        {
            InitializeComponent();
            Model = model;
            emitter = node.Data as Particle_Emitter_1;
                FillData();
        }
        private void FillData()
        {
            BoxEmissionRate.Text = emitter.Emission_Rate.GetStaticValue(1);
            BoxGravity.Text = emitter.Gravity.GetStaticValue(1);
            BoxInitialVelocity.Text = emitter.Initial_Velocity.GetStaticValue(1);
            BoxLatitude.Text = emitter.Latitude.GetStaticValue(1);
            BoxLongtitude.Text = emitter.Longitude.GetStaticValue(1);
            BoxVisibility.IsChecked = emitter.Visibility.BoolValue;
            BoxLifespan.Text = emitter.Life_Span.GetStaticValue(1);

            Check_AnimatedEmissionRate.IsChecked = emitter.Emission_Rate.isStatic = false;
            Check_AnimatedLifespan.IsChecked = emitter.Life_Span.isStatic = false;
            Check_AnimatedInitialVelocity.IsChecked = emitter.Initial_Velocity.isStatic = false;
            Check_AnimatedGravity.IsChecked = emitter.Gravity.isStatic = false;
            Check_Longtitude.IsChecked = emitter.Longitude.isStatic = false;
            Check_Latitude.IsChecked = emitter.Latitude.isStatic = false;
            Check_Visbility.IsChecked = emitter.Visibility.isStatic = false;
        }
        private void SetPartocle(object sender, TextChangedEventArgs e)
        {
            emitter.Particle_Filename = BoxParticle.Text.Trim();
           
        }

        private void SetLongtitude(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxLongtitude.Text, out float value);
            if (parsed)
            {
                emitter.Longitude.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Longitude.StaticValue = [min];
            }
        }

        private void Checked_Longtitude(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxLongtitude, Check_Longtitude, ButtonLongtitude, emitter.Longitude);
        }

        private void SetLifespan(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxLifespan.Text, out float value);
            if (parsed)
            {
                emitter.Life_Span.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Life_Span.StaticValue = [min];
            }
        }

        private void Checked_AnimatedGravity(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxGravity, Check_AnimatedGravity, ButtonAnimatedGravity, emitter.Gravity);

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
     
        private void SetEmissionRate(object sender, RoutedEventArgs e)
        {

        }

        private void SetAnimatedVisibility(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Visibility, TransformationType.Visibility);
            tw.ShowDialog();
        }

        private void SetAnimatedLatitude(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Latitude, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetAnimatedGravity(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Emission_Rate, TransformationType.Float);
            tw.ShowDialog();
          
        }

        private void Checked_Visbility(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxVisibility, Check_Visbility, buttonAnimatedVisibility, emitter.Visibility);

        }

        private void Checked_Tag1(object sender, RoutedEventArgs e)
        {
            emitter.Emitter_Uses_MDL = Check_Tag1.IsChecked == true;
        }

        private void Checked_Tag2(object sender, RoutedEventArgs e)
        {
            emitter.Emitter_Uses_TGA = Check_Tag2.IsChecked == true;
        }

        private void Checked_Latitude(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxLatitude, Check_Latitude, ButtonLatitude, emitter.Latitude);

        }

        private void Checked_AnimatedLifespan(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxLifespan, Check_AnimatedLifespan, ButtonEditAnimatedLifespan, emitter.Life_Span);

        }

        private void Checked_AnimatedEmissionRate(object sender, RoutedEventArgs e)
        {
            DialogAnimateChecker.SwitchAnimated(BoxEmissionRate, Check_AnimatedEmissionRate, ButtonEditEmissionRate, emitter.Emission_Rate);

        }

       

        private void SetLatitude(object sender, TextChangedEventArgs e)
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

        private void SetInitialVelocity(object sender, TextChangedEventArgs e)
        {
            float min = 0;
            float max = 100000000;
            bool parsed = float.TryParse(BoxInitialVelocity.Text, out float value);
            if (parsed)
            {
                emitter.Initial_Velocity.StaticValue = value >= 0 && value <= max ? [value] : [0];
            }
            else
            {
                emitter.Initial_Velocity.StaticValue = [min];
            }
        }

        private void SetGravity(object sender, TextChangedEventArgs e)
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

        private void SetAnimatedLifespan(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Life_Span, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetAnimatedInitialVelocity(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Initial_Velocity, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetLongtitude(object sender, RoutedEventArgs e)
        {
            
        }

        private void SetEmissionRateAnimate(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Emission_Rate, TransformationType.Float);
            tw.ShowDialog();
        }

        private void SetLongtitudeAnimated(object sender, RoutedEventArgs e)
        {
            Transformation_window tw = new Transformation_window(Model, emitter.Longitude, TransformationType.Visibility);
            tw.ShowDialog();
        }

        private void SetVisibility(object sender, RoutedEventArgs e)
        {
            emitter.Visibility.StaticValue = BoxVisibility.IsChecked == true ? [1] : [0];

        }
    }
}
