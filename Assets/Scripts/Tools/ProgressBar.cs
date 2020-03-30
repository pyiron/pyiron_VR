using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Component of MyObjects/ProgressBar
// Activates the Progressbar when the current frame and the amount of frames in the animation is known and updates the bar
public class ProgressBar : MonoBehaviour {
    // a reference to this Script
    public static ProgressBar Inst;
    
    // the reference to the animationController of the ProgressBar
    private Animator _anim;
    // the reference to the TextMeshes of the ProgressBar
    private TextMesh[] _textMeshes;

    private static readonly Dictionary<int, string> AnimationSpeedLabels = new Dictionary<int, string>()
    {
        {0, "-2x" },
        {1, "-1x" },
        {2, "-0.5x" },
        {3, "0.5x" },
        {4, "1x" },
        {5, "2x" }
    };

    private void Awake()
    {
        Inst = this;
    }

    void Start () {
        // get the reference to the animationController of the ProgressBar
        _anim = GetComponent<Animator>();
        // get the reference to the TextMeshes of the ProgressBar
        _textMeshes = GetComponentsInChildren<TextMesh>();
        // set the TextMeshes to the right size and position
        for (int i = 0; i < _textMeshes.Length; i++)
        {
            _textMeshes[i].transform.localScale = Vector3.one * 0.3f;
            _textMeshes[i].transform.localPosition += Vector3.up * (i + 1) / 2;
        }
    }
	
	void Update () {
        if (StructureData.GetCurrFrameData() != null)
        {
            // update the progress of the ProgressBar
            _anim.SetFloat("Progress", 1f * AnimationController.frame / (StructureData.GetCurrFrameData().frames - 1));
            foreach (TextMesh TM in _textMeshes)
                // update the text which shows the progress
                if (TM.name.Contains("Progress"))
                    TM.text = (AnimationController.frame + 1) + " / " + StructureData.GetCurrFrameData().frames;
                // update the text which shows how fast the animation is being played
                else if (TM.name.Contains("AnimationSpeed"))
                {
                    // if the text is activated while it should be deactivated deaktivate it and vice versa
                    if (AnimationController.run_anim != TM.gameObject.activeSelf)
                        TM.gameObject.SetActive(AnimationController.run_anim);

                    // update the text of how fast the anim speed is
                    if (AnimationController.run_anim)
                        TM.text = AnimationSpeedLabels[AnimationController.animSpeed];
                }
        }
    }
}
