using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonExplanation : MonoBehaviour
{
    [SerializeField] private Sprite symbolActive;
    [SerializeField] private Sprite symbolInactive;
    
    [SerializeField] private Image img;
    
    void Start()
    {
        SetIcon();
    }

    private void SetIcon()
    {
        if (ButtonExplanation.helpIsActive)
        {
            img.sprite = symbolActive;
        }
        else
        {
            img.sprite = symbolInactive;
        }
    }

    public void OnIconPressed()
    {
        ButtonExplanation.helpIsActive = !ButtonExplanation.helpIsActive;
        SetIcon();
    }
}
