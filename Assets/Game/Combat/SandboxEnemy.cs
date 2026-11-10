using System;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public enum EnemyPhase { Idle, Approach, Windup, Active, Recovery, Staggered, Defeated, Returning }

    [RequireComponent(typeof(CharacterController))]
    public sealed class SandboxEnemy : MonoBehaviour
    {
        [SerializeField] private CombatVitality player;
        [SerializeField] private bool guarded = true;
        [SerializeField, Min(0.1f)] private float speed = 2f;
        [SerializeField, Min(1)] private float turnSpeed = 120f;
        [SerializeField, Min(1)] private float awarenessRange = 8f;
        [SerializeField, Min(1)] private float leashDistance = 12f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.9f;
        [SerializeField, Range(1, 180)] private float attackArc = 80f;
        [SerializeField, Min(0.05f)] private float windup = 0.85f, activeTime = 0.16f, recovery = 1.15f;
        [SerializeField, Min(0)] private float damage = 18f, poiseDamage = 20f;
        [SerializeField, Min(1)] private int repeatedHitThreshold = 3;
        [SerializeField, Min(0.1f)] private float patternMemory = 6f;
        [SerializeField] private bool logTransitions;
        public EnemyPhase Phase { get; private set; }
        public bool CanGuard => guarded && vitality != null && !vitality.Defeated && !vitality.Staggered
            && (Phase == EnemyPhase.Idle || Phase == EnemyPhase.Approach || Phase == EnemyPhase.Windup);
        public bool Bracing => lightPressure >= repeatedHitThreshold && Time.time - lastPatternAt < patternMemory;
        public bool AlertToFlank => flankRepeats >= 2 && Time.time - lastPatternAt < patternMemory;
        public string Pattern => AlertToFlank ? "Tracking repeated flank" : Bracing ? guarded ? "Bracing repeated light pressure" : "Retreating from light pressure" : "Observing";
        public float PhaseProgress { get; private set; }
        public int AttacksStarted { get; private set; }
        public int HitsLanded { get; private set; }
        public int EncounterRevision { get; private set; }
        public float LastAttackStartedAt { get; private set; } = float.NegativeInfinity;
        public bool EngagedWith(HitReceiver source) => isActiveAndEnabled && source != null && source == player && !player.Defeated;
        public string LastAttack { get; private set; } = "No strike yet";
        public float AttackRange => attackRange;
        public float AttackArc => attackArc;
        public bool GuardedArchetype => guarded;
        public EnemyThreat? CurrentThreat { get; private set; }
        public EnemyThreat? LastCommittedThreat { get; private set; }
        public event Action<EnemyThreat> ThreatCommitted;
        public event Action<ThreatResolution> ThreatFinished;
        private CombatVitality observedPlayer;
        public bool HasUnchangedThreatParameters(EnemyThreat threat) => threat.Target == player
            && Mathf.Approximately(threat.Geometry.Range, attackRange) && Mathf.Approximately(threat.Geometry.Arc, attackArc)
            && Mathf.Abs(threat.ActiveAt - threat.CommittedAt - windup) < 0.002f
            && Mathf.Abs(threat.ActiveUntil - threat.ActiveAt - activeTime) < 0.002f;
        private EnemyVitality vitality;
        private CharacterController motor;
        private Vector3 home;
        private Quaternion homeFacing, committedFacing;
        private float elapsed, verticalSpeed, lastPatternAt;
        private bool hitThisSwing;
        private int lightPressure, flankRepeats, lastFlank;

        public void Configure(CombatVitality target, bool fast)
        {
            player = target; guarded = !fast;
            if (fast) { speed = 3.6f; turnSpeed = 180; windup = 0.45f; activeTime = 0.12f; recovery = 0.7f; damage = 12; poiseDamage = 12; }
            ObserveTarget();
        }
        private bool initialized;
        private void Awake() => Initialize();
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            motor = GetComponent<CharacterController>(); vitality = GetComponent<EnemyVitality>();
            home = transform.position; homeFacing = transform.rotation;
            committedFacing = homeFacing;
            if (vitality != null) vitality.HitReceived += OnHit;
        }
        private void OnEnable() => ObserveTarget();
        private void OnDisable()
        {
            FinishThreat(ThreatEnd.Disabled);
            if (observedPlayer != null) observedPlayer.ResetPerformed -= TargetReset;
            observedPlayer = null;
        }
        private void OnDestroy() { if (vitality != null) vitality.HitReceived -= OnHit; }
        private void ObserveTarget()
        {
            if (!Application.isPlaying) return;
            if (!ReferenceEquals(observedPlayer, player)) FinishThreat(ThreatEnd.TargetChanged);
            if (observedPlayer != null) observedPlayer.ResetPerformed -= TargetReset;
            observedPlayer = player;
            if (isActiveAndEnabled && observedPlayer != null) observedPlayer.ResetPerformed += TargetReset;
        }
        private void TargetReset() => FinishThreat(ThreatEnd.TargetChanged);
        private void FinishThreat(ThreatEnd end)
        {
            if (!CurrentThreat.HasValue) return;
            var threat = CurrentThreat.Value;
            CurrentThreat = null; // Clear before callbacks: disable/reset is reentrant.
            ThreatFinished?.Invoke(new ThreatResolution(threat, end, Time.time, hitThisSwing));
        }

        private void Update() => Simulate(Time.deltaTime);

        private void Simulate(float delta)
        {
            if (vitality == null || player == null || !motor.enabled)
            { FinishThreat(ThreatEnd.Disabled); return; }
            if (player.Defeated) FinishThreat(ThreatEnd.TargetChanged);
            Vector3 travel = Vector3.zero;
            if (Time.time - lastPatternAt >= patternMemory) { lightPressure = flankRepeats = lastFlank = 0; }
            if (vitality.Defeated) Change(EnemyPhase.Defeated);
            else if (vitality.Staggered) Change(EnemyPhase.Staggered);
            else if (Phase == EnemyPhase.Staggered) Change(EnemyPhase.Recovery);
            else if (Phase == EnemyPhase.Windup || Phase == EnemyPhase.Active || Phase == EnemyPhase.Recovery)
            {
                float previous = elapsed; elapsed += delta;
                transform.rotation = committedFacing;
                if (Phase == EnemyPhase.Recovery)
                {
                    if (!guarded && Bracing && elapsed < 0.4f) travel = -transform.forward * speed;
                    PhaseProgress = Mathf.Clamp01(elapsed / recovery);
                    if (elapsed >= recovery) Change(EnemyPhase.Approach);
                }
                else
                {
                    // Test every frame overlapping the active window, even when a frame skips its boundary.
                    if (elapsed >= windup && previous < windup + activeTime && !hitThisSwing) ResolveStrike();
                    if (elapsed >= windup + activeTime) { LastAttack = hitThisSwing ? "Strike hit" : "Strike missed"; FinishThreat(ThreatEnd.Completed); Change(EnemyPhase.Recovery); }
                    else
                    {
                        Phase = elapsed < windup ? EnemyPhase.Windup : EnemyPhase.Active;
                        PhaseProgress = Phase == EnemyPhase.Windup ? elapsed / windup : (elapsed - windup) / activeTime;
                    }
                }
            }
            else
            {
                Vector3 toPlayer = Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up);
                bool returning = player.Defeated || Vector3.Distance(player.transform.position, home) > leashDistance;
                if (returning || Phase == EnemyPhase.Returning)
                {
                    Change(EnemyPhase.Returning);
                    Vector3 offset = Vector3.ProjectOnPlane(home - transform.position, Vector3.up);
                    if (offset.magnitude < 0.15f) { transform.rotation = homeFacing; Change(EnemyPhase.Idle); }
                    else { Turn(offset, delta); travel = offset.normalized * Mathf.Min(speed, offset.magnitude / Mathf.Max(delta, 0.001f)); }
                }
                else if (toPlayer.magnitude > awarenessRange && Phase == EnemyPhase.Idle) { }
                else
                {
                    Change(EnemyPhase.Approach); Turn(toPlayer, delta);
                    if (toPlayer.magnitude > attackRange + 0.1f) travel = toPlayer.normalized * speed;
                    else if (Vector3.Angle(transform.forward, toPlayer) < 15f && HasLineOfSight())
                    {
                        committedFacing = transform.rotation; hitThisSwing = false; AttacksStarted++; LastAttackStartedAt = Time.time;
                        LastAttack = "Committed strike"; Change(EnemyPhase.Windup);
                        CurrentThreat = new EnemyThreat(this, player, EncounterRevision, AttacksStarted, Time.time, windup, activeTime,
                            new StrikeGeometry(transform.position + Vector3.up, committedFacing * Vector3.forward, attackRange, attackArc));
                        LastCommittedThreat = CurrentThreat;
                        ThreatCommitted?.Invoke(CurrentThreat.Value);
                    }
                }
            }
            verticalSpeed = motor.isGrounded ? -2f : Mathf.Max(-20f, verticalSpeed - 20f * delta);
            motor.Move((travel + Vector3.up * verticalSpeed) * delta);
        }

        private void Turn(Vector3 offset, float delta)
        {
            if (offset.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(offset), turnSpeed * (AlertToFlank ? 1.5f : 1f) * delta);
        }
        private bool HasLineOfSight()
        {
            Vector3 origin = transform.position + Vector3.up;
            return StrikeContactQuery.Visible(origin, player.transform.position + Vector3.up, player, out _);
        }
        private void ResolveStrike()
        {
            var collider = player.GetComponent<CharacterController>();
            if (player.Defeated || collider == null) return;
            var geometry = CurrentThreat.HasValue ? CurrentThreat.Value.Geometry.At(transform.position + Vector3.up)
                : new StrikeGeometry(transform.position + Vector3.up, committedFacing * Vector3.forward, attackRange, attackArc);
            if (!StrikeContactQuery.Contacts(geometry, collider, player)) return;
            var identity = CurrentThreat ?? LastCommittedThreat;
            if (identity.HasValue && identity.Value.Target != player) identity = null;
            if (player.ReceiveHit(new HitData(guarded ? "Guarded strike" : "Quick strike", damage, poiseDamage, Color.red,
                transform.position, vitality, identity), out _))
            { hitThisSwing = true; HitsLanded++; }
        }
        private void OnHit(HitData hit, HitResolution result)
        {
            if (Time.time - lastPatternAt >= patternMemory) lightPressure = flankRepeats = lastFlank = 0;
            lightPressure = hit.PoiseDamage <= 15f ? lightPressure + 1 : 0;
            if (hit.SourcePosition.HasValue)
            {
                Vector3 direction = Vector3.ProjectOnPlane(hit.SourcePosition.Value - transform.position, Vector3.up);
                float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                int side = Mathf.Abs(angle) > 70f ? (angle > 0 ? 1 : -1) : 0;
                flankRepeats = side != 0 && side == lastFlank ? flankRepeats + 1 : side != 0 ? 1 : 0;
                lastFlank = side;
            }
            lastPatternAt = Time.time;
            if (result.Defeated) Change(EnemyPhase.Defeated);
            else if (result.PoiseBroken) { committedFacing = transform.rotation; Change(EnemyPhase.Staggered); }
        }
        private void Change(EnemyPhase next)
        {
            if (Phase == next) return;
            if (next == EnemyPhase.Staggered || next == EnemyPhase.Defeated || next == EnemyPhase.Returning || next == EnemyPhase.Idle)
                FinishThreat(ThreatEnd.Interrupted);
            Phase = next; elapsed = 0; PhaseProgress = 0;
            if (logTransitions) Debug.Log($"{name}: {next} | {Pattern}", this);
        }
        // A contract can change the patrol anchor only at deployment, never during a live exchange.
        public void ResetEnemyAt(Vector3 anchor)
        {
            Initialize();
            home = anchor;
            ResetEnemy();
        }
        public void ResetEnemy()
        {
            Initialize();
            FinishThreat(ThreatEnd.Reset);
            LastCommittedThreat = null;
            motor.enabled = false; transform.SetPositionAndRotation(home, homeFacing); motor.enabled = true;
            vitality.ResetVitality(); Phase = EnemyPhase.Idle; elapsed = verticalSpeed = 0;
            lightPressure = flankRepeats = lastFlank = AttacksStarted = HitsLanded = 0;
            EncounterRevision++; LastAttackStartedAt = float.NegativeInfinity;
            hitThisSwing = false; PhaseProgress = 0; LastAttack = "Reset";
        }
    }
}
