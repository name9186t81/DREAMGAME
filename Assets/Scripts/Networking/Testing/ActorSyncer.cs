using Networking.Packages;
using UnityEngine;

namespace Networking
{
    public sealed class ActorSyncer : MonoBehaviour
    {
        [SerializeField] private float _syncRate;
        private float _elapsed;

        private void Update()
        {
            _elapsed += Time.deltaTime;

            if(_elapsed > 1 / _syncRate)
            {
                SendSync();
            }
        }

        private void SendSync()
        {
            if (NetworkManager.Instance == null || NetworkManager.Instance.Client == null || NetworkManager.Instance.Client.Server == null) return;

            NetworkManager.Instance.Client.SendPackageToServerNextTick(new TestActorSyncPackage(transform.position, transform.eulerAngles, 0, 0));
        }
    }
}