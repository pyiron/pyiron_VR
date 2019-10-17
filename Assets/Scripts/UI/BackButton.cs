using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour, IButton
{
    public void WhenClickDown()
    {
        ExplorerMenuController.inst.DeleteOptions();
        //ExplorerMenuController.shouldDelete = true;
        ExplorerMenuController.inst.ClearOptions();
        PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, "..");
    }
}
