using System;
using System.Collections.Generic;

public static class JsonHelper
{
    public static string WrapToClass(string source, string topClass){
        return string.Format("{{ \"{0}\": {1}}}", topClass, source);
    }
}

// needed because JsonUtilities don't support dictionaries
[Serializable]
public class StringList
{
    public List<string> data;
}