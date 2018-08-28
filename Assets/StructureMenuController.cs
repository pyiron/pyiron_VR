using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StructureMenuController : MonoBehaviour {
    // reference to the deployed scripts
    internal static StructureMenuController inst;
    public GameObject OptionPrefab;
    public GameObject OptionFolder;
    internal static bool shouldRefresh = false;
    internal static bool shouldDelete = false;

    public static Dictionary<OptionType, List<string>> options = new Dictionary<OptionType, List<string>>();

    private void Awake()
    {
        inst = this;
        ClearOptions();
    }

    internal void ClearOptions()
    {
        options.Clear();
        foreach (OptionType t in System.Enum.GetValues(typeof(OptionType)))
        {
            options.Add(t, new List<string>());
        }
    }

    private void Update()
    {
        if (shouldDelete)
        {
            foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
                Destroy(btn.gameObject);
            shouldDelete = false;
        }
        if (shouldRefresh)
        {
            foreach (StructureMenu sm in inst.transform.GetComponentsInChildren<StructureMenu>())
            {
                if (!sm.GetComponent<Button>().interactable)
                {
                    foreach (string opt in options[sm.type])
                    {
                        foreach (Text txt in OptionFolder.GetComponentsInChildren<Text>())
                        {
                            if (txt.text == opt)
                                goto next;
                            
                        }
                        GameObject newButton = Instantiate(OptionPrefab);
                        newButton.transform.SetParent(OptionFolder.transform);
                        newButton.transform.localPosition = Vector3.zero;
                        newButton.transform.eulerAngles = Vector3.up * 90;
                        newButton.GetComponentInChildren<Text>().text = opt;
                        next:;
                    }
                }
            }
            shouldRefresh = false;
        }
    }

    public void AddOption(OptionType t, string opt)
    {
        options[t].Add(opt);
        shouldRefresh = true;
    }
}

public enum OptionType
{
    Folder, Script, Job
}
