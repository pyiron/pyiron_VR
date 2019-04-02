using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using HTC.UnityPlugin.Vive;
using UnityEditor;

// Component of both controllers
public class LaserGrabber : MonoBehaviour
{
    // reference to both instances
    public static LaserGrabber[] instances = new LaserGrabber[2];
    public static ControllerSymbols[] controllerSymbols = new ControllerSymbols[2];
    
    [Header("Scene Data")]
    // the gameobject of the structure
    public GameObject AtomStructure;
    // the script of the controller printer
    //public InGamePrinter printer;

    [Header("Move Objects")]
    // the object the controller is currently colliding with
    private GameObject collidingObject;
    // the object which is currently attached to the controller
    public GameObject attachedObject;
    // the Vector between the controller position and the grabbed object position
    private Vector3 objToHandPos;
    // the Vector between the controller rotation and the grabbed object rotation
    public Transform boundingbox;

    [Header("Laser")]
    // the prefab for the laser
    public GameObject LaserPrefab;
    // the instance of the laser in the game
    public GameObject laser;
    // the length of the laser
    private float laserLength;
    // the max distance when the laser still detects an object to attach
    private static int laserMaxDistance = 100;

    [Header("Change Animation")]
    // shows whether it is the first time an animation should be played,
    // so that the python program knows whether to load a new animation or not
    public static bool firstAnimStart = true;
    // shows if the current lammps is a calc_md or calc_minimize
    private static bool lammpsIsMd;
    // shows whether the positions of the atoms are still the same es they were when the last ham_lammps was created
    private static bool positionsHaveChanged;
    // a timer, which counts when the program should go a frame forward or backwards, when keeping the one frame forward button pressed
    private float moveOneFrameTimer = -1;
    // the time until the program should go a frame forward or backwards, when keeping the "one frame forward button" pressed
    private float timeUntilMoveOneFrame = 0.5f;

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
    // the controller mask and it's name
    public LayerMask ctrlMask;
    public string ctrlMaskName;
    public Layer ctrlLayer;

    // the other controller
    private LaserGrabber otherLg;

    void Awake()
    {
        if (ctrlMaskName.Contains("Atom"))
        {
            ctrlLayer = Layer.Atom;
            print("Inited LG Atom");
        }
        else
        {
            ctrlLayer = Layer.Structure;
            print("Inited LG");
        }
        controllerSymbols[(int) ctrlLayer] = GetComponent<ControllerSymbols>();
        instances[(int) ctrlLayer] = this;

        InitLaser();
    }

    void Start()
    {
        otherLg = instances[((int) ctrlLayer + 1) % 2];
        // set the variable to the name of the mask
        ctrlMaskName = ProgramSettings.GetLayerName(ctrlMask);
        // get the reference to the other controller

        // get the transform of the boundingbox
        foreach (Transform tr in AtomStructure.GetComponentsInChildren<Transform>())
            if (tr.name == "Boundingbox(Clone)")
                boundingbox = tr;

        textSize = textSize / ProgramSettings.textResolution * 10;
        InfoText.transform.localScale = Vector3.one * textSize;
        InfoText.fontSize = (int)ProgramSettings.textResolution;

        // deactivate the trashcan
        TrashCan.inst.SetState(false);
    }

    private void InitLaser()
    {
        // create an instance of the laser
        laser = Instantiate(LaserPrefab);
        
        // set the controller as the parent of the laser
        laser.transform.parent = gameObject.transform;
    }

    void Update()
    {
        if (ModeData.currentMode.playerCanMoveAtoms)
        {
            // move the grabbed object
            if (attachedObject)
            {
                MoveGrabbedObject();
                if (ctrlMaskName.Contains("Atom"))
                {
                    // update the trashcan, if it is shown in the current mode
                    if (ModeData.currentMode.showTrashcan)
                        TrashCan.inst.UpdateTrashCan(attachedObject);
                }
                else
                    // update the rotation of the Hourglass
                    Hourglass.inst.transform.parent.eulerAngles = Vector3.up * transform.eulerAngles.y;
            }
        }

        // show the white rect if this controller is ready for a resize of the structure
        if (resizeableRect != null)
            resizeableRect.SetActive(readyForResize);
        else
        {
            //print("Resizable Rect should not be null!");
        }
    }

    public void HairTriggerDown()
    {
        // if the controller gets pressed, it should try to attach an object to it
        if (ModeData.currentMode.playerCanMoveAtoms)
        {
            // test if the other controller is ready for a resize
            if (otherLg.readyForResize)
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
    }

    public void WhileHairTriggerDown()
    {
        if (!ModeData.currentMode.playerCanMoveAtoms)
        {
            SendRaycast();
            if (ModeData.currentMode.showInfo)
                ShowInfo();
        }
        else
            InfoText.gameObject.SetActive(false);
    }

    private void ShowInfo()
    {
        if (laser.activeSelf || collidingObject)
        {
            // set the Info text to active and edit it 
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
                    InfoText.text = StructureData.atomInfos[atomId].m_type;
                    // test if the forces are known
                    if (!float.IsNaN(PythonExecuter.allForces[atomId][0]))
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
                InfoText.text = StructureData.structureName;
                InfoText.text += "\nAtoms: "
                        + StructureData.atomInfos.Count;
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
        // check that the move mode is currently active
        if (ModeData.currentMode.playerCanMoveAtoms)
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
                    StructureData.inst.SearchMaxAndMin();
                    // set the boundingbox so that it encloses the structure
                    StructureData.inst.UpdateBoundingbox();
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

        //if (ModeData.currentMode.showPossibleStructures)
        //    ChooseStructure.inst.HairTriggerUp(transform);
    }

    // show that the laser is currently active and it's possible in the current move to move atoms, and that the laser doesn't point on the thermometer
    private bool ScaleAbleLaser()
    {
        return (ModeData.currentMode.playerCanMoveAtoms && laser.activeSelf && !Thermometer.laserOnThermometer);
    }

    public void TouchpadTouchDown(Vector2 touchPos)
    {
        if (ScaleAbleLaser())
            // mark the start touch point
            startTouchPoint = touchPos;
    }

    public void WhileTouchpadTouchDown(Vector2 touchPos)
    {
        if (ScaleAbleLaser())
        {
            // scale the laser
            currentTouch = touchPos;
            ScaleLaser(currentTouch.y - startTouchPoint.y);

            // set the max distance the laser should have to exist and not grab the object
            float minLaserLength;
            if (ctrlMaskName == "BoundingboxLayer")
                minLaserLength = 0;
            else
                // set the distance to the width of the atom, so that the atom is in front of the controller, and not in it
                minLaserLength = attachedObject.transform.localScale.x * ProgramSettings.size / 2;

            // if the laser length is changed to a value less than the minimum distance, the attached object is going to be grabbed
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
            // scale the laser to the new laser length
            laserLength += currentTouch.y - startTouchPoint.y;
            ScaleLaser();
        }
    }

    public void TouchpadPressDown(Vector2 touchPos)
    {
        // look if an animation should be started or stopped
        if (ModeData.currentMode.showTemp || ModeData.currentMode.showRelaxation)
            // check that the player isn't currently trying to change the length of the laser
            if (!laser.activeSelf)
                ControllAnimation(touchPos);
    }

    public void WhileTouchpadPressDown(Vector2 touchPos)
    {
        if (moveOneFrameTimer >= 0)
        {
            moveOneFrameTimer += Time.deltaTime;
            if (moveOneFrameTimer >= timeUntilMoveOneFrame)
            {
                if (touchPos.x > 0)
                    AnimationController.move_one_frame(true);
                else
                    AnimationController.move_one_frame(false);
                moveOneFrameTimer = 0;
            }
        }
    }

    public void TouchpadPressUp()
    {
        moveOneFrameTimer = -1;
    }

    // initialize the resize if both controllers are ready, else just set the controller to ready
    private void SetControllerToReady()
    {
        if (otherLg.readyForResize)
            InitResize();
        else
            readyForResize = true;
    }

    private void ControllAnimation(Vector2 touchPos)
    {
        if (touchPos.x > 0.5)
            if (AnimationController.run_anim)
                AnimationController.ChangeAnimSpeed(1);
            else
            {
                LoadNewLammps();

                // go one frame forward
                AnimationController.move_one_frame(true);
                // show that the user pressed the button to go one step forward
                moveOneFrameTimer = 0;
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
                LoadNewLammps();

                // go one frame back
                AnimationController.move_one_frame(false);
                moveOneFrameTimer = 0;
            }
        else if (AnimationController.run_anim)
            AnimationController.RunAnim(false);
        else
        {
            LoadNewLammps();

            // tell Python to start sending the dataframes from the current ham_lammps
            AnimationController.RunAnim(true);
        }

        // update the symbols on on all active controllers
        UpdateSymbols();
    }

    // send an Order to Python that it should create a new ham_lammps
    public static void LoadNewLammps()
    {
        // remember that a new ham_lammps has to be loaded
        bool temperatureHasChanged = Thermometer.inst.SendTemp();

        // check if the positions of any atom has been changed since the last animation has been started
        if (positionsHaveChanged || lammpsIsMd != ModeData.currentMode.showTemp) // add this: || PythonExecuter.frame != 0    to let the program load a new anima if the frame has changed
        {
            // send the new positions to Python
            positionsHaveChanged = true;
            // send Python the new positions of all atoms
            OrdersToPython.SetNewPositions();
        }

        print("Loading Anim if " + firstAnimStart + " or " + temperatureHasChanged + " or " + positionsHaveChanged);
        // when loading the first animation, show Python that it's the first time, so that it can check if there is already a loaded ham_lammps
        if (firstAnimStart)
        {
            LoadNewLammps("self.create_new_lammps");// self.calculate");
            firstAnimStart = false;
            positionsHaveChanged = false;
        }
        // tell Python to create a new ham_lammps because the structure or it's temperature has changed
        else if (temperatureHasChanged || positionsHaveChanged)
        {
            LoadNewLammps("self.create_new_lammps");
            // remember that the ham_lammps is now according to the current structure
            positionsHaveChanged = false;
        }
        // load a new ham_lammps if the current ham_lammps is for md and the animation for minimize is needed or vice versa
        //else if (lammpsIsMd != MD.modes[MD.activeMode].showTemp)
        //    LoadNewLammps("self.create_new_lammps");
    }

    // update the symbols on all active controllers
    public void UpdateSymbols()
    {
        gameObject.GetComponent<ControllerSymbols>().SetSymbol();
        if (otherLg.gameObject.activeSelf)
            controllerSymbols[(int) otherLg.ctrlLayer].SetSymbol();
    }

    private static void LoadNewLammps(string loadOrder)
    {
        AnimationController.frame = 0;
        if (ModeData.currentMode.showTemp)
            PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.exec, loadOrder + "('md')");
        else if (ModeData.currentMode.showRelaxation)
            PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.exec, loadOrder + "('minimize')");
        AnimationController.waitForLoadedStruc = true;
        lammpsIsMd = ModeData.currentMode.showTemp;
    }

    // checks if the laser hits an object, which it should hit (an atom or a structure)
    private void SendRaycast()
    {
        // check that there isn't an object in range to grab
        if (true) //(collidingObject == null)
        {
            if (!attachedObject || !ModeData.currentMode.playerCanMoveAtoms)
                // send out a raycast to detect if there is an object in front of the laser 
                if (Physics.Raycast(transform.position, transform.forward, out var hit, laserMaxDistance, ctrlMask))
                {
                    GameObject hitObject = hit.transform.gameObject;
                    if (!hitObject.name.Contains("Thermometer")) {
                        laser.SetActive(true);
                        //hitPoint = hit.point;
                        ShowLaser(hit);

                        if (!Thermometer.laserOnThermometer)
                            if (ModeData.currentMode.playerCanMoveAtoms)
                                AttachObject(hitObject);
                            else
                                attachedObject = hitObject;
                    }
                }
                // show that the controller is ready to resize the structure, if it is the AtomLayer controller
                else if (ModeData.currentMode.playerCanResizeAtoms)
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
            }
            else
                // sets the position of the grabbed object to the end of the laser
                newPos = transform.position + (laser.transform.position - transform.position) * 2;

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
            StructureData.atomCtrlPos[attachedObject.GetComponent<AtomID>().ID] += newPos - oldPos;
        else if (ctrlMaskName == "BoundingboxLayer")
            // update the data how the structure has been moved by the player 
            StructureData.inst.structureCtrlPos += newPos - oldPos;
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
            if (!ModeData.currentMode.playerCanMoveAtoms)
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
        StructureResizer.inst.InitResize();
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

            // the laser length has to be set to the length from the controller to the boundingbox, because it's attached to it's middle point,
            // and the object should be at the same distance before and after the start of the grab
            laserLength = (boundingbox.position - transform.position).magnitude;
        }
        else if (ctrlMaskName == "AtomLayer")
        {
            // set the atom as the attached object
            attachedObject = grabAbleObject;
            // the laser length has to be set to the length from the controller to the atom, because it's attached to it's middle point, 
            //and the object should be at the same distance before and after the start of the grab
            laserLength = (attachedObject.transform.position - transform.position).magnitude;
            // spawn the trashcan
            TrashCan.inst.ActivateCan();

            positionsHaveChanged = true;
            // deactivate the animation
            AnimationController.RunAnim(false);
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
        if (ModeData.currentMode.showTrashcan)
        {
            if (TrashCan.inst.atomInCan && ctrlMaskName == "AtomLayer")
                // check that there isn't just one atom left, because this atom would have no temperature/force/velocity,
                // so it can't build a ham_lammps function
                if (StructureData.atomInfos.Count >= 3)
                    //DestroyAtom();
                    OrdersToPython.inst.ExecuteOrder("Destroy Atom Nr " + attachedObject.GetComponent<AtomID>().ID);

            if (ctrlMaskName == "AtomLayer")
                // deactivate the trash can
                TrashCan.inst.gameObject.SetActive(false);
        }

        // detach the attached object
        attachedObject = null;
    }
    
    public void ShowLaser(RaycastHit hit) 
    {
        // set the laserposition in the middle between the controller and the hitpoint
        laser.transform.position = Vector3.Lerp(transform.position, hit.point, .5f);
        // rotate the laser
        laser.transform.LookAt(hit.point);
        // scale the vector
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y,
            hit.distance);
    }
}

public enum Layer
{
    Structure, Atom
}
