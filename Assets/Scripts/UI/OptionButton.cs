using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour, IButton {
    public void Update()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    public void WhenClickDown()
    {
        StructureMenuController.shouldDelete = true;
        StructureMenuController.inst.ClearOptions();
        if (StructureMenuController.inst.ActiveType().type != OptionType.Folder)
            ModeData.inst.SetMode(Modes.Temperature);
        PythonExecuter.inst.SendOrder(PythonScript.ProjectExplorer, PythonCommandType.pr_input, GetComponentInChildren<Text>().text);
    }
}
