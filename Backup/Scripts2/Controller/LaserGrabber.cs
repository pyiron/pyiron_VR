using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGrabber : MonoBehaviour
{
    private SteamVR_TrackedObject trackedObj;
    private GameObject collidingObject;
    private GameObject objectInHand;
    private Vector3 objToHandPos;
    private Vector3 objToHandRot;
    public Transform cameraRigTransform;
    private Transform teleportReticleTransform;
    public Transform headTransform;
    public LayerMask teleportMask;
    private bool canGrab;
    public GameObject laserPrefab;
    private GameObject laser;
    private GameObject grabAbleObject;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        laser.SetActive(false);
    }

    void Update()
    {
        if (Controller.GetHairTriggerDown())
        {
            if (collidingObject)
            {
                GrabObject(collidingObject);
            }
        }

        if (objectInHand)
        {
            objectInHand.transform.position = transform.position + objToHandPos;
            //objectInHand.transform.eulerAngles = -transform.eulerAngles + objToHandRot;
        }

        /*
        if (Controller.GetHairTriggerUp())
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }*/

        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger) && collidingObject == null)
        //if (Controller.GetHairTriggerDown())
        {
            RaycastHit hit;

            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100, teleportMask))
            {
                laser.SetActive(true);
                hitPoint = hit.point;
                ShowLaser(hit);
                canGrab = true;
                GrabObject(hit.transform.gameObject);
            }
            else
                laser.SetActive(false);
        }
        if (laser.activeSelf && collidingObject)
        {
            laser.SetActive(false);
            GrabObject(collidingObject);
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && canGrab)
        //if (Controller.GetHairTriggerUp() && canGrab)
        {
            // Teleport();
            laser.SetActive(false);
            if (objectInHand)
            {
                ReleaseObject();
            }
        }
    }

    private void SetCollidingObject(Collider col)
    {
        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }
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

    private void GrabObject(GameObject grabAbleObject)
    {
        objectInHand = grabAbleObject.transform.root.gameObject;
        objToHandPos = objectInHand.transform.position - transform.position;
        objToHandRot = objectInHand.transform.eulerAngles - transform.eulerAngles;
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
        objectInHand = null;
    }

    private void ShowLaser(RaycastHit hit)
    {
        //laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }
}
