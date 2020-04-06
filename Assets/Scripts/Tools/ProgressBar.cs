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

    public TextMesh ProgressText;
    public TextMesh AnimationSpeedText;

    private static readonly Dictionary<int, string> AnimationSpeedLabels = new Dictionary<int, string>()
    {
        {0, "-2x" },
        {1, "-1x" },
        {2, "-0.5x" },
        {3, "0.5x" },
        {4, "1x" },
        {5, "2x" }
    };

    private static readonly int Progress = Animator.StringToHash("Progress");

    private void Awake()
    {
        Inst = this;
    }

    void Start () {
        // get the reference to the animationController of the ProgressBar
        _anim = GetComponent<Animator>();
    }

    public void UpdateBar()
    {
        // update the progress of the ProgressBar
        _anim.SetFloat(Progress,
            1f * AnimationController.frame / (AnimationController.positionData.Length - 1f));
            
        ProgressText.text = (AnimationController.frame + 1) + " / " + AnimationController.positionData.Length;
            
        // if the text is activated while it should be deactivated deactivate it and vice versa
        if (AnimationController.run_anim != AnimationSpeedText.gameObject.activeSelf)
            AnimationSpeedText.gameObject.SetActive(AnimationController.run_anim);

        // update the text of how fast the anim speed is
        if (AnimationController.run_anim)
            AnimationSpeedText.text = AnimationSpeedLabels[AnimationController.animSpeed];
    }
}
