using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimizeMenuController : MenuController
{
    public static MinimizeMenuController Inst;
    
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
        
    }

    public MinimizeData GetData()
    {
        string force_conv = "";
        string max_iterations = "";
        string n_print = "";
        foreach (Dropdown dropdown in _dropdowns)
        {
            if (dropdown.transform.parent.name == "force_conv")
            {
                force_conv = dropdown.options[dropdown.value].text;
            } 
            else if (dropdown.transform.parent.name == "max_iterations")
            {
                max_iterations = dropdown.options[dropdown.value].text;
            }
            else if (dropdown.transform.parent.name == "n_print")
            {
                n_print = dropdown.options[dropdown.value].text;
            }
        }
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
