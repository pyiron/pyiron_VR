using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Networking;
using UI.Log;
using UnityEngine;

public class StructureLoader
{
    private static string structureDataBuffer;

    // signals if this is the first time data of a new job has arrived, needed for initializtaion
    public static bool isFirstDatapart = true;

    private static int loadedFrames;

    private static Vector3[] streamedPositions;

    public static Vector3[][] GetFramePositions(Vector3[] flattenedArray, int struc_len)
    {
        // TODO: keep the old loaded frames and only add the new ones
        
        if (isFirstDatapart)
        {
            streamedPositions = flattenedArray;
        }
        else
        {
            streamedPositions = streamedPositions.Concat(flattenedArray).ToArray();
        }
        loadedFrames = streamedPositions.Length / struc_len;

        Vector3[][] streamedFrames = new Vector3[loadedFrames][];
        for (int i = 0; i < streamedFrames.Length; i++)
        {
            streamedFrames[i] = new Vector3[struc_len];
            Array.Copy(streamedPositions, i * struc_len, streamedFrames[i], 0, struc_len);
        }

        return streamedFrames;
        
        /*Vector3[][] all_frames = new Vector3[frames][];
        for (int i = 0; i < frames; i++)
        {
            all_frames[i] = new Vector3[struc_len];
            Array.Copy(flattenedArray, i * struc_len, all_frames[i], 0, struc_len);
        }

        return all_frames;*/
    }
    
    public static void LoadAnimation(ReturnedMessage data)
    {
        if (data.msg.StartsWith("error:", true, CultureInfo.CurrentCulture))
        {
            LogPublisher.ReceiveLogMsg(data.msg);
            return;
        }

        string newStructureDataBuffer = "";
        string msgBackup = structureDataBuffer + data.msg;
        if (isFirstDatapart)
        {
            data.msg = structureDataBuffer + data.msg;
            structureDataBuffer = "";
        }
        if (!data.msgIsComplete)
        {
            if (isFirstDatapart && !data.msg.Contains("\"positions\": ["))
            {
                structureDataBuffer += data.msg;
                // At least the data for one whole frame is needed, make sure this is true and handle case if it is not
                return;
            }
            
            // Only part of the structure data has arrived yet. Cut off the last bit, so that a valid structure can be read out
            int lastFullPosInd = data.msg.LastIndexOf('}');
            if (data.msg.Length > lastFullPosInd + 3)
            {
                newStructureDataBuffer = data.msg.Substring(lastFullPosInd + 3);
            }

            data.msg = data.msg.Substring(0,  lastFullPosInd + 1) + "]}";
            //Debug.Log("Received incomplete msg :)");
        }

        if (!isFirstDatapart)
        {
            
            // Sometimes the data seems to be in an incorrect format, this should fix these cases
            data.msg = structureDataBuffer + data.msg;
            if (data.msg[0] == ',')
            {
                //Debug.Log("Strange format: " + structureDataBuffer);
                //Debug.Log("data: " + data.msg);
                data.msg = data.msg.Substring(1);
                //Debug.Log("After fix: " + newStructureDataBuffer);
            }
            
            data.msg = "{\"positions\": [" + data.msg;
        }
        structureDataBuffer = newStructureDataBuffer;
        
        Debug.Log("debug : " + data.msg);

        StructureData structureData = JsonUtility.FromJson<StructureData>(data.msg);
        
        if (structureData.positions.Length < structureData.size)
        {

            structureDataBuffer = msgBackup;
            // At least the data for one whole frame is needed, make sure this is true and handle case if it is not
            return;
        }
        
        int structureSize = structureData.size == 0 ? Structure.Inst.AtomAmount() : structureData.size;
        Vector3[][] allPoses = GetFramePositions(structureData.positions, structureSize);
        if (isFirstDatapart)
        {
            Structure.Inst.UpdateStructure(allPoses[0], structureData.elements);
            Boundingbox.Inst.UpdateBoundingBox(structureData.cell);
            HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
        }

        AnimationController.Inst.SetNewAnimation(allPoses);
        // signal if the next data that will be received is the first part of a new job
        isFirstDatapart = data.msgIsComplete;
        if (data.msgIsComplete)
        {
            structureDataBuffer = "";
        }
    }

    public static void LoadStaticStructure(StructureData struc)
    {
        
        // feed the data into the ImportStructure script to create the new structure or update it
        Structure.Inst.UpdateStructure(struc.positions, struc.elements);
        Boundingbox.Inst.UpdateBoundingBox(struc.cell);
        HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
    }
}
