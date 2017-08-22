using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public ModeData MD;
    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;

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

    [Header("Masks")] // the controller mask and it's name
    public LayerMask ctrlMask;
    public string ctrlMaskName;

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

    [Header("Destroy Atom")]

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

    [Header("Transmittion")]
    // the filename of the file which will send orders from unity to pyiron
    private string fileName = "orders";

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
        try {
            // find the trash can
            TrashCanScript = GameObject.Find("MyObjects/Trash Can").GetComponent<TrashCan>();
        }
        catch { TrashCanScript = otherCtrl.GetComponent<LaserGrabber>().TrashCanScript; }
        // get the data of the controller
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        // get the reference to the programm which handles the execution of python
        PE = Settings.GetComponent<PythonExecuter>();
    }

    void Start()
    {
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

        textSize = textSize / Settings.textResolution * 10;
        InfoText.transform.localScale = Vector3.one * textSize;
        InfoText.fontSize = (int)Settings.textResolution;

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
        // check all the input of the controller and fullfill the following actions
        //CheckControllerInput();

        if (MD.modeNr == 0)
        {
            // move the grabbed object
            if (attachedObject)
            {
                MoveGrabbedObject();
                TrashCanScript.UpdateTrashCan(attachedObject);
                if (ctrlMaskName.Contains("Atom"))
                    printer.Ctrl_print(attachedObject.GetComponent<AtomID>().ID.ToString(), 101);
            }
        }
        else
        {
            if (Controller.GetHairTrigger())
            {
                SendRaycast();
                if (MD.modeNr == 1)
                    if (laser.activeSelf || collidingObject)
                    {
                        // set the Infotext to active and edit it 
                        InfoText.gameObject.SetActive(true);
                        // let the InfoText always look in the direction of the player
                        Settings.Face_Player(InfoText.gameObject);
                        //InfoText.transform.eulerAngles = new Vector3(0, HeadTransform.eulerAngles.y, 0);
                        if (ctrlMaskName.Contains("AtomLayer"))
                        {
                            if (attachedObject)
                            {
                                // set the info text to the top of the atom
                                InfoText.transform.position = attachedObject.transform.position // + Vector3.up * 0.1f
                                     + Vector3.up * attachedObject.transform.localScale[0]/2 * Settings.size;
                                InfoText.text = SD.atomInfos[attachedObject.GetComponent<AtomID>().ID].m_type;
                                if (PythonExecuter.extendedData)
                                    InfoText.text += "\nForce: "
                                        + PythonExecuter.structureForce[attachedObject.GetComponent<AtomID>().ID];
                                // might be needed so that the text will stand above the atom
                                //InfoText.GetComponent<TextMesh>().text += "\n";
                            }
                            else
                            {
                                printer.Ctrl_print("No attached Object!", rightCtrl: false);
                                print("No attached Object!");
                            }
                        }
                        else
                        {
                            // set the info text to the top of the boundingbox
                            InfoText.transform.position = boundingbox.transform.position + Vector3.up * 0.1f
                                + Vector3.up * boundingbox.transform.localScale[0]/2 * Settings.size;
                            InfoText.text = SD.structureName;
                            // InfoText.text += "\nForce: " + PythonExecuter.structureForce;
                            //might be needed so that the text will stand above the boundingbox
                            //InfoText.GetComponent<TextMesh>().text += "\n";
                        }
                    }
                    else
                        InfoText.gameObject.SetActive(false);
            }
            else
                InfoText.gameObject.SetActive(false);
        }

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
            if (MD.modeNr == 0)
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
        }

        
        // check if the player released the button
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // check that the move mode is currently active
            if (MD.modeNr == 0)
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
    }

    public void CheckTouchpad()
    {
        if (MD.modeNr == 0)
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
        else if (MD.modeNr == 2)
            if (ctrlMaskName.Contains("BoundingboxLayer"))
                if (collidingObject || laser.activeSelf)
                    if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
                        if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y > 0)
                            if (Settings.transMode == "file")
                                WriteOrder("self.duplicate(2)");
                            else
                                PE.send_order("self.duplicate(2)");
                        else
                            if (SD.atomInfos.Count * 0.5 * 0.5 * 0.5 >= 1)
                                if (Settings.transMode == "file")
                                    WriteOrder("self.duplicate(0.5)");
                                else
                                    PE.send_order("self.duplicate(0.5)");
    }

    private void WriteOrder(string order)
    {
        StreamWriter sw = new StreamWriter(Settings.GetFilePath(fileName:fileName));
        using (sw)
        {
            sw.WriteLine(order);
        }
        printer.Ctrl_print("order", 20);
    }

    // checks if the laser hits an object, which it should hit (an atom or a structure)
    private void SendRaycast()
    {
        // check that there isn't an object in range to grab
        if (collidingObject == null)
        {
            RaycastHit hit;

            if (!attachedObject || MD.modeNr != 0)
                // send out a raycast to detect if there is an object in front of the laser 
                if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, laserMaxDistance, ctrlMask))
                {
                    // print(ctrlMaskName);
                    laser.SetActive(true);
                    hitPoint = hit.point;
                    ShowLaser(hit);
                    if (MD.modeNr == 0)
                        AttachObject(hit.transform.gameObject);
                    else
                        attachedObject = hit.transform.gameObject;
                }
                else if (MD.modeNr == 0)
                    if (ctrlMaskName == "AtomLayer")
                        readyForResize = true;
                    else;
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
        if (collidingObject) // || !col.GetComponent<Rigidbody>())
            return;

        // checks if the colliding object is of interrest to the controller
        if (ctrlMaskName == LayerMask.LayerToName(col.gameObject.layer))
        {
            // set the colliding object
            collidingObject = col.gameObject;
            // disable the laser if the controller is colliding when not in mode 0
            if (MD.modeNr != 0)
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
            // spawn the trashcan
            TrashCanScript.ActivateCan();
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

        if (TrashCanScript.atomInCan && ctrlMaskName == "AtomLayer")
        {
            // check that there isn't just one atom left, because this atom would have no temperature/force/velocity,
            // so it can't build a ham_lammps function
            if (SD.atomInfos.Count >= 3)
                DestroyAtom();
        }
        // deactivate the trash can
        TrashCanScript.gameObject.SetActive(false);
        // detach the attached object
        attachedObject = null;
    }

    private void DestroyAtom()
    {
        // send Python/{yiron the order to destroy the atom
        PE.send_order("self.destroy_atom(" + attachedObject.GetComponent<AtomID>().ID + ")");
        // delete the atom and send python/pyiron that the atom should be excluded in the structure
        SD.waitForDestroyedAtom = true;
        print(attachedObject.GetComponent<AtomID>().ID);
        // remove the atom in the list of the properties of each atom
        SD.atomInfos.RemoveAt(attachedObject.GetComponent<AtomID>().ID);
        // remove the atom in the list which stores the data how the player has removed each atom
        SD.atomCtrlPos.RemoveAt(attachedObject.GetComponent<AtomID>().ID);
        // destroy the gameobject of the destroyed atom. This way, importStructure won't destroy all atoms and load them new
        Destroy(attachedObject);
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
