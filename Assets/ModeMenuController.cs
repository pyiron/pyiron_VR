using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeMenuController : MenuController {
    internal static ModeMenuController inst;

    private void Awake()
    {
        inst = this;
    }
}
