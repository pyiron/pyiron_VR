using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeManager : MonoBehaviour
{
    public GameObject structure;
    
    // Start is called before the first frame update
    void Start()
    {
        structure.transform.localScale = Vector3.one * ProgramSettings.size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
