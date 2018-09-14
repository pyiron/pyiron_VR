using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureCreatorMenuController : MenuController
{
    internal static StructureCreatorMenuController inst;
    public Button CreateBtn;
    private List<GameObject> elements = new List<GameObject>();

    private void Awake()
    {
        inst = this;
    }

    public void AddElement(GameObject elm)
    {
        CreateBtn.interactable = true;
        elements.Add(elm);
    }

    public void OnButtonClicked(Button btn)
    {
        if (btn.GetComponentInChildren<Text>().text == "Create Structure")
        {
            btn.interactable = false;
            string elementData = "";
            foreach (GameObject go in elements)
            {
                foreach (Text txt in go.GetComponentsInChildren<Text>())
                    if (txt.name.Contains("Symbol"))
                        elementData += txt.text;
                Destroy(go);
            }
            ImportStructure.newImport = true;
            PythonExecuter.inst.SendOrder(PythonScript.Executor, PythonCommandType.exec, 
                "self.create_new_struc('" + elementData + "', False, " + AnimationController.frame + ")");
        }
    }
}
