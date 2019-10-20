using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PeriodensystemButtonManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

	public Button m_Button;
    private GameObject generatedElem;
    public GameObject ElementAdder;

    public void OnBeginDrag(PointerEventData eventData)
    {
        /*generatedElem = Instantiate(gameObject);
        Destroy(generatedElem.GetComponent<PeriodensystemButtonManager>());
        generatedElem.transform.SetParent(transform);
        generatedElem.transform.position = Vector3.forward * 0.2f;
        generatedElem.transform.localEulerAngles = Vector3.zero;
        generatedElem.transform.localScale = Vector3.one;*/
    }

    public void OnDrag(PointerEventData eventData)
    {
        /*if (generatedElem != null)
        {
            generatedElem.transform.position = eventData.pointerCurrentRaycast.worldPosition;
            if (Intersects(eventData))
            {
                generatedElem.transform.SetParent(ElementAdder.transform.parent);
                generatedElem.transform.localScale = ElementAdder.transform.localScale;
                ElementAdder.transform.SetParent(transform);
                ElementAdder.transform.SetParent(generatedElem.transform.parent);
                ElementAdder.transform.parent.parent.GetComponent<StructureCreatorMenuController>().AddElement(generatedElem);
                generatedElem = null;
            }
                //generatedElem.transform.position = ElementAdder.transform.position;
        }*/
    }

    /*private bool Intersects(PointerEventData data)
    {
        return data.hovered.Contains(ElementAdder.transform.parent.gameObject);
        //return generatedElem.transform.position.x + generatedElem.transform.lossyScale.x >=
        //    ElementAdder.transform.position.x - ElementAdder.transform.lossyScale.x &&
        //    generatedElem.transform.position.x - generatedElem.transform.lossyScale.x <=
        //    ElementAdder.transform.position.x + ElementAdder.transform.lossyScale.x &&
        //    generatedElem.transform.position.y + generatedElem.transform.lossyScale.y >=
        //    ElementAdder.transform.position.y - ElementAdder.transform.lossyScale.y &&
        //    generatedElem.transform.position.y - generatedElem.transform.lossyScale.y <=
        //    ElementAdder.transform.position.y + ElementAdder.transform.lossyScale.y;
    }*/

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (Intersects())
        //{
        //    generatedElem.transform.SetParent(ElementAdder.transform.parent);
        //    generatedElem.transform.localScale = ElementAdder.transform.localScale;
        //    ElementAdder.transform.SetParent(transform);
        //    ElementAdder.transform.SetParent(generatedElem.transform.parent);
        //    generatedElem = null;
        //}
        //else
        //Destroy(generatedElem);
    }

    void Start()
	{
		Button elementButton = m_Button.GetComponent<Button>();

		//curAtomObject = Resources.Load("Prefabs/AtomPrefab",typeof(GameObject)) as GameObject;

		//Calls the TaskOnClick/TaskWithParameters method when you click the Button
		elementButton.onClick.AddListener(TaskOnClick);

	}

//##########################################################################################

	void TaskOnClick()
	{
		if (PeriodicSysMenuController.inst.togState)
		{
			StructureCreatorMenuController.inst.AddElementSign.SetActive(false);
			generatedElem = Instantiate(gameObject, StructureCreatorMenuController.inst.AddElementSign.transform.parent);
			Destroy(generatedElem.GetComponent<PeriodensystemButtonManager>());
			generatedElem.transform.localPosition = Vector3.zero;
			generatedElem.transform.localEulerAngles = Vector3.zero;
			generatedElem.transform.localScale = Vector3.one;
		}
		else
		{
			// send Python the order to add a new atom and send back the formatted_data
            ImportStructure.newImport = true;
            LaserGrabber.shouldReloadAnim = true;
            
            PythonExecuter.SendOrderSync(PythonScript.Executor, PythonCommandType.eval, 
            	"self.add_new_atom('" + gameObject.name.Remove(0, 7) + "')");
		}
		
		
		/*m_shortname = "";
//		print (this.gameObject.name);
		m_objectName = this.gameObject.name;
		m_shortname = m_objectName.Remove (0, 7);
//		print (m_shortname);
		string fullname = LocalElementData.GetFullName(m_shortname);
		Color atomColor = LocalElementData.GetColour (m_shortname);

//		Instantiate(curAtomObject);
		curAtomObject = Instantiate(curAtomObject, new Vector3(0,0,0), Quaternion.identity) as GameObject;
		curAtomObject.AddComponent<AtomID> ();
		curAtomObject.GetComponent<Atom> ().m_atomID = 1;
		curAtomObject.GetComponent<Atom> ().m_shortName = this.gameObject.name;
		curAtomObject.GetComponent<Atom> ().m_fullName = fullname;
		curAtomObject.GetComponent<Atom> ().m_color = atomColor; 
		curAtomObject.GetComponent<Atom>().material.color = atomColor;
		curAtomObject.transform.position = new Vector3 (0, 0, 0);
		//Output this to console when the Button is clicked
//		Debug.Log("You have clicked the button!");*/
	}



//##########################################################################################
/*
	// steps down the speed of animations
	public void OnHydrogenButtonClicked () {
		
//		Hier die auszuführende Prozedur eintragen

	} // end OnHydrogenButtonClicked
*/
//##########################################################################################


} // end class
