using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Globalization;
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
    // to parse floats correct
    public static CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
    // the file to where the python script file is located
    // old Path: C:/Users/pneugebauer/PycharmProjects/pyiron/tests/Structures
    // TODO: No Hardcoding!!
    public static string pythonPath =
        "C:/Users/Philip/Documents/MPIE/pyiron_vrplugin-master/pyiron_vrplugin-master/vrplugin";
        //"C:/Users/pneugebauer/PycharmProjects/pyiron/vrplugin";///Structures";
    // public static string pythonPath = "C:/Users/pneugebauer/PyIron_data/projects/Structures";
    // start a process which executes the commands in the shell to start the python script
    private Process myProcess = new Process();
    // shows whether the program has loaded a structure or not
    public static bool loadedStructure;

    [Header("Receive data from Python")]
    // the amount of changes the Python program did after the Unity program requested it
    public static int incomingChanges;

    // the forces of all the atoms
    public static float[][] allForces;


    [Header("Send Data to Python")]
    // the amount of changes the Unity program requested the Python program to do
    public static int outgoingChanges;

    /// <summary>
    /// Start the connection to Python.
    /// </summary>

    private void Awake()
    {
        inst = this;
        // the reference to the ProgressBar
        ProgressBar = GameObject.Find("MyObjects/ProgressBar");
        
        // allow float.Parse to parse floats seperated by . correctly
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        //IS = GameObject.Find("AtomStructure").GetComponent<ImportStructure>();
        LoadUnityManager();
        ResetTransferData();
    }

    private void ResetTransferData()
    {
        // the amount of changes the Python program did after the Unity program requested it
        incomingChanges = -1;
        // the amount of changes the Unity program requested the Python program to do
        outgoingChanges = 0;
    }

    // start the UnityManager which will start the ProjectExplorer
    public void LoadUnityManager()
    {
        if (loadedStructure) // not needed at the moment
        {
            ClosePythonProgress();
            ResetTransferData();
        }
        myProcess = new Process();
        var pyPathThread = new Thread(delegate () {
            string executedOrder = "cd " + pythonPath + " && python UnityManager.py";
            print("executed: " + executedOrder);
            Command(executedOrder, myProcess);
        });
        pyPathThread.Start();
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

    /// <summary>
    /// Receive the input from Python and handle it, e.g. by delegating it to other scripts.
    /// </summary>

    private static void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        print(e.Data);
    }

    private static void ReadOutput(object sender, DataReceivedEventArgs e)
    {
        print(e.Data);
        try
        {
            foreach (String partInp in e.Data.Split('%'))
                HandleInp(partInp);
        }
        catch(Exception exc)
        {
            print(exc);
        }
    }

    private static void HandleInp(String inp)
    {
        string[] splittedData = inp.Split();
        if (inp.Contains("print"))
            print(inp);
        else if (inp.Contains("job"))
            return;
        else if (inp.Contains("Order executed"))
        {
            // show that Unity received the change from Python
            incomingChanges += 1;
            return;
        }
        else if (splittedData[0].Contains("mode"))
        {
            if (splittedData[1] == "view")
            {
                ModeData.inst.newMode = Modes.View;
            }
            else
            {
                print(splittedData + " is not yet implemented!");
            }
        }
        else if (splittedData[0] == "groups")
        {
            StructureMenuController.inst.AddOption(OptionType.Folder, inp.Substring(7));
        }
        else if (splittedData[0] == "nodes")
        {
            StructureMenuController.inst.AddOption(OptionType.Job, inp.Substring(6));
        }
        else if (splittedData[0] == "files")
        {
            print("Files detected, they are not needed, but do no harm.");
            //StructureMenuController.inst.AddOption(OptionType.Script, inp.Substring(6));
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
                    allForces[int.Parse(splittedData[4])][i] = float.Parse(splittedData[i + 1],NumberStyles.Any,ci);
            }
        }
        else if (splittedData[0] == "StructureDataStart")
        {
            if (splittedData.Length < 6)
            {
                print("First Line of Structure Data should have 6 parameters!");
                return;
            }

            int strucSize = -1;
            int frame = -1;
            int frames = -1;

            if (ContainsValue(splittedData[2]))
                if (int.Parse(splittedData[2]) != strucSize)
                {
                    strucSize = int.Parse(splittedData[2]);
                    allForces = new float[strucSize][];
                    for (int atomNr = 0; atomNr < strucSize; atomNr++)
                    {
                        allForces[atomNr] = new float[3];
                        allForces[atomNr][0] = -1;
                    }
                }
            if (ContainsValue(splittedData[3]))
            {
                Thermometer.temperature = int.Parse(splittedData[3]);
            }
            if (ContainsValue(splittedData[4]))
                frame = int.Parse(splittedData[4]);
            else
                print("Warning: Current Frame is missing!");

            if (ContainsValue(splittedData[5]))
                frames = int.Parse(splittedData[5]);
            else
                print("Warning: Frame Amount is missing!");

            StructureData.AddFrameDataStart(strucSize, frame, frames);
        }
        else if (splittedData[0] == "StructureDataMid")
        {
            /*for (int i = 1; i < 4; i++)
            {
                float f;
                if (float.TryParse(splittedData[i], out f))
                {
                    print("position Data should be a float");
                }
            }*/

            
            StructureData.AddFrameDataMid(new AtomData(
                new Vector3(float.Parse(splittedData[1],NumberStyles.Any,ci), 
                    float.Parse(splittedData[2],NumberStyles.Any,ci), 
                    float.Parse(splittedData[3],NumberStyles.Any,ci)), splittedData[4]));
        }
        // this is the line where Python sends the data about the cellbox
        else if (splittedData[0] == "StructureDataEnd")
        {
            float[] cellboxData = new float[9];
            Vector3[] cellboxVecs = new Vector3[3];
            if (ContainsValue(splittedData[1]))
            {
                for (int i = 0; i < 9; i++)
                    cellboxData[i] = float.Parse(splittedData[i + 1],NumberStyles.Any,ci);

                // save the data for the cellbox in 3 Vector3s
                cellboxVecs[0] = new Vector3(cellboxData[0], cellboxData[1], cellboxData[2]);
                cellboxVecs[1] = new Vector3(cellboxData[3], cellboxData[4], cellboxData[5]);
                cellboxVecs[2] = new Vector3(cellboxData[6], cellboxData[7], cellboxData[8]);
            }
            StructureData.AddFrameDataEnd(cellboxVecs);
        }
        // Python sends Unity the arguments a function (create_ase_bulk()) needs
        else if (splittedData[0] == "arg")
        {
            Dictionary<string, List<string>> nDict = new Dictionary<string, List<string>>();
            string t = "";
            for (int i = 1; i < splittedData.Length; i++)
            {
                if (new List<string> { "name", "type", "options", "desc", "end" }.Contains(splittedData[i]))
                {
                    t = splittedData[i];
                    if (t == "end")
                        StructureCreatorMenuController.should_build_gui = true;
                    else
                        nDict.Add(t, new List<string>());
                }
                else
                {
                    // print(t + splittedData[i]);
                    nDict[t].Add(splittedData[i]);
                }
            }
            StructureCreatorMenuController.args.Add(nDict);
        }
        else
            print("Warning: Unknown Data: " + inp);
    }

    private static bool ContainsValue(string data)
    {
        return (data != "empty");
    }

    /// <summary>
    /// Send an order to Python.
    /// script is the script that should execute the order.
    /// type defines how the order should be handled. E.g. does exec mean, that it will be executed with the exec() statement.
    /// order contains the data that should be set or the order that should be executed.
    /// </summary>

    // send the given order to Python, where it will be executed with the exec() command
    public void SendOrder(PythonScript script, PythonCommandType type, string order)
    {
        string type_data = type.ToString();
        if (type == PythonCommandType.exec)
            type_data += " " + AnimationController.frame;
        string full_order = script.ToString() + " " + type_data + " " + order;
        print(full_order);
        // show that the Unity program has send the Python program an order
        outgoingChanges += 1;

        // write the command in the input of Python
        myProcess.StandardInput.WriteLine(full_order);
    }

    /// <summary>
    /// (De)Activate the thermometer. Print the outgoing and incoming changes to the controller_printer.
    /// </summary>

    private void Update()
    {
        if (Thermometer.temperature != -1)
        {
            // activate the thermometer when changing into temperature mode, else deactivate it
            Thermometer.inst.SetState(ModeData.currentMode.showTemp);
            Thermometer.inst.UpdateTemperature();
        }

        InGamePrinter.inst[0].Ctrl_print("Send: " + outgoingChanges.ToString());
        InGamePrinter.inst[1].Ctrl_print("Received: " + incomingChanges.ToString());
    }

    // return if Unity is currently waiting for a response of Python
    public static bool IsLoading()
    {
        return outgoingChanges != incomingChanges;
    }

    /// <summary>
    /// Close the Python Progress the clean way.
    /// </summary>

    private void ClosePythonProgress()
    {
        // close the python script
        print("Application ending after " + Time.time + " seconds");
        print("Sent  " + outgoingChanges + " Orders to PyIron");
        print("Received  " + incomingChanges + " Responses from PyIron");
        // let the program stop itself. This way, it can for example delete the scratch folder.
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
