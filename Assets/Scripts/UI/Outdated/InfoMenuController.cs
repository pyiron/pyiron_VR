using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoMenuController : MenuController {
    internal static InfoMenuController inst;

    private void Awake()
    {
        inst = this;
    }
}
