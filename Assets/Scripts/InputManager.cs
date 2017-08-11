using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    [Header("Scene Data")]
    // the global settings of the program
    public ProgramSettings Settings;
    // the script of the controller printer
    public InGamePrinter printer;
    // get the data about the modes
    public ModeData MD;

    [Header("Controller")]
    // get the reference of LaserGrabber
    private LaserGrabber LG;
    // get the device of the controller
    private SteamVR_Controller.Device Controller;


    void Start()
    {
        // get the reference of LaserGrabber
        LG = gameObject.GetComponent<LaserGrabber>();
        // get the reference of the controller from the LaserGrabber script
        Controller = LG.Controller;

        printer.Ctrl_print(MD.modeNr.ToString(), 4);
    }

    // Update is called once per frame
    void Update () {
        // check the state of the button on the back of the controller and perform following actions
        LG.CheckHairTrigger();
        // check the state of the touchpad and perform following actions
        LG.CheckTouchpad();
        
        // check if the application menu button is down to print before the controller
        CheckGripButton();
        // check if the applicationMenu button is down to switch the mode
        CheckapplicationMenu();
    }

    private void CheckGripButton()
    {
        if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
            if (LG.ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(true);
            else
                printer.printers[1].gameObject.SetActive(true);

        if (Controller.GetTouchUp(SteamVR_Controller.ButtonMask.Grip))
            if (LG.ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(false);
            else
                printer.printers[1].gameObject.SetActive(false);
    }

    private void CheckapplicationMenu()
    {
        if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            MD.raiseMode();
            printer.Ctrl_print(MD.modeNr.ToString(), 40);
        }
    }
}
