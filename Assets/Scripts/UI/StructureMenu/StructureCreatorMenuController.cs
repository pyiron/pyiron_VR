using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StructureCreatorMenuController : MenuController
{
    internal static StructureCreatorMenuController inst;
    public Button CreateBtn;
    public GameObject AtomAmountPref;
    public GameObject AddElementSign;
    private List<GameObject> elements = new List<GameObject>();
    private bool gui_created = false;
    internal static bool should_build_gui = false;
    internal static List<Dictionary<string, List<string>>> args = new List<Dictionary<string, List<string>>>();

    private void Awake()
    {
        inst = this;
    }

    internal void OnModeChange()
    {
        if (!gui_created)
        {
            gui_created = true;
            // PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.eval, "self.send_args_create_ase_bulk()");
        }
    }

    private void Update()
    {
        if (should_build_gui)
        {
            foreach (Dictionary<string, List<string>> arg in args)
            {
                print(arg["name"][0]);
            }
            should_build_gui = false;
        }

        // activate the create button if the structure is valid
        CreateBtn.interactable = !AddElementSign.activeSelf;
    }

    public void AddElement(GameObject elm)
    {
        CreateBtn.interactable = true;
        elements.Add(elm);
        GameObject atomAmount = Instantiate(AtomAmountPref);
        atomAmount.transform.SetParent(elm.transform);
        atomAmount.transform.localPosition = Vector3.down * 20;
        atomAmount.transform.localEulerAngles = Vector3.zero;
        atomAmount.GetComponent<Text>().fontSize = 72;
        atomAmount.transform.localScale = Vector3.one * 0.1f;
        foreach (Button btn in AtomAmountPref.GetComponentsInChildren<Button>())
        {
            btn.onClick.AddListener(delegate { OnButtonClicked(btn); });
        }
    }

    public void OnCreatStrucBtnClicked(Button btn)
    {
        btn.interactable = false;
        
        // todo: get the element of the new structure
        string elm = "Fe";
        
        // create the new structure
        ImportStructure.newImport = true;
        PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval, 
            "self.create_new_struc(" + AnimationController.frame + ", '" + elm + "', True)");
        
        
        
        /*string elementData = "";
        foreach (GameObject go in elements)
        {
            foreach (Text txt in go.GetComponentsInChildren<Text>())
                if (txt.name.Contains("Symbol"))
                    elementData += txt.text;
            Destroy(go);
        }
        ImportStructure.newImport = true;
        PythonExecuter.SendOrder(PythonScript.Executor, PythonCommandType.eval, 
            "self.create_new_struc('" + elementData + "', True)");*/
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
            PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval, 
                "self.create_new_struc('" + elementData + "', True)");
        }
        else if (btn.name == "Add")
        {
            Text txt = btn.transform.parent.GetComponent<Text>();
            txt.text = "" + int.Parse(txt.text) * 8;
        }
        else if (btn.name == "Subtract")
        {
            Text txt = btn.transform.parent.GetComponent<Text>();
            txt.text = "" + int.Parse(txt.text) / 8;
        }
    }
}