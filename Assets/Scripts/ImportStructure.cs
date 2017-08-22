using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using System.Reflection;

// component of AtomStructure
// loads the data from the files to create the structure or to animate the structure
public class ImportStructure : MonoBehaviour {
    // name of the data file which contains the information about the atom structure
    public string strucFileName;

    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;

    // the prefab for the atoms
    public GameObject AtomPrefab;
    // gameobject to hold the new instance of an atom
    private GameObject currentAtom;
    // the gameobject which holds the global settings for the program
    public GameObject Settings;
    // the script which stores the global settings
    private ProgramSettings programSettings;
    // script which stores the properties of each element, to give each atom it's properties
    private LocalElementData LED;
    // the data of the structure the atoms are in
    private StructureData SD;
    // the script of the controller printer
    public InGamePrinter printer;

    public GameObject CellboxBorderPrefab;

    // the min expansion of the cluster of each axis
    Vector3 minPositions = Vector3.one * Mathf.Infinity;
    // the max expansion of the atoms of each axis
    Vector3 maxPositions = Vector3.one * Mathf.Infinity * -1;
    // the prefab for the boundingbox
    public GameObject BoundingboxPrefab;
    // checks whether all the instances in the scene have to be created or if the scene just has to be updated
    private bool newImport = true;
    // shows whether it is the first import or just a new amount of atoms
    private bool firstImport = true;
    // time when the last Update() was called
    private float lastTime;
    // shows whether the input data is an animation at the moment (could be also a static structure)
    private string animState;

    [Header("Show Animation FPS")]
    // the canvas of this program
    public Canvas MyCanvas;
    // the display for the fps count
    private UnityEngine.UI.Text fps_display;
    // the cumulated fps in the time before the average count is displayed
    private UnityEngine.UI.Text min_fps_display;
    // the cumulated fps in the time before the average count is displayed
    private int cumulated_fps;
    //the count of how many frames have been cumulated in the time before the average count is displayed
    private int fps_count;
    // the time how often the average fps count should be displayed
    private float time_between_fps_updates = 1;
    // the timer for when the next average fps count should be displayed
    private float fps_timer;
    // the minimum fps rate in the last frames
    private int min_fps;

    [Header("Reading Tools")]
    // the path to the file which holds the data of the current frame
    private string pathName;
    // stores the data of each read line in a file
    private string line;
    // the individual properties announced in the line
    private string[] data;
    // enumerates the atoms
    private int atomCounter = 0;
    // the input file data as a string
    private string input_file_data;

    private int currentFrame;


    private void Awake()
    {
        // get the reference to the programm which handles the execution of python
        PE = Settings.GetComponent<PythonExecuter>();
        // get the scripts from the gameobjects to get their data
        programSettings = Settings.GetComponent<ProgramSettings>();
        // get the path to the transmitter file which holds the data pyiron send to unity
        pathName = programSettings.GetFilePath(strucFileName);
        LED = Settings.GetComponent<LocalElementData>();
        SD = gameObject.GetComponent<StructureData>();
        foreach (UnityEngine.UI.Text text in MyCanvas.GetComponentsInChildren<UnityEngine.UI.Text>())
            if (text.name.Contains("min_fps_display"))
                min_fps_display = text;
            else if (text.name.Contains("fps_display"))
                fps_display = text;

        try { File.Delete(pathName); } catch { } // print("couldn't delete file");}
    }

    void Start()
    {
        LoadStructure();
        //newImport = false;
        //firstImport = false;
        lastTime = Time.time - 1;
        fps_timer = time_between_fps_updates;
        min_fps = 9999;
    }

        void Update()
    {    
        // the old path/way
        //pathName = "AtomStructures/New Folder/new_MD_hydrogen_" + currentFrame + ".dat";  // path + strucFileName + "/" + currentFrame
        // the path to the file which holds all the data for the current frome

        if (fps_timer <= 0)
        {
            if (fps_count > 0)
                fps_display.text = "Animation FPS: " + ((int)(cumulated_fps / fps_count)).ToString();
            else
                fps_display.text = "Animation FPS: 0";
            min_fps_display.text = "Animation min FPS: " + min_fps.ToString();
            fps_timer = time_between_fps_updates;
            printer.Ctrl_print("Animation min FPS: " + min_fps.ToString(), 2, false);
            cumulated_fps = 0;
            min_fps = 9999;
            fps_count = 0;
        }
        else
            fps_timer -= Time.deltaTime;

        if (Time.time - lastTime + Time.deltaTime > 1 / 90)
        {
            if (SD.waitForDestroyedAtom)
            {
                //print(PythonExecuter.structureSize + " and " + SD.atomInfos.Count);
                if (PythonExecuter.structureSize != SD.atomInfos.Count)
                    return;
            }

            if (programSettings.transMode == "file")
            {
                // the max amount of tries this program has to get the script, else it will just go on
                int maxTries;
                maxTries = 1000;

                while (maxTries > 0)
                    if (File.Exists(pathName))
                    {
                        while (maxTries > 0)
                            try
                            {
                                LoadStructure();
                                // print(1 / (Time.time - lastTime));
                                cumulated_fps += (int)(1 / (Time.time - lastTime));
                                if ((int)(1 / (Time.time - lastTime)) < min_fps)
                                    min_fps = (int)(1 / (Time.time - lastTime));
                                fps_count += 1;
                                lastTime = Time.time;
                                newImport = false;
                                break;
                            }
                            catch
                            {
                                if (maxTries == 1)
                                    if (programSettings.showErrors)
                                        print("No Input!");
                                maxTries -= 1;
                                // print("error, probably because both programs want to simultaniously use the file");
                            }
                        break;
                    }
                    else
                    {
                        if (animState == "static")
                            break;
                        if (maxTries == 1)
                            if (programSettings.showErrors)
                                print("No Input!");
                        maxTries -= 1;
                    }
                //print("python too slow");
            }
            else
            {
                LoadStructure();
                cumulated_fps += (int)(1 / (Time.time - lastTime));
                if ((int)(1 / (Time.time - lastTime)) < min_fps)
                    min_fps = (int)(1 / (Time.time - lastTime));
                fps_count += 1;
                lastTime = Time.time;
            }
        }
    }

    private void LoadStructure()
    {
        if (!SD.boundingbox)
        {
            // create the instance of the boundingbox
            SD.boundingbox = Instantiate(BoundingboxPrefab);
            SD.boundingbox.transform.parent = gameObject.transform;
            // create the cubes for the cell box and the parent cellBox
            SD.cellbox = new GameObject();
            SD.cellbox.transform.parent = transform;
            SD.cellbox.name = "Cellbox";
            GameObject newCellboxBorder;
            for (int i = 0; i < 12; i++)
            {
                newCellboxBorder = Instantiate(CellboxBorderPrefab);
                newCellboxBorder.transform.parent = SD.cellbox.transform;
                newCellboxBorder.transform.localScale = Vector3.one;
            }
        }

        //if (programSettings.transMode == "file")
        //    currentFrame = (currentFrame + 1) % 477;  // insert the amount of frames here

        int maxTries;
        maxTries = 1000;
        input_file_data = "";
        if (programSettings.transMode == "file")
            while (maxTries > 0 || firstImport)
                try
                {
                    // save the data of the input file as a string, so that the file is just read as short as possible
                    StreamReader sr = new StreamReader(pathName, Encoding.Default);
                    using (sr)
                    {
                        input_file_data = sr.ReadToEnd();
                        break;
                    }
                }
                catch
                {
                    maxTries -= 1;
                }
        else if (PythonExecuter.newData)
        {
            input_file_data = PythonExecuter.collectedData;
            // print(input_file_data);
            PythonExecuter.newData = false;
        }

        if (input_file_data == "")
        {
            //if (Time.time > 15)
            //    print("something is too slow");
            return;
        }

        // check how big the structure is
        ReadFile("getStructureExpansion");
        if (animState == "static" && !newImport)
            return;

        if (newImport)
        {
            if (!firstImport)
                foreach (AtomInfos oldAtomInfo in SD.atomInfos)
                    Destroy(oldAtomInfo.m_transform.gameObject);
            // set the length of the Arrays which hold the Data of all Atoms to the amount of atoms in the input file
            SD.atomInfos.Clear();
            //SD.atomInfos = new List<AtomInfos>();
            SD.atomCtrlPos.Clear();
        }
        // create the atoms
        ReadFile("initAtoms");

        if (animState != "static")
            try { File.Delete(pathName); } catch { } // print("couldn't delete file");}

        if (newImport)
        {
            // set the size of the cluster to the global scale
            gameObject.transform.localScale = Vector3.one * programSettings.size;
        }
        if (newImport || programSettings.updateBoundingboxEachFrame)
        {
            // check the expansion of the cluster
            SD.SearchMaxAndMin();
            // set the Boundingbox, so that it equals the expansion of the cluster
            SD.UpdateBoundingbox();
        }
        if (animState == "new")
            transform.position += SD.structureCtrlPos;

        SD.waitForDestroyedAtom = false;
        firstImport = false;
        newImport = false;
    }

    private void ReadFile(string action)
    {
        //using (sr = new StringReader(structureFile.text)) // reader to read the input data file
        StringReader sr = new StringReader(input_file_data);
        using (sr)
        {
            // (re)set the counter to 0
            atomCounter = 0;
            // shows if the current read line is the first line of the file/string
            bool firstLine;
            firstLine = true;
            while (true)
            {
                line = sr.ReadLine();
                if (firstLine)
                {
                    if (programSettings.transMode == "file")
                        animState = line;
                    else
                        animState = "anim";
                    if (animState == "static" && !firstImport)
                        return;
                    firstLine = false;
                }
                else
                {
                    // split the data into the position (data[0 - 2]) and it's type (data[3]) or in the cell data
                    data = line.Split(' ');
                    if (data.Length < 9)
                    //if (line != null) // reads line for line, until the end is reached
                    {
                        if (action == "getStructureExpansion")
                            GetStructureExpansion();
                        else if (action == "initAtoms")
                            InitAtoms();
                    }
                    else
                    {
                        //print(data[9]);
                        //need to get cell data here
                        break; // breaks the routine if the end of the file is reached
                    }

                    atomCounter++;
                }
            }
        }
        if (!firstImport)
            if (atomCounter != SD.atomInfos.Count)
                if (!SD.waitForDestroyedAtom)
                    newImport = true;
    }

    private void GetStructureExpansion()
    {
        for (int i = 0; i < 3; i++) // searches for the min and max expansion of the cluster of each axis 
        {
            if (float.Parse(data[i]) - LED.getSize(data[3]) / 2 < minPositions[i])
                minPositions[i] = float.Parse(data[i]) - LED.getSize(data[3]) / 2;
            if (float.Parse(data[i]) + LED.getSize(data[3]) / 2 > maxPositions[i])
                maxPositions[i] = float.Parse(data[i]) + LED.getSize(data[3]) / 2;
        }
    }

    private void InitAtoms()
    {
        if (newImport && !SD.waitForDestroyedAtom)
        {
            // create a new instance of an atom
            currentAtom = Instantiate(AtomPrefab);
            // set the parent of the atom to the structure it belongs to
            currentAtom.transform.parent = gameObject.transform;
        }
        else
            currentAtom = SD.atomInfos[atomCounter].m_transform.gameObject;
            /*foreach (AtomInfos AI in SD.atomInfos)
            {
                print("!!!!!!" + AI.m_ID);
                if (AI.m_ID == atomCounter)
                    currentAtom = AI.m_transform.gameObject;
            }*/

        if (newImport)
        {
            // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
            currentAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                float.Parse(data[2])) - (maxPositions + minPositions) / 2;
            if (animState == "new" || (!firstImport && programSettings.transMode == "shell"))
                currentAtom.transform.position *= programSettings.size;
            SD.atomCtrlPos.Add(Vector3.zero);
        }
        else
        {
            currentAtom.transform.position = (new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                float.Parse(data[2])) - (maxPositions + minPositions) / 2) * programSettings.size;
            currentAtom.transform.position += SD.atomCtrlPos[atomCounter] + transform.position;
        }
        // set the atom colour to the colour this type of atom has
        currentAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3]);
        // set the atoms size to the size this type of atom has 
        currentAtom.transform.localScale = Vector3.one * LED.getSize(data[3]);
        if (newImport || SD.waitForDestroyedAtom)
        {
            // give the atom an ID
            currentAtom.GetComponent<AtomID>().ID = atomCounter;
            if (SD.waitForDestroyedAtom)
                SD.atomInfos[atomCounter] = new AtomInfos(atomCounter, data[3], currentAtom.transform);
            else
                // register the atom in the overwiev of StructureData
                SD.atomInfos.Add(new AtomInfos(atomCounter, data[3], currentAtom.transform));
        }
    }
}

