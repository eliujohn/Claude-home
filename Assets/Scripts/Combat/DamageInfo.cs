using UnityEngine;

namespace Ascension
{
    /// <summary>Mutable damage packet. Sponsors edit it in place on its way from attacker to victim.</summary>
    public class DamageInfo
    {
        public float amount;
        public GameObject source;
        public string sourceName = "Unknown";
        public bool fromPlayer;
        public bool isMelee, isRanged, isCrit, isBackstab, isHazard, isStatusTick;
        public bool ignoreArmor;
        public bool negated;
        public Vector2 direction;
        public StatusType statuses = StatusType.None;
        public float statusDuration = 4f;

        public DamageInfo Clone() => (DamageInfo)MemberwiseClone();
    }
}
