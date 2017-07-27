using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;


public class ImportStructure : MonoBehaviour {
    public TextAsset structureFile;
    public GameObject AtomPrefab;
    public GameObject Settings;
    private GameObject newAtom;
    private GameSettings myGameSettings;
    private StringReader sr;
    private LocalElementData LED;
    private StructureData SD;
    Vector3 minPositions = Vector3.one * Mathf.Infinity; // the min expansion of the cluster of each axis
    Vector3 maxPositions = Vector3.one * Mathf.Infinity * -1; // // the max expansion of the atoms of each axis
    // stores the data of each read line in a file
    private string line;
    // the individual properties announced in the line
    private string[] data;
    // enumerates the atoms
    int atomCounter = 0;


    private void Awake()
    {
        myGameSettings = Settings.GetComponent<GameSettings>();
        LED = Settings.GetComponent<LocalElementData>();
        SD = gameObject.GetComponent<StructureData>();
    }

    void Start () {
        // check how big the structure is

        /*
        using (sr = new StringReader(structureFile.text)) // reader to read the input data file to check how big the structure is
        {
            while (true)
            {
                string line = sr.ReadLine();
                if (line != null) // reads line for line, until the end is reached
                {
                    data = line.Split(' '); // splits the line into the position (data[0 - 2]) and it's type (data[3])
                    for (int i = 0; i < 3; i++) // searches for the min and max expansion of the cluster of each axis 
                    {
                        if (float.Parse(data[i]) - LED.getSize(data[3])/2 < minPositions[i])
                            minPositions[i] = float.Parse(data[i]) - LED.getSize(data[3])/2;
                        if (float.Parse(data[i]) + LED.getSize(data[3])/2 > maxPositions[i])
                            maxPositions[i] = float.Parse(data[i]) + LED.getSize(data[3])/2;
                    }
                    atomCounter++;
                }
                else
                    break; // breaks the routine if the end of the file is reached
            }
        }*/

        readFile("getClusterExpansion");

        // set the length of the Array which holds the Data of all Atoms to the amount of atoms in the input file
        SD.atomInfos = new AtomInfos[atomCounter];

        readFile("initAtoms");

        /*
        atomCounter = 0; // resets the counter, so that it can tell each atom which number it is
        using (sr = new StringReader(structureFile.text)) // reader to read the input data file and set the atoms properties
        {
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    // create a new instance of an atom
                    newAtom = Instantiate(AtomPrefab);
                    // set the parent of the atom to the structure it belongs to
                    newAtom.transform.parent = gameObject.transform;
                    // split the data into the position (data[0 - 2]) and it's type (data[3])
                    data = line.Split(' ');
                    // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
                    newAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                        float.Parse(data[2])) - (maxPositions + minPositions)/2;
                    // set the atom colour to the colour this type of atom has
                    newAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3]);
                    // set the atoms size to the size this type of atom has 
                    newAtom.transform.localScale = Vector3.one * LED.getSize(data[3]);
                    // give the atom an ID
                    newAtom.GetComponent<AtomID>().ID = atomCounter;
                    // register the atom in the overwiev of StructureData
                    SD.atomInfos[atomCounter] = new AtomInfos(atomCounter, data[3], newAtom.transform);

                    atomCounter++;

                }
                else
                    break; // breaks the routine if the end of the file is reached
            }
        }*/

        // check the expansion of the cluster
        SD.searchMaxAndMin();
        // set the Boundingbox, so that it equals the expansion of the cluster
        SD.updateBoundingbox();

        // set the size of the cluster to the global scale
        gameObject.transform.localScale = Vector3.one * myGameSettings.size;
    }

    private void readFile(string action)
    {
        using (sr = new StringReader(structureFile.text)) // reader to read the input data file
        {
            atomCounter = 0;
            while (true)
            {
                line = sr.ReadLine();
                if (line != null) // reads line for line, until the end is reached
                {
                    data = line.Split(' '); // splits the data into the position (data[0 - 2]) and it's type (data[3])
                    if (action == "getClusterExpansion")
                        getClusterExpansion();
                    else if (action == "initAtoms")
                        initAtoms();
                    else
                        print("Error: Unknown action!");
                }
                else
                    break; // breaks the routine if the end of the file is reached

                atomCounter++;
            }
        }
    }

    private void getClusterExpansion()
    {
        for (int i = 0; i < 3; i++) // searches for the min and max expansion of the cluster of each axis 
        {
            if (float.Parse(data[i]) - LED.getSize(data[3]) / 2 < minPositions[i])
                minPositions[i] = float.Parse(data[i]) - LED.getSize(data[3]) / 2;
            if (float.Parse(data[i]) + LED.getSize(data[3]) / 2 > maxPositions[i])
                maxPositions[i] = float.Parse(data[i]) + LED.getSize(data[3]) / 2;
        }
        //atomCounter++;
    }

    private void initAtoms()
    {
        // create a new instance of an atom
        newAtom = Instantiate(AtomPrefab);
        // set the parent of the atom to the structure it belongs to
        newAtom.transform.parent = gameObject.transform;
        // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
        newAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]),
            float.Parse(data[2])) - (maxPositions + minPositions) / 2;
        // set the atom colour to the colour this type of atom has
        newAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3]);
        // set the atoms size to the size this type of atom has 
        newAtom.transform.localScale = Vector3.one * LED.getSize(data[3]);
        // give the atom an ID
        newAtom.GetComponent<AtomID>().ID = atomCounter;
        // register the atom in the overwiev of StructureData
        SD.atomInfos[atomCounter] = new AtomInfos(atomCounter, data[3], newAtom.transform);
    }
}
