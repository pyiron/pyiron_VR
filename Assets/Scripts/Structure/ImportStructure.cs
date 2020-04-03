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
    // the prefab for the atoms
    public GameObject AtomPrefab;

    [Header("Cellbox")]
    private GameObject Cellbox;
    public GameObject CellboxBorderPrefab;
    GameObject[] CellBorders = new GameObject[12];

    // the prefab for the boundingbox
    public GameObject BoundingboxPrefab;
    // checks whether all the instances in the scene have to be created or if the scene just has to be updated
    public static bool newImport = true;
    // shows whether it is the first import or just a new amount of atoms
    public bool firstImport = true;



    private void Awake()
    {
        inst = this;
    }

    public void LoadStructure()
    {
        if (!StructureDataOld.Inst.boundingbox)
        {
            // create the instance of the boundingbox
            StructureDataOld.Inst.boundingbox = Instantiate(BoundingboxPrefab, gameObject.transform, true);

            // show the controllers the reference to the boundingbox
            foreach (LaserGrabber lg in LaserGrabber.instances)
                lg.boundingbox = StructureDataOld.Inst.boundingbox.transform;

            // create the cubes for the cell box and the parent cellBox
            Cellbox = new GameObject();
            Cellbox.transform.parent = transform;
            Cellbox.name = "Cellbox";
            for (int i = 0; i < 12; i++)
            {
                CellBorders[i] = Instantiate(CellboxBorderPrefab);
                CellBorders[i].transform.parent = Cellbox.transform;
                CellBorders[i].transform.localScale = Vector3.one * ProgramSettings.cellboxWidth;
            }
        }

        if (newImport)
        {
            if (!firstImport)
                foreach (AtomInfos oldAtomInfo in StructureDataOld.atomInfos)
                    Destroy(oldAtomInfo.m_transform.gameObject);
            // set the length of the Arrays which hold the Data of all Atoms to the amount of atoms in the input file
            StructureDataOld.atomInfos.Clear();
            //SD.atomInfos = new List<AtomInfos>();
            StructureDataOld.atomCtrlPos.Clear();
        }
        // create the atoms or change their data
        foreach (AtomData atom in StructureDataOld.GetCurrFrameData().atoms)
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
            StructureDataOld.Inst.SearchMaxAndMin();
            // set the Boundingbox, so that it equals the expansion of the cluster
            StructureDataOld.Inst.UpdateBoundingbox();
        }

        StructureDataOld.waitForDestroyedAtom = false;
        if (firstImport)
        {
            firstImport = false;
            //Thermometer.inst.SetState(true);
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

        Vector3[] cellboxData = StructureDataOld.GetCurrFrameData().cellbox;
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
        if (newImport && !StructureDataOld.waitForDestroyedAtom)
        {
            // create a new instance of an atom
            currentAtom = Instantiate(AtomPrefab, transform);
            // set the parent of the atom to the structure it belongs to
            //currentAtom.transform.parent = gameObject.transform;
        }
        else
            currentAtom = StructureDataOld.atomInfos[atom.id].m_transform.gameObject;

        if (newImport)
        {
            // Set the new atom position to the pos from the file and adjust it, so that the clusters middle is in the origin
            currentAtom.transform.position = atom.pos;
            //if (animState == "new" || (!firstImport && ProgramSettings.transMode == "shell"))
            //    currentAtom.transform.position *= ProgramSettings.size;
            StructureDataOld.atomCtrlPos.Add(Vector3.zero);
        }
        else
        {
            currentAtom.transform.position = atom.pos * ProgramSettings.size;
            currentAtom.transform.position += StructureDataOld.atomCtrlPos[atom.id] + transform.position;
        }
        // set the atom colour to the colour this type of atom has
        currentAtom.GetComponent<Renderer>().material.color = LocalElementData.GetColour(atom.type);
        // set the atoms size to the size this type of atom has 
        currentAtom.transform.localScale = Vector3.one * LocalElementData.GetSize(atom.type);
        if (newImport || StructureDataOld.waitForDestroyedAtom)
        {
            // give the atom an ID
            currentAtom.GetComponent<AtomID>().ID = atom.id;
            if (StructureDataOld.waitForDestroyedAtom)
                StructureDataOld.atomInfos[atom.id] = new AtomInfos(atom.id, atom.type, currentAtom.transform);
            else
            {
                // register the atom in the overview of StructureData
                StructureDataOld.atomInfos.Add(new AtomInfos(atom.id, atom.type, currentAtom.transform));
            }
        }
    }
}

