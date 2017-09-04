using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Scene Data")]
    // the script of the controller printer
    public InGamePrinter printer;
    // get the data about the modes
    public ModeData MD;

    [Header("Controller")]
    // the two controllers
    public GameObject[] controllerObjects = new GameObject[2];
    // get the reference of LaserGrabber
    private LaserGrabber[] LGs = new LaserGrabber[2];
    // get the device of the controller
    private SteamVR_Controller.Device[] ControllerDevices = new SteamVR_Controller.Device[2];


    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            if (controllerObjects[0].activeSelf)
                GetControllerReferences(i);

                //printer.Ctrl_print(MD.activeMode.ToString(), 4);
        }
    }

    // get the references to the Script which controlls the controller, which isn't possible before the controller is activated
    private void GetControllerReferences(int ctrlNr)
    {
        // get the reference of LaserGrabber
        LGs[ctrlNr] = controllerObjects[ctrlNr].GetComponent<LaserGrabber>();
        // get the reference of the controller from the LaserGrabber script
        ControllerDevices[ctrlNr] = LGs[ctrlNr].Controller;
    }

    void Update()
    {
        for (int i = 0; i < 2; i++)
            if (controllerObjects[i].activeSelf)
                if (LGs[i] == null)
                    GetControllerReferences(i);
                else
                    CheckViveController(i);
    }

    private void CheckViveController(int nr)
    {
        // check the state of the button on the back of the controller and perform following actions
        LGs[nr].CheckHairTrigger();
        // check the state of the touchpad and perform following actions
        LGs[nr].CheckTouchpad();

        // check if the application menu button is down to print before the controller
        CheckGripButton(nr);
        // check if the applicationMenu button is down to switch the mode
        CheckapplicationMenu(nr);
    }

    private void CheckGripButton(int nr)
    {
        if (ControllerDevices[nr].GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
            if (LGs[nr].ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(true);
            else
                printer.printers[1].gameObject.SetActive(true);

        if (ControllerDevices[nr].GetTouchUp(SteamVR_Controller.ButtonMask.Grip))
            if (LGs[nr].ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(false);
            else
                printer.printers[1].gameObject.SetActive(false);
    }

    // check if the application menu button has been pressed. If that's the case, go to the next mode
    private void CheckapplicationMenu(int nr)
    {
        if (ControllerDevices[nr].GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            MD.RaiseMode();
            printer.Ctrl_print(MD.activeMode.ToString(), 40);
        }
    }
}