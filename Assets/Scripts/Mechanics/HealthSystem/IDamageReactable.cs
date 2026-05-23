using System;

namespace Mechanics.Health
{
    public interface IDamageReactable
    {
        void React(DamageArgs args);
        event Action<DamageArgs> OnDamage;
    }
}
