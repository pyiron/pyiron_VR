using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizeMenuController : MenuController
{
    public static MinimizeMenuController Inst;

    public Dropdown forceConvDropdown;
    public Dropdown maxIterationsDropdown;
    public Dropdown nPrintDropdown;
    
    private Dropdown[] _dropdowns;

    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _dropdowns = GetComponentsInChildren<Dropdown>();
    }
    
    public void OnModeStart()
    {
        string order = "format_minimize_settings()";
        string receivedData = PythonExecuter.SendOrderSync(PythonScript.executor, PythonCommandType.eval_l, order);
        JobData minimizeData = JsonUtility.FromJson<JobData>(receivedData);

        if (minimizeData.f_eps != null)
        {
            Utilities.SetDropdownValue(forceConvDropdown, minimizeData.f_eps);
        }

        Utilities.SetDropdownValue(maxIterationsDropdown, minimizeData.max_iterations);

        if (minimizeData.n_print != null)
        {
            Utilities.SetDropdownValue(nPrintDropdown, minimizeData.n_print);
        }
    }

    public JobData GetData()
    {
        string f_eps = Utilities.GetStringValue(forceConvDropdown);
        string max_iterations = Utilities.GetStringValue(maxIterationsDropdown);
        string n_print = Utilities.GetStringValue(nPrintDropdown);
        return new JobData(fEps:f_eps, maxIterations:max_iterations, nPrint:n_print);
    }
}

// public struct MinimizeData
// {
//     public string f_eps;
//     public string max_iterations;
//     public string n_print;
//
//     public MinimizeData(string f_eps, string maxIterations, string nPrint)
//     {
//         this.f_eps = f_eps;
//         this.max_iterations = maxIterations;
//         this.n_print = nPrint;
//     }
// }
