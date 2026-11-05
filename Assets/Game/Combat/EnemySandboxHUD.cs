using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    public sealed class EnemySandboxHUD : MonoBehaviour
    {
        [SerializeField] private CombatActions player;
        [SerializeField] private SandboxEnemy[] enemies;
        [SerializeField] private bool showOpponentDetails = true;
        private readonly InputAction reset = new InputAction("ResetEncounter", InputActionType.Button, "<Keyboard>/r");
        private readonly InputAction clear = new InputAction("ClearObservations", InputActionType.Button, "<Keyboard>/f6");
        private readonly InputAction details = new InputAction("OpponentDetails", InputActionType.Button, "<Keyboard>/f1");
        private HeavyTechniqueSelector techniques;
        private HeavyPractice practice;
        private CombatVitality vitality;
        private CombatStamina stamina;
        private Vector3 spawn;
        private Quaternion facing;
        private GUIStyle text;
        [SerializeField] private bool showInterface = true;
        public bool InterfaceVisible => showInterface;
        public void SetInterfaceVisible(bool visible) => showInterface = visible;
        public void Configure(CombatActions actor, SandboxEnemy[] opponents) { player = actor; enemies = opponents; }
        public void SetOpponentDetails(bool visible) => showOpponentDetails = visible;
        private void OnEnable() { reset.Enable(); clear.Enable(); details.Enable(); }
        private void OnDisable() { reset.Disable(); clear.Disable(); details.Disable(); }
        private void Start()
        {
            vitality = player.GetComponent<CombatVitality>(); stamina = player.GetComponent<CombatStamina>();
            techniques = player.GetComponent<HeavyTechniqueSelector>();
            practice = player.GetComponent<HeavyPractice>();
            spawn = player.transform.position; facing = player.transform.rotation;
        }
        private void Update()
        {
            if (!Application.isFocused || Cursor.lockState != CursorLockMode.Locked) return;
            if (reset.WasPressedThisFrame()) ResetEncounter();
            if (clear.WasPressedThisFrame()) player.Diagnostics.Clear();
            if (details.WasPressedThisFrame()) showOpponentDetails = !showOpponentDetails;
        }
        public void ResetEncounter()
        {
            var motor = player.GetComponent<CharacterController>();
            motor.enabled = false; player.transform.SetPositionAndRotation(spawn, facing); motor.enabled = true;
            player.GetComponent<ThirdPersonController>().Grounded = true;
            vitality.ResetVitality(); player.Interrupt("Encounter reset"); player.Diagnostics.Clear();
            foreach (var enemy in enemies) enemy.ResetEnemy();
            Physics.SyncTransforms();
        }
        private void OnGUI()
        {
            if (!showInterface || player == null || vitality == null || enemies == null) return;
            if (text == null) { text = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true }; text.normal.textColor = Color.white; }
            float width = Mathf.Min(760, Screen.width - 24);
            float panelHeight = showOpponentDetails ? (techniques != null ? 325 : 265) : (techniques != null ? 180 : 125);
            if (practice != null) panelHeight += 42;
            GUILayout.BeginArea(new Rect(12, 12, width, panelHeight), GUI.skin.box);
            GUILayout.Label(practice != null ? "EVOLVING RPG / HEAVY PRACTICE TRIAL" : techniques != null ? "EVOLVING RPG / HEAVY TECHNIQUE EXPERIMENT" : "EVOLVING RPG / FIRST OPPONENTS", text);
            GUILayout.Label("LMB Light | RMB Heavy | Shift Dodge | Space Jump | R Reset encounter | F6 Clear log", text);
            GUILayout.Label($"Player HP {vitality.Health:0}/{vitality.MaximumHealth:0} | Poise {vitality.Poise:0}/{vitality.PoiseThreshold:0} | Stamina {stamina.Current:0} (unchanged tuning)", text);
            GUILayout.Label($"{player.Phase} | {player.Message} | {vitality.LastHit}", text);
            if (practice != null) GUILayout.Label($"Heavy practice {practice.Points}/{practice.TrialThreshold} | Qualified actions {practice.QualifiedActions} | F2 recovery trial: {(practice.TrialEnabled ? "ON" : "OFF")} / {(practice.TrialReady ? "ready" : "not ready")}", text);
            if (techniques != null)
            {
                GUILayout.Label($"1 Baseline | 2 Breaker | 3 Momentum | Selected: {techniques.SelectedDefinition?.DisplayName ?? "Missing definition"}", text);
                GUILayout.Label(techniques.Message, text);
            }
            GUILayout.Label("F1 toggles opponent details. Yellow windup / Red strike / Green opening / Purple stagger", text);
            if (showOpponentDetails) foreach (var enemy in enemies)
            {
                var health = enemy.GetComponent<EnemyVitality>();
                GUILayout.Label($"{enemy.name}: {enemy.Phase} | HP {health.Health:0}/{health.MaximumHealth:0} | Poise {health.Poise:0}/{health.PoiseThreshold:0} | {enemy.Pattern}", text);
                GUILayout.Label(health.LastHit + " | " + enemy.LastAttack, text);
            }
            if (showOpponentDetails) GUILayout.Label("Approach one lane at a time. No dodge invulnerability.", text);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(12, Mathf.Max(panelHeight + 25, Screen.height - 85), width, 75), GUI.skin.box);
            GUILayout.Label(practice != null ? "Session-only practice / R starts fresh / no unlocks or saves" : "Encounter diagnostics only / no mastery or unlocks / Escape releases cursor", text);
            GUILayout.Label(practice != null ? practice.Latest : player.Diagnostics.Latest, text);
            GUILayout.EndArea();
        }
    }
}
