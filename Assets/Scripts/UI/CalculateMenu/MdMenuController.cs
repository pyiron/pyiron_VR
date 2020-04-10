using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MdMenuController : MenuController {
    public static MdMenuController Inst;
    

    public Dropdown nIonicStepsDropdown;
    public Dropdown nPrintDropdown;

    internal override void SetState(bool active)
    {
        base.SetState(active);
        Thermometer.Inst.SetState(active);
    }

    private void Awake()
    {
        Inst = this;
    }

    public void OnModeStart()
    {
        string order = "format_md_settings()";
        string receivedData = PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.eval_l, order);
        MdData mdData = JsonUtility.FromJson<MdData>(receivedData);

        Thermometer.temperature = mdData.temperature;
        Thermometer.Inst.UpdateTemperature(mdData.temperature);

        Utilities.SetDropdownValue(nIonicStepsDropdown, mdData.n_ionic_steps);
        
        Utilities.SetDropdownValue(nPrintDropdown, mdData.n_print);
    }

    public MdData GetData()
    {
        int temp = Thermometer.temperature;
        string n_ionic_steps = Utilities.GetStringValue(nIonicStepsDropdown);
        string n_print = Utilities.GetStringValue(nPrintDropdown);
        return new MdData(temp, n_ionic_steps, n_print);
    }
}

public struct MdData
{
    public int temperature;
    public string n_ionic_steps;
    public string n_print;

    public MdData(int temperature, string nIonicSteps, string nPrint)
    {
        this.temperature = temperature;
        n_ionic_steps = nIonicSteps;
        n_print = nPrint;
    }
}