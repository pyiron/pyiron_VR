using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script allows to jump through the program in a predefined way to create a better workflow
/// </summary>
public class DebugShortcuts : MonoBehaviour
{
    public InputField serverIpField;
    
    private int counter;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            if (counter == 0)
            {
                TCPClientConnector.Inst.ConnectWithHost(serverIpField.text);
            }

            if (counter == 1)
            {
                ModeController.inst.SetMode(Modes.Explorer.ToString());
            }

            if (counter == 2)
            {
                ExplorerMenuController.Inst.LoadPathContent(
                    "Examples");
            }
            
            if (counter == 3)
            {
                StartCoroutine(OptionButton.HandleLoad("ham_lammps_md"));
            }

            if (counter == 4)
            {
                ModeController.inst.SetMode(Modes.Calculate);
            }

            if (counter == 5)
            {
                AnimationMenuController.Inst.OnSimBtnDown();
            }
            counter++;
        }
    }
}
