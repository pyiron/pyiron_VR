using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeBar : MonoBehaviour
{
    private Button[] modeButtons;
    private void Start()
    {
        modeButtons = GetComponentsInChildren<Button>();
    }

    /// <summary>
    /// Displays the currently active mode among the modes shown on top of the panel and
    /// notifies the ModeController.
    /// </summary>
    /// <param name="clickedButton"></param>
    public void OnButtonClicked(Button clickedButton)
    {
        string btnText = clickedButton.GetComponentInChildren<Text>().text;
        foreach (Button button in modeButtons)
        {
            if (button == clickedButton)
            {
                button.interactable = false;
                button.image.color = Color.cyan;
            }
            else
            {
                button.interactable = true;
                button.image.color = Color.white;
            }
        }
        
        ModeController.inst.SetMode(btnText);
    }
}
