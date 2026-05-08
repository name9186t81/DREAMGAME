using UnityEngine;

namespace Mechanics
{
    public interface IPortalInteractable
    {
        void OnPortalEnter(Vector3 enterPosition, Vector3 enterNormal, Quaternion enterRotation, Vector3 exitPosition, Vector3 exitNormal, Quaternion exitRotation);
        void OnPortalExit(Vector3 enterPosition, Vector3 enterNormal, Quaternion enterRotation, Vector3 exitPosition, Vector3 exitNormal, Quaternion exitRotation);
    }
}
