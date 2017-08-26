using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSymbols : MonoBehaviour {
    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;

    private GameObject[] AnimSymbols = new GameObject[8];

    private static readonly Dictionary<string, Symbol> controllerSymbols = new Dictionary<string, Symbol>
    {
        {"Triangle 0", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Pause 0", new Symbol(new Vector3(0.000f, 0.002f, -0.049f), new Vector3(-90, 0, 0), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 1", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 2", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 3", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 4", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 5", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) },
        {"Triangle 6", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1)) }
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
            if (PE.pythonRunsAnim && AnimSymbol.name.Contains("Pause"))
                AnimSymbol.SetActive(true);
            else if (!PE.pythonRunsAnim && AnimSymbol.name.Contains("Triangle"))
                AnimSymbol.SetActive(true);
            else if (PE.pythonRunsAnim && PE.pythonsAnimSpeed == 4 && AnimSymbol.name.Contains("Fast Forward"))
                AnimSymbol.SetActive(true);
            else
                AnimSymbol.SetActive(false);
    }
}
