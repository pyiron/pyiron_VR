using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

// TODO: I think it would be a good idea if teh was no conversion from HandRole to int.

// Component of Settings
// Handles all the relevant Input Data.
public class InputManager : MonoBehaviour
{
    [Header("Scene Data")]
    // get the data about the modes
    public ModeData MD;

    //[Header("Controller")]
    // the two controllers
    //public GameObject[] Controllers = new GameObject[2];  // TODO: should be private
    // get the reference of LaserGrabber
    // LaserGrabber[] LGs = new LaserGrabber[2];
    // get the device of the controller
    //private SteamVR_Controller.Device[] ControllerDevices = new SteamVR_Controller.Device[2];


    void Update()
    {
        foreach (HandRole handRole in System.Enum.GetValues(typeof(HandRole)))
            CheckViveController(handRole);
        CheckKeyboard();
    }

    private void CheckViveController(HandRole handRole)
    {
        // check the state of the button on the back of the controller and perform following actions
        CheckHairTrigger(handRole);
        // check the state of the touchpad and perform following actions
        CheckTouchpad(handRole);
        // check if the application menu button is down to print before the controller
        CheckGripButton(handRole);
        // check if the applicationMenu button is down to switch the mode
        CheckapplicationMenu(handRole);
    }

    public void CheckHairTrigger(HandRole handRole)
    {
        if (ViveInput.GetPressDown(handRole, ControllerButton.Trigger)) {
            SceneReferences.inst.LGs[(int)handRole].HairTriggerDown();
        }

        if (ViveInput.GetPress(handRole, ControllerButton.Trigger))
            SceneReferences.inst.LGs[(int)handRole].WhileHairTriggerDown();

        if (ViveInput.GetPressUp(handRole, ControllerButton.Trigger))
            SceneReferences.inst.LGs[(int)handRole].HairTriggerUp();
    }

    private void CheckTouchpad(HandRole handRole)
    {
        Vector2 touchPos = ViveInput.GetPadTouchAxis(handRole);
        if (ViveInput.GetPressDown(handRole, ControllerButton.PadTouch))
            SceneReferences.inst.LGs[(int)handRole].TouchpadTouchDown(touchPos);

        if (ViveInput.GetPress(handRole, ControllerButton.PadTouch))
            SceneReferences.inst.LGs[(int)handRole].WhileTouchpadTouchDown(touchPos);

        if (ViveInput.GetPressUp(handRole, ControllerButton.PadTouch))
            SceneReferences.inst.LGs[(int)handRole].TouchpadTouchUp();

        if (ViveInput.GetPressDown(handRole, ControllerButton.Pad))
            SceneReferences.inst.LGs[(int)handRole].TouchpadPressDown(touchPos);

        if (ViveInput.GetPress(handRole, ControllerButton.Pad))
            SceneReferences.inst.LGs[(int)handRole].WhileTouchpadPressDown(touchPos);

        if (ViveInput.GetPressUp(handRole, ControllerButton.Pad))
            SceneReferences.inst.LGs[(int)handRole].TouchpadPressUp();
    }

    private void CheckGripButton(HandRole handRole)
    {
        if (ViveInput.GetPressDown(handRole, ControllerButton.Grip))
            InGamePrinter.inst[(int)handRole].SetState(true);

        if (ViveInput.GetPressUp(handRole, ControllerButton.Grip))
            InGamePrinter.inst[(int)handRole].SetState(false);
    }

    // check if the application menu button has been pressed. If that's the case, go to the next mode
    private void CheckapplicationMenu(HandRole handRole)
    {
        if (ViveInput.GetPressDown(handRole, ControllerButton.Menu))
        {
            ViveInput.TriggerHapticPulse(handRole, 5000);
            print(PythonExecuter.loadedStructure);
            //if (!ModeData.currentMode.showPossibleStructures || PythonExecuter.loadedStructure)
            //{
            MD.RaiseMode();
            //}
        }
    }

    private void CheckKeyboard()
    {
        if (Input.anyKeyDown)
            if (Input.GetKeyDown(KeyCode.Escape))
                // quits the program when it isn't run in the Editor
                Application.Quit();
    }
}