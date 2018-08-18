using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of Settings
// Handles all the relevant Input Data.
public class InputManager : MonoBehaviour
{
    [Header("Scene Data")]
    // the script of the controller printer
    public InGamePrinter printer;
    // get the data about the modes
    public ModeData MD;

    [Header("Controller")]
    // the two controllers
    //public GameObject[] Controllers = new GameObject[2];  // TODO: should be private
    // get the reference of LaserGrabber
    // LaserGrabber[] LGs = new LaserGrabber[2];
    // get the device of the controller
    private SteamVR_Controller.Device[] ControllerDevices = new SteamVR_Controller.Device[2];

    // Use this for initialization
    void Start()
    {
        for (int ctrlNr = 0; ctrlNr < 2; ctrlNr++)
        {
            if (SceneReferences.inst.Controllers[0].activeSelf)
                GetControllerReferences(ctrlNr);

                //printer.Ctrl_print(MD.activeMode.ToString(), 4);
        }
    }

    // get the references to the Script which controlls the controller, which isn't possible before the controller is activated
    private void GetControllerReferences(int ctrlNr)
    {
        // get the reference of the controller from the LaserGrabber script
        ControllerDevices[ctrlNr] = SceneReferences.inst.LGs[ctrlNr].Controller;
    }

    void Update()
    {
        print("update");
        for (int ctrlNr = 0; ctrlNr < 2; ctrlNr++)
            if (SceneReferences.inst.Controllers[ctrlNr].activeSelf)
                if (ControllerDevices[ctrlNr] == null)
                    GetControllerReferences(ctrlNr);
                else
                    CheckViveController(ctrlNr);
        CheckKeyboard();
    }

    private void CheckViveController(int ctrlNr)
    {
        // check the state of the button on the back of the controller and perform following actions
        CheckHairTrigger(ctrlNr);
        // check the state of the touchpad and perform following actions
        CheckTouchpad(ctrlNr);
        // check if the application menu button is down to print before the controller
        CheckGripButton(ctrlNr);
        // check if the applicationMenu button is down to switch the mode
        CheckapplicationMenu(ctrlNr);
        print("checked Controller");
    }

    public void CheckHairTrigger(int ctrlNr)
    {
        if (ControllerDevices[ctrlNr].GetHairTriggerDown())
            SceneReferences.inst.LGs[ctrlNr].HairTriggerDown();

        if (ControllerDevices[ctrlNr].GetHairTrigger())
            SceneReferences.inst.LGs[ctrlNr].WhileHairTriggerDown();

        if (ControllerDevices[ctrlNr].GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            SceneReferences.inst.LGs[ctrlNr].HairTriggerUp();
    }

    private void CheckTouchpad(int ctrlNr)
    {
        if (ControllerDevices[ctrlNr].GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].TouchpadTouchDown();

        if (ControllerDevices[ctrlNr].GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].WhileTouchpadTouchDown();

        if (ControllerDevices[ctrlNr].GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].TouchpadTouchUp();

        if (ControllerDevices[ctrlNr].GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].TouchpadPressDown();

        if (ControllerDevices[ctrlNr].GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].WhileTouchpadPressDown();

        if (ControllerDevices[ctrlNr].GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            SceneReferences.inst.LGs[ctrlNr].TouchpadPressUp();
    }

    private void CheckGripButton(int ctrlNr)
    {
        if (ControllerDevices[ctrlNr].GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            if (SceneReferences.inst.LGs[ctrlNr].ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(true);
            else
                printer.printers[1].gameObject.SetActive(true);

        if (ControllerDevices[ctrlNr].GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            if (SceneReferences.inst.LGs[ctrlNr].ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(false);
            else
                printer.printers[1].gameObject.SetActive(false);
    }

    

    // check if the application menu button has been pressed. If that's the case, go to the next mode
    private void CheckapplicationMenu(int ctrlNr)
    {
        if (ControllerDevices[ctrlNr].GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            print(PythonExecuter.loadedStructure);
            if (!ModeData.currentMode.showPossibleStructures || PythonExecuter.loadedStructure)
                MD.RaiseMode();
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