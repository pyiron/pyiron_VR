using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ChooseStructure : MonoBehaviour
{
    public static ChooseStructure inst;

    [Header("Scene")]
    // the Transform of the Headset
    private Transform HeadTransform;
    // get the data about the modes
    public ModeData MD;
    // the parent of the instantiated buttons
    public Transform StructButtons;

    // start a process which executes the commands in the shell to start the python script
    Process myProcess = new Process();

    public static bool shouldGetPythonScripts;
    //
    public static List<string> PythonFileNames = new List<string>();

    public GameObject PythonFileButtonPrefab;

    private int buttonNr;

    private Vector3 ButtonDistance = new Vector3(4, 1, 10.5f);

    private int buttonRowLength = 5;

    public static bool shouldShowPossibleStructures;
    // the keys are the controllers transforms, the values the buttons the controllers currently point to
    private Dictionary<Transform, GameObject> hittedButtons = new Dictionary<Transform, GameObject>();

    private Dictionary<string, Color> Colors = new Dictionary<string, Color>()
    {
        { "Idle", new Color(0, 1, 0) },
        { "clicked", new Color(0, 0.6f, 0) },
        //{ "clickedButMovedAway", new Color(0, 0.8f, 0) }
    };

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        // get the reference to the transform of the headset
        //HeadTransform = GameObject.Find("[CameraRig]/Camera (head)").transform;
        // look which Python Scripts can be executed
        //GetPythonScripts();
        //foreach (GameObject Controller in SceneReferences.inst.Controllers)
        //    hittedButtons.Add(Controller.transform, null);
    }

    private Vector3 GetFirstButtonPos()
    {
        return new Vector3(-(float)(buttonRowLength - 1) / 2 * ButtonDistance.x, ButtonDistance.y, ButtonDistance.z);
    }

    /*private void GetPythonScripts()
    {
        myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        myProcess.StartInfo.CreateNoWindow = true;  // should be true, just false for debugging purposes
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.RedirectStandardInput = true;
        myProcess.StartInfo.RedirectStandardOutput = true;
        myProcess.OutputDataReceived += new DataReceivedEventHandler(ReadOutput);
        myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
        myProcess.StartInfo.Arguments = "/c cd " + PythonExecuter.pythonPath + " && dir";
        myProcess.EnableRaisingEvents = true;
        myProcess.Start();
        myProcess.BeginOutputReadLine();
        //myProcess.WaitForExit();
    }*/

    private static void ReadOutput(object sender, DataReceivedEventArgs e)
    {
        foreach (string dataFragment in e.Data.Split())
            if (ValidFileName(dataFragment))
                //if (dataFragment.Contains("Structure"))
                PythonFileNames.Add(dataFragment);
    }

    private void ShowPossibleStructures()
    {
        GameObject[] newButtons = new GameObject[4];
        if (PythonFileNames.Count > 0)
            foreach (string scriptName in PythonFileNames)
                if (true) //(ValidFileName(scriptName))
                {
                    newButtons[0] = InstantiatePossibleStructButton(scriptName);
                    SetButtonTransform(newButtons[0].transform, 0);
                    for (int i = 0; i < 3; i++)
                    {
                        int buttonNr = i + 1;
                        newButtons[buttonNr] = Instantiate(newButtons[0]);
                        SetButtonTransform(newButtons[buttonNr].transform, buttonNr);
                    }
                    buttonNr ++;
                }
        //myProcess.Close(); TODO or deleate
    }

    private static bool ValidFileName(string scriptName)
    {
        return scriptName.Contains(".py") && !scriptName.Contains(".txt");
    }

    private GameObject InstantiatePossibleStructButton(string scriptName)
    {
        GameObject NewButton = Instantiate(PythonFileButtonPrefab);
        TextMesh ButtonText = NewButton.GetComponentInChildren<TextMesh>();
        ButtonText.transform.localScale = Vector3.one * 0.2f;
        // position the text in the middle of the button
        ButtonText.alignment = TextAlignment.Center;
        ButtonText.anchor = TextAnchor.MiddleCenter;
        // set the text of the button to the name of the Python Script
        ButtonText.text = scriptName;

        return NewButton;
    }

    private void SetButtonTransform(Transform Button, int direction)
    {
        Button.parent = StructButtons;
        Button.localEulerAngles = Vector3.up * direction * 90;
        Button.GetChild(0).localScale = new Vector3(ButtonDistance.x - 0.5f, ButtonDistance.y - 0.5f, 0.2f);
        //Button.localPosition = lastButtonPos + Vector3.right * ButtonDistance.x;
        Vector3 newPosition = GetFirstButtonPos() + Vector3.right * (buttonNr % buttonRowLength) * ButtonDistance.x
            + Vector3.up * (buttonNr / buttonRowLength);
        if (direction == 0)
            Button.localPosition = newPosition;
        else if (direction == 1)
            Button.localPosition = new Vector3(newPosition.z, newPosition.y, - newPosition.x);
        else if (direction == 2)
            Button.localPosition = new Vector3(- newPosition.x, newPosition.y, -newPosition.z);
        else if (direction == 3)
            Button.localPosition = new Vector3(- newPosition.z, newPosition.y, newPosition.x);
        //SceneReferences.inst.PossiblePythonScripts = gameObject;
    }

    void Update()
    {
        if (shouldGetPythonScripts)
        {
            //GetPythonScripts();
            shouldGetPythonScripts = false;
        }
        if (shouldShowPossibleStructures)
        {
            ShowPossibleStructures();
            PythonFileNames.Clear();
        }
    }

    public void HairTriggerDown()
    {
        
    }

    public void WhileHairTriggerDown(Transform trackedObj)
    {
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.position, trackedObj.forward, out hit, LaserGrabber.laserMaxDistance))
        {
            if (!hit.transform.parent.name.Contains("PythonScript")) return;
            hittedButtons[trackedObj] = hit.transform.gameObject;
            //hittedButton = hit.transform.gameObject;
            hit.transform.GetComponent<Renderer>().material.color = Colors["clicked"];
            trackedObj.GetComponent<LaserGrabber>().laser.SetActive(true);
            trackedObj.GetComponent<LaserGrabber>().ShowLaser(hit);
        }
        else
            if (hittedButtons[trackedObj] != null)
            {
                hittedButtons[trackedObj].GetComponent<Renderer>().material.color = Colors["Idle"];
                hittedButtons[trackedObj] = null;
                trackedObj.GetComponent<LaserGrabber>().laser.SetActive(true);
        }   
    }

    public void HairTriggerUp(Transform trackedObj)
    {
        if (hittedButtons[trackedObj] != null)
        {
            hittedButtons[trackedObj].GetComponent<Renderer>().material.color = Colors["Idle"];
            //SceneReferences.inst.PE.LoadPythonScript(hittedButtons[trackedObj].transform.parent.GetComponentInChildren<TextMesh>().text);
            foreach (GameObject Controller in SceneReferences.inst.Controllers)
            {
                if (Controller.GetComponent<LaserGrabber>().laser != null)
                    Controller.GetComponent<LaserGrabber>().laser.SetActive(false);
                hittedButtons[Controller.transform] = null;
            }
            MD.RaiseMode();
            SceneReferences.inst.Settings.GetComponent<ProgramSettings>().ResetScene();
        }
    }

    private void OnApplicationQuit()
    {
        // be sure the process is closed
        //myProcess.Close();
    }
}
