using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationModeManager : MonoBehaviour
{
    public static SimModes CurrMode = SimModes.None;
    private Button[] _modes; 
    
    void Start()
    {
        _modes = GetComponentsInChildren<Button>();
        //TemperatureMenuController.Inst.SetState(CurrMode==SimModes.MD);
        OnButtonPressed(_modes[0]);
    }

    public void OnButtonPressed(Button btn)
    {
        print(btn.name + " got pressed");
        foreach (Button otherBtn in _modes)
        {
            if (btn.name == otherBtn.name) continue;
            otherBtn.image.color = Color.white;
            otherBtn.interactable = true;
        }
        btn.image.color = Color.green;
        btn.interactable = false;
        CurrMode = (SimModes)Enum.Parse(typeof(SimModes), btn.name); // can be MD, Minimize or Static 
        TemperatureMenuController.Inst.SetState(CurrMode==SimModes.MD);
        MinimizeMenuController.Inst.SetState(CurrMode == SimModes.Minimize);
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
}

public enum SimModes
{
    MD, Minimize, Static, None
}
