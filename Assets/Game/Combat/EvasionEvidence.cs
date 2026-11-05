using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(CombatActions), typeof(CombatVitality), typeof(CharacterController))]
    public sealed class EvasionEvidence : MonoBehaviour
    {
        [SerializeField] private SandboxEnemy[] opponents = Array.Empty<SandboxEnemy>();
        [SerializeField, Range(0.005f, 0.025f)] private float maximumSampleGap = 0.025f;
        [SerializeField, Min(1)] private float maximumSupportedSpeed = 16f;
        private CombatActions actions;
        private CombatVitality vitality;
        private CharacterController body;
        private ThirdPersonController controller;
        private EvasionAttempt pending;
        private int lastDodgeSequence = -1;
        private readonly HashSet<EnemyThreat> used = new HashSet<EnemyThreat>();
        private readonly Queue<EvasionResult> history = new Queue<EvasionResult>();
        public IEnumerable<EvasionResult> History => history.ToArray();
        public EvasionResult? Latest { get; private set; }
        public EnemyThreat? LatestThreat { get; private set; }
        public ThreatResolution? LatestThreatResolution { get; private set; }
        public EvasionAttempt Pending => pending;
        public event Action<EvasionResult> Observed;

        public void Configure(SandboxEnemy[] sources)
        {
            UnsubscribeOpponents(); opponents = sources ?? Array.Empty<SandboxEnemy>();
            if (Application.isPlaying && isActiveAndEnabled) SubscribeOpponents();
        }
        private void Awake()
        {
            actions = GetComponent<CombatActions>(); vitality = GetComponent<CombatVitality>();
            body = GetComponent<CharacterController>(); controller = GetComponent<ThirdPersonController>();
        }
        private void OnEnable()
        {
            actions.Observed += OnAction; vitality.HitReceived += OnHit; vitality.ResetPerformed += OnReset;
            SubscribeOpponents();
        }
        private void OnDisable()
        {
            actions.Observed -= OnAction; vitality.HitReceived -= OnHit; vitality.ResetPerformed -= OnReset;
            UnsubscribeOpponents();
            if (pending != null) { pending.Mark(EvasionReason.IncompleteSamples); Publish(); }
        }
        private void SubscribeOpponents()
        {
            foreach (var enemy in opponents) if (enemy != null)
            { enemy.ThreatCommitted -= OnThreat; enemy.ThreatFinished -= OnFinished; enemy.ThreatCommitted += OnThreat; enemy.ThreatFinished += OnFinished; }
        }
        private void UnsubscribeOpponents()
        {
            foreach (var enemy in opponents) if (enemy != null)
            { enemy.ThreatCommitted -= OnThreat; enemy.ThreatFinished -= OnFinished; }
        }
        private void OnAction(CombatObservation observation)
        {
            if (observation.Kind == CombatObservationKind.Accepted)
            {
                if (observation.ActionKind == CombatActionKind.Dodge && observation.ActionSequence <= lastDodgeSequence) return;
                if (pending != null) { pending.Mark(EvasionReason.AdditionalAction); Publish(); }
                if (observation.ActionKind != CombatActionKind.Dodge || observation.ActionSequence <= lastDodgeSequence) return;
                lastDodgeSequence = observation.ActionSequence;
                EnemyThreat? selected = null; int count = 0; bool untracked = false;
                foreach (var enemy in opponents)
                {
                    if (enemy != null && enemy.isActiveAndEnabled && enemy.CurrentThreat.HasValue && enemy.CurrentThreat.Value.Target == vitality)
                    { selected = enemy.CurrentThreat; count++; }
                    else if (IsUntracked(enemy)) untracked = true;
                }
                used.RemoveWhere(key => key.Source == null || key.Source.EncounterRevision != key.Revision);
                pending = new EvasionAttempt(observation.ActionSequence, observation.Time, observation.Time - Time.deltaTime,
                    StrikeCapsule.Capture(body), actions.DodgeDirection, actions.DodgeTravelDistance, actions.DodgeTravelDuration,
                    selected, maximumSampleGap, maximumSupportedSpeed);
                if (count > 1) pending.Mark(EvasionReason.OverlappingThreats);
                if (actions.DodgeTravelDistance / actions.DodgeTravelDuration > maximumSupportedSpeed)
                    pending.Mark(EvasionReason.IncompleteSamples);
                if (untracked) pending.Mark(EvasionReason.UntrackedThreat);
                if (used.Count >= 64) pending.Mark(EvasionReason.Capacity);
                if (selected.HasValue && used.Contains(selected.Value)) pending.Mark(EvasionReason.ReusedThreat);
            }
            else if (pending != null && observation.ActionKind == CombatActionKind.Dodge && observation.ActionSequence == pending.Sequence)
            {
                if (observation.Kind == CombatObservationKind.Completed) pending.EndDodge(true, StrikeCapsule.Capture(body));
                if (observation.Kind == CombatObservationKind.Interrupted) pending.EndDodge(false, StrikeCapsule.Capture(body));
            }
        }
        private void OnThreat(EnemyThreat threat)
        {
            LatestThreat = threat; LatestThreatResolution = null;
            if (pending == null || threat.Target != vitality) return;
            pending.Mark(pending.Threat.HasValue ? EvasionReason.OverlappingThreats : EvasionReason.BeforeCommitment);
        }
        private void OnFinished(ThreatResolution resolution) { LatestThreatResolution = resolution; pending?.Resolve(resolution); }
        private void OnHit(HitData hit, HitResolution resolution) { pending?.IncomingHit(); }
        private void OnReset()
        {
            if (pending == null) return;
            pending.Mark(EvasionReason.PlayerReset); Publish();
            // Retain used keys until the corresponding enemy revision changes; player-only reset cannot replay one threat.
        }
        private bool IsUntracked(SandboxEnemy enemy) => enemy != null && enemy.isActiveAndEnabled && enemy.EngagedWith(vitality)
            && !enemy.CurrentThreat.HasValue && (enemy.Phase == EnemyPhase.Windup || enemy.Phase == EnemyPhase.Active);
        private void LateUpdate()
        {
            if (pending == null) return;
            foreach (var enemy in opponents)
            {
                if (IsUntracked(enemy)) pending.Mark(EvasionReason.UntrackedThreat);
                if (enemy != null && enemy.isActiveAndEnabled && enemy.CurrentThreat.HasValue && enemy.CurrentThreat.Value.Target == vitality
                    && (!pending.Threat.HasValue || !pending.Threat.Value.Equals(enemy.CurrentThreat.Value)))
                    pending.Mark(pending.Threat.HasValue ? EvasionReason.OverlappingThreats : EvasionReason.BeforeCommitment);
            }
            var pose = StrikeCapsule.Capture(body);
            Vector3 origin = pending.Threat.HasValue ? pending.Threat.Value.Geometry.Origin : Vector3.zero;
            bool threatened = false, actual = false, ambiguous = false;
            if (pending.Threat.HasValue)
            {
                var threat = pending.Threat.Value;
                if (threat.Source == null || !threat.Source.isActiveAndEnabled) pending.Mark(EvasionReason.EnemyInterrupted);
                else
                {
                    if (threat.Source.EncounterRevision != threat.Revision) pending.Mark(EvasionReason.EnemyReset);
                    if (!threat.Source.EngagedWith(vitality)) pending.Mark(EvasionReason.EnemyInterrupted);
                    if (!threat.Source.HasUnchangedThreatParameters(threat)) pending.Mark(EvasionReason.IncompleteSamples);
                    origin = threat.Source.transform.position + Vector3.up;
                    var geometry = threat.Geometry.At(origin);
                    threatened = StrikeContactQuery.ContactsAtPose(geometry, pending.Start, body, vitality, out ambiguous);
                    actual = StrikeContactQuery.Contacts(geometry, body, vitality);
                    if (pose.Supported && body.enabled && (pose.ClosestPoint(body.ClosestPoint(origin)) - body.ClosestPoint(origin)).magnitude > 0.015f)
                        ambiguous = true;
                }
            }
            if (controller == null || !controller.enabled || !actions.isActiveAndEnabled) pending.Mark(EvasionReason.IncompleteSamples);
            if (!Mathf.Approximately(actions.DodgeTravelDistance, pending.ExpectedDistance)
                || !Mathf.Approximately(actions.DodgeTravelDuration, pending.TravelDuration)) pending.Mark(EvasionReason.IncompleteSamples);
            if (controller != null && Mathf.Max(controller.MoveSpeed, controller.SprintSpeed) > maximumSupportedSpeed)
                pending.Mark(EvasionReason.IncompleteSamples);
            pending.Sample(Time.time, pose, (body.collisionFlags & CollisionFlags.Sides) != 0,
                origin, threatened, actual, ambiguous);
            if (pending.Samples >= 512 || Time.time - pending.AcceptedAt > 5f) pending.Mark(EvasionReason.IncompleteSamples);
            if (pending.Ready || pending.Samples >= 512 || Time.time - pending.AcceptedAt > 5f) Publish();
        }
        private void Publish()
        {
            var attempt = pending;
            pending = null; // Clear first so a diagnostic listener cannot publish the same attempt twice.
            if (attempt == null) return;
            var result = attempt.Finish(); Latest = result;
            if (result.Threat.HasValue && used.Count < 64) used.Add(result.Threat.Value);
            if (history.Count == 16) history.Dequeue(); history.Enqueue(result);
            Observed?.Invoke(result);
        }
    }
}
