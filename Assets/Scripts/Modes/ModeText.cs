using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeText : MonoBehaviour
{
    // Make this class a Singleton
    public static ModeText Inst;
    // a timer which will disable the text after a few seconds
    private float modeTextTimer;
    // the reference to the attached Text Object
    private Text textObject;
    // the size the text should have
    private float textSize = 8f;

    private void Awake()
    {
        Inst = this;
        
        textObject = gameObject.GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        textSize = textSize / ProgramSettings.textResolution * 10;
        transform.localScale = Vector3.one * textSize;
        textObject.fontSize = (int)ProgramSettings.textResolution;
        
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (modeTextTimer > 0)
        {
            if (modeTextTimer - Time.deltaTime <= 0)
            {
                transform.localScale = Vector3.one * textSize;
                gameObject.SetActive(false);
            }
            else if (modeTextTimer - Time.deltaTime < 1)
                // let the text fade away
                transform.localScale -= Vector3.one * textSize * Time.deltaTime;
            modeTextTimer -= Time.deltaTime;
        }
    }

    public void OnModeChange()
    {
        textObject.text = ModeController.currentMode.mode + " mode";
        gameObject.SetActive(true);
        modeTextTimer = 3;
        // set the text to it's original size
        transform.localScale =  Vector3.one * textSize;
        //transform.eulerAngles = new Vector3(0, SceneReferences.inst.HeadGO.transform.eulerAngles.y, 0);
        //Vector3 newTextPosition = Vector3.zero;
        //newTextPosition.x += Mathf.Sin(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        //newTextPosition.z += Mathf.Cos(SceneReferences.inst.HeadGO.transform.eulerAngles.y / 360 * 2 * Mathf.PI);
        //newTextPosition.y = 5;
        //CurrentModeText.transform.position = newTextPosition * 5;
    }
}
