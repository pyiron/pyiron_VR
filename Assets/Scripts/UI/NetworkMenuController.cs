using UnityEngine.UI;

public class NetworkMenuController : MenuController {
    public static NetworkMenuController inst;

    public Text portText;
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
    }

    private void Update()
    {
        foreach (Button btn in serverSuggestions)
        {
            btn.interactable = TCPClient.connStatus == null;
        }
        serverAddressField.interactable = TCPClient.connStatus == null;
    }
}
