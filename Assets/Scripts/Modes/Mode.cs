using UnityEngine;


public struct Mode
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
    //public readonly bool showExplorer;
    //public readonly bool showNetwork;
    //public readonly bool showPeriodicSystem;
    //public readonly bool showModes;

    public Mode(Modes mode, bool playerCanMoveAtoms = false, bool playerCanResizeAtoms = false, bool showTemp = false,
        bool showRelaxation = false, bool showInfo = false, bool canDuplicate = false, bool showTrashcan = false,
        bool hideAtoms = false)
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
        //this.showExplorer = showExplorer;
        //this.showNetwork = showNetwork;
        //this.showPeriodicSystem = showPeriodicSystem;
        //this.showModes = showModes;
    }
}

public enum Modes
{
    Network, Explorer, Calculate, Minimize, Animate, Structure, Menu, None // , Info,
}
