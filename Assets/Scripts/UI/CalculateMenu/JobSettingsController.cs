using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public void OnModeStart()
    {
        string order = "format_job_settings()";
        string data = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
        JobData jobData = JsonUtility.FromJson<JobData>(data);
        
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
                break;
            }
        }
        
        // update the job name
        jobNameField.text = jobData.job_name;
        
        // maybe set type
        
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

    public JobData GetData()
    {
        string jobType = "'" + jobTypeDropdown.options[jobTypeDropdown.value].text + "'";
        string jobName = "'" + jobNameField.text + "'";
        string potential = "'" + potentialDropdown.options[potentialDropdown.value].text + "'";
      
        return new JobData(jobType, jobName, potential, null);
    }
}

public struct JobData
{
    public string job_type;
    public string job_name;
    public string currentPotential;
    public string[] potentials;

    public JobData(string jobType, string jobName, string currentPotential, string[] potentials)
    {
        job_type = jobType;
        job_name = jobName;
        this.currentPotential = currentPotential;
        this.potentials = potentials;
    }
}


