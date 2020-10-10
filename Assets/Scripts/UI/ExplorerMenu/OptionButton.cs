using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OptionButton : MonoBehaviour, IButton
{
    public bool isJob;
    
    public static IEnumerator HandleLoad(string jobName)
    {
        // send the order to load the structure
        PythonExecuter.SendOrderAsync(PythonScript.executor, PythonCommandType.eval_l, 
            "load_job(" + PythonScript.unityManager + ".project['" + jobName + "'])");
        
        // save the job name in the SimulationMenuController, but tell it the structure hasnt been shifted, so that it 
        // wont delete the job
        SimulationMenuController.jobName = jobName;
        if (SimulationMenuController.Inst.IsStructureShifted())
        {
            // TODO: this looks like faulty code
            SimulationMenuController.jobName = SimulationMenuController.jobName;
        }

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.taskNumIn;

        // wait until the response to the send message has arrived
        
        yield return new WaitUntil(() => taskNumIn == TCPClient.taskNumOut);

        // get the response
        string result = TCPClient.returnedMsg;
        
        StructureLoader.LoadAnimation(result);
        
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
            StartCoroutine(HandleLoad(job_name));
            ExplorerMenuController.Inst.Deactivate();
        }
        else
        {
            ExplorerMenuController.Inst.LoadPathContent(job_name);
        }
    }
}


