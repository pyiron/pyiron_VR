using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UI.Log;
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
        if (data.StartsWith("error:", true, CultureInfo.CurrentCulture))
        {
            LogManager.ReceiveLogMsg(data);
            return;
        }
        
        StructureData structureData = JsonUtility.FromJson<StructureData>(data);
        Vector3[][] allPoses = GetFramePositions(structureData.positions, structureData.size,
            structureData.frames);
        Structure.Inst.UpdateStructure(allPoses[0], structureData.elements);
        Boundingbox.Inst.UpdateBoundingBox(structureData.cell);
        AnimationController.Inst.SetNewAnimation(allPoses);
        HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
    }

    public static void LoadStaticStructure(StructureData struc)
    {
        
        // feed the data into the ImportStructure script to create the new structure or update it
        Structure.Inst.UpdateStructure(struc.positions, struc.elements);
        Boundingbox.Inst.UpdateBoundingBox(struc.cell);
        HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
    }
}
