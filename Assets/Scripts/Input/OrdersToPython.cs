using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class OrdersToPython : MonoBehaviour {
    [Header("Scene")]
    // the data about the structure
    private StructureData SD;
    // the reference to the programm which handles the execution of python
    private PythonExecuter PE;
    // the reference to the LaserGrabber script of the controller that can move single atoms
    private LaserGrabber AtomLayerLG;

    private bool couldExecuteOrder;

    public readonly Dictionary<string, string> Orders = new Dictionary<string, string>
    {
        {"Destroy Atom", "DestroyAtom" }
    };

    private void Awake()
    {
        // the data about the structure
        // get the script StructureData from AtomStructure
        SD = GameObject.Find("AtomStructure").GetComponent<StructureData>();
        // get the reference to the programm which handles the execution of python
        PE = gameObject.GetComponent<PythonExecuter>();
        // get the reference to the LaserGrabber script of the controller that can move single atoms
        GetReferenceToAtomLayerLG();
    }

    private void GetReferenceToAtomLayerLG()
    {
        // get the reference to the LaserGrabber script of the controller that can move single atoms
        foreach (LaserGrabber LG in GameObject.Find("[CameraRig]").GetComponentsInChildren<LaserGrabber>())
            if (LG.ctrlMaskName.Contains("Atom"))
                AtomLayerLG = LG;
    }

    // Use this for initialization
    void Start () {
        //if (!ExecuteOrder("Destroy Atom 0"))
        //    print("Invalid Input!");
    }

    public bool ExecuteOrder(string order)
    {
        string orderFunctionName = "";
        foreach (string key in Orders.Keys)
            if (order.Contains(key))
                orderFunctionName = Orders[key];
         if (orderFunctionName == "")
            return false;

        //MethodInfo theMethod = this.GetType().GetMethod(orderFunctionName);
        object[] myParams = new object[1];
        myParams[0] = order;
        GetType().GetMethod(orderFunctionName).Invoke(this, myParams);
        return couldExecuteOrder;
    }

    public void DestroyAtom(string order)
    {
        int atomId;
        if (!int.TryParse(order.Split()[2], out atomId))
        {
            couldExecuteOrder = false;
            return;
        }

        // check if the given atomId is valid


        if (AtomLayerLG == null)
        {
            GetReferenceToAtomLayerLG();
            if (AtomLayerLG == null)
            {
                couldExecuteOrder = false;
                return;
            }
        }


        // send Python/Pyiron the order to destroy the atom
        PE.SendOrder("self.destroy_atom(" + atomId + ")");
        // delete the atom and send python/pyiron that the atom should be excluded in the structure
        SD.waitForDestroyedAtom = true;
        // remove the atom in the list of the properties of each atom
        SD.atomInfos.RemoveAt(AtomLayerLG.attachedObject.GetComponent<AtomID>().ID);
        // remove the atom in the list which stores the data how the player has removed each atom
        SD.atomCtrlPos.RemoveAt(AtomLayerLG.attachedObject.GetComponent<AtomID>().ID);
        // destroy the gameobject of the destroyed atom. This way, importStructure won't destroy all atoms and load them new
        Destroy(AtomLayerLG.attachedObject);
        // show that when loading a python anim the next time, it should first load the new one (without the removed atom)
        SD.needsNewAnim = true;

        print("Yaaaaaaaaaaay" + atomId);
        // DestroyAtom.GetType();
    }
}
