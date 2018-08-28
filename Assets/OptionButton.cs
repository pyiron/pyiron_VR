using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour {
    public void WhenClickDown()
    {
        print(name + " should be send to Python");
        StructureMenuController.shouldDelete = true;
        StructureMenuController.inst.ClearOptions();
        ModeData.inst.SetMode("Temperature Mode");
        PythonExecuter.inst.SendOrder(GetComponentInChildren<Text>().text);
    }
}
