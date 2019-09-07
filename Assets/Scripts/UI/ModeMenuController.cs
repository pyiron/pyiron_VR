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
            // todo: it should be possible in the future to go back to the Project Explorer to load a new structure
            if (mode.mode == Modes.Network || mode.mode == Modes.Explorer) continue;
            GameObject newModeBtn = Instantiate(OptionButtonPref, OptionFolder.transform, true);
            newModeBtn.GetComponentInChildren<Text>().text = mode.mode.ToString();
        }
    }

    private void Update()
    {
        foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
        {
            var transform1 = btn.transform;
            var localPosition = transform1.localPosition;
            localPosition -= new Vector3(0, 0, localPosition.z);
            transform1.localPosition = localPosition;
            transform1.localEulerAngles = Vector3.zero;
            transform1.localScale = Vector3.one;
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
