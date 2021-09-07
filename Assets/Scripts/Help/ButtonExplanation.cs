using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExplanation : MonoBehaviour
{
    private static Dictionary<HandRole, ButtonExplanation> instances = new Dictionary<HandRole, ButtonExplanation>();

    public static bool helpIsActive = true;
    
    [SerializeField] private Text explanationText;
    [SerializeField] private GameObject explanationPanel;

    [SerializeField] private HandRole handRole;
    
    [SerializeField] private ReticlePoser reticle;

    private GameObject lastHoveredObject;
    private TooltipText lastHoveredObjectTooltip;

    private static Dictionary<ControllerButton, string> explanationsLeft = new Dictionary<ControllerButton, string>()
    {
        {ControllerButton.Grip, "Move the UI to the controller or to the back"},
        {ControllerButton.Trigger, "Interact with the UI or the structure"},
        {ControllerButton.Pad, "Allows to move an object controlled by the laser to the front or distance"},
        {ControllerButton.Menu, "Toggle the laser"},
    };
    
    private static Dictionary<ControllerButton, string> explanationsRight = new Dictionary<ControllerButton, string>()
    {
        {ControllerButton.Grip, "Move the UI to the controller or to the back"},
        {ControllerButton.Trigger, "Interact with the UI or the structure"},
        {ControllerButton.Pad, "Allows to move an object controlled by the laser to the front or distance"},
    };

    private Dictionary<ControllerButton, string> explanations;

    private void Awake()
    {
        instances[handRole] = this;
    }

    private void SetText(string txt)
    {
        explanationText.text = txt;
    }

    private void SetButtonText(ControllerButton btn)
    {
        // special case: give explanation for resize
        if (btn == ControllerButton.Trigger && 
            (LaserGrabber.instances[0].attachedObject != null || LaserGrabber.instances[1].attachedObject != null))
        {
            SetText("Grab structure with both controllers to resize it");
        }
        else
        {
            SetText(explanations[btn]);
        }
        explanationPanel.SetActive(true);
    }
    
    public static void DeactivateButtonText(HandRole hand)
    {
        instances[hand].gameObject.SetActive(false);
    }

    private void Start()
    {
        explanationPanel.SetActive(false);
        explanations = 
            handRole == HandRole.LeftHand ? explanationsLeft : explanationsRight;
    }

    private void Update()
    {
        explanationPanel.SetActive(false);

        if (!helpIsActive) return;
        
        foreach (ControllerButton btn in explanations.Keys)
        {
            print(ControllerButton.Trigger);
            if (ViveInput.GetPress(handRole, ControllerButton.Trigger))
            {
                SetButtonText(btn);
            }
        }

        if (reticle.hitTarget != null && reticle.hitTarget != lastHoveredObject)
        {
            lastHoveredObjectTooltip = reticle.hitTarget.GetComponent<TooltipText>();
            if (lastHoveredObjectTooltip != null)
            {
                SetText(lastHoveredObjectTooltip.text);
                explanationPanel.SetActive(true);
            }
        }
    }
}
