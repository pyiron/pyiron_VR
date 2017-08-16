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

    [Header("Receive data from Python")]
    // get the reference to the ImportStructure script of the atomstructure
    //private ImportStructure IS;
    private static string currentData = "";
    // the collected data of what Python sent to Unity for one frame
    public static string collectedData = "";

    public static bool newData;

    private void Awake()
    {
        //IS = GameObject.Find("AtomStructure").GetComponent<ImportStructure>();
        var pyPathThread = new Thread(delegate () {
            Command("cd " + pythonPath + " && python " + pythonFileName + ".py", myProcess); });
        pyPathThread.Start();
        //Command("cd " + pythonPath + " && python " + pythonFileName + ".py", myProcess);
    }

    static void Command(string order, Process myProcess)
    {
        try
        {
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;  // should be true, just false for debugging purposes
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.OutputDataReceived += new DataReceivedEventHandler(readOutput);
            myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            myProcess.StartInfo.Arguments = "/c" + order;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            //myProcess.WaitForExit();
            //myProcess.OutputDataReceived -= OutputDataReceived;
            //int ExitCode = myProcess.ExitCode;
        }
        catch (Exception e) { print(e); }
    }

    private static void readOutput(object sender, DataReceivedEventArgs e) 
    {
        if (e.Data.Split().Length == 2)
        {
            //if (IS.input_file_data != "")
            //    print("Unity was too slow");
            collectedData = currentData;
            //print(collectedData);
            currentData = e.Data;
            newData = true;
        }
        else if (!e.Data.Contains("job"))
            currentData += e.Data;
        // print(e.Data);
    }
    /*
    private void setGetOutput(bool getOutput)
    {
        getOutput = false;
    }*/

    public void send_order(string order)
    {
        myProcess.StandardInput.WriteLine(order);
    }

    public void OnApplicationQuit()
    {
        print("Application ending after " + Time.time + " seconds");
        myProcess.StandardInput.WriteLine("Stop!");
        print("stopped");
        myProcess.StandardInput.Close();
        //myProcess.StandardOutput.Close();
        myProcess.Close();
    }
}
