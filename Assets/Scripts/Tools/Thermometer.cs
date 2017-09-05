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
    // the renderer of the thermometer
    private Renderer ThermometerRenderer;
    // the size the text on the thermometer telling the temperature should have
    private float TextSize = 0.4f;
    // the max temperature you can set with the thermometer and when the thermometer won't show any higher temperatures
    private int maxTemperature = 10000;
    // the color of the liquid of the thermometer
    private Color liquidColor = Color.red;
    // the height from where on the thermometer won't show any temperature changes any more, if the controller shows above
    private float highestPoint = 1.76f;
    // the height from where on the thermometer won't show any temperature changes any more, if the controller shows under this height
    private float lowestPoint = 0.31f;
    // determines in which intervals the thermometer should update the temperature text
    public float temperatureStep = 1000;


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
        
        // get the reference to the Renderer of the thermometer
        foreach (Transform Trans in transform)
            if (Trans.name == "Liquid")
                ThermometerRenderer = Trans.GetComponent<Renderer>();

        // make sure the color is the usual one
        ChangeLiquidColor();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateTemperature(bool round=false)
    {
        ThermometerText.text = (Mathf.Round(Settings.temperature / temperatureStep) * temperatureStep).ToString();
        anim.SetFloat("Temperature", (float)Mathf.Round(Settings.temperature / temperatureStep) * temperatureStep / maxTemperature);
    }

    public void ChangeLiquidColor(string state="")
    {
        if (state == "clicked")
            ThermometerRenderer.materials[0].color = new Color(0.7f, 0, 0);
        else if (state == "clickedButMovedAway")
            ThermometerRenderer.materials[0].color = new Color(0.85f, 0, 0);
        else
            ThermometerRenderer.materials[0].color = liquidColor;
    }

    public void ChangeThemperature(float hitPointHeight)
    {
        Settings.temperature = (int)(maxTemperature * (hitPointHeight - lowestPoint) / (highestPoint - lowestPoint));
        UpdateTemperature(round:true);
    }
}
