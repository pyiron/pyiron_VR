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
    private int precision = 100;
    // the temperature of last calculated ham_lammps
    public int lastTemperature;


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

        // set lastTemperature to the value the thermometer has been initialised with
        lastTemperature = Settings.temperature;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // show the current temperature data on the thermometer
    public void UpdateTemperature(int exactTemperature = -1)
    {
        // set the current temperature on the text field
        ThermometerText.text = Settings.temperature.ToString();
        // set the red liquid to the right state / up to the right height
        if (exactTemperature != -1)
            anim.SetFloat("Temperature", (float)exactTemperature / maxTemperature);
        else
            // set the temperature to an exact value, although the temperature is rounded,
            // to make it look smooth how the temperature gets scaled
            anim.SetFloat("Temperature", (float)Settings.temperature / maxTemperature);
    }

    // change the color if the user interacts with the thermometer
    public void ChangeLiquidColor(string state="")
    {
        // set the color to a dark red if the user currently clicks on the thermometer
        if (state == "clicked")
            ThermometerRenderer.materials[0].color = new Color(0.7f, 0, 0);
        // set the color to a slightly dark red if the user has clicked on the thermometer but moved the laser away from it,
        // while still pressing the trigger
        else if (state == "clickedButMovedAway")
            ThermometerRenderer.materials[0].color = new Color(0.85f, 0, 0);
        // set it to the usual bright red color
        else
            ThermometerRenderer.materials[0].color = liquidColor;
    }

    // change the temperature, according to where the user is pointing
    public void ChangeThemperature(float hitPointHeight)
    {
        // calculate where the user is pointing at
        float newTemperatureGradient = (hitPointHeight - lowestPoint) / (highestPoint - lowestPoint);
        // if the user points to a point lower than 0, the temperature will still be 0 and not a negative value
        if (newTemperatureGradient < 0)
            newTemperatureGradient = 0;
        // if the user points to a point higher than 1, the temperature will still be 1 and not a value higher than one.
        // This way, the temperature can't get higher than maxTemperature
        else if (newTemperatureGradient > 1)
            newTemperatureGradient = 1;

        // set the temperature to the new value
        Settings.temperature = (int)(precision * newTemperatureGradient) * maxTemperature / precision;
        // show the current temperature data on the thermometer
        UpdateTemperature(exactTemperature:(int)(maxTemperature * newTemperatureGradient));
    }

    // allows to change the maxTemperature
    public void SetMaxTemperature(float change)
    {
        // prevents that the maxTemperature will fall to a value lower than 100, because this would be of no use for the user
        if (maxTemperature > 100 || change > 1)
            // changes the current maxTemperature to the new value
            maxTemperature = (int)(maxTemperature * change);
    }
}
