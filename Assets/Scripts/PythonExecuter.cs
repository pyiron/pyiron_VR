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
    private string pythonFileName = "StructureLoader1";
    // start a process which executes the commands in the shell to start the python script
    Process myProcess = new Process();

         [Header("Receive data from Python")]
    // the data send from Python before all data of the structure is there.
    private static string currentData = "";
    // the collected data of what Python sent to Unity for one frame
    public static string collectedData = "";
    // shows whether there is some new data about the structure which has to be used in ImportStructure
    public static bool newData;
    // shows whether python has send some data about the force/temperature/etc. already or not
    public static bool extendedData;
    // the force the structure currently posseses, but when the data is incomplete
    private static float[] currentStructureForce;
    // the force the structure currently posseses
    public static float[] structureForce;
    // the amount of atoms the new structure posseses
    public static int structureSize = 99999;
    // shows for which atom the data is currently transmitted from Python
    private static int currentAtomLine;


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
        if (e.Data.Contains("print"))
            print(e.Data);
        else if (currentAtomLine == structureSize + 1)  // e.Data.Split().Length == 3 || 
        {
            StoreData(e.Data);
            collectedData = currentData;
            currentData = "";
            structureForce = currentStructureForce;
            newData = true;
            currentAtomLine = 0;
        }
        else if (currentAtomLine == 0)
            if (e.Data.Split().Length == 3)
            {
                if (int.Parse(e.Data.Split()[1]) != structureSize)
                {
                    structureSize = int.Parse(e.Data.Split()[1]);
                    currentStructureForce = new float[structureSize];
                }
                StoreData(e.Data.Split()[0]);
            } else ;
        else if (!e.Data.Contains("job"))
            StoreData(e.Data);
    }

    private static void StoreData(string data)
    {
        if (data.Split().Length == 6)
        {
            currentStructureForce[currentAtomLine - 1] = float.Parse(data.Split()[4]);
            extendedData = true;
        }
        currentData += data + "\n";
        currentAtomLine += 1;
    }

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
