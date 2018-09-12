using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureMenuButton : MonoBehaviour, IButton
{
    public OptionType type;

    public void WhenClickDown()
    {
        StructureMenuController.inst.SetOptiontype(GetComponentInChildren<Text>().text);
    }

    
}
