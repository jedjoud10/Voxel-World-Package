using System;
using System.Collections.Generic;
using System.Linq;
using static VoxelSavedGraphUtility;
using static VoxelGraphUtility;
using static VoxelUtility;
using UnityEngine;
using UnityEditor;

public static partial class VoxelSavedGraphUtility 
{
    /// <summary>
    /// Custom IEqualityComparer from https://stackoverflow.com/questions/6413108/how-do-i-check-if-two-objects-are-equal-in-terms-of-their-properties-only-withou
    /// </summary>
    public class SavedVoxelNodeComparer : IEqualityComparer<KeyValuePair<string, SavedVoxelNode>>
    {
        public bool Equals(KeyValuePair<string, SavedVoxelNode> x, KeyValuePair<string, SavedVoxelNode> y)
        {
            if (x.Value == null || y.Key == null || x.Value.nodeData.voxelNode.savedPorts == null || y.Value.nodeData.voxelNode.savedPorts == null) return false;

            bool position = x.Value.posx == y.Value.posx && x.Value.posy == y.Value.posy;
            bool type = x.Value.nodeData == y.Value.nodeData;
            bool savedPorts = x.Value.nodeData.voxelNode.savedPorts.SequenceEqual(y.Value.nodeData.voxelNode.savedPorts);
            bool key = x.Key == y.Key;

            return (key && position && savedPorts && type);
            throw new System.NotImplementedException();
        }

        public int GetHashCode(KeyValuePair<string, SavedVoxelNode> obj)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// A node that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelNode
    {
        //Main variables
        public float posx, posy;
        public GraphViewNodeData nodeData;
    }

    /// <summary>
    /// An edge that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelEdge
    {
        //Main variables
        public SavedVoxelPort input;
        public SavedVoxelPort output;
    }

    /// <summary>
    /// A port that was saved in the SavedVoxelGraph
    /// </summary>
    [System.Serializable]
    public class SavedVoxelPort
    {
        //Main variables
        public string nodeGuid;
        public string portGuid;
    }


    /// <summary>
    /// Serializable versions of Vector2, Vector3 and Vector4
    /// </summary>
    [System.Serializable]
    public struct SavableVec2 { public float x, y; };
    [System.Serializable]
    public struct SavableVec3 { public float x, y, z; };
    [System.Serializable]
    public struct SavableVec4 { public float x, y, z, w; };
}