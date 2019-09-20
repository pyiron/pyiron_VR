using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
    [Header("Animation")]
    public static bool run_anim;
    public static int animSpeed = 4;
    public static int frame;
    private static float next_time;
    
    internal static bool waitForLoadedStruc;
    internal static bool shouldLoad;

    // Update is called once per frame
    void Update () {
        if (run_anim && (!waitForLoadedStruc || PythonExecuter.useServer && !TCPClient.isAsync)) // program is stuck while waiting anyway
        {
            if (Time.time >= next_time)
            {
                ImportStructure.inst.LoadStructure();
                if (change_frame())
                {
                    float delta_time = 1f / 90;
                    next_time = Time.time;
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
        else
        {
            FrameData frameData = StructureData.GetCurrFrameData();
            if (shouldLoad && frameData?.cellbox != null)
            {
                ImportStructure.inst.LoadStructure();
                shouldLoad = false;
            }}
    }

    public static void RunAnim(bool shouldRun = false)
    {
        run_anim = shouldRun;
        // update the symbols on all active controllers
        foreach (ControllerSymbols controller in LaserGrabber.controllerSymbols)
            if (controller.gameObject.activeSelf)
                controller.SetSymbol();
    }

    internal static void ChangeAnimSpeed(int change)
    {
        // send Python the order to play the animation faster. if it isn't already at it's fastest speed
        if (change > 0)
            if (animSpeed + change <= 5)
                animSpeed += change;
            else
                animSpeed = 5;
        else
            if (animSpeed + change >= 0)
                animSpeed += change;
            else
                animSpeed = 0;
    }

    public static void move_one_frame(bool forward=true) {
        if (forward)
            frame = Mod((frame + 1), StructureData.GetCurrFrameData().frames);
        else
            //frame = (GetCurrFrameData().frames - (Mod(GetCurrFrameData().frames - frame, GetCurrFrameData().frames))) - 1;
            frame = Mod((frame - 1), StructureData.GetCurrFrameData().frames);
        ImportStructure.inst.LoadStructure();
    }

    private static bool change_frame() {
        int frame_step = 1;
        if (animSpeed == 0 || animSpeed == 5)
            frame_step = 2;
        if (animSpeed < 3)
            frame_step *= -1;
        int newFrame = Mod((frame + frame_step), StructureData.GetCurrFrameData().frames);
        frame = newFrame;
        return Mod((frame + frame_step), StructureData.GetCurrFrameData().frames) == frame + frame_step;
    }

    internal static void ResetAnimation()
    {
        frame = 0;
        if (!run_anim)
            ImportStructure.inst.LoadStructure();
    }

    private static int Mod(int a, int b)
    {
        if (b != 0)
            return (a % b + b) % b;
        return 0;
    }
}
