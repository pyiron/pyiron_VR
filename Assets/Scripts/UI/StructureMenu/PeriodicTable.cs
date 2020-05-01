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
        SetState(false);
        StructureMenuController.Inst.elementButton.interactable = true;
        StructureMenuController.Inst.UpdateElementButton(element);
        StructureMenuController.Inst.OnStructureChange();
//        StructureMenuController.Inst.SetElementDropdown(element);
    }
}
