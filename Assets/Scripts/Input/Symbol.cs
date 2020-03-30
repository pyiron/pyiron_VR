using UnityEngine;


public class Symbol
{
    // create the variables, which hold the data of the properties of an element
    public readonly GameObject m_object;
    public readonly Vector3 m_position;
    public readonly float m_positionRight;
    public readonly Vector3 m_rotation;
    public readonly float m_size;
    public readonly Color m_color;
    public readonly bool m_showWhenAnimRuns;
    public readonly int m_animSpeed;

    // create an object which holds the infos of an atom, without a special size and colour
    public Symbol(Vector3 position, Vector3 rotation, float size, Color color, bool showWhenAnimRuns, int animSpeed=-1, float positionRight = 0)
    {
        m_position = position;
        m_rotation = rotation;
        m_size = size;
        m_color = color;
        m_showWhenAnimRuns = showWhenAnimRuns;
        m_animSpeed = animSpeed;
        m_positionRight = positionRight;
    }
}
