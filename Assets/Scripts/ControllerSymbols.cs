using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSymbols : MonoBehaviour {
    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;

    private GameObject[] AnimSymbols = new GameObject[10];

    private static readonly Dictionary<string, Symbol> controllerSymbols = new Dictionary<string, Symbol>
    {
        {"Triangle 0", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"Pause 0", new Symbol(new Vector3(0.000f, 0.00f, -0.049f), new Vector3(-90, 0, 0), 0.005f, new Color(1, 1, 1, 1), true) },
        {"FastForward 0", new Symbol(new Vector3(-0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 5, 0.028f) },
        {"FastForward 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 0) },
        {"FrameForward 0", new Symbol(new Vector3(0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"FrameForward 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"Triangle 2", new Symbol(new Vector3(-0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 4, 0.028f) },
        {"Triangle 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 1, 0.028f) },
        {"TimelapsForward 0", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.008f, new Color(1, 1, 1, 1), true, 3, 0.028f) },
        {"TimelapsBackward 0", new Symbol(new Vector3(-0.012f, 0.0035f, -0.049f), new Vector3(-90, 0 -90), 0.008f, new Color(1, 1, 1, 1), true, 2, 0.028f) }
    };

    // Use this for initialization
    void Start () {
        //AnimSymbols = Resources.LoadAll("ControllerSymbols", typeof(GameObject)) as GameObject[];
        int objectCounter = 0;
        foreach (string symbolKey in controllerSymbols.Keys)
        {
            AnimSymbols[objectCounter] = Instantiate(Resources.Load("ControllerSymbols/" + symbolKey.Split()[0]) as GameObject);
            AnimSymbols[objectCounter].name = symbolKey;
            AnimSymbols[objectCounter].transform.parent = transform;
            AnimSymbols[objectCounter].transform.eulerAngles = controllerSymbols[symbolKey].m_rotation;
            if (symbolKey.Split()[1] == "1")
                AnimSymbols[objectCounter].transform.eulerAngles += Vector3.forward * 180;
            AnimSymbols[objectCounter].transform.localPosition = controllerSymbols[symbolKey].m_position;
            AnimSymbols[objectCounter].transform.localScale = Vector3.one * controllerSymbols[symbolKey].m_size;
            objectCounter += 1;
        }
        SetSymbol();
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    public void SetSymbol()
    {
        Symbol symbolProperties;
        foreach (GameObject AnimSymbol in AnimSymbols)
        {
            symbolProperties = controllerSymbols[AnimSymbol.name];

            // check that the rotation of the symbols is right
            if (AnimSymbol.transform.eulerAngles != symbolProperties.m_rotation)
                AnimSymbol.transform.eulerAngles = symbolProperties.m_rotation;

            // activate the symbols that have to be activated and deactivate the remaining symbols
            if (symbolProperties.m_showWhenAnimRuns == PE.pythonRunsAnim)
                if (PE.pythonRunsAnim)
                    if (symbolProperties.m_animSpeed == -1)
                        AnimSymbol.SetActive(true);
                    else if (symbolProperties.m_animSpeed == PE.pythonsAnimSpeed - 1)
                    {
                        AnimSymbol.SetActive(true);
                        AnimSymbol.transform.localPosition = symbolProperties.m_position;
                    }
                    else if (symbolProperties.m_animSpeed == PE.pythonsAnimSpeed + 1)
                    {
                        AnimSymbol.SetActive(true);
                        AnimSymbol.transform.localPosition = symbolProperties.m_position + Vector3.right * symbolProperties.m_positionRight;
                    }
                    else
                        AnimSymbol.SetActive(false);
                else
                    AnimSymbol.SetActive(true);
            else
                AnimSymbol.SetActive(false);
        }
    }
}
