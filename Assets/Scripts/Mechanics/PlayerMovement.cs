using System;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public enum JumpState
    {
        Rising,
        Falling
    }
    [Header("Shape settings")]
    [SerializeField] private float _radius = 1f;
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _skinThickness = 0.02f;
    [SerializeField] private LayerMask _collisions = ~0;

    [Header("Rotation")]
    [SerializeField] private float _sensetivity = 1f;
    [SerializeField] private Transform _camera;
    public float _rotationX;
    public float _rotationY;

    [Header("Movement")]
    [SerializeField] private float _maxWalkSpeed;
    [SerializeField] private float _maxSpeedReachTime;
    private Vector2 _walkDirection;
    private Vector3 _moveForce;

    [Header("Gravity")]
    [SerializeField] private float _gravityStrength;
    [SerializeField] private float _maxGravityStrength;
    [SerializeField, Range(0, 1f)] private float _groundedThreshold = 0.9f;
    private Vector3 _gravityForce;
    private int _stepsSinceGrounded;
    public event Action<ContactPoint> OnLanding;
    public event Action OnBecomingAirborn;

    [Header("Jumping")]
    [SerializeField] private float _maxJumpHeight;
    [SerializeField] private float _jumpTime;
    [SerializeField] private AnimationCurve _jumpCurve;
    public event Action OnJump;
    public event Action<float> OnJumpStateProgress;
    public event Action<JumpState> OnJumpStateSwitch;
    private Vector3 _jumpForce;
    private Vector3 _initialJumpVector;
    private float _jumpApex;
    private float _jumpIntegral;
    private float _jumpElapsed;
    private bool _isJumping;
    private bool _wantToJump;

    private Vector3 _groundDirection;
    private Vector3 _groundNormal;
    private bool _wasGrounded;
    private bool _isGrounded;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _groundNormal = _initialJumpVector = _groundDirection = Vector3.up;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.sleepThreshold = 0;
        _rigidbody.useGravity = false;

        _jumpApex = FindApex(_jumpCurve);
        _jumpIntegral = ComputeIntegral(_jumpCurve);
    }

    private void Update()
    {
        if(!_isJumping && Input.GetKeyDown(KeyCode.Space))
        {
            _wantToJump = true;
        }

        _walkDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            _walkDirection += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _walkDirection -= Vector2.up;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _walkDirection += Vector2.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _walkDirection -= Vector2.right;
        }

        float rotationX = Input.GetAxis("Mouse X") * _sensetivity;
        float rotationY = Input.GetAxis("Mouse Y") * _sensetivity;
        _rotationY += rotationY;
        _rotationX += rotationX;

        _rotationY = Mathf.Clamp(_rotationY, -90, 90);
        _camera.transform.localRotation = Quaternion.Euler(-_rotationY, 0, 0);
        transform.rotation = Quaternion.Euler(0, _rotationX, 0);
    }

    private void FixedUpdate()
    {
        if (_wantToJump)
        {
            _wantToJump = false;
            _isJumping = true;
            _jumpElapsed = 0f;
        }

        _rigidbody.linearVelocity = ComputeGravity() + ComputeJump() + ComputeWalking();

        if(_wasGrounded && _isGrounded)
        {
            OnBecomingAirborn?.Invoke();
        }
        _wasGrounded = _isGrounded;
        _isGrounded = false;
    }

    private Vector3 ComputeWalking()
    {
        Vector3 direction = Quaternion.FromToRotation(_groundDirection, _groundNormal) * (transform.forward * _walkDirection.y + transform.right * _walkDirection.x);
        if(RaycastUtils.Raycast(transform.position, direction, transform.IgnoreSelf(), out var hit, _radius + _skinThickness, _collisions))
        {
            direction = Vector3.zero;
        }

        Vector3 desiredWalkForce = Vector3.MoveTowards(_moveForce, direction * _maxWalkSpeed, _maxSpeedReachTime < Mathf.Epsilon ? float.MaxValue : _maxWalkSpeed / _maxSpeedReachTime);
        _moveForce = desiredWalkForce;
        return _moveForce;
    }

    private Vector3 ComputeJump()
    {
        if (!_isJumping) return Vector3.zero;
         
        _jumpElapsed += Time.deltaTime;
        float delta = _jumpElapsed / _jumpTime;

        if(delta < _jumpApex)
        {
            if(delta - Time.deltaTime < 0.005f)
            {
                OnJumpStateSwitch?.Invoke(JumpState.Rising);
            }
            OnJumpStateProgress?.Invoke(Mathf.InverseLerp(0, _jumpApex, delta));
        }
        else
        {
            if(delta - Time.deltaTime < _jumpApex)
            {
                OnJumpStateSwitch?.Invoke(JumpState.Falling);
            }
            OnJumpStateProgress?.Invoke(Mathf.InverseLerp(_jumpApex, 1, delta));
        }

        float curveMoment = _jumpCurve.Evaluate(delta);
        float curveMomentNext = _jumpCurve.Evaluate((_jumpElapsed + Time.fixedDeltaTime) / _jumpTime);

        if (delta > 1 || _isGrounded && delta > 0.2f || Mathf.Abs(curveMoment - curveMomentNext) < 0.01f && delta > 0.9f)
        {
            _gravityForce = Vector3.Dot(_jumpForce.normalized, _gravityForce.normalized) > 0 ? _jumpForce : Vector2.zero;
            _jumpForce = Vector3.zero;
            _isJumping = false;

            return Vector3.zero;
        }

        Vector3 direction = _initialJumpVector;
        float diff = curveMomentNext - curveMoment;

        direction *= (diff) / Time.fixedDeltaTime * _maxJumpHeight;
        _jumpForce = direction;
        return _jumpForce;
    }

    private Vector3 ComputeGravity()
    {
        if (!(_isGrounded || _isJumping))
        {
            _gravityForce -= _groundDirection * _gravityStrength;
            if(_gravityForce.sqrMagnitude > _maxGravityStrength * _maxGravityStrength)
            {
                _gravityForce = _gravityForce.normalized * _maxGravityStrength;
            }

            return _gravityForce;
        }
        else
        {
            _gravityForce = Vector3.zero;
            return Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isGrounded = false;
        ContactPoint? point = null;

        for (int i = 0; i < collision.contactCount && !isGrounded; i++)
        {
            isGrounded |= Vector3.Dot(_groundDirection, collision.GetContact(i).normal) > _groundedThreshold;
            if (!point.HasValue)
            {
                point = collision.GetContact(i);
            }
        }

        if (isGrounded && !(_isGrounded || _wasGrounded))
        {
            OnLanding?.Invoke(point.Value);
        }

        _isGrounded = isGrounded;
    }

    private void OnCollisionStay(Collision collision)
    {
        bool isGrounded = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            float dot = Vector3.Dot(_groundDirection, collision.GetContact(i).normal);
            isGrounded |= dot > _groundedThreshold;
            if(isGrounded)
                _groundNormal = collision.GetContact(i).normal;
            DebugUtils.DebugDrawArrow(collision.GetContact(i).point, collision.GetContact(i).point + collision.GetContact(i).normal, Color.blue, 0.1f);
        }

        _isGrounded = isGrounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _height * 0.5f, _radius);
        Gizmos.DrawWireSphere(transform.position - Vector3.up * _height * 0.5f, _radius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _height * 0.5f, _radius + _skinThickness);
        Gizmos.DrawWireSphere(transform.position - Vector3.up * _height * 0.5f, _radius + _skinThickness);
    }
    private float ComputeIntegral(AnimationCurve curve)
    {
        float delta = 1 / 100f;

        float res = 0;
        float prevVal = curve.Evaluate(0);
        for (int i = 1; i < 100; i++)
        {
            float val = curve.Evaluate(delta * i);

            res += delta * 0.5f * (prevVal + val);
            prevVal = val;
        }

        return res;
    }

    private float FindApex(AnimationCurve curve)
    {
        float delta = 1 / 100f;

        float prevVal = curve.Evaluate(0);
        for (int i = 1; i < 100; i++)
        {
            float val = curve.Evaluate(delta * i);
            float nextVal = curve.Evaluate(delta * (i + 1));

            if (val > prevVal && val > nextVal)
            {
                return val;
            }
            prevVal = val;
        }

        return -1f;
    }
}
