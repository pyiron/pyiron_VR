using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSymbols : MonoBehaviour {
    // get the reference to the programm which handles the execution of python
    public PythonExecuter PE;

    private GameObject[] AnimSymbols = new GameObject[7];

	// Use this for initialization
	void Start () {
        AnimSymbols = Resources.LoadAll("ControllerSymbols", typeof(GameObject)) as GameObject[];
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetSymbol()
    {
        /*foreach (GameObject AnimSymbol in AnimSymbols)
        {
            if (PE.pythonRunsAnim && AnimSymbol.name.Contains("Pause"))
                AnimSymbol.SetActive(true);
            else if (!PE.pythonRunsAnim && AnimSymbol.name.Contains("Triangle"))
                AnimSymbol.SetActive(true);
            else if (PE.pythonRunsAnim && PE.pythonsAnimSpeed == 4 && AnimSymbol.name.Contains("Fast Forward"))
                AnimSymbol.SetActive(true);
            else
                AnimSymbol.SetActive(false);
        }*/
    }
}
