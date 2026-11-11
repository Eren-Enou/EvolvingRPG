using System;
using System.Collections.Generic;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace EvolvingRPG.Expeditions
{
    // One live Warrior; hub and mission are authored zones in one scene. No global session singleton.
    [RequireComponent(typeof(OwnedCharacter))]
    public sealed class ExpeditionLoop : MonoBehaviour, ICharacterDevelopmentBoundary
    {
        [SerializeField] OwnedCharacter character;
        [SerializeField] EnemySandboxHUD encounter;
        [SerializeField] SandboxEnemy guarded, fast;
        [SerializeField] Transform board, development, saveStation, relay, arrival;
        [SerializeField, Min(.5f)] float interactionRadius = 3f;
        // Leave room to read the objective before entering the guard's 8 m awareness radius.
        [SerializeField] Vector3 priorityGuardPost = new Vector3(0, .05f, 2);
        [SerializeField] Vector3 priorityFastPost = new Vector3(7, .05f, 6);
        readonly InputAction interact = new InputAction("ExpeditionInteract", InputActionType.Button, "<Keyboard>/f");
        CombatActions actions;
        CombatVitality vitality;
        CombatStamina stamina;
        ContextualDiscovery discovery;
        RetaliatoryMomentumTrial retaliation;
        EnemyVitality guardHealth, fastHealth;
        Vector3 relayGuardPost, relayFastPost;
        public ExpeditionAttempt Attempt { get; private set; }
        public bool InMission => Attempt != null && Attempt.Phase == ExpeditionPhase.InProgress;
        public bool AllowsDevelopment => !InMission;
        public bool BoardOpen { get; private set; }
        public string Report { get; private set; } = "Restore the relay to earn your first development point.";
        public OwnedCharacter Character => character;
        string[] experienceAtDeployment;
        int exchangesAtDeployment;
        float departedAt;
        Vector2 scroll;
        bool showDiagnostics;
        CombatActionReview.Entry lastAttackBeforeDeployment;
        GUIStyle title, copy;
        enum Station { None, Board, Development, Save, Arrival, Relay }
        public void Configure(OwnedCharacter owner, EnemySandboxHUD hud, SandboxEnemy guard, SandboxEnemy mobile,
            Transform missionBoard, Transform developer, Transform saves, Transform objective, Transform entry)
        { character = owner; encounter = hud; guarded = guard; fast = mobile; board = missionBoard; development = developer; saveStation = saves; relay = objective; arrival = entry; }
        void Awake()
        {
            if(character == null) character = GetComponent<OwnedCharacter>();
            actions = character.GetComponent<CombatActions>(); vitality = character.GetComponent<CombatVitality>();
            stamina = character.GetComponent<CombatStamina>();
            discovery = character.GetComponent<ContextualDiscovery>();
            retaliation = character.GetComponent<RetaliatoryMomentumTrial>();
        }
        void OnEnable() { interact.Enable(); character.CharacterRecordReplaced += RecordReplaced; }
        void OnDisable() { interact.Disable(); character.CharacterRecordReplaced -= RecordReplaced; }
        void RecordReplaced()
        { Attempt = null; Report = "Warrior ready. Choose an expedition at the mission board."; }
        void OnDestroy() => interact.Dispose();
        void Start()
        {
            if(encounter == null || guarded == null || fast == null || board == null || development == null
                || saveStation == null || relay == null || arrival == null)
            { Debug.LogError("Relay loop authoring references are incomplete. Use the explicit Relay Outpost builder.",this); enabled=false; return; }
            guardHealth = guarded.GetComponent<EnemyVitality>(); fastHealth = fast.GetComponent<EnemyVitality>();
            relayGuardPost = guarded.transform.position; relayFastPost = fast.transform.position;
            encounter.AllowEncounterReset = false;
            guarded.gameObject.SetActive(false); fast.gameObject.SetActive(false);
        }
        bool Near(Transform station) => station != null && Vector3.Distance(character.transform.position, station.position) < interactionRadius;
        Station NearbyStation => InMission ? (Attempt.ObjectivesReady && Near(relay) ? Station.Relay : Near(arrival) ? Station.Arrival
            : Near(relay) ? Station.Relay : Station.None)
            : Near(board) ? Station.Board : Near(development) ? Station.Development : Near(saveStation) ? Station.Save : Station.None;
        void Update()
        {
            if (!character.MenuOpen) BoardOpen = false;
            if (!InMission && (vitality.Defeated || character.transform.position.y < -5))
            {
                character.CloseMenu(); BoardOpen = false; ResetToHub();
                Report = "Recovered to the hub. Your Warrior is ready to deploy.";
                return;
            }
            if (InMission)
            {
                if (vitality.Defeated || character.transform.position.y < -5) { Return(ExpeditionPhase.Failed); return; }
                if (guardHealth.Defeated) Attempt.Defeat(Attempt.Id, "guarded");
                if (fastHealth.Defeated) Attempt.Defeat(Attempt.Id, "fast");
            }
            if (!Application.isFocused || !interact.WasPressedThisFrame() || character.MenuOpen || actions.Busy || vitality.Staggered) return;
            switch (NearbyStation)
            {
                case Station.Relay: if (Attempt.ObjectivesReady) Return(ExpeditionPhase.Completed); break;
                case Station.Arrival: Return(ExpeditionPhase.Abandoned); break;
                case Station.Board: OpenBoard(); break;
                case Station.Development:
                case Station.Save: character.TryOpenMenu(); break;
            }
        }
        public bool OpenBoard()
        {
            if (InMission || !character.CanDevelop || !character.TryOpenMenu()) return false;
            BoardOpen = true; scroll = Vector2.zero; return true;
        }
        public bool Deploy(string contract)
        {
            if (InMission || !character.CanDevelop || character.State.HasUnresolvedContent) return false;
            if (contract != ExpeditionAttempt.Relay && contract != ExpeditionAttempt.Priority) return false;
            character.CloseMenu(); BoardOpen = false;
            ResetToHub();
            guarded.gameObject.SetActive(true); fast.gameObject.SetActive(true);
            bool priority = contract == ExpeditionAttempt.Priority;
            guarded.ResetEnemyAt(priority ? priorityGuardPost : relayGuardPost);
            fast.ResetEnemyAt(priority ? priorityFastPost : relayFastPost);
            Attempt = new ExpeditionAttempt(character.State.CharacterId, contract);
            if (!Attempt.Deploy(Attempt.Id)) return false;
            experienceAtDeployment = character.State.ExperienceFacts;
            exchangesAtDeployment = character.State.Exchanges;
            departedAt = Time.time;
            lastAttackBeforeDeployment = character.Review.Latest;
            Teleport(arrival.position);
            return true;
        }
        public bool Return(ExpeditionPhase outcome)
        {
            if (!InMission || outcome == ExpeditionPhase.Completed && (!Attempt.ObjectivesReady || !Near(relay))) return false;
            if (outcome != ExpeditionPhase.Completed && outcome != ExpeditionPhase.Failed && outcome != ExpeditionPhase.Abandoned) return false;
            if (!Attempt.Finish(Attempt.Id, outcome)) return false;
            // Snapshot retained facts before encounter reset changes session-only diagnostic text.
            int exchanges = Mathf.Max(0, character.State.Exchanges - exchangesAtDeployment);
            var newFacts = new List<string>();
            string profile = character.State.Heavy;
            foreach (string fact in HeavyExperienceCatalog.Outcomes)
            {
                if (character.State.HasExperience(profile, fact) && Array.IndexOf(experienceAtDeployment, HeavyExperienceCatalog.Id(profile, fact)) < 0)
                    newFacts.Add(HeavyExperienceCatalog.Label(fact));
            }
            int elapsed = Mathf.Max(0, Mathf.RoundToInt(Time.time - departedAt));
            bool playerDefeated = vitality.Defeated;
            bool guardDefeated = guardHealth.Defeated;
            actions.Interrupt("Expedition " + outcome);
            ResetToHub();
            guarded.gameObject.SetActive(false); fast.gameObject.SetActive(false);
            bool settled = Attempt.Settle(Attempt.Id, character.State);
            string result = outcome == ExpeditionPhase.Failed ? (playerDefeated ? "Defeated. You recovered at the outpost." : "You fell outside the expedition and recovered at the outpost.")
                : outcome == ExpeditionPhase.Abandoned ? "Expedition abandoned. You returned safely."
                : Attempt.Contract == ExpeditionAttempt.Priority ? "Priority fighter eliminated. Relay secured. "
                    + (guardDefeated ? "You also cleared the center guard." : "You left the center guard standing.")
                : "Both defenders defeated. Relay restored.";
            string reward = !settled ? "Reward could not be recorded for this Warrior."
                : Attempt.PointAwarded ? "+1 development point. Inspect your Warrior to choose Breaker or Momentum."
                : outcome != ExpeditionPhase.Completed ? "No completion reward. You can retry from the board."
                : Attempt.Contract == ExpeditionAttempt.Relay ? "First restoration reward already earned." : "No completion point for this contract.";
            string learning = newFacts.Count == 0 ? "No new Heavy experience this trip." : "New Heavy experience: " + string.Join(", ", newFacts) + ".";
            if (exchanges > 0) learning += character.State.Knowledge == DiscoveryKnowledge.Revealed
                ? " Your Warrior discovered a new Momentum application. Inspect development to learn it."
                : " Your evasive movement is starting to shape your advancing strikes.";
            Report = $"{result}\n{reward}\n{elapsed}s in the field · {learning}";
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
            if (Screen.width < 80 || Screen.height < 80) return;
            JourneyGui.Begin();
            copy ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            title ??= new GUIStyle(copy) { fontStyle = FontStyle.Bold, fontSize = 16 };
            if (BoardOpen && character.MenuOpen) DrawBoard();
            else DrawHud();
        }

        void DrawBoard()
        {
            var bounds = new Rect(12, 12, Mathf.Min(500, Screen.width - 24), Screen.height - 24);
            var color = GUI.color;
            GUI.color = new Color(.07f, .09f, .12f, 1);
            GUI.DrawTexture(bounds, Texture2D.whiteTexture);
            GUI.color = color;
            GUILayout.BeginArea(bounds, GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("Relay Outpost · Mission Board", title);
            GUILayout.Label($"{DevelopmentIds.Label(character.State.Heavy)} · {character.State.Balance} development points", copy);
            GUILayout.Space(8);
            GUILayout.Label(Report, copy);
            if (Attempt?.PointAwarded == true && character.State.Balance > 0)
            {
                if (JourneyGui.Button("Choose your specialization")) BoardOpen = false;
            }
            else if (Attempt != null && Attempt.Outcome != ExpeditionPhase.Completed)
            {
                if (JourneyGui.Button("Retry " + ContractTitle(Attempt.Contract))) Deploy(Attempt.Contract);
            }
            GUILayout.Space(8);
            GUILayout.Label("Restore Relay", title);
            GUILayout.Label("Defeat both defenders and activate the blue relay. " + (character.State.RelayMilestone
                ? "First restoration reward already earned." : "First clear earns one development point."), copy);
            if (JourneyGui.Button("Deploy: Restore Relay")) Deploy(ExpeditionAttempt.Relay);
            GUILayout.Space(8);
            GUILayout.Label("Priority Interdiction", title);
            GUILayout.Label("The guard holds the center lane. Take the wider right lane to try to isolate the fast fighter, or clear the guard first. Defeat the fast fighter and activate the relay. No completion point.", copy);
            if (JourneyGui.Button("Deploy: Priority Interdiction")) Deploy(ExpeditionAttempt.Priority);
            GUILayout.Space(8);
            string practice = NextPractice();
            if (practice != null) GUILayout.Label(practice, copy);
            if (JourneyGui.Button("Inspect Warrior / Save and Load")) BoardOpen = false;
            if (GUILayout.Button("Return to the outpost")) { character.CloseMenu(); BoardOpen = false; }
            GUILayout.Label("Save to keep your progress between sessions. Loading an older record replaces unsaved progress.", copy);
            showDiagnostics = GUILayout.Toggle(showDiagnostics, "Show combat diagnostics in the field");
            GUILayout.Label("WASD move · Q/E strafe · Space jump · Mouse look / wheel zoom\nLMB Light · RMB Heavy · Shift Dodge · F Interact · Tab Inspect", copy);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        string NextPractice()
        {
            var state = character.State;
            if (state.Heavy == DevelopmentIds.Breaker)
                return state.HasExperience(DevelopmentIds.Breaker, HeavyExperienceCatalog.GuardBreak)
                    ? "Breaker practice: try the center route. Use Light or step away when the opening is too short for a planted Heavy."
                    : "Breaker practice: look for a safe opening to commit your planted Heavy against the guard.";
            if (state.Heavy != DevelopmentIds.Momentum) return null;
            if (state.RetaliationActive) return "Momentum practice: use the longer return when the lane is clear. Keep ordinary Momentum or reposition when the lunge would leave you exposed.";
            if (state.RetaliationLearned) return "Your learned return is inactive. Inspect your Warrior to equip it for the next expedition.";
            if (state.Knowledge == DiscoveryKnowledge.Revealed) return "A new Momentum application is available to learn in development.";
            return "Momentum practice: face one fighter at a time. Evade a committed strike, land your advancing Heavy on the same fighter, and finish recovery.";
        }

        void DrawHud()
        {
            GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(430, Screen.width - 24), Screen.height - 24));
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(InMission ? ContractTitle(Attempt.Contract) : "Relay Outpost", title);
            if (InMission)
            {
                GUILayout.Label(Attempt.ObjectivesReady ? "Activate the blue relay. Go around the center barrier."
                    : Attempt.Contract == ExpeditionAttempt.Relay ? "Defeat both defenders." : "Defeat the fast fighter. Guarded fighter optional.", copy);
                GUILayout.Label("Guarded: " + DefenderCue(guarded, guardHealth) + "   ·   Fast: " + DefenderCue(fast, fastHealth), copy);
                GUILayout.Label($"Health {vitality.Health:0}/{vitality.MaximumHealth:0}   ·   Stamina {stamina.Current:0}/{stamina.Maximum:0}", copy);
                GUILayout.Label(ActionCue(), copy);
                if (retaliation != null && retaliation.OpportunityReady)
                {
                    string target = retaliation.Opportunity.Value.Source.GuardedArchetype ? "guarded fighter" : "fast fighter";
                    GUILayout.Label($"Return opening: {target} ({retaliation.SecondsRemaining:0.0}s)\nForward + RMB: longer lunge. RMB without forward input: ordinary Momentum. Miss this fighter and recovery lasts longer.", copy);
                }
            }
            GUILayout.Label(StationCue(), copy);
            if (showDiagnostics && InMission)
            {
                GUILayout.Space(6);
                GUILayout.Label($"Guarded HP {guardHealth.Health:0} / poise {guardHealth.Poise:0} · Fast HP {fastHealth.Health:0} / poise {fastHealth.Poise:0}", copy);
                GUILayout.Label(actions.Message, copy);
                GUILayout.Label(character.Experience.Latest, copy);
                if (character.State.Heavy == DevelopmentIds.Momentum && character.State.Knowledge != DiscoveryKnowledge.Revealed
                    && discovery?.Ledger != null)
                    GUILayout.Label($"Momentum discovery {character.State.Exchanges}/3 · {discovery.Ledger.Diagnostic}", copy);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        string StationCue()
        {
            var station = NearbyStation;
            if (station != Station.None && (actions.Busy || vitality.Staggered)) return "Finish recovering to interact.";
            if (!InMission && station != Station.None && !character.CanDevelop) return "Step toward the outpost center to interact.";
            switch (station)
            {
                case Station.Board: return "F  Open mission board";
                case Station.Development: return "F  Inspect development";
                case Station.Save: return "F  Save or load a Warrior";
                case Station.Arrival: return "F  Abandon and return to outpost";
                case Station.Relay: return Attempt.ObjectivesReady ? "F  Activate relay and return" : "Relay locked until the objective is complete.";
                default: return InMission ? "Arrival beacon: abandon if you need to leave." : "Gold: Missions   ·   Green: Development   ·   White: Save / Load";
            }
        }

        string ActionCue()
        {
            if (vitality.Staggered) return "Staggered · wait to regain control";
            if (actions.Busy)
            {
                string action = actions.HasCommittedAttack ? actions.CommittedAttack.DisplayName : "Dodge";
                return action + (actions.Phase == CombatPhase.Recovery || actions.Phase == CombatPhase.DodgeRecovery ? " · Recovering"
                    : actions.Phase == CombatPhase.Windup ? " · Committed" : " · Executing");
            }
            var last = character.Review.Latest;
            string ready = "Ready · RMB " + DevelopmentIds.Label(character.State.Heavy);
            if (last == null || last == lastAttackBeforeDeployment) return ready;
            string result = last.Interrupted ? last.Name + " interrupted" : last.PoiseBroken ? "Staggered the opponent"
                : last.Contacts == 0 ? last.Name + " missed: " + MeleeMissFeedback.Describe(last.MissReason)
                : last.Guarded ? "Hit a raised guard" : "Hit landed";
            return ready + "\n" + result;
        }

        static string ContractTitle(string contract) => contract == ExpeditionAttempt.Priority ? "Priority Interdiction" : "Restore Relay";
        static string DefenderCue(SandboxEnemy enemy, EnemyVitality hp)
        {
            if (hp.Defeated) return "Down";
            switch (enemy.Phase)
            {
                case EnemyPhase.Windup: return "Winding up";
                case EnemyPhase.Active: return "Striking";
                case EnemyPhase.Staggered: return "Staggered";
                case EnemyPhase.Recovery: return "Open";
                case EnemyPhase.Returning: return "Retreating";
                default: return enemy.CanGuard ? "Shield raised" : enemy.Phase == EnemyPhase.Approach ? "Closing" : "Waiting";
            }
        }
    }
}
