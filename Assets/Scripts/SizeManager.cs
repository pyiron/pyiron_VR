using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file should manage the sizes of all the objects in the scene. At the moment it just scales
/// the structure on startup
/// </summary>
public class SizeManager : MonoBehaviour
{
    public GameObject structure;
    
    void Start()
    {
        UpdateStructureSize();
    }

    /// <summary>
    /// Sets all the size of the structure
    /// </summary>
    public void UpdateStructureSize()
    {
        structure.transform.localScale = Vector3.one * ProgramSettings.size;
    }
}
