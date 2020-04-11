using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationMenuController : MenuController {
    internal static SimulationMenuController Inst;
    
    public static bool jobLoaded = false;

    public static string jobName;

    public static readonly string SHIFTED = "_shifted"; 

    private void Awake()
    {
        Inst = this;
    }

    public void OnModeStart()
    {
        string order = "load_job(None)";
        PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.exec_l, order);

        JobSettingsController.Inst.OnModeStart();

        if (SimulationModeManager.CurrMode == SimModes.MD)
        {
            MdMenuController.Inst.OnModeStart();
        }
        else if (SimulationModeManager.CurrMode == SimModes.MINIMIZE)
        {
            MinimizeMenuController.Inst.OnModeStart();
        }

        ActionPanelController.Inst.OnModeStart();

        AnimationMenuController.Inst.SetState(AnimationController.Inst.HasAnimationLoaded());
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
        //PythonExecutor.SendOrderAsync(PythonScript.Executor, PythonCommandType.eval, order);
        PythonExecuter.SendOrderAsync(receivingScript, PythonCommandType.eval_l, order);
        
        // remember the id of the request to wait for the right response id
        int taskNumIn = TCPClient.taskNumIn;
        
        // wait until the response to the send message has arrived
        yield return new WaitUntil(() => taskNumIn == TCPClient.taskNumOut);
        
        // get the response
        string result = TCPClient.returnedMsg;
        
        // handle the response
        Activate();
        
        StructureLoader.LoadAnimation(result);
        
        // AnimationController.frame = 0;
        // PythonExecuter.HandlePythonMsg(result);
        AnimationMenuController.Inst.SetState(true);
        //ModeController.inst.SetMode(Modes.Animate);
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
            MdData data = MdMenuController.Inst.GetData();
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
            MinimizeData data = MinimizeMenuController.Inst.GetData();
            order = "calculate_" + calculation + "(" +
                    data.f_eps + ", " + 
                    data.max_iterations + ", " + 
                    data.n_print + ", " + 
                    jobData.job_type + ", " + 
                    jobData.job_name + ", " + 
                    jobData.currentPotential + ")";
        }
        
        AnimationController.waitForLoadedStruc = true;
        Deactivate();

        // load the new structure in another coroutine
        StartCoroutine(HandleLammpsLoad(order, PythonScript.executor));
        
        print("Wait begun");
        //lammpsIsMd = ModeData.currentMode.showTemp;
    }

    public bool IsStructureShifted()
    {
        return jobName.EndsWith(SHIFTED);
    }
}
