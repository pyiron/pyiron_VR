using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// component of CurrentModeText
public class ModeData : MonoBehaviour
{
    [Header("Scene")]
    // reference to the deployed instance of this script
    public static ModeData inst;

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    //public TextMesh CurrentModeText;
    public static Mode currentMode;
    // a timer which will disable the text after a few seconds
    private float modeTextTimer;
    // the reference to the attached Text Object
    private Text textObject;
    // the size the text should have
    private float textSize = 8f;
    // remember the new mode which should be set with the main thread
    internal Modes newMode = Modes.None;

    // the dictionary which defines what properties each mode has
    // attention: the trashcan will just be shown if m_playerCanMoveAtoms is true, even if m_showTrashcan is true
    // attention: the mode will just be accessible, if m_playerCanMoveAtoms, m_showInfo or m_canDuplicate is true
    internal static List<Mode> modes = new List<Mode>() {
        new Mode(mode:Modes.Network, hideAtoms: true, showNetwork: true),
        new Mode(mode:Modes.Explorer, hideAtoms: true, showExplorer: true),
        new Mode(mode:Modes.Temperature, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showTemp:true, showTrashcan:true, showPeriodicSystem:true),
        new Mode(mode:Modes.Minimize, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showRelaxation:true, showTrashcan:true, showPeriodicSystem:true),
        new Mode(mode:Modes.View, playerCanMoveAtoms:true),
        new Mode(mode:Modes.Info, showInfo:true)
    };

    private void Awake()
    {
        inst = this;
        currentMode = modes[0];
        textObject = gameObject.GetComponent<Text>();
    }

    private void Start()
    {
        SetMode(modes[(int)currentMode.mode].mode);

        textSize = textSize / ProgramSettings.textResolution * 10;
        transform.localScale = Vector3.one * textSize;
        textObject.fontSize = (int)ProgramSettings.textResolution;

        UpdateScene();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (newMode != Modes.None) { 
            SetMode(newMode);
            newMode = Modes.None;
            }
        if (modeTextTimer > 0)
        {
            if (modeTextTimer - Time.deltaTime <= 0)
            {
                transform.localScale = Vector3.one * textSize;
                gameObject.SetActive(false);
            }
            else if (modeTextTimer - Time.deltaTime < 1)
                // let the text fade away
                transform.localScale -= Vector3.one * textSize * Time.deltaTime;
            modeTextTimer -= Time.deltaTime;
        }
    }

    // change the mode. This includes updating scene, e.g. (de)activating the thermometer or UI
    public void SetMode(Modes newMode)
    {
        if (currentMode != null && !currentMode.showExplorer && !currentMode.showNetwork)
            // stop the currently running animation
            AnimationController.RunAnim(false);
        currentMode = modes[(int)newMode];
        UpdateScene();
    }

    // (de)activate objects in the scene, as well as the menu
    private void UpdateScene() { 
        textObject.text = currentMode.mode + " mode";
        gameObject.SetActive(true);
        modeTextTimer = 3;
        // set the text to it's original size
        transform.localScale =  Vector3.one * textSize;
        //transform.eulerAngles = new Vector3(0, SceneReferences.inst.HeadGO.transform.eulerAngles.y, 0);
        //Vector3 newTextPosition = Vector3.zero;
        //newTextPosition.x += Mathf.Sin(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        //newTextPosition.z += Mathf.Cos(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        //newTextPosition.y = 5;
        //CurrentModeText.transform.position = newTextPosition * 5;

        if (Thermometer.temperature != -1)
            // activate the thermometer when changing into temperature mode, else deactivate it
            Thermometer.inst.gameObject.SetActive(modes[(int)currentMode.mode].showTemp);

        if (modes[(int)currentMode.mode].showInfo)
            OrdersToPython.RequestAllForces();
        // deactivate the structure if it shouldn't be shown, else activate it
        StructureData.inst.gameObject.SetActive(!modes[(int)currentMode.mode].hideAtoms);

        UpdateMenu();

        foreach (LaserGrabber lg in LaserGrabber.instances)
            if (lg.gameObject.activeSelf)
            {
                // activate the symbols of the controller, if changing into a mode which can play an animation, else deactivate them
                if (modes[(int)currentMode.mode].showTemp || modes[(int)currentMode.mode].showRelaxation)
                {
                    LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].Symbols.SetActive(true);
                    LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].SetSymbol();
                }
                else {
                    GameObject symbols = LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].Symbols;
                    if (symbols != null)
                        symbols.SetActive(false);
                }

                // detach the currently attached object from the laser and deactivate the laser
                lg.attachedObject = null;
                lg.laser.SetActive(false);
                lg.readyForResize = false;
                lg.InfoText.gameObject.SetActive(false);
        }
    }

    // determine which panels and buttons should be activated/deactivated
    private void UpdateMenu()
    {
        NetworkMenuController.inst.SetState(currentMode.showNetwork);
        StructureMenuController.inst.SetState(currentMode.showExplorer);
        TemperatureMenuController.inst.SetState(currentMode.showTemp);
        ModeMenuController.inst.SetState(currentMode.mode != Modes.Explorer && currentMode.mode != Modes.Network);
        ModeMenuController.inst.OnModeChange();
        AnimationMenuController.inst.SetState(currentMode.mode == Modes.Temperature ||
                                              currentMode.mode == Modes.Minimize || currentMode.mode == Modes.View);
        InfoMenuController.inst.SetState(currentMode.showInfo);
        //StructureMenuController.inst.transform.parent.gameObject.SetActive(currentMode.mode == Modes.Explorer);
        //StructureCreatorMenuController.inst.SetState(currentMode.showPeriodicSystem);
        //if (currentMode.showPeriodicSystem)
        //    StructureCreatorMenuController.inst.OnModeChange();
        PeriodicSysMenuController.inst.SetState(currentMode.showPeriodicSystem);
    }
}
