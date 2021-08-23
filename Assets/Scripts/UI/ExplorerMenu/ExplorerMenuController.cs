using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Networking;

public class ExplorerMenuController : MenuController {
    // reference to the deployed scripts
    internal static ExplorerMenuController Inst;
    public GameObject OptionPrefab;
    public GameObject OptionFolder;
    public GameObject OptionFolderJobs;
    public GameObject PathPrefab;
    public GameObject PathFolder;

    public Gradient jobSizeGradient;

    [Tooltip("Determines how many parts of the Path should be shown")]
    [SerializeField] private int numShownPathNodes = 3;
    
    internal static string currPath;

    private FolderData folderData;

    public static Dictionary<OptionType, List<string>> options = new Dictionary<OptionType, List<string>>();

    private void Awake()
    {
        Inst = this;
        ClearOptions();
    }

    public void OnModeStart()
    {
        // reload the jobs
        LoadPathContent();
    }

    internal void ClearOptions()
    {
        options.Clear();
        foreach (OptionType t in Enum.GetValues(typeof(OptionType)))
        {
            options.Add(t, new List<string>());
        }
    }

    public void DeactivateJobButton(string jobName)
    {
        foreach (OptionButton btn in OptionFolderJobs.GetComponentsInChildren<OptionButton>())
        {
            // TODO replace GetComponent button by an attribute of OptionButton that contains the reference
            btn.GetComponent<Button>().interactable = btn.GetOptionText() != jobName;
        }
    }

    private GameObject InstantiateNewBtn(GameObject Pref, GameObject parent, string txt, Color col, bool interactable=true)
    {
        GameObject newButton = Instantiate(Pref, parent.transform, true);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.localEulerAngles = Vector3.zero;
        newButton.transform.localScale = Vector3.one;
        newButton.GetComponentInChildren<Text>().text = txt;
        newButton.GetComponent<Image>().color = col;
        newButton.GetComponent<Button>().interactable = interactable;
        return newButton;
    }

    public void DeleteOptions()
    {
        foreach (Button btn in OptionFolder.GetComponentsInChildren<Button>())
            Destroy(btn.gameObject);
        foreach (Button btn in OptionFolderJobs.GetComponentsInChildren<Button>())
            Destroy(btn.gameObject);
    }
    
    private void ShowNewFolders(FolderData folderData)
    {
        foreach (string opt in folderData.groups)
        {
            // blend out the scratch folder, access to it would cause errors
            if (opt == "scratch")
            {
                Debug.LogWarning("Scratch Folder will not be shown");
                continue;
            }
            
            InstantiateNewBtn(OptionPrefab, OptionFolder, opt, Color.yellow);
        }
        InstantiateNewBtn(OptionPrefab, OptionFolder, "..", Color.yellow);
    }
    
    public void ShowNewJobs(FolderData folderData, int[] jobSizes)
    {
        for (int i = 0; i < folderData.nodes.Count; i++)
        //foreach (string opt in folderData.nodes)
        {
            string opt = folderData.nodes[i];
            // spawn the new button. If the job is already loaded, show the button as not interactable
            GameObject btn = InstantiateNewBtn(OptionPrefab, OptionFolderJobs, opt, Color.cyan,
                interactable:opt!=SimulationMenuController.jobName);
            btn.GetComponent<OptionButton>().isJob = true;
            
            print(jobSizes);
            print(jobSizes.Length);
            
            if (jobSizes != null && i < jobSizes.Length)
            {
                // color the button according to the expected loading time
                btn.GetComponent<Image>().color = jobSizeGradient.Evaluate(jobSizes[i] / 10000000f);
            }
            else
            {
                Debug.LogWarning("Invalid job sizes: " + jobSizes);
            }
        }
    }
    
    public void ShowNewOptions(FolderData folderData, int[] jobSizes)
    {
        ShowNewFolders(folderData);
        ShowNewJobs(folderData, jobSizes);
    }

    public void PathHasChanged(string newPath = "")
    {
        //Destroy all previously shown buttons
        Button[] pathButtons = PathFolder.GetComponentsInChildren<Button>();
        foreach (Button button in pathButtons)
        {
            Destroy(button.gameObject);
        }
        
        // show the new buttons
        string[] splittedPath = currPath.Split('/');
        for (int i = 0; i < splittedPath.Length; i++)
        {
            // just last numShownPathNodes entries get shown
            if (i + numShownPathNodes >= splittedPath.Length)
            {
                GameObject newButton = InstantiateNewBtn(PathPrefab, PathFolder, splittedPath[i], Color.white);
                newButton.GetComponent<PathButton>().id = i;
            } 
            /*else if (i == 0 && splittedPath.Length > numShownPathNodes)
            {
                GameObject newButton = InstantiateNewBtn(PathPrefab, PathFolder, "...", Color.white);
                newButton.GetComponent<Button>().enabled = false;
            }*/
        }
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
    }
    
    public void LoadPathContentHelper(string jobName) {
        LoadPathContent(jobName);
    }

    public FolderData LoadFolderData()
    {
        string data = PythonExecutor.SendOrderSync(true,
            PythonCmd.GetFolderData);
        print(data);
        return JsonUtility.FromJson<FolderData>(data);
    }

    public void LoadJobSizes()
    {
        //string data = 
        PythonExecutor.SendOrderAsync(true,
            PythonCmd.GetJobSizes, OnJobSizesReceived);
    }

    public void OnJobSizesReceived(ReturnedMessage data)
    {
        print("JobSizes: " + data);
        int[] jobSizes = JsonUtility.FromJson<JobSizes>(data.msg).jobSizes;
        ShowNewOptions(folderData, jobSizes);
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
                PythonExecutor.SendOrderSync(false, PythonCmd.OpenAbsPath(jobName));
            }
            else
            {
                PythonExecutor.SendOrderSync(false, PythonCmd.OpenRelPath(jobName));
            }
        }
        
        currPath = PythonExecutor.SendOrderSync(true, PythonCmd.GetPath);
        PathHasChanged();
            
        // get the jobs and groups 
        folderData = LoadFolderData();
        if (folderData.nodes.Count > 0)
        {
            // load the sizes of the jobs
            PythonExecutor.SendOrderAsync(true,
                PythonCmd.GetJobSizes, OnJobSizesReceived);
        }
        else
        {
            ShowNewOptions(folderData, null);
        }
    }

    public void DeleteJob()
    {
        //PythonExecuter.SendOrderSync(PythonScript.executor,
        //    PythonCommandType.exec_l, "reset_job('" + SimulationMenuController.jobName + "')");
        
        PythonExecutor.SendOrderSync(false, PythonCmd.ResetCurrentJob());
        
        // PythonExecuter.SendOrderSync(PythonScript.unityManager,
        //     PythonCommandType.exec_l, "project['" + jobName + "'].remove()");
        
        // Debug.LogWarning("The following line can be removed when pyiron integrated it into the remove function");
        // PythonExecuter.SendOrderSync(PythonScript.unityManager,
        //     PythonCommandType.exec_l, "project['" + jobName + "']._status = None");
    }
}

// needed because JsonUtilities don't support dictionaries
[Serializable]
public class FolderData
{
    public List<string> groups;
    public List<string> nodes;
    public List<string> files;
}

// needed because JsonUtilities don't support unwrapped arrays
[Serializable]
public class JobSizes
{
    public int[] jobSizes;
}

public enum OptionType
{
    groups, nodes, files
}
