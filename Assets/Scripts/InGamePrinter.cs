using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePrinter : MonoBehaviour {
    private string printText = "nothing to print";
    private int currentImportance = 0;

	// Use this for initialization
	void Start () {
        ctrl_print(transform.position.ToString());
        ctrl_print("Im left!", 3, false);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        GetComponent<TextMesh>().text = printText;
        currentImportance = 0;
    }

    public void ctrl_print(string text, int importance=0, bool rightCtrl = true)
    {
        if ((transform.parent.name.Contains("right") && rightCtrl) || (transform.parent.name.Contains("left") && !rightCtrl))
            if (importance >= currentImportance)
                printText = text;
    }
}
