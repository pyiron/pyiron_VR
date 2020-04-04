using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    public static Structure Inst;

    public GameObject atomPrefab;
    
    private List<GameObject> atoms = new List<GameObject>();


    private void Awake()
    {
        Inst = this;
    }
    
    private void AdjustStructureSize(int diff)
    {
        // if the old structure has more atoms than the new, delete all redundant atoms
        for (int i = 0; i < diff; i++)
        {
            GameObject atom = atoms[0];
            atoms.Remove(atom);
            Destroy(atom);
        }
        
        // if the old structure has less atoms than the new, create new Atoms
        for (int i = 0; i > diff; i--)
        {
            atoms.Add(Instantiate(atomPrefab, transform, false));
        }
    }

    public void UpdateStructure(Vector3[] positions)
    {
        UpdateStructure(positions, null);
    }
    
    
    public void UpdateStructure(Vector3[] positions, string[] elements)
    {
        AdjustStructureSize(atoms.Count - positions.Length);
        
        // set the positions and elements
        for (int i = 0; i < positions.Length; i++)
        {
            // set the atom to its position
            atoms[i].transform.localPosition = positions[i];

            if (elements != null)
            {
                // set the atom color to the color the element of the atom has
                atoms[i].GetComponent<Renderer>().material.color = LocalElementData.GetColour(elements[i]);

                // set the atoms size to the size this type of atom has 
                atoms[i].transform.localScale = Vector3.one * LocalElementData.GetSize(elements[i]);
            }
        }
    }
}
