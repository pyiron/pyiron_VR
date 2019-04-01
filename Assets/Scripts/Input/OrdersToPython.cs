using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.Reflection;
using System.Threading;

public class OrdersToPython : MonoBehaviour
{
    public static OrdersToPython inst;
    
    [Header("Scene")]
    // shows whether the input order of the user could be executed or not
    private bool couldExecuteOrder;

    public static readonly Dictionary<string, string> Orders = new Dictionary<string, string>
    {
        {"Destroy Atom Nr", "DestroyAtom" },
        {"Stop Animation", "RunAnimation" },
        {"Run Animation", "RunAnimation" },
        {"Force of atom Nr ", "RequestForce" }, // outdated
        {"Send all forces", "RequestAllForces" },
        {"Set new positions", "SetNewPositions" }
    };

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        // might not work on mac. Without it, 1.234 could be converted to 1,234
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
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
        if (orderFunctionName != "RequestOrders" || orderFunctionName == "SetNewPositions")
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
        if (atomId >= StructureData.structureSize)
        {
            SendError("The Atom ID has to be less big than the maximum amount of atoms in the structure!");
            return;
        }

        // send Python/Pyiron the order to destroy the atom
        PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.exec, "self.destroy_atom(" + atomId + ")");
        // delete the atom and send python/pyiron that the atom should be excluded in the structure
        StructureData.waitForDestroyedAtom = true;
        // remove the atom in the list of the properties of each atom
        StructureData.atomInfos.RemoveAt(LaserGrabber.instances[(int) Layer.Atom].attachedObject.GetComponent<AtomID>().ID);
        // decrease the atomId of the atoms which have a higher ID than the deleted one by one
        for (int i = LaserGrabber.instances[(int) Layer.Atom].attachedObject.GetComponent<AtomID>().ID; i < StructureData.structureSize - 2; i++)
            StructureData.atomInfos[i + 1].m_ID -= 1;
        // remove the atom in the list which stores the data how the player has removed each atom
        StructureData.atomCtrlPos.RemoveAt(LaserGrabber.instances[(int) Layer.Atom].attachedObject.GetComponent<AtomID>().ID);
        // destroy the gameobject of the destroyed atom. This way, importStructure won't destroy all atoms and load them new
        Destroy(LaserGrabber.instances[(int) Layer.Atom].attachedObject);
    }

    public void RunAnimOrder(string order)
    {
        if (order != "")
            AnimationController.RunAnim(order.Contains("Run"));
    }

    /*public void RunAnim(bool shouldRun=false)
    {
        AnimationController.run_anim = shouldRun;
        // update the symbols on all active controllers
        foreach (GameObject Controller in SceneReferences.inst.Controllers)
            if (Controller.activeSelf)
                Controller.GetComponent<ControllerSymbols>().SetSymbol();
    }*/

    // request the forces of all atoms from Python
    public static void RequestAllForces()
    {
        PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.exec, "self.send_all_forces()");
    }

    public static void SetNewPositions()
    {
        foreach (AtomInfos atomInfo in StructureData.atomInfos)
        {
            string newPosition = "";
            Vector3 atomPosition = atomInfo.m_transform.localPosition;
            for (int i = 0; i < 3; i++)
                newPosition += atomPosition[i] + " ";
            newPosition += atomInfo.m_ID;
            // send the local position of the current atom to Python
            PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.exec,
                "self.set_new_base_position('" + newPosition + "')");
            // show that the player hasn't moved an atom since the last creation of an ham_lammps
            StructureData.atomCtrlPos[atomInfo.m_ID] = Vector3.zero;
        }
    }
}
