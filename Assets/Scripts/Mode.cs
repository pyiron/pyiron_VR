using UnityEngine;


public class Mode
{
    // create the variables, which hold the data of the properties of an element
    public readonly string name;
    public readonly bool playerCanMoveAtoms;
    public readonly bool showTemp;
    public readonly bool showRelaxation;
    public readonly bool showInfo;
    public readonly bool canDuplicate;
    bool showTrashcan;

    // create an object which holds the infos of a mode
    public Mode(string m_name, bool m_playerCanMoveAtoms = false, bool m_showTemp = false, bool m_showRelaxation = false,
        bool m_showInfo = false, bool m_canDuplicate = false, bool m_showTrashcan=false)
    {
        name = m_name;
        playerCanMoveAtoms = m_playerCanMoveAtoms;
        showTemp = m_showTemp;
        showRelaxation = m_showRelaxation;
        showInfo = m_showInfo;
        canDuplicate = m_canDuplicate;
        showTrashcan = m_showTrashcan;
    }
}
