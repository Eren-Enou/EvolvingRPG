using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace EvolvingRPG.Expeditions
{
    // One live Warrior; hub and mission are authored zones in one scene. No global session singleton.
    [RequireComponent(typeof(OwnedCharacter))]
    public sealed class ExpeditionLoop : MonoBehaviour
    {
        [SerializeField] OwnedCharacter character;
        [SerializeField] EnemySandboxHUD encounter;
        [SerializeField] SandboxEnemy guarded, fast;
        [SerializeField] Transform board, development, saveStation, relay, arrival;
        [SerializeField, Min(.5f)] float interactionRadius = 3f;
        readonly InputAction interact = new InputAction("ExpeditionInteract", InputActionType.Button, "<Keyboard>/f");
        CombatActions actions;
        CombatVitality vitality;
        public ExpeditionAttempt Attempt { get; private set; }
        public bool InMission => Attempt != null && Attempt.Phase == ExpeditionPhase.InProgress;
        public bool BoardOpen { get; private set; }
        public string Report { get; private set; } = "Fresh Warrior: restore the relay to earn your first development point. No automatic investment.";
        public OwnedCharacter Character => character;
        string observedCharacter;
        Vector2 scroll;
        public void Configure(OwnedCharacter owner, EnemySandboxHUD hud, SandboxEnemy guard, SandboxEnemy mobile,
            Transform missionBoard, Transform developer, Transform saves, Transform objective, Transform entry)
        { character = owner; encounter = hud; guarded = guard; fast = mobile; board = missionBoard; development = developer; saveStation = saves; relay = objective; arrival = entry; }
        void Awake()
        {
            if(character == null) character = GetComponent<OwnedCharacter>();
            actions = character.GetComponent<CombatActions>(); vitality = character.GetComponent<CombatVitality>();
        }
        void OnEnable() => interact.Enable();
        void OnDisable() => interact.Disable();
        void OnDestroy() => interact.Dispose();
        void Start()
        {
            if(encounter == null || guarded == null || fast == null || board == null || development == null
                || saveStation == null || relay == null || arrival == null)
            { Debug.LogError("Relay loop authoring references are incomplete. Use the explicit Relay Outpost builder.",this); enabled=false; return; }
            encounter.AllowEncounterReset = false;
            guarded.gameObject.SetActive(false); fast.gameObject.SetActive(false);
            observedCharacter = character.State.CharacterId;
        }
        bool Near(Transform station) => station != null && Vector3.Distance(character.transform.position, station.position) < interactionRadius;
        void Update()
        {
            if (observedCharacter != character.State.CharacterId)
            { observedCharacter = character.State.CharacterId; Attempt = null; Report = "Different Warrior loaded or created. No mission attempt or temporary opportunity inherited."; }
            if (!character.MenuOpen) BoardOpen = false;
            if (!InMission && (vitality.Defeated || character.transform.position.y < -5))
            {
                character.CloseMenu(); BoardOpen = false; ResetToHub();
                Report = "Recovered to the hub. No mission completion or reward was recorded.";
                return;
            }
            if (InMission)
            {
                if (vitality.Defeated || character.transform.position.y < -5) { Return(ExpeditionPhase.Failed); return; }
                if (guarded.GetComponent<EnemyVitality>().Defeated) Attempt.Defeat(Attempt.Id, "guarded");
                if (fast.GetComponent<EnemyVitality>().Defeated) Attempt.Defeat(Attempt.Id, "fast");
            }
            if (!Application.isFocused || !interact.WasPressedThisFrame() || character.MenuOpen || actions.Busy || vitality.Staggered) return;
            if (InMission)
            {
                if (Near(relay) && Attempt.ObjectivesReady) Return(ExpeditionPhase.Completed);
                else if (Near(arrival)) Return(ExpeditionPhase.Abandoned);
            }
            else if (Near(board)) OpenBoard();
            else if (Near(development) || Near(saveStation)) character.TryOpenMenu();
        }
        public bool OpenBoard()
        {
            if (InMission || !character.CanDevelop || !character.TryOpenMenu()) return false;
            BoardOpen = true; return true;
        }
        public bool Deploy(string contract)
        {
            if (InMission || !character.CanDevelop || character.State.HasUnresolvedContent) return false;
            if (contract != ExpeditionAttempt.Relay && contract != ExpeditionAttempt.Priority) return false;
            character.CloseMenu(); BoardOpen = false;
            ResetToHub();
            guarded.gameObject.SetActive(true); fast.gameObject.SetActive(true);
            guarded.ResetEnemy(); fast.ResetEnemy();
            Attempt = new ExpeditionAttempt(character.State.CharacterId, contract);
            if (!Attempt.Deploy(Attempt.Id)) return false;
            Teleport(arrival.position);
            Report = "Attempt " + Attempt.Id.Substring(0, 8) + ": no mid-mission save. Defeat/abandon gives no completion point; completed experience remains in memory until explicit hub save.";
            return true;
        }
        public bool Return(ExpeditionPhase outcome)
        {
            if (!InMission || outcome == ExpeditionPhase.Completed && (!Attempt.ObjectivesReady || !Near(relay))) return false;
            if (outcome != ExpeditionPhase.Completed && outcome != ExpeditionPhase.Failed && outcome != ExpeditionPhase.Abandoned) return false;
            if (!Attempt.Finish(Attempt.Id, outcome)) return false;
            actions.Interrupt("Expedition " + outcome);
            ResetToHub();
            guarded.gameObject.SetActive(false); fast.gameObject.SetActive(false);
            bool settled = Attempt.Settle(Attempt.Id, character.State);
            Report = "Attempt " + Attempt.Id.Substring(0, 8) + " / " + outcome + (settled ? " / settled" : " / settlement refused")
                + (Attempt.PointAwarded ? ". First relay milestone: +1 development point. Choose Breaker or Momentum deliberately." : ". No development point: only first relay restoration pays; repeat contracts retain combat experience/discovery, not infinite currency.")
                + " Save explicitly at the hub. Unsaved progress is lost when loading an older record or exiting.";
            OpenBoard(); return settled;
        }
        void Teleport(Vector3 destination)
        {
            Vector3 before = character.transform.position;
            var body = character.GetComponent<CharacterController>(); body.enabled = false;
            character.transform.SetPositionAndRotation(destination, Quaternion.identity); body.enabled = true;
            character.GetComponent<StarterAssets.ThirdPersonController>().Grounded = true; Physics.SyncTransforms();
            WarpCamera(character.transform.position - before);
        }
        void ResetToHub()
        {
            Vector3 before = character.transform.position;
            encounter.ResetEncounter();
            WarpCamera(character.transform.position - before);
        }
        void WarpCamera(Vector3 delta)
        {
            var target = character.GetComponent<StarterAssets.ThirdPersonController>().CinemachineCameraTarget.transform;
            foreach (var camera in FindObjectsByType<CinemachineCamera>())
                if (camera.gameObject.scene == gameObject.scene) camera.OnTargetObjectWarped(target, delta);
        }
        void OnGUI()
        {
            if (character == null || character.State == null || character.MenuOpen && !BoardOpen) return;
            bool wrap = GUI.skin.label.wordWrap; GUI.skin.label.wordWrap = true;
            try
            {
                float width = Mathf.Min(730, Screen.width - 24);
                if (BoardOpen && character.MenuOpen)
                {
                    var old = GUI.color; GUI.color = new Color(.07f,.09f,.12f,1); GUI.DrawTexture(new Rect(12,12,width,Screen.height-24),Texture2D.whiteTexture); GUI.color=old;
                    GUILayout.BeginArea(new Rect(12,12,width,Screen.height-24), GUI.skin.box);
                    scroll=GUILayout.BeginScrollView(scroll);
                    GUILayout.Label("RELAY OUTPOST / Warrior " + character.State.CharacterId.Substring(0,8) + " / points " + character.State.Balance);
                    GUILayout.Label(Report);
                    GUILayout.Label("ABANDONED RELAY: defeat the guarded and fast defenders, then press F at the blue relay. First completion awards one point per Warrior; replays award none. Defeat returns here with a retry option.");
                    if (GUILayout.Button("Accept and deploy: restore relay")) Deploy(ExpeditionAttempt.Relay);
                    GUILayout.Label("PRIORITY INTERDICTION: eliminate the fast opponent and reach the relay; the guarded fighter may be bypassed. No point reward. Same enemy tuning, different objective pressure.");
                    if (GUILayout.Button("Accept and deploy: priority contract")) Deploy(ExpeditionAttempt.Priority);
                    if (Attempt != null && GUILayout.Button("Retry previous contract as a NEW attempt")) Deploy(Attempt.Contract);
                    if (GUILayout.Button("Inspect character / invest / explicit A-B save and load")) BoardOpen=false;
                    if (GUILayout.Button("Close board / walk in hub")) { character.CloseMenu(); BoardOpen=false; }
                    GUILayout.Label("WASD/Q/E move, Space jump, mouse/wheel camera, LMB/RMB attack, Shift dodge. F interacts with stations. Tab inspects only near hub spawn. R reset is disabled here; use explicit retry/abandon. No automatic save or load.");
                    GUILayout.EndScrollView(); GUILayout.EndArea();
                }
                else
                {
                    GUILayout.BeginArea(new Rect(12,12,width,210), GUI.skin.box);
                    GUILayout.Label(InMission ? (Attempt.Contract==ExpeditionAttempt.Relay ? "EXPEDITION / Restore the relay" : "EXPEDITION / Priority interdiction") : "RELAY OUTPOST / HUB");
                    GUILayout.Label("HP " + vitality.Health.ToString("0") + " / " + actions.Phase + " / " + character.State.Heavy + " / points " + character.State.Balance);
                    if(InMission) GUILayout.Label("Defenders: guarded " + DefenderStatus(guarded) + " / fast " + DefenderStatus(fast));
                    GUILayout.Label(InMission ? (Attempt.ObjectivesReady ? "Objective ready: reach the blue relay and press F." : Attempt.Contract==ExpeditionAttempt.Relay ? "Defeat both defenders. Then activate the blue relay." : "Eliminate the fast fighter, then reach the blue relay; guarded fighter is optional.") : "F near the gold mission board deploys. F near green development or white save stations inspects your Warrior. Tab also opens development near spawn.");
                    GUILayout.Label(InMission ? "F at the arrival beacon abandons without reward. Defeat returns to hub. R reset disabled. No mid-mission save." : Report);
                    GUILayout.Label(character.Experience.Latest);
                    GUILayout.EndArea();
                }
            }
            finally { GUI.skin.label.wordWrap=wrap; }
        }
        static string DefenderStatus(SandboxEnemy enemy)
        {
            var hp = enemy.GetComponent<EnemyVitality>();
            return hp.Defeated ? "cleared" : hp.Health.ToString("0") + " HP / " + enemy.Phase;
        }
    }
}
