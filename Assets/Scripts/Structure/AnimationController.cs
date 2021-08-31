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
    public static int frameCount;
//    private static float next_time;

    [Tooltip("The time the animation waits before restarting the animation in seconds")]
    private static float pauseDuration = 1;
    private static float pauseTimer;

    private static bool halfSpeedFlag;

    //    private bool waitFrame;

    // a timer, which counts when the program should go a frame forward or backwards, when keeping the one frame forward button pressed
    //private float _moveOneFrameTimer = -1;
    // the time until the program should go a frame forward or backwards, when keeping the "one frame forward button" pressed
    //private readonly float timeUntilMoveOneFrame = 0.5f;

    private void Awake()
    {
        Inst = this;
    }

    public bool HasAnimationLoaded()
    {
        return positionData != null;
    }

    private static void UpdateStructure()
    {
        Structure.Inst.UpdateStructure(positionData[frame]);
    }

    public void SetNewAnimation(Vector3[][] newData, bool startAnim=true)
    {
        if (startAnim)
        {
            RunAnim(true);
        }

        if (StructureLoader.isFirstDatapart)
        {
            frame = 0;
        }

        positionData = newData;
        AnimationMenuController.Inst.SetState(true);
//        Show();
    }

    public void DeleteAnimation()
    {
        PythonExecutor.SendOrderSync(false,
            PythonCmd.SetStructureToCurrentFrame());
        
        positionData = null;
        run_anim = false;
        frame = 0;
        
        AnimationMenuController.Inst.SetState(false);
    }

    public static void ChangeFrame(int newFrame, bool setPauseTimer = false)
    {

        frame = newFrame;
        if (frame >= positionData.Length)
        {
            frame = 0;
            if (setPauseTimer)
            {
                pauseTimer = pauseDuration;
            }
        }
        else if (frame < 0)
        {
            frame = positionData.Length - 1;
            if (setPauseTimer)
            {
                pauseTimer = pauseDuration;
            }
        }
        
        UpdateStructure();
        ProgressBar.Inst.UpdateBar();
    }

    void FixedUpdate () {
        if (run_anim)
        {
            if (pauseTimer > 0)
            {
                pauseTimer -= Time.deltaTime;
            }
            else
            {
                //UpdateStructure();
//            if (!waitFrame)
//            {
//                Show();
//                waitFrame = false;
//                return;
//            }

                //ProgressBar.Inst.UpdateBar();
                //
                //frame += 1;
                //if (frame >= positionData.Length)
                //{
                //    frame = 0;
                //    pauseTimer = pauseDuration;
                //}

                int[] stepChanges = {-2, -1, -1, 1, 1, 2};
                
                if (animSpeed == 2 || animSpeed == 3)
                {
                    halfSpeedFlag = !halfSpeedFlag;
                    if (halfSpeedFlag)
                    {
                        ChangeFrame(frame + stepChanges[animSpeed], true);
                    }
                }
                else
                {
                    ChangeFrame(frame + stepChanges[animSpeed], true);
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
        //Debug.LogWarning("Function not implemented at the moment");
        animSpeed += change;
        // bound the speed to between 0 and 5
        animSpeed = animSpeed < 0 ? 0 : animSpeed;
        animSpeed = animSpeed > 5 ? 5 : animSpeed;

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
    //Debug.LogWarning("Function not implemented at the moment");
    
    if (forward)
        ChangeFrame(frame + 1);
    else
        //frame = (GetCurrFrameData().frames - (Mod(GetCurrFrameData().frames - frame, GetCurrFrameData().frames))) - 1;
        ChangeFrame(frame - 1);
    
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
            UpdateStructure();
//            ImportStructure.Inst.LoadStructure();
    }
    
    // ----- Control Animation with touchpad / joystick -----
    
    /*public void WhileTouchpadPressDown(Vector2 touchPos)
    {
        if (_moveOneFrameTimer >= 0)
        {
            _moveOneFrameTimer += Time.deltaTime;
            if (_moveOneFrameTimer >= timeUntilMoveOneFrame)
            {
                if (touchPos.x > 0)
                    AnimationController.move_one_frame(true);
                else
                    AnimationController.move_one_frame(false);
                _moveOneFrameTimer = 0;
            }
        }
    }

    public void TouchpadPressUp()
    {
        _moveOneFrameTimer = -1;
    }

    public void TouchpadPressDown(Vector2 touchPos)
    {
        // look if an animation should be started or stopped
        if (ModeController.currentMode.showTemp || ModeController.currentMode.showRelaxation)
            // check that the player isn't currently trying to change the length of the laser
            if (!laser.activeSelf)
                ControllAnimation(touchPos);
    }
    
    private void ControllAnimation(Vector2 touchPos)
    {
        if (touchPos.x > 0.5)
            if (AnimationController.run_anim)
                AnimationController.ChangeAnimSpeed(1);
            else
            {
                //LoadNewLammps();

                // go one frame forward
                AnimationController.move_one_frame(true);
                // show that the user pressed the button to go one step forward
                _moveOneFrameTimer = 0;
            }
        else if (touchPos.x < -0.5)
            if (AnimationController.run_anim)
            {
                // send Python the order to play the animation faster. if it isn't already at it's fastest speed
                if (AnimationController.animSpeed > 0)
                    AnimationController.animSpeed -= 1;
            }
            else
            {
                //LoadNewLammps();

                // go one frame back
                AnimationController.move_one_frame(false);
                _moveOneFrameTimer = 0;
            }
        else if (AnimationController.run_anim)
            AnimationController.RunAnim(false);
        else
        {
            //LoadNewLammps();

            // tell Python to start sending the dataframes from the current ham_lammps
            AnimationController.RunAnim(true);
        }

        // TODO: update the symbols on on all active controllers
        //gameObject.GetComponent<ControllerSymbols>().SetSymbol();
        //if (otherLg.gameObject.activeSelf)
        //    controllerSymbols[(int) otherLg.ctrlLayer].SetSymbol();
    }*/
    
    // ----- Utility ----- (should be in another script)

    private static int Mod(int a, int b)
    {
        if (b != 0)
            return (a % b + b) % b;
        return 0;
    }
}
