using System;
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
            
            // set the new path
            /*PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer,
                PythonCommandType.exec_l, "pr = unity_manager.pr['" + job_name + "']", handleInput:false);
            ExplorerMenuController.currPath = PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer,
                PythonCommandType.eval_l, "pr.path[:-1]");
            ExplorerMenuController.pathHasChanged = true;
            
            // get the jobs and groups 
            PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.exec_l,
                "print(type(pr.list_all()))");
            PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.exec_l,
                "print(type(pr.list_all()['groups']))");

            String a = "a";
            print(JsonUtility.ToJson(a));
            
            List<string> b = new List<string>();
            b.Add(a);
            print(JsonUtility.ToJson(b));
            
            print("end exp");
            
            //Object o = JsonUtility.FromJson<Object>("");
            List<string> allNodes = 
                JsonUtility.FromJson<List<string>>(
                    PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.eval_l,
                "pr.list_all()['nodes']"));
            print("len is " + allNodes.Count);
            print("nodes are " + allNodes[0]);
            
            Dictionary<string, List<string>> allOptions = 
                JsonUtility.FromJson<Dictionary<string, List<string>>>(
                    PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.eval_l,
                "pr.list_all()"));
            print("All keys yay " + allOptions.Keys.Count);
            print("All values yay " + allOptions.Values);
            print("All nodes yay " + allOptions["nodes"]);
            print("files yay " + allOptions["files"][0]);
            
            /*PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, job_name);
            PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, job_name);*/
        }
    }
}
