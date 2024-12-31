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
using W3_Texture_Finder;


namespace Whim_GEometry_Editor
{

    enum ErrorSeverity
    {
        Unused, Warning, Severe, Error, Passed
    }
    enum DefaultSequenceNames
    {
        Stand, Death, Decay, Birth, Spell, Attack, Swim, Morph, Dissipate, Upgrade, Portrait, Walk
    }
    /// <summary>
    /// Interaction logic for ErrorChecker.xaml
    /// </summary>
    public partial class ErrorChecker : Window
    {
        public static bool StartsWithEnumMemberWithWhitespace(string input)
        {
            string name = input.ToLower();
            foreach (var enumName in Enum.GetNames(typeof(DefaultSequenceNames)))
            {
                string eName = enumName.ToLower();
                if (eName == name) { return true; }
                else
                {
                    if (name.StartsWith(eName + " ")) { return true; }
                }
            }
            return false;
        }
        Brush yellow = new SolidColorBrush(Color.FromArgb(255, (byte)240, (byte)204, (byte)0));
        private int CountErrors = 0;
        private void Add(string error, ErrorSeverity severityType, string tooltip)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = error;
            switch (severityType)
            {
                case ErrorSeverity.Unused: textBlock.Foreground = Brushes.Green; break;
                case ErrorSeverity.Warning: textBlock.Foreground = yellow; break;
                case ErrorSeverity.Severe: textBlock.Foreground = Brushes.DarkOrange; break;
                case ErrorSeverity.Error: textBlock.Foreground = Brushes.DarkRed; break;
            }
            textBlock.ToolTip = tooltip;
            textBlock.FontSize = 16;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.FontFamily = new FontFamily("Liberation Serif");
            Title = $"Error Checker - {++CountErrors}";
            Holder.Children.Add(textBlock);
        }
            public ErrorChecker(w3Model model)
        {
            List<TextBlock> errors = new List<TextBlock>();
            InitializeComponent();
            model.RefreshTransformationsList();
            if (model.Nodes.Any(x => x.Data.GetType().Name == typeof(w3Attachment).Name && x.Name.ToLower() == "origin ref") == false)
            {
                Add("Missing the origin attachment point", ErrorSeverity.Warning, "all units use this attachment point. If you try to attach anything, nothign will show.");
            }
            if (model.Materials.Count == 0) { Add("No materials", ErrorSeverity.Warning, "without materials, all geosets will be pitch black"); }
            if (model.Textures.Count == 0) { Add("No textures", ErrorSeverity.Warning, "without textures, all geosets will be pitch black"); }
            if (model.Sequences.Count == 0) { Add("No sequences", ErrorSeverity.Warning, "The model will not be animated"); }
            foreach (w3Material mat in model.Materials) { if (mat.Layers.Count == 0) { Add($"Material {mat.ID}: no layers", ErrorSeverity.Warning, "without layers, all geosets will be pitch black"); } }
            //----------------------------------------------------------
            //--check for unused - texture, material, global sequence, texture animation
            //----------------------------------------------------------
            foreach (w3Texture tx in model.Textures)
            {
               if (!MPQHelper.FileExists(tx.Path) && tx.Replaceable_ID == 0)
                {
                    Add($"Texture \"{tx.Path}\" was not found in the MPQs. Texture \"\" was not found in the MPQs. If it's not imported with the model at this exact path, the whole model will NOT be visible in-game.", ErrorSeverity.Warning, "");

                }
                bool used = false;
                foreach (w3Material mat in model.Materials)
                {
                    foreach (w3Layer l in mat.Layers)
                    {
                        if (l.Diffuse_Texure_ID.isStatic && l.Diffuse_Texure_ID.StaticValue[0] == tx.ID) { used = true; break; }
                        if (l.Diffuse_Texure_ID.isStatic == false &&
                            l.Diffuse_Texure_ID.Keyframes.Any(x => x.Data[0] == tx.ID)) { used = true; break; }
                    }
                    if (used) { break; }
                }
                foreach (w3Node node in model.Nodes)
                {
                    if (node.Data is Particle_Emitter_2)
                    {
                        Particle_Emitter_2 pe = (Particle_Emitter_2)node.Data;
                        if (pe.Texture_ID == tx.ID) { used = true; break; }
                    }
                }
                if (!used)
                {
                    if (tx.Replaceable_ID != 0)
                    {
                        Add($"Texture \"replaceableID {tx.Replaceable_ID}\" is unused", ErrorSeverity.Unused, "");
                    }
                    else
                    {
                        Add($"Texture \"{tx.Path}\" is unused", ErrorSeverity.Unused, "");
                    }
                }
            }
            foreach (w3Material mat in model.Materials)
            {
                if (model.Geosets.Any(x => x.Material_ID == mat.ID) == false)
                {
                    Add($"Material {mat.ID}: is unused", ErrorSeverity.Unused, "");
                }
            }
            foreach (w3Texture_Animation tx in model.Texture_Animations)
            {
                foreach (w3Material mat in model.Materials)
                {
                    foreach (w3Layer l in mat.Layers)
                    {
                        if (l.Animated_Texture_ID == tx.ID) { goto next; }
                    }
                }
                Add($"Texture animation  {tx.ID}: is unused", ErrorSeverity.Unused, "");
            next: continue;
            }
            foreach (w3Global_Sequence gs in model.Global_Sequences)
            {
                if (model.Transformations.Any(x => x.Global_Sequence_ID == gs.ID) == false)
                {
                    Add($"Global sequence {gs.ID}: is unused", ErrorSeverity.Unused, "");
                }
            }
            //----------------------------------------------------------
            //----------------------------------------------------------
            foreach (w3Texture_Animation tx in model.Texture_Animations)
            {
                if (tx.Translation.isStatic == true && tx.Rotation.isStatic == true && tx.Rotation.isStatic == true)
                {
                    Add($"Texture animation  {tx.ID} is not animated", ErrorSeverity.Unused, "");
                }
            next: continue;
            }
            foreach (w3Geoset geo in model.Geosets)
            {
                if (geo.Triangles.Count == 0)
                {
                    { Add($"Geoset {geo.ID}: no triangles", ErrorSeverity.Warning, "no triangles = invisible geoset"); }
                }
                if (geo.Vertices.Count == 0)
                {
                    { Add($"Geoset {geo.ID}: no vertices", ErrorSeverity.Error, "Will crash the map"); }
                }
                if (geo.SequenceExtents.Count != model.Sequences.Count) { Add($"Geoset {geo.ID}: number of sequence extents does not match the number of sequences", ErrorSeverity.Warning, "health bar problems, camera clipping problems, pathfinding problems, selecting problems"); }
                // check if referenced by multiple geoset animations
                int times = 0;
                foreach (w3Geoset_Animation ga in model.Geoset_Animations) { if (ga.Geoset_ID == ga.ID) { times++; } }
                if (times > 1)
                {
                    Add($"Geoset {geo.ID}: referenced by more than 1 geoset animation", ErrorSeverity.Warning, "Could confuse the engine and render some geosets less visible");
                }
                // check for invalid material
                int matid = geo.Material_ID;
                if (model.Materials.Any(x => x.ID == matid) == false)
                {
                    Add($"Geoset {geo.ID}: invalid material", ErrorSeverity.Severe, "Pitch black geoset");
                }
                // check for vertices attached to invalid nodes
                foreach (w3Vertex v in geo.Vertices)
                {
                    if (v.AttachedTo.Count == 0)
                    {
                        Add($"Geoset {geo.ID}: vertex {v.Id}: Not attached to anything", ErrorSeverity.Severe, "will crash the map");
                        continue;
                    }
                    foreach (int id in v.AttachedTo)
                    {
                        if (model.Nodes.Any(x => x.objectId == id) == false)
                        {
                            Add($"Geoset {geo.ID}: vertex {v.Id}: attached to non-existing node", ErrorSeverity.Severe, "will crash the map");
                            continue;
                        }
                        else
                        {
                            if ((model.Nodes.First(x => x.objectId == id).Data is Bone) == false)
                            {
                                Add($"Geoset {geo.ID}: vertex {v.Id}: attached to node that is not a bone", ErrorSeverity.Severe, "Map crash or not displaying in-game depending on the game version");
                                continue;
                            }
                        }
                    }
                }
                // check for 0-area triangles
                for (int i = 0; i < geo.Triangles.Count; i++)
                {


                    w3Triangle tr = geo.Triangles[i];
                    Coordinate c1 = tr.Vertex1.Position;
                    Coordinate c2 = tr.Vertex2.Position;
                    Coordinate c3 = tr.Vertex3.Position;
                    if (IsZeroArea(c1, c2, c3))
                    {
                        Add($"Geoset {geo.ID}: Triangle {i} has no area", ErrorSeverity.Unused, "");
                    }
                }
            }
            if (model.Geosets.Count != model.Geoset_Animations.Count) { Add("The number of geoset animations does not match the number of geosets", ErrorSeverity.Warning, "some geosets might have visibility problems"); }
            foreach (w3Sequence sequence in model.Sequences)
            {
                if (sequence.From == sequence.To) { Add($"Sequence {sequence.Name}: zero length", ErrorSeverity.Unused, "could cause visual glitches if there are transformatiosn associated with it"); }
                if (sequence.From > sequence.To) { Add($"Sequence {sequence.Name}: from is bigger than to", ErrorSeverity.Severe, "Invalid sequence. The model will not be visible in-game"); }
                if (StartsWithEnumMemberWithWhitespace(sequence.Name.ToLower()) == false) { Add($"Sequence {sequence.Name}: does not start with any of the default names for a sequence", ErrorSeverity.Warning, "The only way to play it is with trigger"); }
            }

            foreach (w3Transformation t in model.Transformations)
            {
                foreach (w3Keyframe kf in t.Keyframes)
                {
                    if (KeyframeUsed(model.Sequences, kf.Track) == false)
                    {
                        { Add($"{t.BelongsTo}: keyframe {kf.Track} is not in any sequence", ErrorSeverity.Unused, ""); }
                    }
                }
                // interpolation for certain transformations must be none
                if (t.Type == TransformationType.Visibility)
                {
                    if (t.Interpolation_Type  != InterpolationType.DontInterp)
                    {
                        Add($"{t.BelongsTo}: interpolation type for visibility transformation not set to none", ErrorSeverity.Warning, "must be only true or false, visible or not visible, 1 or 0");
                    }
                }
                // check for inconsistent keyframes
                // check for missing opening/closing track
                foreach (w3Keyframe keyframe in t.Keyframes)
                {
                    string name = FindSequenceNameFromFrame(keyframe.Track);
                    if (HasOpeningTrack(model.Sequences, t.Keyframes, keyframe.Track) == false)
                    { Add($"{t.BelongsTo}:   missing opening track for sequence {name}", ErrorSeverity.Severe, "animation will not work properly"); }
                    if (HasClosinggTrack(model.Sequences, t.Keyframes, keyframe.Track) == false)
                    { Add($"{t.BelongsTo}:   missing closing track for sequence {name}", ErrorSeverity.Severe, "animation will not work properly"); }
                }
                
            }
            foreach (w3Geoset_Animation geoset_Animation in model.Geoset_Animations)
            {
                int gID = geoset_Animation.Geoset_ID;
                // check for invisible geoset anim
                if (model.Geosets.Any(x => x.ID == gID) == false) { Add($"Geoset animation {geoset_Animation.ID} is referencing invalid geoset", ErrorSeverity.Error, "Will crash the map"); }
                // check for invisible gas
                if (geoset_Animation.Alpha.isStatic == true && geoset_Animation.Alpha.StaticValue[0] <= 0.1)
                {
                    Add($"Geoset animation {geoset_Animation.ID} is not visible", ErrorSeverity.Warning, "Its geoset will not be visible");
                }
                if (geoset_Animation.Alpha.isStatic == true)
                {
                    int invis = 0;
                    foreach (w3Keyframe k in geoset_Animation.Alpha.Keyframes)
                    {
                        if (k.Data[0] <= 10) { invis++; }
                        if (invis == geoset_Animation.Alpha.Keyframes.Count)
                        {
                            Add($"Geoset animation {geoset_Animation.ID} is not visible in all keyframes", ErrorSeverity.Warning, "Its geoset will not be visible");
                        }
                    }
                }
                 
            }
            foreach (w3Transformation t in model.Transformations)
            {
                if (t.isStatic == false && t.Keyframes.Count == 0)
                {
                   if (t.Type == TransformationType.Translation ||  t.Type == TransformationType.Rotation || t.Type == TransformationType.Scaling) { continue; }
                    Add($"Marked as animated but no keyframes", ErrorSeverity.Unused, "Will probably take a default value");
                }
            }
            foreach (w3Sequence s in model.Sequences)
            {
                bool hasAnims = false;
                foreach (w3Transformation t in model.Transformations)
                {
                    foreach (w3Keyframe kf in t.Keyframes)
                    {
                        if (kf.Track >= s.From && kf.Track <= s.To) { hasAnims = true; }
                        if (hasAnims) { break; }
                    }
                    if (hasAnims) { break; }
                }
                if (hasAnims == false) { Add($"Sequence \"{s.Name}\" does not contain animations", ErrorSeverity.Unused, ""); }
            }
            if (SequenceOrderCorrect(model.Sequences) == false)
            {
                Add($"Inconsistent order of sequences", ErrorSeverity.Error, "Will not be visible in-game");
            }
            List<string> overlapping = FindOverlappingSequences(model.Sequences);
            foreach (string overlap in overlapping)
            {
                Add($"Sequence \"{overlap}\" is overlapping with another sequence", ErrorSeverity.Error, "Disruption in the animation");
            }
            string FindSequenceNameFromFrame(int frame)
            {
                foreach (w3Sequence sequence in model.Sequences)
                {
                    if (frame >= sequence.From && frame <= sequence.To) { return sequence.Name; }
                }
                return "";
            }
            // check for invalid paths  ]
            if (model.Sequences.Any(x => x.Name.ToLower().StartsWith("stand")) == false)
            {
                Add("Missing stand sequence", ErrorSeverity.Severe, "Stand sequence is the default sequence for a unit or decoration in idle state. Without it, it will be not animated while doing nothing.");
            }
            if (model.Sequences.Any(x => x.Name.ToLower().StartsWith("death")) == false)
            {
                Add("Missing death sequence", ErrorSeverity.Severe, "objects without death sequence will remain un-animated when they die until their timer removes them from the game");
            }
            foreach (w3Node node in model.Nodes)
            {
                if (node.Data is w3Attachment)
                {
                    w3Attachment a = (w3Attachment)node.Data;
                    if (a.Path.Length == 0) { continue; }
                    if (MPQHelper.FileExists(a.Path) == false) { Add($"Attachment \"{node.Name}\": path does not exist", ErrorSeverity.Error, ""); }
                }
                if (node.Data is Particle_Emitter_1)
                {
                    Particle_Emitter_1 a = (Particle_Emitter_1)node.Data;
                    if (a.Particle_Filename.Length == 0) { continue; }
                    if (MPQHelper.FileExists(a.Particle_Filename) == false) { Add($"Particle emitter \"{node.Name}\": path does not exist", ErrorSeverity.Error, ""); }
                }
                // time middle
                if (node.Data is Particle_Emitter_2)
                {
                    Particle_Emitter_2 a = (Particle_Emitter_2)node.Data;
                    if (a.Time < 0 || a.Time > 1)
                    {
                        Add($"Particle emitter 2 \"{node.Name}\": time middle must be between 0 and 1", ErrorSeverity.Severe, "");
                    }
                    if (a.Squirt == true && a.Emission_Rate.isStatic == true)
                    {
                        Add($"Particle emitter 2 \"{node.Name}\": using squirt without animating the emission rate", ErrorSeverity.Severe, "");
                    }
                    if (a.XY_Quad == true)
                    {
                        if (a.Speed.isStatic == true && a.Speed.StaticValue[0] == 0)
                        {
                            Add($"Particle emitter 2 \"{node.Name}\": XY Quad emitters must have a non-zero speed and latitude", ErrorSeverity.Severe, "");
                        }
                        if (a.Latitude.isStatic == true && a.Latitude.StaticValue[0] == 0)
                        {
                            Add($"Particle emitter 2 \"{node.Name}\": XY Quad emitters must have a non-zero speed and latitude", ErrorSeverity.Severe, "");
                        }
                    }
                }
                if (node.Data is Event_Object)
                {
                    Event_Object a = (Event_Object)node.Data;
                    if (a.Tracks.Count == 0)
                    {
                        Add($"Event object \"{node.Name}\": zero tracks", ErrorSeverity.Error, "Map crash");
                    }
                }
                if (node.Data is Helper)
                {
                    bool Has = model.Nodes.Any(x => x.parentId == node.objectId);
                    if (!Has)
                    {
                        Add($"Helper \"{node.Name}\" has no children", ErrorSeverity.Unused, "");
                    }
                }
                if (node.Data is Bone)
                {
                    bool attached = false;
                    foreach (w3Geoset geo in model.Geosets)
                    {
                        foreach (w3Vertex v in geo.Vertices)
                        {
                            if (v.AttachedTo.Contains(node.objectId) == true) { attached = true; break; }
                        }
                        if (attached) { break; }
                    }
                    if (!attached)
                    {
                        Add($"There are no vertices attached to bone \"{node.Name}\"", ErrorSeverity.Warning, "");
                    }
                }
            }
            if (Holder.Children.Count == 0) { Add("Passed", ErrorSeverity.Passed, "Your model is working flawlessly"); } else { }
        }
        public bool IsZeroArea(Coordinate v1, Coordinate v2, Coordinate v3)
        {
            // Calculate the vectors from v1 to v2 and from v1 to v3
            var vectorA = new Coordinate(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v1.Z);
            var vectorB = new Coordinate(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v1.Z);
            // Calculate the cross product of vectorA and vectorB
            var crossProduct = new Coordinate(
                vectorA.Y * vectorB.Z - vectorA.Z * vectorB.Y,
                vectorA.Z * vectorB.X - vectorA.X * vectorB.Z,
                vectorA.X * vectorB.Y - vectorA.Y * vectorB.X
            );
            // If the cross product is a zero vector, the area is zero
            return crossProduct.X == 0 && crossProduct.Y == 0 && crossProduct.Z == 0;
        }
        public bool SequenceOrderCorrect(List<w3Sequence> sequences)
        {
            for (int i = 1; i < sequences.Count; i++)
            {
                // Check if current sequence's 'From' and 'To' values are greater than the previous sequence's 'To'
                if (sequences[i].From <= sequences[i - 1].To || sequences[i].To <= sequences[i - 1].To)
                {
                    return false;
                }
                // Also ensure 'From' is less than 'To' for the current sequence
                if (sequences[i].From >= sequences[i].To)
                {
                    return false;
                }
            }
            return true;
        }
        public List<string> FindOverlappingSequences(List<w3Sequence> sequences)
        {
            List<string> overlappingSequenceNames = new List<string>();
            // Sort the sequences based on the "from" value
            sequences.Sort((x, y) => x.From.CompareTo(y.From));
            // Check for overlaps
            for (int i = 1; i < sequences.Count; i++)
            {
                if (sequences[i].From < sequences[i - 1].To)
                {
                    overlappingSequenceNames.Add(sequences[i].Name);
                    overlappingSequenceNames.Add(sequences[i - 1].Name);
                }
            }
            return overlappingSequenceNames;
        }
        private bool HasOpeningTrack(List<w3Sequence> sqs, List<w3Keyframe> kfs, int frame)
        {
            // find index of containing sequnece
            int sqStart = -1;
            foreach (w3Sequence sequence in sqs)
            {
                if (frame >= sequence.From && frame <= sequence.To) { sqStart = sequence.From; break; }
            }
            if (sqStart == -1) { return true; }
            return kfs.Any(x => x.Track == sqStart);
        }
        private bool HasClosinggTrack(List<w3Sequence> sqs, List<w3Keyframe> kfs, int frame)
        {
            // find index of containing sequnece
            int sqEnd = -1;
            foreach (w3Sequence sequence in sqs)
            {
                if (frame >= sequence.From && frame <= sequence.To) { sqEnd = sequence.To; break; }
            }
            if (sqEnd == -1) { return true; }
            return kfs.Any(x => x.Track == sqEnd);
        }
        private bool KeyframeUsed(List<w3Sequence> ss, int k)
        {
            foreach (w3Sequence s in ss)
            {
                if (k >= s.From && k <= s.To) return true;
            }
            return false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { DialogResult = false; }
        }
    }
    }
