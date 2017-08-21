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
    public Transform HeadTransform;

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
        Vector3 topToBinDistance = gameObject.transform.position - TrashCanTop.transform.position;
        if ((attachedObject.transform.position - gameObject.transform.position).magnitude <
            gameObject.transform.localScale.x)  // / Settings.size
            TrashCanTop.transform.position += new Vector3(topToBinDistance.x, 0, topToBinDistance.z) * Time.deltaTime;
        else if ((attachedObject.transform.position - gameObject.transform.position).magnitude <
            gameObject.transform.localScale.x * 5)  // / Settings.size
            if (topToBinDistance.magnitude < gameObject.transform.localScale.x)
                TrashCanTop.transform.position += Vector3.right * Settings.size * Time.deltaTime;
            else;
        else
            print("to far away!"); // the top should be moved over the bin here
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
