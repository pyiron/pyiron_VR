using Networking;
using UnityEngine;

// component of the AnimationMenu Panel
public class AnimationController : MonoBehaviour
{
    public static AnimationController Inst;
    

    [Header("Animation")] 
    public static Vector3[][] positionData;
    public static bool run_anim;
    public static int animSpeed = 4;
    public static int frame;
//    private static float next_time;

    [Tooltip("The time the animation waits before restarting the animation in seconds")]
    public float pauseDuration = 1;
    private float pauseTimer;

//    private bool waitFrame;

    private void Awake()
    {
        Inst = this;
    }

    public bool HasAnimationLoaded()
    {
        return positionData != null;
    }

    private static void Show()
    {
        Structure.Inst.UpdateStructure(positionData[frame]);
    }

    public void SetNewAnimation(Vector3[][] newData)
    {
        RunAnim(true);
        frame = 0;
        positionData = newData;
        AnimationMenuController.Inst.SetState(true);
//        Show();
    }

    public void DeleteAnimation()
    {
        PythonExecuter.SendOrderSync(PythonScript.structure, PythonCommandType.exec_l,
            "structure = " + PythonScript.executor + ".job.get_structure(" + AnimationController.frame + ")");
        
        positionData = null;
        run_anim = false;
        frame = 0;
        
        AnimationMenuController.Inst.SetState(false);
    }

    // Update is called once per frame
    void Update () {
        if (run_anim)
        {
            if (pauseTimer > 0)
            {
                pauseTimer -= Time.deltaTime;
            }
            else
            {
                Show();
//            if (!waitFrame)
//            {
//                Show();
//                waitFrame = false;
//                return;
//            }

                ProgressBar.Inst.UpdateBar();

                frame += 1;
                if (frame >= positionData.Length)
                {
                    frame = 0;
                    pauseTimer = pauseDuration;
                }
            }
        }
        
//        if (run_anim && !waitForLoadedStruc)
//        {
//            if (Time.time >= next_time)
//            {
//                Show();
////                ImportStructure.Inst.LoadStructure();
//                if (change_frame())
//                {
//                    float delta_time = 1f / 90;
//                    next_time = Time.time;
//                    if (2 <= animSpeed && animSpeed <= 3)
//                    {
//                        next_time += delta_time;
//                    }
//                    next_time += delta_time;
//                }
//                else
//                    next_time += 2 - (0.5f + Mathf.Abs(animSpeed - 2.5f)) / 2;
//            }
//        }
//        else
//        {
//            FrameData frameData = StructureDataOld.GetCurrFrameData();
//            if (shouldLoad && frameData?.cellbox != null)
//            {
//                ImportStructure.Inst.LoadStructure();
//                shouldLoad = false;
//            }}
    }

    /// <summary>
    /// Starts or stops the animation, according to shouldRun.
    /// </summary>
    /// <param name="shouldRun"></param>
    
    public static void RunAnim(bool shouldRun = false)
    {
        run_anim = shouldRun;
        // update the symbols on all active controllers
        foreach (ControllerSymbols controller in LaserGrabber.controllerSymbols)
            if (controller.gameObject.activeSelf)
                controller.SetSymbol();

        AnimationMenuController.Inst.startStopToggle.isOn = shouldRun;
    }

    internal static void ChangeAnimSpeed(int change)
    {
        Debug.LogWarning("Function not implemented at the moment");
        // // send Python the order to play the animation faster. if it isn't already at it's fastest speed
        // if (change > 0)
        //     if (animSpeed + change <= 5)
        //         animSpeed += change;
        //     else
        //         animSpeed = 5;
        // else
        //     if (animSpeed + change >= 0)
        //         animSpeed += change;
        //     else
        //         animSpeed = 0;
    }
    
/// <summary>
/// Moves one frame forward or backwards, according to the parameter forward
/// </summary>
/// <param name="forward"></param>
    public static void move_one_frame(bool forward=true) {
    Debug.LogWarning("Function not implemented at the moment");
//         SimulationMenuController.jobLoaded = true;
//         if (forward)
//             frame = Mod((frame + 1), StructureDataOld.GetCurrFrameData().frames);
//         else
//             //frame = (GetCurrFrameData().frames - (Mod(GetCurrFrameData().frames - frame, GetCurrFrameData().frames))) - 1;
//             frame = Mod((frame - 1), StructureDataOld.GetCurrFrameData().frames);
// //        ImportStructure.Inst.LoadStructure();
//         Show();
    }

    private static bool change_frame() {
        int frame_step = 1;
        if (animSpeed == 0 || animSpeed == 5)
            frame_step = 2;
        if (animSpeed < 3)
            frame_step *= -1;
//        int newFrame = Mod((frame + frame_step), StructureDataOld.GetCurrFrameData().frames);
        int newFrame = Mod((frame + frame_step), positionData.Length);
        frame = newFrame;
        return Mod((frame + frame_step), positionData.Length) == frame + frame_step;
    }

    internal static void ResetAnimation()
    {
        frame = 0;
        if (!run_anim)
            Show();
//            ImportStructure.Inst.LoadStructure();
    }

    private static int Mod(int a, int b)
    {
        if (b != 0)
            return (a % b + b) % b;
        return 0;
    }
}
