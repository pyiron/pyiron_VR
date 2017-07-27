using UnityEngine;


public class AtomInfos
{
    public int m_ID;
    public string m_type;
    public Transform m_transform;


    public AtomInfos(int ID, string type, Transform transform)
    {
        m_ID = ID;
        m_type = type;
        m_transform = transform;
    }
}
