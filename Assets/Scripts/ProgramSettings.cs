using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

// component of Settings
public class ProgramSettings : MonoBehaviour
{
    public static ProgramSettings inst;

    // needed to stop the TCP listener thread after stopping the program
    public static bool programIsRunning = true;

    [Header("Settings")]
    // the global size multiplier
    public static float size = 0.3f; // default is 0.3f;
    // determines whether the boundingbox gets just updated if the player changes it or if it gets updated each frame
//    public static bool updateBoundingboxEachFrame = true;
    // the resolution each text in the scene should have
    public static float textResolution = 100f;
    // the width the borders of the cellbox should have
    public static float cellboxWidth = 0.1f;

    [Header("References")]
    // reference to the Head GO
    public GameObject HeadGO;
    // a Point in short distance before the head of the user
    public GameObject CenterPoint;

    [Header("Debug")] 
    // Messages from python to Unity will be filtered, e.g. to remove empty messages
    public bool showFilteredMsg;

    private void Awake()
    {
        inst = this;
    }

    // let myObject always look in the direction of the player
    public static void Face_Player(GameObject myObject)
    {
        myObject.transform.LookAt(inst.HeadGO.transform.position);
        myObject.transform.eulerAngles = new Vector3(0, (myObject.transform.eulerAngles.y + 180) % 360, 0);
    }

    public static void MoveToCenter(GameObject go, bool onFloor=true, bool facePlayer=true)
    {
        go.transform.position = inst.CenterPoint.transform.position;
        if (onFloor)
            go.transform.position -= Vector3.up * go.transform.position.y;
        if (facePlayer)
            Face_Player(go);
    }

    // a function to get the name of a layer/mask
    public static string GetLayerName(LayerMask layer)
    {
        return (LayerMask.LayerToName((int)Mathf.Log(layer.value, 2)));
    }

    // a function to get the number of a layer/mask
    public static int GetLayerNum(LayerMask layer) // not needed
    {
        return (int)Mathf.Log(layer.value, 2);
    }

    public void ResetScene()
    {
        SimulationMenuController.jobLoaded = false;
        Thermometer.temperature = -1;
        Thermometer.inst.SetState(false); 
        StructureDataOld.atomCtrlPos = new List<Vector3>(StructureDataOld.atomCtrlPos.Count);  // seems not to work
        StructureDataOld.Inst.structureCtrlPos = Vector3.zero;  // seems not to work
        ImportStructure.newImport = true;
        AnimationController.frame = 0;
    }
}
