using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of AtomStructure
public class StructureResizer : MonoBehaviour
{
    // the global settings of the program
    public ProgramSettings Settings;
    // the scripts of the two ctrls
    public GameObject[] Controllers = new GameObject[2];
    // the data about the structure
    private StructureData SD;

    // the distance of the controllers when the resizeStructure begins
    private float startCtrlDistance;
    // a multiplikator to change how drastic the size of the structure reacts to the controllers input
    private float resizeMultiplikator = 1f;
    // the size of the structure before it is resized
    private float oldStructureSize;
    // the new size the structure should have
    private float newStrucSize;
    // the minimum size the structure can be resized too
    private float minStrucSize = 0.02f;
    // the maximum size the structure can be resized too
    private float maxStrucSize = 10;


    public void Awake()
    {
        // get the script StructureData from AtomStructure
        SD = GetComponent<StructureData>();
    }

    void Update()
    {
        // test if the structure has to be resized
        test_for_resize();
    }

    public void init_resize()
    {
        // set the distance of the controllers when the resizeStructure begins
        startCtrlDistance = (Controllers[0].transform.position - Controllers[1].transform.position).magnitude;
        // remember the size the structure had before the resize
        oldStructureSize = Settings.size;
    }

    private void test_for_resize()
    {
        // return if one of the controllers isn't ready
        foreach (GameObject ctrl in Controllers)
            if (!ctrl.GetComponent<LaserGrabber>().readyForResize)
                return;

        resizeStructure();
    }

    public void resizeStructure()
    {
        // the data how far the distance between the controllers is currently
        float currentCtrlDistance;
        currentCtrlDistance = (Controllers[0].transform.position - Controllers[1].transform.position).magnitude;
        // the new size the structure should have
        newStrucSize = oldStructureSize + (currentCtrlDistance - startCtrlDistance) * resizeMultiplikator;
        // test if the new size for the structure is allowed
        if (minStrucSize < newStrucSize && newStrucSize < maxStrucSize)
        {
            // update the values how the player has moved each atom, so that these values depend on the global size
            for (int i = 0; i < SD.atomCtrlPos.Length; i++)
                SD.atomCtrlPos[i] *= newStrucSize / Settings.size;
            // set the global size to the new value and update the structure
            Settings.size = newStrucSize;
            transform.localScale = Vector3.one * Settings.size;        }
    }
}

