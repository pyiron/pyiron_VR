using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeData : MonoBehaviour
{
    [Header("Scene")]
    // get the references of the controllers
    public GameObject[] controllers = new GameObject[2];
    // the Transform of the Headset
    private Transform HeadTransform;
    // the reference to the settings
    public ProgramSettings Settings;
    // get the reference to the programm which handles the execution of python
    private PythonExecuter PE;

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    public TextMesh CurrentModeText;
    // the current mode in the game:
    // 1: move, 2: show infos, 3: edit
    public int activeMode = 0;
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
    public readonly Dictionary<int, Mode> modes = new Dictionary<int, Mode> {
        { 0, new Mode(m_name:"Move Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showTemp:true, m_showTrashcan:true) },
        { 1, new Mode(m_name:"Relaxation Mode", m_playerCanMoveAtoms:true, m_playerCanResizeAtoms:true, m_showRelaxation:true, m_showTrashcan:true) },
        { 2, new Mode(m_name:"Info Mode", m_showInfo:true) },
        { 3, new Mode(m_name:"Edit Mode", m_canDuplicate:true) },
        };

    private void Awake()
    {
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (eye)/Camera (head)").transform;
        // get the reference to the script that handles the connection to python
        PE = Settings.GetComponent<PythonExecuter>();
    }

    // Use this for initialization
    void Start()
    {
        textSize = textSize / Settings.textResolution * 10;
        transform.localScale = Vector3.one * textSize;
        gameObject.GetComponent<TextMesh>().fontSize = (int)Settings.textResolution;
    }

    // Update is called once per frame
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

    public void raiseMode()
    {
        // raise the mode nr by one, except it reached the highest mode, then set it to 0
        activeMode = (activeMode + 1) % modes.Count;
        gameObject.GetComponent<TextMesh>().text = modes[activeMode].name;
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

        // stop the currently running animation
        PE.send_order(runAnim: false);

        // detach the currently attached object from the laser and deactivate the laser
        foreach (GameObject controller in controllers)
            if (controller.activeSelf)
            {
                LaserGrabber LG = controller.GetComponent<LaserGrabber>();
                LG.attachedObject = null;
                LG.laser.SetActive(false);
                LG.readyForResize = false;
                LG.InfoText.gameObject.SetActive(false);
            }
    }
}
