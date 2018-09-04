using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class OrdersToPython : MonoBehaviour
{
    [Header("Scene")]
    // the reference to the programm which handles the execution of python
    //private PythonExecuter PE;
    // the reference to the LaserGrabber script of the controller that can move single atoms
    private LaserGrabber AtomLayerLG;
    // the reference to the LaserGrabber script of the controller that can move the whole structure
    private LaserGrabber BoundingboxLayerLG;  // useless atm
    // the reference to the controllers
    //private GameObject[] Controllers = new GameObject[2];
    // shows whether the input order of the user could be executed or not
    private bool couldExecuteOrder;
    // shows whether Python should be currently sending an animation or just always the same frame
    public static bool pythonRunsAnim = false;

    public static readonly Dictionary<string, string> Orders = new Dictionary<string, string>
    {
        {"Destroy Atom Nr", "DestroyAtom" },
        {"Stop Animation", "RunAnimation" },
        {"Run Animation", "RunAnimation" },
        {"Force of atom Nr ", "RequestForce" }, // outdated
        {"Send all forces", "RequestAllForces" },
        {"Set new positions", "SetNewPositions" }
    };

    private void Start()
    {
        // get the reference to the programm which handles the execution of python
        //SceneReferences.inst.PE = gameObject.GetComponent<PythonExecuter>();
        // get the reference to the LaserGrabber script of the controller that can move single atoms
        GetReferenceToAtomLayerLG();
    }

    private void GetReferenceToAtomLayerLG()
    {
        // get the reference to the LaserGrabber script of the controller that can move single atoms
        foreach (LaserGrabber LG in SceneReferences.inst.LGs)
            if (LG.ctrlMaskName.Contains("Atom"))
                AtomLayerLG = LG;
            else
                BoundingboxLayerLG = LG;
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
        if (orderFunctionName != "RequestOrders" || orderFunctionName == "SetNewPositions") // TODO: add in the dict if the functions take params or not
            orderFunctionName += "Order";
        GetType().GetMethod(orderFunctionName).Invoke(this, myParams);
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
        SceneReferences.inst.PE.SendOrder("Executer exec self.destroy_atom(" + atomId + ")");
        // delete the atom and send python/pyiron that the atom should be excluded in the structure
        StructureData.inst.waitForDestroyedAtom = true;
        // remove the atom in the list of the properties of each atom
        StructureData.inst.atomInfos.RemoveAt(AtomLayerLG.attachedObject.GetComponent<AtomID>().ID);
        // decrease the atomId of the atoms which have a higher ID than the deleted one by one
        for (int i = AtomLayerLG.attachedObject.GetComponent<AtomID>().ID; i < PythonExecuter.structureSize - 2; i++)
            StructureData.inst.atomInfos[i + 1].m_ID -= 1;
        // remove the atom in the list which stores the data how the player has removed each atom
        StructureData.inst.atomCtrlPos.RemoveAt(AtomLayerLG.attachedObject.GetComponent<AtomID>().ID);
        // destroy the gameobject of the destroyed atom. This way, importStructure won't destroy all atoms and load them new
        Destroy(AtomLayerLG.attachedObject);
    }

    public void RunAnimOrder(string order)
    {
        if (order != "")
            RunAnim(order.Contains("Run"));
    }

    public void RunAnim(bool shouldRun=false)
    {
        if (shouldRun)
            SceneReferences.inst.PE.SendOrder("Executer exec self.run_anim()");
        else
            SceneReferences.inst.PE.SendOrder("Executer exec self.stop_anim()");
        pythonRunsAnim = shouldRun;
        // update the symbols on all active controllers
        foreach (GameObject Controller in SceneReferences.inst.Controllers)
            if (Controller.activeSelf)
                Controller.GetComponent<ControllerSymbols>().SetSymbol();
    }

    // request the forces of all atoms from Python
    public void RequestAllForces()
    {
        SceneReferences.inst.PE.SendOrder("Executer exec self.send_all_forces()");
    }

    public void SetNewPositions()
    {
        string newPosition;
        foreach (AtomInfos atomInfo in StructureData.inst.atomInfos)
        {
            newPosition = "";
            Vector3 atomPosition = atomInfo.m_transform.localPosition;
            for (int i = 0; i < 3; i++)
                newPosition += atomPosition[i] + " ";
            newPosition += atomInfo.m_ID;
            // send the local position of the current atom to Python
            SceneReferences.inst.PE.SendOrder("Executer exec self.set_new_base_position('" + newPosition + "')");
            // set the atom back to the position where it was before the player moved it
            atomPosition -= StructureData.inst.atomCtrlPos[atomInfo.m_ID];
            // show that the player hasn't moved an atom since the last creation of an ham_lammps
            StructureData.inst.atomCtrlPos[atomInfo.m_ID] = Vector3.zero;
        }
    }
}
