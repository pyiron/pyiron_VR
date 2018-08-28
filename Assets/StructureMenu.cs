using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureMenu : MonoBehaviour {
    public OptionType type;

    public void WhenClickDown()
    {
        foreach (Button b in transform.parent.GetComponentsInChildren<Button>())
        {
            b.interactable = true;
        }
        GetComponent<Button>().interactable = false;
        StructureMenuController.shouldDelete = true;
        StructureMenuController.shouldRefresh = true;
    }

    
}
