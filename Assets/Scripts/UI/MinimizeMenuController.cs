using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimizeMenuController : MenuController
{
    public static MinimizeMenuController Inst;

    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public MinimizeData GetData()
    {
        throw new NotImplementedException();
    }
}

public struct MinimizeData
{
    public float force_conv;
    public string max_iterations;
    public string n_print;

    public MinimizeData(float forceConv, string maxIterations, string nPrint)
    {
        force_conv = forceConv;
        max_iterations = maxIterations;
        n_print = nPrint;
    }
}
