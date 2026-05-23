using System;

namespace Mechanics.Health
{
    public class BaseHealth
    {
        private float _maxHealth;
        private float _currentHealth;

        private const int ROUND_ACCURACY = 2;

        public event Action OnDeath;
        public event Action OnDamage;

        public BaseHealth(float maxHealth)
        {
            _maxHealth = _currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = (float)Math.Clamp(Math.Round(_currentHealth - damage, ROUND_ACCURACY), 0, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
            else
            {
                OnDamage?.Invoke();
            }
        }
    }
}