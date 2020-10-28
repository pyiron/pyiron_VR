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
        
        PythonExecuter.SendOrderAsync(PythonScript.executor, PythonCommandType.eval_l, 
            "load_job(" + PythonScript.unityManager + ".project['" + jobName + "'])", OnStructureDataReceived);
    }

    public static IEnumerator HandleLoad(string jobName)
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
    }

    private static void OnStructureDataReceived(string data)
    {
        StructureLoader.LoadAnimation(data);
        
        ExplorerMenuController.Inst.Activate();
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


