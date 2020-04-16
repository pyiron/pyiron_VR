using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeriodicSysMenuController : MenuController {
    internal static PeriodicSysMenuController inst;

    public GameObject StructureCreatorPanel;
    // the toggle, deciding wether a single atom or a structure should be created
    public Toggle tog;
    public Text explanation;
    // the state of the toggle, deciding wether a single atom or a structure should be created
    public bool togState;
    /*

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        OnToggle(tog);
    }

    private void Update()
    {
        // keep the explanation Text updated
        if (!togState)
        {
            explanation.text = "Click on an Element to add a new single instance of it to the structure";
        }
        else
        {
            explanation.text = "Click on an Element to create a new structure of it.";
        }
    }

    public void OnToggle(Toggle toggle)
    {
        togState = toggle.isOn;
        StructureCreatorPanel.SetActive(togState);
        StructureCreatorMenuController.inst.OnModeChange();
    }*/
}
