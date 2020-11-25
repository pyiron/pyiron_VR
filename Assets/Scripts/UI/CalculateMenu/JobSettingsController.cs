using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JobSettingsController : MonoBehaviour
{
    public static JobSettingsController Inst;   
    
    public Dropdown jobTypeDropdown;
    public InputField jobNameField;
    public Dropdown potentialDropdown;


    private void Awake()
    {
        Inst = this;
    }
    
    public void OnModeStart(JobData jobData)
    {
        // update the type dropdown (lammps or vasp)
        for (int id = 0; id < jobTypeDropdown.options.Count; id++)
        {
            string typeOption = jobTypeDropdown.options[id].text;
            if (typeOption == jobData.job_type)
            {
                jobTypeDropdown.value = id;
                break;
            }
        }

        potentialDropdown.value = 0;
        
        // Update the potential dropdown. Show all potentials and which one is currently active
        potentialDropdown.options.Clear();
        for (int potId = 0; potId < jobData.potentials.Length; potId++)
//        foreach (string pot in jobData.potentials)
        {
            string pot = jobData.potentials[potId];
            potentialDropdown.options.Add(new Dropdown.OptionData(pot));
            if (pot == jobData.currentPotential)
            {
                potentialDropdown.value = potId;
            }
        }
        potentialDropdown.RefreshShownValue();
        
        // update the job name
        //jobNameField.text = jobData.job_name;
        jobNameField.text = SimulationMenuController.jobName;
        
        // set calculation type (md, minimize or static)
        SimulationModeManager.Inst.SetMode(jobData.calc_mode);
        
        // load all currently available jobs from pyiron
//        order = "job.list_potentials()";
//        string potentials = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
//        potentials = JsonHelper.WrapToClass(potentials, "data");
//        List<string> potentialsArr = JsonUtility.FromJson<StringList>(potentials).data;
//        potentialDropdown.options.Clear();
//        foreach (string pot in potentialsArr)
//        {
//            potentialDropdown.options.Add(new Dropdown.OptionData(pot));
//        }
//        
//        order = "job.structure.get_chemical_formula()";
//        string jobName = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
//        jobNameField.text = jobName;
    }

    public void GetData(ref JobData data)
    {
        // string calculationType = SimulationModeManager.CurrMode.ToString().ToLower();
        // string jobType = "'" + Utilities.GetStringValue(jobTypeDropdown) + "'";
        // string jobName = "'" + jobNameField.text + "'";
        // string potential = "'" + Utilities.GetStringValue(potentialDropdown) + "'";
        //
        // return new JobData(calcMode:calculationType, jobType:jobType, jobName:jobName, currentPotential:potential);
        
        data.calc_mode = SimulationModeManager.CurrMode.ToString().ToLower();
        data.job_type = "'" + Utilities.GetStringValue(jobTypeDropdown) + "'";
        data.job_name = "'" + jobNameField.text + "'";
        data.currentPotential = "'" + Utilities.GetStringValue(potentialDropdown) + "'";
    }
}




