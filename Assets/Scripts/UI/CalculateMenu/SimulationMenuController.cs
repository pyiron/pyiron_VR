using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationMenuController : MenuController {
    internal static SimulationMenuController Inst;
    
    public static bool jobLoaded = false;

    private void Awake()
    {
        Inst = this;
    }

    public void OnModeStart()
    {
        // load if a md, minimize, or static calculation should be performed
//        string order = "";
//        string receivedData = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
        
        JobSettingsController.Inst.OnModeStart();
        ActionPanelController.Inst.UpdateButtons();
    }

    /*public void SetNIonicSteps(Dropdown dropdown)
    {
        string n_ionic_steps = dropdown.options[dropdown.value].text;
        print("Setting n_ionic_steps to " + dropdown.options[dropdown.value].text);
        // set the value
        string order = "n_ionic_steps = " + n_ionic_steps;
        PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.exec_l, order);
    }*/
    
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
        AnimationController.frame = 0;
        PythonExecuter.HandlePythonMsg(result);
        AnimationMenuController.Inst.SetState(true);
        //ModeController.inst.SetMode(Modes.Animate);
        // todo: maybe handle the result here, instead of calling PythonExecutor.HandlePythonMsg
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
                    data.force_conv + ", " + 
                    data.max_iterations + ", " + 
                    data.n_print + ", " + 
                    jobData.job_type + ", " + 
                    jobData.job_name + ", " + 
                    jobData.currentPotential + ")";
        }

        // load the new structure in another coroutine
        StartCoroutine(HandleLammpsLoad(order, PythonScript.Executor));
        
        AnimationController.waitForLoadedStruc = true;
        Deactivate();
        print("Wait begun");
        //lammpsIsMd = ModeData.currentMode.showTemp;
    }
}
