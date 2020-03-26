using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureMenuController : MenuController {
    public static TemperatureMenuController Inst;
    
    private Slider temp_slider;
    public Text tempText;
    public Text minTempText;
    public Text maxTempText;
    private Dropdown[] _dropdowns;

    internal override void SetState(bool active)
    {
        base.SetState(active);
        Thermometer.inst.SetState(active);
    }

    private void Awake()
    {
        Inst = this;
        temp_slider = GetComponentInChildren<Slider>();
    }

    private void Start()
    {
        _dropdowns = GetComponentsInChildren<Dropdown>();
    }

    private void Update()
    {
        temp_slider.maxValue = Thermometer.maxTemperature;
        temp_slider.minValue = 1;
        tempText.text = "Temperature: " + temp_slider.value;
        minTempText.text = "" + temp_slider.minValue;
        maxTempText.text = "" + temp_slider.maxValue;
    }

    public MdData GetData()
    {
        float temp = Thermometer.temperature;
        string n_ionic_steps = "";
        string n_print = "";
        foreach (Dropdown dropdown in _dropdowns)
        {
            if (dropdown.transform.parent.name == "n_ionic_steps")
            {
                n_ionic_steps = dropdown.options[dropdown.value].text;
            } 
            else if (dropdown.transform.parent.name == "n_print")
            {
                n_print = dropdown.options[dropdown.value].text;
            }
        }
        return new MdData(temp, n_ionic_steps, n_print);
    }

    public void OnTemperatureChange()
    {
        Thermometer.temperature = (int)temp_slider.value;
        Thermometer.inst.UpdateTemperature((int)temp_slider.value);
        // a new simulation should be started when the temperature gets changed
        SimulationMenuController.jobLoaded = true;
    }

    public void ChangeTemperature()
    {
        temp_slider.value = Thermometer.temperature;
    }
}

public struct MdData
{
    public float temperature;
    public string n_ionic_steps;
    public string n_print;

    public MdData(float temperature, string nIonicSteps, string nPrint)
    {
        this.temperature = temperature;
        n_ionic_steps = nIonicSteps;
        n_print = nPrint;
    }
}