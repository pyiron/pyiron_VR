using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgramSettings : MonoBehaviour {
    public float size = 1;


    public string getLayerName(LayerMask layer)
    {
        return (LayerMask.LayerToName((int)Mathf.Log(layer.value, 2)));
    }

    public int getLayerNum(LayerMask layer) // I think not needed atm
    {
        return (int)Mathf.Log(layer.value, 2);
    }
}
