using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// component of AnimationPanel
public class AnimationMenuController : MenuController {
    internal static AnimationMenuController inst;
    public Text speedText;

    private void Awake()
    {
        inst = this;
    }

    public void Update()
    {
        speedText.text = "Speed: " + AnimationController.animSpeed;
    }

    // TODO: I think it would be better to call these functions directly from Unity rather then delegating them
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
        if (tog.GetComponentInChildren<Text>().text == "Start / Stop")
        {
            //if (tog.isOn)
            //    LaserGrabber.LoadNewLammps();
            //AnimationController.RunAnim(tog.isOn);
            AnimationController.run_anim = tog.isOn;
        }
    }
}
