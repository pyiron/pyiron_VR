using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathButton : MonoBehaviour, IButton
{
    internal int id;
    
    public void Update()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    public void WhenClickDown()
    {
        ExplorerMenuController.inst.LoadPathContent(GetCurrPath(), true);
    }

    private string GetCurrPath()
    {
        string currPath = "";

        string[] parts = ExplorerMenuController.currPath.Split('/');
        for (int i = 0; i <= id; i++)
        {
            currPath += parts[i] + "/";
        }
        /*foreach (PathButton but in transform.parent.GetComponentsInChildren<PathButton>())
        {
            currPath += but.GetComponentInChildren<Text>().text;
            if (but == this)
                break;
            currPath += "/";
        }*/
        return currPath;
    }
}
