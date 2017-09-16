using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of no GameObject
// Scripts deriving from this class can call the functions of this class to get the needed references
public class SceneReferences : MonoBehaviour {
    [Header("Reference Library")]
    // the reference to this script, but as component of Settings. This way, other scripts can get references by copying them from this scipt
    protected SceneReferences SR;

    [Header("Settings and it's components")]
    // the reference to the Settings object
    protected GameObject Settings;
    // get the reference to the programm which handles the execution of python
    protected PythonExecuter PE;
    // the script that stores the possible orders which can be send to Python
    protected OrdersToPython OTP;

    [Header("Controllers")]
    // the reference to the controllers
    public GameObject[] Controllers = new GameObject[2];
    // the reference to the LaserGrabber script of the controllers
    protected LaserGrabber[] LGs = new LaserGrabber[2];

    // get the reference to this script, but as component of Settings, so first it has to get the reference to the Settings
    protected void GetReferenceToReferences()
    {
        // the reference to the Settings object
        Settings = GameObject.Find("Settings");
        foreach (SceneReferences ReferencesScript in Settings.GetComponents<SceneReferences>())
            // check if the script just derives from SceneReferences or if it is the script itself
            if (ReferencesScript.ToString().Contains("SceneReferences"))
                // get the reference to this script, but as component of Settings
                // this way, other scripts can get references by copying them from this scipt
                SR = ReferencesScript;
    }

    // get the references to all objects and scripts related to the Settings
    protected void GetSettingsReferences()
    {
        // get the reference to the Settings object and the script which stores most of the references (this script as component of Settings)
        GetReferenceToReferences();
        // get the reference to the programm which handles the execution of python
        PE = Settings.GetComponent<PythonExecuter>();
        // get the reference to the script that stores the possible orders which can be send to Python
        OTP = Settings.GetComponent<OrdersToPython>();
    }

    // get the references to the controllers. Has to be called on the first frame of the Update function!!!
    protected void GetControllerReferences()
    {
        if (false) //(SR.Controllers[0] == null || SR.Controllers[1] == null)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            print("Both Controllers have to be active to start the program!");
            return;
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (Controllers[i] == null || LGs[i] == null)
                {
                    // copy the reference to the controllers from the reference library to the specific script which needs the reference
                    Controllers = SR.Controllers;
                    // get the references to the LaserGraber scripts of the controllers
                    //LGs = Controllers[0].transform.parent.GetComponentsInChildren<LaserGrabber>();
                    LGs[i] = Controllers[i].GetComponent<LaserGrabber>();
                }
            }
        }
    }
}
