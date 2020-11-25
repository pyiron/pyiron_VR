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
    
    public void OnModeStart(JobData jobData)
    {
        if (jobData.f_eps != null)
        {
            Utilities.SetDropdownValue(forceConvDropdown, jobData.f_eps);
        }

        Utilities.SetDropdownValue(maxIterationsDropdown, jobData.max_iterations);

        if (jobData.n_print != null)
        {
            Utilities.SetDropdownValue(nPrintDropdown, jobData.n_print);
        }
    }

    public void GetData(ref JobData data)
    {
        // string f_eps = Utilities.GetStringValue(forceConvDropdown);
        // string max_iterations = Utilities.GetStringValue(maxIterationsDropdown);
        // string n_print = Utilities.GetStringValue(nPrintDropdown);
        // return new JobData(fEps:f_eps, maxIterations:max_iterations, nPrint:n_print);
        data.f_eps = Utilities.GetStringValue(forceConvDropdown);
        data.max_iterations = Utilities.GetStringValue(maxIterationsDropdown);
        data.n_print = Utilities.GetStringValue(nPrintDropdown);
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
