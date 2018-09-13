using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeMenuController : MenuController {
    internal static ModeMenuController inst;
    private DropDown dropDown;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        //dropDown.
    }

    public void OnDropDownChange(int mode_nr)
    {
        print(mode_nr);
    }
}
