using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Component of MainVACanvas/Panel/ErrorText
public class ErrorTextController : MonoBehaviour
{
    public static ErrorTextController inst;
    
    // the reference to the attached Text Object
    private static Text textObject;
    private static string errorMsg;
    private static float shrinkTimer;
    private static int activeTime = 12;

    private void Awake()
    {
        inst = this;
    }

    void Start()
    {
        textObject = GetComponent<Text>();
        shrinkTimer = activeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (errorMsg != "")
        {
            textObject.text = errorMsg;
            if (shrinkTimer > 0)
            {
                shrinkTimer -= Time.deltaTime;
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.localScale -= Vector3.one * Time.deltaTime;
                if (transform.localScale.x <= 0)
                {
                    errorMsg = "";
                    textObject.text = errorMsg;
                }
            }
        }
    }

    public void ShowMsg(string msg)
    {
        errorMsg = msg;
        shrinkTimer = activeTime;
    }
}
