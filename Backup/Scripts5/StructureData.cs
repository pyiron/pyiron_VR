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
    private GameSettings programSettings;

    public void Awake()
    {
        programSettings = Settings.GetComponent<GameSettings>();
    }

    // transform the Boundingbox, so that it encloses the molecule
    public void updateBoundingbox()
    {
        boundingbox.transform.localScale = maxPositions - minPositions;
        boundingbox.transform.position = minPositions + (maxPositions - minPositions) / 2;// + transform.position * 2;
    }

    // search for the min and max position of the atoms in the cluster for each axis
    public void searchMaxAndMin()
    {
        minPositions = Vector3.one * Mathf.Infinity;
        maxPositions = Vector3.one * Mathf.Infinity * -1;
        foreach (AtomInfos atom in atomInfos)
            for (int i = 0; i < 3; i++)
            {
                if (atom.m_transform.position[i] + atom.m_transform.localScale[i] / 2 > maxPositions[i])
                    maxPositions[i] = atom.m_transform.position[i] + atom.m_transform.localScale[i] / 2;
                if (atom.m_transform.position[i] - atom.m_transform.localScale[i] / 2 < minPositions[i])
                    minPositions[i] = atom.m_transform.position[i] - atom.m_transform.localScale[i] / 2;
            }
    }

    /*
    // I think useless
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

