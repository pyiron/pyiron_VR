using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeBar : MonoBehaviour
{
    public static ModeBar Inst;
    
    private Button[] modeButtons;
    // private string currentMode;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        modeButtons = GetComponentsInChildren<Button>();
        // UpdateButtons("Structure");
    }

    private void Update()
    {
        // if (currentMode != ModeController.currentMode.mode.ToString())
        // {
        //     currentMode = ModeController.currentMode.mode.ToString();
        //     UpdateButtons(currentMode);
        // }
    }

    /// <summary>
    /// Displays the currently active mode among the modes shown on top of the panel and
    /// notifies the ModeController.
    /// </summary>
    /// <param name="clickedButton"></param>
    public void OnButtonClicked(Button clickedButton)
    {
        string btnText = clickedButton.GetComponentInChildren<Text>().text;
        //UpdateButtons(btnText);
        
        ModeController.inst.SetMode(btnText);
    }

    /// <summary>
    /// Displays the currently active mode among the modes shown on top of the panel
    /// </summary>
    /// <param name="activeMode"></param>
    // public void UpdateButtons(string activeMode)
    // {
    //     foreach (Button button in modeButtons)
    //     {
    //         print(button.name + " vs " + activeMode + " is the same: " + (button.name == activeMode));
    //         if (button.name == activeMode)
    //         {
    //             button.interactable = false;
    //             button.image.color = Color.cyan;
    //         }
    //         else
    //         {
    //             button.interactable = true;
    //             button.image.color = Color.white;
    //         }
    //     }
    // }
}
