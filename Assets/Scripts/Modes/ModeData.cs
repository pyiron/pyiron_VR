using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of CurrentModeText
public class ModeData : MonoBehaviour
{
    [Header("Scene")]
    // get the references of the controllers
    public GameObject[] controllers = new GameObject[2];
    // the Transform of the Headset
    private Transform HeadTransform;
    // the reference to the settings
    private GameObject Settings;
    // get the reference to the programm which handles the execution of python
    private PythonExecuter PE;
    // the thermometer, needed to handle the temperature of the structure
    private GameObject ThermometerObject;
    // the script that stores the possible orders which can be send to Python
    private OrdersToPython OTP;
    // the reference to the atomstructure
    private GameObject AtomStructure;
    // the reference to the PossiblePythonScripts
    private GameObject PossiblePythonScripts;

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    public TextMesh CurrentModeText;
    // the current mode in the game:
    // 1: move, 2: show infos, 3: edit
    private static int currentModeNr = 0;
    public static Mode currentMode;
    // a timer which will disable the text after a few seconds
    private float modeTextTimer;
    // the size the text should have
    private float textSize = 1f;

    /*public readonly Dictionary<int, Mode> modes = new Dictionary<int, Mode> {
        { 0, new Mode(m_name:"Move Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showTemp:true, m_showTrashcan:true) },
        //{ 1, new Mode(m_name:"Relaxation Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showRelaxation:true, m_showTrashcan:true) },
        { 1, new Mode(m_name:"Info Mode", m_showInfo:true) },
        { 2, new Mode(m_name:"Edit Mode", m_canDuplicate:true) },
        };*/

    // the dictionary which defines what properties each mode has
    // attention: the trashcan will just be shown if m_playerCanMoveAtoms is true, even if m_showTrashcan is true
    // attention: the mode will just be accessable, if m_playerCanMoveAtoms, m_showInfo or m_canDuplicate is true
    private static readonly Dictionary<int, Mode> modes = new Dictionary<int, Mode> {
        { 0, new Mode(m_name:"Choose a Structure!", m_hideAtoms:true, m_showPossibleStructures:true) },
        { 1, new Mode(m_name:"Temperature Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showTemp:true, m_showTrashcan:true) },
        { 2, new Mode(m_name:"Relaxation Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showRelaxation:true, m_showTrashcan:true) },
        { 3, new Mode(m_name:"Info Mode", m_showInfo:true) }
        //{ 3, new Mode(m_name:"Edit Mode", m_canDuplicate:true) },
        };

    private void Awake()
    {
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (eye)/Camera (head)").transform;
        // get the reference to the Settings
        Settings = GameObject.Find("Settings");
        // get the reference to the script that handles the connection to python
        PE = Settings.GetComponent<PythonExecuter>();
        // get the reference to the thermometer
        ThermometerObject = GameObject.Find("MyObjects/Thermometer");
        // get the reference to the script that stores the possible orders which can be send to Python
        OTP = Settings.GetComponent<OrdersToPython>();
        // get the reference to the atomstructure
        AtomStructure = GameObject.Find("AtomStructure");
        // get the reference to PossiblePythonScripts
        PossiblePythonScripts = GameObject.Find("PossiblePythonScripts");
        // set the current mode to the mode according to the currentModeNr
        currentMode = modes[currentModeNr];
    }

    void Start()
    {
        textSize = textSize / ProgramSettings.textResolution * 10;
        transform.localScale = Vector3.one * textSize;
        gameObject.GetComponent<TextMesh>().fontSize = (int)ProgramSettings.textResolution;

        UpdateScene();
        gameObject.SetActive(false);
    }

    void Update()
    {
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

    public void RaiseMode()
    {
        if (!currentMode.showPossibleStructures)
            // stop the currently running animation
            OTP.RunAnim(false);
        // raise the mode nr by one, except it reached the highest mode, then set it to 0
        currentModeNr = (currentModeNr + 1) % modes.Count;
        currentMode = modes[currentModeNr];
        UpdateScene();
    }

    private void UpdateScene() { 
        gameObject.GetComponent<TextMesh>().text = modes[currentModeNr].name;
        gameObject.SetActive(true);
        modeTextTimer = 3;
        // set the text to it's original size
        transform.localScale =  Vector3.one * textSize;
        transform.eulerAngles = new Vector3(0, HeadTransform.eulerAngles.y, 0);
        Vector3 newTextPosition = Vector3.zero;
        newTextPosition.x += Mathf.Sin(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI);
        newTextPosition.z += Mathf.Cos(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI);
        CurrentModeText.transform.position = newTextPosition * 5;
        // CurrentModeText.transform.position = HeadTransform.position + Vector3.forward * 5;
        // let the CurrentModeText always look in the direction of the player
        //Face_Player(CurrentModeText.gameObject);

        if (PythonExecuter.temperature != -1)
            // activate the thermometer when changing into temperature mode, else deactivate it
            ThermometerObject.SetActive(modes[currentModeNr].showTemp);

        if (modes[currentModeNr].showInfo)
            OTP.RequestAllForces();
        // deactivate the structure if it shouldn't be shown, else activate it
        AtomStructure.SetActive(!modes[currentModeNr].hideAtoms);
        PossiblePythonScripts.SetActive(modes[currentModeNr].showPossibleStructures);

        if (modes[currentModeNr].showPossibleStructures)
            ChooseStructure.shouldShowPossibleStructures = true;

        foreach (GameObject controller in controllers)
            if (controller.activeSelf)
            {
                // activate the symbols of the controller, if changing into a mode which can play an animation, else deactivate them
                if (modes[currentModeNr].showTemp || modes[currentModeNr].showRelaxation)
                {
                    controller.GetComponent<ControllerSymbols>().Symbols.SetActive(true);
                    controller.GetComponent<ControllerSymbols>().SetSymbol();
                }
                else
                    controller.GetComponent<ControllerSymbols>().Symbols.SetActive(false);

                // detach the currently attached object from the laser and deactivate the laser
                LaserGrabber LG = controller.GetComponent<LaserGrabber>();
                LG.attachedObject = null;
                LG.laser.SetActive(false);
                LG.readyForResize = false;
                LG.InfoText.gameObject.SetActive(false);
        }
    }
}
