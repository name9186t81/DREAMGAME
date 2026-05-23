using Networking;
using Networking.Packages;
using TMPro;
using UnityEngine;

public class Player : NetworkEntity
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Camera _playerCamera;
    private Vector3 _previousPosition;
    private Vector3 _previousRotation;

    protected override void UpdateInternal()
    {
        if (NetworkManager.Instance.Connected &&
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
        if (!IsOwner)
        {
            _movement.enabled = false;
            _playerCamera.enabled = false;
        }
        return base.Init(data);
    }

    protected override bool ProcessPackage(IPackage package)
    {
        if (package.Type == PackageType.TransformSync) //todo: add interpolation
        {
            var sync = (TransformSyncPackage)package;
            transform.position = sync.Position;
            transform.eulerAngles = sync.Rotation;
        }
        return true;
    }
}
