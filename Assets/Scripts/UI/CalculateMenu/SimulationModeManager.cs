using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationModeManager : MonoBehaviour
{
    public static SimulationModeManager Inst;
    public static SimModes CurrMode = SimModes.MD;
    private Button[] _modes;

    private void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        _modes = GetComponentsInChildren<Button>();
        //TemperatureMenuController.Inst.SetState(CurrMode==SimModes.MD);
        OnButtonPressed(_modes[0]);
    }

    public void SetMode(string modeName)
    {
        Button currModeBtn = null;
        foreach (Button otherBtn in _modes)
        {
            if (String.Equals(modeName, otherBtn.name, StringComparison.CurrentCultureIgnoreCase))
            {
                currModeBtn = otherBtn;
                continue;
            }
            otherBtn.image.color = Color.white;
            otherBtn.interactable = true;
        }

        if (currModeBtn != null)
        {
            currModeBtn.image.color = Color.green;
            currModeBtn.interactable = false;
        }
        CurrMode = (SimModes)Enum.Parse(typeof(SimModes), modeName.ToUpper()); // can be MD, Minimize or Static 
        MdMenuController.Inst.SetState(CurrMode==SimModes.MD);
        MinimizeMenuController.Inst.SetState(CurrMode == SimModes.MINIMIZE);
        /*if (btn.name == "MD")
        {
            //activate Thermometer
            //if (Thermometer.temperature != -1)
            //{
                // activate the thermometer when changing into temperature mode, else deactivate it
                
                //Thermometer.inst.UpdateTemperature();
            //}
        }*/
    }

    public void OnButtonPressed(Button btn)
    {
        SetMode(btn.name);
    }
}

public enum SimModes
{
    MD, MINIMIZE, STATIC
}
