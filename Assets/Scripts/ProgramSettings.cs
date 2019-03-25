using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

// component of Settings
public class ProgramSettings : MonoBehaviour {

    [Header("Settings")]
    // the global size multiplikator
    public static float size = 0.3f; // default is 0.3f;
    // determines whether the boundingbox gets just updated if the player changes it or if it gets updated each frame
    public static bool updateBoundingboxEachFrame = true;
    // determines whether errors should be printed
    public static bool showErrors = false;
    // the resolution each text in the scene should have
    public static float textResolution = 100f;
    // the width the borders of the cellbox should have
    public static float cellboxWidth = 0.1f;

    // let myObject always look in the direction of the player
    public static void Face_Player(GameObject myObject)
    {
        myObject.transform.LookAt(SceneReferences.inst.HeadGO.transform.position);
        myObject.transform.eulerAngles = new Vector3(0, (myObject.transform.eulerAngles.y + 180) % 360, 0);
    }

    public static void MoveToCenter(GameObject GO, bool onFloor=true, bool facePlayer=true)
    {
        GO.transform.position = SceneReferences.inst.CenterPoint.transform.position;
        if (onFloor)
            GO.transform.position -= Vector3.up * GO.transform.position.y;
        if (facePlayer)
            Face_Player(GO);
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

    public void ResetScene()
    {
        LaserGrabber.firstAnimStart = true;
        Thermometer.temperature = -1;
        Thermometer.inst.SetState(false); 
        StructureData SD = StructureData.inst;
        SD.atomCtrlPos = new List<Vector3>(SD.atomCtrlPos.Count);  // seems not to work
        SD.structureCtrlPos = Vector3.zero;  // seems not to work
    }
}
