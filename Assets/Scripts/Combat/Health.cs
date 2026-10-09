using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ascension
{
    public class Health : MonoBehaviour
    {
        public float max = 100f, current = 100f;
        public float armor;                 // fraction ignored, 0..0.9
        public bool armorDissolved;         // set by Corroded
        public bool invulnerable;
        public bool IsDead { get; private set; }
        public float Fraction => max > 0 ? Mathf.Clamp01(current / max) : 0f;

        /// <summary>Raised first so modifiers can edit or negate the packet.</summary>
        public event Action<DamageInfo> BeforeDamage;
        public event Action<DamageInfo, float> Damaged;
        public event Action<DamageInfo> Died;

        /// <summary>Each hook may return true to prevent death (it must restore health itself).</summary>
        readonly List<Func<DamageInfo, bool>> fatalHooks = new List<Func<DamageInfo, bool>>();
        public void AddFatalHook(Func<DamageInfo, bool> hook) { if (!fatalHooks.Contains(hook)) fatalHooks.Add(hook); }
        public void RemoveFatalHook(Func<DamageInfo, bool> hook) => fatalHooks.Remove(hook);

        public void Init(float maxHp)
        {
            max = current = maxHp;
            IsDead = false;
            invulnerable = false;
        }

        public void Revive(float fraction)
        {
            IsDead = false;
            current = Mathf.Max(1f, max * fraction);
        }

        public float TakeDamage(DamageInfo d)
        {
            if (IsDead || invulnerable || d == null) return 0f;
            BeforeDamage?.Invoke(d);
            if (d.negated || d.amount <= 0f) return 0f;

            float amt = d.amount;
            if (!d.ignoreArmor && !armorDissolved) amt *= 1f - Mathf.Clamp(armor, 0f, 0.9f);
            current -= amt;
            Damaged?.Invoke(d, amt);

            if (current <= 0f)
            {
                for (int i = 0; i < fatalHooks.Count; i++)
                    if (fatalHooks[i](d)) return amt;
                current = 0f;
                IsDead = true;
                Died?.Invoke(d);
            }
            return amt;
        }

        public float Heal(float amount)
        {
            if (IsDead || amount <= 0f) return 0f;
            float before = current;
            current = Mathf.Min(max, current + amount);
            return current - before;
        }

        public void SetMax(float newMax, bool scaleCurrent)
        {
            float delta = newMax - max;
            max = newMax;
            current = scaleCurrent ? Mathf.Clamp(current + Mathf.Max(0, delta), 1f, max) : Mathf.Min(current, max);
        }
    }
}
