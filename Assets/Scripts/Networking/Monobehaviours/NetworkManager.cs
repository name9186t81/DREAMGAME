using Networking.Packages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Networking
{
    public sealed class NetworkManager : MonoBehaviour
    {
        private struct SpawnData
        {
            public int SpawnID;
            public int LocalID;
            public Vector3 Position;
            public Quaternion Rotation;
            public byte Client;
            public byte[] Data;

            public SpawnData(int spawnID, int localID, Vector3 position, Quaternion rotation, byte client, byte[] data)
            {
                SpawnID = spawnID;
                LocalID = localID;
                Position = position;
                Rotation = rotation;
                Client = client;
                Data = data;
            }
        }

        [Header("Debug Levels")]
        [SerializeField] private ListenerBase.DebugLevel _clientDebugLevel;
        [SerializeField] private ListenerBase.DebugLevel _serverDebugLevel;

        [Header("Tick Rate")]
        [SerializeField] private int _tickRate = 20;

        [Header("NetEntities")]
        [SerializeField] private NetworkEntity[] _entities;
        private ConcurrentPairedDictionary<NetworkEntity, int> _entitiesIDs = new ConcurrentPairedDictionary<NetworkEntity, int>();
        private ConcurrentPairedDictionary<NetworkEntity, int> _spawnedEntitiesIDs = new ConcurrentPairedDictionary<NetworkEntity, int>();
        private Dictionary<int, NetworkEntity> _pendingObjectsForServerID = new Dictionary<int, NetworkEntity>();
        private Dictionary<int, NetworkEntity> _spawnedObjects = new Dictionary<int, NetworkEntity>();
        private ConcurrentBag<SpawnData> _pendingSpawns = new ConcurrentBag<SpawnData>();

        [Header("Debug")]
        [SerializeField] private long _clientRunTime;
        [SerializeField] private long _serverRuntime;


        private bool _needFinishConnectEvent;
        public event Action OnFinishConnect;

        private Client _client;
        private Server _server;

        private int _localID;
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

            _instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.name = "NETWORK_MANAGER";
            for(int i = 0; i < _entities.Length; i++)
            {
                _entitiesIDs.Add(_entities[i], i);
            }
        }

        private void Update()
        {
            _client?.ChangeDebugLevel(_clientDebugLevel);
            _server?.ChangeDebugLevel(_serverDebugLevel);

            while(_pendingSpawns.TryTake(out var spawnData))
            {
                if(!TrySpawnEntityInternal(spawnData.SpawnID, spawnData.LocalID, spawnData.Position, spawnData.Rotation, spawnData.Client, spawnData.Data))
                {
                    Debug.LogError("Failed to spawn net object");
                }
            }

            if (_needFinishConnectEvent)
            {
                Debug.Log("YG");
                _needFinishConnectEvent = false;
                OnFinishConnect?.Invoke();
            }

            if (ServerExists)
            {
                _serverRuntime = _server.RunTime;
            }
            if (ClientExists)
            {
                _clientRunTime = _client.RunTime;
            }
        }

        public void CreateServer(int workers, int listeners, int port)
        {
            if (ServerExists)
            {
                Debug.LogError("Server already exists");
                return;
            }
            _server = new Server(workers, listeners, port);
        }

        public bool TryCreateEntity<T>(T prefab, Vector3 position, Quaternion rot, out T result, params object[] spawnArgs) where T : NetworkEntity
        {
            if(!_entitiesIDs.TryGet(prefab, out var id))
            {
                Debug.LogError("No id for entity prefab - " + prefab);
                result = null;
                return false;
            }

            if(_client == null)
            {
                Debug.LogError("Client not initiated");
                result = null;
                return false;
            }

            if(_client.Server == null)
            {
                Debug.LogError("Client not connected");
                result = null;
                return false;
            }

            if(prefab.SpawnPermission == NetworkEntity.Permissions.HostOnly)
            {
                Debug.LogError($"Trying to spawn({prefab}) with host only permission");
                result = null;
                return false;
            }

            var data = prefab.GetSpawnData(spawnArgs);
            if(data.Length > ListenerBase.MTU)
            {
                Debug.LogError("Spawn data overflow on " + prefab.name);
                result = null;
                return false;
            }

            var clone = Instantiate(prefab, position, rot);
            clone.SetSpawnID(id);
            clone.SetOwner(true, Client.ID);
            int localID = LocalID;
            clone.SetID(localID, false);
            clone.Init(data);
            _pendingObjectsForServerID.Add(localID, clone);
            _client.SendPackageToServerNextTick(new NetObjectSpawnPackage(position, rot.eulerAngles, localID, id, _client.ID, data));
            _spawnedEntitiesIDs.Add(clone, localID);

            result = clone;
            return true;
        }

        public void SpawnEntity(int spawnID, int netID, Vector3 position, Quaternion rot, byte client, byte[] data)
        {
            _pendingSpawns.Add(new SpawnData(spawnID, netID, position, rot, client, data));
        }

        public void AssignID(int clientID, int serverID)
        {
            if(_pendingObjectsForServerID.TryGetValue(clientID, out var netObj))
            {
                netObj.SetID(serverID, true);

                _spawnedEntitiesIDs.Remove(clientID);
                _spawnedEntitiesIDs.Add(netObj, serverID);
            }
        }

        private bool TrySpawnEntityInternal(int spawnID, int netID, Vector3 position, Quaternion rot, byte client, byte[] data)
        {
            if(!_entitiesIDs.TryGet(spawnID, out var prefab))
            {
                Debug.LogError("Unknown entity with id " + spawnID);
                return false;
            }

            var clone = Instantiate(prefab, position, rot);
            if (!clone.Init(data))
            {
                Debug.LogError("failed to init custom data on " + clone);
                Destroy(clone);
                return false;
            }

            _spawnedObjects.Add(netID, clone);
            clone.SetSpawnID(spawnID);
            clone.SetOwner(false, client);
            clone.SetID(netID, true);
            _spawnedEntitiesIDs.Add(clone, netID);
            return true;
        }

        public void KillEntity(int id)
        {
            if (TryGetEnityByID(id, out var entity))
            {
                entity.Kill(false);
            }
        }

        public void CreateServer(int port)
        {
            if (ServerExists)
            {
                Debug.LogError("Server already exists");
                return;
            }
            _server = new Server(-1, -1, port);
        }

        public void CreateClient(int workers, int listeners)
        {
            if (ClientExists)
            {
                Debug.LogError("Client already exists");
                return;
            }
            _client = new Client(workers, listeners);
            _client.OnFinishConnect += FinishConnect;
        }

        private void FinishConnect()
        {
            _needFinishConnectEvent = true;
        }

        public void CreateClient()
        {
            if (ClientExists)
            {
                Debug.LogError("Client already exists");
                return;
            }
            _client = new Client(-1, -1);
            _client.OnFinishConnect += FinishConnect;
        }

        public void KillServer()
        {
            if (ServerExists)
            {
                _server.Kill();
            }
        }

        public void KillClient()
        {
            if (ClientExists)
            {
                _client.Kill();
            }
        }

        public bool ApplyPackageToEntity(int entityID, IPackage package)
        {
            if (_spawnedObjects.TryGetValue(entityID, out var entity))
            {
                entity.ApplyPackage(package);
                return true;
            }
            return false;
        }

        public NetworkEntity GetEntityByID(int id)
        {
            return _entitiesIDs[id];
        }

        public bool TryGetSpawnedEnityByID(int id, out NetworkEntity entity)
        {
            return _spawnedObjects.TryGetValue(id, out entity);
        }

        public bool TryGetEnityByID(int id, out NetworkEntity entity)
        {
            return _entitiesIDs.TryGet(id, out entity);
        }

        public bool TryToSendPackageToServer(IPackage package)
        {
            if (Connected)
            {
                _client.SendPackageToServer(package);
                return true;
            }
            return false;
        }

        public bool OwnsEntity(int id, byte owner)
        {
            if(TryGetEnityByID(id, out var entity))
            {
                return entity.OwnerID == owner;
            }
            return false;
        }

        public IEnumerable<KeyValuePair<NetworkEntity, int>> Entities => _spawnedEntitiesIDs;
        private int LocalID => _localID++;
        private bool ServerExists => _server != null;
        private bool ClientExists => _client != null;
        public Server Server => _server;
        public Client Client => _client;
        public bool IsHost => _server != null && _client != null;
        public bool Connected => _client != null && _client.Server != null;

    }
}