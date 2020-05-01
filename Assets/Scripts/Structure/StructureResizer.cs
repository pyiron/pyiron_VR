using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// component of AtomStructure. maybe merge with SizeManager?
public class StructureResizer : MonoBehaviour
{
    public static StructureResizer inst;

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
        inst = this;
    }

    void Update()
    {
        // test if the structure has to be resized
        TestForResize();
    }

    public void InitResize()
    {
        // set the distance of the controllers when the resizeStructure begins
        startCtrlDistance =
            (LaserGrabber.instances[0].transform.position - LaserGrabber.instances[1].transform.position).magnitude;
        // remember the size the structure had before the resize
        oldStructureSize = ProgramSettings.size;
    }

    private void TestForResize()
    {
        // return if one of the controllers isn't ready
        foreach (LaserGrabber lg in LaserGrabber.instances)
            if (!lg.readyForResize)
                return;

        ResizeStructure();
    }

    public void ResizeStructure()
    {
        // the data how far the distance between the controllers is currently
        var currentCtrlDistance =
            (LaserGrabber.instances[0].transform.position - LaserGrabber.instances[1].transform.position).magnitude;
        // the new size the structure should have
        newStrucSize = oldStructureSize + (currentCtrlDistance - startCtrlDistance) * resizeMultiplikator;
        // test if the new size for the structure is allowed
        if (minStrucSize < newStrucSize && newStrucSize < maxStrucSize)
        {
            // update the values how the player has moved each atom, so that these values depend on the global size
            for (int i = 0; i < StructureDataOld.atomCtrlPos.Count; i++)
                StructureDataOld.atomCtrlPos[i] *= newStrucSize / ProgramSettings.size;
            // set the global size to the new value and update the structure
            ProgramSettings.size = newStrucSize;
            transform.localScale = Vector3.one * ProgramSettings.size;        }
    }
}

