using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePrinter : MonoBehaviour {
    // TODO: let each controller have its own script
    public static InGamePrinter[] inst = new InGamePrinter[2];

    [Header("Scene")]
    TextMesh textMesh;
    // the size the text should have
    private float textSize = 0.2f;

    private void Awake()
    {
        if (inst[0] == null)
            inst[0] = this;
        else
            inst[1] = this;
    }

    // Use this for initialization
    void Start () {
        textMesh = GetComponentInChildren<TextMesh>();

        textSize = textSize / ProgramSettings.textResolution * 10;
        textMesh.transform.localScale = Vector3.one * textSize;
        textMesh.fontSize = (int)ProgramSettings.textResolution;
        SetState(false);
    }

    public void SetState(bool active)
    {
        textMesh.gameObject.SetActive(active);
    }

    public void Ctrl_print(string text)
    {
        // test if the controller has already been active
        if (textMesh != null)
            textMesh.text = text;
    }
}
