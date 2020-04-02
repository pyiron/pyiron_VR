using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugShortcuts : MonoBehaviour
{
    public InputField serverIpField;
    
    private int counter;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("x"))
        {
            if (counter == 0)
            {
                TCPClient.inst.ConnectWithHost(serverIpField.text);
            }

            if (counter == 1)
            {
                ModeController.inst.SetMode(Modes.Explorer.ToString());
            }

            if (counter == 2)
            {
                ExplorerMenuController.inst.LoadPathContent(
                    "Examples");
            }
            
            if (counter == 3)
            {
                StartCoroutine(OptionButton.HandleLoad("ham_lammps_md"));
            }

            if (counter == 4)
            {
                AnimationMenuController.Inst.OnSimBtnDown();
            }
            counter++;
        }
    }
}
