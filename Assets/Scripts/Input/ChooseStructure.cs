using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ChooseStructure : MonoBehaviour
{
    [Header("Scene")]
    // the Transform of the Headset
    private Transform HeadTransform;

    // start a process which executes the commands in the shell to start the python script
    Process myProcess = new Process();

    public static bool shouldGetPythonScripts;
    //
    public static List<string> PythonFileNames = new List<string>();

    public GameObject PythonFileButtonPrefab;

    private Vector3 lastButtonPos;

    private Vector2 ButtonDistance = new Vector2(4, 1);

    public static bool shouldShowPossibleStructures;

    private void Awake()
    {
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (head)").transform;

        lastButtonPos = new Vector3(-ButtonDistance.x, -ButtonDistance.y, 8);
        GetPythonScripts();
    }

    private void GetPythonScripts()
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
    }

    private static void ReadOutput(object sender, DataReceivedEventArgs e)
    {
        foreach (string dataFragment in e.Data.Split())
            if (ValidFileName(dataFragment))
                //if (dataFragment.Contains("Structure"))
                PythonFileNames.Add(dataFragment);
    }

    // Use this for initialization
    void Start()
    {
        foreach (string scriptName in PythonFileNames)
            print(scriptName);
    }

    private void ShowPossibleStructures(Vector3 direction)
    {
        if (PythonFileNames.Count > 0)
            foreach (string scriptName in PythonFileNames)
                if (ValidFileName(scriptName))
                    InstantiatePossibleStructurButton(scriptName, direction);
    }

    private static bool ValidFileName(string scriptName)
    {
        return scriptName.Contains(".py");
    }

    private void InstantiatePossibleStructurButton(string scriptName, Vector3 direction)
    {
        GameObject NewButton = Instantiate(PythonFileButtonPrefab);
        NewButton.transform.parent = transform;
        NewButton.transform.localEulerAngles = direction * 90;
        NewButton.transform.localPosition = lastButtonPos + direction * ButtonDistance.x;
        NewButton.transform.GetChild(0).localScale = new Vector3(ButtonDistance.x - 0.5f, ButtonDistance.y - 0.5f, 0.2f);
        lastButtonPos = NewButton.transform.localPosition;
        TextMesh ButtonText = NewButton.GetComponentInChildren<TextMesh>();
        ButtonText.transform.localScale = Vector3.one * 0.2f;
        // position the text in the middle of the button
        ButtonText.alignment = TextAlignment.Center;
        ButtonText.anchor = TextAnchor.MiddleCenter;
        // set the text of the button to the name of the Python Script
        ButtonText.text = scriptName;
    }

    void Update()
    {
        if (shouldGetPythonScripts)
        {
            GetPythonScripts();
            shouldGetPythonScripts = false;
        }
        if (shouldShowPossibleStructures)
        {
            for (int i = 0; i < 4; i++)
                ShowPossibleStructures(Vector3.up * i);
            PythonFileNames.Clear();
        }
    }

    private void OnApplicationQuit()
    {
        // be sure the process is closed
        myProcess.Close();
    }
}
