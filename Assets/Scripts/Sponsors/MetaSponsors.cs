using UnityEngine;

namespace Ascension
{
    /// <summary>10. The Procrastinating God of Tomorrow — Slacker. (Replaces the stand-alone GimmickEngine.)</summary>
    public class ProcrastinatorBehaviour : SponsorBehaviour
    {
        float still;
        bool charged, armed;

        public override void Tick(float dt)
        {
            bool idle = P.Body.linearVelocity.sqrMagnitude < 0.01f && !P.Controller.IsDashing && !GameManager.InputLocked;
            if (!idle) { still = 0f; return; }
            still += dt;
            if (still >= 2f) charged = true;
        }

        public override void OnAttackBegin() { armed = charged; if (charged) { charged = false; still = 0f; } }
        public override void OnAttackEnd() => armed = false;
        public override void ModifyOutgoing(DamageInfo d) { if (armed) d.amount *= 1f + 3f * Scale; }   // +300%
        public override string HudStatus => charged ? "CHARGED +" + Mathf.RoundToInt(300 * Scale) + "%" : still > 0.25f ? still.ToString("0.0") + "/2.0" : null;
    }

    /// <summary>11. The Sponsor Who Throws Money At Problems — Whale.</summary>
    public class MoneyThrowerBehaviour : SponsorBehaviour
    {
        public override bool TryPayRangedCost()
        {
            if (P.Run.gold < 1) return false;
            P.Run.gold--;
            return true;
        }

        public override void ModifyOutgoing(DamageInfo d)
        {
            d.amount *= 1f + Mathf.Min(P.Run.gold, 600) / 150f * Scale;   // wallet = power
            if (d.isRanged) d.amount *= 2f;                              // coins hit hard
        }

        public override void ModifyProjectile(Projectile p)
        {
            p.SetColor(new Color(1f, 0.85f, 0.1f));
            if (Rank >= 3) p.Pierce = 1;
        }

        public override string HudStatus => P.Run.gold < 1 ? "OUT OF COINS" : "Wealth x" + (1f + Mathf.Min(P.Run.gold, 600) / 150f * Scale).ToString("0.0");
    }

    /// <summary>12. The Critic of Terrible Tropes — Meta-Commentator.</summary>
    public class TropeCriticBehaviour : SponsorBehaviour
    {
        public override void Tick(float dt)
        {
            var boss = Boss.Current;
            if (boss != null && boss.ShieldActive) boss.BreakShield();   // "uninspired writing"
            for (int i = Breakable.Active.Count - 1; i >= 0; i--)
            {
                var b = Breakable.Active[i];
                if (b != null && ((Vector2)b.transform.position - P.Position).sqrMagnitude < 49f) b.Break();
            }
        }

        public override void ModifyVsTarget(Enemy target, DamageInfo d) { if (target.IsBoss) d.amount *= 1f + 0.1f * Scale; }
    }

    /// <summary>13. The Chef Who Seasoned the Universe — Culinaromancer.</summary>
    public class CosmicChefBehaviour : SponsorBehaviour
    {
        public override void OnEnemyKilled(Enemy e)
        {
            if (GameManager.State != GameFlow.Run) return;
            if (Random.value > 0.45f + 0.08f * Rank) return;
            float r = Random.value;
            var type = r < 0.4f ? SpiceType.Salt : r < 0.8f ? SpiceType.Chili : SpiceType.MSG;
            Spice.Spawn(e.Position, type, Scale);
        }

        public override void Tick(float dt) { if (P.Run.chiliTimer > 0f) P.Run.chiliTimer -= dt; }
        public override void ModifyOutgoing(DamageInfo d) { if (P.Run.chiliTimer > 0f) d.statuses |= StatusType.Ignite; }
        public override string HudStatus => P.Run.chiliTimer > 0f ? "Chili " + P.Run.chiliTimer.ToString("0") + "s" : (P.Run.metaDropMult > 1f ? "MSG x" + P.Run.metaDropMult.ToString("0.00") : null);
    }

    /// <summary>14. The Over-Enthusiastic Chat Moderator — Streamer.</summary>
    public class ChatModBehaviour : SponsorBehaviour
    {
        float timer = 6f, speedBuff, dmgBuff;

        public override void Tick(float dt)
        {
            speedBuff -= dt; dmgBuff -= dt;
            if (GameManager.State != GameFlow.Run) return;
            timer -= dt;
            if (timer <= 0f) { timer = 11f / Scale; PostRandomPrompt(); }
            if (InputReader.AcceptPromptPressed) ChatPromptBoard.Accept(0);
        }

        void PostRandomPrompt()
        {
            switch (Random.Range(0, 4))
            {
                case 0: ChatPromptBoard.Post("SUB HYPE!", "+100% movement speed for 5s", () => speedBuff = 5f * Scale); break;
                case 1: ChatPromptBoard.Post("BIG RAID INCOMING", "+60% damage for 6s", () => dmgBuff = 6f * Scale); break;
                case 2: ChatPromptBoard.Post("CLIP IT!!", "Heal 15% max health", () => P.Heal(P.Health.max * 0.15f, false)); break;
                default: ChatPromptBoard.Post("DASH TRAIN CHOO CHOO", "Dash recharged instantly", () => P.Controller.ResetDash()); break;
            }
        }

        public override float MoveSpeedMultiplier => speedBuff > 0f ? 2f : 1f;
        public override void ModifyOutgoing(DamageInfo d) { if (dmgBuff > 0f) d.amount *= 1.6f; }
        public override void OnUnequip() => ChatPromptBoard.Clear();
        public override string HudStatus => speedBuff > 0f ? "HYPE " + speedBuff.ToString("0.0") : dmgBuff > 0f ? "RAID " + dmgBuff.ToString("0.0") : null;
    }

    /// <summary>15. The Entity Obsessed with Direct Punching — Meathead.</summary>
    public class MuscleBrainBehaviour : SponsorBehaviour
    {
        public override bool AllowsRanged => false;
        public override float MeleeCooldownMultiplier => 1.4f;
        public override void ModifyOutgoing(DamageInfo d) { if (d.isMelee) d.amount *= 1f + 10f * Scale; }   // +1000%
    }
}
