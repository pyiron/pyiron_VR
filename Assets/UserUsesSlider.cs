using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class UserUsesSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler// required interface when using the OnPointerDown method.
{
    public static bool userUsesSlider;
    
    //Do this when the mouse is clicked over the selectable object this script is attached to.
    public void OnPointerDown (PointerEventData eventData) 
    {
        Debug.Log (this.gameObject.name + " Was Clicked.");
        userUsesSlider = true;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log (this.gameObject.name + " Click ended.");
        userUsesSlider = false;
    }
}