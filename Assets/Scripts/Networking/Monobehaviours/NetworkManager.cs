using Networking.Packages;
using Networking.Testing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public sealed class NetworkManager : MonoBehaviour
    {
        [Header("Debug Levels")]
        [SerializeField] private ListenerBase.DebugLevel _clientDebugLevel;
        [SerializeField] private ListenerBase.DebugLevel _serverDebugLevel;

        [Header("Tick Rate")]
        [SerializeField] private int _tickRate = 20;

        [Header("Testing")]
        [SerializeField] private ActorSyncerDisplayer _debugDisplayer;
        private ConcurrentDictionary<byte, ActorSyncerDisplayer> _idToDisplayer = new ConcurrentDictionary<byte, ActorSyncerDisplayer>();
        private ConcurrentBag<TestActorSyncPackage> _testPackages;

        private Client _client;
        private Server _server;

        private static NetworkManager _instance;
        public static NetworkManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogError("NETWORK MANAGER ALREADY EXIST");
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            gameObject.name = "NETWORK_MANAGER";
        }

        private void Update()
        {
            while(_testPackages.TryTake(out var package))
            {
                if (_idToDisplayer.TryGetValue(package.ClientID, out var actorSyncer))
                {
                    actorSyncer.AddPackage(package);
                }
                else
                {
                    var obj = Instantiate(actorSyncer);
                    actorSyncer.AddPackage(package);
                }
            }
        }
        public void CreateServer(int workers, int listeners, int port)
        {
            _server = new Server(workers, listeners, port);
        }

        public void CreateServer(int port)
        {
            _server = new Server(-1, -1, port);
        }

        public void CreateClient(int workers, int listeners)
        {
            _client = new Client(workers, listeners);
        }

        public void CreateClient()
        {
            _client = new Client(-1, -1);
        }

        public void UpdateDisplayer(TestActorSyncPackage package)
        {
            _testPackages.Add(package);
        }

        public Server Server => _server;
        public Client Client => _client;
    }
}