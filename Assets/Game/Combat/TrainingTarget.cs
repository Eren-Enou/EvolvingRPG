using System;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public sealed class TrainingTarget : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumHealth = 250f;
        [SerializeField, Min(1f)] private float poiseThreshold = 60f;
        [SerializeField, Min(0f)] private float poiseRecoveryDelay = 1.5f;
        [SerializeField, Min(0f)] private float poiseRecoveryPerSecond = 10f;
        [SerializeField, Min(0.1f)] private float staggerDuration = 0.8f;
        [SerializeField, Min(0f)] private float automaticResetDelay = 3f;

        public float Health { get; private set; }
        public float Poise { get; private set; }
        public float MaximumHealth => maximumHealth;
        public float PoiseThreshold => poiseThreshold;
        public bool Defeated => Health <= 0f;
        public bool Staggered => Time.time < staggerUntil;
        public string LastHit { get; private set; } = "Ready";
        public event Action<HitData, bool> HitReceived;
        public event Action ResetPerformed;
        private float lastHitAt;
        private float staggerUntil;
        private float defeatedAt;

        private void Awake() => ResetTarget();

        public bool ReceiveHit(HitData hit)
        {
            if (Defeated) return false;
            Health = Mathf.Max(0f, Health - hit.Damage);
            lastHitAt = Time.time;
            bool brokePoise = false;
            if (!Staggered)
            {
                Poise += hit.PoiseDamage;
                if (Poise >= poiseThreshold)
                {
                    Poise = 0f;
                    staggerUntil = Time.time + staggerDuration;
                    brokePoise = true;
                }
            }
            if (Defeated) defeatedAt = Time.time;
            LastHit = $"{hit.AttackName}: -{hit.Damage:0} HP / +{hit.PoiseDamage:0} poise"
                + (Defeated ? " | DEFEATED" : brokePoise ? " | STAGGERED" : "");
            HitReceived?.Invoke(hit, brokePoise);
            return true;
        }

        public void ResetTarget()
        {
            Health = maximumHealth;
            Poise = 0f;
            staggerUntil = 0f;
            defeatedAt = 0f;
            lastHitAt = float.NegativeInfinity;
            LastHit = "Ready";
            ResetPerformed?.Invoke();
        }

        private void Update()
        {
            if (Defeated)
            {
                if (automaticResetDelay > 0f && Time.time - defeatedAt >= automaticResetDelay)
                    ResetTarget();
            }
            else if (!Staggered && Time.time - lastHitAt >= poiseRecoveryDelay)
                Poise = Mathf.MoveTowards(Poise, 0f, poiseRecoveryPerSecond * Time.deltaTime);
        }
    }
}
