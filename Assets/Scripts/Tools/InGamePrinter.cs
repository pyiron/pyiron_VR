using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePrinter : MonoBehaviour {
    [Header("Scene")]
    public GameObject[] printers;
    public LaserGrabber[] LG;
    private string[] printText = new string[2];
    private int[] currentImportance = new int[2];
    // the size the text should have
    private float textSize = 0.2f;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < 2; i++)
        {
            printText[i] = "";
            currentImportance[i] = 0;
        }

        textSize = textSize / ProgramSettings.textResolution * 10;
        foreach (GameObject printerText in printers)
        {
            printerText.transform.localScale = Vector3.one * textSize;
            printerText.GetComponent<TextMesh>().fontSize = (int)ProgramSettings.textResolution;
        }
    }
	
    
	// Update is called once per frame
	void LateUpdate () {
        for (int i = 0; i < 2; i++)
        {
            printers[i].GetComponent<TextMesh>().text = printText[i];
            currentImportance[i] = 0;
        }
    }

    public void Ctrl_print(string text, int importance=0, bool rightCtrl = true)
    {
        int ctrlNr = 0;
        if (rightCtrl)
            ctrlNr = 1;

        if (importance >= currentImportance[ctrlNr])
        {
            printText[ctrlNr] = text;
            currentImportance[ctrlNr] = importance;
        }
    }
}
