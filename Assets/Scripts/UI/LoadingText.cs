using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingText : MonoBehaviour
{
    public static LoadingText Inst; 
    
    private Text text;
    
    private float _startTime;

    private int state = 1;

    private void Awake()
    {
        Inst = this;
        text = GetComponent<Text>();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        ResetText();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void ResetText()
    {
        state = 1;
        _startTime = Time.time;
        text.text = "Loading";
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 4 && Time.time - _startTime > 2f)
        {
            ResetText();
        }
        if (Time.time - _startTime > state * 0.5f && state < 4)
        {
            text.text += ".";
            state += 1;
        }
    }
}
