using Networking;
using Networking.Packages;
using System;
using TMPro;
using UnityEngine;

public class Player : NetworkEntity
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private Camera _playerCamera;
    private Vector3 _previousPosition;
    private Vector3 _previousRotation;

    public event Action<EntityEvent, byte[]> OnEvent;

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
            _movement.DisableInput();
            _movement.Freeze();
            _playerCamera.enabled = false;
            _input.enabled = false;
        }
        else
        {
            _movement.OnJump += Jump;
            _movement.OnSlideStart += Slide;
        }
        return base.Init(data);
    }

    private void Slide()
    {
        SendEvent(EntityEvent.Slide, null);
    }

    private void Jump()
    {
        SendEvent(EntityEvent.Jump, null);
    }

    protected override void ProcessEvent(EntityEvent @event, byte[] data)
    {
        OnEvent?.Invoke(@event, data);
        if (@event == EntityEvent.Slide)
        {
            _movement.Slide();
        }
        else if (@event == EntityEvent.Jump)
        {
            _movement.Jump();
        }

        base.ProcessEvent(@event, data);
    }

    public void SendNewEvent(EntityEvent @event, byte[] data)
    {
        SendEvent(@event, data);
    }

    protected override bool ProcessPackage(IPackage package)
    {
        if (package.Type == PackageType.TransformSync) //todo: add interpolation
        {
            var sync = (TransformSyncPackage)package;
            transform.position = sync.Position;
            transform.eulerAngles = sync.Rotation;
        }
        base.ProcessPackage(package);
        return true;
    }
}
