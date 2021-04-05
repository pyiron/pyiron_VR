using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using System.IO;
using HTC.UnityPlugin.Vive;
using UnityEditor;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("LaserPrefab")] [Header("Laser")]
    // the prefab for the laser
    public GameObject laserPrefab;
    // the instance of the laser in the game
    public GameObject laser;
    // the length of the laser
    private float _laserLength;
    // the distance between the controller and the attached object
    private float _distToAttachedObject;
    // the max distance when the laser still detects an object to attach
    private static int laserMaxDistance = 100;

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
        }
        else
        {
            ctrlLayer = Layer.Structure;
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
            if (tr.name == "Boundingbox(Clone)") // TODO: returns null
                boundingbox = tr;

        textSize = textSize / ProgramSettings.textResolution * 10;
        // Todo: if info is introduced again, it should not be handled in this script
        //InfoText.transform.localScale = Vector3.one * textSize;
        //InfoText.fontSize = (int)ProgramSettings.textResolution;

        // deactivate the trashcan
        TrashCan.inst.SetState(false);
    }

    private void InitLaser()
    {
        // create an instance of the laser
        laser = Instantiate(laserPrefab);
        
        // set the controller as the parent of the laser
        laser.transform.parent = gameObject.transform;
    }

    void Update()
    {
        if (ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer")
        {
            // move the grabbed object
            if (attachedObject)
            {
                MoveGrabbedObject();
                if (ctrlMaskName.Contains("Atom"))
                {
                    // update the trashcan, if it is shown in the current mode
                    if (ModeController.currentMode.showTrashcan)
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
        if (ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer")
        {
            // test if the other controller is ready for a resize
            if (otherLg.readyForResize)
            {
                // init the resize, because now are both controllers ready
                if (ctrlMaskName == "AtomLayer")
                {
                    InitResize();
                }
            }
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
        if (!ModeController.currentMode.playerCanMoveAtoms)
        {
            SendRaycast();
        }
        else
            InfoText.gameObject.SetActive(false);
    }

    public void HairTriggerUp()
    {
        // check that the move mode is currently active
        if (ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer")
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
                if (ctrlMaskName == "AtomLayer" && (!ModeController.currentMode.showTrashcan || !TrashCan.inst.atomInCan))
                {
                    // tell Python the new position
                    Structure.Inst.OnAtomPositionChanged(attachedObject);
                }
                
                if (ModeController.currentMode.showTrashcan)
                {
                    if (ctrlMaskName == "AtomLayer")
                    {
                        // check that there are atoms left, which would cause pyiron to fail
                        // so it can't build a ham_lammps function)
                        if (TrashCan.inst.atomInCan && Structure.Inst.AtomAmount() >= 1)
                        {
                            Structure.Inst.OnAtomDeleted(attachedObject);
                            //DestroyAtom();
                            //OrdersToPython.inst.ExecuteOrder(
                            //    "Destroy Atom Nr " + attachedObject.GetComponent<AtomID>().ID);
                        }

                        // deactivate the trash can
                        TrashCan.inst.gameObject.SetActive(false);
                    }
                }
                // detach the attached object
                attachedObject = null;
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
        return ((ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer") && laser.activeSelf && !Thermometer.laserOnThermometer);
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
            // TODO: Use laser of VIU
            // scale the laser
            currentTouch = touchPos;
            float modification;
            if (InputManager.DeviceHasJoystick)
            {
                modification = currentTouch.y * Time.deltaTime;
                // print(currentTouch.y); joystick pos not working on quest at the moment
                _laserLength += modification;
                ScaleLaser();
            }
            else
            {
                modification = currentTouch.y - startTouchPoint.y;
                ScaleLaser(modification);
            }

            // set the max distance the laser should have to exist and not grab the object
            float minLaserLength;
            if (ctrlMaskName == "BoundingboxLayer")
                minLaserLength = 0;
            else
                // set the distance to the width of the atom, so that the atom is in front of the controller, and not in it
                minLaserLength = attachedObject.transform.localScale.x * ProgramSettings.size / 2;

            // if the laser length is changed to a value less than the minimum distance, the attached object is going to be grabbed
            //if (_laserLength + modification < minLaserLength)
            if (_laserLength < minLaserLength)
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
            if (Application.platform != RuntimePlatform.Android)
            {
                // scale the laser to the new laser length
                _laserLength += currentTouch.y - startTouchPoint.y;
                ScaleLaser();
            }
        }
    }

    // initialize the resize if both controllers are ready, else just set the controller to ready
    private void SetControllerToReady()
    {
        if (otherLg.readyForResize)
            InitResize();
        else
            readyForResize = true;
    }

    // checks if the laser hits an object, which it should hit (an atom or a structure)
    private void SendRaycast()
    {
        // check that there isn't an object in range to grab
        if (true) //(collidingObject == null)
        {
            if (!attachedObject)// || !ModeController.currentMode.playerCanMoveAtoms)
                // send out a raycast to detect if there is an object in front of the laser 
                if (Physics.Raycast(transform.position, transform.forward, out var hit, laserMaxDistance, ctrlMask))
                {
                    GameObject hitObject = hit.transform.gameObject;
                    if (!hitObject.name.Contains("Thermometer")) {
                        laser.SetActive(true);
                        //hitPoint = hit.point;
                        ShowLaser(hit);

                        if (!Thermometer.laserOnThermometer)
                            if (ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer")
                                AttachObject(hitObject);
                            else
                                attachedObject = hitObject;
                    }
                }
                // show that the controller is ready to resize the structure, if it is the AtomLayer controller
                else if (ModeController.currentMode.playerCanResizeAtoms)
                {
                    if (ctrlMaskName == "AtomLayer")
                    {
                        readyForResize = true;
                    }
                }
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
        // the location after the object got moved
        Vector3 newPos;
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
            // (because the laser already disables itself in this mode)
            if (!ModeController.currentMode.playerCanMoveAtoms || ctrlMaskName == "BoundingboxLayer")
                laser.SetActive(false);
        }
    }
    
    #region Trigger

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
    
    #endregion

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
            (laser.transform.position - transform.position) * (_laserLength + modification) / laserSize.z;
        laserSize.z = _laserLength + modification;
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
            _laserLength = (boundingbox.position - transform.position).magnitude;
        }
        else if (ctrlMaskName == "AtomLayer")
        {
            // set the atom as the attached object
            attachedObject = grabAbleObject;
            // the laser length has to be set to the length from the controller to the atom, because it's attached to it's middle point, 
            //and the object should be at the same distance before and after the start of the grab
            _laserLength = (attachedObject.transform.position - transform.position).magnitude;
            // spawn the trashcan
            TrashCan.inst.ActivateCan();

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
