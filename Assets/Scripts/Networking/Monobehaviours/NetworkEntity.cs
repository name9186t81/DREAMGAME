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
        private byte _owner;
        private bool _isOwner;
        private bool _fullyInited;
        private int _entityID;
        private int _spawnID;

        /// <summary>
        /// Called once the object spawns from server
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool Init(byte[] data)
        {
            return true;
        }

        public void Kill(bool sendMessage)
        {

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

        public bool FullyInited => _fullyInited;
        public Permissions SpawnPermission => _spawnPermission;
        public Permissions KillPermission => _killPermision;
    }
}