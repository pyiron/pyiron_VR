using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Settings
public class ProgramSettings : MonoBehaviour {
    // the global size multiplikator
    public float size;
    // determines, whether the boundingbox should be updated each frame
    public bool framesUpdateBoundingbox;
    // determines whether errors should be printed
    public bool showErrors = false;

    // a function to get the name of a layer/mask
    public string getLayerName(LayerMask layer)
    {
        return (LayerMask.LayerToName((int)Mathf.Log(layer.value, 2)));
    }

    // a function to get the number of a layer/mask
    public int getLayerNum(LayerMask layer) // I think not needed atm
    {
        return (int)Mathf.Log(layer.value, 2);
    }
}
