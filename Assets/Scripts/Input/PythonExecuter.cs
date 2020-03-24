using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Debug = UnityEngine.Debug;

// component of Settings
// this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
public class PythonExecuter : MonoBehaviour {
    #region Attributes

    internal static PythonExecuter inst;

    [Header("Scene")]
    // the script of the controller printer
    public InGamePrinter printer;

    // to parse floats correct
    private static CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();

    [Header("Receive data from Python")]
    // the amount of changes the Python program did after the Unity program requested it
    public static int incomingChanges;

    // the forces of all the atoms (not in use at the moment)
    public static float[][] allForces;
    

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
        
        // allow float.Parse to parse floats seperated by . correctly
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
    } 
    
    /// <summary>
    /// (De)Activate the thermometer. Print the outgoing and incoming changes to the controller_printer.
    /// </summary>

    private void Update()
    {
        InGamePrinter.inst[0].Ctrl_print("Send: " + outgoingChanges);
        InGamePrinter.inst[1].Ctrl_print("Received: " + incomingChanges);
    }
    #endregion

    #region ReceiveData

    /// <summary>
    /// Receive the input from Python and handle it, e.g. by delegating it to other scripts. In the future, the script
    /// that send a message to python should handle the response, instead of calling this method.
    /// </summary>
    // can be called by the TCPServer when receiving a new msg
    public void ReadReceivedInput()
    {
        HandlePythonMsg(TCPClient.returnedMsg);
    }

    public static void HandlePythonMsg(string data)
    {
        print(data);
        if (data == "async" || data == "") return;
        
        // remove the "" from the beginning end end if a string got send via the Server
        // TODO: JsonUtilities can't parse this and this shouldn't be parsed by hand. I think the exclamation marks
        // are not needed anyway
        if (data[0] == '"')
            data = data.Substring(1, data.Length - 1);
        if(data[data.Length - 1] == '"') 
            data = data.Substring(0, data.Length - 1);

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
        // seems not to be in use currently
        else if (splittedData[0].Contains("mode"))
        {
            if (splittedData[1] == "view")
            {
                Debug.LogWarning("Oh wow, this is actually in use! Please mark here, that the animation has already been calculated");
                //ModeData.inst.newMode = Modes.Simulation;
                ModeController.inst.newMode = Modes.Animate;
            }
            else
            {
                print(splittedData + " is not implemented!");
            }
        }
        else if (splittedData[0] == "SDS")
        {
            if (splittedData.Length < 5)
            {
                print("First Line of Structure Data should have 6 parameters!");
                return;
            }

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
        // todo: simplify method and eliminate duplicate code at similar methods
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

    // send the given order to Python, where it will be executed with the exec() or eval() command
    public static void SendOrderAsync(PythonScript script, PythonCommandType type, string order)
    {
        string fullOrder = ProcessOrder(script, type, order);
        
        // send the order via TCP 
        if (type == PythonCommandType.exec_l || type == PythonCommandType.eval_l)
        {
            fullOrder = order;
        }
        else
        {
            type = (type != PythonCommandType.exec ? PythonCommandType.eval : PythonCommandType.exec);
        }

        // send the message using TCP
        TCPClient.SendMsgToPython(type, fullOrder);
    }

    #endregion

    // return if Unity is currently waiting for a response of Python
    public static bool IsLoading()
    {
        return outgoingChanges != incomingChanges;
    }
}

public enum PythonScript
{
    None, Executor
}

public enum PythonCommandType
{
    exec_l, eval_l, exec, eval
}
