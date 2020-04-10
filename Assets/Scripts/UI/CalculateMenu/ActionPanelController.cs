using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanelController : MonoBehaviour
{
    public static ActionPanelController Inst;
    
    [SerializeField]
    private Button CalculateButton;
    [SerializeField]
    private Button DeleteButton;

    private void Awake()
    {
        Inst = this;
    }

    public void OnModeStart()
    {
        CalculateButton.gameObject.SetActive(!SimulationMenuController.jobLoaded);
        DeleteButton.gameObject.SetActive(SimulationMenuController.jobLoaded);
    }

    public void OnCalculateBtnDown()
    {
        SimulationMenuController.jobLoaded = true;
        SimulationMenuController.Inst.CalculateNewJob();
        OnModeStart();
    }
    
    public void OnDeleteBtnDown()
    {
        SimulationMenuController.jobLoaded = false;
        
        // set the structure to the current position data
        // TODO
        
        // TODO: Reactivate when it is possible to create a new job (including the base)
        string order = "project.remove_job(" + JobSettingsController.Inst.GetData().job_name + ")";
        PythonExecuter.SendOrderSync(PythonScript.UnityManager, PythonCommandType.exec_l, order);
        OnModeStart();
        //ModeController.inst.SetMode(Modes.Animate);
        //AnimationMenuController.Inst.SetState(true);
    }

    public void OnRefreshDown()
    {
        // dont know what to do here
    }
}
