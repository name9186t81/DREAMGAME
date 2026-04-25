using Networking;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ClientServerConnectionHandler : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMPro.TMP_InputField _connectIP;
    [SerializeField] private TMPro.TMP_InputField _connectPort;
    [SerializeField] private TMPro.TMP_InputField _serverPort;

    [Header("Inputs")]
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _createServerButton;

    private void Awake()
    {
        _connectButton.onClick.AddListener(() =>
        {
            if(!IPAddress.TryParse(_connectIP.text, out var adress))
            {
                Debug.LogError("Failed to parse IP adress");
            }

            if(!short.TryParse(_connectPort.text, out var port))
            {
                Debug.LogError("Failed to parse port");
            }

            NetworkManager.Instance.CreateClient();
            NetworkManager.Instance.Client.Connect(new IPEndPoint(adress, port));
        });
        _createServerButton.onClick.AddListener(() =>
        {
            if (!short.TryParse(_serverPort.text, out var port))
            {
                Debug.LogError("Failed to parse port");
            }

            NetworkManager.Instance.CreateServer(1, 1, port);
            NetworkManager.Instance.CreateClient();
            NetworkManager.Instance.Client.Connect(new IPEndPoint(IPAddress.Loopback, port));
        });
    }
}
