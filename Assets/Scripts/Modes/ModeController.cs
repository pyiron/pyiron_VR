using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// component of CurrentModeText
public class ModeController : MonoBehaviour
{
    [Header("Scene")]
    // reference to the deployed instance of this script
    public static ModeController inst;

    [Header("Modes")]
    // the dictionary which defines what properties each mode has
    // attention: the trashcan will just be shown if m_playerCanMoveAtoms is true, even if m_showTrashcan is true
    // attention: the mode will just be accessible, if m_playerCanMoveAtoms, m_showInfo or m_canDuplicate is true
    internal static List<Mode> modes;
    
    // get the textmesh from the 3D Text which shows the current mode
    //public TextMesh CurrentModeText;
    public static Mode currentMode;
    
    // remember the new mode which should be set with the main thread
    internal Modes newMode;

    

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        modes = new List<Mode>
        {
            new Mode(mode:Modes.Network, NetworkMenuController.Inst, hideAtoms: true),
            new Mode(mode:Modes.Explorer, ExplorerMenuController.Inst, playerCanResizeAtoms:true),
            new Mode(mode:Modes.Calculate, SimulationMenuController.Inst, playerCanResizeAtoms:true, showTemp:true,
                showTrashcan:true),
            //new Mode(mode:Modes.Minimize, playerCanMoveAtoms:true, playerCanResizeAtoms:true, showRelaxation:true,
            //    showTrashcan:true),
            //new Mode(mode:Modes.Animate),
            new Mode(mode:Modes.Structure, StructureMenuController.Inst, playerCanMoveAtoms:true, playerCanResizeAtoms:true),
        };
        
        SetMode(Modes.Network);
        UpdateScene();
    }

    void Update()
    {
        if (newMode != Modes.None) { 
            SetMode(newMode);
            newMode = Modes.None;
        }
    }

    public void SetMode(string newMode)
    {
        SetMode((Modes)Enum.Parse(typeof(Modes), newMode));
    }

    // change the mode. This includes updating scene, e.g. (de)activating the thermometer or UI
    public void SetMode(Modes newMode)
    {
        if (currentMode.mode != Modes.None && currentMode.mode != Modes.Explorer &&
            currentMode.mode != Modes.Network)
        {
            // stop the currently running animation
//            AnimationController.RunAnim(true);
        }

        currentMode = modes[(int)newMode];
        UpdateScene();
    }

    // (de)activate objects in the scene, as well as the menu
    private void UpdateScene() { 
        ModeText.Inst.OnModeChange();

        if (Thermometer.temperature != -1)
            // activate the thermometer when changing into temperature mode, else deactivate it
            Thermometer.Inst.gameObject.SetActive(modes[(int)currentMode.mode].showTemp);

        // if (modes[(int)currentMode.mode].showInfo)
            // OrdersToPython.RequestAllForces();
        // deactivate the structure if it shouldn't be shown, else activate it
        StructureDataOld.Inst.gameObject.SetActive(!modes[(int)currentMode.mode].hideAtoms);

        if (currentMode.mode == Modes.Explorer)
        {
            ProgramSettings.inst.ResetScene();
        }

        UpdateMenu();

        foreach (LaserGrabber lg in LaserGrabber.instances)
            if (lg.gameObject.activeSelf)
            {
                // activate the symbols of the controller, if changing into a mode which can play an animation, else deactivate them
                if (modes[(int)currentMode.mode].showTemp || modes[(int)currentMode.mode].showRelaxation)
                {
                    LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].Symbols.SetActive(true);
                    LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].SetSymbol();
                }
                else {
                    GameObject symbols = LaserGrabber.controllerSymbols[(int) lg.ctrlLayer].Symbols;
                    if (symbols != null)
                        symbols.SetActive(false);
                }

                // detach the currently attached object from the laser and deactivate the laser
                lg.attachedObject = null;
                lg.laser.SetActive(false);
                lg.readyForResize = false;
                lg.InfoText.gameObject.SetActive(false);
        }
    }

    // determine which panels and buttons should be activated/deactivated
    private void UpdateMenu()
    {
        NetworkMenuController.Inst.SetState(currentMode.mode == Modes.Network);
        ExplorerMenuController.Inst.SetState(currentMode.mode == Modes.Explorer);
        if (currentMode.mode == Modes.Explorer)
        {
            ExplorerMenuController.Inst.OnModeStart();
        }
        MdMenuController.Inst.SetState(currentMode.showTemp &&
                                                SimulationModeManager.CurrMode==SimModes.MD);
        //ModeMenuController.inst.SetState(currentMode.mode == Modes.Menu);
        //ModeMenuController.inst.OnModeChange();
        if (currentMode.mode == Modes.Calculate)
        {
            SimulationMenuController.Inst.OnModeStart();
        }

        bool modeCanHaveAnimation = currentMode.mode == Modes.Explorer || currentMode.mode == Modes.Calculate;
        AnimationController.Inst.enabled = modeCanHaveAnimation;
        if (!modeCanHaveAnimation)
        {        
            AnimationMenuController.Inst.SetState(false);
        }

        //                                      currentMode.mode == Modes.Minimize || currentMode.mode == Modes.Animate);
        SimulationMenuController.Inst.SetState(currentMode.mode == Modes.Calculate);
        //InfoMenuController.inst.SetState(currentMode.showInfo);
        //StructureMenuController.inst.transform.parent.gameObject.SetActive(currentMode.mode == Modes.Explorer);
        //StructureCreatorMenuController.inst.SetState(currentMode.showPeriodicSystem);
        //if (currentMode.showPeriodicSystem)
        //    StructureCreatorMenuController.inst.OnModeChange();
        //PeriodicSysMenuController.inst.SetState(currentMode.mode == Modes.Structure);
        StructureMenuController.Inst.SetState(currentMode.mode == Modes.Structure);
        if (currentMode.mode == Modes.Structure)
        {
            StructureMenuController.Inst.OnModeStart();
        }

        PeriodicTable.Inst.SetState(false);
        // ModeBar.Inst.UpdateButtons(currentMode.mode.ToString());
    }
}
