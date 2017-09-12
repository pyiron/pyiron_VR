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
    // shows whether the input order of the user could be executed or not
    private bool couldExecuteOrder;
    // shows whether Python should be currently sending an animation or just always the same frame
    public static bool pythonRunsAnim = false;

    public readonly Dictionary<string, string> Orders = new Dictionary<string, string>
    {
        {"Destroy Atom Nr", "DestroyAtom" },
        {"Stop Animation", "RunAnimation" },
        {"Run Animation", "RunAnimation" }
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

        // check if the order is known, else return false
         if (orderFunctionName == "")
            return false;

        //MethodInfo theMethod = this.GetType().GetMethod(orderFunctionName);
        object[] myParams = new object[1];
        myParams[0] = order;
        print(myParams[0]);
        GetType().GetMethod(orderFunctionName + "Order").Invoke(this, myParams);
        return couldExecuteOrder;
    }

    private void SendError(string errorMessage)
    {
        print(errorMessage);
        couldExecuteOrder = false;
    }

    public void DestroyAtomOrder(string order)
    {
        // the ID of the atom that should be destroyed
        int atomId;
        // checks if the argument that should contain the atom ID is an integer, else return and just print what the error was
        if (!int.TryParse(order.Split()[3], out atomId))
        {
            SendError("The Atom ID has to be an Integer!");
            return;
        }
        else
            DestroyAtom(atomId);
    }

    // destroys the atom, the user wants to destroy
    private void DestroyAtom(int atomId)
    {
        // check if the given atomId is less big than the maximum amount of atoms in the structure
        if (atomId >= PythonExecuter.structureSize)
        {
            SendError("The Atom ID has to be less big than the maximum amount of atoms in the structure!");
            return;
        }

        // Get the reference to the LaserGrabber of the AtomLayer, if it is still unknown
        if (AtomLayerLG == null)
        {
            GetReferenceToAtomLayerLG();
            // return if the controller hasn't been activated yet
            if (AtomLayerLG == null)
            {
                SendError("Left Controller has to be active to do this!");
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
    }

    public void RunAnimOrder(string order)
    {
        if (order != "")
            RunAnim(order.Contains("Run"));
    }

    public void RunAnim(bool shouldRun=false)
    {
        if (shouldRun)
            PE.SendOrder("self.start_anim()");
        else
            PE.SendOrder("self.runAnim = False");
        pythonRunsAnim = shouldRun;
    }
}
