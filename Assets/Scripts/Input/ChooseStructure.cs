using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ChooseStructure : MonoBehaviour {
    // start a process which executes the commands in the shell to start the python script
    Process myProcess = new Process();
    //
    public static List<string> PythonFileNames = new List<string>();

    private void Awake()
    {
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
        print(e.Data);
        foreach (string dataFragment in e.Data.Split())
            if (dataFragment.Contains("Structure"))
                PythonFileNames.Add(dataFragment);
    }

    // Use this for initialization
    void Start () {
        foreach (string scriptName in PythonFileNames)
            print(scriptName);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnApplicationQuit()
    {
        // be sure the process is closed
        myProcess.StandardInput.Close();
        myProcess.Close();
    }
}
