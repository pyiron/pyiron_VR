using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneReferences : MonoBehaviour {
    [Header("Settings and it's components")]
    // the reference to the Settings object
    protected GameObject Settings;
    // the reference to the script, which contains all the settings of the program
    protected ProgramSettings SettingsScript;
    // get the reference to the programm which handles the execution of python
    protected PythonExecuter PE;
    // the script that stores the possible orders which can be send to Python
    protected OrdersToPython OTP;

    protected void GetReferences()
    {
        // the reference to the Settings object
        Settings = GameObject.Find("Settings");
        // get the reference to the script, which contains all the settings of the program
        SettingsScript = Settings.GetComponent<ProgramSettings>();
        // get the reference to the programm which handles the execution of python
        PE = SettingsScript.GetComponent<PythonExecuter>();
        // get the reference to the script that stores the possible orders which can be send to Python
        OTP = SettingsScript.GetComponent<OrdersToPython>();
    }
}
