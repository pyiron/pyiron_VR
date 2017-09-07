using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class OrdersToPython : MonoBehaviour {


    public readonly Dictionary<string, string> Orders = new Dictionary<string, string>
    {
        {"Destroy Atom", "DestroyAtom" }
    };

    public void DestroyAtom(string order)
    {
        int atomId = int.Parse(order.Split()[2]);
        print("Yaaaaaaaaaaay" + atomId);
        // DestroyAtom.GetType();
    }

    // Use this for initialization
    void Start () {
        if (!ExecuteOrder("Destroy Atom 0"))
            print("Invalid Input!");
    }

    public bool ExecuteOrder(string order)
    {
        string orderFunctionName = "";
        int paramCounter = 0;
        foreach (string key in Orders.Keys)
            if (order.Contains(key))
                orderFunctionName = Orders[key];
         if (orderFunctionName == "")
            return false;

        //MethodInfo theMethod = this.GetType().GetMethod(orderFunctionName);
        object[] myParams = new object[1];
        myParams[0] = order;
        GetType().GetMethod(orderFunctionName).Invoke(this, myParams);
        return true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
