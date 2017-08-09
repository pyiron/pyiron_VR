using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of both controllers
public class LaserGrabber : MonoBehaviour
{
    [Header("Scene Data")]
    // the global settings of the program
    public ProgramSettings Settings;
    // the data about the structure
    private StructureData SD;
    // the properties about the resize
    private StructureResizer SR;
    // the gameobject of the structure
    public GameObject AtomStructure;
    // the script of the controller printer
    public InGamePrinter printer;

    [Header("Move Objects")]
    // the object the controller is currently colliding with
    private GameObject collidingObject;
    // the object which is currently attached to the controller
    private GameObject attachedObject;
    // the Vector between the controller position and the grabbed object position
    private Vector3 objToHandPos;
    // the Vector between the controller rotation and the grabbed object rotation
    // private Vector3 objToHandRot; // might be needed to rotate the atom
    // the vector between the positions of the boundingbox and it's parent
    private Transform boundingbox;

    [Header("Masks")] // the controller mask and it's name
    public LayerMask ctrlMask;
    private string ctrlMaskName;

    [Header("Laser")]
    // the prefab for the laser
    public GameObject LaserPrefab;
    // the instance of the laser in the game
    private GameObject laser;
    // the transform of the laser gameobject
    private Transform laserTransform;
    // the hitpoint where the laser met an object
    private Vector3 hitPoint;
    // the length of the laser
    private float laserLength;
    // the max distance when the laser still detects an object to attach
    private int laserMaxDistance = 100;

    [Header("Resize")]
    // shows, which of the controllers currently allows to resize the structure
    public GameObject resizeableRect;
    // the state of the other controller, needed to look if both hairtriggers are pressed, so that the structure should be resized
    public bool readyForResize;

    [Header("Controller")]
    // the start touch point from when the player lays his finger on the touchpad
    private Vector2 startTouchPoint;
    // the point where the player currently touches the touchpad
    private Vector2 currentTouch;
    // the object of the controller
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
        // get the data of the controller
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        // initialize the laser
        InitLaser();

        // set the variable to the name of the mask
        ctrlMaskName = Settings.getLayerName(ctrlMask);
        // get the script StructureData from AtomStructure
        SD = AtomStructure.GetComponent<StructureData>();
        // get the script StructureResizer from AtomStructure
        SR = AtomStructure.GetComponent<StructureResizer>();

        // get the transform of the boundingbox
        foreach (Transform tr in AtomStructure.GetComponentsInChildren<Transform>())
            if (tr.name == "Boundingbox(Clone)")
                boundingbox = tr;
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
        // check all the input of the controller and fullfill the following actions
        //CheckControllerInput();

        // move the grabbed object
        if (attachedObject)
            MoveGrabbedObject();

        if (readyForResize)
            resizeableRect.SetActive(true);
        else
            resizeableRect.SetActive(false);
    }

    public void CheckHairTrigger()
    {
        // if the controller gets pressed, it should try to attach an object to it
        if (Controller.GetHairTriggerDown())
        {
            // if an object is colliding with the controller, it should be attached
            if (collidingObject)
            {
                AttachObject(collidingObject);
                // set the controller ready for size, if it grabs the boundingbox
                if (ctrlMaskName == "BoundingboxLayer")
                    if (otherCtrl.GetComponent<LaserGrabber>().readyForResize)
                    {
                        InitResize();
                    }
                    else
                        readyForResize = true;
            }
            // test if the other controller is ready for a resize
            else if (otherCtrl.GetComponent<LaserGrabber>().readyForResize)
                // init the resize, because now are both controllers ready
                if (ctrlMaskName == "AtomLayer")
                    InitResize();
                else;
            else
                // send out a raycast to detect objects in front of the controller
                SendRaycast();
        }

        // check if the player released the button
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // set the state of the controller to not ready for resizeStructure, if it isn't already
            if (readyForResize)
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
    }

    public void CheckTouchpad()
    {
        // just do anything if the laser is active, because else the touchpad has no function (yet)
        if (laser.activeSelf)
        {
            // mark the start touchpoint
            if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                startTouchPoint = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            }

            // scale the laser
            if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
            {
                currentTouch = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
                ScaleLaser(currentTouch.y - startTouchPoint.y);

                // set the max distance the laser should have to exist and not grab the object
                float minLaserLength;
                if (ctrlMaskName == "BoundingboxLayer")
                    minLaserLength = 0;
                else
                    // set the distance to the width of the atom, so that the atom is in front of the controller, and not in it
                    minLaserLength = attachedObject.transform.localScale.x * Settings.size / 2;

                // if the laserlength is changed to a value less than the minimum distance, the attached object is going to be grabbed
                if (laserLength + currentTouch.y - startTouchPoint.y <= minLaserLength)
                {
                    AttachObject(attachedObject);
                    laser.SetActive(false);
                    if (ctrlMaskName == "BoundingboxLayer")
                        readyForResize = true;
                }
            }

            // scale the laser to the new laserlength
            if (Controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
            {
                laserLength += currentTouch.y - startTouchPoint.y;
                ScaleLaser();
            }
        }
    }

    // checks if the laser hits an object, which it should hit (an atom or a structure)
    private void SendRaycast()
    {
        // check that there isn't an object in range to grab
        if (collidingObject == null)
        {
            RaycastHit hit;

            if (!attachedObject)
                // send out a raycast to detect if there is an object in front of the laser 
                if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, laserMaxDistance, ctrlMask))
                {
                    laser.SetActive(true);
                    hitPoint = hit.point;
                    ShowLaser(hit);
                    AttachObject(hit.transform.gameObject);
                }
                else
                    if (ctrlMaskName == "AtomLayer")
                        readyForResize = true;
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
        if (collidingObject || !col.GetComponent<Rigidbody>())
            return;

        // checks if the colliding object is of interrest to the controller
        if (ctrlMaskName == LayerMask.LayerToName(col.gameObject.layer))
            collidingObject = col.gameObject;
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
        SR.InitResize();
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
        // would allow the object to fly away with the velocity and angularvelocity it has when it gets detached
        //objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
        //objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;

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
