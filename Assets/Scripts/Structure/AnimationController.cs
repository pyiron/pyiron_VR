using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
    [Header("Animation")]
    public static bool run_anim;
    public static int animSpeed = 3;
    public static int frame;
    private static float next_time;
    private static bool shouldLoad;
    
    [Header("Structure Data")]
    public static List<FrameData> animation_data = new List<FrameData>();
    private static FrameData currFrame;
    public static int structureSize;
    public static int frame_amount = 0;

    // Update is called once per frame
    void Update () {
        if (run_anim)
        {
            if (Time.time >= next_time)
            {
                ImportStructure.inst.LoadStructure();
                if (change_frame())
                {
                    float delta_time = 1f / 90;
                    if (2 <= animSpeed && animSpeed <= 3)
                    {
                        next_time += delta_time;
                    }
                    next_time += delta_time;
                }
                else
                    next_time += 2 - (0.5f + Mathf.Abs(animSpeed - 2.5f)) / 2;
            }
        }
        else if (shouldLoad && GetCurrFrameData().cellbox != null)
        {
            print("should now load");
            ImportStructure.inst.LoadStructure();
            shouldLoad = false;
        }
    }

    public static void move_one_frame(bool forward=true) {
        if (forward)
            frame = Mod((frame + 1), GetCurrFrameData().frames);
        else
            //frame = (GetCurrFrameData().frames - (Mod(GetCurrFrameData().frames - frame, GetCurrFrameData().frames))) - 1;
            frame = Mod((frame - 1), GetCurrFrameData().frames);
        ImportStructure.inst.LoadStructure();
    }

    private static bool change_frame() {
        int frame_step = 1;
        if (animSpeed == 0 || animSpeed == 5)
            frame_step = 2;
        if (animSpeed < 3)
            frame_step *= -1;
        int newFrame = Mod((frame + frame_step), GetCurrFrameData().frames);
        frame = newFrame;
        return Mod((frame + frame_step), GetCurrFrameData().frames) == frame + frame_step;
    }

    public static void AddFrameDataStart(int size, int frame, int frames)
    {
        if (frame == 0)
            animation_data.Clear();
        currFrame = new FrameData(size, frame, frames);
        structureSize = size;
        frame_amount = frames;
        if (frame == 0)
            shouldLoad = true;
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
        if (animation_data.Count != 0)
        {
            return animation_data[Mod(frame, animation_data.Count)];
        }
        return null;
    }

    private static int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}
