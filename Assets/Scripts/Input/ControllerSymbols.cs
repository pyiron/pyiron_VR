using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// component of both controllers
public class ControllerSymbols : MonoBehaviour {
    // an array which contains all the GameObjects of the symbols
    private GameObject[] AnimSymbols = new GameObject[10];
    // an empty GameObject which holds the symbols
    public GameObject Symbols;

    // the dictionary that holds all information about the symbols
    private static readonly Dictionary<string, Symbol> controllerSymbols = new Dictionary<string, Symbol>
    {
        {"Triangle 0", new Symbol(new Vector3(0.002f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"Pause 0", new Symbol(new Vector3(0.000f, 0, -0.049f), new Vector3(-90, 0, 0), 0.005f, new Color(1, 1, 1, 1), true) },
        {"FastForward 0", new Symbol(new Vector3(-0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 5, 0.028f) },
        {"FastForward 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), true, 0) },
        {"FrameForward 0", new Symbol(new Vector3(0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"FrameForward 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.005f, new Color(1, 1, 1, 1), false) },
        {"Triangle 2", new Symbol(new Vector3(-0.012f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 4, 0.028f) },
        {"Triangle 1", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.003f, new Color(1, 1, 1, 1), true, 1, 0.028f) },
        {"TimelapsForward 0", new Symbol(new Vector3(-0.016f, 0.005f, -0.049f), new Vector3(-90, 0 -90), 0.008f, new Color(1, 1, 1, 1), true, 3, 0.028f) },
        {"TimelapsBackward 0", new Symbol(new Vector3(-0.012f, 0.0035f, -0.049f), new Vector3(-90, 0 -90), 0.008f, new Color(1, 1, 1, 1), true, 2, 0.028f) }
    };

    // Use this for initialization
    void Start () {
        // create an empty GameObject which holds the symbols
        Symbols = new GameObject();
        // rename the new GameObject to Symbols
        Symbols.name = "Symbols";
        // make it a child of the controller
        Symbols.transform.parent = transform;
        // set its localPosition and localEulerAngles to zero, so that the symbols will be at the right place
        Symbols.transform.localPosition = Vector3.zero;
        Symbols.transform.localEulerAngles = Vector3.zero;

        //AnimSymbols = Resources.LoadAll("ControllerSymbols", typeof(GameObject)) as GameObject[];
        int objectCounter = 0;
        // set each symbol to the right place on the controller, with the right size and tell it its name
        foreach (string symbolKey in controllerSymbols.Keys)
        {
            // create a new symbol
            AnimSymbols[objectCounter] = Instantiate(Resources.Load("ControllerSymbols/" + symbolKey.Split()[0]) as GameObject);
            // set its name to the form it is and how it can be found in the dictionary
            AnimSymbols[objectCounter].name = symbolKey;
            // set its parent to the controller.
            AnimSymbols[objectCounter].transform.parent = Symbols.transform;
            // set it's rotation so that it's above the controller, not going in the controller
            AnimSymbols[objectCounter].transform.localEulerAngles = controllerSymbols[symbolKey].m_rotation;
            if (symbolKey.Split()[1] == "1")
                AnimSymbols[objectCounter].transform.localEulerAngles += Vector3.forward * 180;
            // set the position of the symbol
            AnimSymbols[objectCounter].transform.localPosition = controllerSymbols[symbolKey].m_position;
            // set the size of the symbol
            AnimSymbols[objectCounter].transform.localScale = Vector3.one * controllerSymbols[symbolKey].m_size;

            objectCounter += 1;
        }
        if (ModeController.currentMode.mode != Modes.None)
        {
            if (ModeController.currentMode.showRelaxation || ModeController.currentMode.showTemp)
                // activate the symbols that have to be activated and deactivate the remaining symbols
                SetSymbol();
            else
                // deactivate the symbols, if the mode is already without possible animation when starting
                Symbols.SetActive(false);
        }
    }

    // activate the symbols that have to be activated and deactivate the remaining symbols
    public void SetSymbol()
    {
        Symbol symbolProperties;
        foreach (GameObject AnimSymbol in AnimSymbols)
        {
            symbolProperties = controllerSymbols[AnimSymbol.name];
            // deactivate a symbol, if it should be shown while the animation is on and it isn't on or vice versa
            if (symbolProperties.m_showWhenAnimRuns == AnimationController.run_anim)
                if (AnimationController.run_anim)
                    // activate the pause symbol, because it should be always active if the animation is active
                    if (symbolProperties.m_animSpeed == -1)
                        AnimSymbol.SetActive(true);
                    // activate the symbol which show which animation speed will come when clicking on the left side
                    else if (symbolProperties.m_animSpeed == AnimationController.animSpeed - 1)
                    {
                        AnimSymbol.SetActive(true);
                        // set the symbol on the left side of the touchpad
                        AnimSymbol.transform.localPosition = symbolProperties.m_position;
                    }
                    // activate the symbols which show which animation speed will come when clicking on the right side
                    else if (symbolProperties.m_animSpeed == AnimationController.animSpeed + 1)
                    {
                        AnimSymbol.SetActive(true);
                        // set the symbol on the right side of the touchpad
                        AnimSymbol.transform.localPosition = symbolProperties.m_position + Vector3.right * symbolProperties.m_positionRight;
                    }
                    else
                        // deactivate all other symbols
                        AnimSymbol.SetActive(false);
                else
                    // activate all symbols that should be active when the animation isn't,
                    // because they don't depend on the animation speed
                    AnimSymbol.SetActive(true);
            else
                AnimSymbol.SetActive(false);
        }
    }
}
