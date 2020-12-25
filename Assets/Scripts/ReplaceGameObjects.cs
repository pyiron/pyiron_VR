using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

// CopyComponents - by Michael L. Croswell for Colorado Game Coders, LLC
// March 2010
 
//Modified by Kristian Helle Jespersen
//June 2011
 
/*[ExecuteInEditMode]
public class ReplaceGameObjects : MonoBehaviour
{
    public bool copyValues = true;
    public GameObject NewType;
    public GameObject Container;

    // [MenuItem("Custom/Replace GameObjects")]
    
    private void Start()
    {
        OnWizardCreate();
    }


    // static void CreateWizard()
    // {
    //     ScriptableWizard.DisplayWizard("Replace GameObjects", typeof(ReplaceGameObjects), "Replace");
    // }
 
    void OnWizardCreate()
    {
        //Transform[] Replaces;
        //Replaces = Replace.GetComponentsInChildren<Transform>();
 
        foreach (Button btn in Container.GetComponentsInChildren<Button>())
        {
            GameObject go = btn.gameObject;
            print(go.name);
            //EditorUtility.ReplacePrefab(btn.gameObject, NewType, ReplacePrefabOptions.ConnectToPrefab);
            GameObject newObject;
            newObject = (GameObject)EditorUtility.InstantiatePrefab(NewType);
            newObject.name = go.name;
            newObject.transform.position = go.transform.position;
            newObject.transform.rotation = go.transform.rotation;
            newObject.transform.SetParent(go.transform.parent, worldPositionStays:true);
            newObject.transform.localScale = go.transform.localScale;
            Text[] oldTexts = go.GetComponentsInChildren<Text>();
            Text[] newTexts = newObject.GetComponentsInChildren<Text>();
            for (int i = 0; i < oldTexts.Length; i++)
            {
                newTexts[i].text = oldTexts[i].text;
                newTexts[i].gameObject.name = oldTexts[i].gameObject.name;
            }
            
            // Image[] oldImages = go.GetComponentsInChildren<Image>();
            // Image[] newImages = newObject.GetComponentsInChildren<Image>();
            // for (int i = 0; i < oldTexts.Length; i++)
            // {
            //     newImages[i].sprite = oldImages[i].sprite;
            // }
            
            Button[] oldButtons = go.GetComponentsInChildren<Button>();
            Button[] newButtons = newObject.GetComponentsInChildren<Button>();
            for (int i = 0; i < oldButtons.Length; i++)
            {
                newButtons[i].onClick = oldButtons[i].onClick;
                newButtons[i].colors = oldButtons[i].colors;
            }
            
            DestroyImmediate(go);
 
        }
 
    }
}*/

