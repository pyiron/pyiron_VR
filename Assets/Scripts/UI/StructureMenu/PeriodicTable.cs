using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PeriodicTable : MenuController
{
    public static PeriodicTable Inst;


    private void Awake()
    {
        Inst = this;
    }

    public void OnElementButtonDown()
    {
        string element = EventSystem.current.currentSelectedGameObject.name.Split('-')[1];
        StructureMenuController.Inst.SetElementButton(element);
        StructureMenuController.Inst.SetElementDropdown(element);
        StructureMenuController.Inst.elementButton.interactable = true;
        SetState(false);
    }
}
