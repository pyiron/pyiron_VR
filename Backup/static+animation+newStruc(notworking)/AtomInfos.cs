using UnityEngine;
 
public class AtomInfos
{
    // the individual ID each atom has
    public int m_ID;
    // the type of the atom, f.e. He
    public string m_type;
    // the transform of the atom
    public Transform m_transform;
    // the data, how far the player has currently moved the atom and how he resized it
    public Transform m_ctrlTrans;


    // create an object which holds the infos of an atom
    public AtomInfos(int ID, string type, Transform transform)
    {
        m_ID = ID;
        m_type = type;
        m_transform = transform;
        m_ctrlTrans = new GameObject().transform;
    }
}
