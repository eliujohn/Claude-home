namespace Ascension
{
    /// <summary>Base combat/movement numbers. Meta upgrades (Stellar Mirror Matrix) are folded in at run start.</summary>
    public class PlayerStats
    {
        public float maxHp = 100f;
        public float damageMult = 1f;
        public float moveSpeed = 6.5f;
        public float dashCooldown = 0.9f, dashSpeed = 24f, dashTime = 0.17f;
        public float meleeDamage = 26f, meleeRange = 2.3f, meleeArc = 120f, meleeCooldown = 0.36f;
        public float rangedDamage = 14f, rangedSpeed = 15f, rangedCooldown = 0.5f;

        public static PlayerStats FromMeta()
        {
            var s = new PlayerStats();
            s.maxHp += 10f * MetaProgression.Rank(MatrixId.IronWill);
            s.damageMult += 0.04f * MetaProgression.Rank(MatrixId.DefiantEdge);
            s.moveSpeed *= 1f + 0.025f * MetaProgression.Rank(MatrixId.SwiftSoul);
            s.dashCooldown *= 1f - 0.04f * MetaProgression.Rank(MatrixId.PhantomStep);
            return s;
        }
    }
}
