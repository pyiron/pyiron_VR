using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourglassActivator : MonoBehaviour {
    private GameObject Hourglass;

    private float timer;

    private float timeUntilHourglassGetsShown = 0.4f;

    private void Awake()
    {
        foreach (Transform Child in transform)
            Hourglass = Child.gameObject;
    }

    // Use this for initialization
    void Start () {
        timer = timeUntilHourglassGetsShown;
    }
	
	// Update is called once per frame
	void Update () {
        if ((PythonExecuter.outgoingChanges != PythonExecuter.incomingChanges) != Hourglass.activeSelf)
            if (Hourglass.activeSelf || timer <= 0)
            {
                Hourglass.GetComponent<Hourglass>().ActivateHourglass(!Hourglass.activeSelf);
                if (Hourglass.activeSelf)
                    timer = timeUntilHourglassGetsShown;
            }
            else
                timer -= Time.deltaTime;

    }
}
