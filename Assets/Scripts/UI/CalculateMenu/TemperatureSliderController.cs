using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureSliderController : MonoBehaviour
{
    public static TemperatureSliderController Inst;
    
    public Slider temp_slider;
    public Text tempText;
    public Text minTempText;
    public Text maxTempText;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        temp_slider.minValue = 1;
        minTempText.text = "" + temp_slider.minValue;
        SetMaxTemperature();
        
    }

    // I think we should manage to leave out the update function
    private void Update()
    {
//        temp_slider.maxValue = Thermometer.maxTemperature;
//        temp_slider.minValue = 1;
//        tempText.text = "Temperature: " + temp_slider.value;
//        minTempText.text = "" + temp_slider.minValue;
//        maxTempText.text = "" + temp_slider.maxValue;
    }

    public void UpdateSlider()
    {
        tempText.text = "Temperature: " + Thermometer.temperature;
        temp_slider.value = Thermometer.temperature;
    }

    public void SetMaxTemperature()
    {
        temp_slider.maxValue = Thermometer.maxTemperature;
        maxTempText.text = "" + temp_slider.maxValue;
    }

    public void ChangeTemperature()
    {
        int sliderVal = (int) temp_slider.value;
        Thermometer.temperature = sliderVal;
        Thermometer.Inst.UpdateTemperature(sliderVal);
        tempText.text = "Temperature: " + Thermometer.temperature;
    }
}
