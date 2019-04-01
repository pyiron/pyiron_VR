using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureMenuController : MenuController {
    public static TemperatureMenuController inst;
    private Slider temp_slider;
    public Text tempText;
    public Text minTempText;
    public Text maxTempText;

    private void Awake()
    {
        inst = this;
        temp_slider = GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        temp_slider.maxValue = Thermometer.maxTemperature;
        temp_slider.minValue = 1;
        tempText.text = "Temperature: " + temp_slider.value;
        minTempText.text = "" + temp_slider.minValue;
        maxTempText.text = "" + temp_slider.maxValue;
    }

    public void OnTemperatureChange()
    {
        Thermometer.temperature = (int)temp_slider.value;
        Thermometer.inst.UpdateTemperature((int)temp_slider.value);
    }

    public void ChangeTemperature()
    {
        temp_slider.value = Thermometer.temperature;
    }
}
