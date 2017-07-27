using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Reflection;


public class ImportStructure : MonoBehaviour {
    public TextAsset structureFile;
    public GameObject AtomPrefab;
    public GameObject Settings;
    private GameObject newAtom;
    private GameSettings myGameSettings;
    private StringReader sr;
    private LocalElementData LED;
    private StructureData SD;
    private string line = "";
    private string[] data;
    //public List<Atom> Atoms;

    // Use this for initialization

    private void Awake()
    {
        myGameSettings = Settings.GetComponent<GameSettings>();
        LED = Settings.GetComponent<LocalElementData>();
        SD = gameObject.GetComponent<StructureData>();
    }

    void Start () {
        // Ausdehnung der Struktur ermitteln
        Vector3 minPositions = Vector3.one * Mathf.Infinity;
        Vector3 maxPositions = Vector3.one * Mathf.Infinity * -1;
        int atomCounter = 0;

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
                    atomCounter++;
                }
                else
                    break;
            }
        }
        SD.atomPositions = new Vector3[3];

        SD.minPositions = minPositions;
        SD.maxPositions = maxPositions;

        //BoundingBox.transform.localScale = maxPositions - minPositions;

        atomCounter = 0;
        using (sr = new StringReader(structureFile.text))
        {
            string line = "";
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    newAtom = Instantiate(AtomPrefab);
                    newAtom.transform.parent = gameObject.transform;
                    data = line.Split(' ');
                    newAtom.transform.position = new Vector3(float.Parse(data[0]), float.Parse(data[1]),
                        float.Parse(data[2])) - (maxPositions + minPositions)/2;
                    SD.atomPositions[atomCounter] = newAtom.transform.position;
                    SD.updateMaxAndMin(newAtom.transform);
                    // need to check which type the atom is and decline its properties
                    newAtom.GetComponent<Renderer>().material.color = LED.getColour(data[3].ToString());
                    newAtom.transform.localScale = Vector3.one * LED.getSize(data[3].ToString());
                    atomCounter++;

                }
                else
                    break;
            }
        }

        print(SD.maxPositions + " " + SD.minPositions);
        SD.updateBoundingBox();
        gameObject.transform.localScale = Vector3.one * myGameSettings.size;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
