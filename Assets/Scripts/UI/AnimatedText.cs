using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedText : MonoBehaviour
{
    public static Dictionary<TextInstances, AnimatedText> Instances = new Dictionary<TextInstances, AnimatedText>();

    private TextInstances _textInstance;
    //public static AnimatedText Inst; 
    
    private Text text;
    
    private float _startTime;

    private int state = 1;

    private void Awake()
    {
        //Inst = this;
        // save every instance accessable by it's representative enum
        _textInstance = (TextInstances)System.Enum.Parse(typeof(TextInstances), gameObject.name);
        Instances.Add(_textInstance, this);
        
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
        if (_textInstance == TextInstances.LoadingText)
        {
            text.text = "Loading";
        } 
        else if (_textInstance == TextInstances.ReconnectingText)
        {
            text.text = "Disconnected!\nTrying to reconnect";
        } 
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

// all the texts that can be shown (used for the references)
public enum TextInstances {
    LoadingText, ReconnectingText
}
