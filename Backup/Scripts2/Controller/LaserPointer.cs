using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {
    public Transform cameraRigTransform;
    private Transform teleportReticleTransform;
    public Transform headTransform;
    public LayerMask teleportMask;
    private bool canGrab;
    private SteamVR_TrackedObject trackedObj;
    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool activeLaser = false; //  ight be needed to know when the laser has to be moved
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Use this for initialization
    void Start () {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        laser.SetActive(false);
    }

    private void ShowLaser(RaycastHit hit)
    {
        //laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }

    // Update is called once per frame
    void Update () {
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        //if (Controller.GetHairTriggerDown())
        {
            RaycastHit hit;

            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100, teleportMask))
            {
                laser.SetActive(true);
                hitPoint = hit.point;
                ShowLaser(hit);
                print("yay");
                canGrab = true;
            }
            else
                laser.SetActive(false);
        }
        
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && canGrab)
        //if (Controller.GetHairTriggerUp() && canGrab)
        {
            // Teleport();
            laser.SetActive(false);
        }
    }

    /*
    private void grab()
    {
        canGrab = false;
        reticle.SetActive(false);
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        cameraRigTransform.position = hitPoint + difference;
    }
    */
}
