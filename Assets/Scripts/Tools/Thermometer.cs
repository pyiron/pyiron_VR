using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thermometer : MonoBehaviour {

    // the Settings of the program
    private ProgramSettings Settings;

    // the reference to the animationController of the thermometer
    private Animator anim;
    // the text on the thermometer how high the temperature currently is
    private TextMesh ThermometerText;
    // the size the text on the thermometer telling the temperature should have
    private float TextSize = 0.4f;
    // the max temperature you can set with the thermometer and when the thermometer won't show any higher temperatures
    private float maxTemperature = 10000;

    private void Awake()
    {
        // get the reference to the animationController of the thermometer
        anim = gameObject.GetComponent<Animator>();
        // get the Settings of the program
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
        print(Settings.temperature / maxTemperature);
        anim.SetFloat("Temperature", Settings.temperature / maxTemperature);
        print("get " + anim.GetFloat("Temperature"));
    }
}
