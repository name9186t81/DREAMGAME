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
    [Header("Movement")]
    [SerializeField] private float _maxWalkSpeed;
    [SerializeField] private float _accelerationSpeed;

    [Header("Gravity")]
    [SerializeField] private float _gravityStrength;
    [SerializeField] private float _maxGravityStrength;
    [SerializeField, Range(0, 1f)] private float _groundedThreshold = 0.9f;
    private Vector3 _gravityForce;
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
    private bool _wasGrounded;
    private bool _isGrounded;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _initialJumpVector = _groundDirection = Vector3.up;
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
    }

    private void FixedUpdate()
    {
        if (_wantToJump)
        {
            _wantToJump = false;
            _isJumping = true;
            _jumpElapsed = 0f;
        }

        _rigidbody.linearVelocity = ComputeGravity() + ComputeJump();

        if(_wasGrounded && _isGrounded)
        {
            OnBecomingAirborn?.Invoke();
        }
        _wasGrounded = _isGrounded;
        _isGrounded = false;
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

        for (int i = 0; i < collision.contactCount && !isGrounded; i++)
        {
            isGrounded |= Vector3.Dot(_groundDirection, collision.GetContact(i).normal) > _groundedThreshold;
        }

        _isGrounded = isGrounded;
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
