using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of AtomStructure
public class StructureData : MonoBehaviour
{
    // the boundingbox which defines where you can grab the structure. It will be created in importStructure
    public GameObject boundingbox;
    // the cellbox of the structure. It will be created in importStructure
    public GameObject cellbox;
    // the min and max value of the structure in each axis, needed for the boundingbox
    public Vector3 minPositions;
    public Vector3 maxPositions;
    // list with all infos about the atoms
    public List<AtomInfos> atomInfos = new List<AtomInfos>();
    // the name of the structure, at the moment the name is defined here
    public string structureName = "Fe-Structure";

    // the data, how each atom has been relocated by the player
    public List<Vector3> atomCtrlPos = new List<Vector3>();
    // the data, how each atom has been relocated by the player
    public Vector3[] atomCtrlSize;  // might be unnecessary
    // the data, how the structure has been relocated and resized by the player
    //public Transform structureCtrlTrans;
    // the data, how the structure has been relocated
    public Vector3 structureCtrlPos;
    // the data, how the structure has been resized
    public Vector3 structureCtrlSize;  // might be unnecessary
    // the gameobject which holds the global settings for the program
    public GameObject Settings;
    // the script which stores the global settings
    private ProgramSettings programSettings;
    // shows whether the structure should check if Pyiron send a structure without the destroyed atom
    public bool waitForDestroyedAtom;

    public void Awake()
    {
        // load the settings from the GameObject Settings
        programSettings = Settings.GetComponent<ProgramSettings>();
        // create a new empty Instance which holds the data, how the structure has been relocated by the player
        structureCtrlPos = Vector3.zero;
        // create a new empty Instance which holds the data, how the structure has been resized by the player
        structureCtrlSize = Vector3.zero; // might be useless!
    }

    // transform the Boundingbox, so that it encloses the structure
    public void UpdateBoundingbox()
    {
        // set it to the size of the structure
        boundingbox.transform.localScale = maxPositions - minPositions;
        // place it in the the middle of the structure
        boundingbox.transform.position = (minPositions + (maxPositions - minPositions) / 2) * programSettings.size;
    }

    // search for the min and max position of the atoms in the cluster for each axis
    public void SearchMaxAndMin()
    {
        // sets the values to the extremes, so that the first atom will change them to it's values
        minPositions = Vector3.one * Mathf.Infinity;
        maxPositions = Vector3.one * Mathf.Infinity * -1;
        foreach (AtomInfos atom in atomInfos)
            for (int i = 0; i < 3; i++)
            {
                if (atom.m_transform.position[i] / programSettings.size + atom.m_transform.localScale[i] / 2 > maxPositions[i])
                    maxPositions[i] = atom.m_transform.position[i] / programSettings.size + atom.m_transform.localScale[i] / 2;
                if (atom.m_transform.position[i] / programSettings.size - atom.m_transform.localScale[i] / 2 < minPositions[i])
                    minPositions[i] = atom.m_transform.position[i] / programSettings.size - atom.m_transform.localScale[i] / 2;
            }
    }

    /*
    // might be needed in the future to increase the perfomance speed
    public void updateMaxAndMin(Transform testTransform)
    {
        for (int i = 0; i < 3; i++)
        {
            if (testTransform.position[i] + testTransform.localScale[i]/2 > maxPositions[i])
                maxPositions[i] = testTransform.position[i] + testTransform.localScale[i]/2;
            if (testTransform.position[i] - testTransform.localScale[i]/2 < minPositions[i])
                minPositions[i] = testTransform.position[i] - testTransform.localScale[i]/2;
        }
    }*/
}

