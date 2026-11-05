using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    public enum CombatPhase { Ready, Windup, Active, Recovery, Dodge, DodgeRecovery }

    [DefaultExecutionOrder(-40)]
    [RequireComponent(typeof(MeleeHitDetector), typeof(CombatStamina), typeof(CombatMovement))]
    public sealed class CombatActions : MonoBehaviour
    {
        [SerializeField] private AttackDefinition lightAttack;
        [SerializeField] private AttackDefinition heavyAttack;
        [SerializeField] private MonoBehaviour attackResolver;
        [SerializeField] private InputAction light = new InputAction("Light", InputActionType.Button, "<Mouse>/leftButton");
        [SerializeField] private InputAction heavy = new InputAction("Heavy", InputActionType.Button, "<Mouse>/rightButton");
        [SerializeField] private InputAction dodge = new InputAction("Dodge", InputActionType.Button, "<Keyboard>/leftShift");
        [SerializeField, Min(0.1f)] private float dodgeDistance = 3f;
        [SerializeField, Min(0.05f)] private float dodgeDuration = 0.25f;
        [SerializeField, Min(0.05f)] private float dodgeRecovery = 0.35f;
        [SerializeField, Min(0f)] private float dodgeCost = 25f;

        public AttackDefinition LightAttack => lightAttack;
        public AttackDefinition HeavyAttack => heavyAttack;
        public AttackDefinition CurrentAttack { get; private set; }
        public AttackSnapshot CommittedAttack { get; private set; }
        public bool HasCommittedAttack { get; private set; }
        public AttackCommitment? CommittedContext { get; private set; }
        private bool contextTargetHit;
        public CombatPhase Phase { get; private set; }
        public bool Busy => Phase != CombatPhase.Ready;
        public bool MovementBlocked => vitality != null && (vitality.Defeated || vitality.Staggered);
        public Quaternion Facing { get; private set; }
        public Vector3 DodgeDirection { get; private set; }
        public float DodgeTravelDistance => dodgeDistance;
        public float DodgeTravelDuration => dodgeDuration;
        public float DodgeSpeedThisFrame { get; private set; }
        public float AttackAdvanceSpeedThisFrame { get; private set; }
        public float PhaseProgress { get; private set; }
        public string Message { get; private set; } = "Ready";
        public int AcceptedActions { get; private set; }
        public CombatObservationLog Diagnostics { get; } = new CombatObservationLog();
        public event Action<CombatObservation> Observed;
        private StarterAssetsInputs inputs;
        private ThirdPersonController controller;
        private CombatStamina stamina;
        private CombatVitality vitality;
        private MeleeHitDetector detector;
        private Transform movementCamera;
        private readonly HashSet<HitReceiver> hitTargets = new HashSet<HitReceiver>();
        private float elapsed;
        private bool pointerWasLocked;
        private bool dodging;
        private MeleeMissReason missReason;

        private void Awake()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            controller = GetComponent<ThirdPersonController>();
            stamina = GetComponent<CombatStamina>();
            detector = GetComponent<MeleeHitDetector>();
        }

        private void Start() { if (Camera.main != null) movementCamera = Camera.main.transform; }
        private void OnEnable()
        {
            vitality = GetComponent<CombatVitality>();
            if (vitality != null) vitality.HitReceived += OnImpact;
            light.Enable(); heavy.Enable(); dodge.Enable();
        }
        private void OnDisable()
        {
            if (vitality != null) vitality.HitReceived -= OnImpact;
            if (Busy) Observe(CombatObservationKind.Interrupted);
            light.Disable(); heavy.Disable(); dodge.Disable();
            Phase = CombatPhase.Ready;
            CurrentAttack = null;
            HasCommittedAttack = false;
            CommittedAttack = default;
            CommittedContext = null; contextTargetHit = false;
            AttackAdvanceSpeedThisFrame = 0;
            pointerWasLocked = false;
        }

        public void Configure(AttackDefinition lightDefinition, AttackDefinition heavyDefinition)
        {
            lightAttack = lightDefinition;
            heavyAttack = heavyDefinition;
        }

        public void ConfigureResolver(MonoBehaviour resolver)
        {
            if (resolver != null && !(resolver is IAttackResolver)) throw new ArgumentException("Resolver must implement IAttackResolver.");
            attackResolver = resolver;
        }

        private void Update()
        {
            bool locked = Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
            bool acceptInput = locked && pointerWasLocked;
            pointerWasLocked = locked;
            DodgeSpeedThisFrame = 0f;
            AttackAdvanceSpeedThisFrame = 0f;
            if (Busy)
            {
                TickAction();
                return; // No same-frame chain at the recovery boundary, or buffered/held-button repeat.
            }
            if (!acceptInput) return; // A click used to capture the cursor never attacks.
            if (dodge.WasPressedThisFrame()) TryDodge();
            else if (heavy.WasPressedThisFrame()) TryAttack(heavyAttack);
            else if (light.WasPressedThisFrame()) TryAttack(lightAttack);
            if (Busy) TickAction();
        }

        private bool CanStart(float cost)
        {
            if (!isActiveAndEnabled || MovementBlocked) { Message = "Defeated or staggered"; return false; }
            if (Busy) return false;
            if (!controller.Grounded) { Message = "Land before acting"; return false; }
            if (!stamina.TrySpend(cost)) { Message = "Not enough stamina"; return false; }
            Facing = transform.rotation;
            elapsed = 0f;
            AcceptedActions++;
            return true;
        }

        private void OnImpact(HitData hit, HitResolution resolution)
        {
            if (resolution.PoiseBroken || resolution.Defeated) Interrupt(resolution.Defeated ? "Defeated - R resets" : "Staggered");
        }

        public void Interrupt(string message)
        {
            if (Busy) Observe(CombatObservationKind.Interrupted);
            Phase = CombatPhase.Ready; CurrentAttack = null; CommittedAttack = default;
            CommittedContext = null; contextTargetHit = false;
            HasCommittedAttack = false; dodging = false; DodgeSpeedThisFrame = 0; PhaseProgress = 0;
            AttackAdvanceSpeedThisFrame = 0;
            Facing = transform.rotation; Message = message;
        }

        public bool TryAttack(AttackDefinition attack)
        {
            if (attack == null) return false;
            var snapshot = new AttackSnapshot(attack);
            if (attackResolver != null && attackResolver.isActiveAndEnabled && attackResolver is IAttackResolver resolver)
                snapshot = resolver.Resolve(attack, snapshot);
            if (!CanStart(snapshot.StaminaCost)) return false;
            CurrentAttack = attack;
            CommittedAttack = snapshot;
            CommittedContext = attackResolver != null && attackResolver.isActiveAndEnabled && attackResolver is IAttackAcceptanceResolver acceptance
                ? acceptance.Accepted(attack, snapshot) : (AttackCommitment?)null;
            contextTargetHit = false;
            HasCommittedAttack = true;
            dodging = false;
            hitTargets.Clear();
            missReason = MeleeMissReason.NoTargetInReach;
            Phase = CombatPhase.Windup;
            Message = snapshot.DisplayName + " committed";
            Observe(CombatObservationKind.Accepted);
            return true;
        }

        public bool TryDodge()
        {
            if (!CanStart(dodgeCost)) return false;
            Vector3 inputDirection = new Vector3(inputs.move.x, 0f, inputs.move.y);
            DodgeDirection = inputDirection.sqrMagnitude > 0.001f && movementCamera != null
                ? Quaternion.Euler(0f, movementCamera.eulerAngles.y, 0f) * inputDirection.normalized
                : -(Facing * Vector3.forward);
            CurrentAttack = null;
            HasCommittedAttack = false;
            CommittedAttack = default;
            CommittedContext = null; contextTargetHit = false;
            dodging = true;
            Phase = CombatPhase.Dodge;
            Message = "Dodge (no invulnerability)";
            Observe(CombatObservationKind.Accepted);
            return true;
        }

        private void TickAction()
        {
            float previous = elapsed;
            elapsed += Time.deltaTime;
            if (dodging)
            {
                // Integrate only the part of this frame inside the travel window: stable distance at low FPS.
                float travelTime = Mathf.Max(0f, Mathf.Min(elapsed, dodgeDuration) - Mathf.Min(previous, dodgeDuration));
                DodgeSpeedThisFrame = Time.deltaTime > 0f ? dodgeDistance / dodgeDuration * travelTime / Time.deltaTime : 0f;
                Phase = elapsed < dodgeDuration ? CombatPhase.Dodge : CombatPhase.DodgeRecovery;
                PhaseProgress = Phase == CombatPhase.Dodge ? elapsed / dodgeDuration
                    : Mathf.Clamp01((elapsed - dodgeDuration) / dodgeRecovery);
                if (elapsed >= dodgeDuration + dodgeRecovery) EndAction();
                return;
            }

            var attack = CommittedAttack;
            // Integrate only this frame's windup interval, just as dodge integrates its travel window.
            float advanceTime = Mathf.Max(0f, Mathf.Min(elapsed, attack.Windup) - Mathf.Min(previous, attack.Windup));
            AttackAdvanceSpeedThisFrame = Time.deltaTime > 0f ? attack.AdvanceDistance / attack.Windup * advanceTime / Time.deltaTime : 0f;
            float activeEnd = attack.Windup + attack.ActiveTime;
            if (elapsed >= attack.Windup && previous < activeEnd)
            {
                int hits = detector.ResolveSwing(attack, Facing, hitTargets, out MeleeMissReason queryReason, ReportResolvedHit);
                if (queryReason > missReason) missReason = queryReason;
                if (hits > 0) Message = $"{attack.DisplayName} HIT | {attack.Damage:0} damage / {attack.PoiseDamage:0} poise";
            }
            Phase = elapsed < attack.Windup ? CombatPhase.Windup : elapsed < activeEnd ? CombatPhase.Active : CombatPhase.Recovery;
            float recoveryTime = attack.Recovery + (hitTargets.Count == 0 ? attack.MissRecovery : 0)
                + (CommittedContext.HasValue && (!contextTargetHit || !CommittedContext.Value.SameGeneration) ? CommittedContext.Value.TargetMissRecovery : 0);
            PhaseProgress = Phase == CombatPhase.Windup ? elapsed / attack.Windup : Phase == CombatPhase.Active
                ? (elapsed - attack.Windup) / attack.ActiveTime : Mathf.Clamp01((elapsed - activeEnd) / recoveryTime);
            if (elapsed >= activeEnd + recoveryTime) EndAction();
        }

        private void EndAction()
        {
            Observe(CombatObservationKind.Completed);
            if (!dodging && hitTargets.Count == 0)
                Message = CommittedAttack.DisplayName + " missed: " + MeleeMissFeedback.Describe(missReason);
            Phase = CombatPhase.Ready;
            CurrentAttack = null;
            HasCommittedAttack = false;
            CommittedAttack = default;
            CommittedContext = null; contextTargetHit = false;
            dodging = false;
            PhaseProgress = 0f;
        }

        private void ReportResolvedHit(HitReceiver target, HitResolution resolution)
        {
            if (CommittedContext.HasValue && CommittedContext.Value.Matches(target, resolution)) contextTargetHit = true;
            Observe(CombatObservationKind.HitResolved, target, resolution);
        }

        private void Observe(CombatObservationKind kind, HitReceiver target = null, HitResolution resolution = default)
        {
            var observation = new CombatObservation(AcceptedActions, kind,
                dodging ? CombatActionKind.Dodge : CombatActionKind.Attack,
                CurrentAttack, target, resolution, dodging ? 0 : hitTargets.Count,
                HasCommittedAttack ? CommittedAttack : (AttackSnapshot?)null,
                kind == CombatObservationKind.Completed && !dodging && hitTargets.Count == 0 ? missReason : MeleeMissReason.None);
            Diagnostics.Record(observation);
            Observed?.Invoke(observation);
        }
    }
}
