using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Component of both controllers
public class LaserGrabber : SceneReferences
{
    [Header("Scene Data")]
    // the data about the structure
    private StructureData SD;
    // the properties about the resize
    private StructureResizer StrucResizer;
    // the gameobject of the structure
    public GameObject AtomStructure;
    // the script of the controller printer
    public InGamePrinter printer;
    // the Script of the Hourglass, which indicates that the structure is currently loading
    private Hourglass HourglassScript;

    // all data about the modes, f.e. which mode is currently active
    public ModeData MD;

    // the script of the trashcan in which atoms can be thrown
    private TrashCan TrashCanScript;

    [Header("Move Objects")]
    // the object the controller is currently colliding with
    private GameObject collidingObject;
    // the object which is currently attached to the controller
    public GameObject attachedObject;
    // the Vector between the controller position and the grabbed object position
    private Vector3 objToHandPos;
    // the Vector between the controller rotation and the grabbed object rotation
    // private Vector3 objToHandRot; // might be needed to rotate the atom
    // the vector between the positions of the boundingbox and it's parent
    private Transform boundingbox;

    [Header("Laser")]
    // the prefab for the laser
    public GameObject LaserPrefab;
    // the instance of the laser in the game
    public GameObject laser;
    // the transform of the laser gameobject
    private Transform laserTransform;
    // the hitpoint where the laser met an object
    private Vector3 hitPoint;
    // the length of the laser
    private float laserLength;
    // the max distance when the laser still detects an object to attach
    private int laserMaxDistance = 100;

    [Header("Thermometer")]
    // shows if the user hitted the thermometer with the laser when he pressed the hair trigger down the last time
    private bool laserOnThermometer = false;
    // shows whether the laser is currently pointing at the thermometer
    private bool laserCurrentlyOnThermometer = false;
    // the reference to the thermometer
    private GameObject ThermometerObject;
    // the script of the thermometer
    private Thermometer thermometerScript;

    [Header("Change Animation")]
    // shows whether it is the first time an animation should be played,
    // so that the python program knows whether to load a new animation or not
    private bool firstAnimStart = true;
    // shows if the current lammps is a calc_md or calc_minimize
    private bool lammpsIsMd;
    // shows whether the positions of the atoms are still the same es they were when the last ham_lammps was created
    private static bool positionsHaveChanged;

    [Header("Resize")]
    // shows, which of the controllers currently allows to resize the structure
    public GameObject resizeableRect;
    // the state of the other controller, needed to look if both hairtriggers are pressed, so that the structure should be resized
    public bool readyForResize;

    [Header("Show Infos")]
    // get the reference of the InfoText
    public TextMesh InfoText;
    // the size the text should have
    private float textSize = 1f;

    [Header("Controller")]
    // the start touch point from when the player lays his finger on the touchpad
    private Vector2 startTouchPoint;
    // the point where the player currently touches the touchpad
    private Vector2 currentTouch;
    // the object of the controller
    // the controller mask and it's name
    public LayerMask ctrlMask;
    public string ctrlMaskName;

    private SteamVR_TrackedObject trackedObj;
    // get the device of the controller
    public SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    // the other controller
    public GameObject otherCtrl;

    void Awake()
    {
        // get the references to all objects and scripts related to the Settings
        GetSettingsReferences();
        try {
            // find the trash can
            TrashCanScript = GameObject.Find("Trash Can").GetComponent<TrashCan>();
        }
        catch { TrashCanScript = otherCtrl.GetComponent<LaserGrabber>().TrashCanScript; }
        // get the data of the controller
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        
        // get the Script of the Hourglass, which indicates that the structure is currently loading
        HourglassScript = GameObject.Find("HourglassRotator").transform.GetChild(0).gameObject.GetComponent<Hourglass>();
}

    void Start()
    {
        InitLaser();

        // set the variable to the name of the mask
        ctrlMaskName = ProgramSettings.GetLayerName(ctrlMask);
        // get the script StructureData from AtomStructure
        SD = AtomStructure.GetComponent<StructureData>();
        // get the script StructureResizer from AtomStructure
        StrucResizer = AtomStructure.GetComponent<StructureResizer>();

        // get the transform of the boundingbox
        foreach (Transform tr in AtomStructure.GetComponentsInChildren<Transform>())
            if (tr.name == "Boundingbox(Clone)")
                boundingbox = tr;

        textSize = textSize / ProgramSettings.textResolution * 10;
        InfoText.transform.localScale = Vector3.one * textSize;
        InfoText.fontSize = (int)ProgramSettings.textResolution;

        // get the references to the thermometer related objects from the other controller, if it is active and knows them
        if (otherCtrl.activeSelf)
        {
            ThermometerObject = otherCtrl.GetComponent<LaserGrabber>().ThermometerObject;
            thermometerScript = otherCtrl.GetComponent<LaserGrabber>().thermometerScript;
        }

        // deactivate the trashcan
        TrashCanScript.gameObject.SetActive(false);
    }

    private void InitLaser()
    {
        // create an instance of the laser
        laser = Instantiate(LaserPrefab);

        laserTransform = laser.transform;
        // set the controller as the parent of the laser
        laserTransform.parent = gameObject.transform;
    }

    void Update()
    {
        printer.Ctrl_print(PythonExecuter.outgoingChanges.ToString(), 120);
        printer.Ctrl_print(PythonExecuter.incomingChanges.ToString(), 120, false);
        if (MD.modes[MD.activeMode].playerCanMoveAtoms)
        {
            // move the grabbed object
            if (attachedObject)
            {
                MoveGrabbedObject();
                if (ctrlMaskName.Contains("Atom"))
                {
                    // update the trashcan, if it is shown in the current mode
                    if (MD.modes[MD.activeMode].showTrashcan)
                        TrashCanScript.UpdateTrashCan(attachedObject);
                    // let the controller printer print the atom ID of the current attached atom
                    printer.Ctrl_print(attachedObject.GetComponent<AtomID>().ID.ToString(), 101);
                }
                else
                    // update the rotation of the Hourglass
                    HourglassScript.transform.parent.eulerAngles = Vector3.up * transform.eulerAngles.y;
            }
        }

        // show the white rect if this controller is ready for a resize of the structure
        resizeableRect.SetActive(readyForResize);
    }

    public void HairTriggerDown()
    {
        // if the controller gets pressed, it should try to attach an object to it
        if (MD.modes[MD.activeMode].playerCanMoveAtoms)
        {
            // test if the other controller is ready for a resize
            if (otherCtrl.GetComponent<LaserGrabber>().readyForResize)
                // init the resize, because now are both controllers ready
                if (ctrlMaskName == "AtomLayer")
                    InitResize();
                else;
            // if an object is colliding with the controller, it should be attached
            else if (collidingObject)
            {
                AttachObject(collidingObject);
                // set the controller ready for size, if it grabs the boundingbox
                if (ctrlMaskName == "BoundingboxLayer")
                    // initialize the resize if both controllers are ready, else just set the controller to ready
                    SetControllerToReady();
            }
            else
                // send out a raycast to detect objects in front of the controller
                SendRaycast();
        }
        //if (MD.modes[MD.activeMode].showTemp)
        //    SendRaycast(thermometerMask);
    }

    public void WhileHairTriggerDown()
    {
        if (laserOnThermometer)
            SendRaycast();
        else if (!MD.modes[MD.activeMode].playerCanMoveAtoms)
        {
            SendRaycast();
            if (MD.modes[MD.activeMode].showInfo)
                ShowInfo();
        }
        else
            InfoText.gameObject.SetActive(false);
    }

    private void ShowInfo()
    {
        if (laser.activeSelf || collidingObject)
        {
            // set the Infotext to active and edit it 
            InfoText.gameObject.SetActive(true);
            // let the InfoText always look in the direction of the player
            ProgramSettings.Face_Player(InfoText.gameObject);
            //InfoText.transform.eulerAngles = new Vector3(0, HeadTransform.eulerAngles.y, 0);
            if (ctrlMaskName.Contains("AtomLayer"))
            {
                if (attachedObject != null)
                {
                    int atomId = attachedObject.GetComponent<AtomID>().ID;
                    InfoText.transform.position = attachedObject.transform.position // + Vector3.up * 0.1f
                            + Vector3.up * attachedObject.transform.localScale[0] / 2 * ProgramSettings.size;
                    InfoText.text = SD.atomInfos[atomId].m_type;
                    // test if the forces are known
                    if (PythonExecuter.allForces[atomId][0] != -1)
                    {
                        // show the force of the atom in each direction
                        InfoText.text += "\nForce:";
                        for (int i = 0; i < 3; i++)
                            InfoText.text += " " + PythonExecuter.allForces[atomId][i];
                    }
                }
            }
            else
            {
                // set the info text to the top of the boundingbox
                InfoText.transform.position = boundingbox.transform.position + Vector3.up * 0.1f
                    + Vector3.up * boundingbox.transform.localScale[0] / 2 * ProgramSettings.size;
                InfoText.text = SD.structureName;
                InfoText.text += "\nAtoms: "
                        + SD.atomInfos.Count;
                // InfoText.text += "\nForce: " + PythonExecuter.structureForce;
                //might be needed so that the text will stand above the boundingbox
                //InfoText.GetComponent<TextMesh>().text += "\n";
            }
        }
        else
            InfoText.gameObject.SetActive(false);
    }

    public void HairTriggerUp()
    {
        // check if the player has clicked on the thermometer
        if (laserOnThermometer)
        {
            // show that the thermometer isn't being clicked anymore
            laserOnThermometer = false;
            laserCurrentlyOnThermometer = false;
            // turn the color of the thermometer back to it's usual color
            thermometerScript.ChangeLiquidColor();
            // deactivate the laser
            laser.SetActive(false);
        }
            // check that the move mode is currently active
            if (MD.modes[MD.activeMode].playerCanMoveAtoms)
        {
            // set the state of the controller to not ready for resizeStructure
            readyForResize = false;

            // disattach the attached object
            if (attachedObject)
            {
                // deactivate the laser
                if (laser.activeSelf)
                    laser.SetActive(false);

                // change the boundingbox to the new extension of the structure, if an atom has been attached
                if (ctrlMaskName == "AtomLayer")
                {
                    // check the new extension of the structure
                    SD.SearchMaxAndMin();
                    // set the boundingbox so that it encloses the structure
                    SD.UpdateBoundingbox();
                }

                ReleaseObject();
            }
        }
        else
        {
            // detach the attached object and deactivate the laser when the press on the trigger is up
            attachedObject = null;
            laser.SetActive(false);
        }
    }

    // show that the laser is currently active and it's possible in the current move to move atoms, and that the laser doesn't point on the thermometer
    private bool ScaleAbleLaser()
    {
        return (MD.modes[MD.activeMode].playerCanMoveAtoms && laser.activeSelf && !laserOnThermometer);
    }

    public void TouchpadTouchDown()
    {
        if (ScaleAbleLaser())
            // mark the start touchpoint
            startTouchPoint = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
    }

    public void WhileTouchpadTouchDown()
    {
        if (ScaleAbleLaser())
        {
            // scale the laser
            currentTouch = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            ScaleLaser(currentTouch.y - startTouchPoint.y);

            // set the max distance the laser should have to exist and not grab the object
            float minLaserLength;
            if (ctrlMaskName == "BoundingboxLayer")
                minLaserLength = 0;
            else
                // set the distance to the width of the atom, so that the atom is in front of the controller, and not in it
                minLaserLength = attachedObject.transform.localScale.x * ProgramSettings.size / 2;

            // if the laserlength is changed to a value less than the minimum distance, the attached object is going to be grabbed
            if (laserLength + currentTouch.y - startTouchPoint.y <= minLaserLength)
            {
                AttachObject(attachedObject);
                laser.SetActive(false);
                if (ctrlMaskName == "BoundingboxLayer")
                    readyForResize = true;
            }
        }
    }

    public void TouchpadTouchUp()
    {
        if (ScaleAbleLaser())
        {
            // scale the laser to the new laserlength
            laserLength += currentTouch.y - startTouchPoint.y;
            ScaleLaser();
        }
    }

    public void TouchpadPressDown()
    {
        if (MD.modes[MD.activeMode].canDuplicate)
            DuplicateStructure();
        // look if an animation should be started or stopped
        else if (MD.modes[MD.activeMode].showTemp || MD.modes[MD.activeMode].showRelaxation)
            // check that the player isn't currently trying to change the length of the laser
            if (!laser.activeSelf)
                ControllAnimation();
        if (laserOnThermometer)
            // increases the maxTemperature by a factor of 10 when pressing on the upper half of the touchpad
            if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0)
                thermometerScript.SetMaxTemperature(10);
            // decreases the maxTemperature by a factor of 10 when pressing on the lower half of the touchpad, if possible
            else
                thermometerScript.SetMaxTemperature(0.1f);
    }

    // initialize the resize if both controllers are ready, else just set the controller to ready
    private void SetControllerToReady()
    {
        if (otherCtrl.GetComponent<LaserGrabber>().readyForResize)
            InitResize();
        else
            readyForResize = true;
    }

    private void DuplicateStructure()
    {
        if (ctrlMaskName.Contains("BoundingboxLayer"))
            if (collidingObject || laser.activeSelf)
                if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0)
                    PE.SendOrder("self.duplicate(2)");
                else
                    if (SD.atomInfos.Count * 0.5 * 0.5 * 0.5 >= 1)
                        PE.SendOrder("self.duplicate(0.5)");
    }

    private void ControllAnimation()
    {
        if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x > 0.5)
            if (OrdersToPython.pythonRunsAnim)
                // send Python the order to play the animation faster. if it isn't already at it's fastest speed
                if (PE.pythonsAnimSpeed < 5)
                    PE.ChangeAnimSpeed(1);
                else;
            else
                // go one frame forward
                PE.SendOrder("self.frame = (self.frame + 1) % len(self.all_positions)");
        //PE.SendOrder(runAnim: true);
        else if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x < -0.5)
            if (OrdersToPython.pythonRunsAnim)
                // send Python the order to play the animation faster. if it isn't already at it's fastest speed
                if (PE.pythonsAnimSpeed > 0)
                    PE.ChangeAnimSpeed(-1);
                else;
            else
                // go one frame back
                PE.SendOrder("self.frame = (len(self.all_positions) - ((len(self.all_positions) - self.frame) " +
                    "% len(self.all_positions))) - 1");
        else if (OrdersToPython.pythonRunsAnim)
            OTP.RunAnim(false);
        else
        {
            bool temperatureHasChanged = false;
            // check if the thermometer has been initialised yet and is currently active
            if (thermometerScript != null)
            {

                // send Python the order to change the temperature if the user has changed the temperature on the thermometer
                if (thermometerScript.lastTemperature != ProgramSettings.temperature)
                {
                    PE.SendOrder("self.temperature = " + ProgramSettings.temperature);
                    // remember that a new ham_lammps has to be loaded
                    temperatureHasChanged = true;
                    // remember that the last ham_lammps has been created with the current temperature
                    thermometerScript.lastTemperature = ProgramSettings.temperature;
                }
            }

            // check if the positions of any atom has been changed since the last animation has been started
            if (PythonExecuter.frame != 0 || positionsHaveChanged)
            {
                // send the new positions to Python
                positionsHaveChanged = true;
                // send Python the new positions of all atoms
                OTP.SetNewPositions();
            }

            // when loading the first animation, show Python that it's the first time, so that it can check if there is already a loaded ham_lammps
            if (firstAnimStart)
            {
                LoadNewLammps("self.calculate");
                firstAnimStart = false;
            }
            // tell Python to create a new ham_lammps because the structure or it's temperature has changed
            else if (temperatureHasChanged || positionsHaveChanged)
            {
                LoadNewLammps("self.create_new_lammps");
                // remember that the ham_lammps is now according to the current structure
                positionsHaveChanged = false;
            }
            // load a new ham_lammps if the current ham_lammps is for md and the animation for minimize is needed or vice versa
            else if (lammpsIsMd != MD.modes[MD.activeMode].showTemp)
                LoadNewLammps("self.create_new_lammps");

            // tell Python to start sending the dataframes from the current ham_lammps
            OTP.RunAnim(true);
        }

        // update the symbols on on all active controllers
        UpdateSymbols();
    }

    // update the symbols on all active controllers
    public void UpdateSymbols()
    {
        gameObject.GetComponent<ControllerSymbols>().SetSymbol();
        if (otherCtrl.activeSelf)
            otherCtrl.GetComponent<ControllerSymbols>().SetSymbol();
    }

    private void LoadNewLammps(string loadOrder)
    {
        if (MD.modes[MD.activeMode].showTemp)
            PE.SendOrder(loadOrder + "('md')");
        else if (MD.modes[MD.activeMode].showRelaxation)
            PE.SendOrder(loadOrder + "('minimize')");
        lammpsIsMd = MD.modes[MD.activeMode].showTemp;
    }

    // checks if the laser hits an object, which it should hit (an atom or a structure)
    private void SendRaycast()
    {
        // check that there isn't an object in range to grab
        if (true) //(collidingObject == null)
        {
            RaycastHit hit;

            if (!attachedObject || !MD.modes[MD.activeMode].playerCanMoveAtoms || laserOnThermometer)
                // send out a raycast to detect if there is an object in front of the laser 
                if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, laserMaxDistance, ctrlMask))
                {
                    GameObject hittedObject = hit.transform.gameObject;
                    laser.SetActive(true);
                    hitPoint = hit.point;
                    ShowLaser(hit);

                    if (hittedObject.name.Contains("Thermometer"))
                    {
                        GetThermometerReference(hittedObject);
                        // set the references for the other controller as well, if the controller is activated yet
                        if (otherCtrl.activeSelf)
                            otherCtrl.GetComponent<LaserGrabber>().GetThermometerReference(hittedObject);

                        thermometerScript.ChangeThemperature(hitPoint.y);

                        if (!laserOnThermometer || !laserCurrentlyOnThermometer)
                        {
                            laserOnThermometer = true;
                            laserCurrentlyOnThermometer = true;
                            // set the color to a dark red to show that the user currently clicks on the thermometer
                            thermometerScript.ChangeLiquidColor("clicked");
                        }

                        if (OrdersToPython.pythonRunsAnim)
                            // stop the animation
                            OTP.RunAnim(false);
                    }
                    else if (!laserOnThermometer)
                        if (MD.modes[MD.activeMode].playerCanMoveAtoms)
                            AttachObject(hittedObject);
                        else
                            attachedObject = hittedObject;
                }
            // show that the laser no longer points on the thermometer
                else if (laserOnThermometer)
                {
                    laserCurrentlyOnThermometer = false;
                    laser.SetActive(false);
                    thermometerScript.ChangeLiquidColor("clickedButMovedAway");
                }
                // show that the controller is ready to resize the structure, if it is the AtomLayer controller
                else if (MD.modes[MD.activeMode].playerCanResizeAtoms)
                    if (ctrlMaskName == "AtomLayer")
                        readyForResize = true;
                    else;
                // deactivate the laser and show, detach the attached object if there is an object attached to the controller 
                else
                {
                    laser.SetActive(false);
                    attachedObject = null;
                }
        }
    }

    public void GetThermometerReference(GameObject hittedObject)
    {
        // get the reference to the thermometer, if it is not yet defined
        if (ThermometerObject == null)
        {
            if (hittedObject.name == "Thermometer")
                ThermometerObject = hittedObject;
            else
                ThermometerObject = hittedObject.transform.parent.gameObject;
            thermometerScript = ThermometerObject.GetComponent<Thermometer>();
        }
    }

    private void MoveGrabbedObject()
    {
        // the location before the object gets moved
        Vector3 oldPos;
        // the location after the object got moved
        Vector3 newPos;
        oldPos = attachedObject.transform.position;
        if (laser.activeSelf)
        {
            if (ctrlMaskName == "BoundingboxLayer")
            {
                // sets the position of the structure to the end of the laser plus the Vector between the structure and the boundingbox
                newPos = transform.position + (laser.transform.position - transform.position) * 2
                    + AtomStructure.transform.position - boundingbox.position;
                //attachedObject.transform.position = transform.position + (laser.transform.position - transform.position) * 2
                //        + AtomStructure.transform.position - boundingbox.position;
            }
            else
                // sets the position of the grabbed object to the end of the laser
                newPos = transform.position + (laser.transform.position - transform.position) * 2;
            //attachedObject.transform.position = transform.position + (laser.transform.position - transform.position) * 2;

            // would keep always the same side to the viewer:
            //  objectInHand.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + objToHandRot.y, 0);
        }
        else
        {
            newPos = transform.position + objToHandPos;
            // attachedObject.transform.position = transform.position + objToHandPos;
        }
        // set the position of the attached object to the new position it should have
        attachedObject.transform.position = newPos;
        if (ctrlMaskName == "AtomLayer")
            // update the data how the atom has been moved by the player 
            SD.atomCtrlPos[attachedObject.GetComponent<AtomID>().ID] += newPos - oldPos;
        else if (ctrlMaskName == "BoundingboxLayer")
            // update the data how the structure has been moved by the player 
            SD.structureCtrlPos += newPos - oldPos;
            // SD.structureCtrlTrans.position += newPos - oldPos;
    }

    // set the colliding object as the collidingObject, if it fulfills these conditions
    private void SetCollidingObject(Collider col)
    {
        // check that the colliding object has a rigidbody
        if (collidingObject || col.gameObject.name.Contains("Thermometer"))
            return;

        // checks if the colliding object is of interrest to the controller
        if (ctrlMaskName == LayerMask.LayerToName(col.gameObject.layer))
        {
            // set the colliding object
            collidingObject = col.gameObject;
            // disable the laser if the controller is colliding when the player can't move the atoms 
            // (because the lasr already disables itself in this mode)
            if (!MD.modes[MD.activeMode].playerCanMoveAtoms)
                laser.SetActive(false);
        }
    }

    // checks, if the controller collider collides with an object
    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    // checks, if the controller collider collides with an object
    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    // checks if the controller collider exits an object
    public void OnTriggerExit(Collider other)
    {
        // remove the object from collidingObject if there is any collidingObject
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    // init the resize and set the controllers state to ready for resize
    private void InitResize()
    {
        readyForResize = true;
        StrucResizer.InitResize();
    }

    private void ScaleLaser(float modification = 0)
    {
        Vector3 laserSize = laser.transform.localScale;
        laser.transform.position = transform.position + 
            (laser.transform.position - transform.position) * (laserLength + modification) / laserSize.z;
        laserSize.z = laserLength + modification;
        laser.transform.localScale = laserSize;
    }

    private void AttachObject(GameObject grabAbleObject)
    {
        // test, which controller is trying to grab the object
        if (ctrlMaskName == "BoundingboxLayer")
        {
            // the grabbed object is the boundingbox, it's parent the atomstructure. so this controller grabs the whole structure
            attachedObject = grabAbleObject.transform.root.gameObject;

            // the laserlength has to be set to the length from the controller to the boundingbox, because it's attached to it's middle point,
            // and the object should be at the same distance before and after the start of the grab
            laserLength = (boundingbox.position - transform.position).magnitude;
        }
        else if (ctrlMaskName == "AtomLayer")
        {
            // set the atom as the attached object
            attachedObject = grabAbleObject;
            // the laserlength has to be set to the length from the controller to the atom, because it's attached to it's middle point, 
            //and the object should be at the same distance before and after the start of the grab
            laserLength = (attachedObject.transform.position - transform.position).magnitude;
            // spawn the trashcan
            TrashCanScript.ActivateCan();

            positionsHaveChanged = true;
            // deactivate the animation
            OTP.RunAnim(false);
        }
        
        // set the length of the laser to it's new length
        ScaleLaser();
        // remembers the position where the object is grabbed, relativ to the objects middle point
        objToHandPos = attachedObject.transform.position - transform.position;
        // would be needed to remember the rotation at the beginning of the grab
        // objToHandRot = attachedObject.transform.eulerAngles - transform.eulerAngles;
    }

    private void ReleaseObject()
    {
        if (MD.modes[MD.activeMode].showTrashcan)
        {
            if (TrashCanScript.atomInCan && ctrlMaskName == "AtomLayer")
                // check that there isn't just one atom left, because this atom would have no temperature/force/velocity,
                // so it can't build a ham_lammps function
                if (SD.atomInfos.Count >= 3)
                    //DestroyAtom();
                    OTP.ExecuteOrder("Destroy Atom Nr " + attachedObject.GetComponent<AtomID>().ID);

            if (ctrlMaskName == "AtomLayer")
                // deactivate the trash can
                TrashCanScript.gameObject.SetActive(false);
        }

        // detach the attached object
        attachedObject = null;
    }
    
    private void ShowLaser(RaycastHit hit) 
    {
        // set the laserposition in the middle between the controller and the hitpoint
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        // rotate the laser
        laserTransform.LookAt(hitPoint);
        // scale the vector
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }
}
