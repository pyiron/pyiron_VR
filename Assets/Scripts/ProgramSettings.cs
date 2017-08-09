using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Settings
public class ProgramSettings : MonoBehaviour {
    //
    public GameObject[] controllers = new GameObject[2];
    // the global size multiplikator
    public float size;
    // determines, whether the boundingbox should be updated each frame
    public bool framesUpdateBoundingbox;
    // determines whether errors should be printed
    public bool showErrors = false;
    // the amount of modes in the game (because 0 is a mode too)
    public static int maxModeNr = 3;
    // the current mode in the game:
    // 1: move, 2: show infos, 3: edit
    public int modeNr = 0;

    // raise the mode nr by one, except it reached the highest mode, then set it to 0
    public void raiseMode()
    {
        modeNr = (modeNr + 1) % maxModeNr;
        foreach (GameObject controller in controllers)
            controller.GetComponent<LaserGrabber>().attachedObject = null;
    }

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
