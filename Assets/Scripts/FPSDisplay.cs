﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;

    [SerializeField] private Text textDisplay;

    private void Start()
    {
        InvokeRepeating(nameof(ShowFps), 0, 1f);
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void ShowFps()
    {
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        // show fps and time between renders
        //string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        // only show fps
        string text = $"{fps:0.} fps";
        
        if (textDisplay != null)
        {
            textDisplay.text = text;
        }
    }
 
    // void OnGUI()
    // {
    //     int w = Screen.width, h = Screen.height;
    //
    //     GUIStyle style = new GUIStyle();
    //
    //     Rect rect = new Rect(0, 0, w, h * 2 / 100);
    //     style.alignment = TextAnchor.UpperLeft;
    //     style.fontSize = h * 2 / 100;
    //     style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
    //     float msec = deltaTime * 1000.0f;
    //     float fps = 1.0f / deltaTime;
    //     string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    //     GUI.Label(rect, text, style);
    // }
}