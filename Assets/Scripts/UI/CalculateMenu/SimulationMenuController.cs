using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SimulationMenuController : MenuController {
    internal static SimulationMenuController Inst;
    
    [SerializeField]
    private Button SimulationButton;
    private Text _simBtnText;
    
    public static bool ShouldReload = false;

    private void Awake()
    {
        Inst = this;
        _simBtnText = SimulationButton.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        SimulationButton.interactable = SimulationModeManager.CurrMode != SimModes.None || !ShouldReload;
        if (ShouldReload)
        {
            _simBtnText.text = "Start Simulation";
        }
        else
        {
            _simBtnText.text = "Show Animation";
        }
    }

    public void OnModeStart()
    {
        JobSettingsController.Inst.OnModeStart();
    }

    public void SetNIonicSteps(Dropdown dropdown)
    {
        string n_ionic_steps = dropdown.options[dropdown.value].text;
        print("Setting n_ionic_steps to " + dropdown.options[dropdown.value].text);
        // set the value
        string order = "n_ionic_steps = " + n_ionic_steps;
        PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.exec_l, order);
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
        base.Activate();
        AnimationController.frame = 0;
        PythonExecuter.HandlePythonMsg(result);
        AnimationMenuController.Inst.SetState(true);
        //ModeController.inst.SetMode(Modes.Animate);
        // todo: maybe handle the result here, instead of calling PythonExecutor.HandlePythonMsg
    } 

    private void CalculateNewJob()
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
            MdData data = TemperatureMenuController.Inst.GetData();
            order = "calculate_" + calculation + "(" +
                               data.temperature + ", " + 
                               data.n_ionic_steps + ", " + 
                               data.n_print + ", " + 
                               jobData.job_type + ", " + 
                               jobData.job_name + ", " + 
                               jobData.potential + ")";
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
                    jobData.potential + ")";
        }

        // load the new structure in another coroutine
        StartCoroutine(HandleLammpsLoad(order, PythonScript.Executor));
        
        AnimationController.waitForLoadedStruc = true;
        base.Deactivate();
        ShouldReload = false;
        print("Wait begun");
        //lammpsIsMd = ModeData.currentMode.showTemp;
    }

    public void StartSimulation()
    {
        if (_simBtnText.text == "Start Simulation")
        {
            CalculateNewJob();
        }
        else
        {
            //ModeController.inst.SetMode(Modes.Animate);
            AnimationMenuController.Inst.SetState(true);
        }
    }
}
