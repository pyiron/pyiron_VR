using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobSettingsController : MonoBehaviour
{
    public static JobSettingsController Inst;   
    
    private Dropdown[] _dropdowns;
    private InputField _jobNameField;


    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _dropdowns = GetComponentsInChildren<Dropdown>();
        _jobNameField = GetComponentInChildren<InputField>();
    }

    // might not be needed
    public void SetJobName(string newName)
    {
        _jobNameField.text = newName;
    }

    public JobData GetData()
    {
        string job_type = "";
        string job_name = "'" + _jobNameField.text + "'";
        string potential = "";
        foreach (Dropdown dropdown in _dropdowns)
        {
            if (dropdown.transform.parent.name == "job_type")
            {
                job_type = "'" + dropdown.options[dropdown.value].text + "'";
            } 
            else if (dropdown.transform.parent.name == "potential")
            {
                potential = "'" + dropdown.options[dropdown.value].text + "'";
            }
        }
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
