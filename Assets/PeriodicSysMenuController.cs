using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeriodicSysMenuController : MenuController {
    internal static PeriodicSysMenuController inst;

    private void Awake()
    {
        inst = this;
    }
}
