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
        string saveSlot, saveCharacter;
        Vector2 scroll;
        public void Configure(OwnedCharacter owner) => character = owner;
        void OnGUI()
        {
            bool wrap = GUI.skin.label.wordWrap; int depth = GUI.depth;
            try { GUI.skin.label.wordWrap = true; if (character != null && character.MenuOpen) GUI.depth = -20; Draw(); }
            finally { GUI.skin.label.wordWrap = wrap; GUI.depth = depth; }
        }
        void Draw()
        {
            if (character == null || character.State == null) return;
            var state = character.State; var actions = character.GetComponent<CombatActions>();
            if (!character.MenuOpen)
            {
                GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), 185), GUI.skin.box);
                GUILayout.Label("CHARACTER DEVELOPMENT | " + DevelopmentIds.Label(state.Heavy) + " | points " + state.Balance + " | Tab inspect at spawn / R reset");
                GUILayout.Label($"{actions.Phase} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0} | {actions.Message}");
                GUILayout.Label("Knowledge: " + state.Knowledge + " (" + state.Exchanges + "/3) | return learned: " + state.RetaliationLearned + " / active: " + state.RetaliationActive);
                var returnAction = character.GetComponent<RetaliatoryMomentumTrial>();
                GUILayout.Label(returnAction.OpportunityReady ? "RETURN ARMED " + returnAction.SecondsRemaining.ToString("0.0") + "s: forward intent + RMB adds 0.4 m; neutral/lateral/backward keeps ordinary Momentum." : character.GetComponent<ContextualDiscovery>().Ledger.Diagnostic);
                GUILayout.Label(character.Message);
                GUILayout.EndArea(); return;
            }
            var previousColor = GUI.color; GUI.color = new Color(.07f, .09f, .12f, 1);
            GUI.DrawTexture(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), Screen.height - 30), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), Screen.height - 30), GUI.skin.box);
            GUILayout.Label("OWNED CHARACTER / experimental specialization / Tab inspect at spawn / R encounter reset");
            GUILayout.Label($"Character {state.CharacterId.Substring(0, 8)} | points {state.Balance}/{state.Granted} granted | selected {DevelopmentIds.Label(state.Heavy)}");
            GUILayout.Label($"{actions.Phase} | {actions.Message} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0}");
            GUILayout.Label("Knowledge: " + state.Knowledge + " | qualifying exchanges " + state.Exchanges + "/3 | learned return: " + state.RetaliationLearned + " | active: " + state.RetaliationActive);
            GUILayout.Label(character.Message);
            if (preview != null)
            {
                GUILayout.Label("Confirm spending 1 point on: " + DevelopmentIds.Label(preview) + " (preview revision " + previewRevision + ")");
                if (GUILayout.Button("Confirm investment")) { character.Purchase(preview, previewRevision); preview = null; }
                if (GUILayout.Button("Cancel preview")) preview = null;
            }
            if (saveSlot != null)
            {
                GUILayout.Label("Confirm saving character " + saveCharacter.Substring(0, 8) + " to " + saveSlot + ". Existing supported slot becomes the backup.");
                if (GUILayout.Button("Confirm save / overwrite supported slot")) { if (character.State.CharacterId == saveCharacter) character.SaveSlot(saveSlot); saveSlot = null; }
                if (GUILayout.Button("Cancel save")) saveSlot = null;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("Prototype rule: one Heavy discipline per character; no respec. One point initially; one additional, once-only fixture point on reveal. Compare another fresh character. Not a final economy.");
            Card(DevelopmentIds.Baseline, "Baseline Heavy: original commitment, 45 damage / 40 poise; no advance.");
            Card(DevelopmentIds.Breaker, "Breaker: planted 60 poise, 0.65 s windup / 0.85 s recovery; longer exposure. Purchase selects this Heavy.");
            Card(DevelopmentIds.Momentum, "Momentum: 1.2 m committed advance, 40 damage / 25 poise; +0.55 s miss recovery. Purchase selects this Heavy.");
            if (state.Knowledge == DiscoveryKnowledge.Undiscovered) GUILayout.Label("Further connections emerge through invested combat experience.");
            else if (state.Knowledge == DiscoveryKnowledge.Foreshadowed) GUILayout.Label("Hint: your verified evasions are beginning to shape your advancing strikes.");
            else Card(DevelopmentIds.Retaliation, "Retaliatory Momentum: after verified evasion, forward intent + RMB adds 0.4 m. +0.25 s recovery unless the linked opponent generation is hit. Learning does not activate it.");
            if (GUILayout.Button(confirmFresh ? "Confirm fresh character (unsaved current character replaced)" : "Create fresh experimental character"))
            { if (confirmFresh) { character.FreshCharacter(); confirmFresh = false; preview = saveSlot = null; } else confirmFresh = true; }
            foreach (string slot in new[] { "A", "B" })
            {
                if (GUILayout.Button("Save current character to slot " + slot)) { saveSlot = slot; saveCharacter = state.CharacterId; }
                if (GUILayout.Button("Load slot " + slot + " (replaces unsaved current character)")) { character.LoadSlot(slot); preview = saveSlot = null; }
            }
            GUILayout.Label("Local save folder: " + character.SaveLocation);
            if (state.HasUnresolvedContent) GUILayout.Label(state.ContentWarning);
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
