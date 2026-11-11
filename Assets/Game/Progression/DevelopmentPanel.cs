using EvolvingRPG.Combat;
using EvolvingRPG.Expeditions;
using UnityEngine;
namespace EvolvingRPG.Progression
{
    public sealed class DevelopmentPanel : MonoBehaviour
    {
        [SerializeField] OwnedCharacter character;
        DevelopmentPurchasePreview? preview;
        CharacterDevelopmentState observedCharacter;
        bool confirmFresh;
        CharacterSaveStore.SlotPreview[] slots;
        CharacterSaveStore.SlotPreview? pendingSlot;
        bool loadingSlot;
        int pendingRevision;
        string slotNotice;
        Vector2 scroll;
        enum Section { SavedWarriors, Techniques, History }
        Section section;
        bool reviewReturn;
        ExpeditionLoop expedition;
        public void Configure(OwnedCharacter owner) => character = owner;

        void OnEnable()
        {
            expedition = character != null ? character.GetComponent<ExpeditionLoop>() : null;
            if (expedition != null) expedition.TechniquesRequested += ShowTechniques;
        }

        void OnDisable()
        {
            if (expedition != null) expedition.TechniquesRequested -= ShowTechniques;
        }

        void ShowTechniques(bool focusReturn) => ShowSection(Section.Techniques, focusReturn);

        void ShowSection(Section next, bool focusReturn = false)
        {
            section = next;
            reviewReturn = focusReturn;
            scroll = Vector2.zero;
            preview = null; pendingSlot = null; confirmFresh = false; slotNotice = null;
        }
        void OnGUI()
        {
            JourneyGui.Begin();
            bool wrap = GUI.skin.label.wordWrap; int depth = GUI.depth;
            try { GUI.skin.label.wordWrap = true; if (character != null && character.MenuOpen) GUI.depth = -20; Draw(); }
            finally { GUI.skin.label.wordWrap = wrap; GUI.depth = depth; }
        }
        void Draw()
        {
            if (character == null || character.State == null) return;
            var state = character.State;
            if (observedCharacter != state || !character.MenuOpen || (expedition != null && expedition.BoardOpen))
            {
                observedCharacter = state;
                preview = null; pendingSlot = null; slots = null; confirmFresh = false; slotNotice = null;
                if (!character.MenuOpen || (expedition != null && expedition.BoardOpen)) ShowSection(Section.SavedWarriors);
            }
            if (expedition != null && (!character.MenuOpen || expedition.BoardOpen)) return;
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
            if (expedition != null)
            {
                GUILayout.Label($"Warrior {state.CharacterId.Substring(0, 8)} · {DevelopmentIds.Label(state.Heavy)} Heavy · {state.Balance} unspent development {(state.Balance == 1 ? "point" : "points")}");
                GUILayout.BeginHorizontal();
                if (JourneyGui.Button("Techniques")) ShowSection(Section.Techniques);
                if (JourneyGui.Button("Saved Warriors")) ShowSection(Section.SavedWarriors);
                if (JourneyGui.Button("Combat history")) ShowSection(Section.History);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label($"Character {state.CharacterId.Substring(0, 8)} | points {state.Balance}/{state.Granted} granted | selected {DevelopmentIds.Label(state.Heavy)}");
                GUILayout.Label($"{actions.Phase} | {actions.Message} | HP {character.GetComponent<CombatVitality>().Health:0} | stamina {character.GetComponent<CombatStamina>().Current:0}");
                GUILayout.Label("Knowledge: " + state.Knowledge + " | qualifying exchanges " + state.Exchanges + "/3 | learned return: " + state.RetaliationLearned + " | active: " + state.RetaliationActive);
            }
            GUILayout.Label(character.Message);
            if (preview != null)
            {
                GUILayout.Label($"Spend 1 development point to learn {DevelopmentIds.Label(preview.Value.DevelopmentId)}?");
                if (JourneyGui.Button("Confirm investment")) { character.Purchase(preview.Value); preview = null; }
                if (JourneyGui.Button("Cancel preview")) preview = null;
            }
            scroll = GUILayout.BeginScrollView(scroll);
            if (expedition == null || section == Section.SavedWarriors) DrawSavedWarriors(state);
            if (expedition == null || section == Section.Techniques) DrawTechniques(state, expedition != null);
            if (expedition == null || section == Section.History) DrawHistory(state);
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }

        void DrawSavedWarriors(CharacterDevelopmentState state)
        {
            DrawSaveSlots(state);
            if (JourneyGui.Button(confirmFresh ? "Confirm fresh character (unsaved current character replaced)" : "Create fresh experimental character"))
            {
                pendingSlot = null;
                if (confirmFresh) { character.FreshCharacter(); preview = null; confirmFresh = false; }
                else confirmFresh = true;
            }
            GUILayout.Label("Local save folder: " + character.SaveLocation);
            if (state.HasUnresolvedContent) GUILayout.Label(state.ContentWarning);
        }

        void DrawTechniques(CharacterDevelopmentState state, bool inExpedition)
        {
            if (inExpedition)
                GUILayout.Label("Each Warrior can learn one Heavy discipline. Learning it equips that Heavy. Baseline remains available.");
            else
            {
                GUILayout.Label("Prototype rule: one Heavy discipline per character; no respec. Initial fixture points: " + state.InitialPoints + ". First relay completion: one point once per character. Reveal: one additional fixture point. No automatic investment. Not a final economy.");
                Card(DevelopmentIds.Baseline, "Baseline Heavy: the shared starting technique; remains learned after specializing.");
            }
            if (reviewReturn && state.Knowledge == DiscoveryKnowledge.Revealed) DrawReturnCard();
            Card(DevelopmentIds.Breaker, "Breaker: plant your feet to disrupt guards and break poise. Allow time for the longer recovery.");
            Card(DevelopmentIds.Momentum, "Momentum: advance during windup to close a gap. It deals less poise damage and takes longer to recover on a miss.");
            if (inExpedition) Card(DevelopmentIds.Baseline, "Baseline Heavy: the shared starting technique; remains learned after specializing.");
            if (state.Knowledge == DiscoveryKnowledge.Undiscovered) GUILayout.Label("Further connections emerge through invested combat experience.");
            else if (state.Knowledge == DiscoveryKnowledge.Foreshadowed) GUILayout.Label("Hint: your verified evasions are beginning to shape your advancing strikes.");
            else if (!reviewReturn) DrawReturnCard();
            GUILayout.Label("The cards show base attacks. Combat history shows what was committed and what happened.");
        }

        void DrawReturnCard()
        {
            var effect = character.GetComponent<RetaliatoryMomentumTrial>();
            Card(DevelopmentIds.Retaliation, $"Retaliatory Momentum: after verified evasion, forward intent + RMB adds {effect.AddedAdvance:0.##} m. +{effect.AddedMissRecovery:0.##} s recovery unless you hit the linked fighter. Equip it after learning.");
        }

        void DrawHistory(CharacterDevelopmentState state)
        {
            GUILayout.Label($"Heavy experience · {state.ExperienceCount} demonstrated");
            GUILayout.Label(character.Experience.Latest);
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
        }
        void RefreshSlots() => slots = new[] { character.InspectSaveSlot("A"), character.InspectSaveSlot("B") };

        static string SavedWarrior(CharacterSaveStore.SlotPreview slot) =>
            slot.CharacterId.Substring(0, 8) + " · " + DevelopmentIds.Label(slot.Heavy)
                + (slot.RetaliationActive ? " + Retaliatory Momentum" : "")
                + (slot.Unresolved ? " · missing content" : "");

        void DrawSaveSlots(CharacterDevelopmentState state)
        {
            if (slots == null) RefreshSlots();
            GUILayout.Label("Saved Warriors");
            GUILayout.Label(character.SaveStatus);
            if (GUILayout.Button("Refresh saved Warriors")) { RefreshSlots(); pendingSlot = null; slotNotice = null; }
            if (slotNotice != null) GUILayout.Label(slotNotice);
            foreach (var slot in slots)
            {
                string label = slot.CanLoad ? SavedWarrior(slot) + (slot.UsesBackup ? " (backup)" : "")
                    : slot.CanSave && string.IsNullOrEmpty(slot.Detail) ? "Empty" : "Unavailable";
                GUILayout.Label($"Slot {slot.Slot}: {label}");
                if (!string.IsNullOrEmpty(slot.Detail)) GUILayout.Label(slot.Detail);
                GUILayout.BeginHorizontal();
                bool enabled = GUI.enabled;
                GUI.enabled = enabled && slot.CanLoad;
                if (JourneyGui.Button($"Review load {slot.Slot}")) ReviewSlot(slot, state, true);
                GUI.enabled = enabled && slot.CanSave && !state.HasUnresolvedContent;
                if (JourneyGui.Button($"Review save {slot.Slot}")) ReviewSlot(slot, state, false);
                GUI.enabled = enabled;
                GUILayout.EndHorizontal();
            }
            if (!pendingSlot.HasValue) return;
            var selected = pendingSlot.Value;
            string current = state.CharacterId.Substring(0, 8);
            GUILayout.Label(loadingSlot
                ? $"Load {SavedWarrior(selected)} from slot {selected.Slot}{(selected.UsesBackup ? " backup" : "")}? Unsaved changes to Warrior {current} will be lost."
                : $"Save Warrior {current} to slot {selected.Slot}?"
                    + (selected.HasLatest ? $" This replaces {SavedWarrior(selected)}; the previous save becomes the backup."
                        : selected.UsesBackup ? " The existing backup is preserved." : ""));
            if (JourneyGui.Button(loadingSlot ? "Confirm load" : "Confirm save"))
            {
                pendingSlot = null;
                if (character.State != state || state.Revision != pendingRevision || !selected.Equals(character.InspectSaveSlot(selected.Slot)))
                    slotNotice = "The character or save slot changed. Review it again before continuing.";
                else if (loadingSlot) character.LoadSlot(selected.Slot);
                else character.SaveSlot(selected.Slot);
                RefreshSlots();
            }
            if (JourneyGui.Button("Cancel slot action")) pendingSlot = null;
        }

        void ReviewSlot(CharacterSaveStore.SlotPreview slot, CharacterDevelopmentState state, bool load)
        {
            pendingSlot = slot; pendingRevision = state.Revision; loadingSlot = load;
            preview = null; confirmFresh = false; slotNotice = null;
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
                if (state.Eligibility(id).StartsWith("Available") && JourneyGui.Button("Preview investment", id)) { preview = state.PreviewPurchase(id); }
            }
            else if (id == DevelopmentIds.Retaliation)
            { if (JourneyGui.Button(state.RetaliationActive ? "Deactivate return" : "Activate learned return")) character.ConfigureHeavy(DevelopmentIds.Momentum, !state.RetaliationActive); }
            else if (GUILayout.Button("Select this learned Heavy")) character.ConfigureHeavy(id, false);
            GUILayout.EndVertical();
        }
    }
}
