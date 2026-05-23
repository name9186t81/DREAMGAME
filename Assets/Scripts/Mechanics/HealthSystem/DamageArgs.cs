namespace Mechanics.Health
{
    public struct DamageArgs
    {
        public enum DamageType
        {
            Kinetic
        }

        public float Damage;
        public DamageType Type;
    }
}
