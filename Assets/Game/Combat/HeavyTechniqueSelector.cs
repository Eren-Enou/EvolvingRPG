using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    // Experiment tool only: no ownership, investment, mastery or saved loadout policy.
    [DefaultExecutionOrder(-45)]
    [RequireComponent(typeof(CombatActions))]
    public sealed class HeavyTechniqueSelector : MonoBehaviour
    {
        [SerializeField] private AttackDefinition baseline, breaker, momentum;
        private readonly InputAction chooseBaseline = new InputAction("BaselineHeavy", InputActionType.Button, "<Keyboard>/1");
        private readonly InputAction chooseBreaker = new InputAction("BreakerHeavy", InputActionType.Button, "<Keyboard>/2");
        private readonly InputAction chooseMomentum = new InputAction("MomentumHeavy", InputActionType.Button, "<Keyboard>/3");
        private CombatActions actions;
        public int Selected { get; private set; }
        public string Message { get; private set; } = "1 Baseline | 2 Breaker | 3 Momentum - RMB attacks";
        public AttackDefinition SelectedDefinition => Selected == 1 ? breaker : Selected == 2 ? momentum : baseline;
        public void Configure(AttackDefinition original, AttackDefinition planted, AttackDefinition advancing)
        { baseline = original; breaker = planted; momentum = advancing; }
        private void Awake() => actions = GetComponent<CombatActions>();
        private void Start() => TrySelect(0);
        private void OnEnable() { chooseBaseline.Enable(); chooseBreaker.Enable(); chooseMomentum.Enable(); }
        private void OnDisable() { chooseBaseline.Disable(); chooseBreaker.Disable(); chooseMomentum.Disable(); }
        private void Update()
        {
            if (!Application.isFocused || Cursor.lockState != CursorLockMode.Locked) return;
            if (chooseBaseline.WasPressedThisFrame()) TrySelect(0);
            else if (chooseBreaker.WasPressedThisFrame()) TrySelect(1);
            else if (chooseMomentum.WasPressedThisFrame()) TrySelect(2);
        }
        public bool TrySelect(int index)
        {
            if (!isActiveAndEnabled || actions == null || !actions.enabled || index < 0 || index > 2) return false;
            if (actions.Busy || actions.MovementBlocked) { Message = "Finish recovery before selecting; no queued switch"; return false; }
            var definition = index == 1 ? breaker : index == 2 ? momentum : baseline;
            if (definition == null) return false;
            Selected = index; actions.Configure(actions.LightAttack, definition);
            Message = definition.DisplayName + (index == 1 ? ": planted, stronger poise, longer commitment"
                : index == 2 ? ": advancing, lighter impact, extra miss recovery" : ": original Heavy reference");
            return true;
        }
    }
}
