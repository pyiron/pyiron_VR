using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MenuController : MonoBehaviour
{
    private List<Selectable> _activeSelectables = new List<Selectable>();

    internal virtual void SetState(bool active)
    {
        gameObject.SetActive(active);
        //if (active)
        //    ProgramSettings.MoveToCenter(transform.parent.gameObject);
    }
    
    internal virtual void Activate()
    {
        // activates all UI elements which got deactivated by Deactive()
        foreach (Selectable selectable in _activeSelectables)
        {
            selectable.interactable = true;
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
}
