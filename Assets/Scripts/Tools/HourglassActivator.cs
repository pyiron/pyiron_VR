using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourglassActivator : MonoBehaviour {
    private GameObject Hourglass;

    private void Awake()
    {
        foreach (Transform Child in transform)
            Hourglass = Child.gameObject;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if ((PythonExecuter.outgoingChanges != PythonExecuter.incomingChanges) != Hourglass.activeSelf)
            Hourglass.GetComponent<Hourglass>().ActivateHourglass(!Hourglass.activeSelf);
	}
}
