using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// component of AnimationPanel
public class AnimationMenuController : MenuController {
    internal static AnimationMenuController Inst;
    
    [Header("The toggle in the menu that shows and decides whether the animation should run or not")]
    public Toggle startStopToggle;
    
    public Text speedText;
    
    internal override void SetState(bool active)
    {
        base.SetState(active);
        ProgressBar.Inst.gameObject.SetActive(active);
    }

    private void Awake()
    {
        Inst = this;
    }

    public void Update()
    {
        speedText.text = "Speed: " + AnimationController.animSpeed;
    }

    // TODO: I think it might be better to call these functions directly from Unity rather then delegating them
    public void OnButtonDown(Button btn)
    {
        Text btn_txt = btn.GetComponentInChildren<Text>();
        if (btn_txt.text == "Up")
            AnimationController.ChangeAnimSpeed(1);
        else if (btn_txt.text == "Down")
            AnimationController.ChangeAnimSpeed(-1);
        else if (btn_txt.text == "Step forward")
            AnimationController.move_one_frame(true);
        else if (btn_txt.text == "Step backward")
            AnimationController.move_one_frame(false);
        else if (btn_txt.text == "Reset animation")
            AnimationController.ResetAnimation();
    }

    public void OnToggleChange(Toggle tog)
    {
        if (tog.GetComponentInChildren<Text>().text.Contains("Start / Stop"))
        {
            //if (tog.isOn)
            //    LaserGrabber.LoadNewLammps();
            AnimationController.RunAnim(tog.isOn);
            //AnimationController.run_anim = tog.isOn;
        }
    }

    public void OnSimBtnDown()
    {
        // Send an update to Pyiron what the current frame is
        PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l,
            "job.structure = unity_manager.Executor.job.get_structure("
            + AnimationController.frame + ")");
            //"unity_manager.Executor.frame = " + AnimationController.frame);
        ModeController.inst.SetMode(Modes.Calculate);
    }
}
