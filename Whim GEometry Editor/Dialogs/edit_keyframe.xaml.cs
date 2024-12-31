using MDLLib;
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
    /// Interaction logic for edit_keyframe.xaml
    /// </summary>
    public partial class edit_keyframe : Window
    {
        w3Node EditedNode;
        int EditedKeyframe;
        private float T1,T2, T3, R1, R2, R3, S1, S2, S3;
        public edit_keyframe(MDLLib.w3Node node, int keyframe)
        {
            InitializeComponent();
            EditedNode = node;
            EditedKeyframe = keyframe;
            FillKeyframe();
            Title = $"Editing keyframe {keyframe} of node \"{node.Name}\"";

        }

        private void FillKeyframe()
        {
            if (EditedNode.Translation.Keyframes.Any(x=>x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Translation.Keyframes.First(x => x.Track == EditedKeyframe);
                TranslationXTextBox.Text = kf.Data[0].ToString();
                TranslationYTextBox.Text = kf.Data[1].ToString();
                TranslationZTextBox.Text = kf.Data[2].ToString();
            }

            if (EditedNode.Rotation.Keyframes.Any(x => x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Rotation.Keyframes.First(x => x.Track == EditedKeyframe);
                RotationXTextBox.Text = kf.Data[0].ToString();
                RotationYTextBox.Text = kf.Data[1].ToString();
                RotationZTextBox.Text = kf.Data[2].ToString();
            }
            if (EditedNode.Scaling.Keyframes.Any(x => x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Scaling.Keyframes.First(x => x.Track == EditedKeyframe);
                ScaleXTextBox.Text = kf.Data[0].ToString();
                ScaleYTextBox.Text = kf.Data[1].ToString();
                ScaleZTextBox.Text = kf.Data[2].ToString();
            }
        }

        private void SetKeyframes()
        {
            if (EditedNode.Translation.Keyframes.Any(x => x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Translation.Keyframes.First(x => x.Track == EditedKeyframe);
                kf.Data = [T1, T2, T3];
            }
            else
            {
                w3Keyframe kf = new w3Keyframe(EditedKeyframe, [T1, T2, T3], [0,0,0], [0,0,0]);
                EditedNode.Translation.Keyframes.Add(kf);
            }
            if (EditedNode.Rotation.Keyframes.Any(x => x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Rotation.Keyframes.First(x => x.Track == EditedKeyframe);
                kf.Data = [R1, R2, R3];
            }
            else
            {
                w3Keyframe kf = new w3Keyframe(EditedKeyframe, [R1, R2, R3], [0, 0, 0], [0, 0, 0]);
                EditedNode.Translation.Keyframes.Add(kf);
                EditedNode.Translation.Keyframes.OrderBy(x => x.Track);
            }
            if (EditedNode.Scaling.Keyframes.Any(x => x.Track == EditedKeyframe))
            {
                w3Keyframe kf = EditedNode.Scaling.Keyframes.First(x => x.Track == EditedKeyframe);
                kf.Data = [S1, S2, S3];
                EditedNode.Rotation.Keyframes.OrderBy(x => x.Track);
            }
            else
            {
                w3Keyframe kf = new w3Keyframe(EditedKeyframe, [S1, S2, S3], [0, 0, 0], [0, 0, 0]);
                EditedNode.Scaling.Keyframes.Add(kf);
                EditedNode.Scaling.Keyframes.OrderBy(x=>x.Track);
            }
        }
        private bool InputCorrect()
        {
            bool parset1 = float.TryParse(TranslationXTextBox.Text, out float t1);
            bool parset2 = float.TryParse(TranslationYTextBox.Text, out float t2);
            bool parset3 = float.TryParse(TranslationZTextBox.Text, out float t3);
            bool parser1 = float.TryParse(RotationXTextBox.Text, out float r1);
            bool parser2 = float.TryParse(RotationYTextBox.Text, out float r2);
            bool parser3 = float.TryParse(RotationZTextBox.Text, out float r3);
            bool parses1 = float.TryParse(RotationXTextBox.Text, out float s1);
            bool parses2 = float.TryParse(RotationYTextBox.Text, out float s2);
            bool parses3 = float.TryParse(RotationZTextBox.Text, out float s3);

            if (!parset1 || !parset2 || !parset3 || parser1 || parser2 || parser3 || parses1 || parses2 || parses3) { return false; }
            
            if (r1 < -360 || r1 > 360)return false;
            if (r2 < -360 || r2 > 360)return false;
            if (r3 < -360 || r3 > 360)return false;
            T1 = t1;
            T2 = t2;
            T3 = t3;
            R1 = r1;
            R2 = r2;
            R3 = r3;
            S1 = s1;
            S2 = s2;
            S3 = s3;

            return true;
        }
        private void ok(object sender, RoutedEventArgs e)
        {
            if (InputCorrect())
            {
              
                SetKeyframes();
                DialogResult = true;
            }
        }
    }
}
