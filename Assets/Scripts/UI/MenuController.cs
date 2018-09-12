using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuController : MonoBehaviour {

    internal void SetState(bool active)
    {
        gameObject.SetActive(active);
        if (active)
            ProgramSettings.MoveToCenter(transform.parent.gameObject);
    }
}
