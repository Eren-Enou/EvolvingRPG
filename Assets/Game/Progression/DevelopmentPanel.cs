using EvolvingRPG.Combat;
using UnityEngine;
namespace EvolvingRPG.Progression
{
    public sealed class DevelopmentPanel : MonoBehaviour
    {
        [SerializeField] OwnedCharacter character;
        string preview;
        int previewRevision;
        bool confirmFresh;
        Vector2 scroll;
        public void Configure(OwnedCharacter owner) => character = owner;
        void OnGUI()
        {
            if (character == null || character.State == null) return;
            var state = character.State; var actions = character.GetComponent<CombatActions>();
            GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), character.MenuOpen ? Screen.height - 30 : 170), GUI.skin.box);
            GUILayout.Label("OWNED CHARACTER / experimental specialization / Tab inspect at spawn / R encounter reset");
            GUILayout.Label($"Character {state.CharacterId.Substring(0, 8)} | points {state.Balance}/{state.Granted} granted | selected {state.Heavy}");
            GUILayout.Label($"{actions.Phase} | {actions.Message} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0}");
            GUILayout.Label("Knowledge: " + state.Knowledge + " | qualifying exchanges " + state.Exchanges + "/3 | learned return: " + state.RetaliationLearned + " | active: " + state.RetaliationActive);
            GUILayout.Label(character.Message);
            if (!character.MenuOpen)
            {
                GUILayout.Label(character.GetComponent<ContextualDiscovery>().Ledger.Diagnostic);
                GUILayout.Label("WASD / Q,E | Space | Shift dodge | mouse look/wheel | LMB / RMB. Debug 1/2/3 and F2/F4/F5/F8 do not grant power here.");
                GUILayout.EndArea(); return;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("Prototype rule: one Heavy discipline per character; no respec. One point initially; one additional, once-only fixture point on reveal. Compare another fresh character. Not a final economy.");
            Card(DevelopmentIds.Baseline, "Baseline Heavy: original commitment, 45 damage / 40 poise; no advance.");
            Card(DevelopmentIds.Breaker, "Breaker: planted 60 poise, 0.65 s windup / 0.85 s recovery; longer exposure. Purchase selects this Heavy.");
            Card(DevelopmentIds.Momentum, "Momentum: 1.2 m committed advance, 40 damage / 25 poise; +0.55 s miss recovery. Purchase selects this Heavy.");
            if (state.Knowledge == DiscoveryKnowledge.Undiscovered) GUILayout.Label("Further connections emerge through invested combat experience.");
            else if (state.Knowledge == DiscoveryKnowledge.Foreshadowed) GUILayout.Label("Hint: your verified evasions are beginning to shape your advancing strikes.");
            else Card(DevelopmentIds.Retaliation, "Retaliatory Momentum: after verified evasion, forward intent + RMB adds 0.4 m. +0.25 s recovery unless the linked opponent generation is hit. Learning does not activate it.");
            if (preview != null)
            {
                GUILayout.Label("Confirm spending 1 point on: " + preview + " (preview revision " + previewRevision + ")");
                if (GUILayout.Button("Confirm investment")) { character.Purchase(preview, previewRevision); preview = null; }
                if (GUILayout.Button("Cancel preview")) preview = null;
            }
            if (GUILayout.Button(confirmFresh ? "Confirm fresh character (unsaved current character replaced)" : "Create fresh experimental character"))
            { if (confirmFresh) { character.FreshCharacter(); confirmFresh = false; preview = null; } else confirmFresh = true; }
            GUILayout.Label("Journal: invested Momentum + verified evasion + successful same-opponent Momentum within 1.5 s of dodge recovery + full Heavy recovery. Three distinct strikes reveal; no empty defensive talent is sold.");
            foreach (var entry in character.GetComponent<ContextualDiscovery>().Ledger.Journal) GUILayout.Label(entry);
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        void Card(string id, string description)
        {
            var state = character.State;
            GUILayout.BeginVertical(GUI.skin.box); GUILayout.Label(description);
            GUILayout.Label(state.Owns(id) ? "Learned" + (state.Heavy == id || id == DevelopmentIds.Retaliation && state.RetaliationActive ? " / Active" : " / Inactive") : state.Eligibility(id));
            if (!state.Owns(id))
            {
                if (state.Eligibility(id).StartsWith("Available") && GUILayout.Button("Preview investment")) { preview = id; previewRevision = state.Revision; }
            }
            else if (id == DevelopmentIds.Retaliation)
            { if (GUILayout.Button(state.RetaliationActive ? "Deactivate return" : "Activate learned return")) character.ConfigureHeavy(DevelopmentIds.Momentum, !state.RetaliationActive); }
            else if (GUILayout.Button("Select this learned Heavy")) character.ConfigureHeavy(id, false);
            GUILayout.EndVertical();
        }
    }
}
