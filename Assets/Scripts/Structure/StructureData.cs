using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of AtomStructure
public class StructureData : MonoBehaviour
{
    public static StructureData inst;
    // the boundingbox which defines where you can grab the structure. It will be created in importStructure
    public GameObject boundingbox;
    // the min and max value of the structure in each axis, needed for the boundingbox
    public Vector3 minPositions;
    public Vector3 maxPositions;
    // list with all infos about the atoms
    public static List<AtomInfos> atomInfos = new List<AtomInfos>();
    // the name of the structure, at the moment the name is defined here
    public static string structureName = "Atom Structure";

    // the data, how each atom has been relocated by the player
    public static List<Vector3> atomCtrlPos = new List<Vector3>();
    // the data, how each atom has been relocated by the player
    public Vector3[] atomCtrlSize;  // might be unnecessary
    // the data, how the structure has been relocated and resized by the player
    //public Transform structureCtrlTrans;
    // the data, how the structure has been relocated
    public Vector3 structureCtrlPos;
    // the data, how the structure has been resized
    public Vector3 structureCtrlSize;  // might be unnecessary
    // shows whether the structure should check if Pyiron send a structure without the destroyed atom
    public static bool waitForDestroyedAtom;

    [Header("Structure Data")]
    public static List<FrameData> animation_data = new List<FrameData>();
    private static FrameData currFrame;
    public static int structureSize;
    public static int frame_amount = 0;

    public void Awake()
    {
        inst = this;
        // create a new empty Instance which holds the data, how the structure has been relocated by the player
        structureCtrlPos = Vector3.zero;
        // create a new empty Instance which holds the data, how the structure has been resized by the player
        structureCtrlSize = Vector3.zero; // might be useless!
    }

    // transform the Boundingbox, so that it encloses the structure
    public void UpdateBoundingbox()
    {
        // set it to the size of the structure
        boundingbox.transform.localScale = maxPositions - minPositions;
        // place it in the the middle of the structure
        boundingbox.transform.position = (minPositions + (maxPositions - minPositions) / 2) * ProgramSettings.size;
    }

    // search for the min and max position of the atoms in the cluster for each axis
    public void SearchMaxAndMin()
    {
        // sets the values to the extremes, so that the first atom will change them to it's values
        minPositions = Vector3.one * Mathf.Infinity;
        maxPositions = Vector3.one * Mathf.Infinity * -1;
        foreach (AtomInfos atom in atomInfos)
            for (int i = 0; i < 3; i++)
            {
                if (atom.m_transform.position[i] / ProgramSettings.size + atom.m_transform.localScale[i] / 2 > maxPositions[i])
                    maxPositions[i] = atom.m_transform.position[i] / ProgramSettings.size + atom.m_transform.localScale[i] / 2;
                if (atom.m_transform.position[i] / ProgramSettings.size - atom.m_transform.localScale[i] / 2 < minPositions[i])
                    minPositions[i] = atom.m_transform.position[i] / ProgramSettings.size - atom.m_transform.localScale[i] / 2;
            }
    }

    /*
    // might be needed in the future to increase the perfomance speed
    public void updateMaxAndMin(Transform testTransform)
    {
        for (int i = 0; i < 3; i++)
        {
            if (testTransform.position[i] + testTransform.localScale[i]/2 > maxPositions[i])
                maxPositions[i] = testTransform.position[i] + testTransform.localScale[i]/2;
            if (testTransform.position[i] - testTransform.localScale[i]/2 < minPositions[i])
                minPositions[i] = testTransform.position[i] - testTransform.localScale[i]/2;
        }
    }*/

    public static void AddFrameDataStart(int size, int frame_id, int frames)
    {
        if (frame_id == 0)
            animation_data.Clear();
        currFrame = new FrameData(size, frame_id, frames);
        structureSize = size;
        frame_amount = frames;
        if (frame_id == 0)
            AnimationController.shouldLoad = true;
    }

    public static void AddFrameDataMid(AtomData atom)
    {
        currFrame.AddAtom(atom);
    }

    public static void AddFrameDataEnd(Vector3[] newCellbox)
    {
        currFrame.AddCellbox(newCellbox);
        animation_data.Add(currFrame);
        AnimationController.waitForLoadedStruc = false;
    }

    public static FrameData GetCurrFrameData()
    {
        if (animation_data.Count != 0)
        {
            return animation_data[AnimationController.frame];
        }
        return null;
    }
}

// could be combined with AtomInfos.
public class AtomData
{
    internal Vector3 pos;
    internal string type;
    internal int id;

    public AtomData(Vector3 pos, string type)
    {
        this.pos = pos;
        this.type = type;
    }
}

public class FrameData
{
    internal int size;
    internal int frame;
    internal int frames;
    internal List<AtomData> atoms = new List<AtomData>();
    internal Vector3[] cellbox;

    public FrameData(int size, int frame, int frames, List<AtomData> atoms, Vector3[] cellbox)
    {
        this.size = size;
        this.frame = frame;
        this.frames = frames;
        this.atoms = atoms;
        this.cellbox = cellbox;
    }

    public FrameData(int size, int frame, int frames)
    {
        this.size = size;
        this.frame = frame;
        this.frames = frames;
    }

    public void AddAtom(AtomData atom)
    {
        atom.id = atoms.Count;
        atoms.Add(atom);
    }

    public void AddCellbox(Vector3[] newCellbox)
    {
        cellbox = newCellbox;
    }
}

