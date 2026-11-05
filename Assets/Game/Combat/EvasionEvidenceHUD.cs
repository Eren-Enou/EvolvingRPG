using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    public sealed class EvasionEvidenceHUD : MonoBehaviour
    {
        [SerializeField] private EvasionEvidence evidence;
        [SerializeField] private SandboxEnemy secondOpponent;
        private readonly InputAction overlap = new InputAction("EvidenceSecondOpponent", InputActionType.Button, "<Keyboard>/f3");
        private GUIStyle text;
        public string EvidenceTitle
        {
            get
            {
                var discovery = evidence != null ? evidence.GetComponent<ContextualDiscovery>() : null;
                return discovery != null && discovery.Development != null
                    ? "EVASION EVIDENCE / discovery and learned returns require verified exchanges"
                    : "C1 / EVASION EVIDENCE ONLY / no rewards or evolution effects";
            }
        }
        public void Configure(EvasionEvidence collector, SandboxEnemy optionalOpponent)
        { evidence = collector; secondOpponent = optionalOpponent; }
        private void OnEnable() => overlap.Enable();
        private void OnDisable() => overlap.Disable();
        private void Update()
        {
            if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && overlap.WasPressedThisFrame() && secondOpponent != null)
                secondOpponent.gameObject.SetActive(!secondOpponent.gameObject.activeSelf);
        }
        private void OnGUI()
        {
            if (evidence == null) return;
            if (text == null) { text = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true }; text.normal.textColor = Color.white; }
            int previousDepth = GUI.depth;
            GUI.depth = -10;
            GUILayout.BeginArea(new Rect(12, Mathf.Max(205, Screen.height - 270), Mathf.Min(760, Screen.width - 24), 180), GUI.skin.box);
            GUILayout.Label(EvidenceTitle, text);
            var threat = evidence.Pending?.Threat ?? evidence.LatestThreat;
            GUILayout.Label("Threat: " + (threat.HasValue ? threat.Value.Label : "none committed")
                + (evidence.LatestThreatResolution.HasValue ? " / " + evidence.LatestThreatResolution.Value.End : ""), text);
            if (evidence.Pending != null)
                GUILayout.Label($"Dodge #{evidence.Pending.Sequence}: collecting actual movement / {evidence.Pending.Samples} samples", text);
            if (evidence.Latest.HasValue)
            {
                var latest = evidence.Latest.Value;
                GUILayout.Label($"Dodge #{latest.DodgeSequence}: {latest.Classification} / {latest.Displacement.magnitude:0.00} m displacement / {latest.Samples} samples", text);
                GUILayout.Label(latest.Explanation, text);
            }
            else GUILayout.Label("Let the guarded opponent commit, then dodge early enough to leave its arc. No invulnerability.", text);
            GUILayout.Label("R resets encounter | F3 toggles second opponent (R first for overlap test) | F6 clears combat log only", text);
            GUILayout.EndArea();
            GUI.depth = previousDepth;
        }
    }
}
