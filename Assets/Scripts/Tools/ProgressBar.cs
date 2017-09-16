using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of MyObjects
// Activates the Progressbar when the current frame and the amount of frames in the animation is known and updates the bar
public class ProgressBar : MonoBehaviour {
    // the reference to the PythonExecuter
    private PythonExecuter PE;
    // the reference to the ProgressBar
    private GameObject ProgressBarObject;
    // the reference to the animationController of the ProgressBar
    private Animator anim;
    // the reference to the TextMeshes of the ProgressBar
    private TextMesh[] TextMeshes;

    private static readonly Dictionary<int, string> animationSpeedLabels = new Dictionary<int, string>()
    {
        {0, "-2x" },
        {1, "-1x" },
        {2, "-0.5x" },
        {3, "0.5x" },
        {4, "1x" },
        {5, "2x" }
    };

	void Awake () {
        // the reference to the PythonExecuter
        PE = GameObject.Find("Settings").GetComponent<PythonExecuter>();
        // get the reference to the ProgressBar
        ProgressBarObject = GameObject.Find("MyObjects/ProgressBar");
        // get the reference to the animationController of the ProgressBar
        anim = ProgressBarObject.GetComponent<Animator>();
        // get the reference to the TextMeshes of the ProgressBar
        TextMeshes = ProgressBarObject.GetComponentsInChildren<TextMesh>();
        // set the TextMeshes to the right size and position
        for (int i = 0; i < TextMeshes.Length; i++)
        {
            TextMeshes[i].transform.localScale = Vector3.one * ProgramSettings.size;
            TextMeshes[i].transform.localPosition += Vector3.up * (i + 1) / 2;
        }

        // deactivate the ProgressBar at the beginning because the current frame and the amount of frames is not yet known
        ProgressBarObject.SetActive(false);
    }
	
	void Update () {
        // if the ProgressBar is activated while it should be deactivated deaktivate  it and vice versa
        if ((PythonExecuter.frame != -1 && PythonExecuter.frameAmount > 0) != ProgressBarObject.activeSelf)
            ProgressBarObject.SetActive(!ProgressBarObject.activeSelf);

        if (ProgressBarObject.activeSelf)
        {
            // update the progress of the ProgressBar
            anim.SetFloat("Progress", 1f * PythonExecuter.frame / PythonExecuter.frameAmount);
            foreach (TextMesh TM in TextMeshes)
                // update the text which shows the progress
                if (TM.name.Contains("Progress"))
                    TM.text = PythonExecuter.frame + " / " + PythonExecuter.frameAmount;
                // update the text which shows how fast the animation is being played
                else if (TM.name.Contains("AnimationSpeed"))
                {
                    // if the text is activated while it should be deactivated deaktivate it and vice versa
                    if (OrdersToPython.pythonRunsAnim != TM.gameObject.activeSelf)
                        TM.gameObject.SetActive(OrdersToPython.pythonRunsAnim);

                    // update the text of how fast the anim speed is
                    if (OrdersToPython.pythonRunsAnim)
                        TM.text = animationSpeedLabels[PE.pythonsAnimSpeed];
                }
        }
    }
}
