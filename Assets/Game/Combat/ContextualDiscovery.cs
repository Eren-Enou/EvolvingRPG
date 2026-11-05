using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(CombatActions), typeof(EvasionEvidence))]
    public sealed class ContextualDiscovery : MonoBehaviour
    {
        [SerializeField] AttackDefinition momentum;
        [SerializeField, Min(0.1f)] float counterWindow = 1.5f;
        [SerializeField] bool momentumInvestment, evasiveInvestment;
        CombatActions actions;
        CombatVitality vitality;
        EvasionEvidence evidence;
        int investmentVersion;
        bool observedMomentum, observedEvasion;
        public event System.Action FreshCharacterStarted;
        readonly InputAction investMomentum = new InputAction("MomentumFixture", InputActionType.Button, "<Keyboard>/f4");
        readonly InputAction investEvasion = new InputAction("EvasionFixture", InputActionType.Button, "<Keyboard>/f5");
        readonly InputAction fresh = new InputAction("FreshDiscoveryCharacter", InputActionType.Button, "<Keyboard>/f8");
        public RetaliationDiscovery Ledger { get; private set; }
        public bool MomentumInvestment => momentumInvestment;
        public bool EvasiveInvestment => evasiveInvestment;
        public AttackDefinition Momentum => momentum;
        public float Window => Ledger != null ? Ledger.Window : counterWindow;
        public int InvestmentVersion => investmentVersion;
        public void Configure(AttackDefinition definition) { momentum = definition; }
        void Awake()
        {
            actions = GetComponent<CombatActions>(); vitality = GetComponent<CombatVitality>(); evidence = GetComponent<EvasionEvidence>();
            Ledger = new RetaliationDiscovery(counterWindow);
            observedMomentum = momentumInvestment; observedEvasion = evasiveInvestment;
        }
        void OnEnable()
        {
            actions.Observed += OnAction; evidence.Observed += OnEvidence; vitality.ResetPerformed += OnReset;
            investMomentum.Enable(); investEvasion.Enable(); fresh.Enable();
        }
        void OnDisable()
        {
            actions.Observed -= OnAction; evidence.Observed -= OnEvidence; vitality.ResetPerformed -= OnReset;
            Ledger.Invalidate("Discovery collector disabled.");
            investMomentum.Disable(); investEvasion.Disable(); fresh.Disable();
        }
        public bool SetInvestments(bool advancing, bool defensive)
        {
            if (actions.Busy || actions.MovementBlocked) return false;
            if (advancing == momentumInvestment && defensive == evasiveInvestment) return true;
            momentumInvestment = observedMomentum = advancing; evasiveInvestment = observedEvasion = defensive; investmentVersion++;
            Ledger.Invalidate("Experimental investment fixture changed; pending exchange discarded."); return true;
        }
        public bool FreshCharacter()
        {
            if (actions.Busy || actions.MovementBlocked) return false;
            momentumInvestment = evasiveInvestment = observedMomentum = observedEvasion = false; investmentVersion++; Ledger.FreshCharacter();
            FreshCharacterStarted?.Invoke(); return true;
        }
        void Update()
        {
            if (!Application.isFocused || Cursor.lockState != CursorLockMode.Locked) return;
            if (investMomentum.WasPressedThisFrame()) SetInvestments(!momentumInvestment, evasiveInvestment);
            if (investEvasion.WasPressedThisFrame()) SetInvestments(momentumInvestment, !evasiveInvestment);
            if (fresh.WasPressedThisFrame()) FreshCharacter();
        }
        void OnReset() => Ledger.ClearEncounter();
        void OnEvidence(EvasionResult result) { CheckInspectorFixtures(); Ledger.Evidence(result); }
        void CheckInspectorFixtures()
        {
            if (observedMomentum == momentumInvestment && observedEvasion == evasiveInvestment) return;
            observedMomentum = momentumInvestment; observedEvasion = evasiveInvestment; investmentVersion++;
            Ledger.Invalidate("Investment fixture changed in Inspector; exchange discarded.");
        }
        void OnAction(CombatObservation value)
        {
            CheckInspectorFixtures();
            if (value.Kind == CombatObservationKind.Accepted)
            {
                if (value.ActionKind == CombatActionKind.Dodge) Ledger.BeginDodge(value.ActionSequence, momentumInvestment && evasiveInvestment, investmentVersion);
                else Ledger.BeginAttack(value.ActionSequence, value.Definition == momentum, value.Time, investmentVersion);
            }
            else if (value.Kind == CombatObservationKind.Completed)
            {
                if (value.ActionKind == CombatActionKind.Dodge) Ledger.CompleteDodge(value.ActionSequence, value.Time);
                else Ledger.CompleteAttack(value.ActionSequence, investmentVersion, value.Time);
            }
            else if (value.Kind == CombatObservationKind.HitResolved) Ledger.Hit(value.ActionSequence, value.Target, value.Resolution);
            else if (value.Kind == CombatObservationKind.Interrupted) Ledger.Interrupt(value.ActionSequence);
        }
    }
}
