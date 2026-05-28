using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mechanics.Weapons
{
    public class CannonBall : MonoBehaviour
    {
        public interface ISpecialCombinationHandler
        {
            void Init(Vector3 worldPos, Vector3 initialDirection);
            void Update(Vector3 worldPos, Vector3 prevWorldPos, RaycastHit2D? hit);
            void Hit(Vector3 worldPos, Vector3 initialDirection);
        }

        [SerializeField] private float _explosionRadius;
        [SerializeField] private float _defaultDamage;
        [SerializeField] private float _projectileSpeed;
        [SerializeField] private AnimationCurve _damageFalloff;
        [SerializeField] private LayerMask _hitMask;

        [Space]
        [SerializeField] private ParticleSystem[] _lightningSystems;
        [SerializeField] private Material _mat;
        [SerializeField] private MeshRenderer _renderer;

        private Dictionary<int, ISpecialCombinationHandler> _specialCombinations = new Dictionary<int, ISpecialCombinationHandler>();
        private ISpecialCombinationHandler _handler;

        private int _combination;
        private Vector3 _initialWorldPos;
        private Vector3 _initialDirection;

        private static Dictionary<int, Material> _materialPerCombination = new Dictionary<int, Material>();
        private static List<Cannon.ElementType> __staticElementCounts = new List<Cannon.ElementType>();
        private Dictionary<Cannon.ElementType, int> _elementsCounts = new Dictionary<Cannon.ElementType, int>();

        private float _explosionRadiusModifier;
        private float _damageModifier;
        private int _maxHits;

        private Vector3 _prevPosition;

        static CannonBall()
        {
            var ar = Enum.GetValues(typeof(Cannon.ElementType));
            for(int i = 0; i < ar.Length; i++)
            {
                __staticElementCounts.Add((Cannon.ElementType)ar.GetValue(i));
            }
        }

        public void Init(int combination, Vector3 worldPos, Vector3 initialDirection)
        {
            _combination = combination;
            transform.position = _initialWorldPos = worldPos;
            transform.forward = _initialDirection = initialDirection;

            if(_specialCombinations.TryGetValue(combination, out var handler))
            {
                _handler = handler;
                _handler.Init(worldPos, initialDirection);
            }
            else
            {
                ExtractCombination(combination);
                FormStats();
                SetMaterial();
            }
        }

        private void Update()
        {
            _prevPosition = transform.position;
        }

        private void LateUpdate()
        {
            transform.position += _initialDirection * (_projectileSpeed * Time.deltaTime);

            var cast = Physics.Raycast(_prevPosition, transform.position, _projectileSpeed * Time.deltaTime, _hitMask);
            if (cast)
            {
                Destroy(gameObject);
            }
        }

        private void FormStats()
        {
            _explosionRadiusModifier = 1 + _elementsCounts[Cannon.ElementType.Fire] * 1.5f;
            _damageModifier = 1 + _elementsCounts[Cannon.ElementType.Lightning] * 2f;
            _maxHits = 1 + _elementsCounts[Cannon.ElementType.Ghost];
        }

        private void ExtractCombination(int combination)
        {
            _elementsCounts = new Dictionary<Cannon.ElementType, int>();
            Dictionary<int, int> rawCombinations = new Dictionary<int, int>();

            for(int i = 0; i < 4; i++)
            {
                int num = combination & 3;
                Debug.LogError(num);
                if(rawCombinations.TryGetValue(num, out _))
                {
                    rawCombinations[num]++;
                }
                else
                {
                    rawCombinations.Add(num, 1);
                }
                combination >>= 2;
            }

            foreach(var el in rawCombinations)
            {
                _elementsCounts.Add((Cannon.ElementType)el.Key, el.Value);
                Debug.Log($"key - {(Cannon.ElementType)el.Key} val - {el.Value}");  
            }

            foreach(var el in __staticElementCounts)
            {
                if (!_elementsCounts.ContainsKey(el))
                {
                    _elementsCounts.Add(el, 0);
                }
            }
        }

        private void SetMaterial()
        {
            int combination = _elementsCounts[Cannon.ElementType.Fire] + _elementsCounts[Cannon.ElementType.Ghost] << 2;
            if(_materialPerCombination.TryGetValue(combination, out var material))
            {
                _renderer.material = material;
            }
            else
            {
                var copy = new Material(_mat);
                float fireDelta = Mathf.Clamp01(_elementsCounts[Cannon.ElementType.Fire] / 3f);
                float ghostDelta = Mathf.Clamp01(_elementsCounts[Cannon.ElementType.Ghost] / 3f);
                copy.SetColor("_MainColor", Color.HSVToRGB(0, 1 - ghostDelta, fireDelta * 0.8f));
                copy.SetFloat("_Power1", Mathf.Lerp(55f, 3f, Mathf.Sqrt(1 - (1 - fireDelta) * (1 - fireDelta))));
                copy.SetFloat("_Power3", Mathf.Lerp(55f, 9f, Mathf.Sqrt(1 - (1 - ghostDelta) * (1 - ghostDelta))));
                _renderer.material = copy;
                _materialPerCombination.Add(combination, copy);
            }

            if(_elementsCounts[Cannon.ElementType.Lightning] == 0)
            {
                for (int i = 0; i < _lightningSystems.Length; i++)
                {
                    _lightningSystems[i].gameObject.SetActive(false);
                }
                return;
            }

            float lightningDelta = _elementsCounts[Cannon.ElementType.Lightning] / 3f;
            for(int i = 0; i < _lightningSystems.Length; i++)
            {
                var emission = _lightningSystems[i].emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(15 * lightningDelta, 30 * lightningDelta); 
            }
        }
    }
}
