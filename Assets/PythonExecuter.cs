using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System;

// component of Settings
public class PythonExecuter : MonoBehaviour {
    [Header("Start Python")]
    // the file to where the python script file is located
    private string pythonPath = "C:/Users/pneugebauer/PycharmProjects/pyiron/tests";
    // the name of the python file which creates the structure for Unity
    private string pythonFileName = "animationTest4";
    // start a process which executes the commands in the shell to start the python script
    Process myProcess = new Process();

    private void Awake()
    {
        //var pyPathThread = new Thread(delegate () {
        //    StartPython("cd " + pythonPath + " && python " + pythonFileName + ".py", myProcess); });
        //pyPathThread.Start();
        StartPython("cd " + pythonPath + " && python " + pythonFileName + ".py", myProcess);
    }

    static void StartPython(string order, Process myProcess)
    {
        try
        {
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = false;  // should be true
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            //myProcess.StartInfo.RedirectStandardOutput = true;
            //myProcess.OutputDataReceived += OutputDataReceived;
            //myProcess.OutputDataReceived += new DataReceivedEventHandler(myProcess);
            myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            myProcess.StartInfo.Arguments = "/c" + order;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            //myProcess.BeginOutputReadLine();
            //print(myProcess.StandardOutput.ReadLine());
            //myProcess.BeginOutputReadLine();
            //myProcess.WaitForExit();
            //myProcess.OutputDataReceived -= OutputDataReceived;
            //int ExitCode = myProcess.ExitCode;
        }
        catch (Exception e) { print(e); }
    }

    // Update is called once per frame
    void Update () {

    }

    // allows to send an order to python
    public void send_order(string order)
    {
        myProcess.StandardInput.WriteLine(" " + order);
    }

    // end the python application when Unity stops running
    public void OnApplicationQuit()
    {
        print("Application ending after " + Time.time + " seconds");
        // write something in the input log of python to stop the script the smooth way
        myProcess.StandardInput.WriteLine(" Stop!");
        // close everything regarding python
        myProcess.StandardInput.Close();
        //myProcess.StandardOutput.Close();
        myProcess.Close();
    }
}
