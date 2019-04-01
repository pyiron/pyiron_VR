using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StructureMenuController : MenuController {
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

    internal void ClearOptions()
    {
        options.Clear();
        foreach (OptionType t in Enum.GetValues(typeof(OptionType)))
        {
            options.Add(t, new List<string>());
        }
    }

    private GameObject InstantiateNewBtn(GameObject Pref, GameObject parent, string txt, Color col)
    {
        GameObject newButton = Instantiate(Pref);
        newButton.transform.SetParent(parent.transform);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.eulerAngles = Vector3.up * 90;
        newButton.transform.localScale = Vector3.one;
        newButton.GetComponentInChildren<Text>().text = txt;
        newButton.GetComponent<Image>().color = col;
        return newButton;
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
            PathButton[] pathButtons = GetComponentsInChildren<PathButton>();
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
                InstantiateNewBtn(PathPrefab, PathFolder, splittedPath[i], Color.white);
            }
            pathHasChanged = false;
        }
        if (shouldRefresh)
        {
            for (int i = 0; i < 2; i++)
            {
                OptionType sm = (OptionType)(i);
                foreach (string opt in options[sm])
                {
                    foreach (Text txt in OptionFolder.GetComponentsInChildren<Text>())
                    {
                        if (txt.text == opt)
                            goto next;
                    }
                    Color btn_col;
                    if (sm == OptionType.nodes)
                        btn_col = Color.cyan;
                    else
                        btn_col = Color.yellow;
                    GameObject btn = InstantiateNewBtn(OptionPrefab, OptionFolder, opt, btn_col);
                    btn.GetComponent<OptionButton>().isJob = sm == OptionType.nodes;
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
        switch (t)
        {
            case OptionType.groups:
            case OptionType.nodes:
                if (opt[0] != '_')
                {
                    
                    options[t].Add(opt);
                }
                break;
            case OptionType.files:
                // ignore
                break;
        }
        shouldRefresh = true;
    }
}

public enum OptionType
{
    groups, nodes, files
}
