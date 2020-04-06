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
        string receivedData = PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval_l, order);
        MinimizeData minimizeData = JsonUtility.FromJson<MinimizeData>(receivedData);

        if (minimizeData.force_conv != null)
        {
            Utilities.SetDropdownValue(forceConvDropdown, minimizeData.force_conv);
        }

        Utilities.SetDropdownValue(maxIterationsDropdown, minimizeData.max_iterations);

        if (minimizeData.n_print != null)
        {
            Utilities.SetDropdownValue(nPrintDropdown, minimizeData.n_print);
        }
    }

    public MinimizeData GetData()
    {
        string force_conv = Utilities.GetStringValue(forceConvDropdown);
        string max_iterations = Utilities.GetStringValue(maxIterationsDropdown);
        string n_print = Utilities.GetStringValue(nPrintDropdown);
        return new MinimizeData(force_conv, max_iterations, n_print);
    }
}

public struct MinimizeData
{
    public string force_conv;
    public string max_iterations;
    public string n_print;

    public MinimizeData(string forceConv, string maxIterations, string nPrint)
    {
        force_conv = forceConv;
        max_iterations = maxIterations;
        n_print = nPrint;
    }
}
