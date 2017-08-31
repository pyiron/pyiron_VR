using UnityEngine;

// component of Trash Can
public class TrashCan : MonoBehaviour
{
    // the global settings of the program
    private ProgramSettings Settings;
    // the upper part of the trashcan
    private GameObject TrashCanTop;
    // the Transform of the Headset
    private Transform HeadTransform;

    // the distance between the upper part of the trashbin and the main part
    Vector3 topToCan;
    // shows whether the atom is currently in the can or not
    public bool atomInCan;

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

        // scale the trashcan size according to the player given global size multiplicator
        transform.localScale = Vector3.one * Settings.size;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTrashCan(GameObject attachedObject)
    {
        topToCan = gameObject.transform.position - TrashCanTop.transform.position;
        float atomToCanDist = (attachedObject.transform.position - gameObject.transform.position).magnitude;
        if (atomToCanDist < gameObject.transform.localScale.x / 2
            + attachedObject.transform.localScale.x * Settings.size)  // / Settings.size
        {
            MoveTopToCan(topToCan);
            // show that the atom is currently in the can
            atomInCan = true;
        }
        else
        {
            if (atomToCanDist < gameObject.transform.localScale.x * 5)  // / Settings.size
                if ((topToCan - Vector3.up * topToCan.y).magnitude < gameObject.transform.localScale.x * 2.5)
                    MoveTopAway();
                else;
            else
                MoveTopToCan(topToCan);
            // show that the atom is currently not in the can
            atomInCan = false;
        }
    }

    private void MoveTopAway()
    {
        TrashCanTop.transform.position += Vector3.right * Settings.size * Time.deltaTime * 4;
    }

    private void MoveTopToCan(Vector3 topToCan)
    {
        TrashCanTop.transform.position += new Vector3(topToCan.x, 0, topToCan.z) * Time.deltaTime * 4;
    }

    public void ActivateCan()
    {
        // show the trashbin when holding a single atom
        gameObject.SetActive(true);
        // set the size of the can to global size
        gameObject.transform.localScale = Vector3.one * Settings.size;
        // close the trash can
        TrashCanTop.transform.position += new Vector3(topToCan.x, 0, topToCan.z);
        // move the trashbin to the place it should be
        Vector3 newBinPosition = Vector3.zero;
        newBinPosition.x += Mathf.Sin(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI - Mathf.PI / 4);
        newBinPosition.z += Mathf.Cos(HeadTransform.eulerAngles.y / 360 * 2 * Mathf.PI - Mathf.PI / 4);
        gameObject.transform.position = HeadTransform.position + (newBinPosition * 5f + Vector3.down / 0.3f) * Settings.size;
    }
}
