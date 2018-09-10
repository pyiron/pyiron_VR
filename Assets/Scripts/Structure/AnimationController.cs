using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
    [Header("Animation")]
    public static bool run_anim;
    public static int animSpeed;
    public static int frame;
    private static float next_time;
    
    [Header("Structure Data")]
    public static List<FrameData> animation_data = new List<FrameData>();
    private static FrameData currFrame;
    public static int structureSize;
    public static int frame_amount = 0;

    // Update is called once per frame
    void Update () {
        if (run_anim)
            if (Time.time >= next_time) {
                ImportStructure.inst.LoadStructure();
                if (change_frame()) {
                    float delta_time = 1 / 95;
                    if (2 <= animSpeed && animSpeed <= 3) {
                        next_time += delta_time;
                    }
                    next_time += delta_time;
                }
                else
                    next_time += 2 - (0.5f + Mathf.Abs(animSpeed - 2.5f)) / 2;
            }
    }

    public void move_one_frame(bool forward= true) {
        if (forward)
            frame = (frame + 1) % GetCurrFrameData().frames;
        else
            frame = (GetCurrFrameData().frames - ((GetCurrFrameData().frames - frame) % GetCurrFrameData().frames)) - 1;
        ImportStructure.inst.LoadStructure();
        }

    private static bool change_frame() {
        int frame_step = 1;
        if (animSpeed == 0 || animSpeed == 5)
            frame_step = 2;
        if (animSpeed < 3)
            frame_step *= -1;
        int newFrame = (frame + frame_step) % GetCurrFrameData().frames;
        frame = newFrame;
        return (frame + frame_step) % GetCurrFrameData().frames == frame + frame_step;
    }

    public static void AddFrameDataStart(int size, int frame, int frames)
    {
        if (frame == 0)
            animation_data.Clear();
        currFrame = new FrameData(size, frame, frames);
        structureSize = size;
        frame_amount = frames;
    }

    public static void AddFrameDataMid(AtomData atom)
    {
        currFrame.AddAtom(atom);
    }

    public static void AddFrameDataEnd(Vector3[] newCellbox)
    {
        currFrame.AddCellbox(newCellbox);
        animation_data.Add(currFrame);
    }

    public static FrameData GetCurrFrameData()
    {
        if (animation_data.Count > frame)
            return animation_data[frame];
        return null;
    }
}

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
