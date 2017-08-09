using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    [Header("Scene Data")]
    // the global settings of the program
    public ProgramSettings Settings;
    // the script of the controller printer
    public InGamePrinter printer;

    [Header("Masks")] // the controller mask and it's name
    public LayerMask ctrlMask;
    private string ctrlMaskName;

    [Header("Controller")]
    // get the reference of LaserGrabber
    private LaserGrabber LG;
    // the object of the controller
    //private SteamVR_TrackedObject trackedObj;
    // get the device of the controller
    private SteamVR_Controller.Device Controller;
    //{
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}

    void Start()
    {
        // set the variable to the name of the mask
        ctrlMaskName = Settings.getLayerName(ctrlMask);
        // get the reference of LaserGrabber
        LG = gameObject.GetComponent<LaserGrabber>();
        // get the reference of the controller from the LaserGrabber script
        Controller = LG.Controller;
    }

    // Update is called once per frame
    void Update () {
        // check if the application menu button is down to print before the controller
        CheckApplicationMenu();
    }

    private void CheckApplicationMenu()
    {
        if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
            if (ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(true);
            else
                printer.printers[1].gameObject.SetActive(true);

        if (Controller.GetTouchUp(SteamVR_Controller.ButtonMask.Grip))
            if (ctrlMaskName.Contains("Atom"))
                printer.printers[0].gameObject.SetActive(false);
            else
                printer.printers[1].gameObject.SetActive(false);
    }
}
