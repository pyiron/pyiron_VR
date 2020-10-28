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

    public void UpdateButtons(bool jobExists)
    {
        CalculateButton.gameObject.SetActive(!jobExists);
        DeleteButton.gameObject.SetActive(jobExists);
    }

    public void OnCalculateBtnDown()
    {
        SimulationMenuController.Inst.CalculateNewJob();
    }
    
    public void OnDeleteBtnDown(bool saveStructure=true)
    {
        UpdateButtons(false);

        if (saveStructure)
        {
            AnimationController.Inst.DeleteAnimation();
        }

        ExplorerMenuController.Inst.DeleteJob();
    }
}
