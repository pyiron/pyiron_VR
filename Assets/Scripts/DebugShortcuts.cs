using System.Collections;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script allows to jump through the program in a predefined way to create a better workflow
/// </summary>
public class DebugShortcuts : MonoBehaviour
{
    [SerializeField] private Text serverAddressField;
    
    private int counter;

    void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            if (counter == 0)
            {
                TCPClientConnector.Inst.ConnectWithHost(serverAddressField.text);
            }

            if (counter == 1)
            {
                ModeController.inst.SetMode(Modes.Explorer.ToString());
            }

            if (counter == 2) // outdated
            {
                ExplorerMenuController.Inst.LoadPathContent(
                    "Examples");
            }
            
            if (counter == 3)
            {
                //StartCoroutine(OptionButton.HandleLoad("ham_lammps_md"));
                OptionButton.LoadJob("ham_lammps_md");
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
