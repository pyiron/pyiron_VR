using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationMenuController : MenuController {
    internal static SimulationMenuController inst;
    public Button SimulationButton;
    private Text _simBtnText;
    public static bool ShouldReload = false;

    private void Awake()
    {
        inst = this;
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
    
    // TODO: Deactivate and activate UI Elements
    private IEnumerator HandleLammpsLoad(string order)
    {
        // send the order to execute lammps
        //PythonExecutor.SendOrderAsync(PythonScript.Executor, PythonCommandType.eval, order);
        PythonExecuter.SendOrderAsync(PythonScript.Executor, PythonCommandType.eval_l, order);
        
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
        ModeData.inst.SetMode(Modes.Animate);
        // todo: maybe handle the result here, instead of calling PythonExecutor.HandlePythonMsg
    } 

    private void LoadNewLammps()
    {
        // update the temperture
        PythonExecuter.SendOrderSync(PythonScript.None, PythonCommandType.exec_l,
            "unity_manager.Executor.temperature = " + Thermometer.temperature);
        //string calculation = ModeData.currentMode.showTemp ? "md" : "minimize";
        string calculation = "unity_manager.Executor.create_new_lammps('" + 
                             SimulationModeManager.CurrMode.ToString().ToLower() + "')";
        
        // load the new structure in another coroutine
        StartCoroutine(HandleLammpsLoad(calculation));
        
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
            LoadNewLammps();
        }
        else
        {
            ModeData.inst.SetMode(Modes.Animate);
        }
    }
}
