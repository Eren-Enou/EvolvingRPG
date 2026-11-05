using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(ContextualDiscovery), typeof(EvasionEvidence), typeof(CombatActions))]
    public sealed class RetaliatoryMomentumTrial : MonoBehaviour, IAttackResolver, IAttackAcceptanceResolver
    {
        [SerializeField, Min(0)] float addedAdvance = .4f, targetMissRecovery = .25f;
        CombatActions actions;
        ContextualDiscovery discovery;
        EvasionEvidence evidence;
        StarterAssetsInputs inputs;
        CombatVitality vitality;
        EnemyThreat? opportunity;
        int dodgeSequence = -1, fixtureVersion;
        float dodgeCompletedAt, expiresAt;
        readonly InputAction toggle = new InputAction("RetaliationTrial", InputActionType.Button, "<Keyboard>/f2");
        bool trialEnabled;
        public bool TrialEnabled => discovery != null && discovery.Development != null ? discovery.Development.RetaliationActive : trialEnabled;
        public int Consumed { get; private set; }
        public string Diagnostic { get; private set; } = "Reveal the connection before enabling the optional F2 trial.";
        public float AddedAdvance => addedAdvance;
        public float AddedMissRecovery => targetMissRecovery;
        public bool OpportunityReady => Eligible();
        public float SecondsRemaining => Mathf.Max(0, expiresAt - Time.time);
        public EnemyThreat? Opportunity => opportunity;
        void Awake()
        {
            actions = GetComponent<CombatActions>(); discovery = GetComponent<ContextualDiscovery>();
            evidence = GetComponent<EvasionEvidence>(); inputs = GetComponent<StarterAssetsInputs>(); vitality = GetComponent<CombatVitality>();
        }
        void OnEnable() { actions.Observed += OnAction; evidence.Observed += OnEvidence; vitality.ResetPerformed += ResetOpportunity; discovery.FreshCharacterStarted += FreshCharacter; toggle.Enable(); }
        void OnDisable() { actions.Observed -= OnAction; evidence.Observed -= OnEvidence; vitality.ResetPerformed -= ResetOpportunity; discovery.FreshCharacterStarted -= FreshCharacter; trialEnabled = false; ResetOpportunity(); toggle.Disable(); }
        public bool SetTrialEnabled(bool enabled)
        {
            if (discovery.Development != null || actions.Busy || actions.MovementBlocked || enabled && discovery.Ledger.Knowledge != DiscoveryKnowledge.Revealed) return false;
            trialEnabled = enabled; ResetOpportunity(); Diagnostic = enabled
                ? "TRIAL ON: verified evasion arms one longer return. Forward intent + RMB opts in; neutral/strafe/backward uses ordinary Momentum."
                : "TRIAL OFF: ordinary Momentum.";
            return true;
        }
        void Update()
        {
            if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && toggle.WasPressedThisFrame())
                SetTrialEnabled(!TrialEnabled);
        }
        bool Eligible()
        {
            return isActiveAndEnabled && TrialEnabled && discovery.Ledger.Knowledge == DiscoveryKnowledge.Revealed
                && discovery.MomentumInvestment && discovery.EvasiveInvestment && fixtureVersion == discovery.InvestmentVersion
                && opportunity.HasValue && Time.time <= expiresAt && opportunity.Value.Source != null
                && opportunity.Value.Source.isActiveAndEnabled && opportunity.Value.Source.EncounterRevision == opportunity.Value.Revision
                && !opportunity.Value.Source.GetComponent<EnemyVitality>().Defeated;
        }
        public bool ForwardIntent
        {
            get
            {
                if (inputs == null || inputs.move.sqrMagnitude < .01f || Camera.main == null) return false;
                Vector3 world = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * new Vector3(inputs.move.x, 0, inputs.move.y).normalized;
                return Vector3.Dot(world, transform.forward) > .5f;
            }
        }
        public AttackSnapshot Resolve(AttackDefinition definition, AttackSnapshot authored)
        {
            return definition == discovery.Momentum && Eligible() && ForwardIntent
                ? authored.WithAdvance(authored.AdvanceDistance + addedAdvance, authored.DisplayName + " / Retaliatory trial") : authored;
        }
        public AttackCommitment? Accepted(AttackDefinition definition, AttackSnapshot committed)
        {
            if (definition != discovery.Momentum || !Eligible() || !ForwardIntent
                || !Mathf.Approximately(committed.AdvanceDistance, definition.AdvanceDistance + addedAdvance)) return null;
            var context = new AttackCommitment(opportunity.Value, targetMissRecovery);
            opportunity = null; Consumed++;
            Diagnostic = "Long return committed; +" + addedAdvance.ToString("0.00") + " m. Missing linked opponent adds " + targetMissRecovery.ToString("0.00") + " s recovery, including bystander-only hits.";
            return context;
        }
        void OnAction(CombatObservation value)
        {
            if (value.ActionKind != CombatActionKind.Dodge) return;
            if (value.Kind == CombatObservationKind.Accepted) { opportunity = null; dodgeSequence = value.ActionSequence; dodgeCompletedAt = float.NegativeInfinity; }
            else if (value.Kind == CombatObservationKind.Completed && value.ActionSequence == dodgeSequence) dodgeCompletedAt = value.Time;
            else if (value.Kind == CombatObservationKind.Interrupted) ResetOpportunity();
        }
        void OnEvidence(EvasionResult value)
        {
            if (!TrialEnabled || discovery.Ledger.Knowledge != DiscoveryKnowledge.Revealed
                || value.DodgeSequence != dodgeSequence || value.Classification != EvasionClassification.Verified || !value.Threat.HasValue) return;
            expiresAt = dodgeCompletedAt + discovery.Window;
            if (Time.time > expiresAt) { Diagnostic = "Verified evidence arrived after the original return window expired."; return; }
            opportunity = value.Threat; fixtureVersion = discovery.InvestmentVersion;
            Diagnostic = "Return armed for " + value.Threat.Value.Label + ". Forward intent + RMB requests +0.4 m; neutral/strafe/backward keeps ordinary Momentum.";
        }
        void FreshCharacter() { trialEnabled = false; ResetOpportunity(); Diagnostic = "Fresh character: trial disabled; discover the connection again."; }
        public void InvalidateOpportunity() => ResetOpportunity();
        void ResetOpportunity() { opportunity = null; dodgeSequence = -1; expiresAt = float.NegativeInfinity; }
    }
}
