using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of CurrentModeText
public class ModeData : MonoBehaviour
{
    [Header("Scene")]
    // reference to the deployed instance of this script
    public static ModeData inst;
    // get the reference to the programm which handles the execution of python
    private PythonExecuter PE;
    // the script that stores the possible orders which can be send to Python
    private OrdersToPython OTP;
    // the reference to the PossiblePythonScripts
    //private GameObject PossiblePythonScripts;

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    public TextMesh CurrentModeText;
    public static Mode currentMode;
    // a timer which will disable the text after a few seconds
    private float modeTextTimer;
    // the size the text should have
    private float textSize = 1f;
    // remember the new mode which should be set with the main thread
    internal Modes newMode = Modes.None;

    /*public readonly Dictionary<int, Mode> modes = new Dictionary<int, Mode> {
        { 0, new Mode(m_name:"Move Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showTemp:true, m_showTrashcan:true) },
        //{ 1, new Mode(m_name:"Relaxation Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showRelaxation:true, m_showTrashcan:true) },
        { 1, new Mode(m_name:"Info Mode", m_showInfo:true) },
        { 2, new Mode(m_name:"Edit Mode", m_canDuplicate:true) },
        };*/

    // the dictionary which defines what properties each mode has
    // attention: the trashcan will just be shown if m_playerCanMoveAtoms is true, even if m_showTrashcan is true
    // attention: the mode will just be accessable, if m_playerCanMoveAtoms, m_showInfo or m_canDuplicate is true
    /*private static readonly Dictionary<int, Mode> modes = new Dictionary<int, Mode> {
        { 0, new Mode(mode:Modes.Explorer, hideAtoms:true, showPossibleStructures:true) },
        { 1, new Mode(mode:Modes.Temperature, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showTemp:true, showTrashcan:true) },
        { 2, new Mode(mode:Modes.Minimize, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showRelaxation:true, showTrashcan:true) },
        { 3, new Mode(mode:Modes.Info, showInfo:true) }
        //{ 3, new Mode(m_name:"Edit Mode", m_canDuplicate:true) },
        };*/
    private static List<Mode> modes = new List<Mode>() {
        new Mode(mode:Modes.Explorer, hideAtoms: true, showPossibleStructures: true),
        new Mode(mode:Modes.Temperature, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showTemp:true, showTrashcan:true),
        new Mode(mode:Modes.Minimize, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showRelaxation:true, showTrashcan:true),
        new Mode(mode:Modes.View, playerCanMoveAtoms:true),
        new Mode(mode:Modes.Info, showInfo:true)
    };

    private void Awake()
    {
        inst = this;
        currentMode = modes[0];
    }

    private void Start()
    {
        SetMode(modes[(int)currentMode.mode].mode);
        // get the reference to the script that handles the connection to python
        PE = SceneReferences.inst.PE;
        // get the reference to the script that stores the possible orders which can be send to Python
        OTP = SceneReferences.inst.OTP;

        textSize = textSize / ProgramSettings.textResolution * 10;
        transform.localScale = Vector3.one * textSize;
        gameObject.GetComponent<TextMesh>().fontSize = (int)ProgramSettings.textResolution;

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
                gameObject.SetActive(false);
            else if (modeTextTimer - Time.deltaTime < 1)
                // let the text fade away
                transform.localScale -= Vector3.one * textSize * Time.deltaTime;
            modeTextTimer -= Time.deltaTime;
        }
    }

    public void SetMode(Modes newMode)
    {
        if (currentMode != null && !currentMode.showExplorer)
            // stop the currently running animation
            OTP.RunAnim(false);
        currentMode = modes[(int)newMode];
        UpdateScene();
    }

    public void RaiseMode()
    {
        if (!currentMode.showExplorer)
            // stop the currently running animation
            OTP.RunAnim(false);
        currentMode = modes[((int)currentMode.mode + 1) % modes.Count];
        UpdateScene();
    }

    private void UpdateScene() { 
        gameObject.GetComponent<TextMesh>().text = currentMode.mode.ToString();
        gameObject.SetActive(true);
        modeTextTimer = 3;
        // set the text to it's original size
        transform.localScale =  Vector3.one * textSize;
        transform.eulerAngles = new Vector3(0, SceneReferences.inst.HeadGO.transform.eulerAngles.y, 0);
        Vector3 newTextPosition = Vector3.zero;
        newTextPosition.x += Mathf.Sin(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        newTextPosition.z += Mathf.Cos(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        CurrentModeText.transform.position = newTextPosition * 5;
        // CurrentModeText.transform.position = HeadTransform.position + Vector3.forward * 5;
        // let the CurrentModeText always look in the direction of the player
        //Face_Player(CurrentModeText.gameObject);

        if (Thermometer.temperature != -1)
            // activate the thermometer when changing into temperature mode, else deactivate it
            Thermometer.inst.gameObject.SetActive(modes[(int)currentMode.mode].showTemp);

        if (modes[(int)currentMode.mode].showInfo)
            OTP.RequestAllForces();
        // deactivate the structure if it shouldn't be shown, else activate it
        StructureData.inst.gameObject.SetActive(!modes[(int)currentMode.mode].hideAtoms);
        // TODO! activate the new UI
        //ChooseStructure.inst.StructButtons.gameObject.SetActive(modes[currentModeNr].showPossibleStructures);

        UpdateMenu();

        foreach (GameObject controller in SceneReferences.inst.Controllers)
            if (controller.activeSelf)
            {
                // activate the symbols of the controller, if changing into a mode which can play an animation, else deactivate them
                if (modes[(int)currentMode.mode].showTemp || modes[(int)currentMode.mode].showRelaxation)
                {
                    controller.GetComponent<ControllerSymbols>().Symbols.SetActive(true);
                    controller.GetComponent<ControllerSymbols>().SetSymbol();
                }
                else {
                    GameObject symbols = controller.GetComponent<ControllerSymbols>().Symbols;
                    if (symbols!= null)
                        symbols.SetActive(false);
                }

                // detach the currently attached object from the laser and deactivate the laser
                LaserGrabber LG = controller.GetComponent<LaserGrabber>();
                LG.attachedObject = null;
                LG.laser.SetActive(false);
                LG.readyForResize = false;
                LG.InfoText.gameObject.SetActive(false);
        }
    }

    private void UpdateMenu()
    {
        StructureMenuController.inst.SetState(modes[(int)currentMode.mode].showExplorer);
        TemperatureMenuController.inst.SetState(modes[(int)currentMode.mode].showTemp);
        ModeMenuController.inst.SetState(currentMode.mode != Modes.Explorer);
        AnimationMenuController.inst.SetState(currentMode.mode == Modes.Temperature || currentMode.mode == Modes.Minimize ||
            currentMode.mode == Modes.View);
    }
}
