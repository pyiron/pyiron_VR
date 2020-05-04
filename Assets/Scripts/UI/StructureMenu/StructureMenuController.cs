using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureMenuController : MenuController
{
    public static StructureMenuController Inst;

//    public Dropdown elementDropdown;
    public Button elementButton;
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

    internal override void SetState(bool active)
    {
        base.SetState(active);
        elementButton.interactable = true;
    }

//    public void SetElementDropdown(string newElement)
//    {
//        bool foundValue = false;
//        for (int i = 0; i < elementDropdown.options.Count; i++)
//        {
//            if (newElement == elementDropdown.options[i].text)
//            {
//                elementDropdown.value = i;
//                foundValue = true;
//            }
//        }
//
//        if (!foundValue)
//        {
//            List<Dropdown.OptionData> options = elementDropdown.options;
//            options.Add(new Dropdown.OptionData(newElement));
//            elementDropdown.value = options.Count - 1;
//        }
//    }

    public void UpdateElementButton(string newElement)
    {
        elementButton.GetComponentInChildren<Text>().text = newElement;
        elementButton.image.color = LocalElementData.GetColour(newElement);
    }
    
    private void LoadStructure(string structure)
    {
        StructureData struc;
        if (structure == "")
        {
            ErrorText.text = "Couldn't load the new structure. Please try again!";
            return;
        }
        if (structure.StartsWith("Error"))
        {
            ErrorText.text = structure;
            return;
        }

        ErrorText.text = "";
            
        struc = JsonUtility.FromJson<StructureData>(structure);

//        SetElementDropdown(struc.elements[0]);
        UpdateElementButton(struc.elements[0]);
        
        // send the new structureName to the CalculationMenu
        SimulationMenuController.jobName = struc.formula;
        
        StructureLoader.LoadStaticStructure(struc);
    }
    
    public void OnModeStart()
    {
        // delete the animation if it still exists
        if (AnimationController.Inst.HasAnimationLoaded())
        {
            AnimationController.Inst.DeleteAnimation();
        }
        else
        {
            // Load the data
            string order = "get_data()";
            string structure = PythonExecuter.SendOrderSync(PythonScript.structure, PythonCommandType.eval_l, order);
            
            // visualize the structure
            LoadStructure(structure);
        }
    }

    private void UpdateStructure(string order)
    {
        string structure = PythonExecuter.SendOrderSync(PythonScript.structure, PythonCommandType.eval_l, order);
        LoadStructure(structure);
    }

    public void OnElementButtonDown()
    {
        // open periodic table here
        PeriodicTable.Inst.SetState(true);
        elementButton.interactable = false;
    }

    public void OnStructureChange()
    {
        string repeat = repeatDropdown.options[repeatDropdown.value].text;

//        string element = "'" + elementDropdown.options[elementDropdown.value].text + "'";
        string element = "'" + elementButton.GetComponentInChildren<Text>().text + "'";

        UpdateStructure("create(" + element + ", " + repeat + ", " + Utilities.ToggleToPythonBool(cubicToggle) + ", " + 
                        Utilities.ToggleToPythonBool(orthorombicToggle) + ")");
    }

    /// <summary>
    /// not used atm but might be used in the future. Repeats the CURRENT structure instead of the BASE structure.
    /// </summary>
    public void OnRepeatChange()
    {
        string repeat = repeatDropdown.options[repeatDropdown.value].text;
        UpdateStructure("structure.repeat([" + repeat + ", " + repeat + ", " + repeat + "])");
    }
}

[Serializable]
public class StructureData
{
    public string[] elements;

    public int size;

    public int frames;

    public string formula;

    public Vector3[] positions;

    public Vector3[] cell;
}
