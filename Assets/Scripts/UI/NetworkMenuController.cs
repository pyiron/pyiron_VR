using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuController : MenuController {
    public static NetworkMenuController inst;

    public Text portText;
    public GameObject keyboard;
    private Button[] serverSuggestions;
    private InputField serverAddressField;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        portText.text = "Port: " + TCPClient.PORT;
        serverSuggestions = gameObject.GetComponentsInChildren<Button>();
        serverAddressField = gameObject.GetComponentInChildren<InputField>();
        
        // set the Input field to the last entered value
        serverAddressField.text = PlayerPrefs.GetString("ServerIp", "");
    }

    private void Update()
    {
        foreach (Button btn in serverSuggestions)
        {
            btn.interactable = TCPClient.connStatus == null;
        }
        serverAddressField.interactable = TCPClient.connStatus == null;
    }

    // Activate a keyboard in VR
    // The keyboard allows to Input a costume server IP
    public void OnKeyboardToggle(Toggle toggle)
    {
        keyboard.SetActive(toggle.isOn);
    }

    // receive Input from the keyboard and set the IP Address accordingly
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
        else
        {
            serverAddressField.text += keyText.text;
        }
    }
}
