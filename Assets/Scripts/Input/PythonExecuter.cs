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
    internal static PythonExecuter inst;

    [Header("Scene")]
    // the script of the controller printer
    public InGamePrinter printer;
    // the reference to the ProgressBar
    private GameObject ProgressBar;

    [Header("Start Python")]
    // the file to where the python script file is located
    // old Path: C:/Users/pneugebauer/PycharmProjects/pyiron/tests/Structures
    public static string pythonPath = "C:/Users/pneugebauer/PycharmProjects/pyiron/vrplugin";///Structures";
    // public static string pythonPath = "C:/Users/pneugebauer/PyIron_data/projects/Structures";
    // start a process which executes the commands in the shell to start the python script
    private Process myProcess = new Process();
    // shows whether the program has loaded a structure or not
    public static bool loadedStructure;

    [Header("Receive data from Python")]
    // the data send from Python before all data of the structure is there.
    private static string currentData = "";
    // the collected data of what Python sent to Unity for one frame
    public static string collectedData = "";
    // shows whether there is some new data about the structure which has to be used in ImportStructure
    public static bool newData;

    // shows whether python has send some data about the force/temperature/etc. already or not
    //public static bool extendedData;
    // the force the structure currently posseses, but when the data is incomplete
    //private static float[] currentStructureForce;
    // the force the structure currently posseses
    //public static float[] structureForce;

    // stores the data how the data of the animation changes (if it changes and if the amount of atoms is the same or a new one)
    public static string animKind = "";
    // the amount of atoms the new structure posseses
    public static int structureSize = 99999;
    // the temperature Python sends to Unity when sending the first structure data
    public static int temperature = -1;
    // the frame Python sends to Unity this frame
    public static int frame = -1;
    // the amount of frames the current ham_lammps has
    public static int frameAmount = -1;
    // the amount of changes the Python program did after the Unity program requested it
    public static int incomingChanges;
    // the data how the cellbox can be build
    public static Vector3[] cellBorderVecs = new Vector3[3];

    // the forces of all the atoms
    public static float[][] allForces;


    [Header("Send Data to Python")]
    // the speed with which Python currently runs the animation
    public int pythonsAnimSpeed = 4;

    // the amount of changes the Unity program requested the Python program to do
    public static int outgoingChanges;


    private void Awake()
    {
        inst = this;
        // the reference to the ProgressBar
        ProgressBar = GameObject.Find("MyObjects/ProgressBar");

        //IS = GameObject.Find("AtomStructure").GetComponent<ImportStructure>();
        LoadPythonScript();

        ResetTransferData();
    }

    private void ResetTransferData()
    {
        // the amount of changes the Python program did after the Unity program requested it
        incomingChanges = -1;
        // the amount of changes the Unity program requested the Python program to do
        outgoingChanges = 0;
    }

    public void LoadPythonScript()
    {
        if (loadedStructure)
        {
            ClosePythonProgress();
            ResetTransferData();
        }
        myProcess = new Process();
        var pyPathThread = new Thread(delegate () {
            string executedOrder = "cd " + pythonPath + " && python UnityManager.py"; // "Executing: cd " + pythonPath + " && python " + fileName
            print("executed: " + executedOrder);
            Command(executedOrder, myProcess);
        });
        pyPathThread.Start();
        //Command("cd " + pythonPath + " && python " + pythonFileName + ".py", myProcess);
    }

    static void Command(string order, Process myProcess)
    {
        try
        {
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardInput = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.OutputDataReceived += new DataReceivedEventHandler(ReadOutput);
            myProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);
            myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            myProcess.StartInfo.Arguments = "/c" + order;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();
            loadedStructure = true;
        }
        catch (Exception e) { print(e); }
    }

    private static void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        print(e.Data);
    }

    private void Update()
    {
        if (temperature != -1)
        {
            // activate the thermometer when changing into temperature mode, else deactivate it
            Thermometer.inst.SetState(ModeData.currentMode.showTemp);
            Thermometer.inst.UpdateTemperature();
        }
    }

    private static void ReadOutput(object sender, DataReceivedEventArgs e)
    {
        print(e.Data);
        string[] splittedData = e.Data.Split();
        if (e.Data.Contains("print"))
            print(e.Data);
        else if (e.Data.Contains("job"))
            return;
        else if (e.Data.Contains("Order executed"))
        {
            // show that Unity received the change from Python
            incomingChanges += 1;
            return;
        }
        else if (splittedData[0].Contains("mode"))
        {
            if (splittedData[1] == "view")
            {
                ModeData.inst.newMode = "View Mode";
            }
            else
            {
                print(splittedData + " is not yet implemented!");
            }
        }
        else if (splittedData[0] == "groups")
        {
            StructureMenuController.inst.AddOption(OptionType.Folder, e.Data.Substring(7));
        }
        else if (splittedData[0] == "nodes")
        {
            StructureMenuController.inst.AddOption(OptionType.Job, e.Data.Substring(6));
        }
        else if (splittedData[0] == "files")
        {
            StructureMenuController.inst.AddOption(OptionType.Script, e.Data.Substring(6));
        }
        else if (splittedData[0] == "path")
        {
            if (StructureMenuController.currPath != splittedData[1])
            {
                StructureMenuController.currPath = splittedData[1];
                StructureMenuController.pathHasChanged = true;
            }
        }
        else if (splittedData[0] == "force")
        {
            if (ContainsValue(splittedData[1]))
            {
                allForces[int.Parse(splittedData[4])] = new float[3];
                for (int i = 0; i < 3; i++)
                    allForces[int.Parse(splittedData[4])][i] = float.Parse(splittedData[i + 1]);
            }
        }
        else if (splittedData[0] == "StructureDataStart")
        {
            if (splittedData.Length < 6)
            {
                print("First Line of Structure Data should have 6 parameters!");
                return;
            }
            if (ContainsValue(splittedData[1]))
                animKind = splittedData[1];
            if (ContainsValue(splittedData[2]))
                if (int.Parse(splittedData[2]) != structureSize)
                {
                    structureSize = int.Parse(splittedData[2]);
                    allForces = new float[structureSize][];
                    for (int atomNr = 0; atomNr < structureSize; atomNr++)
                    {
                        allForces[atomNr] = new float[3];
                        allForces[atomNr][0] = -1;
                    }
                }
            if (ContainsValue(splittedData[3]))
            {
                temperature = int.Parse(splittedData[3]);
            }
            if (ContainsValue(splittedData[4]))
                frame = int.Parse(splittedData[4]);
            if (ContainsValue(splittedData[5]))
                frameAmount = int.Parse(splittedData[5]);
        }
        else if (splittedData[0] == "StructureDataMid")
            StoreData(e.Data);
        // this is the line where Python sends the data about the cellbox
        else if (splittedData[0] == "StructureDataEnd")
        {
            float[] cellboxData = new float[9];
            if (ContainsValue(e.Data))
            {
                for (int i = 0; i < 9; i++)
                    cellboxData[i] = float.Parse(splittedData[i + 1]);

                // save the data for the cellbox in 3 Vector3s
                cellBorderVecs[0] = new Vector3(cellboxData[0], cellboxData[1], cellboxData[2]);
                cellBorderVecs[1] = new Vector3(cellboxData[3], cellboxData[4], cellboxData[5]);
                cellBorderVecs[2] = new Vector3(cellboxData[6], cellboxData[7], cellboxData[8]);
            }

            collectedData = currentData;
            // print(collectedData);
            currentData = "";
            newData = true;
        }
        else
            print("Warning: Unknown Data!");
    }

    private static bool ContainsValue(string data)
    {
        return (data != "empty");
    }

    private static void StoreData(string data)
    {
        currentData += data + "\n";
    }

    // send the given order to Python, where it will be executed with the exec() command
    public void SendOrder(PythonScript script, PythonCommandType type, string order)
    {
        string full_order = script.ToString() + " " + type.ToString() + " " + order;
        print(full_order);
        // show that the Unity program has send the Python program an order
        outgoingChanges += 1;

        // write the command in the input of Python
        myProcess.StandardInput.WriteLine(full_order);
    }

    public void ChangeAnimSpeed(int speedChange)
    {
        // send python the order to change the animation speed, but also remember what the new animation speed is in unity
        SendOrder(PythonScript.Executor, PythonCommandType.exec, "self.animSpeed += " + speedChange);
        pythonsAnimSpeed += speedChange;
    }

    private void ClosePythonProgress()
    {
        // close the python script
        print("Application ending after " + Time.time + " seconds");
        print("Sent  " + outgoingChanges + " Orders to PyIron");
        print("Received  " + incomingChanges + " Responses from PyIron");
        myProcess.StandardInput.WriteLine("stop");
        myProcess.StandardInput.Close();
        myProcess.Close();
    }

    public void OnApplicationQuit()
    {
        ClosePythonProgress();
    }
}

public enum PythonScript
{
    UnityManager, Executor, ProjectExplorer
}

public enum PythonCommandType
{
    path, pr_input, exec
}
