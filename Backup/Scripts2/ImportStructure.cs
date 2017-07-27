using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;


public class ImportStructure : MonoBehaviour {
    //public GameObject AtomStructure;
    public TextAsset structureFile;
    public GameObject atomPrefab;
    public GameObject Settings;
    public GameObject BoundingBox;
    // get acces to 
    private GameObject newAtom;
    private GameSettings myGameSettings;
    private StringReader sr;
    private LocalElementData LED;
    private string line = "";
    private string[] data;
    //public List<Atom> Atoms;

    // Use this for initialization

    private void Awake()
    {
        myGameSettings = Settings.GetComponent<GameSettings>();
        LED = Settings.GetComponent<LocalElementData>();
    }

    void Start () {
        // Ausdehnung der Struktur ermitteln
        Vector3 minPositions = Vector3.one * Mathf.Infinity;
        Vector3 maxPositions = Vector3.one * Mathf.Infinity * -1;
        using (sr = new StringReader(structureFile.text))
        {
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    data = line.Split(' ');
                    for (int i = 0; i < 3; i++)
                    {
                        if (float.Parse(data[i]) - LED.getSize(data[3])/2 < minPositions[i])
                            minPositions[i] = float.Parse(data[i]) - LED.getSize(data[3])/2;
                        if (float.Parse(data[i]) + LED.getSize(data[3])/2 > maxPositions[i])
                            maxPositions[i] = float.Parse(data[i]) + LED.getSize(data[3])/2;
                    }
                }
                else
                    break;
            }
        }

        print("maxmin" + maxPositions + minPositions);
        BoundingBox.transform.localScale = (maxPositions - minPositions) * 1;

        using (sr = new StringReader(structureFile.text))
        {
            string line = "";
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    newAtom = Instantiate(atomPrefab);
                    newAtom.transform.parent = gameObject.transform;
                    data = line.Split(' ');
                    newAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])) - (maxPositions + minPositions)/2;
                    // need to check which type the atom is and decline its properties
                    newAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3].ToString());
                    newAtom.transform.localScale = Vector3.one * LED.getSize(data[3].ToString());

                }
                else
                    break;
            }
        }

        gameObject.transform.localScale = Vector3.one * myGameSettings.size;


        /*
        //for ()
        //print(structureFile.text);
        Type t = typeof(System.String);

        // Iterate over all the methods from the System.String class and display
        // return type and parameters.
        // This reveals all the things you can do with a String.
        foreach (MethodInfo mi in t.GetMethods())
        {
            System.String s = System.String.Format("{0} {1} (", mi.ReturnType, mi.Name);
            ParameterInfo[] pars = mi.GetParameters();

            for (int j = 0; j < pars.Length; j++)
            {
                s = String.Concat(s, String.Format("{0}{1}", pars[j].ParameterType, ((j == pars.Length - 1) ? "" : ", ")));
            }
            s = String.Concat(s, ")");
            Debug.Log(s);
        }
        */
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
