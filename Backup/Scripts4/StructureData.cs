using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureData : MonoBehaviour
{
    public GameObject BoundingBox;
    public Vector3 minPositions;
    public Vector3 maxPositions;
    public AtomInfos[] atomInfos;

    // transform the Boundingbox, so that it encloses the molecule
    public void updateBoundingbox()
    {
        BoundingBox.transform.localScale = maxPositions - minPositions;
        BoundingBox.transform.position = minPositions + (maxPositions - minPositions)/2 + transform.position;
    }

    // search for the min and max position of the atoms in the cluster for each axis
    public void searchMaxAndMin()
    {
        minPositions = Vector3.one * Mathf.Infinity;
        maxPositions = Vector3.one * Mathf.Infinity * -1;
        foreach (AtomInfos atom in atomInfos)
            for (int i = 0; i < 3; i++)
            {
                if (atom.m_transform.position[i] + atom.m_transform.localScale[i]/2 > maxPositions[i])
                    maxPositions[i] = atom.m_transform.position[i] + atom.m_transform.localScale[i]/2;
                if (atom.m_transform.position[i] - atom.m_transform.localScale[i]/2 < minPositions[i])
                    minPositions[i] = atom.m_transform.position[i] - atom.m_transform.localScale[i]/2;
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

