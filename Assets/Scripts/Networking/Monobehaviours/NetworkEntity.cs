using Networking.Packages;
using System.Collections.Concurrent;
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
        private byte _owner;
        private bool _isOwner;
        private bool _fullyInited;
        private int _entityID;
        private int _spawnID;

        private void Update()
        {
            while(_receivedPackages.TryPop(out var package))
            {
                if (!ProcessPackage(package))
                {
                    Debug.LogError($"Failed to process {package} on {gameObject.name} gameObject");
                }
            }
            UpdateInternal();
        }

        protected virtual void UpdateInternal()
        {

        }

        /// <summary>
        /// Called once the object spawns from server
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Init(byte[] data)
        {
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
            }
        }

        public bool IsOwner => _isOwner;
        public int NetID => _entityID;
        public byte OwnerID => _owner;
        public bool FullyInited => _fullyInited;
        public Permissions SpawnPermission => _spawnPermission;
        public Permissions KillPermission => _killPermision;
    }
}