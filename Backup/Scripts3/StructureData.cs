using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureData : MonoBehaviour
{
    public GameObject BoundingBox;
    public Vector3 minPositions;
    public Vector3 maxPositions;
    public Vector3[] atomPositions;

    public void updateBoundingBox()
    {
        BoundingBox.transform.localScale = maxPositions - minPositions;
        BoundingBox.transform.position = minPositions + (maxPositions - minPositions)/2;
    }

    public void updateMaxAndMin(Transform testTransform)
    {
        for (int i = 0; i < 3; i++)
        {
            if (testTransform.position[i] + testTransform.localScale[i] > maxPositions[i])
                maxPositions[i] = testTransform.position[i] + testTransform.localScale[i];
            if (testTransform.position[i] - testTransform.localScale[i] < minPositions[i])
                minPositions[i] = testTransform.position[i] - testTransform.localScale[i];
        }
    }
}

