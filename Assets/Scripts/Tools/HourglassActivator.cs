using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component of HourglassRotator
public class HourglassActivator : MonoBehaviour {
    // the reference to the Hourglass
    private GameObject Hourglass;
    // the hourglass will just be activated, if this timer is <= 0
    private float timer;
    // the time, how long the structure should be at least loading until activating the hourglass
    private float timeUntilHourglassGetsShown = 0.3f;

    private void Awake()
    {
        // get the reference to the Hourglass
        Hourglass = transform.GetChild(0).gameObject;
    }

    void Start () {
        // set the timer
        ResetTimer();
    }

    public void ResetTimer()
    {
        // set the timer
        timer = timeUntilHourglassGetsShown;
    }
	
	void Update () {
        // check if the hourglass is deactivated while it should be activated or vice versa
        if ((PythonExecuter.outgoingChanges != PythonExecuter.incomingChanges) != Hourglass.activeSelf)
            // check if the hourglass just has to be deactivated or if the timer is already <= 0
            if (Hourglass.activeSelf || timer <= 0)
            {
                // activate or deactivate the hourglass
                Hourglass.GetComponent<Hourglass>().ActivateHourglass(!Hourglass.activeSelf);
                // set the timer for the next time, if the hourglass got deactivated
                if (!Hourglass.activeSelf)
                    timer = timeUntilHourglassGetsShown;
            }
            else
                // decrease the time passed after the last update from the timer
                timer -= Time.deltaTime;

    }
}
