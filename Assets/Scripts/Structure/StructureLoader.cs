using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureLoader
{
    public static Vector3[][] GetFramePositions(Vector3[] flattenedArray, int struc_len, int frames)
    {
        Vector3[][] all_frames = new Vector3[frames][];
        for (int i = 0; i < frames; i++)
        {
            all_frames[i] = new Vector3[struc_len];
            Array.Copy(flattenedArray, i * struc_len, all_frames[i], 0, struc_len);
        }

        return all_frames;
    }
    
    public static void LoadAnimation(string data)
    {
        StructureData structureData = JsonUtility.FromJson<StructureData>(data);
        Vector3[][] allPoses = GetFramePositions(structureData.positions, structureData.size,
            structureData.frames);
        Structure.Inst.UpdateStructure(allPoses[0], structureData.elements);
        Boundingbox.Inst.UpdateBoundingBox(structureData.cell);
        AnimationController.Inst.SetNewAnimation(allPoses);
    }
}
