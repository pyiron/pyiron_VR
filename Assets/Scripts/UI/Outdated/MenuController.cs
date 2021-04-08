using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private List<Selectable> _activeSelectables = new List<Selectable>();

    /// <summary>
    /// Hide or Show this menu. Allows to execute code when this menu gets hidden or showed, e.g. to update the position
    /// </summary>
    /// <param name="active"></param>
    public virtual void SetState(bool active)
    {
        gameObject.SetActive(active);
        //if (active)
        //    ProgramSettings.MoveToCenter(transform.parent.gameObject);

        if (active)
        {
            UpdatePanelPosition();
        }
    }
    
    internal virtual void Activate()
    {
        // activates all UI elements which got deactivated by Deactivate()
        foreach (Selectable selectable in _activeSelectables)
        {
            // the selectable could have been destroyed while the panel was inactive
            if (selectable != null)
            {
                // activate the UI element again
                selectable.interactable = true;
            }
        }
        _activeSelectables.Clear();
    }

    internal virtual void Deactivate()
    {
        if (_activeSelectables.Count!= 0) return;
        // deactivates all UI elements which are not already deactivated and remembers them
        foreach (Selectable selectable in GetComponentsInChildren<Selectable>())
        {
            if (selectable.interactable)
            {
                selectable.interactable = false;
                _activeSelectables.Add(selectable);
            }
        }
    }

    /// <summary>
    /// Find the reference to the current Main Panel that should be in the middle.
    /// </summary>
    /// <returns></returns>
    private static MenuController GetCurrentMainPanel()
    {
        Dictionary<Modes, MenuController> mainPanels = new Dictionary<Modes, MenuController>()
        {
            {Modes.Network, NetworkMenuController.Inst},
            {Modes.Calculate, SimulationMenuController.Inst},
            {Modes.Explorer, ExplorerMenuController.Inst},
            {Modes.Structure, StructureMenuController.Inst},
        };
        return mainPanels[ModeController.currentMode.mode];
    }

    internal static void UpdatePanelPosition()
    {
        // Adjust the position of all Panels, so that the main Panel stays in the middle
        MenuController mainPanel = GetCurrentMainPanel();
        var mainPanelTransform = mainPanel.transform;
        var localPosition = mainPanelTransform.parent.localPosition;
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainPanelTransform.parent.GetComponent<RectTransform>());
        localPosition = new Vector3(-1f * mainPanelTransform.localPosition.x, 
            localPosition.y, localPosition.z);
        mainPanelTransform.parent.localPosition = localPosition;
    }
}
