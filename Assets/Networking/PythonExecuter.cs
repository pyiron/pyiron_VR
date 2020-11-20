using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Networking
{
    // component of Settings
    // this script handles everything related to Python, e.g. it receives the data from Python, formats it and can send data to Python
    public class PythonExecuter : MonoBehaviour {
        #region Attributes

        internal static PythonExecuter Inst;

        [Header("Scene")]
        // the script of the controller printer
        public InGamePrinter printer;

        // to parse floats correct
        private static CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();

        // the forces of all the atoms (not in use at the moment)
        public static float[][] allForces;
    
        #endregion

        #region MonoBehaviour Callbacks
    
        /// <summary>
        /// Start the connection to Python.
        /// </summary>

        private void Awake()
        {
            Inst = this;
        
            // allow float.Parse to parse floats seperated by . correctly
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
        } 
    
        /// <summary>
        /// (De)Activate the thermometer. Print the outgoing and incoming changes to the controller_printer.
        /// </summary>

        private void Update()
        {
            InGamePrinter.inst[0].Ctrl_print("Send: " + TCPClient.TaskNumOut); // outgoingChanges
            InGamePrinter.inst[1].Ctrl_print("Received: " + TCPClient.TaskNumOut); //incomingChanges
        }
        #endregion

        #region ReceiveData
        public static void HandlePythonMsg(string data)
        {
            print("received: " + data);
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
                Debug.LogError(exc);
            }
        }

        private static void HandleMsg(String inp)
        {
            string[] splittedData = inp.Split();
            if (inp.Contains("print"))
                print(inp);
            else if (inp.Contains("error"))
            {
                Debug.LogError(inp);
                ErrorTextController.inst.ShowMsg("Error:\n" + inp.Substring(7));
            }
            else if (inp.Contains("job")) return;
            else if (inp.Contains("Order executed"))
            {
                // show that Unity received the change from Python
                //incomingChanges += 1;
            }
            // seems not to be in use currently
            else if (splittedData[0].Contains("mode"))
            {
                if (splittedData[1] == "view")
                {
                    Debug.LogWarning("TODO: Please mark here, that the animation has already been calculated");
                    ModeController.inst.newMode = Modes.Calculate;
                    //ModeController.inst.newMode = Modes.Animate;
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

                StructureDataOld.AddFrameDataStart(strucSize, frame, frames); 
            }
            else if (splittedData[0] == "SDM")
            {
                StructureDataOld.AddFrameDataMid(new AtomData(
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
                StructureDataOld.AddFrameDataEnd(cellboxVecs);
            }
            // Python sends Unity the arguments a function (create_ase_bulk()) needs
            else if (splittedData[0] == "arg")
            {
                /*Dictionary<string, List<string>> nDict = new Dictionary<string, List<string>>();
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
                StructureCreatorMenuController.args.Add(nDict);*/
            }
            else if (splittedData[0] == "")
            {
                Debug.LogWarning("Unknown Data: " + inp + ". Closing the server");
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

        private static string ProcessOrder(PythonScript script, string order)
        {
            string fullOrder = order;

            if (script != PythonScript.None)
            {
                fullOrder = script + "." + order;
            }
            
            print(fullOrder);
            
            return fullOrder;
        }
    
        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// Warning: this method might result in a screen freeze.
        /// </summary>
        /// <param name="script">The python script that should execute the command.</param>
        /// <param name="type">Defines whether the command has a return value (=eval) or not</param>
        /// <param name="order">The command that should be executed.</param>
        /// <param name="handleInput">Should the PythonExecutor handle the response.</param>
        /// <returns></returns>
        public static string SendOrderSync(PythonScript script, PythonCommandType type, string order, bool handleInput=true)
        {
            string fullOrder = ProcessOrder(script, order);

            string response = TCPClient.SendMsgToPythonSync(type, fullOrder);
            if (handleInput && !response.StartsWith("Error"))
            {
                HandlePythonMsg(response);
            }
            return response;
        }

        /// <summary>
        /// Send a command that will be executed in python and wait for the response. The command should be from
        /// PythonCmd so that it is possible to see which Python calls are used.\n
        /// </summary>
        /// <param name="script">The python script that should execute the command.</param>
        /// <param name="type">Defines whether the command has a return value (=eval) or not</param>
        /// <param name="order">The command that should be executed.</param>
        /// <param name="onReceiveCallback">The method that will be called when the response from Python arrives.</param>
        /// <returns></returns>
        public static void SendOrderAsync(PythonScript script, PythonCommandType type, string order, 
            Action<string> onReceiveCallback)
        {
            // show that the program is loading
            AnimatedText.Instances[TextInstances.LoadingText].Activate();
            // deactivate the scene while loading
            Utilities.DeactivateInteractables();
        
            string fullOrder = ProcessOrder(script, order);

            // send the message using TCP
            TCPClient.SendMsgToPython(type, fullOrder, callback:onReceiveCallback);
        }

        #endregion

        // return if Unity is currently waiting for a response of Python
        /*public static bool IsLoading()
        {
            return outgoingChanges != incomingChanges;
        }*/
    }

    public enum PythonScript
    {
        None, unityManager, executor, structure
    }

    public enum PythonCommandType
    {
        exec_l, eval_l
    }
}