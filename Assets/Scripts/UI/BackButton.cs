using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour, IButton
{
    public void WhenClickDown()
    {
        StructureMenuController.shouldDelete = true;
        StructureMenuController.inst.ClearOptions();
        PythonExecuter.SendOrder(PythonScript.ProjectExplorer, PythonCommandType.pr_input, "..");
    }
}
