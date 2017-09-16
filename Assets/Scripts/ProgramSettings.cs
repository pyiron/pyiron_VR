using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Settings
public class ProgramSettings : MonoBehaviour {
    // the path to the directory where the transmitter files are stored
    private static string pathToAtomStructure;

    [Header("Scene")]
    // the Transform of the Headset
    public Transform HeadTransform;
    // the HeadTransform but as a static variable
    private static Transform StaticHeadTransform;

    [Header("Settings")]
    // the global size multiplikator
    public static float size = 0.3f;
    // determines whether the boundingbox gets just updated if the player changes it or if it gets updated each frame
    public static bool updateBoundingboxEachFrame;
    // determines whether errors should be printed
    public static bool showErrors = false;
    // the resolution each text in the scene should have
    public static float textResolution = 100f;
    // decides how the data should be transmitted between Unity and Python
    public static string transMode;
    // the width the borders of the cellbox should have
    public static float cellboxWidth;

    [Header("Data")]
    // the temperature each structure in the Scene has
    public static int temperature;


    private void Awake()
    {
        // defines where the file is stored which holds the data about the structure 
        // and sometimes where the orders to python get transmitted by a file
        if (Application.isEditor)
            pathToAtomStructure = "AtomStructures/";
        else
        {
            pathToAtomStructure = "VABuild9wUI_Data/AtomStructures/";
        }
        
        // make the Headtransform static
        StaticHeadTransform = HeadTransform;
    }

    // just a little function that returns the whole path to a text file from its components
    public static string GetFilePath(string fileName)
    {
        return pathToAtomStructure + fileName + ".txt";
    }

    // let myObject always look in the direction of the player
    public static void Face_Player(GameObject myObject)
    {
        myObject.transform.LookAt(StaticHeadTransform.position);
        myObject.transform.eulerAngles = new Vector3(0, (myObject.transform.eulerAngles.y + 180) % 360, 0);
    }

    // a function to get the name of a layer/mask
    public static string GetLayerName(LayerMask layer)
    {
        return (LayerMask.LayerToName((int)Mathf.Log(layer.value, 2)));
    }

    // a function to get the number of a layer/mask
    public static int GetLayerNum(LayerMask layer) // I think not needed atm
    {
        return (int)Mathf.Log(layer.value, 2);
    }
}
