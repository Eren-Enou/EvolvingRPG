using EvolvingRPG.Combat;
using UnityEngine;
namespace EvolvingRPG.Progression
{
    public sealed class DevelopmentPanel : MonoBehaviour
    {
        [SerializeField] OwnedCharacter character;
        DevelopmentPurchasePreview? preview;
        string observedCharacter;
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
            var expedition = character.GetComponent<EvolvingRPG.Expeditions.ExpeditionLoop>();
            if (expedition != null && (!character.MenuOpen || expedition.BoardOpen)) return;
            var state = character.State;
            if (observedCharacter != state.CharacterId)
            {
                observedCharacter = state.CharacterId;
                preview = null; saveSlot = saveCharacter = null; confirmFresh = false;
            }
            var actions = character.GetComponent<CombatActions>();
            if (!character.MenuOpen)
            {
                GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), 185), GUI.skin.box);
                GUILayout.Label("CHARACTER DEVELOPMENT | " + DevelopmentIds.Label(state.Heavy) + " | points " + state.Balance + " | Tab inspect at spawn / R reset");
                GUILayout.Label($"{actions.Phase} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0} | {actions.Message}");
                GUILayout.Label("Knowledge: " + state.Knowledge + " (" + state.Exchanges + "/3) | return learned: " + state.RetaliationLearned + " / active: " + state.RetaliationActive);
                var returnAction = character.GetComponent<RetaliatoryMomentumTrial>();
                GUILayout.Label(returnAction.OpportunityReady ? "RETURN ARMED " + returnAction.SecondsRemaining.ToString("0.0") + "s: forward intent + RMB adds 0.4 m; neutral/lateral/backward keeps ordinary Momentum." : character.GetComponent<ContextualDiscovery>().Ledger.Diagnostic);
                GUILayout.Label(character.Message);
                GUILayout.Label(character.Experience.Latest);
                GUILayout.EndArea(); return;
            }
            var previousColor = GUI.color; GUI.color = new Color(.07f, .09f, .12f, 1);
            GUI.DrawTexture(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), Screen.height - 30), Texture2D.whiteTexture);
            GUI.color = previousColor;
            GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(760, Screen.width - 24), Screen.height - 30), GUI.skin.box);
            GUILayout.Label(expedition != null ? "OWNED WARRIOR / Tab closes / return to the mission board to deploy" : "OWNED CHARACTER / experimental specialization / Tab inspect at spawn / R encounter reset");
            GUILayout.Label($"Character {state.CharacterId.Substring(0, 8)} | points {state.Balance}/{state.Granted} granted | selected {DevelopmentIds.Label(state.Heavy)}");
            GUILayout.Label($"{actions.Phase} | {actions.Message} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0}");
            GUILayout.Label("Knowledge: " + state.Knowledge + " | qualifying exchanges " + state.Exchanges + "/3 | learned return: " + state.RetaliationLearned + " | active: " + state.RetaliationActive);
            GUILayout.Label(character.Message);
            if (preview != null)
            {
                GUILayout.Label("Confirm spending 1 point on: " + DevelopmentIds.Label(preview.Value.DevelopmentId) + " (preview revision " + preview.Value.Revision + ")");
                if (GUILayout.Button("Confirm investment")) { character.Purchase(preview.Value); preview = null; }
                if (GUILayout.Button("Cancel preview")) preview = null;
            }
            if (saveSlot != null)
            {
                GUILayout.Label("Confirm saving character " + saveCharacter.Substring(0, 8) + " to " + saveSlot + ". Existing supported slot becomes the backup.");
                if (GUILayout.Button("Confirm save / overwrite supported slot")) { if (character.State.CharacterId == saveCharacter) character.SaveSlot(saveSlot); saveSlot = null; }
                if (GUILayout.Button("Cancel save")) saveSlot = null;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.Label("CHARACTER EXPERIENCE / retained facts, not XP or power. " + state.ExperienceCount + " demonstrated. Save explicitly; encounter resets retain completed facts.");
            GUILayout.Label(character.Experience.Latest);
            GUILayout.Label("Prototype rule: one Heavy discipline per character; no respec. Initial fixture points: " + state.InitialPoints + ". First relay completion: one point once per character. Reveal: one additional fixture point. No automatic investment. Not a final economy.");
            GUILayout.Label("Technique numbers describe base attacks. Contextual return is separate; guard and collision can change results. Recent attacks show the form actually committed.");
            Card(DevelopmentIds.Baseline, "Baseline Heavy: the shared starting technique; remains learned after specializing.");
            Card(DevelopmentIds.Breaker, "Breaker: planted disruption with longer exposure. Compare poise and commitment below. Purchase selects this Heavy.");
            Card(DevelopmentIds.Momentum, "Momentum: committed windup advance with lighter impact and additional miss exposure. Purchase selects this Heavy.");
            if (state.Knowledge == DiscoveryKnowledge.Undiscovered) GUILayout.Label("Further connections emerge through invested combat experience.");
            else if (state.Knowledge == DiscoveryKnowledge.Foreshadowed) GUILayout.Label("Hint: your verified evasions are beginning to shape your advancing strikes.");
            else
            {
                var effect = character.GetComponent<RetaliatoryMomentumTrial>();
                Card(DevelopmentIds.Retaliation, $"Retaliatory Momentum: after verified evasion, forward intent + RMB adds {effect.AddedAdvance:0.##} m. +{effect.AddedMissRecovery:0.##} s recovery unless the linked opponent generation is hit. Ordinary miss recovery also applies. Learning does not activate it.");
            }
            if (GUILayout.Button(confirmFresh ? "Confirm fresh character (unsaved current character replaced)" : "Create fresh experimental character"))
            { if (confirmFresh) { character.FreshCharacter(); preview = null; saveSlot = null; confirmFresh = false; } else confirmFresh = true; }
            foreach (string slot in new[] { "A", "B" })
            {
                if (GUILayout.Button("Save current character to slot " + slot)) { saveSlot = slot; saveCharacter = state.CharacterId; }
                if (GUILayout.Button("Load slot " + slot + " (replaces unsaved current character)")) { character.LoadSlot(slot); preview = null; saveSlot = null; confirmFresh = false; }
            }
            GUILayout.Label("Local save folder: " + character.SaveLocation);
            if (state.HasUnresolvedContent) GUILayout.Label(state.ContentWarning);
            GUILayout.Label("Journal: invested Momentum + verified evasion + successful same-opponent Momentum within 1.5 s of dodge recovery + full Heavy recovery. Three distinct strikes reveal; no empty defensive talent is sold.");
            foreach (var entry in character.GetComponent<ContextualDiscovery>().Ledger.Journal) GUILayout.Label(entry);
            GUILayout.Label("Recent attacks / session review only / retained across encounter resets; cleared by character creation or load. No mastery or rewards.");
            if (character.Review.Count == 0) GUILayout.Label("No completed or interrupted attacks yet. Fight, then return to the hub or safe spawn to inspect.");
            foreach (var entry in character.Review.NewestFirst)
            {
                GUILayout.Label(entry.Summary);
                string explanation = character.Experience.ExplanationFor(entry.Sequence);
                if (explanation != null) GUILayout.Label(explanation);
            }
            GUILayout.Label("Displacement is the observed start-to-end position change, not distance walked or proof that a wall caused shortened travel. Applied totals include all contacted targets.");
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        void Card(string id, string description)
        {
            var state = character.State;
            GUILayout.BeginVertical(GUI.skin.box); GUILayout.Label(description);
            if (character.TryPreviewHeavy(id, out var attack))
            {
                GUILayout.Label($"Authored impact: {attack.Damage:0.#} HP / {attack.PoiseDamage:0.#} poise | stamina {attack.StaminaCost:0.#}.");
                GUILayout.Label($"Commitment: {attack.Windup:0.00}s windup + {attack.ActiveTime:0.00}s active + {attack.Recovery:0.00}s recovery | miss adds {attack.MissRecovery:0.00}s.");
                GUILayout.Label($"Reach {attack.Range:0.00}m / {attack.Arc:0.#} deg | windup advance {attack.AdvanceDistance:0.00}m; collision may shorten travel | directional movement {attack.MovementMultiplier:P0}.");
            }
            if (id != DevelopmentIds.Retaliation && state.Owns(id))
            {
                string history = "Demonstrated: "; bool any = false;
                foreach (string outcome in HeavyExperienceCatalog.Outcomes)
                    if (state.HasExperience(id, outcome)) { history += (any ? ", " : "") + HeavyExperienceCatalog.Label(outcome); any = true; }
                GUILayout.Label(history + (any ? "." : "none yet. Finish a Heavy in a live exchange; no enemy strike prerequisite."));
            }
            GUILayout.Label(state.Owns(id) ? "Learned" + (state.Heavy == id || id == DevelopmentIds.Retaliation && state.RetaliationActive ? " / Active" : " / Inactive") : state.Eligibility(id));
            if (!state.Owns(id))
            {
                if (state.Eligibility(id).StartsWith("Available") && GUILayout.Button("Preview investment")) { preview = state.PreviewPurchase(id); }
            }
            else if (id == DevelopmentIds.Retaliation)
            { if (GUILayout.Button(state.RetaliationActive ? "Deactivate return" : "Activate learned return")) character.ConfigureHeavy(DevelopmentIds.Momentum, !state.RetaliationActive); }
            else if (GUILayout.Button("Select this learned Heavy")) character.ConfigureHeavy(id, false);
            GUILayout.EndVertical();
        }
    }
}
