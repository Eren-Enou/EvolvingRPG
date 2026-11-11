using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Movement;
using EvolvingRPG.Progression;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    [InitializeOnLoad]
    public static class DevelopmentPanelInputChecks
    {
        const string Requested = "EvolvingRPG.OwnedPanelInput";
        const string Report = "Logs/DevelopmentPanelInputChecks.txt";
        const string SlotChecksPassed = "PASS Game-view save and load review, cancel, confirm, stale review rejection and backup preservation.";
        static DevelopmentPanelInputChecks() => EditorApplication.playModeStateChanged += Changed;
        [MenuItem("Tools/EvolvingRPG/Run Development Panel Input Checks %#F9")]
        public static void Run() => RunMode(false, false);
        [MenuItem("Tools/EvolvingRPG/Run Development Panel Exit Checks %#F10")]
        public static void RunExitChecks() => RunMode(true, false);
        [MenuItem("Tools/EvolvingRPG/Inspect Action Review Fixture %#&F12")]
        public static void RunReviewInspection() => RunMode(false, true);
        static void RunMode(bool exitAfter, bool review)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits first.");
            DevelopmentSandboxBuilder.Create(); EditorSceneManager.OpenScene(DevelopmentSandboxBuilder.ScenePath);
            SessionState.SetBool("EvolvingRPG.PanelExitCheck", exitAfter);
            SessionState.SetBool("EvolvingRPG.PanelReview", review);
            SessionState.SetString("EvolvingRPG.PanelExitErrors", "");
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Development panel checks requested.\n");
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("EvolvingRPG.PanelExitCheck", false))
            {
                SessionState.SetBool("EvolvingRPG.PanelExitCheck", false); Application.logMessageReceived -= CaptureExitError;
                string errors = SessionState.GetString("EvolvingRPG.PanelExitErrors", "");
                bool slotChecksPassed = File.Exists(Report) && File.ReadAllText(Report).Contains(SlotChecksPassed);
                File.AppendAllText(Report, errors.Length != 0 ? "FAIL teardown: " + errors + "\n"
                    : !slotChecksPassed ? "FAIL Exit check ended before the save-slot controls were exercised.\n"
                    : "PASS Exit Play mode with panel open: no runtime teardown errors.\n");
            }
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false); Begin();
        }
        static void CaptureExitError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                SessionState.SetString("EvolvingRPG.PanelExitErrors", SessionState.GetString("EvolvingRPG.PanelExitErrors", "") + message + "\n" + stack);
        }
        static void Begin()
        {
            if (SessionState.GetBool("EvolvingRPG.PanelExitCheck", false)) Application.logMessageReceived += CaptureExitError;
            OwnedCharacter owner = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) if (owner == null) owner = root.GetComponentInChildren<OwnedCharacter>(true);
            if (owner == null) throw new InvalidOperationException("Missing owned-character scene.");
            string saveRoot = Path.GetFullPath(Path.Combine("Logs", "PanelInputSaves", Guid.NewGuid().ToString("N")));
            owner.UseTestSaveDirectory(saveRoot);
            EvasionEvidenceHUD evidenceHUD = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                if (evidenceHUD == null) evidenceHUD = root.GetComponentInChildren<EvasionEvidenceHUD>(true);
            if (evidenceHUD == null || evidenceHUD.EvidenceTitle.Contains("no rewards or evolution effects"))
                throw new InvalidOperationException("Owned scene has misleading C1-only overlay wording.");
            var action = (InputAction)typeof(OwnedCharacter).GetField("inspect", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
            var keyboard = InputSystem.AddDevice<Keyboard>("DevelopmentPanelCheckKeyboard");
            action.ApplyBindingOverride(0, keyboard.path + "/tab");
            var gameType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(gameType); gameView.Focus();
            string report = Report;
            File.WriteAllText(report, "Focused Input System Tab check; leaves Play mode with panel open for visual inspection.\n");
            bool reviewFixture = SessionState.GetBool("EvolvingRPG.PanelReview", false);
            bool runSlotChecks = SessionState.GetBool("EvolvingRPG.PanelExitCheck", false);
            var buttons = new Dictionary<string, Vector2>();
            void ObserveButton(string label, Vector2 center) => buttons[label] = center;
            void Click(string label)
            {
                if (!buttons.TryGetValue(label, out var center) || !new Rect(0, 0, Screen.width, Screen.height).Contains(center))
                    throw new InvalidOperationException("Slot control is not visible in Game view: " + label);
                buttons.Clear();
                // Queue events in the game only, without moving the user's pointer or typing into the Editor.
                EditorGUIUtility.QueueGameViewInputEvent(new Event { type = EventType.MouseDown, button = 0, clickCount = 1, mousePosition = center });
                EditorGUIUtility.QueueGameViewInputEvent(new Event { type = EventType.MouseUp, button = 0, clickCount = 1, mousePosition = center });
                gameView.Repaint();
            }
            IEnumerator slotChecks = null;
            double nextClick = 0;
            if (runSlotChecks)
            {
                var store = new CharacterSaveStore(saveRoot);
                var momentum = Warrior(DevelopmentIds.Momentum);
                var breaker = Warrior(DevelopmentIds.Breaker);
                var baselineContact = new[] { HeavyExperienceCatalog.Id(DevelopmentIds.Baseline, HeavyExperienceCatalog.Contact) };
                Require(momentum.RecordExperience(baselineContact) == 1 && breaker.RecordExperience(baselineContact) == 1,
                    "could not seed saved practice");
                Require(store.Save("A", momentum, out _) && store.Save("B", breaker, out _), "could not seed isolated slot fixtures");
                slotChecks = ExerciseSlots(owner, store, momentum, breaker, Click, buttons.ContainsKey, report);
                JourneyGui.ButtonRepainted += ObserveButton;
            }
            bool pressed = false; int stage = reviewFixture ? -2 : 0, frame = -1; float started = Time.time;
            double deadline = EditorApplication.timeSinceStartup + 15;
            Action inject = () => { if (InputState.currentUpdateType == InputUpdateType.Dynamic) InputSystem.QueueStateEvent(keyboard, pressed ? new KeyboardState(Key.Tab) : new KeyboardState()); };
            InputSystem.onBeforeUpdate += inject;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                bool finished = false, failed = false;
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Panel input check needs a focused Game view.");
                    if (frame == Time.frameCount) return; frame = Time.frameCount;
                    if (stage == -2)
                    {
                        var actor = owner.GetComponent<CombatActions>();
                        if (Time.time - started < .5f || !actor.TryAttack(actor.HeavyAttack)) return;
                        stage = -1; return;
                    }
                    if (stage == -1)
                    {
                        if (owner.GetComponent<CombatActions>().Busy) return;
                        if (owner.Review.Latest == null || owner.Review.Latest.Interrupted || owner.Review.Latest.Contacts != 0)
                            throw new InvalidOperationException("Native spawn miss did not produce a completed review.");
                        File.AppendAllText(report, "PASS Native spawn Heavy miss produces a readable review; no rewards or save writes.\n");
                        stage = 0; started = Time.time; return;
                    }
                    if (stage >= 6)
                    {
                        if (EditorApplication.timeSinceStartup < nextClick) return;
                        finished = !slotChecks.MoveNext();
                        nextClick = EditorApplication.timeSinceStartup + .2;
                    }
                    else
                    {
                        if (Time.time - started < (stage == 0 ? .5f : .15f)) return;
                        if (stage == 0 || stage == 2 || stage == 4) pressed = true;
                        else
                        {
                            pressed = false;
                            bool expectedOpen = stage == 1 || stage == 5;
                            if (owner.MenuOpen != expectedOpen || owner.GetComponent<PlayerInput>().inputIsActive == expectedOpen
                                || owner.GetComponent<PlaygroundMouseLook>().enabled == expectedOpen)
                                throw new InvalidOperationException("Tab failed menu/input ownership: stage " + stage + ", focused=" + Application.isFocused + ", menu=" + owner.MenuOpen);
                            File.AppendAllText(report, "PASS Tab " + (expectedOpen ? "opens inspectable panel, releases cursor and suspends movement/look" : "closes panel and restores input/look") + ".\n");
                            if (stage == 5) { finished = !runSlotChecks; nextClick = EditorApplication.timeSinceStartup + .2; }
                        }
                        stage++; started = Time.time;
                    }
                }
                catch (Exception e) { File.AppendAllText(report, "FAIL " + e + "\n"); finished = failed = true; }
                if (!finished) return;
                InputSystem.onBeforeUpdate -= inject; EditorApplication.update -= tick;
                JourneyGui.ButtonRepainted -= ObserveButton;
                action.RemoveAllBindingOverrides(); InputSystem.RemoveDevice(keyboard);
                if (reviewFixture && !failed)
                    typeof(DevelopmentPanel).GetField("scroll", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(UnityEngine.Object.FindFirstObjectByType<DevelopmentPanel>(), new Vector2(0, 10000));
                if (failed || SessionState.GetBool("EvolvingRPG.PanelExitCheck", false)) EditorApplication.ExitPlaymode();
                else Debug.Log("Development panel input checks passed. Panel remains open; exit Play mode after visual inspection.");
            };
            EditorApplication.update += tick;
        }
        static CharacterDevelopmentState Warrior(string heavy)
        {
            var state = new CharacterDevelopmentState();
            Require(state.Purchase(heavy, state.Revision, out _), "could not create slot build");
            return state;
        }
        static string SameSummaryWithDifferentPractice(CharacterDevelopmentState state)
        {
            var record = state.Export();
            record.heavyExperience = new[] { HeavyExperienceCatalog.Id(state.Primary, HeavyExperienceCatalog.Contact) };
            Require(CharacterDevelopmentState.TryRestore(record, out var branch, out _) && branch.Revision == state.Revision
                && branch.CharacterId == state.CharacterId && branch.Heavy == state.Heavy
                && !branch.HasExperience(DevelopmentIds.Baseline, HeavyExperienceCatalog.Contact),
                "could not create a different save with the same summary");
            return JsonUtility.ToJson(record, true);
        }
        static IEnumerator ExerciseSlots(OwnedCharacter owner, CharacterSaveStore store, CharacterDevelopmentState momentum,
            CharacterDevelopmentState breaker, Action<string> click, Func<string, bool> isVisible, string report)
        {
            string original = owner.State.CharacterId;
            string slotA = Path.Combine(store.Location, "character-a.json");
            string originalA = File.ReadAllText(slotA);
            click("Review load A"); yield return null;
            Require(owner.State.CharacterId == original && isVisible("Confirm load"), "review load did not stage a confirmation without replacing the Warrior");
            click("Cancel slot action"); yield return null;
            Require(owner.State.CharacterId == original && !isVisible("Confirm load") && isVisible("Review load A"),
                "cancel load replaced the current Warrior or left confirmation active");
            click("Review load A"); yield return null;
            click("Confirm load"); yield return null;
            Require(owner.State.CharacterId == momentum.CharacterId && owner.State.Heavy == DevelopmentIds.Momentum,
                "confirmed load did not restore the reviewed Warrior and technique");
            string slotB = Path.Combine(store.Location, "character-b.json");
            click("Review load B"); yield return null;
            File.WriteAllText(slotB, SameSummaryWithDifferentPractice(breaker));
            click("Confirm load"); yield return null;
            Require(owner.State.CharacterId == momentum.CharacterId && !isVisible("Confirm load"),
                "stale review loaded a changed record with the same summary");
            click("Review load B"); yield return null;
            var replacement = Warrior(DevelopmentIds.Breaker);
            Require(replacement.CharacterId != breaker.CharacterId && store.Save("B", replacement, out _), "could not update reviewed slot fixture");
            click("Confirm load"); yield return null;
            Require(owner.State.CharacterId == momentum.CharacterId, "stale review loaded a different Warrior");
            click("Review load B"); yield return null;
            click("Confirm load"); yield return null;
            Require(owner.State.CharacterId == replacement.CharacterId && owner.State.Heavy == DevelopmentIds.Breaker,
                "reviewing the changed slot did not allow the new Warrior to load");
            click("Review save A"); yield return null;
            Require(isVisible("Confirm save") && File.ReadAllText(slotA) == originalA && !File.Exists(slotA + ".bak"),
                "review save did not stage a confirmation without writing the slot");
            click("Cancel slot action"); yield return null;
            Require(!isVisible("Confirm save") && isVisible("Review save A") && File.ReadAllText(slotA) == originalA && !File.Exists(slotA + ".bak"),
                "cancel save altered the slot or left confirmation active");
            click("Review save A"); yield return null;
            string changedA = SameSummaryWithDifferentPractice(momentum);
            File.WriteAllText(slotA, changedA);
            click("Confirm save"); yield return null;
            Require(File.ReadAllText(slotA) == changedA && !File.Exists(slotA + ".bak"),
                "stale review overwrote a changed record with the same summary");
            click("Review save A"); yield return null;
            click("Confirm save"); yield return null;
            Require(store.Load("A", out var saved, out _) && saved.CharacterId == replacement.CharacterId
                && File.ReadAllText(slotA + ".bak") == changedA, "confirmed overwrite did not save the active Warrior and retain the reviewed file");
            File.AppendAllText(report, SlotChecksPassed + "\n");
        }
        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
