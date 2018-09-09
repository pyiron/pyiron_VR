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
    public GameObject PathPrefab;
    public GameObject PathFolder;
    internal static bool shouldRefresh = false;
    internal static bool shouldDelete = false;
    internal static string currPath;
    internal static bool pathHasChanged;

    public static Dictionary<OptionType, List<string>> options = new Dictionary<OptionType, List<string>>();

    private void Awake()
    {
        inst = this;
        ClearOptions();
    }

    internal void SetState(bool active)
    {
        ProgramSettings.MoveToCenter(transform.parent.parent.gameObject);
    }

    internal void ClearOptions()
    {
        options.Clear();
        foreach (OptionType t in Enum.GetValues(typeof(OptionType)))
        {
            options.Add(t, new List<string>());
        }
    }

    private void InstantiateNewBtn(GameObject Pref, GameObject parent, string txt)
    {
        GameObject newButton = Instantiate(Pref);
        newButton.transform.SetParent(parent.transform);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.eulerAngles = Vector3.up * 90;
        newButton.transform.localScale = Vector3.one;
        newButton.GetComponentInChildren<Text>().text = txt;
    }

    private void Update()
    {
        //transform.parent.parent.position = SceneReferences.inst.CenterPoint.transform.position;
        //ProgramSettings.Face_Player(transform.parent.parent.gameObject);
        if (shouldDelete)
        {
            foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
                Destroy(btn.gameObject);
            shouldDelete = false;
        }
        if (pathHasChanged)
        {
            PathButton[] pathButtons = transform.parent.GetComponentsInChildren<PathButton>();
            bool correctPath = true;
            string[] splittedPath = currPath.Split('/');
            for (int i = 0; i < pathButtons.Length; i++)
            {
                PathButton btn = pathButtons[i];
                if (correctPath)
                {
                    if (splittedPath.Length > i)
                        if (btn.GetComponentInChildren<Text>().text == splittedPath[i])
                        {
                            continue;
                        }
                    correctPath = false;
                }
                if (!correctPath)
                {
                    Destroy(btn.gameObject);
                }
            }
            for (int i = pathButtons.Length; i < splittedPath.Length; i++)
            {
                InstantiateNewBtn(PathPrefab, PathFolder, splittedPath[i]);
            }
            pathHasChanged = false;
        }
        if (shouldRefresh)
        {
            StructureMenuButton sm = ActiveType();
            if (sm != null)
            {
                foreach (string opt in options[sm.type])
                {
                    foreach (Text txt in OptionFolder.GetComponentsInChildren<Text>())
                    {
                        if (txt.text == opt)
                            goto next;
                    }
                    InstantiateNewBtn(OptionPrefab, OptionFolder, opt);
                    next:;
                }
            }
            shouldRefresh = false;
        }

        //foreach (Button btn in transform.parent.GetComponentsInChildren<Button>())
        //    btn.interactable = !PythonExecuter.inst.IsLoading();
    }

    public void AddOption(OptionType t, string opt)
    {
        options[t].Add(opt);
        shouldRefresh = true;
    }

    internal StructureMenuButton ActiveType()
    {
        foreach (StructureMenuButton sm in inst.transform.GetComponentsInChildren<StructureMenuButton>())
        {
            if (!sm.GetComponent<Button>().interactable)
                return sm;
        }
        return null;
    }
}

public enum OptionType
{
    Folder, Script, Job
}
