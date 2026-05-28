using UnityEngine;

public class DebugPlayerEffects : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Transform _target;

    private void Update()
    {
        if (_movement.IsSliding)
        {
            float delta = _movement.SlideElapsed / _movement.SlideTime;
            if(delta < 0.3f)
            {
                delta = delta / 0.3f;
            }
            else if(delta < 0.6f)
            {
                delta = 1f;
            }
            else
            {
                delta = 1 - (delta - 0.6f) / 0.4f;
            }

            _target.localRotation = Quaternion.Euler(Mathf.Lerp(0, -90, delta), _target.localEulerAngles.y, _target.localEulerAngles.z);
        }
    }
}
