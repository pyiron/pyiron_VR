using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobSettingsController : MonoBehaviour
{
    public static JobSettingsController Inst;   
    
    public Dropdown jobType;
    public InputField jobNameField;
    public Dropdown potentialDropdown;


    private void Awake()
    {
        Inst = this;
    }
    
    public void OnModeStart()
    {
        // load all currently available jobs from pyiron
        string order = "pr.list_potentials()";
        string potentials = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
        potentials = JsonHelper.WrapToClass(potentials, "data");
        List<string> potentialsArr = JsonUtility.FromJson<StringList>(potentials).data;
        potentialDropdown.options.Clear();
        foreach (string pot in potentialsArr)
        {
            potentialDropdown.options.Add(new Dropdown.OptionData(pot));
        }
        
        order = "pr.structure.get_chemical_formula()";
        string jobName = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
        jobNameField.text = jobName;
    }

    public JobData GetData()
    {
        string job_type = "'" + jobType.options[jobType.value].text + "'";
        string job_name = "'" + jobNameField.text + "'";
        string potential = "'" + potentialDropdown.options[potentialDropdown.value].text + "'";
      
        return new JobData(job_type, job_name, potential);
    }
}

public struct JobData
{
    public string job_type;
    public string job_name;
    public string potential;

    public JobData(string jobType, string jobName, string potential)
    {
        job_type = jobType;
        job_name = jobName;
        this.potential = potential;
    }
}


