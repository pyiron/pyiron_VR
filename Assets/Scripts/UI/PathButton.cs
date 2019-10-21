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
        //ExplorerMenuController.inst.DeleteOptions();
        //ExplorerMenuController.inst.ClearOptions();
        ExplorerMenuController.inst.LoadPathContent(GetCurrPath(), true);
        //PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.path, GetCurrPath());
    }

    private string GetCurrPath()
    {
        string currPath = "";
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
