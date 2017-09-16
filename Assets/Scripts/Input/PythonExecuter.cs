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
    // shows for which atom the data is currently transmitted from Python
    private static int currentAtomLine;

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
    public static int temperature;
    // the frame Python sends to Unity this frame
    public static int frame;
    // the amount of frames the current ham_lammps has
    public static int frameAmount;
    // the amount of changes the Python program did after the Unity program requested it
    public static int incomingChanges = -1;
    // the data how the cellbox can be build
    public static float[] cellboxData = new float[9];

    // the force the atom the player requested the force from has
    public static float[] atomForces = new float[3];
    // the atom ID from the atom which force was sent the last time a force was requested
    public static int lastAtomForceId = -1;
    // the forces of all the atoms
    public static float[][] allForces;


    [Header("Send Data to Python")]
    // the filename of the file which will send orders from unity to pyiron
    private string fileName = "orders";

    // the speed with which Python currently runs the animation
    public int pythonsAnimSpeed = 4;

    // the amount of changes the Unity program requested the Python program to do
    public static int outgoingChanges;


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
        string[] splittedData = e.Data.Split();
        if (e.Data.Contains("print"))
            print(e.Data);
        else if (e.Data.Contains("job"))
            return;
        else if (splittedData[0] == "force")
        {
            atomForces[0] = float.Parse(splittedData[1]);
            atomForces[1] = float.Parse(splittedData[2]);
            atomForces[2] = float.Parse(splittedData[3]);
            lastAtomForceId = int.Parse(splittedData[4]);
        }
        // this is the line where Python sends the data about the cellbox
        else if (currentAtomLine == structureSize + 1)
        {
            if (ContainsValue(e.Data))
                for (int i = 0; i < 9; i++)
                    cellboxData[i] = float.Parse(splittedData[i]);
            collectedData = currentData;
            // print(collectedData);
            currentData = "";
            //structureForce = currentStructureForce;
            newData = true;
            currentAtomLine = 0;
        }
        else if (currentAtomLine == 0)
        {
            if (ContainsValue(splittedData[0]))
                animKind = splittedData[0];
            if (ContainsValue(splittedData[1]))
                if (int.Parse(splittedData[1]) != structureSize)
                {
                    structureSize = int.Parse(splittedData[1]);
                    allForces = new float[structureSize][];
                    for (int atomNr = 0; atomNr < structureSize; atomNr++)
                        allForces[atomNr] = new float[3];
                }
            if (ContainsValue(splittedData[2]))
                temperature = int.Parse(splittedData[2]);
            if (ContainsValue(splittedData[3]))
                frame = int.Parse(splittedData[3]);
            if (ContainsValue(splittedData[4]))
                frameAmount = int.Parse(splittedData[4]);
            if (ContainsValue(splittedData[5]))
                incomingChanges = int.Parse(splittedData[5]);

            currentAtomLine += 1;

            /*
            if (e.Data.Split()[0].Contains("new"))
            {
                // remember the temperature with which the structure has been initialised
                if (e.Data.Split()[0] == "newTemperature")
                    temperature = int.Parse(e.Data.Split()[4]);
                // remember the frame which Python will send to Unity next
                frame = int.Parse(e.Data.Split()[3]);
                // remember the size of the structure
                if (int.Parse(e.Data.Split()[2]) != structureSize)
                {
                    structureSize = int.Parse(e.Data.Split()[2]);
                    currentStructureForce = new float[structureSize];
                }
                // remember if the structure is static (irrelevant when sending data this way), has a changed size or will be send normally
                StoreData(e.Data.Split()[1]);
            }*/
        }
        else
            StoreData(e.Data);
    }

    private static bool ContainsValue(string data)
    {
        return (data != "empty");
    }

    private static void StoreData(string data)
    {
        // should be done in an other way, but will be solved if just sending the force when the player requests the info for an atom
        /*if (data.Split().Length == 6)
        {
            currentStructureForce[currentAtomLine - 1] = float.Parse(data.Split()[4]);
            extendedData = true;
        }*/
        currentData += data + "\n";
        currentAtomLine += 1;
    }

    // send the given order to Python, where it will be executed with the exec() command
    public void SendOrder(string order)
    {
        // show that the Unity program has send the Python program an order
        outgoingChanges += 1;

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
        print("Sent  " + outgoingChanges + " Orders to PyIron");
        print("Received  " + incomingChanges + " Responses from PyIron");
        myProcess.StandardInput.WriteLine("Stop!");
        myProcess.StandardInput.Close();
        myProcess.Close();
    }
}
