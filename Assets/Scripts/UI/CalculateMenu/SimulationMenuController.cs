using System;
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
        //string order = "format_job_settings()";
        string data = PythonExecutor.SendOrderSync(true, PythonCmd.FormatJobSettings);
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
                    /*string job = PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.eval_l, 
                        "load_job(" + PythonScript.unityManager + ".project['"+ jobName + "'])");
                    OnJobLoaded(job);*/
                    PythonExecutor.SendOrderAsync(true,
                        PythonCmd.LoadJob(jobName), OnJobLoaded);
                }
            }
            else
            {
                // create a new job, then load the information from it
                PythonExecutor.SendOrderAsync(false, PythonCmd.LoadNoneJob,
                    s => UpdatePanels());
                //PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l, "load_job(None)");
                //UpdatePanels();
            }
        }
    }

    private void OnJobLoaded(string job)
    {
        StructureLoader.LoadAnimation(job);
        // Load the information of the old job. The structure should have been set in Explorer already
        // PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l, order);
        UpdatePanels();
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

    private void OnJobdataReceived(string data)
    {
        // handle the response
        Activate();
        
        ActionPanelController.Inst.UpdateButtons(true);

        StructureLoader.LoadAnimation(data);
    }

    public void CalculateNewJob()
    {
        JobData jobData = new JobData();
        JobSettingsController.Inst.GetData(ref jobData);
        if (jobData.calc_type == "md")
        {
            MdMenuController.Inst.GetData(ref jobData);
        }
        else
        {
            MinimizeMenuController.Inst.GetData(ref jobData);
        }

        Deactivate();

        string order = PythonCmd.CalculateJob(jobData);

        // load the new structure in another coroutine
        PythonExecutor.SendOrderAsync(true, order, OnJobdataReceived);

        print("Job calculation started");
    }

    public bool IsStructureShifted()
    {
        return jobName.EndsWith(SHIFTED);
    }
}

[Serializable]
public struct JobData
{
    // data for all calculation modes
    // is and should the job be calculated using minimize or md (or static, not implemented)
    public string calc_type;
    // is the job Lammps or Vasp?
    public string job_type;
    // the name under which the current job is saved
    public string job_name;
    // the potential that is or should currently be used
    public string currentPotential;
    // all potentials that can be used for the current structure
    public string[] potentials;
    
    // data for md and minimize
    public string n_print;
    
    // data for md
    public int temperature;
    public string n_ionic_steps;
    
    // data for minimize
    public string f_eps;
    public string max_iterations;

    // private void SetAllToNull()
    // {
    //     this.calc_mode = null;
    //     job_type = null;
    //     job_name = null;
    //     this.currentPotential = null;
    //     this.potentials = null;
    //     n_print = null;
    //     temperature = 0;
    //     n_ionic_steps = null;
    //     f_eps = null;
    //     max_iterations = null;
    // }
    //
    // public JobData(string calcMode=null, string jobType=null, string jobName=null, string currentPotential=null,
    //     string[] potentials=null, string nPrint=null, int temperature=0, string nIonicSteps=null, string fEps=null,
    //     string maxIterations=null, string calcType=null)
    // {
    //     calc_mode = calcMode;
    //     job_type = jobType;
    //     job_name = jobName;
    //     this.currentPotential = currentPotential;
    //     this.potentials = potentials;
    //     n_print = nPrint;
    //     this.temperature = temperature;
    //     n_ionic_steps = nIonicSteps;
    //     f_eps = fEps;
    //     max_iterations = maxIterations;
    //     calc_type = calcType;
    // }
}