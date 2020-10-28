using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteJobButton : MonoBehaviour, IButton
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WhenClickDown()
    {
        //AnimationController.Inst.DeleteAnimation();

        ExplorerMenuController.Inst.DeleteJob();
        
        // Problem: it loads the path first, because the job gets deleted async. If everything is async, this should be ok
        
        ExplorerMenuController.Inst.LoadPathContent();

        OptionButton options = ExplorerMenuController.Inst.OptionFolderJobs.GetComponentInChildren<OptionButton>();
        if (options == null)
        {
            // There does not exist any other job
            // TODO
        }
        else
        {
            string job_name = options.GetOptionText();
            //StartCoroutine(OptionButton.HandleLoad(job_name));
            ExplorerMenuController.Inst.Deactivate();
            OptionButton.LoadJob(job_name);
        }
        
    }
}
