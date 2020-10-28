using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class SimulationMenuController : MenuController
{
    internal static SimulationMenuController Inst;

    public static string jobName;

    public static readonly string SHIFTED = "_shifted";

    private void Awake()
    {
        Inst = this;
    }

    private void UpdatePanels()
    {
        string order = "format_job_settings()";
        string data = PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.eval_l, order);
        JobData jobData = JsonUtility.FromJson<JobData>(data);
        
        JobSettingsController.Inst.OnModeStart(jobData);
        MdMenuController.Inst.OnModeStart(jobData);
        MinimizeMenuController.Inst.OnModeStart(jobData);

        ActionPanelController.Inst.OnModeStart();

        AnimationMenuController.Inst.SetState(AnimationController.Inst.HasAnimationLoaded());
    }

    public void OnModeStart()
    {
        // check if the job has been loaded already
        if (AnimationController.Inst.HasAnimationLoaded())
        {
            UpdatePanels();
        }
        else
        {
            if (CheckJobExists())
            {
                if (IsStructureShifted())
                {
                    // Reset the job, then load all information except for the structure data
                    ActionPanelController.Inst.OnDeleteBtnDown(false);
                    UpdatePanels();
                }
                else
                {
                    string job = PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.eval_l, 
                        "load_job(" + PythonScript.unityManager + ".project['"+ jobName + "'])");
                    StructureLoader.LoadAnimation(job);
                    // Load the information of the old job. The structure should have been set in Explorer already
                    // PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l, order);
                    UpdatePanels();
                }
            }
            else
            {
                // create a new job, then load the information from it
                PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l, "load_job(None)");
                UpdatePanels();
            }
        }
    }
    
    /*public void SetNIonicSteps(Dropdown dropdown)
    {
        string n_ionic_steps = dropdown.options[dropdown.value].text;
        print("Setting n_ionic_steps to " + dropdown.options[dropdown.value].text);
        // set the value
        string order = "n_ionic_steps = " + n_ionic_steps;
        PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.exec_l, order);
    }*/

    /// <summary>
    /// Checks whether a job with the current name/structure already exists.
    /// </summary>
    /// <returns>true if the job exists</returns>
    public bool CheckJobExists()
    {
        FolderData folderData = ExplorerMenuController.Inst.LoadFolderData();
        return folderData.nodes.Contains(jobName);
    }

    // TODO: Deactivate and activate UI Elements
    private IEnumerator HandleLammpsLoad(string order, PythonScript receivingScript)
    {
        // send the order to execute lammps
        ////PythonExecutor.SendOrderAsync(PythonScript.Executor, PythonCommandType.eval, order);
        //PythonExecuter.SendOrderAsync(receivingScript, PythonCommandType.eval_l, order);

        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.TaskNumIn;

        // wait until the response to the send message has arrived
        yield return new WaitUntil(() => taskNumIn == TCPClient.TaskNumOut);

        // get the response
        string result = TCPClient.ReturnedMsg;

        OnJobdataReceived(result);

        // AnimationController.frame = 0;
        // PythonExecuter.HandlePythonMsg(result);
        // AnimationMenuController.Inst.SetState(true);
        //ModeController.inst.SetMode(Modes.Animate);
    }

    private void OnJobdataReceived(string data)
    {
        // handle the response
        Activate();
        
        ActionPanelController.Inst.UpdateButtons(true);

        StructureLoader.LoadAnimation(data);
    }

    public void CalculateNewJob()
    {
        // update the temperture
        //PythonExecuter.SendOrderSync(PythonScript.None, PythonCommandType.exec_l,
        //    "unity_manager.Executor.temperature = " + Thermometer.temperature);
        //string calculation = ModeData.currentMode.showTemp ? "md" : "minimize";
        string calculation = SimulationModeManager.CurrMode.ToString().ToLower();
        string order;
        JobData jobData = JobSettingsController.Inst.GetData();
        if (calculation == "md")
        {
            JobData data = MdMenuController.Inst.GetData();
            order = "calculate_" + calculation + "(" +
                    data.temperature + ", " +
                    data.n_ionic_steps + ", " +
                    data.n_print + ", " +
                    jobData.job_type + ", " +
                    jobData.job_name + ", " +
                    jobData.currentPotential + ")";
        }
        else
        {
            JobData data = MinimizeMenuController.Inst.GetData();
            order = "calculate_" + calculation + "(" +
                    data.f_eps + ", " +
                    data.max_iterations + ", " +
                    data.n_print + ", " +
                    jobData.job_type + ", " +
                    jobData.job_name + ", " +
                    jobData.currentPotential + ")";
        }

        Deactivate();

        // load the new structure in another coroutine
        //StartCoroutine(HandleLammpsLoad(order, PythonScript.executor));
        PythonExecuter.SendOrderAsync(PythonScript.executor, PythonCommandType.eval_l, order, OnJobdataReceived);

        print("Job calculation startet");
        //lammpsIsMd = ModeData.currentMode.showTemp;
    }

    public bool IsStructureShifted()
    {
        return jobName.EndsWith(SHIFTED);
    }
}

public struct JobData
{
    // data for all calculation modes
    public string calc_mode;
    public string job_type;
    public string job_name;
    public string currentPotential;
    public string[] potentials;
    
    // data for md and minimize
    public string n_print;
    
    // data for md
    public int temperature;
    public string n_ionic_steps;
    
    // data for minimize
    public string f_eps;
    public string max_iterations;

    private void SetAllToNull()
    {
        this.calc_mode = null;
        job_type = null;
        job_name = null;
        this.currentPotential = null;
        this.potentials = null;
        n_print = null;
        temperature = 0;
        n_ionic_steps = null;
        f_eps = null;
        max_iterations = null;
    }

    public JobData(string calcMode=null, string jobType=null, string jobName=null, string currentPotential=null,
        string[] potentials=null, string nPrint=null, int temperature=0, string nIonicSteps=null, string fEps=null,
        string maxIterations=null)
    {
        calc_mode = calcMode;
        job_type = jobType;
        job_name = jobName;
        this.currentPotential = currentPotential;
        this.potentials = potentials;
        n_print = nPrint;
        this.temperature = temperature;
        n_ionic_steps = nIonicSteps;
        f_eps = fEps;
        max_iterations = maxIterations;
    }
}