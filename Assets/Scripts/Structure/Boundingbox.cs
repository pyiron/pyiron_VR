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
        
        for (int i = 0; i < 12; i++)
        {
            _borders[i] = Instantiate(borderPrefab);
            _borders[i].transform.parent = transform;
            _borders[i].transform.localScale = Vector3.one * ProgramSettings.cellboxWidth;
        }
        
        gameObject.SetActive(false);
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
    }

    public void UpdateBoundingBox(Vector3[] data)
    {
        print(data);
        
        // reset the positions of the cellbox
        transform.localPosition = Vector3.zero;

        //set the position and length for each part of the cellbox
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 newSize = Vector3.one * ProgramSettings.cellboxWidth;
                newSize[2] = data[j].magnitude + ProgramSettings.cellboxWidth;
//                print(j * 4 + i);
//                _borders[j * 4 + i].transform.localScale = newSize;
                _borders[j + i * 3].transform.localScale = newSize;
                
//                Vector3 cellBorderSize = _borders[j * 4 + i].transform.localScale;
//                cellBorderSize[j] = data[j].magnitude + ProgramSettings.cellboxWidth;
//                _borders[j * 4 + i].transform.localScale = cellBorderSize;

//                _borders[j * 4 + i].transform.localPosition = data[j] * 0.5f;
//                if (i == 1 || i == 3)
//                    _borders[j * 4 + i].transform.localPosition += data[(j + 1) % 3];
//                if (i == 2 || i == 3)
//                    _borders[j * 4 + i].transform.localPosition += data[(j + 2) % 3];
            }
        }

//        _borders[0].transform.localPosition = data[0] * 0.5f;
//        _borders[1].transform.localPosition = data[0] * 0.5f + data[1];
//        _borders[2].transform.localPosition = data[0] * 0.5f + data[2];
//        _borders[3].transform.localPosition = data[0] * 0.5f + data[1] + data[2];
//        
//        _borders[4].transform.localPosition = data[1] * 0.5f;
//        _borders[5].transform.localPosition = data[1] * 0.5f + data[0];
//        _borders[6].transform.localPosition = data[1] * 0.5f + data[2];
//        _borders[7].transform.localPosition = data[1] * 0.5f + data[0] + data[2];
//        
//        _borders[8].transform.localPosition = data[2] * 0.5f;
//        _borders[9].transform.localPosition = data[2] * 0.5f + data[1];
//        _borders[10].transform.localPosition = data[2] * 0.5f + data[0];
//        _borders[11].transform.localPosition = data[2] * 0.5f + data[1] + data[0];

        int[] signs = {1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1};

        Vector3 edgePose = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            if (i == 1)
            {
                edgePose = data[0] + data[1];
            }
            else if (i == 2)
            {
                edgePose = data[0] + data[2];
            }
            else if (i == 3)
            {
                edgePose = data[1] + data[2];
            }

            for (int j = 0; j < 3; j++)
            {
                _borders[i * 3 + j].transform.localPosition = edgePose;
                Vector3 newPos = edgePose + data[j] / 2f * signs[i * 3 + j];
                _borders[i * 3 + j].transform.LookAt(newPos);
                _borders[i * 3 + j].transform.localPosition = newPos;
                
                
//                Vector3 localPos = _borders[i * 3 + j].transform.localPosition;
//                localPos[j % 3] += _borders[i * 3 + j].transform.localScale.z / 2f;
//                _borders[i * 3 + j].transform.localPosition = localPos;
                
//                _borders[i * 3 + j].transform.localPosition +=
//                    Vector3.right * _borders[i * 3 + j].transform.localScale.z;
            }
        }
        
        mid = new Vector3(data[0][0], data[1][1], data[2][2]);
    }
}
