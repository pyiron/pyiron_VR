using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of Hourglass
// Handles the actions of the Hourglass, which shows when the structure is currently loading
public class Hourglass : MonoBehaviour {
    // the animationController of the Hourglass
    private Animator anim;

    private void Awake()
    {
        // get the reference to the animationController of the Hourglass
        anim = gameObject.GetComponent<Animator>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ActivateHourglass(bool active)
    {
        gameObject.SetActive(active);
        anim.enabled = active;
        if (active)
            // start the rotation animation
            anim.Play("Rotate"); 
    }
}
