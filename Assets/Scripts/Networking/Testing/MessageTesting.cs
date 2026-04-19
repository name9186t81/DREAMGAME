using UnityEngine;
using System.Net;

namespace Networking.Testing
{
    public sealed class MessageTesting : MonoBehaviour
    {
        [SerializeField] private int _serverPort;
        [SerializeField] private string _message;
        [SerializeField] private bool _sendToServer;
        [SerializeField] private bool _sendToClient;

        private Server _server;
        private Client _client;

        private void Awake()
        {
            _server = new Server(-1, -1, _serverPort);
            _client = new Client(1, 1);

            _server.ChangeDebugLevel(ListenerBase.DebugLevel.High);
            _client.ChangeDebugLevel(ListenerBase.DebugLevel.High);
        }

        private void Update()
        {
            if (_sendToServer)
            {
                _sendToServer = false;
                _client.SendTestMessage(_message, new IPEndPoint(IPAddress.Parse("127.0.0.1"), _serverPort));
            }
            if (_sendToClient)
            {
                _sendToClient = false;
                _server.SendTestMessage(_message, new IPEndPoint(IPAddress.Parse("127.0.0.1"), _client.OwnPort));
            }
        }
    }
}
