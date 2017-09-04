using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thermometer : MonoBehaviour {

    // the Settings of the program
    private ProgramSettings Settings;

    // the text on the thermometer how high the temperature currently is
    private TextMesh ThermometerText;
    // the size the text on the thermometer telling the temperature should have
    private float TextSize = 0.4f;

    private void Awake()
    {

        Settings = GameObject.Find("Settings").GetComponent<ProgramSettings>();
        // get the reference to the TextMesh of the temperature
        ThermometerText = GetComponentInChildren<TextMesh>();
    }

    // Use this for initialization
    void Start () {
        UpdateTemperature();
        ThermometerText.transform.localScale = Vector3.one * TextSize;
        //ThermometerText.transform.eulerAngles = Vector3.up * 225;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateTemperature()
    {
        ThermometerText.text = Settings.temperature.ToString();
    }
}
