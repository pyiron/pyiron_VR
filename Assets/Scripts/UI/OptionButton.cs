using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour, IButton {
    public bool isJob;

    public void Update()
    {
        transform.localEulerAngles = Vector3.zero;
    }

    public void WhenClickDown()
    {
        ExplorerMenuController.inst.DeleteOptions();
        //ExplorerMenuController.shouldDelete = true;
        ExplorerMenuController.inst.ClearOptions();
        PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer, PythonCommandType.pr_input, GetComponentInChildren<Text>().text);
        if (isJob)
            ModeData.inst.SetMode(Modes.Temperature);
    }
}
