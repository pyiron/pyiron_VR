using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;

// component of Settings
// this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
public class PythonExecuter : MonoBehaviour {
    // the Settings of the program
    private ProgramSettings Settings;
    // the script of the controller printer
    public InGamePrinter printer;

    [Header("Start Python")]
    // the file to where the python script file is located
    private string pythonPath = "C:/Users/pneugebauer/PycharmProjects/pyiron/tests";
    // the name of the python file which creates the structure for Unity
    public string pythonFileName;
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
    // the temperature Python sends to Unity when sending the first structure data
    public static int temperature;
    // the amount of atoms the new structure posseses
    public static int structureSize = 99999;
    // shows for which atom the data is currently transmitted from Python
    private static int currentAtomLine;

    [Header("Send Data to Python")]
    // the filename of the file which will send orders from unity to pyiron
    private string fileName = "orders";

    // shows whether Python should be currently sending an animation or just always the same frame
    public bool pythonRunsAnim = false;
    // the speed with which Python currently runs the animation
    public int pythonsAnimSpeed = 4;


    private void Awake()
    {
        // get the scripts from the gameobjects to get their data
        Settings = GameObject.Find("Settings").GetComponent<ProgramSettings>();
        // return if the data of the structure should be received by an file or files
        if (Settings.transMode == "file")
            return;

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
            myProcess.OutputDataReceived += new DataReceivedEventHandler(ReadOutput);
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

    private static void ReadOutput(object sender, DataReceivedEventArgs e) 
    {
        if (e.Data.Contains("print"))
            print(e.Data);
        else if (e.Data.Contains("job"))
            return;
        else if (currentAtomLine == structureSize + 1)
        {
            StoreData(e.Data);
            collectedData = currentData;
            // print(collectedData);
            currentData = "";
            structureForce = currentStructureForce;
            newData = true;
            currentAtomLine = 0;
        }
        else if (currentAtomLine == 0)
            {
            if (e.Data.Split()[0] == "new")
                temperature = int.Parse(e.Data.Split()[3]);
            if (int.Parse(e.Data.Split()[2]) != structureSize)
            {
                structureSize = int.Parse(e.Data.Split()[2]);
                currentStructureForce = new float[structureSize];
            }
            StoreData(e.Data.Split()[1]);
            }
        else
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

    // send the given order to Python, where it will be executed with the exec() command
    public void SendOrder(string order)
    {
        if (Settings.transMode == "file")
            // write an Order to Python via a file
            WriteOrder(order);
        else
            // write the command in the input of Python
            myProcess.StandardInput.WriteLine(order);
    }

    // write an Order to Python via a file
    private void WriteOrder(string order)
    {
        StreamWriter sw = new StreamWriter(Settings.GetFilePath(fileName: fileName));
        using (sw)
        {
            sw.WriteLine(order);
        }
        printer.Ctrl_print("order", 20);
    }

    public void SendOrder(bool runAnim=false)
    {
        if (runAnim)
        {
            SendOrder("self.start_anim()");
            pythonRunsAnim = true;
        }
        else
        {
            SendOrder("self.runAnim = False");
            pythonRunsAnim = false;
        }
    }

    public void ChangeAnimSpeed(int speedChange)
    {
        // send python the order to change the animation speed, but also remember what the new animation speed is in unity
        SendOrder("self.animSpeed += " + speedChange);
        pythonsAnimSpeed += speedChange;
    }

    public void OnApplicationQuit()
    {
        // close the python script
        print("Application ending after " + Time.time + " seconds");
        myProcess.StandardInput.WriteLine("Stop!");
        myProcess.StandardInput.Close();
        myProcess.Close();
    }
}
