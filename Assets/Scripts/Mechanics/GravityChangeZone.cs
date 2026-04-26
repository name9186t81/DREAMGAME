using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GravityChangeZone : MonoBehaviour
{
    [SerializeField] private bool _useX;
    [SerializeField] private bool _useY;
    [SerializeField] private bool _useZ;

    private void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.TryGetComponent<PlayerMovement>(out var movement))
        {
            movement.SetGravity(GetDirection);
        }
    }

    private void OnDrawGizmos()
    {
        DebugUtils.DebugDrawArrow(transform.position, transform.position + GetDirection);
    }

    private Vector3 GetDirection => _useX ? transform.right : _useY ? transform.up : _useZ ? transform.forward : transform.forward;
}
