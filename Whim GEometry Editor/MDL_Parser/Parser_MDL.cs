using MDLLib;
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;


namespace test_parser_mdl
{
    public static partial class Parser_MDL
    {
        public static bool ParsingFailed = false;
        static int calledpp = 0;

     

        //-------------------------------------------------------
        // split the categories to be processed by the parser later
        //-------------------------------------------------------
        private static List<Token> Version = new List<Token>();
        private static List<Token> Model = new List<Token>();
        private static List<Token> Sequences = new List<Token>();
        private static List<Token> Textures = new List<Token>();
        private static List<Token> Geosets = new List<Token>();
        private static List<Token> GeosetAnims = new List<Token>();
        private static List<Token> Materials = new List<Token>();

        private static List<Token> GlobalSequences = new List<Token>();
        private static List<Token> PivotPoints = new List<Token>();
        private static List<Token> Cameras = new List<Token>();
        private static List<Token> Nodes = new List<Token>();
        private static List<Token> TextureAnims = new List<Token>();
        //-------------------------------------------------------

        enum MDL_Root_Keyword
        {
            Version,// main category
            Model,// main category
            Sequences,// main category
            Textures,// main category
            Geoset,// main category
            GeosetAnim,// main category
            Materials,// main category
            TextureAnims,// main category
            GlobalSequences, // main category
            Camera,// main category
            PivotPoints,// main category
            //+++++++++++++++++++++++++++++++++++++++
            Bone, // main category
            Helper,// main category
            ParticleEmitter2,// main category
            ParticleEmitter,// main category
            Light,// main category
            Attachment,// main category
            RibbonEmitter,// main category
            EventObject,// main category
            CollisionShape,// main category
        }
        enum MDL_Keyword
        {
            AnimationFile,
            DontInterp,
            //---------------------------
            Version,// main category
            Model,// main category
            Sequences,// main category
            Textures,// main category
            Geoset,// main category
            GeosetAnim,// main category
            Materials,// main category
            TextureAnims,// main category
            GlobalSequences, // main category
            Camera,// main category
            PivotPoints,// main category
            //+++++++++++++++++++++++++++++++++++++++
            Bone, // main category
            Helper,// main category
            ParticleEmitter2,// main category
            ParticleEmitter,// main category
            Light,// main category
            Attachment,// main category
            RibbonEmitter,// main category
            EventObject,// main category
            CollisionShape,// main category
                           //+++++++++++++++++++++++++++++++++++++++

            //------------------------------------------
            DropShadow,
            Image, // textures>bitmap>image
            Layer, // Materials>Material>Layer
                   // static for values
            Triangles, // geoset>fast>triangles
            Matrices, // geoset>groups

            Material,
            Anim,
            Bitmap, // textures>bitmap
            // static for values
            //geoset:
            //--------------------------------
            Vertices,
            Normals,
            TVertices,
            Faces,
            Groups,
            MaterialID,
            SelectionGroup,
            BoundsRadius,
            //-----------------------
            //node
            ObjectId,
            Translation,
            Rotation,
            Scaling,
            Directional,
            Ambient,
            AttachmentID,
            Unselectable,
            InTan,
            OutTan,
            AlphaKey,
            LineEmitter,
            ModelSpace,
            Both,
            FormatVersion,
            NumLights,
            BlendTime,
            NumHelpers,
            NumAttachments,
            NumEvents,
            NumParticleEmitters,
            NumParticleEmitters2,
            NumRibbonEmitters,
            NumGeosets,
            NumGeosetAnims,
            NumBones,
            MinimumExtent,
            MaximumExtent,
            Interval,
            MoveSpeed,
            NonLooping,
            Rarity,
            Duration,
            WrapWidth,
            WrapHeight,
            FilterMode,
            Transparent,
            Blend,
            Modulate,
            Additive,
            AddAlpha,
            TextureID,
            TwoSided,
            Unshaded,
            Unfogged,
            SphereEnvMap,
            NoDepthTest,
            NoDepthSet,
            ConstantColor,
            GeosetId,
            Multiple,
            GeosetAnimId,
            None,

            Omnidirectional,
            AttenuationStart,
            AttenuationEnd,
            Color,
            Intensity,
            AmbIntensity,
            Visibility,
            EmissionRate,
            Gravity,
            Longitude,
            Variation,
            Width,
            Length,
            SegmentColor,
            Alpha,
            ParticleScaling,

            LifeSpanUVAnim,
            DecayUVAnim,
            TailUVAnim,
            TailDecayUVAnim,
            Rows,
            Columns,
            HeightAbove,
            HeightBelow,
            TextureSlot,
            Latitude,
            Particle,
            LifeSpan,
            InitVelocity,
            Path,
            Speed,
            AmbColor,
            Static,
            Box,
            ReplaceableId,
            TVertexAnimId,
            TVertexAnim,
            Linear,
            Bezier,
            Hermite,
            GlobalSeqId,
            VertexGroup,

            Parent,
            Billboarded,
            BillboardedLockZ,
            BillboardedLockY,
            BillboardedLockX,
            Time,
            TailLength,
            Head,
            SortPrimsFarZ,
            Squirt,
            PriorityPlane,

            FullResolution,
            Tail,
            XYQuad,
            EventTrack,
            FieldOfView,
            FarClip,
            NearClip,
            Position,
            Target,
            Sphere,
            DontInherit,
            CameraAnchored,

        }
        enum Keywords_Root
        {
            //---------------------------
            Version,// main category
            Model,// main category
            Sequences,// main category
            Textures,// main category
            Geoset,// main category
            GeosetAnim,// main category
            Materials,// main category
            TextureAnims,// main category
            GlobalSequences, // main category
            Camera,// main category
            PivotPoints,// main category
            //+++++++++++++++++++++++++++++++++++++++
            Bone, // main category
            Helper,// main category
            ParticleEmitter2,// main category
            ParticleEmitter,// main category
            Light,// main category
            Attachment,// main category
            RibbonEmitter,// main category
            EventObject,// main category
            CollisionShape,// main category
            //+++++++++++++++++++++++++++++++++++++++
        }
     
        static List<string> AlreadyMetCategories = new List<string>();
        
        private static bool KeywordPresent(string Value)
        {
            if (Enum.TryParse<MDL_Keyword>(char.ToUpper(Value[0]) + Value.Substring(1), out var parsedType)) { return true; }

            return false;
        }
        private static bool RootKeywordPresent(string Value)
        {
            if (Enum.TryParse<MDL_Root_Keyword>(char.ToUpper(Value[0]) + Value.Substring(1), out var parsedType)) { return true; }

            return false;
        }
        public static List<TemporaryObject> SplitCollectObjects(List<Token> tokens)
        {
            int Nesting = 0;


            //-------------------------------------
            // check for valid keywords
            //-------------------------------------
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type != TokenType.Keyword) continue;
                // check if all keywords are correct
                if (KeywordPresent(tokens[i].Value) == false && i > 0) MessageBox.Show($"Error at parser:\nAt line {tokens[i].LineNumber} after \"{tokens[i - 1]}\" unexpected keyword \"{tokens[i].Value}\"");

            }
            //-------------------------------------
            // split
            //-------------------------------------
            List<TemporaryObject> objects = new List<TemporaryObject>();
            TemporaryObject current = new TemporaryObject();
            string kword = "";
            string value = "";
            bool stat = false;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (stat)
                {
                    if (tokens[i].Type == TokenType.ClosingBrace) { stat = false; }
                    continue;
                }
                if (Nesting == 0)
                {
                    if (tokens[i].Type == TokenType.Keyword)
                    {
                        if (tokens[i].Value == "static") { stat = true; continue; }
                        kword = tokens[i].Value;
                    }
                    else if (tokens[i].Type == TokenType.String)
                    {
                        value = tokens[i].Value; continue;
                    }
                    else if (tokens[i].Type == TokenType.Number)
                    {
                        value = tokens[i].Value; continue;
                    }
                    else if (tokens[i].Type == TokenType.OpeningBrace)
                    {
                        current.Name = kword;

                        stat = false;
                        current.Value = value;
                        Nesting++; continue;
                    }
                }
                else
                {


                    if (tokens[i].Type == TokenType.ClosingBrace)
                    {
                        Nesting--; stat = false;
                        if (Nesting > 0)
                        {

                            current.Tokens.Add(tokens[i]);
                        }

                    }
                    else if (tokens[i].Type == TokenType.OpeningBrace)
                    {
                        Nesting++;
                        if (Nesting > 1)
                        {
                            current.Tokens.Add(tokens[i]);
                        }
                    }
                    else
                    {

                        current.Tokens.Add(tokens[i]);
                    }
                    if (Nesting == 0)
                    {

                        objects.Add(current.Clone());
                        stat = false;
                        current = new TemporaryObject();  // Reset current for the next object

                    }
                }
            }


            return objects;
        }

        
        public static w3Model  Parse(List<TemporaryObject> tempObjects)
        {
            
            w3Model model = new w3Model();
            
            foreach (TemporaryObject temp in tempObjects)
            {
              
                if (temp.Name == MDL_Keyword.Version.ToString())
                { if (ParseVersion(model, temp.Tokens) == false) { ParsingFailed = true;  return new w3Model(); } continue; }
                 
              
                if (temp.Name == MDL_Keyword.Model.ToString())
                { if (ParseModel(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true;   return new w3Model(); } continue; }
                
              
                if (temp.Name == MDL_Keyword.Textures.ToString())
                   
                { if (ParseTextures(model, temp.Tokens) == false) { ParsingFailed = true;   return new w3Model(); } continue; }
                
                if (temp.Name == MDL_Keyword.Materials.ToString())
                {
                    if (ParseMaterials(model, temp.Tokens) == false) { ParsingFailed = true;   return new w3Model();    }continue;
                }
                
                if (temp.Name == MDL_Keyword.Geoset.ToString())
                { if (ParseGeoset(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.GeosetAnim.ToString())
                { if (ParseGeosetAnim(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Sequences.ToString())
                { if (ParseSequences(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.GlobalSequences.ToString())
                { if (ParseGlobalSequences(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.TextureAnims.ToString())
                { if (ParseTextureAnims(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Camera.ToString())
                { if (ParseCamera(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.PivotPoints.ToString())
                { if (ParsePivotPoints(model, temp.Tokens) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Bone.ToString())
                { if (ParseBone(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Helper.ToString())
                { if (ParseHelper(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.CollisionShape.ToString())
                { if (ParseCollisionShape(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.EventObject.ToString())
                { if (ParseEventObject(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.ParticleEmitter.ToString())
                { if (ParseParticleEmitter(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.ParticleEmitter2.ToString())
                { if (ParseParticleEmitter2(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.RibbonEmitter.ToString())
                { if (ParseRibbonEmitter(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Light.ToString())
                { if (ParseLight(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }
                if (temp.Name == MDL_Keyword.Attachment.ToString())
                { if (ParseAttachment(model, temp.Tokens, temp.Value) == false) { ParsingFailed = true; return new w3Model(); } continue; }

 
            }





            return model;
        }
        //ready
        private static bool ParseVersion(w3Model model, List<Token> tokens)
        {
            if (tokens.Count < 3) { return false; }
            if (tokens[0].Value != MDL_Keyword.FormatVersion.ToString())
            {
                MessageBox.Show($"At line {tokens[0].LineNumber}: at model: Expected \"Modelversion\", but got \"{tokens[0].Type}\"");
                return false;
            }
            if (tokens[1].Value != "800")
            {
                MessageBox.Show("Only model version 800 is supported"); return false;
            }
            if (tokens[2].Type != TokenType.Comma)
            {
                MessageBox.Show($"At line {tokens[0].LineNumber}: at version: Expected \",\", but got \"{tokens[0].Type}\"");
                return false;
            }
            model.ModelVersion = 800;
            return true;
        }
        //ready
        private static bool ParseModel(w3Model model, List<Token> tokens, string name)
        {
            model.Name = name;
          
            string CurrentProperty = "";
            bool inVector = false; // Track if we are inside a vector {x, y, z}

            List<float> vector = new List<float>();
            int vectorIndex = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Value == "BlendTime")
                {
                    model.BlendTime = int.Parse(tokens[i+1].Value);
                }
                if (tokens[i].Value == "AnimationFile")
                {
                    model.AnimationFile =  tokens[i].Value;
                }
                if (tokens[i].Value == "MinimumExtent")
                {
                    model.Extents.Minimum_X = float.Parse(tokens[i+2].Value);
                    model.Extents.Minimum_Y = float.Parse(tokens[i+4].Value);
                    model.Extents.Minimum_Z = float.Parse(tokens[i+6].Value);
                }
                if (tokens[i].Value == "MaximumExtent")
                {
                    model.Extents.Maximum_X = float.Parse(tokens[i+2].Value);
                    model.Extents.Maximum_Y = float.Parse(tokens[i+4].Value);
                    model.Extents.Maximum_Z = float.Parse(tokens[i+6].Value);
                }
            
            }
            return true;
        }

        //ready
        private static bool ParseSequences(w3Model model, List<Token> objects)
        {
            List<w3Sequence> sequences = new List<w3Sequence>();
            List<TemporaryObject> temporaryObjects = SplitCollectObjects(objects); // Assumes you split objects correctly
            foreach (TemporaryObject temporaryObject in temporaryObjects)
            {
                if (temporaryObject.Name == "Anim")
                {
                    w3Sequence s = new w3Sequence();
                    s.Name = temporaryObject.Value;


                    for (int i = 0; i < temporaryObject.Tokens.Count; i++)
                    {
                        switch (temporaryObject.Tokens[i].Value)
                        {
                            case "Rarity": s.Rarity = float.Parse(temporaryObject.Tokens[i+1].Value); break;
                            case "MoveSpeed": s.Move_Speed = float.Parse(temporaryObject.Tokens[i+1].Value);  break;
                            case "BoundsRadius": s.Extent.Bounds_Radius = float.Parse(temporaryObject.Tokens[i+1].Value); break;
                            case "Interval":
                                int one = int.Parse(temporaryObject.Tokens[i+2].Value); 
                                int two = int.Parse(temporaryObject.Tokens[i+4].Value); 
                                s.From = one;
                                s.To = two;
                                break;
                            case "NonLooping": s.Looping = false; break;
                            case "MinimumExtent":
                                float minx = float.Parse(temporaryObject.Tokens[i + 2].Value);
                                float miny = float.Parse(temporaryObject.Tokens[i + 4].Value);
                                float minz = float.Parse(temporaryObject.Tokens[i + 6].Value);
                                s.Extent.Minimum_X = minx;
                                s.Extent.Minimum_Y = miny;
                                s.Extent.Minimum_Z = minz;
                                break;
                            case "MaximumExtent":
                                float maxx = float.Parse(temporaryObject.Tokens[i + 2].Value);
                                float maxy = float.Parse(temporaryObject.Tokens[i + 4].Value);
                                float maxz = float.Parse(temporaryObject.Tokens[i + 6].Value);
                                s.Extent.Maximum_X = maxx;
                                s.Extent.Maximum_Y = maxy;
                                s.Extent.Maximum_Z = maxz;
                                break;
                        }
                    }
                    sequences.Add(s); // Add the parsed sequence
                }
                else
                {
                    MessageBox.Show($"At line {temporaryObject.Tokens[0].LineNumber}: expected anim but got '{temporaryObject.Value}' "); ParsingFailed = true;break;
                }

                
            }

            model.Sequences = sequences;
            return true; // Successfully parsed the sequences

        }
        //ready
        private static bool ParseGlobalSequences(w3Model model, List<Token> objects)
        {
            List<w3Global_Sequence> globalSequences = new List<w3Global_Sequence>(); // Assuming global sequences are stored as durations (integers)

            
            string CurrentProperty = "";

            foreach (Token token in objects)
            {
                if (token.Type == TokenType.Number)
                {
                    model.Global_Sequences.Add(new w3Global_Sequence(int.Parse(token.Value)));
                }
                 
            }

            
            return true; // Parsing was successful
        }


        //ready

        private static bool ParsePivotPoints(w3Model model, List<Token> objects)
        {
            if (calledpp >= 1) { return true; }
            calledpp++;
            //MessageBox.Show(string.Join("", objects.Select(x=>x.Value)));

            List<float> nums = new List<float>();
            foreach (Token t in objects)
            {
                if (t.Type == TokenType.Number)
                {
                    nums.Add(float.Parse(t.Value));
                }
            }
            if (nums.Count % 3 != 0)
            {
                MessageBox.Show($"Inconsistent count of pivot points: got {nums.Count}"); return false;
            }
            List<Coordinate> pivotPoints = new List<Coordinate>(); // Store the parsed pivot points
            for (int i = 0; i < nums.Count; i += 3)
            {
                pivotPoints.Add(new Coordinate(nums[i], nums[i + 1], nums[i + 2]));
            }

            // After parsing all pivot points, assign them to the model
            model.PivotPoints = pivotPoints;
            return true; // Parsing was successful
        }

        // ready
        private static bool ParseTextures(w3Model model, List<Token> tokens)
        {
            List<TemporaryObject> bitmaps = SplitCollectObjects(tokens);
          
                foreach (TemporaryObject bitmap in bitmaps)
            {
                if (bitmap.Name == MDL_Keyword.Bitmap.ToString())
                {
                    w3Texture texture = new w3Texture();
                    for (int i = 0; i < bitmap.Tokens.Count; i++)
                    {
                        if (bitmap.Tokens[i].Value == "Image") { texture.Path = bitmap.Tokens[i + 1].Value; }
                        if (bitmap.Tokens[i].Value == "WrapWidth") { texture.Wrap_Width = true; }
                        if (bitmap.Tokens[i].Value == "WrapHeight") { texture.Wrap_Height = true; }
                        if (bitmap.Tokens[i].Value == "ReplaceableId") { texture.Replaceable_ID = int.Parse(bitmap.Tokens[i+1].Value); }
                    }
                    model.Textures.Add(texture.Clone());
                }
                    
            }
            return true;
        }



        // done
        private static bool ParseTextureAnims(w3Model model, List<Token> objects)
        {
            List<TemporaryObject> temporaryObjects = SplitCollectObjects(objects);

            foreach (TemporaryObject temporaryObject1 in temporaryObjects)
            {
                if (temporaryObject1.Name == MDL_Keyword.TVertexAnim.ToString())
                {
                    w3Texture_Animation anim = new w3Texture_Animation();
                    List<TemporaryObject> temporaryObjects2 = SplitCollectObjects(temporaryObject1.Tokens);

                    foreach (TemporaryObject temporaryObject2 in temporaryObjects2)
                    {
                        w3Transformation transformation = new w3Transformation();

                        if (temporaryObject2.Name == MDL_Keyword.Translation.ToString())
                        {
                            transformation.Type = TransformationType.Translation;
                            if (!ParseTransformation(transformation, temporaryObject2.Tokens, 3, false))
                                return false;
                            anim.Translation = transformation;
                        }
                        else if (temporaryObject2.Name == MDL_Keyword.Rotation.ToString())
                        {
                            transformation.Type = TransformationType.Rotation;
                            if (!ParseTransformation(transformation, temporaryObject2.Tokens, 4, false))
                                return false;
                            anim.Rotation = transformation;
                        }
                        else if (temporaryObject2.Name == MDL_Keyword.Scaling.ToString())
                        {
                            transformation.Type = TransformationType.Scaling;
                            if (!ParseTransformation(transformation, temporaryObject2.Tokens, 3, false))
                                return false;
                            anim.Scaling = transformation;
                        }
                        else
                        {
                            MessageBox.Show($"At line {temporaryObject2.Tokens[0].LineNumber}: unrecognized keyword {temporaryObject2.Name}");
                            ParsingFailed = true;
                            return false;
                        }
                    }

                    model.Texture_Animations.Add(anim);
                }
                else
                {
                    MessageBox.Show($"At line {temporaryObject1.Tokens[0].LineNumber}: unrecognized keyword {temporaryObject1.Name}");
                    ParsingFailed = true;
                    return false;
                }
            }

            return true;
        }
        // done
        private static bool ParseTransformation(w3Transformation transformation, List<Token> tokens, int ExpectedValuesPerKeyframe, bool sttic = false)
        {
             
            InterpolationType interpolation = InterpolationType.DontInterp;
            transformation.isStatic = sttic;
            bool hasTangents = false;

            // get interpolation type and global seq id
            for (int i = 0; i<tokens.Count; i++)
            {
                Token token = tokens[i];
                if (token.Type == TokenType.Keyword)
                {
                    switch (token.Value)
                    {
                        case "DontInterp":
                            transformation.Interpolation_Type = InterpolationType.DontInterp;

                            break;
                        case "Linear":
                            transformation.Interpolation_Type = InterpolationType.Linear;
                            break;
                        case "Hermite":
                            transformation.Interpolation_Type = InterpolationType.Hermite;   hasTangents= true;
                            break;
                        case "Bezier":
                            transformation.Interpolation_Type = InterpolationType.Bezier;  hasTangents=true;
                            break;
                        case "GlobalSeqId":
                            transformation.Global_Sequence_ID = int.Parse(tokens[i+1].Value);
                            continue;
                        case "InTan":
                        case "OutTan":
                            break;  
                        default:
                            MessageBox.Show($"At line {token.LineNumber}: unrecognized property {token.Value}");
                            return false;
                    }
                }
            }
            // get keyframes

 
            transformation.Keyframes = ParseKeyframes( tokens, ExpectedValuesPerKeyframe, hasTangents);
            

                return true;
        }

       
        enum KeyframeBuildMode
        {
            Track, Data, Intan,Outtan 
        }
        private static List<w3Keyframe> ParseKeyframes(List<Token> tokens, int expectedValuesPerKeyframe, bool HasTangents)
        {
            List<w3Keyframe> list = new List<w3Keyframe>();
            w3Keyframe k = new w3Keyframe();  // Current keyframe being parsed
            List<float> data = new List<float>();
            List<float> intan = new List<float>();
            List<float> outtan = new List<float>();
            KeyframeBuildMode mode = KeyframeBuildMode.Track;

            void FinalizeCheck()
            {
                // Check if the necessary data has been fully collected for keyframe finalization
                if (HasTangents)
                {
                    if (data.Count == expectedValuesPerKeyframe &&
                        intan.Count == expectedValuesPerKeyframe &&
                        outtan.Count == expectedValuesPerKeyframe)
                    {
                        k.Data = data.ToArray();
                        k.InTan = intan.ToArray();
                        k.OutTan = outtan.ToArray();
                        list.Add(k.Clone());

                        // Reset for the next keyframe
                        data.Clear();
                        intan.Clear();
                        outtan.Clear();
                        k = new w3Keyframe();
                        mode = KeyframeBuildMode.Track;
                    }
                }
                else
                {
                    if (data.Count == expectedValuesPerKeyframe)
                    {
                        k.Data = data.ToArray();
                        list.Add(k.Clone());

                        // Reset for the next keyframe
                        data.Clear();
                        k = new w3Keyframe();
                        mode = KeyframeBuildMode.Track;
                    }
                }
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];

                if (t.Type == TokenType.Keyword)
                {
                    // Switch to parsing InTan or OutTan if the keywords are encountered
                    if (t.Value == "InTan")
                    {
                        mode = KeyframeBuildMode.Intan;
                        continue;
                    }
                    if (t.Value == "OutTan")
                    {
                        mode = KeyframeBuildMode.Outtan;
                        continue;
                    }
                }

                if (t.Type == TokenType.Number)
                {
                    if (mode == KeyframeBuildMode.Track)
                    {
                        // Set the track number, and switch to parsing data
                        k.Track = int.Parse(t.Value);
                        mode = KeyframeBuildMode.Data;
                        continue;
                    }
                    if (mode == KeyframeBuildMode.Data)
                    {
                        // Collect the transformation data values but stop when the limit is reached
                        if (data.Count < expectedValuesPerKeyframe)
                        {
                            data.Add(float.Parse(t.Value));
                        }
                        continue;
                    }
                    if (mode == KeyframeBuildMode.Intan)
                    {
                        // Collect the InTan values but stop when the limit is reached
                        if (intan.Count < expectedValuesPerKeyframe)
                        {
                            intan.Add(float.Parse(t.Value));
                        }
                        continue;
                    }
                    if (mode == KeyframeBuildMode.Outtan)
                    {
                        // Collect the OutTan values but stop when the limit is reached
                        if (outtan.Count < expectedValuesPerKeyframe)
                        {
                            outtan.Add(float.Parse(t.Value));
                        }
                        continue;
                    }
                }

                // Finalize a keyframe when the required values have been collected and a transition occurs
                if (t.Type == TokenType.Comma || i == tokens.Count - 1)
                {
                    // Only finalize after parsing everything for the current keyframe
                    FinalizeCheck();
                }
            }

            return list;
        }


        //done
        private static bool ParseGeosetAnim(w3Model model, List<Token> objects)
        {
            w3Geoset_Animation ga = new w3Geoset_Animation();


            for (int i = 0; i< objects.Count; i++)
            {
                Token t = objects[i];

                if (t.Value == "DropShadow")
                {
                    ga.Drop_Shadow = true;
                }

                if (t.Value == "GeosetId")
                {
                    ga.Geoset_ID = int.Parse(objects[i+1].Value);
                }
                if (t.Value == "static")
                {
                   if (objects[i+1].Value == "Alpha")
                    {
                        ga.Alpha.isStatic = true;
                        ga.Alpha.StaticValue = [float.Parse(objects[i+2].Value)];
                    }
                    if (objects[i + 1].Value == "Color")
                    {
                        float one = float.Parse(objects[i + 3].Value);
                        float two = float.Parse(objects[i + 5].Value);
                        float three = float.Parse(objects[i + 7].Value);
                        ga.Use_Color = true;
                        ga.Color.isStatic = true;
                        ga.Color.StaticValue = [one,two,three];
                    }
                }
            
            }
            List<TemporaryObject> temps = SplitCollectObjects(objects);
            foreach (TemporaryObject temp in temps)
            {
                if (temp.Name == "Alpha")
                {
                    ParseTransformation(ga.Alpha, temp.Tokens, 1, false);
                   
                }
                if (temp.Name == "Color")
                {
                    ParseTransformation(ga.Color, temp.Tokens, 3, false);
                    ga.Color.isStatic = false;
                }

            }
              model.Geoset_Animations.Add(ga);
            return true;

        }


        private static float[] ParseVector(Token token)
        {
            string[] values = token.Value.Trim(new char[] { '{', '}' }).Split(',');
            return values.Select(v => float.Parse(v.Trim())).ToArray();
        }

        //--------------------------------------------
        static bool ParseMaterials(w3Model model, List<Token> objects)
        {

            List<TemporaryObject> Materials = SplitCollectObjects(objects);

            foreach (TemporaryObject material in Materials)
            {
                if (material.Name != MDL_Keyword.Material.ToString())
                {
                    MessageBox.Show($"At {material.Tokens[0].LineNumber}: At materials: Expected material");
                    return false;
                }
                w3Material m = new w3Material();

                if (material.Name == MDL_Keyword.Material.ToString())
                {


                    int Curly = 0;
                    
                    // parse properties

                    for (int i = 0; i < material.Tokens.Count; i++)
                    {
                        Token token = material.Tokens[i];
                        if (token.Type == TokenType.OpeningBrace) { Curly++; continue; }
                        if (token.Type == TokenType.ClosingBrace) { Curly--; continue; }
                        if (token.Type != TokenType.Keyword) {  continue; }
                        if (Curly != 0) { continue; }
                        switch (token.Value)
                        {
                            case "ConstantColor": m.Constant_Color = true; break;
                            case "SortPrimsFarZ": m.Sort_Primitives_Far_Z = true; break;
                            case "FullResolution": m.Full_Resolution = true; break;
                            case "PriorityPlane":
                                m.Priority_Plane = int.Parse(material.Tokens[i+1].Value);
                                break;
                            case "Layer": break;

                            default:
                                MessageBox.Show($"At line {token.LineNumber}: Unrecognized material property {token.Value}");
                                ParsingFailed = true;
                                return false;
                        }

                    }

                }
                // get layers
                List<TemporaryObject> layers = SplitCollectObjects(material.Tokens);
                foreach (TemporaryObject layer in layers)
                {
                    if (layer.Name == "Layer")
                    {
                        m.Layers.Add(ParseLayer(layer.Tokens));
                    }
                    else
                    {
                        MessageBox.Show($"At line {layer.Tokens[0].LineNumber}: at material: Unrecognized object {layer.Name}.");
                    }
                  
                }
                model.Materials.Add(m);
            }




            return true;
        }







        private static w3Layer ParseLayer(List<Token> tokens)
        {
            w3Layer layer = new w3Layer();
             
            bool isStatic = false; // To track if a property is marked as static

            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i].Value)
                {
                    case "FilterMode":
                        switch (tokens[i + 1].Value)
                        {
                            case "None": layer.Filter_Mode = FilterMode.None; break;
                            case "Blend": layer.Filter_Mode = FilterMode.Blend; break;
                            case "Additive": layer.Filter_Mode = FilterMode.Additive; break;
                            case "Modulate": layer.Filter_Mode = FilterMode.Modulate; break;
                            case "Modulate2x": layer.Filter_Mode = FilterMode.Modulate2x; break;
                            case "Transparent": layer.Filter_Mode = FilterMode.Transparent; break;
                            case "AddAlpha": layer.Filter_Mode = FilterMode.AddAlpha; break;

                        }
                        break;
                    case "TwoSided": layer.Two_Sided = true; break;
                    case "Unshaded": layer.Unshaded = true; break;
                    case "Unfogged": layer.Unfogged = true; break;
                    case "SphereEnvMap": layer.Sphere_Environment_Map = true; break;
                    case "NoDepthTest": layer.No_Depth_Test = true; break;
                    case "NoDepthSet": layer.No_Depth_Set = true; break;
                    case "static":
                        switch (tokens[i + 1].Value)
                        {
                            case "TextureID": layer.Diffuse_Texure_ID.isStatic = true; layer.Diffuse_Texure_ID.StaticValue = [int.Parse(tokens[i + 2].Value)]; break;
                            case "Alpha": layer.Alpha.isStatic = true; layer.Alpha.StaticValue = [float.Parse(tokens[i + 2].Value)]; break;
                            default: MessageBox.Show($"Unexpected keyword after 'static' at line {tokens[i].LineNumber}, at layer"); break;
                        }
                        break;
                }

            }
            List<TemporaryObject> objects = SplitCollectObjects(tokens);
            foreach (TemporaryObject obj in objects)
            {
                if (obj.Name == "Alpha")
                {
                    layer.Alpha.isStatic = false;
                    ParseTransformation(layer.Alpha, obj.Tokens, 1);
                }
                else if (obj.Name == "TextureID")
                {
                    ParseTransformation(layer.Diffuse_Texure_ID, obj.Tokens, 1);
                }
                else
                {
                    MessageBox.Show($"Unexpected object at line {obj.Tokens[0].LineNumber}, at layer");
                }
            }

            return layer;
        }



        private static List<Token> GetRemainingTokens(Token startToken, List<Token> tokens)
        {
            int index = tokens.IndexOf(startToken);
            if (index >= 0)
            {
                return tokens.Skip(index + 1).ToList();
            }
            return new List<Token>();
        }

        private static float[] ParseVector(List<Token> tokens, int amount)
        {
            List<float> vector = new List<float>();
            int count = 0;
            foreach (Token t in tokens)
            {
                if (count >= amount) { break; }
                if (t.Type == TokenType.Number)
                {
                    vector.Add(int.Parse(t.Value));
                    count++;
                }
            }
            return vector.ToArray();
        }
        //-----------------------------------------------------------------------------
        private static bool ParseGeoset(w3Model model, List<Token> objects)
        {
            w3Geoset geo = new w3Geoset();
            // parse properties
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "MaterialID")
                {
                    if ((objects[i + 1].Type == TokenType.Number))
                    {
                        geo.Material_ID = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i + 1].LineNumber}: at geoset: expected number after MaterialID"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "SelectionGroup")
                {
                    if ((objects[i + 1].Type == TokenType.Number))
                    {
                        geo.Selection_Group = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i + 1].LineNumber}: at geoset: expected number after SelectionGroup"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "BoundsRadius")
                {
                    if ((objects[i + 1].Type == TokenType.Number))
                    {
                        geo.Extent.Bounds_Radius = float.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i + 1].LineNumber}: at geoset: expected number after BoundsRadius"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "MinimumExtent")
                {
                    try
                    {
                        geo.Extent.Minimum_X = float.Parse(objects[i + 2].Value);
                        geo.Extent.Minimum_Y = float.Parse(objects[i + 4].Value);
                        geo.Extent.Minimum_Z = float.Parse(objects[i + 6].Value);
                    }
                    catch
                    {
                        MessageBox.Show($"At line {objects[i + 1].LineNumber}: at geoset: could not parse the minimum extent becasue the tokens are not in the expected order, or missing"); return false;
                    }

                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "MaximumExtent")
                {
                    try
                    {
                        geo.Extent.Maximum_X = float.Parse(objects[i + 2].Value);
                        geo.Extent.Maximum_Y = float.Parse(objects[i + 4].Value);
                        geo.Extent.Maximum_Z = float.Parse(objects[i + 6].Value);
                    }
                    catch
                    {
                        MessageBox.Show($"At line {objects[i + 1]}: at geoset: could not parse the maximum extent becasue the tokens are not in the expected order, or missing"); return false;
                    }

                }
            }
            // parse categories
            List<TemporaryObject> temporaryObjects = SplitCollectObjects(objects);

            foreach (TemporaryObject temporaryObject in temporaryObjects)
            {

                if (temporaryObject.Name == MDL_Keyword.Anim.ToString())
                {
                    geo.SequenceExtents.Add(ParseExtent(temporaryObject.Tokens));
                }
                else if (temporaryObject.Name == MDL_Keyword.Vertices.ToString())
                {
                    geo.Vertices = ParseVertices(temporaryObject.Tokens);
                }
                else if (temporaryObject.Name == MDL_Keyword.Normals.ToString())
                {

                    ParseNormals(geo.Vertices, temporaryObject.Tokens);
                }
                else if (temporaryObject.Name == MDL_Keyword.TVertices.ToString())
                {
                    ParseTxCoordinates(geo.Vertices, temporaryObject.Tokens);
                }
                else if (temporaryObject.Name == MDL_Keyword.VertexGroup.ToString())
                {
                    ParseVertexGroup(geo, temporaryObject.Tokens);
                }
                else if (temporaryObject.Name == MDL_Keyword.Groups.ToString())
                {
                    ParseMatrixGroups(geo, temporaryObject.Tokens);
                }

                else if (temporaryObject.Name == MDL_Keyword.Faces.ToString())
                {
                    ParseTriangles(geo, temporaryObject.Tokens);
                }
                else if (temporaryObject.Name == MDL_Keyword.MinimumExtent.ToString())
                {
                    geo.Extent.Minimum_X = float.Parse(temporaryObject.Tokens[0].Value);
                    geo.Extent.Minimum_Y = float.Parse(temporaryObject.Tokens[2].Value);
                    geo.Extent.Minimum_Z = float.Parse(temporaryObject.Tokens[4].Value);
                }
                else if (temporaryObject.Name == MDL_Keyword.MaximumExtent.ToString())
                {
                    geo.Extent.Maximum_X = float.Parse(temporaryObject.Tokens[0].Value);
                    geo.Extent.Maximum_Y = float.Parse(temporaryObject.Tokens[2].Value);
                    geo.Extent.Maximum_Z = float.Parse(temporaryObject.Tokens[4].Value);
                }
                else
                {
                    MessageBox.Show($"At line {temporaryObject.Tokens[0].LineNumber}: at geoset: unrecognized keyword {temporaryObject.Name}");
                    ParsingFailed = true;
                    return false;

                }

            }
            model.Geosets.Add(geo);
            return true;
        }

        private static Extent ParseExtent(List<Token> tokens)
        {
            Extent extent = new Extent();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Value == "MinimumExtent")
                {
                    extent.Minimum_X = float.Parse(tokens[i + 2].Value);
                    extent.Minimum_Y = float.Parse(tokens[i + 4].Value);
                    extent.Minimum_Z = float.Parse(tokens[i + 6].Value);
                }
                if (tokens[i].Value == "MaximumExtent")
                {
                    extent.Maximum_X = float.Parse(tokens[i + 2].Value);
                    extent.Maximum_Y = float.Parse(tokens[i + 4].Value);
                    extent.Maximum_Z = float.Parse(tokens[i + 6].Value);
                }
                if (tokens[i].Value == "BoundsRadius")
                {
                    extent.Bounds_Radius = float.Parse(tokens[i + 1].Value);

                }
            }

            return extent;
        }

        private static void ParseTriangles(w3Geoset geo, List<Token> tokens)
        {
          //  MessageBox.Show("faces tokens: " + tokens.Count.ToString());
           // MessageBox.Show(string.Join("", tokens.Select(x => x.Value)));

            List<w3Triangle> triangles = new List<w3Triangle>();
            w3Triangle Current = new w3Triangle();
            List<int> ints = new List<int>();
            for (int i = 0; i < tokens.Count; i++)
            {

                if (tokens[i].Type == TokenType.Number) { ints.Add(int.Parse(tokens[i].Value)); }


            }

            if (ints.Count % 3 != 0)
            {
                MessageBox.Show($"At line {tokens[0].LineNumber}: At geoset: count of triangle indices not a power of 3"); return;
            }

            for (int i = 0; i < ints.Count; i += 3)
            {
                w3Triangle t = new w3Triangle();
                t.Index1 = ints[i];
                t.Index2 = ints[i + 1];
                t.Index3 = ints[i + 2];
                triangles.Add(t);
            }


            geo.Triangles = triangles;

        }

        private static void ParseVertexGroup(w3Geoset geo, List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Number)
                {
                    geo.VertexGroup.Add(int.Parse(token.Value));
                }
            }

        }
        private static void ParseMatrixGroups(w3Geoset geo, List<Token> tokens)
        {
            List<int> CurrentMatrixGroup = new List<int>();
            bool insideMatrixGroup = false;  // To track if we are inside a matrix group

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];

                // Start of a matrix group: "Matrices {"
                if (token.Type == TokenType.Keyword && token.Value == "Matrices")
                {
                    // Expect the opening brace next
                    if (tokens[i + 1].Type == TokenType.OpeningBrace)
                    {
                        insideMatrixGroup = true;
                        i++; // Skip the opening brace token
                        continue;
                    }
                }

                // Handle numbers inside the matrix group
                if (insideMatrixGroup && token.Type == TokenType.Number)
                {
                    CurrentMatrixGroup.Add(int.Parse(token.Value));
                    continue;
                }

                // End of a matrix group: "}"
                if (insideMatrixGroup && token.Type == TokenType.ClosingBrace)
                {
                    // Finalize the matrix group
                    geo.MatrixGroups.Add(CurrentMatrixGroup.ToList());  // Copy the current group
                    CurrentMatrixGroup.Clear();  // Clear the current group for the next one
                    insideMatrixGroup = false;   // We're done with this matrix group
                    continue;
                }

                // Comma between numbers (optional handling, depends on format)
                if (insideMatrixGroup && token.Type == TokenType.Comma)
                {
                    continue;  // Just skip commas
                }
            }

            // Check for any unfinished matrix groups (in case of missing closing brace)
            if (insideMatrixGroup && CurrentMatrixGroup.Count > 0)
            {
                geo.MatrixGroups.Add(CurrentMatrixGroup.ToList());
                CurrentMatrixGroup.Clear();
            }
        }


        private static void ParseNormals(List<w3Vertex> vertices, List<Token> tokens)
        {
            Coordinate current = new Coordinate();
            int countCoordinates = 0;
            List<Coordinate> normals = new List<Coordinate>();

            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Number)
                {
                    switch (countCoordinates)
                    {
                        case 0: current.X = float.Parse(token.Value); countCoordinates++; break;
                        case 1: current.Y = float.Parse(token.Value); countCoordinates++; break;
                        case 2:
                            current.Z = float.Parse(token.Value);
                            normals.Add(current.Clone());
                            countCoordinates = 0;
                            break;
                    }


                }
            }
            for (int i = 0; i < normals.Count; i++)
            {
                if (i < vertices.Count)
                {
                    vertices[i].Normal = normals[i];
                }
            }
        }
        private static void ParseTxCoordinates(List<w3Vertex> vertices, List<Token> tokens)
        {
            Coordinate2D current = new Coordinate2D();
            int countCoordinates = 0;
            List<Coordinate2D> txCoords = new List<Coordinate2D>();


            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Number)
                {
                    switch (countCoordinates)
                    {
                        case 0: current.U = float.Parse(token.Value); countCoordinates++; break;

                        case 1:
                            current.V = float.Parse(token.Value);
                            txCoords.Add(current.Clone());
                            countCoordinates = 0;
                            break;
                    }
                  

                }
            }
            for (int i = 0; i < txCoords.Count; i++)
            {
                if (i < vertices.Count)
                {
                    vertices[i].Texture_Position = txCoords[i];
                }
            }
        }
        private static List<w3Vertex> ParseVertices(List<Token> tokens)
        {
            Coordinate current = new Coordinate();
            int countCoordinates = 0;
            List<w3Vertex> vertices = new List<w3Vertex>();
            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.Number)
                {
                    switch (countCoordinates)
                    {
                        case 0: current.X = float.Parse(token.Value); countCoordinates++; break;
                        case 1: current.Y = float.Parse(token.Value); countCoordinates++; break;
                        case 2:
                            current.Z = float.Parse(token.Value);
                            vertices.Add(new w3Vertex() { Position = current.Clone() });
                            countCoordinates = 0;
                            break;
                    }

                }
            }

            return vertices;
        }

        private static bool ParseCamera(w3Model model, List<Token> objects, string name)
        {
            w3Camera camera = new w3Camera();
            camera.Name = name;
            for (int i = 0; i < objects.Count; i++)
            {
                Token t = objects[i];

                if (t.Value == "FieldOfView") { camera.Field_Of_View = float.Parse(objects[i + 1].Value); continue; }
                if (t.Value == "FarClip") {camera.Far_Distance = int.Parse(objects[i + 1].Value); continue; }
                if (t.Value == "NearClip") {camera.Near_Distance = int.Parse(objects[i + 1].Value); continue; }
            }
            List<TemporaryObject> nested = SplitCollectObjects(objects);
            foreach (TemporaryObject obj in nested)
            {
                if (obj.Name == "Position")
                {
                    float one = float.Parse(obj.Tokens[0].Value);
                    float two = float.Parse(obj.Tokens[2].Value);
                    float three = float.Parse(obj.Tokens[4].Value);
                    camera.Position.StaticValue = [one,two,three];
                }
                if (obj.Name == "Translation")
                {
                    camera.Position.isStatic = false;
                    ParseTransformation(camera.Position, obj.Tokens, 3,false);
                    // position translation
                }
                if (obj.Name == "Target")
                {
                    List<TemporaryObject> nestedTarget = SplitCollectObjects(obj.Tokens);
                    foreach (TemporaryObject targetobj in nestedTarget)
                    {
                        if (targetobj.Name == "Position")
                        {
                            float one = float.Parse(targetobj.Tokens[0].Value);
                            float two = float.Parse(targetobj.Tokens[2].Value);
                            float three = float.Parse(targetobj.Tokens[4].Value);
                            camera.Target.StaticValue = [one, two, three];
                        }
                        if (targetobj.Name == "Translation")
                        {
                            camera.Target.isStatic = false;
                            ParseTransformation(camera.Target, targetobj.Tokens, 3, false);
                        }
                    }
                        // has position and translation
                    }
                if (obj.Name == "Rotation")
                {
                    camera.Rotation.isStatic = false;
                    ParseTransformation(camera.Rotation, obj.Tokens, 3, false);
                    // values are 1 per keyframe - int
                }
               
            }

            model.Cameras.Add(camera);
            return true;
        }
        private static bool ParseBone(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Bone bone = new Bone();


            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for parent"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "GeosetId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        bone.Geoset_ID = int.Parse(objects[i + 1].Value);
                    }
                    else if (objects[i + 1].Type == TokenType.Keyword && objects[i + 1].Value == "Multiple")
                    {
                        bone.Geoset_ID = -1;
                    }

                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for geosetid"); return false;
                    }
                }
            }
            // here get the transformations for translation, rotation and scaling, using   ParseTransformation()
            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "Translation") { if (ParseTransformation(node.Translation, t.Tokens, 3,false) == false) { return false; } }
                if (t.Name == "Rotation") { if (ParseTransformation(node.Rotation, t.Tokens, 4,false) == false) { return false; } }
                if (t.Name == "Scaling") { if (ParseTransformation(node.Scaling, t.Tokens, 3,false) == false) { return false; } }
            }
            node.Data = bone;
            model.Nodes.Add(node);
            return true;

        }
        private static bool ParseHelper(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Helper helper = new Helper();


            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for parent"); return false;
                    }
                }
            }

            // here get the transformations for translation, rotation and scaling, using   ParseTransformation()
            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "Translation") { if (ParseTransformation(node.Translation, t.Tokens, 3, false) == false) { return false; } }
                if (t.Name == "Rotation") { if (ParseTransformation(node.Rotation, t.Tokens, 4, false) == false) { return false; } }
                if (t.Name == "Scaling") { if (ParseTransformation(node.Scaling, t.Tokens, 3, false) == false) { return false; } }
            }
            node.Data = helper;
            model.Nodes.Add(node);
            return true;
        }
        private static bool ParseParticleEmitter2(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Particle_Emitter_2 pe = new Particle_Emitter_2();
          
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at ParticleEmitter2: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for parent"); return false;
                    }
                }
                // properties
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Transparent") pe.Filter_Mode = FilterMode.Transparent;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "None") pe.Filter_Mode = FilterMode.None;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Modulate") pe.Filter_Mode = FilterMode.Modulate;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Blend") pe.Filter_Mode = FilterMode.Blend;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Additive") pe.Filter_Mode = FilterMode.Additive;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "AddAlpha") pe.Filter_Mode = FilterMode.AddAlpha;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Rows") pe.Rows = int.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Columns") pe.Columns = int.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Time") pe.Time = float.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "LifeSpan") pe.Life_Span = float.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "TailLength") pe.Tail_Length = float.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "SortPrimsFarZ") pe.Sort_Primitives_Far_Z = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "LineEmitter") pe.Line_Emitter = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ModelSpace") pe.Model_Space = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "AlphaKey") pe.AlphaKey = true;

                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Unshaded") pe.Unshaded = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Unfogged") pe.Unfogged = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "XYQuad") pe.XY_Quad = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Squirt") pe.Squirt = true;
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Both")
                { pe.Head = true; pe.Tail = true; }
                else if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Head")
                {
                    pe.Head = true;
                }
                else if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Tail") { pe.Tail = true; }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "TextureID") pe.Texture_ID=  int.Parse(objects[i + 1].Value);

                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "static")
                {
                    switch (objects[i + 1].Value)
                    {

                        case "Speed":
                            pe.Speed.isStatic = true;
                            pe.Speed.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Variation":
                            pe.Variation.isStatic = true;
                            pe.Variation.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Latitude":
                            pe.Latitude.isStatic = true;
                            pe.Latitude.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Gravity":
                            pe.Gravity.isStatic = true;
                            pe.Gravity.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "EmissionRate":
                            pe.Emission_Rate.isStatic = true;
                            pe.Emission_Rate.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Width":
                            pe.Width.isStatic = true;
                            pe.Width.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Length":
                            pe.Length.isStatic = true;
                            pe.Length.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Visibility":
                           
                            pe.Visibility.isStatic = true;
                            pe.Visibility.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                    }
                }
            }
            // objects
            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "SegmentColor")
                {
                    try
                    {
                        float one = float.Parse(t.Tokens[2].Value);
                        float two = float.Parse(t.Tokens[4].Value);
                        float three = float.Parse(t.Tokens[6].Value);

                        float four = float.Parse(t.Tokens[11].Value);
                        float five = float.Parse(t.Tokens[13].Value);
                        float six = float.Parse(t.Tokens[15].Value);

                        float seven = float.Parse(t.Tokens[20].Value);
                        float eight = float.Parse(t.Tokens[22].Value);
                        float nine = float.Parse(t.Tokens[24].Value);
                        pe.Color_Segment1 = [one, two, three];
                        pe.Color_Segment2 = [four, five, six];
                        pe.Color_Segment3 = [seven, eight, nine];
                    }
                    catch
                    {
                        MessageBox.Show($"At line {t.Tokens[0]}: could not parse particleEmittter2's SegmentColor due to insufficient data in the stream");
                    }


                }
                if (t.Name == "Alpha")
                {
                    int one = int.Parse(t.Tokens[0].Value);
                    int two = int.Parse(t.Tokens[2].Value);
                    int three = int.Parse(t.Tokens[4].Value);
                    pe.ALpha_Segment1 = one;
                    pe.ALpha_Segment2 = two;
                    pe.ALpha_Segment3 = three;


                }
                if (t.Name == "ParticleScaling")
                {
                    float one = float.Parse(t.Tokens[0].Value);
                    float two = float.Parse(t.Tokens[2].Value);
                    float three = float.Parse(t.Tokens[4].Value);
                    pe.Scaling_Segment1 = one;
                    pe.Scaling_Segment2 = two;
                    pe.Scaling_Segment3 = three;

                }
                if (t.Name == "LifeSpanUVAnim")
                {
                    int one = int.Parse(t.Tokens[0].Value);
                    int two = int.Parse(t.Tokens[2].Value);
                    int three = int.Parse(t.Tokens[4].Value);
                    pe.Head_Lifespan_Start = one;
                    pe.Head_Lifespan_End = two;
                    pe.Head_Lifespan_Repeat = three;
                }
                if (t.Name == "DecayUVAnim")
                {
                    int one = int.Parse(t.Tokens[0].Value);
                    int two = int.Parse(t.Tokens[2].Value);
                    int three = int.Parse(t.Tokens[4].Value);
                    pe.Head_Decay_Start = one;
                    pe.Head_Decay_End = two;
                    pe.Head_Decay_Repeat = three;
                }
                if (t.Name == "TailUVAnim")
                {
                    int one = int.Parse(t.Tokens[0].Value);
                    int two = int.Parse(t.Tokens[2].Value);
                    int three = int.Parse(t.Tokens[4].Value);
                    pe.Tail_Lifespan_Start = one;
                    pe.Tail_Lifespan_End = two;
                    pe.Tail_Lifespan_Repeat = three;
                }
                if (t.Name == "TailDecayUVAnim")
                {
                    int one = int.Parse(t.Tokens[0].Value);
                    int two = int.Parse(t.Tokens[2].Value);
                    int three = int.Parse(t.Tokens[4].Value);
                    pe.Tail_Decay_Start = one;
                    pe.Tail_Decay_End = two;
                    pe.Tail_Decay_Repeat = three;
                }
                if (t.Name == "Speed"){ ParseTransformation(pe.Speed, t.Tokens, 1); continue; }

            if (t.Name == "Variation") { ParseTransformation(pe.Variation, t.Tokens, 1,false); continue;}
                if (t.Name == "Latitude") { ParseTransformation(pe.Latitude, t.Tokens, 1,false); continue;}
                if (t.Name == "Gravity") { ParseTransformation(pe.Gravity, t.Tokens, 1,false); continue;}
                if (t.Name == "EmissionRate") { ParseTransformation(pe.Emission_Rate, t.Tokens, 1,false); continue;}
                if (t.Name == "Width") { ParseTransformation(pe.Width, t.Tokens, 1,false); continue;}
                if (t.Name == "Length") { ParseTransformation(pe.Length, t.Tokens, 1,false); continue;}
                if (t.Name == "Visibility") {   ParseTransformation(pe.Visibility, t.Tokens, 1,false); continue;}

                // dynamic properties
            }
            node.Data = pe;
            model.Nodes.Add(node);
            return true;
        }
        private static bool ParseParticleEmitter(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Particle_Emitter_1 pe = new Particle_Emitter_1();
            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for parent"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "EmitterUsesMDL")
                {
                    pe.Emitter_Uses_MDL = true;
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "EmitterUsesTGA")
                {
                    pe.Emitter_Uses_TGA = true;
                }

                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "static")
                {
                    if (objects[i + 1].Type == TokenType.Keyword)
                    {
                        switch (objects[i + 1].Value)
                        {
                            case "EmissionRate": pe.Emission_Rate.isStatic = true; pe.Emission_Rate.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "Gravity": pe.Emission_Rate.isStatic = true; pe.Emission_Rate.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "Longitude": pe.Longitude.isStatic = true; pe.Longitude.StaticValue = [float.Parse(objects[i + 2].Value)]; break;
                            case "Latitude": pe.Latitude.isStatic = true; pe.Latitude.StaticValue = [float.Parse(objects[i + 2].Value)]; break;
                            case "Visibility": pe.Visibility.isStatic = true; pe.Visibility.StaticValue = [float.Parse(objects[i + 2].Value)]; break;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at light: expected keyword after static"); return false;
                    }
                }
            }

            List<TemporaryObject> temporaryObjects = SplitCollectObjects(objects);
            foreach (TemporaryObject obj in temporaryObjects)
            {
                switch (obj.Name)
                {
                    case "Particle":
                        for (int i = 0; i < obj.Tokens.Count; i++)
                        {
                            if (obj.Tokens[i].Value == "static")
                            {
                                switch (obj.Tokens[i + 1].Value)
                                {
                                    case "LifeSpan":
                                        pe.Life_Span.isStatic = true;
                                        pe.Life_Span.StaticValue = [float.Parse(obj.Tokens[i + 2].Value)];
                                        break;
                                    case "InitVelocity":
                                        pe.Initial_Velocity.isStatic = true;
                                        pe.Initial_Velocity.StaticValue = [float.Parse(obj.Tokens[i + 2].Value)];
                                        break;
                                    case "Path":
                                        pe.Particle_Filename = obj.Tokens[i + 2].Value;
                                        break;
                                }
                            }
                            List<TemporaryObject> particleObjects = SplitCollectObjects(objects);
                            for (int px = 0; i < particleObjects.Count; px++)
                            {
                                if (particleObjects[px].Value == "LifeSpan")
                                {
                                    if (ParseTransformation(pe.Life_Span, obj.Tokens, 1) == false)
                                    {
                                        MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic LifeSpan"); return false;
                                    }
                                }
                                if (particleObjects[px].Value == "InitVelocity")
                                {
                                    if (ParseTransformation(pe.Initial_Velocity, obj.Tokens, 1) == false)
                                    {
                                        MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic InitVelocity"); return false;
                                    }
                                }
                            }

                        }
                        break;
                    case "EmissionRate":
                        if (ParseTransformation(pe.Emission_Rate, obj.Tokens, 1,false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic EmissionRate"); return false;
                        }
                        break;
                    case "Gravity":
                        if (ParseTransformation(pe.Gravity, obj.Tokens, 1, false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Gravity"); return false;
                        }
                        break;
                    case "Longitude":
                        if (ParseTransformation(pe.Longitude, obj.Tokens, 1, false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Longitude"); return false;
                        }
                        break;
                    case "Latitude":
                        if (ParseTransformation(pe.Latitude, obj.Tokens, 1) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Latitude"); return false;
                        }
                        break;
                    case "Visibility":
                        if (ParseTransformation(pe.Visibility, obj.Tokens, 1) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Visibility"); return false;
                        }
                        break;

                }

            }
            node.Data = pe;
            model.Nodes.Add(node);
            return true;
        }
        private static bool ParseLight(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            w3Light light = new w3Light();
            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at bone: expected value for parent"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Omnidirectional")
                {
                    light.Type = LightType.Omnidirectional;

                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Directional")
                {
                    light.Type = LightType.Directional;

                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Ambient")
                {
                    light.Type = LightType.Ambient;

                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "static")
                {
                    if (objects[i + 1].Type == TokenType.Keyword)
                    {
                        switch (objects[i + 1].Value)
                        {
                            case "AttenuationStart": light.Attenuation_Start.isStatic = true; light.Attenuation_Start.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "AttenuationEnd": light.Attenuation_End.isStatic = true; light.Attenuation_End.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "Intensity": light.Intensity.isStatic = true; light.Intensity.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "AmbIntensity": light.Ambient_Intensity.isStatic = true; light.Ambient_Intensity.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "Visibility": light.Visibility.isStatic = true; light.Visibility.StaticValue = [int.Parse(objects[i + 2].Value)]; break;
                            case "Color":
                                light.Color.isStatic = true;
                                light.Color.StaticValue = [float.Parse(objects[i + 3].Value), float.Parse(objects[i + 5].Value), float.Parse(objects[i + 7].Value)];
                                break;
                            case "AmbColor": light.Ambient_Color.isStatic = true; light.Ambient_Color.StaticValue = [float.Parse(objects[i + 3].Value), float.Parse(objects[i + 5].Value), float.Parse(objects[i + 7].Value)]; break;

                        }
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at light: expected keyword after static"); return false;
                    }
                }
            }

            List<TemporaryObject> temporaryObjects = SplitCollectObjects(objects);
            foreach (TemporaryObject obj in temporaryObjects)
            {
                switch (obj.Name)
                {

                    case "AttenuationStart":
                        if (ParseTransformation(light.Attenuation_Start, obj.Tokens, 1, false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic AttenuationStart"); return false;
                        }
                        break;

                    case "AttenuationEnd":
                        if (ParseTransformation(light.Attenuation_End, obj.Tokens, 1) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic AttenuationEnd"); return false;
                        }
                        break;
                    case "Intensity":
                        if (ParseTransformation(light.Intensity, obj.Tokens, 1,false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Intensity"); return false;
                        }
                        break;
                    case "AmbIntensity":
                        if (ParseTransformation(light.Ambient_Intensity, obj.Tokens, 1,false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic AmbIntensity"); return false;
                        }
                        break;
                    case "Visibility":
                        if (ParseTransformation(light.Visibility, obj.Tokens, 1, false) == false)
                        {
                            MessageBox.Show($"At line {obj.Tokens[0].LineNumber}: at light: could not parse dynamic Visibility"); return false;
                        }
                        break;
                    case "Color":
                        light.Color.isStatic = true;
                        light.Color.StaticValue = [float.Parse(obj.Tokens[1].Value), float.Parse(obj.Tokens[3].Value), float.Parse(obj.Tokens[5].Value)];

                        break;
                    case "AmbColor":
                        light.Ambient_Color.isStatic = true;
                        light.Ambient_Color.StaticValue = [float.Parse(obj.Tokens[1].Value), float.Parse(obj.Tokens[3].Value), float.Parse(obj.Tokens[5].Value)];

                        break;
                }

            }
            node.Data = light;
            model.Nodes.Add(node);
            return true;
        }
       
        private static bool ParseRibbonEmitter(w3Model model, List<Token> objects, string name)
        {

            w3Node node = new w3Node();
            node.Name = name;
            Ribbon_Emitter re = new Ribbon_Emitter();


            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for parent"); return false;
                    }
                }
                // properties
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Rows") re.Rows = int.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Columns") re.Columns = int.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "LifeSpan") re.Life_Span = float.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Gravity") re.Gravity = float.Parse(objects[i + 1].Value);
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "MaterialID") re.Material_ID = int.Parse(objects[i + 1].Value);


                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "static")
                {
                    switch (objects[i + 1].Value)
                    {

                        case "HeightAbove":
                            re.Height_Above.isStatic = true;
                            re.Height_Above.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "HeightBelow":
                            re.Height_Below.isStatic = true;
                            re.Height_Below.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Alpha":
                            re.Alpha.isStatic = true;
                            re.Alpha.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "TextureSlot":
                            re.Texture_Slot.isStatic = true;
                            re.Texture_Slot.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Visibility":
                            re.Visibility.isStatic = true;
                            re.Visibility.StaticValue = [float.Parse(objects[i + 2].Value)];

                            break;
                        case "Color":
                            re.Color.isStatic = true;
                            float one = float.Parse(objects[i + 3].Value);
                            float two = float.Parse(objects[i + 5].Value);
                            float three = float.Parse(objects[i + 7].Value);
                            re.Color.StaticValue = [one, two, three];

                            break;
                    }
                }
            }
            // objects
            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "Color") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 3, false);
                if (t.Name == "Visibility") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 1, false);
                if (t.Name == "TextureSlot") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 1, false);
                if (t.Name == "Alpha") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 1, false);
                if (t.Name == "HeightBelow") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 1, false);
                if (t.Name == "HeightAbove") re.Color.isStatic = false; ParseTransformation(re.Color, t.Tokens, 1, false);
            }

            node.Data = re;
            model.Nodes.Add(node);
            return true;
        }
        //dome
        private static bool ParseAttachment(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            w3Attachment att = new w3Attachment();


            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at helper: expected value for parent"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Path")
                {
                    if (objects[i + 1].Type == TokenType.String)
                    {
                        att.Path = objects[i + 1].Value;
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at attachment: expected string value for path"); return false;
                    }
                }
            }

            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "Visibility") { if (ParseTransformation(att.Visibility, t.Tokens, 1, false) == false) { return false; } }
                if (t.Name == "Translation") { if (ParseTransformation(node.Translation, t.Tokens, 3, false) == false) { return false; } }
                if (t.Name == "Rotation") { if (ParseTransformation(node.Rotation, t.Tokens, 4, false) == false) { return false; } }
                if (t.Name == "Scaling") { if (ParseTransformation(node.Scaling, t.Tokens, 3, false) == false) { return false; } }
            }
            node.Data = att;
            model.Nodes.Add(node);
            return true;

        }
        //done
        private static bool ParseEventObject(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Event_Object ev = new Event_Object();


            // get objectid and parent if present
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "ObjectId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.objectId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at eventobject: expected value for objectId"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "Parent")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        node.parentId = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at eventobject: expected value for parent"); return false;
                    }
                }
                if (objects[i].Type == TokenType.Keyword && objects[i].Value == "GlobalSeqId")
                {
                    if (objects[i + 1].Type == TokenType.Number)
                    {
                        ev.Global_sequence_ID = int.Parse(objects[i + 1].Value);
                    }
                    else
                    {
                        MessageBox.Show($"At line {objects[i].LineNumber}: at eventobject: expected value for GlobalSeqId"); return false;
                    }
                }
            }

            List<TemporaryObject> temporaryObject = SplitCollectObjects(objects);
            foreach (TemporaryObject t in temporaryObject)
            {
                if (t.Name == "EventTrack") { ev.Tracks = ExtractTracks(t.Tokens); }
                if (t.Name == "Translation") { if (ParseTransformation(node.Translation, t.Tokens, 3) == false) { return false; } }
                if (t.Name == "Rotation") { if (ParseTransformation(node.Rotation, t.Tokens, 4) == false) { return false; } }
                if (t.Name == "Scaling") { if (ParseTransformation(node.Scaling, t.Tokens, 3) == false) { return false; } }
            }
            node.Data = ev;
            model.Nodes.Add(node);
            return true;
        }
        private static List<int> ExtractTracks(List<Token> tokens)
        {
            List<int> list = new List<int>();
            foreach (Token t in tokens)
            {
                if (t.Type == TokenType.Number)
                {
                    list.Add(int.Parse(t.Value));
                }
            }
            return list;
        }
        private static bool ParseCollisionShape(w3Model model, List<Token> objects, string name)
        {
            w3Node node = new w3Node();
            node.Name = name;
            Collision_Shape collision_Shape = new Collision_Shape();
             
           
            for (int i = 0; i<objects.Count; i++)
            {
                switch (objects[i].Value)
                {
                    case "ObjectId":node.objectId = int.Parse(objects[i+1].Value); break;
                    case "Parent": node.parentId = int.Parse(objects[i + 1].Value); break;
                    case "Box": collision_Shape.Type = CollisionShapeType.Box; break;
                    case "Sphere": collision_Shape.Type = CollisionShapeType.Sphere; break;
                    case "Vertices":
                        int num = int.Parse(objects[i + 1].Value);
                            if (num == 1)
                        {
                            float minx = float.Parse(objects[i + 4].Value);
                            float miny = float.Parse(objects[i + 6].Value);
                            float minz = float.Parse(objects[i + 8].Value);
                            collision_Shape.Extents.Minimum_X = minx;
                            collision_Shape.Extents.Minimum_Y = miny;
                            collision_Shape.Extents.Minimum_Z = minz;

                        }
                        if (num == 2)
                        {
                            float minx = float.Parse(objects[i + 4].Value);
                            float miny = float.Parse(objects[i + 6].Value);
                            float minz = float.Parse(objects[i + 8].Value);
                            float maxx = float.Parse(objects[i + 12].Value);
                            float maxy = float.Parse(objects[i + 14].Value);
                            float maxz = float.Parse(objects[i + 16].Value);
                            collision_Shape.Extents.Minimum_X = minx;
                            collision_Shape.Extents.Minimum_Y = miny;
                            collision_Shape.Extents.Minimum_Z = minz;
                            collision_Shape.Extents.Maximum_X = maxx;
                            collision_Shape.Extents.Maximum_Y = maxy;
                            collision_Shape.Extents.Maximum_Z = maxz;

                        }
                        break;
                    case "BoundsRadius": collision_Shape.Extents.Bounds_Radius = float.Parse(objects[i + 1].Value); break;
                }
            }
            
            
            
             

            // Finalize and add the node to the model
            node.Data = collision_Shape;
            model.Nodes.Add(node);
            return true;  // Parsing successful
        }

        private static void GetMinMaxExtents(Extent ex, List<float> list)
        {
            ex.Minimum_X = list.Count >= 1 ? list[0] : 0;
            ex.Minimum_Y = list.Count >= 2 ? list[1] : 0;
            ex.Minimum_Z = list.Count >= 3 ? list[2] : 0;
            ex.Maximum_X = list.Count >= 4 ? list[3] : 0;
            ex.Maximum_Y = list.Count >= 5 ? list[4] : 0;
            ex.Maximum_Z = list.Count >= 6 ? list[5] : 0;

        }
        public class TemporaryObject
        {
            public string Name { get; set; } = "";
            public string Value { get; set; } = "";
            public List<Token> Tokens { get; set; } = new List<Token>();

            public TemporaryObject Clone()
            {
                var clonedTokens = Tokens.Select(token => token.Clone()).ToList();
                return new TemporaryObject
                {
                    Name = Name,
                    Value = Value,
                    Tokens = clonedTokens
                };
            }
        }




       
        public class MDLObject
        {
            public string Header = "";
            public object Data; // either string or another MDLObject or or list of MDLObjects

        }
    }
}


