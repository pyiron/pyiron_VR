using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This script handles everything related to the atom structure
/// </summary>
public class Structure : MonoBehaviour
{
    public static Structure Inst;

    // the prefab to spawn a new atom
    public GameObject atomPrefab;
    // references to all currently instanciated atoms
    private List<GameObject> atoms = new List<GameObject>();

    private bool isActive = true;


    private void Awake()
    {
        Inst = this;
    }

    public void Activate()
    {
        isActive = true;
        // set the color of the box to grey
        Boundingbox.Inst.SetColor(Boundingbox.Inst.baseColor);
        // make the atoms again opaque
        foreach (GameObject atom in atoms)
        {
            Renderer atomRenderer = atom.GetComponent<Renderer>();
            Color atomColor = atomRenderer.material.color;
//            atomColor.a = 1f;
            atomRenderer.material.color = atomColor * 2;
        }
    }

    public void Deactivate()
    {
        isActive = false;
        Boundingbox.Inst.SetColor(Color.gray);
        foreach (GameObject atom in atoms)
        {
            Renderer atomRenderer = atom.GetComponent<Renderer>();
            Color atomColor = atomRenderer.material.color;
//            atomColor.a = 0.7f;
            atomRenderer.material.color = atomColor / 2;
        }
    }

    /// <summary>
    /// returns how many atoms the current structure has
    /// </summary>
    /// <returns>the amount of atoms</returns>
    public int AtomAmount()
    {
        return atoms.Count;
    }

    public void OnAtomPositionChanged(GameObject movedAtom)
    {
        int id = atoms.IndexOf(movedAtom);
        string order = "structure.positions[" + id + "] = " + Utilities.Vec3ToArrayString(movedAtom.transform.localPosition);
        //string order = "print(Structure.Structure.structure.positions[" + id + "])";
        PythonExecutor.SendOrderSync(PythonScript.structure, false, order);
        //PythonExecuter.SendOrderSync(PythonScript.None, PythonCommandType.exec_l, order);

        if (!SimulationMenuController.Inst.IsStructureShifted())
        {
            SimulationMenuController.jobName += SimulationMenuController.SHIFTED;
        }
    }

    public void OnAtomDeleted(GameObject deletedAtom)
    {
        print("TODO: the structure should send this information to the pyiron program here");
        int id = atoms.IndexOf(deletedAtom);
    }
    
    /// <summary>
    /// Set the number of atoms in the scene to the one that should be shown by spawning new atoms or deleting some
    /// </summary>
    /// <param name="diff"></param>
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

    /// <summary>
    /// Update the structure according to the position data, but don't update the type of each atom
    /// </summary>
    /// <param name="positions"></param>
    public void UpdateStructure(Vector3[] positions)
    {
        UpdateStructure(positions, null);
    }
    
    /// <summary>
    /// Update the structure according to the position data and the given elements
    /// </summary>
    /// <param name="positions">the positions the atoms should be set at</param>
    /// <param name="elements">the element of each atom. Set to null to keep the old elements</param>
    public void UpdateStructure(Vector3[] positions, string[] elements)
    {
        // we need as many atoms in the scene as should be shown
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
//                Light haloLight = atoms[i].GetComponent<Light>();
//                haloLight.color = LocalElementData.GetColour(elements[i]);
//                haloLight.range = ProgramSettings.size * LocalElementData.GetSize(elements[i]);
//                Behaviour halo = (Behaviour)atoms[i].GetComponent("Halo");
//                halo..color = LocalElementData.GetColour(elements[i]);

                // set the atoms size to the size this type of atom has 
                atoms[i].transform.localScale = Vector3.one * LocalElementData.GetSize(elements[i]);
            }
        }
    }
}
