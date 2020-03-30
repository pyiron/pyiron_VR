using UnityEngine;


public class Element
{
    // create the variables, which hold the data of the properties of an element
    public readonly string m_casNumber;
    public readonly string m_fullName;
    public readonly int m_ordinalNumber;
    public readonly float m_size;
    public readonly Color m_colour;

    // create an object which holds the infos of an atom, without a special size and colour
    public Element(string casNumber, string fullName, int ordinalNumber)
    {
        m_casNumber = casNumber;
        m_fullName = fullName;
        m_ordinalNumber = ordinalNumber;
        m_size = 1;
        m_colour = new Color(1, 1, 1, 1);
    }

    // create an object which holds the infos of an atom, with a special size and colour
    public Element(string casNumber, string fullName, int ordinalNumber, float size, Color colour)
    {
        m_casNumber = casNumber;
        m_fullName = fullName;
        m_ordinalNumber = ordinalNumber;
        m_size = size;
        m_colour = colour;
    }
}
