using Networking.Packages;
using UnityEngine;

namespace Networking.Testing
{
	public class ActorSyncerDisplayer: MonoBehaviour
	{
		private TestActorSyncPackage _package;

        private void Update()
        {
            if(_package != null)
			{
				var state = Random.state;
				Random.InitState(_package.ClientID);
				Color c = Color.HSVToRGB(Random.value, 1, 1);
				Random.state = state;

				Debug.DrawLine(transform.position, _package.Position, c, 1f);

				transform.position = _package.Position;
				transform.eulerAngles = _package.Rotation;

				DebugUtils.DebugDrawArrow(transform.position, transform.position + transform.forward, c, 1f);
			}
        }

        public void AddPackage(TestActorSyncPackage package)
		{
			_package = package;
		}
	}
}