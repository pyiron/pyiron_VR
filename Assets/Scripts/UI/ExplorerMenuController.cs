using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
//using Newtonsoft.Json;

public class ExplorerMenuController : MenuController {
    // reference to the deployed scripts
    internal static ExplorerMenuController inst;
    public GameObject OptionPrefab;
    public GameObject OptionFolder;
    public GameObject PathPrefab;
    public GameObject PathFolder;
    public static bool shouldRefresh = false;
    //internal static bool shouldDelete;
    private static string currPath;
    public static bool pathHasChanged;

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
        GameObject newButton = Instantiate(Pref, parent.transform, true);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.localEulerAngles = Vector3.zero;
        newButton.transform.localScale = Vector3.one;
        newButton.GetComponentInChildren<Text>().text = txt;
        newButton.GetComponent<Image>().color = col;
        return newButton;
    }

    public void DeleteOptions()
    {
        foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
            Destroy(btn.gameObject);
    }

    private void Update()
    {
        //transform.parent.parent.position = SceneReferences.inst.CenterPoint.transform.position;
        //ProgramSettings.Face_Player(transform.parent.parent.gameObject);
        /*if (shouldDelete)
        {
            foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
                Destroy(btn.gameObject);
            shouldDelete = false;
        }*/
        if (pathHasChanged)
        {
            PathHasChanged();
        }
        if (shouldRefresh)
        {
            ShowNewOptionsOld();
        }

        //foreach (Button btn in transform.parent.GetComponentsInChildren<Button>())
        //    btn.interactable = !PythonExecuter.inst.IsLoading();
    }

    public void ShowNewOptionsOld()
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
    
    public void ShowNewOptions(FolderData folderData)
    {
        List<string> opts;
        for (int i = 0; i < 2; i++)
        {
            if (i == 0)
            {
                opts = folderData.groups;
            }
            else
            {
                opts = folderData.nodes;
            }
            
            Color btnCol;
            if (i == 1)
                btnCol = Color.cyan;
            else
                btnCol = Color.yellow;
            foreach (string opt in opts)
            {
                GameObject btn = InstantiateNewBtn(OptionPrefab, OptionFolder, opt, btnCol);
                btn.GetComponent<OptionButton>().isJob = i == 1;
            }
        }
    }

    public void PathHasChanged(string newPath="")
    {
        Button[] pathButtons = PathFolder.GetComponentsInChildren<Button>();
        bool correctPath = true;
        string[] splittedPath = currPath.Split('/');
        for (int i = 0; i < pathButtons.Length; i++)
        {
            Button btn = pathButtons[i];
            if (correctPath)
            {
                if (splittedPath.Length > i)
                    if (btn.GetComponentInChildren<Text>().text == splittedPath[i])
                    {
                        continue;
                    }
                correctPath = false;
            }
            Destroy(btn.gameObject);
        }
        for (int i = pathButtons.Length; i < splittedPath.Length; i++)
        {
            InstantiateNewBtn(PathPrefab, PathFolder, splittedPath[i], Color.white);
        }
        pathHasChanged = false;
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
    
    public void LoadPathContentHelper(string jobName) {
        LoadPathContent(jobName);
    }

    public void LoadPathContent(string jobName="", bool isAbsPath=false)
    {
        DeleteOptions();
        ClearOptions();
        
        // set the new path
        if (jobName != "")
        {
            if (isAbsPath)
            {
                PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer,
                    PythonCommandType.exec_l, "unity_manager.pr = Project('" + jobName + "')", handleInput: false);
            }
            else
            {
                PythonExecuter.SendOrderSync(PythonScript.ProjectExplorer,
                    PythonCommandType.exec_l, "unity_manager.pr = unity_manager.pr['" + jobName + "']", handleInput: false);
            }
        }
        
        currPath = PythonExecuter.SendOrderSync(PythonScript.None,
            PythonCommandType.eval_l, "unity_manager.pr.path[:-1]");
        PathHasChanged();
            
        // get the jobs and groups 
        FolderData folderData = 
            JsonUtility.FromJson<FolderData>(
                PythonExecuter.SendOrderSync(PythonScript.None, PythonCommandType.eval_l,
                    "unity_manager.pr.list_all()"));

        ShowNewOptions(folderData);
    }
}

// needed because Unitys JsonUtilities don't support dictionaries
[Serializable]
public class FolderData
{
    public List<string> groups;
    public List<string> nodes;
    public List<string> files;
}

public enum OptionType
{
    groups, nodes, files
}
