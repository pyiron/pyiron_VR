using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;

// Component of HourglassRotator
public class HourglassActivator : MonoBehaviour {
    public static HourglassActivator Inst;
    
    // the hourglass will just be activated, if this timer is <= 0
    private static float timer;
    // the time, how long the structure should be at least loading until activating the hourglass
    private static float timeUntilHourglassGetsShown = 0.3f;

    private void Awake()
    {
        Inst = this;
    }

    void Start () {
        // set the timer
        ResetTimer();
    }

    public static void ResetTimer()
    {
        // set the timer
        timer = timeUntilHourglassGetsShown;
    }
	
	void Update () {
        // check if the hourglass is deactivated while it should be activated or vice versa
        if (TCPClient.IsLoading() != Hourglass.inst.gameObject.activeSelf)
            // check if the hourglass just has to be deactivated or if the timer is already <= 0
            if (Hourglass.inst.gameObject.activeSelf || timer <= 0)
            {
                // activate or deactivate the hourglass
                Hourglass.inst.ActivateHourglass(!Hourglass.inst.gameObject.activeSelf);
                // set the timer for the next time, if the hourglass got deactivated
                if (!Hourglass.inst.gameObject.activeSelf)
                    timer = timeUntilHourglassGetsShown;
            }
            else
                // decrease the time passed after the last update from the timer
                timer -= Time.deltaTime;

    }
}
