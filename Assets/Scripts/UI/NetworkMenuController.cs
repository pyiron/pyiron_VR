using Networking;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuController : MenuController {
    public static NetworkMenuController Inst;

    [SerializeField] private Text portText;
    public GameObject keyboard;
    [SerializeField] private Text serverAddressField;
    [SerializeField] private Text serverAddressPlaceholder;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        portText.text = "Port: " + TCPClientConnector.PORT;
        
        // set the Input field to the last entered value
        serverAddressField.text = PlayerPrefs.GetString("ServerIp", "");
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

    /// <summary>
    /// Activate a keyboard in VR.
    /// The keyboard allows to Input a costume server IP.
    /// </summary>
    /// <param name="toggle">The toggle that decides whether the keyboard should be active or inactive</param>
    public void OnKeyboardToggle(Toggle toggle)
    {
        keyboard.SetActive(toggle.isOn);
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
            serverAddressField.text = "";
        } else if (keyText.text == "Del" && serverAddressField.text != "")
        {
            // remove the last char
            serverAddressField.text = 
                serverAddressField.text.Substring(0, serverAddressField.text.Length - 1);
        }
        else if (keyText.text != "Del")
        {
            serverAddressField.text += keyText.text;
        }
        
        UpdatePlaceholder();
    }
}
