using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StructureMenuController : MenuController
{
    public static StructureMenuController Inst;

    public Dropdown elementDropdown;
    public Dropdown repeatDropdown;
    public Toggle cubicToggle;
    public Toggle orthorombicToggle;
    public Text ErrorText;

    private void Awake()
    {
        Inst = this;
    }

    private void LoadStructure(string structure)
    {
        print(structure);
        if (structure.StartsWith("Error"))
        {
            ErrorText.text = structure;
        }
        else
        {
            ErrorText.text = "";
            
            for (int i = 0; i < elementDropdown.options.Count; i++)
            {
                if (structure.StartsWith(elementDropdown.options[i].text))
                {
                    elementDropdown.value = i;
                }
            }

            repeatDropdown.value = 0;
            
            // set cubic and orthorhombic to default value? Or maybe load it somehow
        }
        // feed the data into the ImportStructure script to create the new structure or update it
//        ImportStructure.inst.
    }
    
    public void OnModeStart()
    {
        // Load the data
        string order = "get_data()";
        string structure = PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.eval_l, order);

        // visualize the structure
        LoadStructure(structure);
    }

//    public void OnElementChange(Dropdown elementDropdown)
//    {
//        string order = "structure.name = '" + elementDropdown.options[elementDropdown.value].text + "'";
//        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
//        LoadStructure();
//    }
//    
//    public void OnRepeatChange(Dropdown repeatDropdown)
//    {
//        string val = repeatDropdown.options[repeatDropdown.value].text;
//        string order = "structure = structureManager.structure.repeat([" + val + ", " + val + ", " + val + "])";
//        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
//        LoadStructure();
//    }
//    
//    public void OnCubicChange(Toggle cubicToggle)
//    {
//        string isOn = "False";
//        if (cubicToggle.isOn)
//        {
//            isOn = "True";
//        }
//        string order = "structure.cubic = " + isOn;
//        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
//        LoadStructure();
//    }
//    
//    public void OnOrthorombicChange(Toggle orthorombicToggle)
//    {
//        string isOrthorombic = "False";
//        if (orthorombicToggle.isOn)
//        {
//            isOrthorombic = "True";
//        }
//        string order = "structure.cubic = " + isOrthorombic;
//        PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.exec_l, order);
//        LoadStructure();
//    }

    public void OnStructureChange()
    {
        string isCubic = "False";
        if (cubicToggle.isOn)
        {
            isCubic = "True";
        }
        
        string isOrthorombic = "False";
        if (orthorombicToggle.isOn)
        {
            isOrthorombic = "True";
        }
        
        string repeat = repeatDropdown.options[repeatDropdown.value].text;

        string element = "'" + elementDropdown.options[elementDropdown.value].text + "'";

        string order = "create(" + element + ", " + repeat + ", " + isCubic + ", " + isOrthorombic + ")";
        string structure = PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.eval_l, order);
        LoadStructure(structure);
    }
}
