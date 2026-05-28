using Networking;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Mechanics.Weapons
{
    public class Cannon : MonoBehaviour
    {
        public enum ElementType
        {
            None,
            Fire,
            Lightning,
            Ghost
        }

        [SerializeField] private float _cooldown;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private PlayerInputReader _reader;
        [SerializeField] private Player _player;
        [SerializeField] private CannonBall _prefab;
        [SerializeField] private UnityEvent _onShoot;
        private ElementType[] _combination = new ElementType[4];
        private int _index = 0;
        private bool _readingCombination;

        private void OnEnable()
        {
            _reader.OnInputPressed += ReadInputPress;
            _reader.OnInputReleased += ReadInputRelease;
            _player.OnEvent += ReadPlayerEvent;
        }

        private void OnDisable()
        {
            _reader.OnInputPressed -= ReadInputPress;
            _reader.OnInputReleased -= ReadInputRelease;
            _player.OnEvent -= ReadPlayerEvent;
        }

        private void ReadPlayerEvent(Networking.EntityEvent arg1, byte[] arg2)
        {
            if (arg1 == Networking.EntityEvent.Shoot)
            {
                var worldPos = NetworkUtils.GetVector3FromBuffer(arg2, 0);
                var direction = NetworkUtils.GetVector3FromBuffer(arg2, sizeof(float) * 3);
                SetCombination(arg2[sizeof(float) * 6]);
                Shoot(worldPos, direction);
            }
        }

        private void Shoot(Vector3 worldPosition, Vector3 direction)
        {
            var ball = Instantiate(_prefab, worldPosition, Quaternion.identity, null);
            ball.Init(GetCombination(), worldPosition, direction);
            _onShoot?.Invoke();
        }

        private void ReadInputRelease(PlayerInputReader.InputType obj)
        {
            if (!_readingCombination || obj != PlayerInputReader.InputType.Attack2) return;

            _readingCombination = false;
            Shoot(_shootPoint.position, _shootPoint.forward);

            byte[] data = new byte[sizeof(float) * 6 + sizeof(byte)];
            NetworkUtils.AddVector3ToBuffer(_shootPoint.position, data, 0);
            NetworkUtils.AddVector3ToBuffer(_shootPoint.forward, data, sizeof(float) * 3);
            data[sizeof(float) * 6] = (byte)GetCombination();
            _player.SendNewEvent(EntityEvent.Shoot, data);

            ResetCombination();
        }

        private void ReadInputPress(PlayerInputReader.InputType obj)
        {
            _readingCombination |= obj == PlayerInputReader.InputType.Attack2;

            if (!_readingCombination || !obj.IsSelect() || _index == 4 || obj > PlayerInputReader.InputType.Select3) return;

            _combination[_index++] = (ElementType)((byte)obj - PlayerInputReader.InputType.Select1 + 1);
        }

        private void ResetCombination()
        {
            for (int i = 0; i < _combination.Length; i++)
            {
                _combination[i] = ElementType.None;
            }
            _index = 0;
        }

        private void SetCombination(byte raw)
        {
            byte mask = byte.MaxValue >> 6;
            for(int i = 0; i < 4; i++)
            {
                _combination[i] = (ElementType)(raw & mask);
                raw >>= 2;
            }
        }

        private int GetCombination()
        {
            int num = 0;
            for(int i = 0; i < _combination.Length; i++)
            {
                num += (int)_combination[i] << (i * 2);
            }
            return num;
        }

        private bool HasCombination
        {
            get
            {
                bool res = false;
                for (int i = 0; i < _combination.Length && !res; i++)
                {
                    res |= _combination[i] > 0;
                }
                return res;
            }
        }
    }
}