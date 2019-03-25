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
    public static ImportStructure inst;

    [Header("Scene")]
    // the prefab for the atoms
    public GameObject AtomPrefab;
    // script which stores the properties of each element, to give each atom it's properties
    private LocalElementData LED;
    // the data of the structure the atoms are in
    private StructureData SD;

    [Header("Cellbox")]
    private GameObject Cellbox;
    public GameObject CellboxBorderPrefab;
    GameObject[] CellBorders = new GameObject[12];

    // the prefab for the boundingbox
    public GameObject BoundingboxPrefab;
    // checks whether all the instances in the scene have to be created or if the scene just has to be updated
    internal static bool newImport = true;
    // shows whether it is the first import or just a new amount of atoms
    private bool firstImport = true;
    


    private void Awake()
    {
        inst = this;
        Thermometer.inst.SetState(false);
        
        LED = SceneReferences.inst.Settings.GetComponent<LocalElementData>();
        SD = gameObject.GetComponent<StructureData>();
    }

    // TODO: is the Update function useless?
    void Update()
    {    
        if (SD.waitForDestroyedAtom)
        {
            //print(PythonExecuter.structureSize + " and " + SD.atomInfos.Count);
            if (StructureData.structureSize != SD.atomInfos.Count)
                return;
        }
    }

    public void LoadStructure()
    {
        if (!SD.boundingbox)
        {
            // create the instance of the boundingbox
            SD.boundingbox = Instantiate(BoundingboxPrefab);
            SD.boundingbox.transform.parent = gameObject.transform;

            // show the controllers the reference to the boundingbox
            foreach (LaserGrabber LG in SceneReferences.inst.LGs)
                LG.boundingbox = SD.boundingbox.transform;

            // create the cubes for the cell box and the parent cellBox
            Cellbox = new GameObject();
            Cellbox.transform.parent = transform;
            Cellbox.name = "Cellbox";
            for (int i = 0; i < 12; i++)
            {
                CellBorders[i] = Instantiate(CellboxBorderPrefab);
                CellBorders[i].transform.parent = Cellbox.transform;
                CellBorders[i].transform.localScale = Vector3.one;
                CellBorders[i].transform.localScale = Vector3.one * ProgramSettings.cellboxWidth;
            }
        }

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
        // create the atoms or change their data
        foreach (AtomData atom in StructureData.GetCurrFrameData().atoms)
        {
            InitAtoms(atom);
        }

        if (newImport)
        {
            // set the size of the cluster to the global scale
            gameObject.transform.localScale = Vector3.one * ProgramSettings.size;
        }

        SetCellbox();

        if (newImport || ProgramSettings.updateBoundingboxEachFrame)
        {
            // check the expansion of the cluster
            SD.SearchMaxAndMin();
            // set the Boundingbox, so that it equals the expansion of the cluster
            SD.UpdateBoundingbox();
        }

        SD.waitForDestroyedAtom = false;
        if (firstImport)
        {
            firstImport = false;
            Thermometer.inst.SetState(true);
        }
        newImport = false;
    }

    // set the cellbox according to the data given from Python
    private void SetCellbox()
    {
        // activate the cellbox
        Cellbox.SetActive(true);

        // reset the positions of the cellbox
        Cellbox.transform.localPosition = Vector3.zero;

        Vector3[] cellboxData = StructureData.GetCurrFrameData().cellbox;
        //set the position and length for each part of the cellbox
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
            {
                Vector3 cellBorderSize = CellBorders[j * 4 + i].transform.localScale;
                cellBorderSize[j] = cellboxData[j].magnitude + ProgramSettings.cellboxWidth;
                CellBorders[j * 4 + i].transform.localScale = cellBorderSize;

                CellBorders[j * 4 + i].transform.localPosition = cellboxData[j] * 0.5f;
                if (i == 1 || i == 3)
                    CellBorders[j * 4 + i].transform.localPosition += cellboxData[(j + 1) % 3];
                if (i == 2 || i == 3)
                    CellBorders[j * 4 + i].transform.localPosition += cellboxData[(j + 2) % 3];
            }

        // set the position of the Hourglass to the middle of the cellbox
        HourglassActivator.inst.transform.localPosition = Vector3.zero;
        for (int i = 0; i < 3; i++)
            HourglassActivator.inst.transform.localPosition += cellboxData[i] / 2;
        // set the size of the Hourglass to the size it should have
        HourglassActivator.inst.transform.GetChild(0).localScale = Vector3.one;
    }

    private void InitAtoms(AtomData atom)
    {
        GameObject currentAtom;
        if (newImport && !SD.waitForDestroyedAtom)
        {
            // create a new instance of an atom
            currentAtom = Instantiate(AtomPrefab, transform);
            // set the parent of the atom to the structure it belongs to
            //currentAtom.transform.parent = gameObject.transform;
        }
        else
            currentAtom = SD.atomInfos[atom.id].m_transform.gameObject;

        if (newImport)
        {
            // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
            currentAtom.transform.position = atom.pos;
            //if (animState == "new" || (!firstImport && ProgramSettings.transMode == "shell"))
            //    currentAtom.transform.position *= ProgramSettings.size;
            SD.atomCtrlPos.Add(Vector3.zero);
        }
        else
        {
            currentAtom.transform.position = atom.pos * ProgramSettings.size;
            currentAtom.transform.position += SD.atomCtrlPos[atom.id] + transform.position;
        }
        // set the atom colour to the colour this type of atom has
        currentAtom.GetComponent<Renderer>().material.color = LED.getColour(atom.type);
        // set the atoms size to the size this type of atom has 
        currentAtom.transform.localScale = Vector3.one * LED.getSize(atom.type);
        if (newImport || SD.waitForDestroyedAtom)
        {
            // give the atom an ID
            currentAtom.GetComponent<AtomID>().ID = atom.id;
            if (SD.waitForDestroyedAtom)
                SD.atomInfos[atom.id] = new AtomInfos(atom.id, atom.type, currentAtom.transform);
            else
            {
                // register the atom in the overwiev of StructureData
                SD.atomInfos.Add(new AtomInfos(atom.id, atom.type, currentAtom.transform));
            }
        }

    }
}

