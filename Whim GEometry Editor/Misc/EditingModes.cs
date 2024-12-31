namespace MDLLibs.Classes.Misc
{
    public enum EditMode
    {
        Translate,Rotate,   Scale, Morph
    }
    public enum MorphMode
    {
        None,
        Translate, 
        Rotate, 
        Scale,
        Inset, 
        InsetEach, 
        Extrude,
        ExtrudeEach, 
        Extract,
        ExtractGeoset,
        RotateNormals ,
        Extent,
        Widen,
        Narrow,
        Bevel,
        Curve
    }
    public enum RotateScaleMethod  { Individual, Group   }
    public enum RotateScaleAroundMethod  { Centroid, Bone  }
    public enum AxisMode
    {
        X,Y,Z,XY,ZY,XYZ, Facing,
        None
    }
   public enum AppMode
    {
        Animator, Rigging, Viewer, Nodes, Render, // render = info, properties, components
        Geometry
    }
    public enum GeometryMode { Vertices, Faces, Edges, Normals}
    internal static class EditingModes
    {
        public static SelectionType SelectType = SelectionType.ClearSelect;
        public static EditMode EditMode = EditMode.Translate;
        public static MorphMode MorphMpde = MorphMode.None;
        public static AxisMode AxisMode = AxisMode.X;
        public static GeometryMode GeometryMode = GeometryMode.Vertices;
        public static RotateScaleMethod RotateScaleMethod = RotateScaleMethod.Group;
        public static RotateScaleAroundMethod RotateScaleAroundMethod = RotateScaleAroundMethod.Centroid;
        public static AppMode appMode = AppMode.Animator;
    }
}
