
using UnityEngine.UI;

public class NetworkMenuController : MenuController {
    public static NetworkMenuController inst;

    public Text portText;

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        portText.text = "Port: " + TCPClient.PORT;
    }
}
