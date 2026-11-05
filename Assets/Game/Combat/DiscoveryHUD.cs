using UnityEngine;
namespace EvolvingRPG.Combat
{
    public sealed class DiscoveryHUD : MonoBehaviour
    {
        [SerializeField] ContextualDiscovery discovery;
        GUIStyle text;
        public void Configure(ContextualDiscovery value) => discovery = value;
        void OnGUI()
        {
            if (discovery == null || discovery.Ledger == null) return;
            if (text == null) { text = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true }; text.normal.textColor = Color.white; }
            GUILayout.BeginArea(new Rect(Mathf.Max(12, Screen.width - 470), 12, Mathf.Min(458, Screen.width - 24), Mathf.Min(440, Screen.height - 24)), GUI.skin.box);
            GUILayout.Label("CONTEXTUAL DISCOVERY / session experiment", text);
            GUILayout.Label("F4 Momentum investment fixture: " + discovery.MomentumInvestment + " | F5 evasive investment fixture: " + discovery.EvasiveInvestment, text);
            GUILayout.Label("Fixtures are experimental prerequisites, not purchases. Select 3 for Momentum separately. No Heavy Practice refinement.", text);
            GUILayout.Label("Knowledge: " + discovery.Ledger.Knowledge + " | qualified exchanges: " + discovery.Ledger.Exchanges + "/3 | learned: NO", text);
            GUILayout.Label(discovery.Ledger.Diagnostic, text);
            foreach (string entry in discovery.Ledger.Journal) GUILayout.Label(entry, text);
            GUILayout.Label("R: encounter reset retains knowledge/fixtures. F8: Fresh Character clears them. All progress ends with Play mode.", text);
            GUILayout.EndArea();
        }
    }
}
