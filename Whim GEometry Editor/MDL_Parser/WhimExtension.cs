 
using System.Numerics;
 
namespace Whim_GEometry_Editor.MDL_Parser
{
    public class wModel
    {
        public string Name, AnimationFile;
        public float BlendTime;
        public int Extent;
        public List<wCamera> Cameras;
        public List<wNode> nodes;
        public List<wMaterial> Materials;
        public List<wTexture> Textures;
        public List<wTextureAnim> TextureAnimations;
     
       
        public List<wGeoset> Geosets;
        public List<wGeosetAnimation> GeosetAnims;
        public List<wSequence> sequences;
        public List<wGlobalSequence> GlobalSequences;
        // to be referenced:
        public List<wTransformation> Transformations;
        public List<wExtent> Extents;
        
    }
 public class wCamera
    {
        public string Name;
        public int Rotation, Position, Taget; // transformation ids
        public float FOV, Near, Far;
    }
    public class wGeosetAnimation
    {
        public int ID;
        public int GeosetID;
        public int Alpha, Color;
        public bool UseColor, HasShadow;
    }
    public class wExtent
    {
        public float minX, minY, minZ, maxX, maxY, maxZ, Bounds;
        public int ID;
    }
    public enum wInterpolationType
    {
        NoInterp, Linear, Bezier, Hermite
    }
   
    public class wTransformation
    {
        public int ID;
        public float StaticValue;
        public int GlobalSeqID;
        public wInterpolationType InterpolationType;
         public List<wKeyframe> Keyframes;
    }
    public class wKeyframe
    {
         
        public int Track;
        public Vector4 Data;
        public Vector4 Intan;
        public Vector4 Outtan;
       

    }
   
    public class wSequence
    {
        public string Name;
        public float Rarity;
        public float MoveSpeed;
        public int From;
        public int To;
        public int Extent;
        public bool NonLooping;
      

    }
    public class wGlobalSequence
    {
        public int ID;
        public int Duration;
    }
    public class wMaterial
    {
        public int ID;
        public List<wLayer> Layers;
        public bool FullRes, ConstantColor, SortPrimitivesFarZ;
        public int PriorityPlane;

    }
    public enum wFilterMode
    {
        None, Transparent, Blend, Additive, AddAlpha, Modulate
    }
    public class wLayer
    {
        public int ID;
        public int Alpha;  // transformation
        public int TextureID; // transformation
        public int TextureAnimID; 
          public bool Unshaded, Unfogged, Twosided, Sphere, NoDepthTest, NoDepthSet;
        public wFilterMode FilterMode;   
    }
    public class wTexture
    {
        public int ID;
        public string Path;
        public bool WrapWidth, WrapHeight;
        public int ReplaceableID;
    }
    public class wGeoset
    {
        public List<wVertex> Vertices;
        public List<wTriangle> Triangles;
        public List<wExtent> Extents;
        public wExtent Extent;
        public int SelectionGroup;
        public bool UnSelectable;
        public int MaterialID;
    }
    public class wVertex
    {
        public int ID;
        public Vector3 Position, Normal;
        public Vector2 TPosition;

    }
    public  class wTriangle {
        public int Vertex1ID, Vertex2ID, Vertex3ID;
        }

    public class wTextureAnim
    {
        public int ID;
        public int TranslationID, RotationID, ScalingID;
    }
    public class wNode
    {
        public int ID;
        public string Name;
        public int ParentID;
        public int Translation, Rotation, Scale;
        public bool DontInheritTranslation, DontInheritRotation, DontInheritScale, Billboarded, BillboardedX, BillboardedY, BillboardedZ, CameraAnchored;
        public Vector3 PivotPoint;
        public object Data; // here goes the type of the node and its data
    }
    public class wBone
    {
        public int GeosetID;
        public int GeosetAnimID;
    }
    public class wHelper { }
    public class wAttachment
    {
        public string Path;
        public int VisibilityID;
    }
    public class wCollision
    {
        public int Type; // 0 = box, 1 = sphere
        public wExtent Extent;
    }
    public class wEvent
    {
        public List<int> Tracks;
        public string Type, Data;
        public char Identifier;
        public int GlobalSequenceID;
      
    }
    public class wEmitter1
    {
        public int EmissionRate, LifeSpan, InitialVelocity, Gravity, Longtitude, Latittude, Visibility;
        // tranformation ids ^
        public string ParticlePath;
        public bool UsesMDL, usesTGA;

    }
    public class wRibbon
    {
        public int Color, Alpha, Velocity, HeightAbove, HeightBelow, TextureSlot;
        // tranformation ids ^
        public int MaterialID;
        public int Rows, Columns, EmissionRate;
        public float Lifespan, Gravity;
    }
    public class wLight
    {
        public int Color, AmColor, AttentuationStart, AttentuationEnd, Intensity, AmIntensity, Visibility;
        public int Type;
        // 0 = omnidirection, 1 = direction, 2 = ambient 
    }
    public class wEmitter2
    {
        public int Visibility, EmissionRate, Speed, Variation, Latitude, Width, Height, Gravity;
        // tranformation ids ^
        public wFilterMode FilterMode;    
        public int TextureID;
        public Vector3 Color1, Color2, Color3;
        public int Alpha1,Alpha2, Alpha3; //0=255
        public float Scaling1, Scaling2, Scaling3;

        public int Rows, Columns, ReplaceAbleID, PriorityPlane;
        public float Time, Lifespan, TailLength;
        public float HeadLifespanStart, HeadLifespanEnd, HeadLifespanRepeat;
        public float HeadDecayStart, HeadDecayEnd, HeadDecayRepeat;
        public float TailLifespanStart, TailLifespanEnd, TailLifespanRepeat;    
        public float TailDecayStart, TailDecayEnd, TailDecayRepeat;
        public bool Unshaded, Unfogged, Alphakey, LineEmitter, SortPrimsFarZ, ModelSpace, XYQuad, Suirt, Head, Tail;
    }
}
