using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of no GameObject
// Scripts deriving from this class can call the functions of this class to get the needed references
public class SceneReferences : MonoBehaviour {
    public static SceneReferences inst;
    //[Header("Reference Library")]
    // the reference to this script, but as component of Settings. This way, other scripts can get references by copying them from this scipt
    //protected SceneReferences SR;

    [Header("Settings and it's components")]
    // the reference to the Settings object
    public GameObject Settings;
    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;
    // the script that stores the possible orders which can be send to Python
    public OrdersToPython OTP;

    [Header("User")]
    // reference to the Head GO
    public GameObject HeadGO;
    // the reference to the controllers
    public GameObject[] Controllers = new GameObject[2];
    // the reference to the LaserGrabber script of the controllers
    public LaserGrabber[] LGs = new LaserGrabber[2];

    [Header("Structure")]
    // the reference to the Script that handles the mode in which the user can choose the structure he wants to see
    public ChooseStructure CS;

    public GameObject PossiblePythonScripts;

    private void Awake()
    {
        inst = this;
        // get the reference to the programm which handles the execution of python
        PE = GetComponent<PythonExecuter>();
        // get the reference to the script that stores the possible orders which can be send to Python
        OTP = GetComponent<OrdersToPython>();

        for (int i = 0; i < 2; i++)
        {
            LGs[i] = Controllers[i].GetComponent<LaserGrabber>();
        }

        // the reference to the Script that handles the mode in which the user can choose the structure he wants to see
        GetComponent<ChooseStructure>();
    }
}
