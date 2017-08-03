using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureResizer : MonoBehaviour
{
    // the global settings of the program
    public ProgramSettings Settings;
    // the scripts of the two ctrls
    public GameObject[] Controllers = new GameObject[2];

    // the distance of the controllers when the resizeStructure begins
    private float startCtrlDistance;
    // a multiplikator to change how drastic the size of the structure reacts to the controllers input
    private float resizeMultiplikator = 1f;
    // the size of the structure before it is resized
    private float oldStructureSize;
    // the new size the structure should have
    private float newStrucSize;
    // the minimum size the structure can be resized too
    private float minStrucSize = 0.05f;
    // the maximum size the structure can be resized too
    private float maxStrucSize = 10;


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
            // set the global size to the new value and update the structure
            Settings.size = newStrucSize;
            transform.localScale = Vector3.one * Settings.size;
        }
    }
}

