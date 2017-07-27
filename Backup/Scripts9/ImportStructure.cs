using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System;
using System.Reflection;

// component of AtomStructure
public class ImportStructure : MonoBehaviour
{
    // name of the data file which contains the information about the atom structure
    public string strucFileName;
    // path to the files with the atomstructure data
    private string path = "ressources/AtomStructures/";
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
    // the min expansion of the cluster of each axis
    Vector3 minPositions = Vector3.one * Mathf.Infinity;
    // the max expansion of the atoms of each axis
    Vector3 maxPositions = Vector3.one * Mathf.Infinity * -1;
    // the prefab for the boundingbox
    public GameObject BoundingboxPrefab;
    // checks whether all the instances in the scene have to be created or if the scene just has to be updated
    private bool firstImport = true;
    // holds the data of which frame should be loaded 
    private int currentFrame = 0;

    [Header("Reading Tools")]
    // the reader which reads the file
    private StringReader sr;
    // stores the data of each read line in a file
    private string line;
    // the individual properties announced in the line
    private string[] data;
    // enumerates the atoms
    int atomCounter = 0;


    private void Awake()
    {
        // get the scripts from the gameobjects to get their data
        programSettings = Settings.GetComponent<ProgramSettings>();
        LED = Settings.GetComponent<LocalElementData>();
        SD = gameObject.GetComponent<StructureData>();
    }

    void Start()
    {
        loadStructure();
        firstImport = false;
    }

    public void Update()
    {
        //loadStructure();
    }

    private void loadStructure()
    {
        if (firstImport)
        {
            // create the instance of the boundingbox
            SD.boundingbox = Instantiate(BoundingboxPrefab);
            SD.boundingbox.transform.parent = gameObject.transform;
        }

        // check how big the structure is
        readFile("getStructureExpansion");
        if (firstImport)
        {
            // set the length of the Arrays which hold the Data of all Atoms to the amount of atoms in the input file
            SD.atomInfos = new AtomInfos[atomCounter];
            SD.ctrlTrans = new Transform[atomCounter];
        }
        // create the atoms
        readFile("initAtoms");
        
        if (firstImport)
            // set the size of the cluster to the global scale
            gameObject.transform.localScale = Vector3.one * programSettings.size;
        if (firstImport || programSettings.framesUpdateBoundingbox)
        {
            // check the expansion of the cluster
            SD.searchMaxAndMin();
            // set the Boundingbox, so that it equals the expansion of the cluster
            SD.updateBoundingbox();
        }

        currentFrame = (currentFrame + 1) % 4;
    }

    private void readFile(string action)
    {
        //using (sr = new StringReader(structureFile.text)) // reader to read the input data file
        StreamReader sr = new StreamReader(path + strucFileName + "/" + currentFrame + ".txt", Encoding.Default);
        using (sr)
        {
            // (re)set the counter to 0
            atomCounter = 0;
            while (true)
            {
                line = sr.ReadLine();
                // split the data into the position (data[0 - 2]) and it's type (data[3]) or in the cell data
                data = line.Split(' ');
                if (data.Length < 5)
                //if (line != null) // reads line for line, until the end is reached
                {
                    if (action == "getStructureExpansion")
                        getStructureExpansion();
                    else if (action == "initAtoms")
                        initAtoms();
                    else
                        print("Error: Unknown action!");
                }
                else
                {
                    //print(data[9]);
                    //need to get bondary data here
                    break; // breaks the routine if the end of the file is reached
                }

                atomCounter++;
            }
        }
    }

    private void getStructureExpansion()
    {
        for (int i = 0; i < 3; i++) // searches for the min and max expansion of the cluster of each axis 
        {
            if (float.Parse(data[i]) - LED.getSize(data[3]) / 2 < minPositions[i])
                minPositions[i] = float.Parse(data[i]) - LED.getSize(data[3]) / 2;
            if (float.Parse(data[i]) + LED.getSize(data[3]) / 2 > maxPositions[i])
                maxPositions[i] = float.Parse(data[i]) + LED.getSize(data[3]) / 2;
        }
    }

    private void initAtoms()
    {
        if (firstImport)
        {
            // create a new instance of an atom
            currentAtom = Instantiate(AtomPrefab);
            // set the parent of the atom to the structure it belongs to
            currentAtom.transform.parent = gameObject.transform;
        }
        else
            foreach (AtomInfos AI in SD.atomInfos)
                if (AI.m_ID == atomCounter)
                    currentAtom = AI.m_transform.gameObject;

        if (firstImport)
        {
            // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
            currentAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                float.Parse(data[2])) - (maxPositions + minPositions) / 2;
            SD.ctrlTrans[atomCounter] = new GameObject().transform;
        }
        else
            currentAtom.transform.position = (new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                float.Parse(data[2])) - (maxPositions + minPositions) / 2) * programSettings.size
                + SD.ctrlTrans[atomCounter].position;
        // set the atom colour to the colour this type of atom has
        currentAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3]);
        // set the atoms size to the size this type of atom has 
        currentAtom.transform.localScale = Vector3.one * LED.getSize(data[3]);
        if (firstImport)
        {
            // give the atom an ID
            currentAtom.GetComponent<AtomID>().ID = atomCounter;
            // register the atom in the overwiev of StructureData
            SD.atomInfos[atomCounter] = new AtomInfos(atomCounter, data[3], currentAtom.transform);
        }
    }
}

