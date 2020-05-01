using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utilities
{
    /// <summary>
    /// Takes a Dropdown and returns the text of the currently active option
    /// </summary>
    /// <param name="dropdown">A dropdown from which the current active option should be returned</param>
    /// <returns>The text of the currently active option</returns>
    public static string GetStringValue(Dropdown dropdown)
    {
        return dropdown.options[dropdown.value].text;
    }
    
    /// <summary>
    /// Allows to set the option of a Dropdown by the text of the option. If no option with this name exists,
    /// a new option will be created
    /// </summary>
    /// <param name="dropdown">The dropdown that should be set to a specific option</param>
    /// <param name="value">the name of the option that should be set</param>
    public static void SetDropdownValue(Dropdown dropdown, string value)
    {
        // iterates through all the indices to look if the value and the text of the option match
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == value)
            {
                // sets the dropdown to the correct option
                dropdown.value = i;
                return;
            }
        }
        // creates a new option
        dropdown.options.Add(new Dropdown.OptionData(value));
        dropdown.value = dropdown.options.Count - 1;
    }
    
    /// <summary>
    /// not needed atm. can transform a primitive into json format
    /// </summary>
    /// <param name="source">the value that should be stored in a json</param>
    /// <param name="topClass">the type of the value</param>
    /// <returns>the new json</returns>
    public static string WrapToClass(string source, string topClass){
        return string.Format("{{ \"{0}\": {1}}}", topClass, source);
    }
    
    
    /// <summary>
    /// Converts a vector 3 into the string representation of an array
    /// </summary>
    /// <param name="v3">The Vector3 object</param>
    /// <returns>the Array representation of the vector3</returns>
    public static string Vec3ToArrayString(Vector3 v3)
    {
        return v3.ToString().Replace('(', '[').Replace(')', ']');
    }
}