using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButton : MonoBehaviour {
    public void WhenClickDown()
    {
        StructureMenuController.shouldDelete = true;
        StructureMenuController.inst.ClearOptions();
        PythonExecuter.inst.SendOrder("back");
    }
}
