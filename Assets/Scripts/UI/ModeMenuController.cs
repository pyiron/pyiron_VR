using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeMenuController : MenuController {
    internal static ModeMenuController inst;
    public GameObject OptionFolder;
    public GameObject OptionButtonPref;
    private Dropdown dropDown;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        foreach (Mode mode in ModeData.modes)
        {
            GameObject newModeBtn = Instantiate(OptionButtonPref);
            newModeBtn.transform.SetParent(OptionFolder.transform);
            newModeBtn.GetComponentInChildren<Text>().text = mode.mode.ToString();
        }
    }

    private void Update()
    {
        foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
        {
            btn.transform.localPosition -= new Vector3(0, 0, btn.transform.localPosition.z);
            btn.transform.localEulerAngles = Vector3.zero;
            btn.transform.localScale = Vector3.one;
        }
    }

    public void OnButtonClicked(Button btn)
    {
        ModeData.inst.SetMode((Modes)System.Enum.Parse(typeof(Modes), btn.GetComponentInChildren<Text>().text));
    }

    internal void OnModeChange()
    {
        foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
        {
            bool activeMode = ModeData.currentMode.mode.ToString() == btn.GetComponentInChildren<Text>().text;
            if (activeMode)
            {
                btn.GetComponent<Image>().color = Color.green;
            }
            else
                btn.GetComponent<Image>().color = Color.white;
            btn.interactable = !activeMode;
        }
    }
}
