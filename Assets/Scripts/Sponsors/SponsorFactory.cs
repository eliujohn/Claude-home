using System;

namespace Ascension
{
    public static class SponsorFactory
    {
        public static SponsorBehaviour Create(ConstellationId id)
        {
            switch (id)
            {
                case ConstellationId.BoneThrone: return new BoneThroneBehaviour();
                case ConstellationId.FatalParadox: return new FatalParadoxBehaviour();
                case ConstellationId.BloodlineScribe: return new BloodlineScribeBehaviour();
                case ConstellationId.VoidExecutioner: return new VoidExecutionerBehaviour();
                case ConstellationId.EndlessHorizon: return new EndlessHorizonBehaviour();
                case ConstellationId.PrimordialFlame: return new PrimordialFlameBehaviour();
                case ConstellationId.UnyieldingAegis: return new UnyieldingAegisBehaviour();
                case ConstellationId.AbyssalHorror: return new AbyssalHorrorBehaviour();
                case ConstellationId.ClockworkFate: return new ClockworkFateBehaviour();
                case ConstellationId.Procrastinator: return new ProcrastinatorBehaviour();
                case ConstellationId.MoneyThrower: return new MoneyThrowerBehaviour();
                case ConstellationId.TropeCritic: return new TropeCriticBehaviour();
                case ConstellationId.CosmicChef: return new CosmicChefBehaviour();
                case ConstellationId.ChatMod: return new ChatModBehaviour();
                case ConstellationId.MuscleBrain: return new MuscleBrainBehaviour();
                default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }
    }
}
