using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureMenuController : MenuController
{
    public static StructureMenuController Inst;

    private void Awake()
    {
        Inst = this;
    }

    private void LoadStructure()
    {
        // Load the data
        string order = "get_data()";
        string structure = PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.eval_l, order);
        print(structure);
        // feed the data into the ImportStructure script to create the new structure or update it
//        ImportStructure.inst.
    }
    
    public void OnModeStart()
    {
        LoadStructure();
    }

    public void OnElementChange(Dropdown elementDropdown)
    {
        string order = "structure.name = '" + elementDropdown.options[elementDropdown.value].text + "'";
        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
        LoadStructure();
    }
    
    public void OnRepeatChange(Dropdown repeatDropdown)
    {
        string val = repeatDropdown.options[repeatDropdown.value].text;
        string order = "structure = structureManager.structure.repeat([" + val + ", " + val + ", " + val + "])";
        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
        LoadStructure();
    }
    
    public void OnCubicChange(Toggle cubicToggle)
    {
        string isOn = "False";
        if (cubicToggle.isOn)
        {
            isOn = "True";
        }
        string order = "structure.cubic = " + isOn;
        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
        LoadStructure();
    }
    
    public void OnOrthorombicChange(Toggle orthorombicToggle)
    {
        string isOrthorombic = "False";
        if (orthorombicToggle.isOn)
        {
            isOrthorombic = "True";
        }
        string order = "structure.cubic = " + isOrthorombic;
        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
        LoadStructure();
    }
}
