using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using Networking;

// TODO: I think it would be a good idea if teh was no conversion from HandRole to int.

// Component of Settings
// Handles all the relevant Input Data.
public class InputManager : MonoBehaviour
{
    [Header("Scene Data")]
    // the Canvas for the menus. Needed to dis/enable it.
    public GameObject Canvas;
    // the pointers which contain the laser. Needed to dis/enable the laser.
    public GameObject[] pointers;

    public static bool DeviceHasJoystick = true;

    void Update()
    {
        if (TCPClient.TaskNumOut == TCPClient.TaskNumIn)
        {
            foreach (HandRole handRole in System.Enum.GetValues(typeof(HandRole)))
                CheckViveController(handRole);
            CheckKeyboard();
        }
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
            LaserGrabber.instances[(int)handRole].HairTriggerDown();
        }

        if (ViveInput.GetPress(handRole, ControllerButton.Trigger))
            LaserGrabber.instances[(int)handRole].WhileHairTriggerDown();

        if (ViveInput.GetPressUp(handRole, ControllerButton.Trigger))
            LaserGrabber.instances[(int)handRole].HairTriggerUp();
    }

    private void CheckTouchpad(HandRole handRole)
    {
        //Vector2 touchPos = ViveInput.GetPadAxis(handRole);
        Vector2 touchPos = ViveInput.GetPadPressVector(handRole);
        if (ViveInput.GetPressDown(handRole, ControllerButton.Pad))
            LaserGrabber.instances[(int)handRole].TouchpadTouchDown(touchPos);

        if (ViveInput.GetPress(handRole, ControllerButton.Pad))
            LaserGrabber.instances[(int)handRole].WhileTouchpadTouchDown(touchPos);

        if (ViveInput.GetPressUp(handRole, ControllerButton.Pad))
            LaserGrabber.instances[(int)handRole].TouchpadTouchUp();

        if (ViveInput.GetPressDown(handRole, ControllerButton.Pad))
        {
            // LaserGrabber.instances[(int)handRole].TouchpadPressDown(touchPos);
            Thermometer.Inst.TouchpadPressDown(touchPos);
        }

        if (ViveInput.GetPress(handRole, ControllerButton.Pad))
        {
            //LaserGrabber.instances[(int) handRole].WhileTouchpadPressDown(touchPos);
        }

        if (ViveInput.GetPressUp(handRole, ControllerButton.Pad))
        {
            //LaserGrabber.instances[(int) handRole].TouchpadPressUp();
        }
    }

    private void CheckGripButton(HandRole handRole)
    {
        //if (ViveInput.GetPressDown(handRole, ControllerButton.Grip))
        //    InGamePrinter.inst[(int)handRole].SetState(true);

        //if (ViveInput.GetPressUp(handRole, ControllerButton.Grip))
        //    InGamePrinter.inst[(int)handRole].SetState(false);

        if (ViveInput.GetPressDown(handRole, ControllerButton.Grip))
        {
            //print(Canvas.transform.parent);
            //Canvas.SetActive(!Canvas.activeSelf);
            Transform Controller = LaserGrabber.instances[1].transform;
            Transform Reticle = Controller.parent.parent.GetComponentsInChildren<ReticlePoser>()[0].transform;
            if (Canvas.transform.parent == null)
            {
                Canvas.transform.SetParent(Controller);
                Reticle.localScale = Vector3.one * 0.1f;
                Canvas.transform.localPosition = Vector3.up * 0.25f;
                Canvas.transform.localEulerAngles = Vector3.up * 0;
                Canvas.transform.localScale /= 10;
            }
            else
            {
                Canvas.transform.SetParent(null);
                Reticle.localScale = Vector3.one;
                Canvas.transform.localPosition = new Vector3(0, 2.9f, 3);
                Canvas.transform.localEulerAngles = Vector3.zero;
                Canvas.transform.localScale *= 10;
            }
        }
    }

    // check if the application menu button has been pressed. If that's the case, go to the next mode
    private void CheckapplicationMenu(HandRole handRole)
    {
        if (ViveInput.GetPressDown(handRole, ControllerButton.Menu))
        {
            // Switch the VIU laser on/off
            pointers[(int)handRole].SetActive(!pointers[(int)handRole].activeSelf);
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