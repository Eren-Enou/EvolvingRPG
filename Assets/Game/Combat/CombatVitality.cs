using System;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public class CombatVitality : HitReceiver
    {
        [SerializeField, Min(1)] private float maximumHealth = 150f;
        [SerializeField, Min(1)] private float poiseThreshold = 40f;
        [SerializeField, Min(0.1f)] private float staggerDuration = 0.65f;
        [SerializeField, Min(0)] private float poiseRecoveryDelay = 2f;
        [SerializeField, Min(0)] private float poiseRecoveryRate = 8f;
        public float Health { get; private set; }
        public float Poise { get; private set; }
        public float MaximumHealth => maximumHealth;
        public float PoiseThreshold => poiseThreshold;
        public override bool Defeated => Health <= 0f;
        public bool Staggered => Time.time < staggerUntil;
        public string LastHit { get; private set; } = "Ready";
        public event Action<HitData, HitResolution> HitReceived;
        public event Action ResetPerformed;
        private float staggerUntil, lastHitAt;
        protected virtual void Awake() => ResetVitality();

        public void Configure(float health, float poise, float stagger)
        { maximumHealth = health; poiseThreshold = poise; staggerDuration = stagger; ResetVitality(); }

        public override bool ReceiveHit(HitData hit, out HitResolution resolution) => ApplyHit(hit, 1f, false, false, out resolution);

        protected bool ApplyHit(HitData hit, float damageMultiplier, bool guarded, bool vulnerable, out HitResolution resolution,
            EnemyImpactContext? context = null)
        {
            resolution = default;
            if (Defeated) return false;
            float damage = Mathf.Clamp(hit.Damage * damageMultiplier, 0, Health);
            float poise = Staggered ? 0f : Mathf.Clamp(hit.PoiseDamage, 0, poiseThreshold - Poise);
            Health -= damage; Poise += poise; lastHitAt = Time.time;
            bool broken = !Staggered && Poise >= poiseThreshold;
            if (broken) { Poise = 0; staggerUntil = Time.time + staggerDuration; }
            resolution = new HitResolution(damage, poise, broken, Defeated, guarded, vulnerable, context);
            LastHit = $"{hit.AttackName}: -{damage:0.#} HP / +{poise:0.#} poise"
                + (guarded ? " GUARDED" : vulnerable ? " OPENING" : "") + (broken ? " STAGGER" : "");
            HitReceived?.Invoke(hit, resolution);
            return true;
        }

        public void ResetVitality()
        {
            Health = maximumHealth; Poise = 0; staggerUntil = 0;
            lastHitAt = float.NegativeInfinity; LastHit = "Ready";
            ResetPerformed?.Invoke();
        }
        protected virtual void Update()
        {
            if (!Defeated && !Staggered && Time.time - lastHitAt >= poiseRecoveryDelay)
                Poise = Mathf.MoveTowards(Poise, 0, poiseRecoveryRate * Time.deltaTime);
        }
    }
}
