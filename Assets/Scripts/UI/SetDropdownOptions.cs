using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//##########################################################################################

// 2018.06.27
// SetDropdownOptions darf erst nach VAController starten, damit der Pfad vorhanden ist und übernaommen werden kann!
// Dazu die script execute order für VAInputManagement.SetDropdownOptions ändern und hinter
// SteamVR_Render eintragen!!! Jetzt wird der Pfad erkannt und der Caption.text richtig gesetzt.

//##########################################################################################
	
namespace VAInputManagement {
//**************************************************

[System.Serializable]
// public class SetDropdownOptions : MonoBehaviour {

internal class SetDropdownOptions : AVAObserverClient {
//**************************************************
	private VAController m_localController;

	public string SimNumber;
	public bool SimNumberGewaehlt = false;
	public string Labeltext = "";
	private Dropdown m_DropdownBox;
	private string m_PathToStructures;

	private GameObject m_Blocker;

	public bool ControllerStartUpdate = false;

	public static SetDropdownOptions Dropoptions { private set; get; }

//##########################################################################################

	internal void init(VAController controller) {
		base.init(controller);
		m_localController = controller;
	} // init

//##########################################################################################

	void ReadDropDownListEntries()
	{
		//Use these for adding options to the Dropdown List
		Dropdown.OptionData m_NewData;
		//The list of messages for the Dropdown
		List<Dropdown.OptionData> m_Messages = new List<Dropdown.OptionData>();
			int m_Index;

		// Daeiname an den Pfad zur Strukturdatei anhängen
		m_PathToStructures =  VAController.pathToAtomStructure;

		string CompleteSource = m_PathToStructures + "Structureslist.conf";

		// Neues Readerobjekt erzeugen
		var StructureReader = new StreamReader (File.OpenRead (@CompleteSource));

		// Solange die Datei nicht zuende ist ...
		while (!StructureReader.EndOfStream) {		
			var StructureLine = StructureReader.ReadLine ();

			m_NewData = new Dropdown.OptionData();
			m_NewData.text = StructureLine;
			m_Messages.Add(m_NewData);
		} // end while

		//Take each entry in the message List
		foreach (Dropdown.OptionData message in m_Messages)
		{
			//Add each entry to the Dropdownlist
			m_DropdownBox.options.Add(message);
			//Make the index equal to the total number of entries
			m_Index = m_Messages.Count - 1;
		}
	} // ReadStructure

//##########################################################################################

	public string GetLabelText(){
//		Debug.Log ("GetLabeltext  ");
		string result = "";
//		string m_Text;
		int m_DropdownValue;

		//Keep the current index of the Dropdown in a variable
		m_DropdownValue = m_DropdownBox.value;
//		Debug.Log ("m_DropdownValue  " + m_DropdownValue);  // z.Zt. immer 0 !!!
		if (m_DropdownValue != 0) {
			//Change the message to say the name of the current Dropdown selection using the value
			result = m_DropdownBox.options [m_DropdownValue].text;
			//Change the onscreen Text to reflect the current Dropdown selection		
			//SimNumberGewaehlt = true;
//			Debug.Log ("result 1  " + result);			
		}
		return result;

	} // function GetLabelText


//##########################################################################################

		void AktuallisiereSimNumber() {			
			// Labeltext wird aktualisiert!
			Labeltext = GetLabelText ();
//		Debug.Log ("Labeltext 1  " + Labeltext);
			if (ControllerStartUpdate) {
//			Debug.Log ("AktuallisiereSimNumber 1  " + ControllerStartUpdate);
				if ((Labeltext != "") || (Labeltext != "Choose Presentation")){					
					SimNumber = Labeltext;
				Debug.Log ("SimNumber 0 " + SimNumber);
//				Debug.Log ("AktuallisiereSimNumber 2  " + SimNumberGewaehlt);
					if (SimNumber != "") {
//					Debug.Log ("AktuallisiereSimNumber 3  " + SimNumber);
						if (!SimNumberGewaehlt) {
//						Debug.Log ("AktuallisiereSimNumber 4  " + SimNumberGewaehlt);
//							Debug.Log ("SimNumber 1  " + SimNumber);
//							m_dataManager.loadNewSimulation(SimNumber);
						} else {
							Debug.Log ("SimNumber 2  " + SimNumber);
//							m_dataManager.loadNewSimulation(SimNumber);
							SimNumberGewaehlt = false;
						}
						ControllerStartUpdate = false;					
						ResetDropOptions ();
//					Debug.Log ("ControllerStartUpdate  " + ControllerStartUpdate);
					}				
				}
			}
		} // AktuallisiereSimNumber

	
//##########################################################################################

	void ResetDropOptions(){
//		m_DropdownBox.value = 0;
		m_DropdownBox.captionText.text = "Choose next Simulation";
		SimNumberGewaehlt = true;
		ControllerStartUpdate = true;
	}//ResetDropOptions			

//##########################################################################################

		// Use this for initialization
	void Start () {

			// Dropdown referenzieren
			m_DropdownBox = GameObject.Find("MainVADropdown").GetComponent<Dropdown> ();
			//Dropdown Liste löschen
			m_DropdownBox.ClearOptions();
			// Liste der Strukturen aus file lesen
			ReadDropDownListEntries ();
			// Anfangswert Captiontext setzen
			m_DropdownBox.captionText.text = "Choose new Structure";


			SimNumberGewaehlt = false;
			ControllerStartUpdate = true;

			m_Blocker = GameObject.Find("Blocker");  // ist evtl. nicht notwendig
			Destroy (m_Blocker);  // ist evtl. nicht notwendig
			//		m_dataManager.init ();

	} // Start

//##########################################################################################

	// Update is called once per frame

		void Update () {

			Destroy (m_Blocker);

//		AktuallisiereSimNumber();

		} // Update

//##########################################################################################

	} // class
//##########################################################################################
} // namespace