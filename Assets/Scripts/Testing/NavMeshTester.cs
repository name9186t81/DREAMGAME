using UnityEngine;
using UnityEngine.AI;

namespace Testing
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshTester : MonoBehaviour
    {
        [SerializeField] private Transform _endPoint;
        private NavMeshAgent _agent;

        private void Update()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.destination = _endPoint.position;
            _agent.isStopped = false;
        }
    }
}