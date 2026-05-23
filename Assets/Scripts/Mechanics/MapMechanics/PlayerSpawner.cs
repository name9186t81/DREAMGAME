using Networking;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 _size;
    [SerializeField] private NetworkEntity _player;

    private void Awake()
    {
        if (NetworkManager.Instance.Connected)
        {
            Spawn();
        }
        else
        {
            NetworkManager.Instance.OnFinishConnect += Spawn;
        }
    }

    private void Spawn()
    {
        NetworkManager.Instance.TryCreateEntity(_player, 
            transform.position + Vector3.right * Random.Range(-_size.x, _size.x) + Vector3.forward * Random.Range(-_size.z, _size.z),
            Quaternion.identity, 
            out _);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, _size);
    }
}
