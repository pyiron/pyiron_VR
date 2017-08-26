using UnityEngine;


public class Symbol
{
    // create the variables, which hold the data of the properties of an element
    public readonly GameObject m_object;
    public readonly Vector3 m_position;
    public readonly Vector3 m_rotation;
    public readonly float m_size;
    public readonly Color m_color;

    // create an object which holds the infos of an atom, without a special size and colour
    public Symbol(Vector3 position, Vector3 rotation, float size, Color color)
    {
        m_position = position;
        m_rotation = rotation;
        m_size = size;
        m_color = color;
    }
}
