using MDLLib;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace MDLLibs.Classes.Misc
{
    internal static class ModelHelper
    {
        internal static string Path = string.Empty;
        internal static Dictionary<int, Bitmap> ImagesForDirectUse = new Dictionary<int, Bitmap>();
        public static string GetPathFromTextureID(int texture_ID)
        {
            if (Current.Textures.Any(x=>x.ID == texture_ID) == false) { return string.Empty; }
            return  Current.Textures.First(x => x.ID == texture_ID).Path;
        }
        internal static w3Model? Current = null;
        internal static w3Model? Temporary = null; // for target optimization
        private static List<  List<w3Geoset>> Collection = new ();
        public static void ClearStack()
        {
            Collection.Clear();
            Index = 0;
        }
        internal static int Index = 0;
        internal static int Limit = 0;
        internal static void Reset()
        {
          Collection.Clear ();  
            Index = 0;
        }
         internal static void Next()
        {
            if (Limit <= 1) { return; }
            if (Index + 1 == Limit)
            {
                Collection.RemoveAt(0);
            }
            else { Index++; }
            List<w3Geoset> geosets = new();
            foreach (w3Geoset geoset in Current.Geosets)
            {
                geosets.Add(geoset.Clone());
            }
            Collection.Add(geosets);
        }
        internal static void Undo()
        {
            if (Index> 0) { Index--; } else { return; }
            ApplyState();
        }
        internal static void Redo()
        {
            if (Index < Collection.Count -1) { Index++; } else {  return; }
            ApplyState();
        }
        private static void ApplyState()
        {
            Current.Geosets = Collection[Index];
        }
       
        internal static int GetTextureIDFromPath(string? v)
        {
            return Current.Textures.First(x => x.Path == v).ID;
        }
    }
}
