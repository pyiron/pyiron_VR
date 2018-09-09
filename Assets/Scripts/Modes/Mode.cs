using UnityEngine;


public class Mode
{
    // create the variables, which hold the data of the properties of an element
    public readonly Modes mode;
    public readonly bool playerCanMoveAtoms;
    public readonly bool playerCanResizeAtoms;
    public readonly bool showTemp;
    public readonly bool showRelaxation;
    public readonly bool showInfo;
    public readonly bool canDuplicate;
    public readonly bool showTrashcan;
    public readonly bool hideAtoms;
    public readonly bool showPossibleStructures;

    public Mode(Modes mode, bool playerCanMoveAtoms=false, bool playerCanResizeAtoms = false, bool showTemp = false, bool showRelaxation = false,
        bool showInfo = false, bool canDuplicate = false, bool showTrashcan = false, bool hideAtoms = false, bool showPossibleStructures = false)
    {
        this.mode = mode;
        this.playerCanMoveAtoms = playerCanMoveAtoms;  // switch to type handrole
        this.playerCanResizeAtoms = playerCanResizeAtoms;
        this.showTemp = showTemp;
        this.showRelaxation = showRelaxation;
        this.showInfo = showInfo;
        this.canDuplicate = canDuplicate;
        this.showTrashcan = showTrashcan;
        this.hideAtoms = hideAtoms;
        this.showPossibleStructures = showPossibleStructures;
    }
}

public enum Modes
{
    Explorer, Temperature, Minimize, View, Info, None
}
