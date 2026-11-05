using UnityEngine;
using UnityEngine.InputSystem;
namespace EvolvingRPG.Combat
{
    public sealed class DiscoveryHUD : MonoBehaviour
    {
        [SerializeField] ContextualDiscovery discovery;
        GUIStyle text;
        bool expanded, previousInterface;
        EnemySandboxHUD combatHud;
        void Start()
        {
            foreach (var hud in FindObjectsByType<EnemySandboxHUD>(FindObjectsSortMode.None))
                if (hud.gameObject.scene == gameObject.scene) { combatHud = hud; previousInterface = hud.InterfaceVisible; hud.SetInterfaceVisible(false); break; }
        }
        readonly InputAction journal = new InputAction("DiscoveryJournal", InputActionType.Button, "<Keyboard>/f7");
        public void Configure(ContextualDiscovery value) => discovery = value;
        void OnEnable() => journal.Enable();
        void OnDisable() { journal.Disable(); if (combatHud != null) combatHud.SetInterfaceVisible(previousInterface); }
        void Update() { if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && journal.WasPressedThisFrame()) expanded = !expanded; }
        void OnGUI()
        {
            if (discovery == null || discovery.Ledger == null) return;
            if (text == null) { text = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true }; text.normal.textColor = Color.white; }
            var trial = discovery.GetComponent<RetaliatoryMomentumTrial>();
            // Keep combat state and journal in one compact panel; the old HUD still owns reset input.
            float x = 12;
            float width = Mathf.Min(760, Screen.width - 24);
            GUILayout.BeginArea(new Rect(x, 12, width, Mathf.Min(expanded ? 520 : trial != null ? 370 : 350, Screen.height - 24)), GUI.skin.box);
            GUILayout.Label("CONTEXTUAL DISCOVERY / session experiment / F7 journal details", text);
            {
                var actor = discovery.GetComponent<CombatActions>(); var health = discovery.GetComponent<CombatVitality>();
                GUILayout.Label("WASD / Q,E | Space jump | Shift dodge | LMB Light | RMB Heavy | 1/2/3 techniques | mouse look/zoom", text);
                GUILayout.Label($"HP {health.Health:0}/{health.MaximumHealth:0} | Stamina {discovery.GetComponent<CombatStamina>().Current:0} | {actor.Phase} | {actor.Message}", text);
            }
            GUILayout.Label("F4 Momentum investment fixture: " + discovery.MomentumInvestment + " | F5 evasive investment fixture: " + discovery.EvasiveInvestment, text);
            GUILayout.Label("Fixtures are experimental prerequisites, not purchases. Select 3 for Momentum separately. No Heavy Practice refinement.", text);
            GUILayout.Label("Knowledge: " + discovery.Ledger.Knowledge + " | validated exchanges: " + discovery.Ledger.Exchanges + " / reveal at 3 (debug) | learned: NO", text);
            GUILayout.Label(discovery.Ledger.Diagnostic, text);
            string latest = null;
            foreach (string entry in discovery.Ledger.Journal) { latest = entry; if (expanded) GUILayout.Label(entry, text); }
            if (!expanded && latest != null) GUILayout.Label(latest, text);
            if (trial != null)
            {
                GUILayout.Label("F2 Retaliatory trial (not purchased): " + trial.TrialEnabled + " | opportunity: " + trial.OpportunityReady + " / " + trial.SecondsRemaining.ToString("0.0") + " s", text);
                GUILayout.Label(trial.Diagnostic, text);
            }
            GUILayout.Label("R: encounter reset retains knowledge/fixtures. F8: Fresh Character clears them. All progress ends with Play mode.", text);
            GUILayout.EndArea();
        }
    }
}
