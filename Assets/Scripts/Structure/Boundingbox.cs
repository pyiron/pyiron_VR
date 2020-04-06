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
        // show the controllers the reference to the boundingbox
        foreach (LaserGrabber lg in LaserGrabber.instances)
            lg.boundingbox = transform;
    }

    public void UpdateBoundingBox(Vector3[] data)
    {
        // reset the positions of the cellbox
        transform.localPosition = Vector3.zero;

        //set the position and length for each part of the cellbox
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector3 newSize = Vector3.one * ProgramSettings.cellboxWidth;
                newSize[2] = data[j].magnitude;
                _borders[j + i * 3].transform.localScale = newSize;
            }
        }

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
                Vector3 targetWorldPos = newPos * ProgramSettings.size + transform.parent.localPosition;
                _borders[i * 3 + j].transform.LookAt(targetWorldPos);
                _borders[i * 3 + j].transform.localPosition = newPos;
            }
        }
        
        mid = (data[0] + data[1] +  data[2]) / 2f;

        CellboxCollider.Inst.SetCollider(data);
        
        // set the position of the Hourglass to the middle of the cellbox
        HourglassActivator.Inst.transform.position = mid;
        
        // not sure if this line is still needed
        HourglassActivator.Inst.transform.GetChild(0).localScale = Vector3.one;
    }

    
}
