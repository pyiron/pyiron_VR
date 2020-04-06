using System;
using System.Collections.Generic;
using UnityEngine.UI;

public static class Utilities
{
    public static string GetStringValue(Dropdown dropdown)
    {
        return dropdown.options[dropdown.value].text;
    }
    
    public static void SetDropdownValue(Dropdown dropdown, string value)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == value)
            {
                dropdown.value = i;
                return;
            }
        }
        dropdown.options.Add(new Dropdown.OptionData(value));
        dropdown.value = dropdown.options.Count - 1;
    }
    
    public static string WrapToClass(string source, string topClass){
        return string.Format("{{ \"{0}\": {1}}}", topClass, source);
    }
}