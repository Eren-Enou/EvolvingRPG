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
        public CombatPhase Phase { get; private set; }
        public bool Busy => Phase != CombatPhase.Ready;
        public Quaternion Facing { get; private set; }
        public Vector3 DodgeDirection { get; private set; }
        public float DodgeSpeedThisFrame { get; private set; }
        public float PhaseProgress { get; private set; }
        public string Message { get; private set; } = "Ready";
        public int AcceptedActions { get; private set; }
        private StarterAssetsInputs inputs;
        private ThirdPersonController controller;
        private CombatStamina stamina;
        private MeleeHitDetector detector;
        private Transform movementCamera;
        private readonly HashSet<TrainingTarget> hitTargets = new HashSet<TrainingTarget>();
        private float elapsed;
        private bool pointerWasLocked;
        private bool dodging;

        private void Awake()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            controller = GetComponent<ThirdPersonController>();
            stamina = GetComponent<CombatStamina>();
            detector = GetComponent<MeleeHitDetector>();
        }

        private void Start() { if (Camera.main != null) movementCamera = Camera.main.transform; }
        private void OnEnable() { light.Enable(); heavy.Enable(); dodge.Enable(); }
        private void OnDisable()
        {
            light.Disable(); heavy.Disable(); dodge.Disable();
            Phase = CombatPhase.Ready;
            CurrentAttack = null;
            pointerWasLocked = false;
        }

        public void Configure(AttackDefinition lightDefinition, AttackDefinition heavyDefinition)
        {
            lightAttack = lightDefinition;
            heavyAttack = heavyDefinition;
        }

        private void Update()
        {
            bool locked = Application.isFocused && Cursor.lockState == CursorLockMode.Locked;
            bool acceptInput = locked && pointerWasLocked;
            pointerWasLocked = locked;
            DodgeSpeedThisFrame = 0f;
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
            if (Busy) return false;
            if (!controller.Grounded) { Message = "Land before acting"; return false; }
            if (!stamina.TrySpend(cost)) { Message = "Not enough stamina"; return false; }
            Facing = transform.rotation;
            elapsed = 0f;
            AcceptedActions++;
            return true;
        }

        public bool TryAttack(AttackDefinition attack)
        {
            if (attack == null || !CanStart(attack.StaminaCost)) return false;
            CurrentAttack = attack;
            dodging = false;
            hitTargets.Clear();
            Phase = CombatPhase.Windup;
            Message = attack.DisplayName + " committed";
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
            dodging = true;
            Phase = CombatPhase.Dodge;
            Message = "Dodge (no invulnerability)";
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

            var attack = CurrentAttack;
            float activeEnd = attack.Windup + attack.ActiveTime;
            if (elapsed >= attack.Windup && previous < activeEnd)
            {
                int hits = detector.ResolveSwing(attack, Facing, hitTargets);
                if (hits > 0) Message = $"{attack.DisplayName} HIT | {attack.Damage:0} damage / {attack.PoiseDamage:0} poise";
            }
            Phase = elapsed < attack.Windup ? CombatPhase.Windup : elapsed < activeEnd ? CombatPhase.Active : CombatPhase.Recovery;
            PhaseProgress = Phase == CombatPhase.Windup ? elapsed / attack.Windup : Phase == CombatPhase.Active
                ? (elapsed - attack.Windup) / attack.ActiveTime : Mathf.Clamp01((elapsed - activeEnd) / attack.Recovery);
            if (elapsed >= attack.Duration) EndAction();
        }

        private void EndAction()
        {
            if (!dodging && hitTargets.Count == 0) Message = CurrentAttack.DisplayName + " missed";
            Phase = CombatPhase.Ready;
            CurrentAttack = null;
            dodging = false;
            PhaseProgress = 0f;
        }
    }
}
