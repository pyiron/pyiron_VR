using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathButton : MonoBehaviour, IButton
{
    public void WhenClickDown()
    {
        StructureMenuController.shouldDelete = true;
        StructureMenuController.inst.ClearOptions();
        PythonExecuter.inst.SendOrder("path " + GetCurrPath());
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
