using UnityEngine;

namespace Ascension
{
    [CreateAssetMenu(fileName = "NewConstellation", menuName = "Ascension/Constellation")]
    public class ConstellationSponsor : ScriptableObject
    {
        public ConstellationId id;
        public SponsorCategory category;
        public string sponsorName;
        public string sponsorTitle;
        public Color constellationColor = Color.white;
        [TextArea(2, 5)] public string blessingDescription;
        public float damageMod = 1.0f;
        public float speedMod = 1.0f;
        public bool altersAttackBehavior = false;
        [Range(1, 9)] public int maxRank = 5;
    }
}
