using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// component of MyObjects/Thermometer
public class Thermometer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // reference to the Thermometer
    public static Thermometer inst;
    // the reference to the animationController of the thermometer
    private Animator anim;
    // the text on the thermometer how high the temperature currently is
    private TextMesh ThermometerText;
    // the renderer of the thermometer
    private Renderer ThermometerRenderer;
    // the current temperature
    public static int temperature = 0;
    // the size the text on the thermometer telling the temperature should have
    private float TextSize = 0.4f;
    // the max temperature you can set with the thermometer and when the thermometer won't show any higher temperatures
    internal static int maxTemperature = 10000;
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

    // shows if the user hitted the thermometer with the laser when he pressed the hair trigger down the last time
    internal static bool laserOnThermometer;
    // shows whether the laser is currently pointing at the thermometer
    internal static bool laserCurrentlyOnThermometer;


    private void Awake()
    {
        // instantiate the reference to the thermometer
        inst = this;
        // get the reference to the animationController of the thermometer
        anim = gameObject.GetComponent<Animator>();
        // get the reference to the TextMesh of the temperature
        ThermometerText = GetComponentInChildren<TextMesh>();
    }
    
    void Start () {
        //UpdateTemperature();
        ThermometerText.transform.localScale = Vector3.one * TextSize;
        
        // get the reference to the Renderer of the thermometer
        foreach (Transform Trans in transform)
            if (Trans.name == "Liquid")
                ThermometerRenderer = Trans.GetComponent<Renderer>();

        // make sure the color is the usual one
        ChangeLiquidColor();

        // set lastTemperature to the value the thermometer has been initialised with
        lastTemperature = temperature;
        
        // update the thermometer
        UpdateTemperature();
    }

    private void Update()
    {
        //activate Thermometer
        //if (temperature != -1)
        //{
            // activate the thermometer when changing into temperature mode, else deactivate it
            //inst.SetState(ModeData.currentMode.showTemp);
        //inst.UpdateTemperature();
        //}
    }

    // a method to set the state of the thermometer object to see which scripts  change the state
    public void SetState(bool newState)
    {
        gameObject.SetActive(newState);
    }

    // show the current temperature data on the thermometer
    public void UpdateTemperature(int exactTemperature = -1)
    {
        // set the current temperature on the text field
        ThermometerText.text = temperature.ToString();
        float tmp = float.NaN;
        // set the red liquid to the right state / up to the right height
        if (exactTemperature != -1)
            tmp = (float) exactTemperature / maxTemperature;
            // anim.SetFloat("Temperature", (float)exactTemperature / maxTemperature);
        else
            // set the temperature to an exact value, although the temperature is rounded,
            // to make it look smooth how the temperature gets scaled
            if (anim.gameObject.activeSelf)
            {
                tmp = (float) temperature / maxTemperature;
                //anim.SetFloat("Temperature", (float)temperature / maxTemperature);
            }

        if (!float.IsNaN(tmp))
        {
            anim.SetFloat("Temperature", tmp);
            TemperatureMenuController.inst.ChangeTemperature();
        }
    }

    // change the color if the user interacts with the thermometer
    public void ChangeLiquidColor(string state="idle")
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
        // if the user points to a point higher than 1, the temperature will still be 1 and not a value higher than one.
        // This way, the temperature can't get higher than maxTemperature
        if (newTemperatureGradient > 1)
            newTemperatureGradient = 1;

        // set the temperature to the new value
        temperature = (int)(precision * newTemperatureGradient) * maxTemperature / precision;

        // if the temperature would be less or equal 0, the program will set it to 1, because PyIron would crash if it would get the temperature 0
        if (temperature <= 0)
        {
            temperature = 1;
            // if the user points to a point lower than 0, the temperature will still be shown as 0 and not a negative value
            if (newTemperatureGradient <= 0)
                newTemperatureGradient = 0;
        }

        // show the current temperature data on the thermometer
        UpdateTemperature(exactTemperature:(int)(maxTemperature * newTemperatureGradient));
        // a new simulation should be started when the temperature gets changed
        SimulationMenuController.ShouldReload = true;
    }

    // allows to change the maxTemperature
    public void SetMaxTemperature(float change)
    {
        // prevents that the maxTemperature will fall to a value lower than 100, because this would be of no use for the user
        if ((maxTemperature > 100 || change > 1) && (maxTemperature < 100000 || change < 1))
            // changes the current maxTemperature to the new value
            maxTemperature = (int)(maxTemperature * change);
    }

    internal bool SendTemp()
    {
        // send Python the order to change the temperature if the user has changed the temperature on the thermometer
        if (lastTemperature != temperature && temperature > -1)
        {
            PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval, "self.set_temperature(" + temperature + ")");
            // remember that the last ham_lammps has been created with the current temperature
            lastTemperature = temperature;
            // show that the temperature changed
            return true;
        }
        return false;
    }

    #region interaction

    public void OnBeginDrag(PointerEventData eventData)
    {
        laserOnThermometer = true;
        laserCurrentlyOnThermometer = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        bool laserWasOnThermometer = laserCurrentlyOnThermometer;
        laserCurrentlyOnThermometer = eventData.hovered.Contains(gameObject);
        if (laserCurrentlyOnThermometer)
        {
            ChangeThemperature(eventData.pointerCurrentRaycast.worldPosition.y - transform.position.y);
            TemperatureMenuController.inst.ChangeTemperature();

            if (!laserWasOnThermometer)
            {
                // set the color to a dark red to show that the user currently clicks on the thermometer
                ChangeLiquidColor("clicked");
            }

            if (AnimationController.run_anim)
                // stop the animation
                AnimationController.RunAnim(false);
        }
        else
        {
            ChangeLiquidColor("clickedButMovedAway");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        laserOnThermometer = false;
        laserCurrentlyOnThermometer = false;
        ChangeLiquidColor("idle");
    }

    public void TouchpadPressDown(Vector2 touchPos)
    {
        if (laserOnThermometer)
            // increases the maxTemperature by a factor of 10 when pressing on the upper half of the touchpad
            if (touchPos.y > 0)
                SetMaxTemperature(10);
            // decreases the maxTemperature by a factor of 10 when pressing on the lower half of the touchpad, if possible
            else
                SetMaxTemperature(0.1f);
    }

    #endregion
}
