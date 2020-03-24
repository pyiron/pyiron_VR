using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OptionButton : MonoBehaviour, IButton
{
    public bool isJob;

    private IEnumerator HandleLoad(string jobName)
    {
        // send the order to load the structure
        //PythonExecuter.SendOrderAsync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, order);
        
        PythonExecuter.SendOrderSync(PythonScript.None,
            PythonCommandType.exec_l, "unity_manager.pr = unity_manager.pr['" + jobName + "']", handleInput: false);

        PythonExecuter.SendOrderAsync(PythonScript.None, PythonCommandType.eval_l, 
            "unity_manager.send_job()");

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.taskNumIn;

        // wait until the response to the send message has arrived
        
        yield return new WaitUntil(() => taskNumIn == TCPClient.taskNumOut);

        // get the response
        string result = TCPClient.returnedMsg;
        
        // handle the response
        PythonExecuter.HandlePythonMsg(result);
        ExplorerMenuController.inst.DeleteOptions();
        ExplorerMenuController.inst.ClearOptions();
        ModeController.inst.SetMode(Modes.Animate);
        // todo: handle the result here, instead of calling PythonExecuter.HandlePythonMsg
    }

    public void WhenClickDown()
    {
        string job_name = GetComponentInChildren<Text>().text;
        if (isJob)
        {
            StartCoroutine(HandleLoad(job_name));
            foreach (Button btn in transform.parent.GetComponentsInChildren<Button>())
            {
                btn.interactable = false;
            }
        }
        else
        {
            ExplorerMenuController.inst.LoadPathContent(job_name);
        }
    }
}


