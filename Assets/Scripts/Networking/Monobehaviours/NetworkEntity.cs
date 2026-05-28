using Networking.Packages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Networking
{
    public abstract class NetworkEntity : MonoBehaviour
    {
        public enum Permissions
        {
            Everyone,
            HostOnly
        }
        [SerializeField] private Permissions _spawnPermission;
        [SerializeField] private Permissions _killPermision;
        private ConcurrentStack<IPackage> _receivedPackages = new ConcurrentStack<IPackage>();
        private ConcurrentQueue<byte> _snapshotRequest = new ConcurrentQueue<byte>();
        private ConcurrentQueue<(byte[] data, int startOffset, int size)> _pendingSnapshot = new ConcurrentQueue<(byte[] data, int startOffset, int size)>();

        private Vector3 _position;
        private Quaternion _rotation;
        private byte[] _spawnData;
        private byte _owner;
        private bool _isOwner;
        private bool _fullyInited;
        private int _entityID;
        private int _spawnID;

        private void Update()
        {
            _position = transform.position;
            _rotation = transform.rotation;

            while (_receivedPackages.TryPop(out var package))
            {
                if (!ProcessPackage(package))
                {
                    Debug.LogError($"Failed to process {package} on {gameObject.name} gameObject");
                }
            }

            if (_snapshotRequest.Count != 0)
            {
                var snapshot = GetSnapshot();

                while (_snapshotRequest.TryDequeue(out var point))
                {
                    SendPackageToServer(new SnapshotPackage(_entityID, point, snapshot));
                }
            }

            while (_pendingSnapshot.TryDequeue(out var val))
            {
                TryProcessSnapshot(val.data, val.startOffset, val.size);
            }

            UpdateInternal();
        }

        protected virtual void UpdateInternal()
        {

        }

        /// <summary>
        /// Called once the object spawns from server, or once the object is created(before Awake).
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Init(byte[] data)
        {
            _spawnData = data;
            return true;
        }

        protected void SendEvent(EntityEvent @event, byte[] data)
        {
            SendPackageToServer(new EntityEventPackage(_entityID, (byte)@event, NetworkManager.Instance.Client.RunTime, data));
        }

        protected virtual void ProcessEvent(EntityEvent @event, byte[] data)
        {

        }

        public void AddSnapshotRequest(byte target)
        {
            _snapshotRequest.Enqueue(target);
        }

        public void ApplySnapshot(byte[] data, int startOffset, int size)
        {
            _pendingSnapshot.Enqueue((data, startOffset, size));
        }

        public virtual IEnumerable<int> GetAllRequiredSystemsForSnapshot() { return Array.Empty<int>(); }

        private byte[] GetSnapshot()
        {
            var systems = GetAllRequiredSystemsForSnapshot();
            List<INetworkEntitySystem> activeSystems = new List<INetworkEntitySystem>();
            Dictionary<INetworkEntitySystem, int> systemToSize = new Dictionary<INetworkEntitySystem, int>();
            int size = 0;

            foreach (var system in systems)
            {
                if (system == -1) continue;

                if (!NetworkEntitySystemSynchronizer.TryGetSystem(system, out var localSystem))
                {
                    Debug.LogError("Failed to add system with id - " + system);
                    continue;
                }

                int locSize = localSystem.GetSizeFor(this);
                if (locSize == 0) continue;

                activeSystems.Add(localSystem);
                size += NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes;
                size += (int)localSystem.MaxSize;
                systemToSize.Add(localSystem, locSize);
                size += locSize;
            }

            if (size > ListenerBase.MTU)
            {
                Debug.LogError("Data overflow when creating snapshot for " + gameObject.name + " data size: " + size);
                return null;
            }

            byte[] array = new byte[size];
            int systemOffset = 0;
            foreach (var system in activeSystems)
            {
                if (!NetworkEntitySystemSynchronizer.TryGetSystemID(system, out var val))
                {
                    Debug.LogError("Failed to find system with id " + val);
                    continue;
                }
                val.Convert(array, systemOffset, NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes);
                systemToSize[system].Convert(array, systemOffset + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes, (int)system.MaxSize);

                if (!system.TryTakeShapshot(this, systemOffset + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes + (int)system.MaxSize, array))
                {
                    Debug.LogError("Failed to take snapshot system: " + system.ToString() + " for object: " + gameObject.name);
                }

                systemOffset += systemToSize[system] + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes + (int)system.MaxSize;
            }

            return array;
        }

        private bool TryProcessSnapshot(byte[] data, int startOffset, int size)
        {
            int systemOffset = 0;

            while (systemOffset < size)
            {
                int type = (NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes == 1 ? data[systemOffset + startOffset] : BitConverter.ToInt16(data, systemOffset + startOffset));

                if (!NetworkEntitySystemSynchronizer.TryGetSystem(type, out var system))
                {
                    Debug.LogError("Failed to find type - " + type);
                    return false;
                }

                int dataSize = system.MaxSize == INetworkEntitySystem.HeaderSize.Byte ? data[systemOffset + startOffset + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes] : BitConverter.ToInt16(data, systemOffset + startOffset + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes);
                system.TryProcessShapshot(this, systemOffset + startOffset + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes + (int)system.MaxSize, dataSize, data);

                systemOffset += dataSize + NetworkEntitySystemSynchronizer.SystemHeaderSizeInBytes + (int)system.MaxSize;
            }

            return true;
        }

        public virtual void Kill(bool sendMessage)
        {
            if (sendMessage)
            {
                SendPackageToServer(new EntityDestroyPackagePackage(NetID));
            }
        }

        public virtual byte[] GetSpawnData(params object[] args)
        {
            return new byte[0];
        }

        public void SetOwner(bool isOwner, byte id)
        {
            _isOwner = isOwner;
            if (_isOwner)
            {
                _owner = id;
            }
        }

        public void ApplyPackage(IPackage package)
        {
            _receivedPackages.Push(package);
        }

        protected virtual bool ProcessPackage(IPackage package)
        {
            if(package.Type == PackageType.EntityEvent)
            {
                EntityEventPackage converted = (EntityEventPackage)package;
                ProcessEvent((EntityEvent)converted.EventID, converted.EventData);
            }
            return true;
        }

        protected void SendPackageToServer(IPackage package)
        {
            if (!NetworkManager.Instance.TryToSendPackageToServer(package))
            {
                Debug.LogError($"gameObject {gameObject.name} failed to send package to server");
            }
        }

        public void SetSpawnID(int id)
        {
            _spawnID = id;
        }

        public void SetID(int id, bool isServerID)
        {
            _entityID = id;
            if (isServerID)
            {
                _fullyInited = true;

                if (NetworkEntitySystemSynchronizer.TryGetSnapshot(id, out var snapshot))
                {
                    ApplySnapshot(snapshot.data, snapshot.offset, snapshot.size);
                }
            }
        }

        public bool IsOwner => _isOwner;
        public int NetID => _entityID;
        public byte OwnerID => _owner;
        public byte[] SpawnData => _spawnData;
        public int SpawnID => _spawnID;
        public bool FullyInited => _fullyInited;
        public Permissions SpawnPermission => _spawnPermission;
        public Permissions KillPermission => _killPermision;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
    }
}