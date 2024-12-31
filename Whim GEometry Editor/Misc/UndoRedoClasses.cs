using MDLLib;
using MDLLibs.Classes.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Whim_GEometry_Editor.Misc
{
    /*
     can undo:
    geometry
    nodes
    node animation

     */
    // For nodes
    
    public class NodeAnimateHistory
    {
        public w3Node Node;
        public Vector3 Translation, Rotation, Scaling;
        int Track;
        public NodeAnimateHistory(w3Node node, Vector3 translation, int track)
        {
            Node = node;
            Translation = translation;
            Rotation = Vector3.Zero;
            Scaling = Vector3.Zero;
            Track = track;
        }

        public NodeAnimateHistory(w3Node node, Vector3 rotation, bool isRotation, int track)
        {
            Node = node;
            Translation = Vector3.Zero;
            Rotation = rotation;
            Scaling = Vector3.Zero;
            Track = track;
        }

        public NodeAnimateHistory(w3Node node, Vector3 scaling, float scaleFactor, int track)
        {
            Node = node;
            Translation = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scaling = scaling;
            Track = track;
        }


    }
    public class NodeHistory
    {
        public int NodeId;
        public Coordinate Coordinate;
        public NodeHistory(int id, Coordinate coordinate)
        {
            NodeId = id;
            Coordinate = coordinate;
        }
    }
    public class AnimatorHistory
    {

        public int noteId;
        public TransformationType TransformationType;
        public Vector3 Instructions;
    }

}
