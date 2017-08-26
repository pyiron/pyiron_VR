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
        {"FastForward 0", new Symbol(new Vector3(0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 5) },
        {"FastForward 1", new Symbol(new Vector3(-0.008f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 0) },
        {"FrameForward 0", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false, -2) },
        {"FrameForward 1", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false, -3) },
        {"Triangle 2", new Symbol(new Vector3(0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 4) },
        {"Triangle 1", new Symbol(new Vector3(-0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 1) },
        {"TimelapsForward 0", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 2) },
        {"TimelapsForward 1", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 3) }
    };

    // Use this for initialization
    void Start () {
        //AnimSymbols = Resources.LoadAll("ControllerSymbols", typeof(GameObject)) as GameObject[];
        int objectCounter = 0;
        foreach (string symbolKey in controllerSymbols.Keys)
        {
            AnimSymbols[objectCounter] = Instantiate(Resources.Load("ControllerSymbols/" + symbolKey.Split()[0]) as GameObject);
            AnimSymbols[objectCounter].transform.parent = transform;
            AnimSymbols[objectCounter].transform.eulerAngles = controllerSymbols[symbolKey].m_rotation;
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
        foreach (GameObject AnimSymbol in AnimSymbols)
        {

            /*if (PE.pythonRunsAnim)
                if (AnimSymbol.name.Contains("Pause"))
                    AnimSymbol.SetActive(true);
                else if (PE.pythonsAnimSpeed == 0)
                    if (AnimSymbol.name.Contains("Triangle 1"))
                        AnimSymbol.SetActive(true);
                    else;
                else if (PE.pythonsAnimSpeed == 1)
                    if (AnimSymbol.name.Contains("Fast Forward 1") || AnimSymbol.name.Contains("TimelapsForward 1"))
                        AnimSymbol.SetActive(true);
                    else;
                else if (PE.pythonsAnimSpeed == 4)
                    if (AnimSymbol.name.Contains("Fast Forward 0") || AnimSymbol.name.Contains("TimelapsForward 0"))
                        AnimSymbol.SetActive(true);
                    else;
                else if (PE.pythonsAnimSpeed == 5)
                    if (AnimSymbol.name.Contains("Triangle 2"))
                        AnimSymbol.SetActive(true);
                    else;
                else
                    AnimSymbol.SetActive(false);
            else
                if (AnimSymbol.name.Contains("Triangle 0"))
                    AnimSymbol.SetActive(true);
                else
                    AnimSymbol.SetActive(false);*/


            /*else if (PE.pythonRunsAnim && PE.pythonsAnimSpeed == 3 && AnimSymbol.name.Contains("Triangle 2"))
                AnimSymbol.SetActive(true);
            else if (PE.pythonRunsAnim && PE.pythonsAnimSpeed == 2 && AnimSymbol.name.Contains("Triangle 1"))
                AnimSymbol.SetActive(true);
            else
                AnimSymbol.SetActive(false);*/
        }
    }
}
