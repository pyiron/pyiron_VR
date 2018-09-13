using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMenuController : MenuController {
    internal static AnimationMenuController inst;

    private void Awake()
    {
        inst = this;
    }
}
