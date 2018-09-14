using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PeriodensystemButtonManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

	public Button m_Button;
	public GameObject curAtomObject;
	//private LocalElementData LED;
	string m_objectName = "";
	string m_shortname = "";
    private GameObject generatedElem;
    public GameObject ElementAdder;

    public void OnBeginDrag(PointerEventData eventData)
    {
        generatedElem = Instantiate(gameObject);
        generatedElem.transform.SetParent(transform);
        generatedElem.transform.position = Vector3.forward * 0.1f;
        generatedElem.transform.localEulerAngles = Vector3.zero;
        generatedElem.transform.localScale = Vector3.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        generatedElem.transform.position = eventData.pointerCurrentRaycast.worldPosition;
        if (Intersects())
            generatedElem.transform.position = ElementAdder.transform.position;
    }

    private bool Intersects()
    {
        return generatedElem.transform.position.x + generatedElem.transform.lossyScale.x >= ElementAdder.transform.position.x - ElementAdder.transform.lossyScale.x &&
            generatedElem.transform.position.x + generatedElem.transform.lossyScale.x >= ElementAdder.transform.position.x - ElementAdder.transform.lossyScale.x &&
            generatedElem.transform.position.x + generatedElem.transform.lossyScale.x >= ElementAdder.transform.position.x - ElementAdder.transform.lossyScale.x &&
            generatedElem.transform.position.x + generatedElem.transform.lossyScale.x >= ElementAdder.transform.position.x - ElementAdder.transform.lossyScale.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Intersects())
        {
            generatedElem.transform.SetParent(ElementAdder.transform.parent);
            generatedElem = null;
        }
        else
            Destroy(generatedElem);
    }

    void Start()
	{
		Button ElementButton = m_Button.GetComponent<Button>();

		//curAtomObject = Resources.Load("Prefabs/AtomPrefab",typeof(GameObject)) as GameObject;

		//Calls the TaskOnClick/TaskWithParameters method when you click the Button
		ElementButton.onClick.AddListener(TaskOnClick);

	}

//##########################################################################################

	void TaskOnClick()
	{
		/*m_shortname = "";
//		print (this.gameObject.name);
		m_objectName = this.gameObject.name;
		m_shortname = m_objectName.Remove (0, 7);
//		print (m_shortname);
		string fullname = LED.getFullName(m_shortname);
		Color atomColor = LED.getColour (m_shortname);

//		Instantiate(curAtomObject);
		curAtomObject = Instantiate(curAtomObject, new Vector3(0,0,0), Quaternion.identity) as GameObject;
		curAtomObject.AddComponent<Atom> ();
		curAtomObject.GetComponent<Atom> ().m_atomID = 1;
		curAtomObject.GetComponent<Atom> ().m_shortName = this.gameObject.name;
		curAtomObject.GetComponent<Atom> ().m_fullName = fullname;
		curAtomObject.GetComponent<Atom> ().m_color = atomColor; 
		curAtomObject.GetComponent<Atom>().material.color = atomColor;
		curAtomObject.transform.position = new Vector3 (0, 0, 0);*/
		//Output this to console when the Button is clicked
//		Debug.Log("You have clicked the button!");
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
