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

    private void Start()
    {
        SetNewElement("Fe");
    }

    private void SetNewElement(string element)
    {
        StructureMenuController.Inst.elementButton.interactable = true;
        StructureMenuController.Inst.elementButton.image.color = LocalElementData.GetColour(element);
        SetState(false);
    }

    public void OnElementButtonDown()
    {
        string element = EventSystem.current.currentSelectedGameObject.name.Split('-')[1];
        SetNewElement(element);
        StructureMenuController.Inst.SetElementButton(element);
        StructureMenuController.Inst.SetElementDropdown(element);
    }
}
