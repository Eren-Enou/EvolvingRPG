using EvolvingRPG.Combat;
using EvolvingRPG.Movement;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Progression
{
    [RequireComponent(typeof(CombatActions), typeof(ContextualDiscovery))]
    public sealed class OwnedCharacter : MonoBehaviour, ICharacterDevelopment
    {
        [SerializeField] AttackDefinition baseline, breaker, momentum;
        [SerializeField] EnemySandboxHUD encounter;
        CombatActions actions;
        ContextualDiscovery discovery;
        RetaliatoryMomentumTrial trial;
        PlaygroundMouseLook mouseLook;
        PlayerInput playerInput;
        Vector3 spawn;
        bool menu, previousMouseLook, previousInput;
        readonly InputAction inspect = new InputAction("InspectDevelopment", InputActionType.Button, "<Keyboard>/tab");
        public CharacterDevelopmentState State { get; private set; }
        public string Message { get; private set; } = "One experimental point: inspect Breaker or Momentum with Tab at spawn.";
        public bool MenuOpen => menu;
        public bool MomentumOwned => State != null && State.Owns(DevelopmentIds.Momentum);
        public bool RetaliationActive => State != null && State.RetaliationActive && State.Heavy == DevelopmentIds.Momentum;
        public int BuildRevision => State?.BuildRevision ?? 0;
        public bool CanDevelop => actions != null && !actions.Busy && !actions.MovementBlocked
            && Vector3.Distance(transform.position, spawn) < 2f && SafeOpponents();
        public void Configure(AttackDefinition original, AttackDefinition planted, AttackDefinition advancing, EnemySandboxHUD hud)
        { baseline = original; breaker = planted; momentum = advancing; encounter = hud; }
        void Awake()
        {
            actions = GetComponent<CombatActions>(); discovery = GetComponent<ContextualDiscovery>(); trial = GetComponent<RetaliatoryMomentumTrial>();
            mouseLook = GetComponent<PlaygroundMouseLook>(); playerInput = GetComponent<PlayerInput>();
            State = new CharacterDevelopmentState(); spawn = transform.position;
        }
        void Start()
        {
            if (baseline == null || breaker == null || momentum == null || encounter == null) { enabled = false; Debug.LogError("Owned-character authoring references are incomplete.", this); return; }
            discovery.Ledger.Changed += OnKnowledge; encounter.SetInterfaceVisible(false); Apply();
        }
        void OnEnable() => inspect.Enable();
        void OnDisable() { inspect.Disable(); CloseMenu(); if (discovery != null && discovery.Ledger != null) discovery.Ledger.Changed -= OnKnowledge; }
        void OnKnowledge() => State.RecordKnowledge(discovery.Ledger.Exchanges);
        bool SafeOpponents()
        {
            foreach (var enemy in FindObjectsByType<SandboxEnemy>(FindObjectsSortMode.None))
                if (enemy.gameObject.scene == gameObject.scene && enemy.isActiveAndEnabled && enemy.Phase != EnemyPhase.Idle && enemy.Phase != EnemyPhase.Defeated) return false;
            return true;
        }
        public bool Purchase(string id, int revision)
        {
            if (!CanDevelop) { Message = "Return to spawn with R and finish recovery before developing."; return false; }
            bool success = State.Purchase(id, revision, out string message); Message = message; if (success) Apply(); return success;
        }
        public bool ConfigureHeavy(string id, bool retaliation)
        {
            if (!CanDevelop) { Message = "Configure only at the safe spawn while ready."; return false; }
            bool success = State.Configure(id, retaliation, out string message); Message = message; if (success) Apply(); return success;
        }
        public bool FreshCharacter()
        {
            if (!CanDevelop) { Message = "Return to spawn with R before creating a fresh character."; return false; }
            State = new CharacterDevelopmentState(); discovery.RestoreCharacterKnowledge(0); Apply();
            Message = "Fresh character: no knowledge, purchases or active evolution. Unsaved previous character was replaced."; return true;
        }
        void Apply()
        {
            actions.Configure(actions.LightAttack, State.Heavy == DevelopmentIds.Momentum ? momentum : State.Heavy == DevelopmentIds.Breaker ? breaker : baseline);
            if (trial != null) trial.InvalidateOpportunity();
        }
        void Update()
        {
            if (!Application.isFocused) { CloseMenu(); return; }
            if (menu && !CanDevelop) CloseMenu();
            if (inspect.WasPressedThisFrame())
            {
                if (menu) CloseMenu();
                else if (CanDevelop)
                {
                    previousMouseLook = mouseLook != null && mouseLook.enabled; previousInput = playerInput != null && playerInput.inputIsActive;
                    if (mouseLook != null) mouseLook.enabled = false;
                    if (playerInput != null) playerInput.DeactivateInput();
                    var input = GetComponent<StarterAssetsInputs>(); input.move = input.look = Vector2.zero; input.jump = input.sprint = false;
                    menu = true; Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
                }
                else Message = "Inspect at the safe spawn: press R first. Combat continues normally.";
            }
        }
        public void CloseMenu()
        {
            if (!menu) return;
            menu = false;
            if (mouseLook != null) mouseLook.enabled = previousMouseLook;
            if (playerInput != null && previousInput) playerInput.ActivateInput();
            // Keep cursor released; the next deliberate Game-view click captures it without attacking.
        }
    }
}
