using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Hourglass
// Handles the actions of the Hourglass, which shows when the structure is currently loading
public class Hourglass : MonoBehaviour
{
    public static Hourglass inst;
    // the animationController of the Hourglass
    private Animator anim;
    // the Transform of the Headset
    private Transform HeadTransform;
    // the Script of the HourglassRotator
    private HourglassActivator HA;

    private void Awake()
    {
        inst = this;
        // get the reference to the animationController of the Hourglass
        anim = gameObject.GetComponent<Animator>();
        // get the reference to the transform of the headset
        HeadTransform = GameObject.Find("[CameraRig]/Camera (head)").transform;
        // 
        HA = GameObject.Find("AtomStructure/HourglassRotator").GetComponent<HourglassActivator>();
    }

    void Start()
    {
        // scale the hourglass according to the global size
        transform.localScale = Vector3.one * ProgramSettings.size;
        // let the hourglass face the player
        transform.parent.eulerAngles = Vector3.up * HeadTransform.eulerAngles.y;
    }

    void Update()
    {

    }

    public void ActivateHourglass(bool active)
    {
        // activate or deactivate the Hourglass Gameobject
        gameObject.SetActive(active);
        // start or stop the Animator of the Hourglass
        anim.enabled = active;
        if (active)
        {
            // start the rotation animation
            anim.Play("Rotate");
            HA.ResetTimer();
        }
    }
}
