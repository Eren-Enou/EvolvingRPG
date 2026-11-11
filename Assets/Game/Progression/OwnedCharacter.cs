using EvolvingRPG.Combat;
using EvolvingRPG.Movement;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Progression
{
    // Optional same-actor lifecycle gate, independent of safe-spawn distance and combat readiness.
    public interface ICharacterDevelopmentBoundary
    {
        bool AllowsDevelopment { get; }
    }

    [RequireComponent(typeof(CombatActions), typeof(ContextualDiscovery))]
    public sealed class OwnedCharacter : MonoBehaviour, ICharacterDevelopment
    {
        [SerializeField] AttackDefinition baseline, breaker, momentum;
        [SerializeField] EnemySandboxHUD encounter;
        [SerializeField, Range(0, 1)] int initialFixturePoints = 1;
        [SerializeField] string saveFolder = "CharacterDevelopmentPrototype";
        [SerializeField, Min(1)] float developmentRadius = 2f;
        CombatActions actions;
        ContextualDiscovery discovery;
        RetaliatoryMomentumTrial trial;
        PlaygroundMouseLook mouseLook;
        PlayerInput playerInput;
        Vector3 spawn;
        bool menu, previousMouseLook, previousInput;
        CharacterSaveStore saves;
        ICharacterDevelopmentBoundary boundary;
        public string SaveLocation => saves?.Location;
        int checkpointRevision;
        string checkpointSlot;
        public bool HasUnsavedProgress => State != null && State.Revision != checkpointRevision;
        public string SaveStatus => HasUnsavedProgress ? "Unsaved progress. Save before exiting to keep it."
            : checkpointSlot != null ? $"No new progress since slot {checkpointSlot}."
            : "Fresh Warrior. Resume an existing save in Saved Warriors.";
        readonly InputAction inspect = new InputAction("InspectDevelopment", InputActionType.Button, "<Keyboard>/tab");
        public CharacterDevelopmentState State { get; private set; }
        // A load replaces the record even when its stable character ID is unchanged.
        public event System.Action CharacterRecordReplaced;
        public CombatActionReview Review { get; private set; }
        public HeavyExperienceCollector Experience { get; private set; }
        public string Message { get; private set; } = "One experimental point: inspect Breaker or Momentum with Tab at spawn.";
        public bool MenuOpen => menu;
        public bool MomentumOwned => State != null && State.Owns(DevelopmentIds.Momentum);
        public bool RetaliationActive => State != null && State.RetaliationActive && State.Heavy == DevelopmentIds.Momentum;
        public int BuildRevision => State?.BuildRevision ?? 0;
        public bool CanDevelop => actions != null && !actions.Busy && !actions.MovementBlocked
            && (boundary == null || boundary.AllowsDevelopment)
            && Vector3.Distance(transform.position, spawn) < developmentRadius && SafeOpponents();
        public void Configure(AttackDefinition original, AttackDefinition planted, AttackDefinition advancing, EnemySandboxHUD hud)
        { baseline = original; breaker = planted; momentum = advancing; encounter = hud; }
        void Awake()
        {
            actions = GetComponent<CombatActions>(); discovery = GetComponent<ContextualDiscovery>(); trial = GetComponent<RetaliatoryMomentumTrial>();
            mouseLook = GetComponent<PlaygroundMouseLook>(); playerInput = GetComponent<PlayerInput>();
            boundary = GetComponent<ICharacterDevelopmentBoundary>();
            State = new CharacterDevelopmentState(initialFixturePoints); spawn = transform.position;
            RememberCheckpoint(null);
            if (initialFixturePoints == 0) Message = "Fresh Warrior: earn a mission milestone point before making a deliberate investment.";
            Review = new CombatActionReview(actions, transform);
            Experience = new HeavyExperienceCollector(actions, GetComponent<CombatVitality>(), () => State, ExperienceProfile);
            saves = new CharacterSaveStore(System.IO.Path.Combine(Application.persistentDataPath, saveFolder));
        }
        void Start()
        {
            if (baseline == null || breaker == null || momentum == null || encounter == null) { enabled = false; Debug.LogError("Owned-character authoring references are incomplete.", this); return; }
            discovery.Ledger.Changed -= OnKnowledge; discovery.Ledger.Changed += OnKnowledge; encounter.SetInterfaceVisible(false); Apply();
        }
        void OnEnable()
        {
            Review?.Attach();
            Experience?.Attach();
            inspect.Enable();
            if (discovery != null && discovery.Ledger != null) { discovery.Ledger.Changed -= OnKnowledge; discovery.Ledger.Changed += OnKnowledge; OnKnowledge(); }
        }
        void OnDisable() { inspect.Disable(); CloseMenu(); Review?.Dispose(); Experience?.Dispose(); if (discovery != null && discovery.Ledger != null) discovery.Ledger.Changed -= OnKnowledge; }
        string ExperienceProfile(AttackDefinition definition) => definition == baseline ? DevelopmentIds.Baseline
            : definition == breaker ? DevelopmentIds.Breaker : definition == momentum ? DevelopmentIds.Momentum : null;
        // Inspection of the authored technique only: no resolvers, acceptance or opportunity consumption.
        public bool TryPreviewHeavy(string id, out AttackSnapshot preview)
        {
            var definition = id == DevelopmentIds.Baseline ? baseline : id == DevelopmentIds.Breaker ? breaker
                : id == DevelopmentIds.Momentum ? momentum : null;
            preview = definition != null ? new AttackSnapshot(definition) : default;
            return definition != null;
        }
        void OnKnowledge() => State.RecordKnowledge(discovery.Ledger.Exchanges);
        bool SafeOpponents()
        {
            foreach (var enemy in FindObjectsByType<SandboxEnemy>(FindObjectsSortMode.None))
                if (enemy.gameObject.scene == gameObject.scene && enemy.isActiveAndEnabled && enemy.Phase != EnemyPhase.Idle && enemy.Phase != EnemyPhase.Defeated) return false;
            return true;
        }
        public bool Purchase(string id, int revision)
        {
            if (!CanDevelop) { Message = "Return to the hub or safe spawn and finish recovery before developing."; return false; }
            bool success = State.Purchase(id, revision, out string message); Message = message; if (success) Apply(); return success;
        }
        public bool Purchase(DevelopmentPurchasePreview preview)
        {
            if (!CanDevelop) { Message = "Return to the hub or safe spawn and finish recovery before developing."; return false; }
            bool success = State.Purchase(preview, out string message); Message = message; if (success) Apply(); return success;
        }
        public bool ConfigureHeavy(string id, bool retaliation)
        {
            if (!CanDevelop) { Message = "Configure only at the safe spawn while ready."; return false; }
            bool success = State.Configure(id, retaliation, out string message); Message = message; if (success) Apply(); return success;
        }
        public bool FreshCharacter()
        {
            if (!CanDevelop) { Message = "Return to the hub or safe spawn before creating a fresh character."; return false; }
            State = new CharacterDevelopmentState(initialFixturePoints); discovery.RestoreCharacterKnowledge(0); Apply();
            RememberCheckpoint(null);
            Review.Clear();
            Experience.ReplaceCharacter();
            Message = "Fresh character: no knowledge, purchases or active evolution. Unsaved previous character was replaced.";
            CharacterRecordReplaced?.Invoke(); return true;
        }
        #if UNITY_EDITOR
        public void UseTestSaveDirectory(string path) => saves = new CharacterSaveStore(path);
        #else
        internal void UseTestSaveDirectory(string path)
        {
            if (!Debug.isDebugBuild) throw new System.InvalidOperationException("Packaged test saves require a development build.");
            saves = new CharacterSaveStore(path);
        }
        #endif
        public bool SaveSlot(string slot)
        {
            if (!CanDevelop) { Message = "Save only at safe spawn while ready."; return false; }
            bool success = saves.Save(slot, State, out string message); Message = message;
            if (success) RememberCheckpoint(slot);
            return success;
        }
        public CharacterSaveStore.SlotPreview InspectSaveSlot(string slot) => saves.Inspect(slot);
        public bool LoadSlot(string slot)
        {
            if (!CanDevelop) { Message = "Load only at safe spawn while ready."; return false; }
            if (!saves.Load(slot, out var loaded, out string message)) { Message = message; return false; }
            // Validate first; only then replace live state and reset the whole encounter.
            encounter.ResetEncounter(); State = loaded;
            Review.Clear();
            Experience.ReplaceCharacter();
            discovery.RestoreCharacterKnowledge(State.Exchanges); Apply(); Message = message;
            RememberCheckpoint(slot);
            CharacterRecordReplaced?.Invoke(); return true;
        }
        void RememberCheckpoint(string slot)
        {
            checkpointSlot = slot;
            checkpointRevision = State.Revision;
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
                else TryOpenMenu();
            }
        }
        public bool TryOpenMenu()
        {
            if (menu) return true;
            if (!CanDevelop) { Message = "Inspect at the hub or safe spawn while ready. Combat continues normally."; return false; }
            previousMouseLook = mouseLook != null && mouseLook.enabled; previousInput = playerInput != null && playerInput.inputIsActive;
            if (mouseLook != null) mouseLook.enabled = false;
            if (playerInput != null) playerInput.DeactivateInput();
            var input = GetComponent<StarterAssetsInputs>(); input.move = input.look = Vector2.zero; input.jump = input.sprint = false;
            menu = true; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; return true;
        }
        public void CloseMenu()
        {
            if (!menu) return;
            menu = false;
            if (mouseLook != null) mouseLook.enabled = previousMouseLook;
            if (playerInput != null && previousInput && playerInput.isActiveAndEnabled)
            {
                // During scene/Play teardown PlayerInput may already have unregistered itself.
                // Never reactivate an input component after its own OnDisable has run.
                foreach (var activePlayer in PlayerInput.all)
                    if (activePlayer == playerInput) { playerInput.ActivateInput(); break; }
            }
            // Keep cursor released; the next deliberate Game-view click captures it without attacking.
        }
    }
}
