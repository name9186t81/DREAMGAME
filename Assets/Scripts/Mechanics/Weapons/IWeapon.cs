using System;

namespace Mechanics.Weapons
{
    public enum WeaponState
    {
        Idle,
        Charging,
        Attacking,
        Cooldown
    }

    public enum AttackKey : byte
    {
        Fire1,
        Fire2,
        Fire3,
        Fire4,
        Fire5,
        Fire6,
        Fire7,
        Fire8,
        Fire9,
        Fire10
    }

    public interface IWeapon
    {
        WeaponState State { get; }
        event Action OnStateChanged;
        bool TryAttack(AttackKey key);
    }
}