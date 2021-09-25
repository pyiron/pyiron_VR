using System;
using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OptionButton : MonoBehaviour, IButton
{
    // the reference to the button component
    //public Button button;
    
    public bool isJob;

    private static StructureData structureData;

    public static void LoadJob(string jobName)
    {
        // save the job name in the SimulationMenuController, but tell it the structure hasnt been shifted, so that it 
        // wont delete the job
        SimulationMenuController.jobName = jobName;
        if (SimulationMenuController.Inst.IsStructureShifted())
        {
            // TODO: this looks like faulty or incomplete code
            SimulationMenuController.jobName = SimulationMenuController.jobName;
        }
        
        PythonExecutor.SendOrderAsync(true, 
            PythonCmd.LoadJob(jobName), OnStructureDataReceived, returnIncompleteMsgs:false);
    }

    /*public static IEnumerator HandleLoad(string jobName)
    {
        // send the order to load the structure
        //PythonExecuter.SendOrderAsync(PythonScript.executor, PythonCommandType.eval_l, 
        //    "load_job(" + PythonScript.unityManager + ".project['" + jobName + "'])");
        
        LoadJob(jobName);

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.TaskNumIn;

        // wait until the response to the send message has arrived
        
        yield return new WaitUntil(() => taskNumIn == TCPClient.TaskNumOut);

        // get the response
        string result = TCPClient.ReturnedMsg;

        OnStructureDataReceived(result);
    }*/

    private static void OnStructureDataReceived(ReturnedMessage data)
    {
        // TODO: really bad hack, pls fix
        /*if (!data.msgIsComplete)
        {
            return;
        }*/
        structureData = JsonUtility.FromJson<StructureData>(data.msg);
        // first case should theoretically not occur anymore
        if (structureData.positions != null)
        {
            print("structureData.positions != null");
            //StructureLoader.LoadAnimation(data);
        }
        else if (!TCPClient.returnBytes)
        {
            TCPClient.returnBytes = true;
            TCPClient._msgLen = structureData.size * structureData.frames * 12;
            TCPClient.currentCallback = new CallbackFunction(OnStructureByteDataReceived, false, true);
        }
        /*else 
        {
            if (data.msgIsComplete)
            {
                TCPClient.returnBytes = false;
            }

            //if (!data.msgIsComplete)
            //{
            //    return;
            //}
            print("structureData.positions is null");
            // num_atoms * num_frames * 3 * size(float)
            //byte[] byteData = TCPClient.GetByteData(structureData.size * structureData.frames * 12);
            byte[] byteData = data.structureData;
            print("System is little Endian: " + BitConverter.IsLittleEndian);
            if (!BitConverter.IsLittleEndian) // TODO: might be wrong and not work on BigEndianSystems
            {
                Array.Reverse(byteData);
            }
            // BitConverter.
            var floatData = new float[byteData.Length / 4];
            var vec3Data = new Vector3[byteData.Length / 4 / 3];
            Buffer.BlockCopy(byteData, 0, floatData, 0, byteData.Length);
            for (int i = 0; i < vec3Data.Length; i++)
            {
                vec3Data[i] = new Vector3(floatData[i * 3], floatData[i * 3 + 1], floatData[i * 3 + 2]);
            }
            //Buffer.BlockCopy(byteData, 0, vec3Data, 0, vec3Data.Length);

            structureData.positions = vec3Data;
            StructureLoader.LoadAnimation(structureData);
        }*/
        //StructureLoader.LoadAnimation(data);

        //if (data.msgIsComplete)
        //{
        ExplorerMenuController.Inst.Activate();
        //}
    }

    private static void OnStructureByteDataReceived(ReturnedMessage data)
    {
        if (!data.msgIsComplete)
        {
            return;
        }
        print("structureData.positions is " + data.structureData.Length);
        // num_atoms * num_frames * 3 * size(float)
        //byte[] byteData = TCPClient.GetByteData(structureData.size * structureData.frames * 12);
        byte[] byteData = data.structureData;
        //print("System is little Endian: " + BitConverter.IsLittleEndian);
        if (!BitConverter.IsLittleEndian) // TODO: might be wrong and not work on BigEndianSystems
        {
            Array.Reverse(byteData);
        }
        // BitConverter.
        var floatData = new float[byteData.Length / 4];
        var vec3Data = new Vector3[byteData.Length / 4 / 3];
        Buffer.BlockCopy(byteData, 0, floatData, 0, data.byteCount);
        for (int i = 0; i < vec3Data.Length; i++)
        {
            vec3Data[i] = new Vector3(floatData[i * 3], floatData[i * 3 + 1], floatData[i * 3 + 2]);
        }
        //Buffer.BlockCopy(byteData, 0, vec3Data, 0, vec3Data.Length);

        structureData.positions = vec3Data;
        //print(structureData.positions.Length);
        StructureLoader.LoadAnimation(structureData);
    }

    public string GetOptionText()
    {
        return GetComponentInChildren<Text>().text;
    }

    public void WhenClickDown()
    {
        string job_name = GetOptionText();
        if (isJob)
        {
            // reactivate the current job button
            ExplorerMenuController.Inst.DeactivateJobButton(job_name);
                
            //StartCoroutine(HandleLoad(job_name));
            ExplorerMenuController.Inst.Deactivate();
            LoadJob(job_name);
        }
        else
        {
            ExplorerMenuController.Inst.LoadPathContent(job_name);
        }
    }
}


