using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

// component of Settings
// this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
public class PythonExecuter : MonoBehaviour {
    #region Attributes

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
    // TODO: No Hardcoding. Could be achieved by using a client/server or ssh
    public static string pythonPath =
        "C:/Users/Philip/Documents/MPIE/vrplugin/pyiron_mpie/vrplugin";
    //"C:/Users/Philip/Documents/MPIE/pyiron_vrplugin-master/pyiron_vrplugin-master/vrplugin";
    //"C:/Users/pneugebauer/PycharmProjects/pyiron/vrplugin";///Structures";
    // public static string pythonPath = "C:/Users/pneugebauer/PyIron_data/projects/Structures";
    // start a process which executes the commands in the shell to start the python script
    private static Process myProcess = new Process();
    // shows whether the program has loaded a structure or not
    public static bool loadedStructure;

    [Header("Receive data from Python")]
    // the amount of changes the Python program did after the Unity program requested it
    public static int incomingChanges;

    // the forces of all the atoms
    public static float[][] allForces;
    
    // should the server be used for transfer of data or the shell
    //public static bool useServer = true;
    public static ConnectionType connType = ConnectionType.AsyncInvoker;

    [Header("Send Data to Python")]
    // the amount of changes the Unity program requested the Python program to do
    public static int outgoingChanges;
    
    #endregion

    #region MonoBehaviour Callbacks
    
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

        if (connType == ConnectionType.Shell)
            LoadUnityManager();
        ResetTransferData();
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

        InGamePrinter.inst[0].Ctrl_print("Send: " + outgoingChanges);
        InGamePrinter.inst[1].Ctrl_print("Received: " + incomingChanges);
    }
    
    public void OnApplicationQuit()
    {
        ClosePythonProgress();
    }

    #endregion

    #region Init

    private void ResetTransferData()
    {
        // the amount of changes the Python program did after the Unity program requested it
        incomingChanges = 0;
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
            myProcess.OutputDataReceived += ReceiveOutput;
            myProcess.ErrorDataReceived += ErrorDataReceived;
            myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            myProcess.StartInfo.Arguments = "/c" + order;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();
            loadedStructure = true;
            SendOrder(PythonScript.None, PythonCommandType.eval, "self.send_group()");
        }
        catch (Exception e) { print(e); }
    }

    #endregion

    #region ReceiveData

    /// <summary>
    /// Receive the input from Python and handle it, e.g. by delegating it to other scripts.
    /// </summary>

    private static void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        print(e.Data);
    }

    private static void ReceiveOutput(object sender, DataReceivedEventArgs e)
    {
        HandlePythonMsg(e.Data);
    }

    // can be called by the TCPServer when receiving a new msg
    public void ReadReceivedInput()
    {
        HandlePythonMsg(TCPClient.returnedMsg);
    }

    public static void HandlePythonMsg(string data)
    {
        print(data);
        //print("Time needed: " + (1.0 + Time.time - TCPClient.st));
        if (data == "async" || data == "") return;
        //if (String.Compare(data, "async", StringComparison.CurrentCultureIgnoreCase) == 0) return;
        // remove the "" from the beginning end end if a string got send via the Server
        // TODO: Use json tools instead
        if (connType != ConnectionType.Shell)
        {
            if (data[0] == '"')
                data = data.Substring(1, data.Length - 1);
            if(data[data.Length - 1] == '"') 
                data = data.Substring(0, data.Length - 1);
        }

        try
        {
            foreach (String partInp in data.Split('%'))
                if (partInp == "" || partInp == "done")
                {
                    if (ProgramSettings.inst.showFilteredMsg)
                        print("Ignored msg from Python: " + partInp);
                }
                else
                {
                    HandleMsg(partInp);
                }
        }
        catch(Exception exc)
        {
            UnityEngine.Debug.LogError(exc);
        }
    }

    private static void HandleMsg(String inp)
    {
        string[] splittedData = inp.Split();
        if (inp.Contains("print"))
            print(inp);
        else if (inp.Contains("error"))
        {
            UnityEngine.Debug.LogError(inp);
            ErrorTextController.inst.ShowMsg("Error:\n" + inp.Substring(7));
        }
        else if (inp.Contains("job")) return;
        else if (inp.Contains("Order executed"))
        {
            // show that Unity received the change from Python
            incomingChanges += 1;
        }
        else if (splittedData[0].Contains("mode"))
        {
            if (splittedData[1] == "view")
            {
                ModeData.inst.newMode = Modes.View;
            }
            else
            {
                print(splittedData + " is not implemented!");
            }
        }
        else if (new[] {"groups", "nodes", "files"}.Contains(splittedData[0]))
        {
            ExplorerMenuController.inst.AddOption((OptionType) Enum.Parse(typeof(OptionType), splittedData[0]),
                inp.Substring(splittedData[0].Length + 1));
        }
        /*else if (splittedData[0] == "groups")
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
        }*/
        else if (splittedData[0] == "force")
        {
            if (ContainsValue(splittedData[1]))
            {
                allForces[int.Parse(splittedData[4])] = new float[3];
                for (int i = 0; i < 3; i++)
                    allForces[int.Parse(splittedData[4])][i] = float.Parse(splittedData[i + 1],NumberStyles.Any,ci);
            }
        }
        else if (splittedData[0] == "SDS")
        {
            if (splittedData.Length < 5)
            {
                print("First Line of Structure Data should have 6 parameters!");
                return;
            }

            //AnimationController.frame = 0; // might not be necessary
            int strucSize = -1;
            int frame = -1;
            int frames = -1;

            if (ContainsValue(splittedData[1]))
                if (int.Parse(splittedData[1]) != strucSize)
                {
                    strucSize = int.Parse(splittedData[1]);
                    allForces = new float[strucSize][];
                    for (int atomNr = 0; atomNr < strucSize; atomNr++)
                    {
                        allForces[atomNr] = new float[3];
                        allForces[atomNr][0] = float.NaN;
                    }
                }
            if (ContainsValue(splittedData[2]))
            {
                Thermometer.temperature = int.Parse(splittedData[2]);
            }
            if (ContainsValue(splittedData[3]))
                frame = int.Parse(splittedData[3]);
            else
                print("Warning: Current Frame is missing!");

            if (ContainsValue(splittedData[4]))
                frames = int.Parse(splittedData[4]);
            else
                print("Warning: Frame Amount is missing!");

            StructureData.AddFrameDataStart(strucSize, frame, frames); 
        }
        else if (splittedData[0] == "SDM")
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
        else if (splittedData[0] == "SDE")
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
        else if (splittedData[0] == "path")
        {
            if (ExplorerMenuController.currPath != splittedData[1])
            {
                ExplorerMenuController.currPath = splittedData[1];
                ExplorerMenuController.pathHasChanged = true;
            }
        }
        else if (splittedData[0] == "")
        {
            UnityEngine.Debug.LogWarning("Unknown Data: " + inp);
            TCPClient.CloseServer();
        }
        else
        {
            print("JSON data or unknown data: " + inp);
        }
    }

    private static bool ContainsValue(string data)
    {
        return data != "empty";
    }

    #endregion

    #region SendData

    /// <summary>
    /// Send an order to Python.
    /// script is the script that should execute the order.
    /// type defines how the order should be handled. E.g. does exec mean, that it will be executed with the exec() statement.
    /// order contains the data that should be set or the order that should be executed.
    /// </summary>

    private static string ProcessOrder(PythonScript script, PythonCommandType type, string order)
    {
        string typeData = type.ToString();
        if (script != PythonScript.None && (type == PythonCommandType.exec || type == PythonCommandType.eval))
            typeData += " " + AnimationController.frame;
        string fullOrder = script + " " + typeData + " " + order;
        print(fullOrder);
        // show that the Unity program has send the Python program an order
        outgoingChanges += 1;
        return fullOrder;
    }
    
    public static string SendOrderSync(PythonScript script, PythonCommandType type, string order, bool handleInput=true)
    {
        string fullOrder = ProcessOrder(script, type, order);
        if (type == PythonCommandType.exec_l || type == PythonCommandType.eval_l)
        {
            fullOrder = order;
        }
        else
        {
            type = (type != PythonCommandType.exec ? PythonCommandType.eval : PythonCommandType.exec);
        }

        string response = TCPClient.SendMsgToPythonSync(type, fullOrder);
        if (handleInput)
        {
            HandlePythonMsg(response);
        }
        return response;
    }

    // send the given order to Python, where it will be executed with the exec() command
    public static IEnumerator SendOrder(PythonScript script, PythonCommandType type, string order,
        MonoBehaviour unityScript=null, string unityMethod="")
    {
        string fullOrder = ProcessOrder(script, type, order);
        if (connType != ConnectionType.Shell)
        {
            // send the order via TCP 
            if (type == PythonCommandType.exec_l || type == PythonCommandType.eval_l)
            {
                fullOrder = order;
            }
            else
            {
                type = (type != PythonCommandType.exec ? PythonCommandType.eval : PythonCommandType.exec);
            }

            if (connType == ConnectionType.AsyncInvoker)
            {
                int id = ++TCPClient.taskNumIn;
                TCPClient.SendMsgToPython(type, fullOrder, unityScript, unityMethod);
                print("Waiting for the right response to arrive...");
                yield return new WaitWhile(() => id == TCPClient.taskNumOut);
                print("The receiver got the response with a matching id");
                // get the response and handle it
                HandlePythonMsg(TCPClient.returnedMsg);
            }
            else
            {
                HandlePythonMsg(TCPClient.SendMsgToPython(type, fullOrder, unityScript, unityMethod));
            }
        }
        else
        {
            // write the command in the input of Python
            myProcess.StandardInput.WriteLine(fullOrder);
        }
    }

    #endregion

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
        if (connType == ConnectionType.Shell)
        {
            // let the program stop itself. This way, it can for example delete the scratch folder.
            myProcess.StandardInput.WriteLine("stop");
            myProcess.StandardInput.Close();
            myProcess.Close();
        }
    }
}

public enum PythonScript
{
    None, Executor, ProjectExplorer
}

public enum PythonCommandType
{
    path, pr_input, exec_l, eval_l, exec, eval
}

public enum ConnectionType
{
    Shell, Sync, AsyncInvoker, AsyncIEnumerator, AsyncThread
}
