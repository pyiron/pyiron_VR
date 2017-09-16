using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of MyObjects
// Activates the Progressbar when the current frame and the amount of frames in the animation is known and updates the bar
public class ProgressBar : MonoBehaviour {
    // the reference to the ProgressBar
    private GameObject ProgressBarObject;
    // the reference to the animationController of the ProgressBar
    private Animator anim;

	void Awake () {
		// the reference to the ProgressBar
        ProgressBarObject = GameObject.Find("MyObjects/ProgressBar");
        // the reference to the animationController of the ProgressBar
        anim = ProgressBarObject.GetComponent<Animator>();
        // deactivate the ProgressBar at the beginning because the current frame and the amount of frames is not yet known
        ProgressBarObject.SetActive(false);
}
	
	// Update is called once per frame
	void Update () {
        /*if (PythonExecuter.frame != -1 && PythonExecuter.frameAmount > 0)
        {
            // activate the ProgressBar if it isn't activated yet
            if (!ProgressBarObject.activeSelf)
                ProgressBarObject.SetActive(true);
        }
        else
            // deactivate the ProgressBar if it isn't deactivated yet
            if (ProgressBarObject.activeSelf)
                ProgressBarObject.SetActive(true);*/

        // if the ProgressBar is activated while it should be deactivated deaktivate  it and vice versa
        if ((PythonExecuter.frame != -1 && PythonExecuter.frameAmount > 0) != ProgressBarObject.activeSelf)
            ProgressBarObject.SetActive(!ProgressBarObject.activeSelf);

        if (ProgressBarObject.activeSelf)
            // update the progress of the ProgressBar
            anim.SetFloat("Progress", 1f * PythonExecuter.frame / PythonExecuter.frameAmount);
    }
}
