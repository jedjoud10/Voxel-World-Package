using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoxelUtility;
public class TerrainEditor : MonoBehaviour
{
    public GameObject preview;
    private VoxelWorld voxelWorld;
    private Vector3 point;
    public float size = 25;
    public float distance = 100;
    public int shape;
    public int editType;
    // Start is called before the first frame update
    void Start()
    {
        voxelWorld = FindObjectOfType<VoxelWorld>();
        //voxelWorld.terrainGenerated += MassEdits;
    }
    //Mass edits
    public void MassEdits() 
    {
        VoxelEditRequestBatch batch = new VoxelEditRequestBatch();
        for (int i = 0; i < 100; i++)
        {
            Vector3 localPoint = Vector3.Scale(new Vector3(1, 0, 1), Random.insideUnitSphere * 500);
            float localSize = (Random.value + 1) * 80;
            batch.AddVoxelEditRequest(
            new VoxelEditRequest
            {
                editRequest = new EditRequest
                {
                    center = localPoint,
                    color = new Vector3(Random.value, Random.value, Random.value),
                    shape = 1,
                    size = localSize,
                    editType = 1,
                },
                bound = new VoxelAABBBound { min = localPoint - new Vector3(localSize, localSize, localSize), max = localPoint + new Vector3(localSize, localSize, localSize) }
            });
        }
        voxelWorld.voxelEditsManager.Edit(batch);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        point = transform.position + transform.forward * distance;
        if (Input.GetKeyDown(KeyCode.H))
        {
            shape++;
            shape = shape % 3;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            editType++;
            editType = editType % 3;
        }

        //Show preview
        preview.transform.position = point;
        preview.transform.localScale = new Vector3(size, size, size);

        //Edit the terrain when we press the middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            voxelWorld.voxelEditsManager.Edit(
            new VoxelEditRequestBatch(
            new VoxelEditRequest
            {
                editRequest = new EditRequest
                {
                    center = point,
                    color = new Vector3(Random.value, Random.value, Random.value),
                    shape = shape,
                    size = size,
                    editType = editType,
                },
                bound = new VoxelAABBBound { min = point - new Vector3(size, size, size), max = point + new Vector3(size, size, size) }
            }
            ));
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            size += Input.mouseScrollDelta.y * 2;
            size = Mathf.Max(size, 0);
        }
        else
        {
            distance += Input.mouseScrollDelta.y * 2;
            distance = Mathf.Max(distance, 0);
        }
    }
    //Show some debug info
    private void OnGUI()
    {
        GUILayout.Space(300);
        GUILayout.BeginVertical("box");
        GUILayout.Label("Distance: " + distance);
        GUILayout.Label("Size: " + size);
        GUILayout.Label("Shape: " + shape);
        GUILayout.Label("Edit type: " + editType);
        GUILayout.EndVertical();
    }
}
