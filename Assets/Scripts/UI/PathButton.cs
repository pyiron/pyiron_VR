using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathButton : MonoBehaviour, IButton
{
    public void Update()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    public void WhenClickDown()
    {
        ExplorerMenuController.inst.DeleteOptions();
        //ExplorerMenuController.shouldDelete = true;
        ExplorerMenuController.inst.ClearOptions();
        PythonExecuter.SendOrder(PythonScript.ProjectExplorer, PythonCommandType.path, GetCurrPath());
    }

    private string GetCurrPath()
    {
        string currPath = "";
        // todo: store path instead of calculating it to reduce lag
        foreach (PathButton but in transform.parent.GetComponentsInChildren<PathButton>())
        {
            currPath += but.GetComponentInChildren<Text>().text;
            if (but == this)
                break;
            currPath += "/";
        }
        return currPath;
    }
}
