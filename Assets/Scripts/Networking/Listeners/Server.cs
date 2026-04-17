using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Networking
{
    public class Server : ListenerBase
    {
        private readonly ConcurrentDictionary<IPEndPoint, byte> _userIDs;
        private readonly ConcurrentDictionary<byte, IPEndPoint> _idToUser;
        private readonly ConcurrentStack<byte> _freeIDs;
        private readonly ConcurrentDictionary<int, byte> _objectOwners = new ConcurrentDictionary<int, byte>();
        private byte _currentID;
        private int _networkObjectID = 255;

        public Server(int workerThreads = -1, int listenerThreads = -1, int listeningPort = -1) : base(workerThreads, listenerThreads, listeningPort)
        {
            SetListenerName("Server");

            _userIDs = new ConcurrentDictionary<IPEndPoint, byte>();
            _idToUser = new ConcurrentDictionary<byte, IPEndPoint>();
            _freeIDs = new ConcurrentStack<byte>();
        }

        protected override bool Process(PackageType type, in ReadOnlySpan<byte> memory, IPEndPoint sender)
        {
            if (TryGetProcessorByType(type, out var processor))
            {
                try
                {
                    processor.Process(memory, CTS, sender, this);
                    return true;
                }
                catch (Exception ex)
                {
                    DebugMessageError(ex.Message, DebugLevel.Low);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void SendPackageNextTickToEveryoneExcept(IPackage package, IPEndPoint destination)
        {
            foreach (var connected in ConnectedUsers)
            {
                if (!connected.Key.Equals(destination))
                {
                    SendPackageNextTick(package, connected.Key);
                }
            }
        }

        public void SendPackageNextTickToEveryone(IPackage package, IPEndPoint destination)
        {
            foreach (var connected in ConnectedUsers)
            {
                SendPackageNextTick(package, connected.Key);
            }
        }


        public void RegisterUser(IPEndPoint point, out byte id)
        {
            if (_userIDs.TryGetValue(point, out id)) return;

            id = GetNextID();
            _userIDs.TryAdd(point, id);
            _idToUser.TryAdd(id, point);
        }

        public void RemoveConnected(IPEndPoint client)
        {
            if (_userIDs.TryRemove(client, out var userID))
            {
                _idToUser.TryRemove(userID, out _);
                _freeIDs.Push(userID);
            }
        }

        public bool TryGetUserID(IPEndPoint point, out byte id) => _userIDs.TryGetValue(point, out id);
        public bool TryGetUserByID(byte id, out IPEndPoint user) => _idToUser.TryGetValue(id, out user);

        private byte GetNextID()
        {
            if (_freeIDs.TryPop(out var id))
            {
                return id;
            }

            return _currentID++;
        }

        public void AddObjectOwner(int networkID, byte userID)
        {
            _objectOwners.TryAdd(networkID, userID);
        }

        public void RemoveObjectOwner(int networkID)
        {
            _objectOwners.TryRemove(networkID, out _);
        }

        public bool TryGetObjectOwner(int objectID, out byte userID) => _objectOwners.TryGetValue(objectID, out userID);

        public IEnumerable<KeyValuePair<int, byte>> AllNetworkObjects => _objectOwners;
        public IEnumerable<KeyValuePair<IPEndPoint, byte>> ConnectedUsers => _userIDs;
        public int NetworkObjectID => _networkObjectID++;
    }
}
