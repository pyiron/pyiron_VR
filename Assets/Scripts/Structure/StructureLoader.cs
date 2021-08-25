using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Networking;
using UI.Log;
using UnityEngine;

public class StructureLoader
{
    //private static string structureDataBuffer;
    private static StringBuilder structureDataBuffer = new StringBuilder();

    // signals if this is the first time data of a new job has arrived, needed for initializtaion
    public static bool isFirstDatapart = true;

    private static int loadedFrames;

    private static Vector3[] streamedPositions;

    public static Vector3[][] GetFramePositions(Vector3[] flattenedArray, int struc_len)
    {
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
        int existingFrames = 0;
        if (!isFirstDatapart)
        {
            Array.Copy(AnimationController.positionData, 0, streamedFrames, 0, AnimationController.positionData.Length);
            existingFrames = AnimationController.positionData.Length;
        }

        for (int i = existingFrames; i < streamedFrames.Length; i++)
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
        
        StringBuilder dataMsg = new StringBuilder(data.msg);
        StringBuilder msgBackup = new StringBuilder(structureDataBuffer.ToString());

        string newStructureDataBuffer = "";
        //string msgBackup = structureDataBuffer + data.msg;
        msgBackup.Append(data.msg);
        if (isFirstDatapart)
        {
            dataMsg.Insert(0, structureDataBuffer);
            //dataMsg.Append(structureDataBuffer);
            //dataMsg.Append(data.msg);
            data.msg = dataMsg.ToString();
            //data.msg = structureDataBuffer + data.msg;
            //structureDataBuffer = "";
            structureDataBuffer.Clear();
        }
        if (!data.msgIsComplete)
        {
            if (isFirstDatapart && !data.msg.Contains("\"positions\": ["))
            {
                structureDataBuffer.Append(data.msg);
                //structureDataBuffer += data.msg;
                // At least the data for one whole frame is needed, make sure this is true and handle case if it is not
                return;
            }
            
            // Only part of the structure data has arrived yet. Cut off the last bit, so that a valid structure can be read out
            int lastFullPosInd = data.msg.LastIndexOf('}');
            if (dataMsg.Length > lastFullPosInd + 3)
            {
                newStructureDataBuffer = dataMsg.ToString(lastFullPosInd + 3, dataMsg.Length - (lastFullPosInd + 3));
                //newStructureDataBuffer = data.msg.Substring(lastFullPosInd + 3);
            }

            dataMsg.Remove(lastFullPosInd + 1, dataMsg.Length - (lastFullPosInd + 1));
            dataMsg.Append("]}");
            //data.msg = data.msg.Substring(0,  lastFullPosInd + 1) + "]}";
            //Debug.Log("Received incomplete msg :)");
        }

        if (!isFirstDatapart)
        {
            // Sometimes the data seems to be in an incorrect format, this should fix these cases
            dataMsg.Insert(0, structureDataBuffer);
            //data.msg = structureDataBuffer + data.msg;
            if (dataMsg[0] == ',')
            //if (data.msg[0] == ',')
            {
                //Debug.Log("Strange format: " + structureDataBuffer);
                //Debug.Log("data: " + data.msg);
                dataMsg.Remove(0, 1);
                //data.msg = data.msg.Substring(1);
                //Debug.Log("After fix: " + newStructureDataBuffer);
            }

            dataMsg.Insert(0, "{\"positions\": [");
            //data.msg = "{\"positions\": [" + data.msg;
        }

        structureDataBuffer = new StringBuilder(newStructureDataBuffer);
        //structureDataBuffer = newStructureDataBuffer;
        
        //Debug.Log("debug : " + data.msg.Length);

        StructureData structureData = JsonUtility.FromJson<StructureData>(dataMsg.ToString());
        //StructureData structureData = JsonUtility.FromJson<StructureData>(data.msg);
        
        if (structureData.positions.Length < structureData.size)
        {
            structureDataBuffer = new StringBuilder(msgBackup.ToString());
            //structureDataBuffer = msgBackup.ToString();
            // At least the data for one whole frame is needed, make sure this is true and handle case if it is not
            return;
        }
        
        int structureSize = structureData.size == 0 ? Structure.Inst.AtomAmount() : structureData.size;
        Vector3[][] allPoses = GetFramePositions(structureData.positions, structureSize);
        if (isFirstDatapart)
        {
            Structure.Inst.Activate();
            Structure.Inst.UpdateStructure(allPoses[0], structureData.elements);
            Boundingbox.Inst.UpdateBoundingBox(structureData.cell);
            HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
            // Deactivate the Loading Message
            LogPublisher.ReceiveLogMsg("", LogPublisher.ErrorSeverity.Status);
        }

        AnimationController.Inst.SetNewAnimation(allPoses);
        // signal if the next data that will be received is the first part of a new job
        isFirstDatapart = data.msgIsComplete;
        if (data.msgIsComplete)
        {
            //structureDataBuffer = "";
            structureDataBuffer.Clear();
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
