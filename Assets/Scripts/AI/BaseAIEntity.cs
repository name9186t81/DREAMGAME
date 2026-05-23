using Mechanics.Health;
using Networking;
using Networking.Packages;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
    public class BaseAIEntity : NetworkEntity, IDamageReactable
    {
        [SerializeField] private bool _autoSyncTransform;
        [SerializeField] private LayerMask _visionMask;
        [SerializeField] private float _visionRadius;
        [SerializeField] private float _maxHealth;
        private Vector3 _previousPosition;
        private Vector3 _previousRotation;

        public event Action<DamageArgs> OnDamage;

        protected override void UpdateInternal()
        {
            if (_autoSyncTransform &&
                NetworkManager.Instance.Connected && 
                Vector3.Dot(_previousRotation, transform.eulerAngles) < 0.9f || (_previousPosition - transform.position).sqrMagnitude > 0.1f &&
                IsOwner)
            {
                _previousPosition = transform.position;
                _previousRotation = transform.eulerAngles;
                SendPackageToServer(new TransformSyncPackage(_previousPosition, _previousRotation, NetID, NetworkManager.Instance.Client.RunTime));
            }

            if (IsOwner)
            {

            }
        }

        private void Start()
        {
            _previousPosition = transform.position;
            _previousRotation = transform.eulerAngles;
        }

        public override bool Init(byte[] data)
        {
            return base.Init(data);
        }

        protected override bool ProcessPackage(IPackage package)
        {
            if(package.Type == PackageType.TransformSync) //todo: add interpolation
            {
                var sync = (TransformSyncPackage)package;
                transform.position = sync.Position;
                transform.eulerAngles = sync.Rotation;
            }
            return true;
        }

        public void React(DamageArgs args)
        {
            throw new NotImplementedException();
        }
    }
}