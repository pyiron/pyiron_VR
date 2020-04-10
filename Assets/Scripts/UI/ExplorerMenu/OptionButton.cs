using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OptionButton : MonoBehaviour, IButton
{
    public bool isJob;

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
    
    public static IEnumerator HandleLoad(string jobName)
    {
        // send the order to load the structure
        //PythonExecuter.SendOrderAsync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, order);

//        string order = "project = unity_manager.project['" + jobName + "']";
//        PythonExecuter.SendOrderSync(PythonScript.UnityManager,
//            PythonCommandType.exec_l, order, handleInput: false);

//        PythonExecuter.SendOrderAsync(PythonScript.UnityManager, PythonCommandType.eval_l, 
//            "send_job()");
        PythonExecuter.SendOrderAsync(PythonScript.Executor, PythonCommandType.eval_l, 
            "load_job(unityManager.project['" + jobName + "'])");

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.taskNumIn;

        // wait until the response to the send message has arrived
        
        yield return new WaitUntil(() => taskNumIn == TCPClient.taskNumOut);

        // get the response
        string result = TCPClient.returnedMsg;
        
        // handle the response
//        PythonExecuter.HandlePythonMsg(result);

//        print(result);
        StructureData structureData = JsonUtility.FromJson<StructureData>(result);
        Vector3[][] allPoses = GetFramePositions(structureData.positions, structureData.size,
            structureData.frames);
        Structure.Inst.UpdateStructure(allPoses[0], structureData.elements);
        Boundingbox.Inst.UpdateBoundingBox(structureData.cell);
        AnimationController.Inst.SetNewAnimation(allPoses);
        
        ExplorerMenuController.Inst.Activate();
        //ExplorerMenuController.inst.DeleteOptions();
        //ExplorerMenuController.inst.ClearOptions();
        //SimulationMenuController.jobLoaded = true;
        //ModeController.inst.SetMode(Modes.Calculate);
        //AnimationMenuController.Inst.SetState(true);
        // todo: handle the result here, instead of calling PythonExecuter.HandlePythonMsg
    }

    public void WhenClickDown()
    {
        string job_name = GetComponentInChildren<Text>().text;
        if (isJob)
        {
            StartCoroutine(HandleLoad(job_name));
            ExplorerMenuController.Inst.Deactivate();
//            foreach (Button btn in transform.parent.GetComponentsInChildren<Button>())
//            {
//                btn.interactable = false;
//            }
        }
        else
        {
            ExplorerMenuController.Inst.LoadPathContent(job_name);
        }
    }
}


