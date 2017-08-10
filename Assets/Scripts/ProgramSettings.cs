using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Settings
public class ProgramSettings : MonoBehaviour {
    // the path to the directory where the transmitter files are stored
    private string pathToAtomStructure;

    [Header("Scene")]
    // get the references of the controllers
    public GameObject[] controllers = new GameObject[2];
    // the Transform of the Headset
    public Transform HeadTransform;

    [Header("Settings")]
    // the global size multiplikator
    public float size;
    // determines whether the boundingbox gets just updated if the player changes it or if it gets updated each frame
    public bool updateBoundingboxEachFrame;
    // determines whether errors should be printed
    public bool showErrors = false;

    [Header("Modes")]
    // get the textmesh from the 3D Text which shows the current mode
    public TextMesh CurrentModeText;
    // the amount of modes in the game (because 0 is a mode too)
    public static int maxModeNr = 3;
    // the current mode in the game:
    // 1: move, 2: show infos, 3: edit
    public int modeNr = 0;


    private static readonly Dictionary<int, string> modes = new Dictionary<int, string> {
        { 0, "Move Mode" },
        { 1, "Info Mode" },
        { 2, "Edit Mode" },
        };

    private void Awake()
    {
        if (Application.isEditor)
            pathToAtomStructure = "AtomStructures/";
        else
        {
            pathToAtomStructure = "VABuild9wUI_Data/AtomStructures/";
        }
    }

    public string GetFilePath(string fileName)
    {
        return pathToAtomStructure + fileName + ".txt";
    }

    public void raiseMode()
    {
        // raise the mode nr by one, except it reached the highest mode, then set it to 0
        modeNr = (modeNr + 1) % maxModeNr;
        CurrentModeText.text = modes[modeNr];

        CurrentModeText.transform.eulerAngles = new Vector3(0, HeadTransform.eulerAngles.y, 0);
        CurrentModeText.transform.position = new Vector3(Mathf.Sin(HeadTransform.eulerAngles.y/360*2*Mathf.PI), 0, Mathf.Cos(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI)) * 5;
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

    // let myObject always look in the direction of the player
    public void Face_Player(GameObject myObject)
    {
        myObject.transform.LookAt(HeadTransform.position);
        print(myObject.transform.eulerAngles);
        myObject.transform.eulerAngles = new Vector3(0, (myObject.transform.eulerAngles.y + 180) % 360, 0);
    }

    // a function to get the name of a layer/mask
    public string getLayerName(LayerMask layer)
    {
        return (LayerMask.LayerToName((int)Mathf.Log(layer.value, 2)));
    }

    // a function to get the number of a layer/mask
    public int getLayerNum(LayerMask layer) // I think not needed atm
    {
        return (int)Mathf.Log(layer.value, 2);
    }
}
