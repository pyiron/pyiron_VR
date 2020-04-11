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
        UpdateButtons(SimulationMenuController.Inst.CheckJobExists());
    }

    private void UpdateButtons(bool jobExists)
    {
        CalculateButton.gameObject.SetActive(!jobExists);
        DeleteButton.gameObject.SetActive(jobExists);
    }

    public void OnCalculateBtnDown()
    {
        SimulationMenuController.jobLoaded = true;
        SimulationMenuController.Inst.CalculateNewJob();
        UpdateButtons(true);
    }
    
    public void OnDeleteBtnDown()
    {
        SimulationMenuController.jobLoaded = false;
        
        
        // TODO: Remember the current frame, stop the animation and delete the position data (I think it is not needed anymore)
        
        // TODO: Reactivate when it is possible to create a new job (including the base)
        //string order = "project.remove_job(" + JobSettingsController.Inst.GetData().job_name + ")";
        //PythonExecuter.SendOrderSync(PythonScript.unityManager, PythonCommandType.exec_l, order);
        UpdateButtons(false);
        //ModeController.inst.SetMode(Modes.Animate);
        
        AnimationController.Inst.DeleteAnimation();

        ExplorerMenuController.Inst.DeleteJob(SimulationMenuController.jobName);
    }

    public void OnRefreshDown()
    {
        // dont know what to do here
    }
}
