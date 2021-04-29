using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using System.IO;
using static VoxelGraphUtility;
using static VoxelUtility;
using static SavedVoxelGraphUtility;
using System.Linq;
/// <summary>
/// Converts a VoxelGraphSO to a compute shader
/// </summary>
public static class CodeConverter
{

    public static Dictionary<string, object> variables = new Dictionary<string, object>();
    public static Dictionary<string, SavedVoxelEdge> savedEdges = new Dictionary<string, SavedVoxelEdge>();
    public static Dictionary<string, SavedVoxelNode> savedNodes = new Dictionary<string, SavedVoxelNode>();
    public static StringBuilder localBuilder;
    /// <summary>
    /// Convert the VoxelGraphSO to a string that is going to be used as compute shader
    /// </summary>
    public static void ConvertAndSave(VoxelGraphSerializer serializer, string path) 
    {        
        
        StringBuilder builder = new StringBuilder();
        //Start
        builder.Append(@"
//Each #kernel tells which function to compile; you can have many kernels
#pragma kernel VoxelMain
#pragma kernel VoxelFinal
//Base values
float3 offset;
float3 scale;
float chunkScaling;
int resolution;
float quality;
float isolevel;

//Data stuff
struct Voxel
{
    float density;
    float3 color;
    float3 normal;
    float2 sm;
};
struct VoxelDetail
{
    float3 position;
    float3 forward;
    float size;
    int type;
};
struct ColorSmoothnessMetallic
{
    float3 color;
    float2 sm;
};
RWStructuredBuffer<Voxel> voxelsBuffer;
AppendStructuredBuffer<VoxelDetail> detailsBuffer;
");

        //Density
        builder.Append(@"

//Density function
float Density(float3 p, float3 lp)
{
    return p.y;
}
");
        builder.Append(@"

//Get the color, smoothness, and metallic all in one function
ColorSmoothnessMetallic GetCSM(float3 p, float3 lp, float3 n)
{
    ColorSmoothnessMetallic csm;
    csm.color = 0;
    csm.sm = 0;
    return csm;
}
");
        builder.Append(@"

//This is ran for every intersecting edge in the volume, allows us to place any voxel detail on the surface
void PlaceVoxelDetailEdge(float3 sp, float3 lp, float3 sn)
{
    
}");

        localBuilder = new StringBuilder();
        for (int i = 0; i < 1; i++)
        {
            SavedLocalVoxelGraph graph = serializer.globalGraph[i];
            savedEdges = graph.edges;
            savedNodes = graph.nodes;
            //Start at the default node and traverse the graph
            string currentNodeGuid = graph.nodes.ElementAt(0).Key;
            SavedVoxelNode currentSavedNode = graph.nodes.ElementAt(0).Value;
            VoxelNode voxelNode = currentSavedNode.nodeData.voxelNode;
            string currentLine = voxelNode.CodeRepresentationPort(graph, voxelNode.savedPorts[0].portGuid);
            Debug.Log(currentLine);            
        }

        builder.Append(localBuilder.ToString());

        //End
        builder.Append(@"
int flt(uint3 pos) { return (pos.z * resolution * resolution) + (pos.y * resolution) + pos.x; }
[numthreads(8, 8, 8)]
void VoxelMain(uint3 id : SV_DispatchThreadID)
{
    float3 p = (id * chunkScaling + offset) * scale;

    Voxel voxel;
    voxel.density = Density(p, id / (float)resolution);
    voxel.normal = 1;
    voxel.color = 1;
    voxel.sm = 0;
    voxelsBuffer[(id.z * resolution * resolution) + (id.y * resolution) + id.x] = voxel;
}

float unlerp(float a, float b, float t) { return (t - a) / (b - a); }

[numthreads(8, 8, 8)]
void VoxelFinal(uint3 id : SV_DispatchThreadID)
{
    float3 p = (id * chunkScaling + offset) * scale;
    int index = flt(id);
    Voxel voxel = voxelsBuffer[index];
    float3 normal = 0;
    normal.x = voxelsBuffer[flt(uint3(1, 0, 0) + id)].density - voxelsBuffer[flt(uint3(-1, 0, 0) + id)].density;
    normal.y = voxelsBuffer[flt(uint3(0, 1, 0) + id)].density - voxelsBuffer[flt(uint3(0, -1, 0) + id)].density;
    normal.z = voxelsBuffer[flt(uint3(0, 0, 1) + id)].density - voxelsBuffer[flt(uint3(0, 0, -1) + id)].density;
    normal = normalize(normal);

    voxel.normal = normal;
    ColorSmoothnessMetallic csm = GetCSM(p, id / (float)resolution, normal);
    voxel.color = csm.color;
    voxel.sm = saturate(csm.sm);
    if (id.x < resolution - 1 && id.y < resolution - 1 && id.z < resolution - 1 && id.x > 1 && id.y > 1 && id.z > 1 && quality == 1)
    {
        float originDensity = voxel.density;
        float densityX = voxelsBuffer[flt(id + uint3(1, 0, 0))].density;
        float densityY = voxelsBuffer[flt(id + uint3(0, 1, 0))].density;
        float densityZ = voxelsBuffer[flt(id + uint3(0, 0, 1))].density;
        if (originDensity < 0 ^ densityX < 0) PlaceVoxelDetailEdge(lerp(id, id + uint3(1, 0, 0), unlerp(originDensity, densityX, isolevel)) * chunkScaling + offset, id / (float)resolution, normal);
        if (originDensity < 0 ^ densityY < 0) PlaceVoxelDetailEdge(lerp(id, id + uint3(0, 1, 0), unlerp(originDensity, densityY, isolevel)) * chunkScaling + offset, id / (float)resolution, normal);
        if (originDensity < 0 ^ densityZ < 0) PlaceVoxelDetailEdge(lerp(id, id + uint3(0, 0, 1), unlerp(originDensity, densityZ, isolevel)) * chunkScaling + offset, id / (float)resolution, normal);
            }
            voxelsBuffer[index] = voxel;
        }");
        /*
        using (StreamWriter stream = File.CreateText(path))
        {
            stream.Write(builder.ToString());
        }
        AssetDatabase.Refresh();
        */
    }

    /// <summary>
    /// Evaluate a specific input port
    /// </summary>
    public static string EvaluatePort(SavedLocalVoxelGraph graph, string portguid, object defaultObj) 
    {
        Debug.Log(portguid);
        //Check if this is a const port and if it already have a valid code representation
        if (!savedEdges.ContainsKey(portguid))
        {
            //This port is a const
            return defaultObj.ToString();
        }
        else
        {
            //Gotta go back the "tree"
            VoxelNode node = savedNodes[savedEdges[portguid].output.nodeGuid].nodeData.voxelNode;
            return node.CodeRepresentationPort(graph, savedEdges[portguid].output.portGuid);
        }
        return null;
    }
}
