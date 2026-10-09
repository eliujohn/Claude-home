using UnityEngine;

namespace Ascension
{
    [System.Serializable]
    public class EnemyDef
    {
        public string name = "Enemy";
        public EnemyBehaviour behaviour = EnemyBehaviour.Chaser;
        public float hp = 40, speed = 3f, damage = 8f, attackRange = 1.2f, attackCooldown = 1.2f;
        public float armor = 0f;            // 0..0.8 fraction of damage ignored (until Corroded)
        public float radius = 0.45f;
        public float projectileSpeed = 8f;
        public Color color = Color.red;
        public int gold = 3;
        public int stardustChance = 8;       // percent chance to drop 1 Stardust
        public int tier = 0;                 // gates spawn weight by room depth

        public EnemyDef Scaled(float hpMul, float dmgMul, float sizeMul = 1f)
        {
            var c = (EnemyDef)MemberwiseClone();
            c.hp *= hpMul; c.damage *= dmgMul; c.radius *= sizeMul;
            return c;
        }
    }

    [System.Serializable]
    public class BossPhase
    {
        public float enterAtHpFraction = 1f;
        public BossPattern[] patterns;
        public float interval = 2.4f;
        public int shieldAdds = 0;          // >0: boss is immune behind a shield until the adds die
    }

    public enum BossPattern { AimedVolley, RadialBurst, Spiral, Charge, SummonAdds }

    [System.Serializable]
    public class BossDef : EnemyDef
    {
        public BossPhase[] phases;
        public EnemyDef[] addPool;
        public string title = "";
    }
}
