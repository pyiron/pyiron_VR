using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    // the global settings of the program
    private ProgramSettings Settings;
    // the upper part of the trashcan
    private GameObject TrashCanTop;
    // the Transform of the Headset
    private Transform HeadTransform;

    // the script of the controller printer
    public InGamePrinter printer;

    private void Awake()
    {
        // find the reference to the settings
        Settings = GameObject.Find("Settings").GetComponent<ProgramSettings>();
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (eye)/Camera (head)").transform;
        // find the upper part of the trash can
        TrashCanTop = GameObject.Find("MyObjects/Trash Can/Top");
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateTrashCan(GameObject attachedObject)  // make a few little functions for move away and move to bin!
    {
        // the distance between the upper part of the trashbin and the main part
        Vector3 topToCan = gameObject.transform.position - TrashCanTop.transform.position;
        float atomToCanDist = (attachedObject.transform.position - gameObject.transform.position).magnitude;
        printer.Ctrl_print(atomToCanDist.ToString(), 90);
        if (atomToCanDist < gameObject.transform.localScale.x
            + attachedObject.transform.localScale.x * Settings.size)  // / Settings.size
            MoveTopToCan(topToCan);
        else if (atomToCanDist < gameObject.transform.localScale.x * 5)  // / Settings.size
            if ((topToCan - Vector3.up * topToCan.y).magnitude < gameObject.transform.localScale.x * 3)
                MoveTopAway();
            else
                print(topToCan.magnitude + " and " + gameObject.transform.localScale.x * 2);
        else
        {
            MoveTopToCan(topToCan);
            printer.Ctrl_print("too far away", 90, false);
        }
    }

    private void MoveTopAway()
    {
        print("moved away");
        printer.Ctrl_print("moved away", 90, false);
        TrashCanTop.transform.position += Vector3.right * Settings.size * Time.deltaTime;
    }

    private void MoveTopToCan(Vector3 topToCan)
    {
        printer.Ctrl_print("movedToCan", 90, false);
        print("movedToCan");
        TrashCanTop.transform.position += new Vector3(topToCan.x, 0, topToCan.z) * Time.deltaTime;
    }

    public void ActivateCan()
    {
        // show the trashbin when holding a single atom
        gameObject.SetActive(true);
        // move the trashbin to the place it should be
        Vector3 newBinPosition = Vector3.zero;
        newBinPosition.x += Mathf.Sin(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI - Mathf.PI / 2);
        newBinPosition.z += Mathf.Cos(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI - Mathf.PI / 2);
        gameObject.transform.position = HeadTransform.position + newBinPosition * 1.5f + Vector3.down;
    }
}
