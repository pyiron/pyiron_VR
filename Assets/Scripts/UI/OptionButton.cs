using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OptionButton : MonoBehaviour, IButton
{
    public bool isJob;

    public void Update()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    private IEnumerator HandleLoad(string order)
    {
        // send the order to load the structure
        PythonExecuter.SendOrderAsync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, order);

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.taskNumIn;

        print(taskNumIn + " vs " + TCPClient.taskNumOut);
        // wait until the response to the send message has arrived
        
        yield return new WaitUntil(() => taskNumIn == TCPClient.taskNumOut);
        print(taskNumIn + " vs " + TCPClient.taskNumOut);

        // get the response
        string result = TCPClient.returnedMsg;

        // handle the response
        PythonExecuter.HandlePythonMsg(result);
        ExplorerMenuController.inst.DeleteOptions();
        ExplorerMenuController.inst.ClearOptions();
        ModeData.inst.SetMode(Modes.Temperature);
        // todo: handle the result here, instead of calling PythonExecuter.HandlePythonMsg
    }

    public void WhenClickDown()
    {
        string job_name = GetComponentInChildren<Text>().text;
        if (isJob)
        {
            StartCoroutine(HandleLoad(job_name));
            GetComponent<Button>().interactable = false;
        }
        else
        {
            ExplorerMenuController.inst.DeleteOptions();
            ExplorerMenuController.inst.ClearOptions();
            PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, job_name);
        }
    }
}
