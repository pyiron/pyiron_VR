using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGrabber : MonoBehaviour
{
    public ProgramSettings Settings; // get access to the Settings Script
    private StructureData SD;
    public GameObject AtomStructure;

    [Header("Move Objects")]
    private GameObject collidingObject;
    private GameObject attachedObject;
    // the Vector between the controller position and the grabbed object position
    private Vector3 objToHandPos;
    // the Vector between the controller rotation and the grabbed object rotation
    private Vector3 objToHandRot; // might be needed to rotate the atom
    private GameObject grabAbleObject;
    // the vector between the positions of the boundingbox and it's parent
    private Transform boundingbox;

    [Header("Masks")] // the controller mask and it's name
    public LayerMask ctrlMask;
    private string ctrlMaskName;

    [Header("Laser")]
    public GameObject LaserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private float laserLength = 0;

    [Header("Controller")]
    private Vector2 startTouchPoint;
    private Vector2 currentTouch;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        // get the data of the controller
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        // get the transform of the boundingbox
        foreach (Transform tr in AtomStructure.GetComponentsInChildren<Transform>())
            if (tr.name == "Boundingbox(Clone)")
                boundingbox = tr;
    }

    void Start()
    {
        initLaser();

        ctrlMaskName = Settings.getLayerName(ctrlMask);
        SD = AtomStructure.GetComponent<StructureData>();
    }

    private void initLaser()
    {
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
        //laserTransform.parent = boundingbox;
        laserTransform.parent = gameObject.transform;
    }

    void Update()
    {
        checkControllerInput();

        

        if (attachedObject)
            moveGrabbedObject();
    }

    private void checkControllerInput()
    {
        checkHairTrigger();
        checkTouchpad();
    }

    private void checkHairTrigger()
    {
        if (Controller.GetHairTriggerDown())
        {
            if (collidingObject)
                AttachObject(collidingObject);
            else
                sendRaycast();
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            laser.SetActive(false);
            if (attachedObject)
            {
                if (ctrlMaskName == "AtomLayer")
                {
                    // check the new extension of the structure
                    SD.searchMaxAndMin();
                    // set the boundingbox so that it encloses the structure
                    SD.updateBoundingbox();
                }
                ReleaseObject();
            }
        }
    }

    private void checkTouchpad()
    {
        if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            startTouchPoint = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
        }

        if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            currentTouch = Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            scaleLaser(currentTouch.y - startTouchPoint.y);
        }

        if (Controller.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            laserLength += currentTouch.y - startTouchPoint.y;
            scaleLaser();
        }
    }

    private void sendRaycast()
    {
        if (collidingObject == null)
        {
            RaycastHit hit;

            if (!attachedObject)
                if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100, ctrlMask))
                {
                    laser.SetActive(true);
                    hitPoint = hit.point;
                    ShowLaser(hit);
                    AttachObject(hit.transform.gameObject);
                }
        }
    }

    private void moveGrabbedObject()
    {
        if (laser.activeSelf)
        {
            if (ctrlMaskName == "BoundingboxLayer")
                // sets the position of the structure to (the end of the laser + the Vector between the structure and the boundingbox)
                attachedObject.transform.position = transform.position + (laser.transform.position - transform.position) * 2
                    + AtomStructure.transform.position - boundingbox.position;
            else
                // sets the position of the grabbed object to the end of the laser
                attachedObject.transform.position = transform.position + (laser.transform.position - transform.position) * 2;

            // would keep always the same side to the viewer:
            //  objectInHand.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + objToHandRot.y, 0);
        }
        else
        {
            attachedObject.transform.position = transform.position + objToHandPos;
        }
    }

    private void SetCollidingObject(Collider col)
    {
        if (col.gameObject.name == "Controller (left)" || col.gameObject.name == "Controller (right)")
            return;
        if (collidingObject || !col.GetComponent<Rigidbody>())
            return;

        if (ctrlMaskName == LayerMask.LayerToName(col.gameObject.layer))
            collidingObject = col.gameObject;
    }

    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
    }

    private void scaleLaser(float modification = 0)
    {
        Vector3 laserSize = laser.transform.localScale;
        laser.transform.position = transform.position + 
            (laser.transform.position - transform.position) * (laserLength + modification) / laserSize.z;
        laserSize.z = laserLength + modification;
        laser.transform.localScale = laserSize;
    }

    private void AttachObject(GameObject grabAbleObject)
    {
        if (ctrlMaskName == "BoundingboxLayer")
        {
            attachedObject = grabAbleObject.transform.root.gameObject;
            laserLength = (boundingbox.position - transform.position).magnitude;
            //laserLength = (attachedObject.transform.position - transform.position).magnitude;
        }
        else if (ctrlMaskName == "AtomLayer")
        {
            attachedObject = grabAbleObject;
            laserLength = (attachedObject.transform.position - transform.position).magnitude;
        }
        
        scaleLaser();
        objToHandPos = attachedObject.transform.position - transform.position;
        objToHandRot = attachedObject.transform.eulerAngles - transform.eulerAngles;
    }

    private void ReleaseObject()
    {
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            //objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            //objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
        attachedObject = null;
    }
    
    private void ShowLaser(RaycastHit hit)
    {
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }
}
