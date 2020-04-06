using System;
using System.Collections;
using System.Collections.Generic;
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
        
//        StructureData sd = new StructureData();
//        sd.positions = new Vector3[0];
//        sd.cell = new[] {new Vector3(1.57f, 3.6f, 7.7f), new Vector3(5.0f, 0f)};
//        print(JsonUtility.ToJson(sd));
//        JsonUtility.FromJson<StructureData>(JsonUtility.ToJson(sd));
    }
    private void LoadStructure(string structure)
    {
        StructureData struc;
        if (structure.StartsWith("Error"))
        {
            ErrorText.text = structure;
            return;
        }

        ErrorText.text = "";
            
        struc = JsonUtility.FromJson<StructureData>(structure);

        string elm = struc.elements[0];
            
        bool foundValue = false;
        for (int i = 0; i < elementDropdown.options.Count; i++)
        {
            if (elm == elementDropdown.options[i].text)
            {
                elementDropdown.value = i;
                foundValue = true;
            }
        }

        if (!foundValue)
        {
            List<Dropdown.OptionData> options = elementDropdown.options;
            options.Add(new Dropdown.OptionData(elm));
            elementDropdown.value = options.Count - 1;
        }

//        repeatDropdown.value = 0;
            
        // set cubic and orthorhombic to default value? Or maybe load it somehow
            
            
        // feed the data into the ImportStructure script to create the new structure or update it
        Structure.Inst.UpdateStructure(struc.positions, struc.elements);
        Boundingbox.Inst.UpdateBoundingBox(struc.cell);
        HourglassActivator.Inst.transform.localPosition = Boundingbox.Inst.mid;
    }
    
    public void OnModeStart()
    {
        // Load the data
        string order = "get_data()";
        string structure = PythonExecuter.SendOrderSync(PythonScript.StructureManager, PythonCommandType.eval_l, order);

        // visualize the structure
        LoadStructure(structure);
    }

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

[Serializable]
class StructureData
{
    public string[] elements;

    public int size;

    public int frames;

    public Vector3[] positions;

    public Vector3[] cell;
}
