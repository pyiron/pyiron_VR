using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;


public class ImportStructure : MonoBehaviour {
    public GameObject AtomStructure;
    public TextAsset structureFile;
    public GameObject atomPrefab;
    public GameObject Settings;
    // get acces to 
    private GameObject newAtom;
    private GameSettings myGameSettings;
    //public List<Atom> Atoms;

    // Use this for initialization

    private void Awake()
    {
        myGameSettings = Settings.GetComponent<GameSettings>();
        print("the name: " + Settings.GetComponent<LocalElementData>().getCasNumber("O"));
    }

    void Start () {
        using (StringReader sr = new StringReader(structureFile.text))
        {
            string line = "";
            string[] data;
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    newAtom = Instantiate(atomPrefab);
                    newAtom.transform.parent = AtomStructure.transform;
                    data = line.Split(' ');
                    newAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    // need to check which type the atom is and decline its properties
                    newAtom.GetComponent<Renderer>().material.color = Settings.GetComponent<LocalElementData>().getColour(data[3].ToString());
                    newAtom.transform.localScale = Vector3.one * Settings.GetComponent<LocalElementData>().getSize(data[3].ToString());

                }
                else
                    break;
            }
        }

        AtomStructure.transform.localScale = Vector3.one * myGameSettings.size;


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
