using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundingbox : MonoBehaviour
{
    public static Boundingbox Inst;
    
    public GameObject borderPrefab;

    private GameObject[] _borders = new GameObject[12];

    public Vector3 mid;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        // create the instance of the boundingbox
//        StructureDataOld.Inst.boundingbox = Instantiate(BoundingboxPrefab, gameObject.transform, true);

        // show the controllers the reference to the boundingbox
//        foreach (LaserGrabber lg in LaserGrabber.instances)
//            lg.boundingbox = StructureDataOld.Inst.boundingbox.transform;

        // create the cubes for the cell box and the parent cellBox
//        Cellbox = new GameObject();
//        Cellbox.transform.parent = transform;
//        Cellbox.name = "Cellbox";
        for (int i = 0; i < 12; i++)
        {
            _borders[i] = Instantiate(borderPrefab);
            _borders[i].transform.parent = transform;
            _borders[i].transform.localScale = Vector3.one * ProgramSettings.cellboxWidth;
        }
        
        gameObject.SetActive(false);
    }

    public void UpdateBoundingBox(Vector3[] data)
    {
        print(data);
        
        // reset the positions of the cellbox
        transform.localPosition = Vector3.zero;

//        Vector3[] cellboxData = StructureDataOld.GetCurrFrameData().cellbox;
        //set the position and length for each part of the cellbox
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 cellBorderSize = _borders[j * 4 + i].transform.localScale;
                cellBorderSize[j] = data[j].magnitude + ProgramSettings.cellboxWidth;
                _borders[j * 4 + i].transform.localScale = cellBorderSize;

                _borders[j * 4 + i].transform.localPosition = data[j] * 0.5f;
                if (i == 1 || i == 3)
                    _borders[j * 4 + i].transform.localPosition += data[(j + 1) % 3];
                if (i == 2 || i == 3)
                    _borders[j * 4 + i].transform.localPosition += data[(j + 2) % 3];
            }
        }
        
        mid = new Vector3(data[0][0], data[1][1], data[2][2]);
    }
}
