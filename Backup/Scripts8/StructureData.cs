using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureData : MonoBehaviour
{
    public GameObject boundingbox; // muss man spater erst holen, wenn die box erstellt wurde
    public Vector3 minPositions;
    public Vector3 maxPositions;
    public AtomInfos[] atomInfos;
    // the gameobject which holds the global settings for the program
    public GameObject Settings;
    // the script which stores the global settings
    private ProgramSettings programSettings;

    public void Awake()
    {
        // load the settings from the GameObject Settings
        programSettings = Settings.GetComponent<ProgramSettings>();
    }

    // transform the Boundingbox, so that it encloses the structure
    public void updateBoundingbox()
    {
        // set it to the size of the structure
        boundingbox.transform.localScale = maxPositions - minPositions;
        // place it in the the middle of the structure
        boundingbox.transform.position = (minPositions + (maxPositions - minPositions) / 2) * programSettings.size;
    }

    // search for the min and max position of the atoms in the cluster for each axis
    public void searchMaxAndMin()
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

