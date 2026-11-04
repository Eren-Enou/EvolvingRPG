using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(CombatActions), typeof(CombatVitality))]
    public sealed class HeavyPractice : MonoBehaviour, IAttackResolver
    {
        [SerializeField] private AttackDefinition[] heavyDefinitions;
        [SerializeField, Min(1)] private int trialThreshold = 5;
        [SerializeField, Range(0, 0.15f)] private float recoveryReduction = 0.08f;
        [SerializeField, Min(0.1f)] private float recentThreatWindow = 8f;
        private readonly InputAction toggleTrial = new InputAction("PracticeTrial", InputActionType.Button, "<Keyboard>/f2");
        public int Points { get; private set; }
        public int QualifiedActions { get; private set; }
        public int GuardBreaks { get; private set; }
        public int WindupInterrupts { get; private set; }
        public int TrialThreshold => trialThreshold;
        public float RecoveryReduction => recoveryReduction;
        public bool TrialEnabled { get; private set; }
        public bool TrialReady => Points >= trialThreshold;
        public string Latest { get; private set; } = "Finish a Heavy against a recently attacking opponent. No dummy credit.";
        private CombatActions actions;
        private CombatVitality vitality;
        private int pendingSequence = -1;
        private bool pendingHeavy;
        private string rejection;
        private readonly List<CombatObservation> candidates = new List<CombatObservation>();
        private readonly Dictionary<HitReceiver, EnemyImpactContext> consumed = new Dictionary<HitReceiver, EnemyImpactContext>();
        private readonly Queue<string> history = new Queue<string>();
        public IEnumerable<string> History => history.ToArray();

        public void Configure(AttackDefinition[] definitions) => heavyDefinitions = definitions;
        private void Awake() { actions = GetComponent<CombatActions>(); vitality = GetComponent<CombatVitality>(); }
        private void OnEnable()
        {
            actions.Observed += OnObservation; vitality.ResetPerformed += ResetPractice; toggleTrial.Enable();
        }
        private void OnDisable()
        {
            actions.Observed -= OnObservation; vitality.ResetPerformed -= ResetPractice; toggleTrial.Disable();
            pendingSequence = -1; candidates.Clear();
        }
        private void Update()
        {
            if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && toggleTrial.WasPressedThisFrame()) ToggleTrial();
        }
        public void ToggleTrial()
        {
            TrialEnabled = !TrialEnabled;
            Latest = TrialEnabled ? TrialReady ? $"Trial enabled: next Heavy has {recoveryReduction:P0} less base recovery." : "Trial requested; earn practice first."
                : "Trial disabled: authored recovery on future attacks.";
        }
        public AttackSnapshot Resolve(AttackDefinition definition, AttackSnapshot authored)
            => TrialEnabled && TrialReady && IsHeavy(definition) ? authored.WithRecovery(authored.Recovery * (1f - recoveryReduction)) : authored;

        private bool IsHeavy(AttackDefinition definition)
        {
            if (heavyDefinitions != null) foreach (var item in heavyDefinitions) if (item != null && item == definition) return true;
            return false;
        }
        private void OnObservation(CombatObservation observation)
        {
            if (observation.Kind == CombatObservationKind.Accepted)
            {
                pendingSequence = observation.ActionSequence; pendingHeavy = observation.ActionKind == CombatActionKind.Attack && IsHeavy(observation.Definition);
                candidates.Clear(); rejection = pendingHeavy ? "No qualifying hit" : "Only listed Heavy definitions qualify; dodge is not verified evasion";
                return;
            }
            if (observation.ActionSequence != pendingSequence) return;
            if (observation.Kind == CombatObservationKind.Interrupted)
            {
                Record($"#{pendingSequence} rejected: interrupted before recovery completed"); pendingSequence = -1; candidates.Clear(); return;
            }
            if (observation.Kind == CombatObservationKind.HitResolved && pendingHeavy)
            {
                string reason = Reject(observation);
                if (reason != null) { rejection = reason; Latest = "Pending rejection: " + reason; }
                else if (candidates.Count < 16) { candidates.Add(observation); Latest = "Eligible contact pending - finish recovery"; }
                return;
            }
            if (observation.Kind != CombatObservationKind.Completed) return;
            CombatObservation? best = null;
            foreach (var candidate in candidates)
            {
                string reason = Reject(candidate);
                if (reason != null) { rejection = reason; continue; }
                if (!best.HasValue || Credit(candidate) > Credit(best.Value)) best = candidate;
            }
            if (best.HasValue)
            {
                var hit = best.Value; var context = hit.Resolution.EnemyContext.Value;
                consumed[hit.Target] = context;
                int credit = Credit(hit);
                Points = Points > int.MaxValue - credit ? int.MaxValue : Points + credit;
                QualifiedActions++;
                bool guardBreak = hit.Resolution.Guarded && hit.Resolution.PoiseBroken;
                bool interrupt = context.PhaseBeforeHit == EnemyPhase.Windup && hit.Resolution.PoiseBroken;
                if (guardBreak) GuardBreaks++; if (interrupt) WindupInterrupts++;
                Record($"#{pendingSequence} +{credit}: completed Heavy" + (guardBreak ? " / guard break" : "")
                    + (interrupt ? " / interrupted windup" : "") + $" / threat {context.ThreatSequence}");
            }
            else Record($"#{pendingSequence} rejected: {rejection}");
            pendingSequence = -1; candidates.Clear();
        }
        private string Reject(CombatObservation hit)
        {
            if (hit.Target == null) return "Receiver was removed before completion";
            if (!(hit.Target is EnemyVitality) || !hit.Resolution.EnemyContext.HasValue) return "Training/non-enemy contact gives no practice";
            var context = hit.Resolution.EnemyContext.Value;
            var enemy = hit.Target.GetComponent<SandboxEnemy>();
            if (enemy == null || enemy.EncounterRevision != context.EncounterRevision) return "Opponent reset or removed before completion";
            if (!context.EngagedWithSource) return "Opponent was not engaging this actor";
            if (context.ThreatSequence <= 0 || hit.Time < context.ThreatStartedAt || hit.Time - context.ThreatStartedAt > recentThreatWindow)
                return "No recent committed enemy strike";
            if (context.PhaseBeforeHit == EnemyPhase.Idle || context.PhaseBeforeHit == EnemyPhase.Returning || context.PhaseBeforeHit == EnemyPhase.Defeated)
                return "Opponent was not in a live exchange";
            if (hit.Resolution.DamageApplied <= 0 && hit.Resolution.PoiseApplied <= 0) return "No applied impact";
            if (consumed.TryGetValue(hit.Target, out var previous))
            {
                if (previous.EncounterRevision != context.EncounterRevision) return "Opponent reset: reset the practice trial before a new encounter";
                if (context.ThreatSequence <= previous.ThreatSequence) return "This enemy strike already supplied practice";
            }
            else if (consumed.Count >= 64) return "Practice target capacity reached; reset trial";
            return null;
        }
        private static int Credit(CombatObservation hit) => hit.Resolution.PoiseBroken
            && (hit.Resolution.Guarded || hit.Resolution.EnemyContext.Value.PhaseBeforeHit == EnemyPhase.Windup) ? 2 : 1;
        private void Record(string message)
        {
            Latest = message; if (history.Count == 8) history.Dequeue(); history.Enqueue(message);
        }
        public void ResetPractice()
        {
            Points = QualifiedActions = GuardBreaks = WindupInterrupts = 0; TrialEnabled = false;
            pendingSequence = -1; candidates.Clear(); consumed.Clear(); history.Clear(); Latest = "Fresh practice trial; no inherited credit";
        }
    }
}
