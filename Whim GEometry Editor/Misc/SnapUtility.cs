using MDLLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    public static class SnapUtility
    {
        public static float WorldSnapSpacing { get; set; } = 1f;

        // Snap along the positive X axis
        public static Coordinate SnapXPlus(Coordinate vertex)
        {
            float snappedX = (float)Math.Ceiling(vertex.X / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(snappedX, vertex.Y, vertex.Z);
        }

        // Snap along the negative X axis
        public static Coordinate SnapXMinus(Coordinate vertex)
        {
            float snappedX = (float)Math.Floor(vertex.X / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(snappedX, vertex.Y, vertex.Z);
        }

        // Snap along the positive Y axis
        public static Coordinate SnapYPlus(Coordinate vertex)
        {
            float snappedY = (float)Math.Ceiling(vertex.Y / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(vertex.X, snappedY, vertex.Z);
        }

        // Snap along the negative Y axis
        public static Coordinate SnapYMinus(Coordinate vertex)
        {
            float snappedY = (float)Math.Floor(vertex.Y / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(vertex.X, snappedY, vertex.Z);
        }

        // Snap along the positive Z axis
        public static Coordinate SnapZPlus(Coordinate vertex)
        {
            float snappedZ = (float)Math.Ceiling(vertex.Z / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(vertex.X, vertex.Y, snappedZ);
        }

        // Snap along the negative Z axis
        public static Coordinate SnapZMinus(Coordinate vertex)
        {
            float snappedZ = (float)Math.Floor(vertex.Z / WorldSnapSpacing) * WorldSnapSpacing;
            return new Coordinate(vertex.X, vertex.Y, snappedZ);
        }

        public static Coordinate SnapToNearest(Coordinate vertex)
        {
            float snappedX = (float)Math.Round(vertex.X / WorldSnapSpacing) * WorldSnapSpacing;
            float snappedY = (float)Math.Round(vertex.Y / WorldSnapSpacing) * WorldSnapSpacing;
            float snappedZ = (float)Math.Round(vertex.Z / WorldSnapSpacing) * WorldSnapSpacing;

            return new Coordinate(snappedX, snappedY, snappedZ);
        }


 
    }
}
