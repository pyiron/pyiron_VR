using System;
using HTC.UnityPlugin.Vive;
using Networking;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuController : MenuController {
    public static NetworkMenuController Inst;

    [SerializeField] private Text portText;
    public MenuController keyboard;
    [SerializeField] private Text serverAddressField;
    [SerializeField] private Text serverAddressPlaceholder;

    private const string ServerIPKey = "ServerIp";
    private const string ServerPortKey = "ServerPort";
    private bool keyboardon = false;
    private bool portkeyboard = false;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        
        // set the Input field to the last entered value
        serverAddressField.text = PlayerPrefs.GetString(ServerIPKey, "");
        portText.text = PlayerPrefs.GetString(ServerPortKey, "" + TCPClientConnector.Inst.PORT);
        UpdatePlaceholder();
    }

    
    /// <summary>
    /// Show the placeholder if and only if the text field is empty
    /// </summary>
    private void UpdatePlaceholder()
    {
        serverAddressPlaceholder.gameObject.SetActive(serverAddressField.text == "");
    }

   
    private void Update()
    {
        if (TCPClientConnector.ConnectionStatus == null)
        {
            base.Activate();
        }
        else
        {
            base.Deactivate();
        }
    }

    public void SetServerAdress(Text server_address)
    {
        serverAddressField.text = server_address.text;
        UpdatePlaceholder();
    }
    
    /// <summary>
    /// Try to connect to the server with the IP address of the text of this button.
    /// </summary>
    /// <param name="btnText">The button that got pressed.</param>
    public void ConnectToBtnText(Text btnText)
    {
        // save the Ip adress
        PlayerPrefs.SetString(ServerIPKey, btnText.text);
        PlayerPrefs.SetString(ServerPortKey, portText.text);
        TCPClientConnector.Inst.UpdatePort(portText.text);


        // The vibration does not get triggered on the Quest, but it should work on the Vive
        // If needed on the Quest, the function of the OVR Plugin could be used
        ViveInput.TriggerHapticVibration(HandRole.LeftHand, 0.2f);
        ViveInput.TriggerHapticVibration(HandRole.RightHand, 0.2f);
		

        TCPClientConnector.Inst.ConnectWithHost(btnText.text);
    }

    /// <summary>
    /// Activate a keyboard in VR.
    /// The keyboard allows to Input a costume server IP.
    /// </summary>
    /// <param name="toggle">The toggle that decides whether the keyboard should be active or inactive.</param>
    public void OnKeyboardToggle()
    {

        // The panels only update if a new panel is activated to save performance, so we have to manually call the
        // update for this case
        ToggleKeyboard();
        portkeyboard = false;
    }

    public void OnConnectToggle(Toggle toggle)
    {
        ConnectToBtnText(serverAddressField);
    }

    public void ToggleKeyboard() 
    {
        keyboardon = !keyboardon ;

        keyboard.SetState(keyboardon);
        UpdatePanelPosition() ;
    }

    public void OnKeyboardButton()
    {
        ToggleKeyboard();
        portkeyboard = true;
     }

    /// <summary>
    /// Receive Input from the keyboard and set the IP Address accordingly.
    /// </summary>
    /// <param name="key">The Button of the key that got pressed</param>
    public void OnKeyboardPressDown(Button key)
    {
        Text keyText = key.GetComponentInChildren<Text>();
        if (keyText.text == "Clear")
        {
            if (portkeyboard)
            {
                portText.text = "";
            }
            else
            {
                serverAddressField.text = "";
            }
        } else if (keyText.text == "Del")
        {
            // remove the last char
            if (portkeyboard && portText.text != "")
            {
                portText.text =
                    portText.text.Substring(0, portText.text.Length - 1);
            }
            else if (!portkeyboard  && serverAddressField.text != ""){
                serverAddressField.text =
                    serverAddressField.text.Substring(0, serverAddressField.text.Length - 1);
            }
        }
        else if (keyText.text != "Del")
        {
            if (portkeyboard) {
                portText.text += keyText.text;
            }
            else
            {
                serverAddressField.text += keyText.text;
            }
        }
        
        UpdatePlaceholder();
    }
}
