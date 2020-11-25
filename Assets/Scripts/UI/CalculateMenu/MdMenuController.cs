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

    public void OnModeStart(JobData jobData)
    {
        Thermometer.temperature = jobData.temperature;
        Thermometer.Inst.UpdateTemperature(jobData.temperature);
        Utilities.SetDropdownValue(nIonicStepsDropdown, jobData.n_ionic_steps);
        Utilities.SetDropdownValue(nPrintDropdown, jobData.n_print);
    }

    public void GetData(ref JobData data)
    {
        // int temp = Thermometer.temperature;
        // string n_ionic_steps = Utilities.GetStringValue(nIonicStepsDropdown);
        // string n_print = Utilities.GetStringValue(nPrintDropdown);
        // return new JobData(temperature:temp, nIonicSteps:n_ionic_steps, nPrint:n_print);
        data.temperature = Thermometer.temperature;
        data.n_ionic_steps = Utilities.GetStringValue(nIonicStepsDropdown);
        data.n_print = Utilities.GetStringValue(nPrintDropdown);
    }
}

// public struct MdData
// {
//     public int temperature;
//     public string n_ionic_steps;
//     public string n_print;
//
//     public MdData(int temperature, string nIonicSteps, string nPrint)
//     {
//         this.temperature = temperature;
//         n_ionic_steps = nIonicSteps;
//         n_print = nPrint;
//     }
// }