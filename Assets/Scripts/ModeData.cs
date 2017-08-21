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

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    public TextMesh CurrentModeText;
    // the amount of modes in the game (because 0 is a mode too)
    public static int maxModeNr = 3;
    // the current mode in the game:
    // 1: move, 2: show infos, 3: edit
    public int modeNr = 0;
    // a timer which will disable the text after a few seconds
    private float modeTextTimer;
    // the size the text should have
    private float textSize = 1f;


    private static readonly Dictionary<int, string> modes = new Dictionary<int, string> {
        { 0, "Move Mode" },
        { 1, "Info Mode" },
        { 2, "Edit Mode" },
        };

    private void Awake()
    {
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (eye)/Camera (head)").transform;
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
        modeNr = (modeNr + 1) % maxModeNr;
        gameObject.GetComponent<TextMesh>().text = modes[modeNr];
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
