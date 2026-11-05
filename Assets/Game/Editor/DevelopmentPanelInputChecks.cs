using System;
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
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("EvolvingRPG.PanelExitCheck", false))
            {
                SessionState.SetBool("EvolvingRPG.PanelExitCheck", false); Application.logMessageReceived -= CaptureExitError;
                string errors = SessionState.GetString("EvolvingRPG.PanelExitErrors", "");
                File.AppendAllText("Logs/DevelopmentPanelInputChecks.txt", errors.Length == 0 ? "PASS Exit Play mode with panel open: no runtime teardown errors.\n" : "FAIL teardown: " + errors + "\n");
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
            EvasionEvidenceHUD evidenceHUD = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                if (evidenceHUD == null) evidenceHUD = root.GetComponentInChildren<EvasionEvidenceHUD>(true);
            if (evidenceHUD == null || evidenceHUD.EvidenceTitle.Contains("no rewards or evolution effects"))
                throw new InvalidOperationException("Owned scene has misleading C1-only overlay wording.");
            var action = (InputAction)typeof(OwnedCharacter).GetField("inspect", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
            var keyboard = InputSystem.AddDevice<Keyboard>("DevelopmentPanelCheckKeyboard");
            action.ApplyBindingOverride(0, keyboard.path + "/tab");
            var gameType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"); EditorWindow.GetWindow(gameType).Focus();
            Directory.CreateDirectory("Logs"); string report = "Logs/DevelopmentPanelInputChecks.txt";
            File.WriteAllText(report, "Focused Input System Tab check; leaves Play mode with panel open for visual inspection.\n");
            bool reviewFixture = SessionState.GetBool("EvolvingRPG.PanelReview", false);
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
                        if (stage == 5) finished = true;
                    }
                    stage++; started = Time.time;
                }
                catch (Exception e) { File.AppendAllText(report, "FAIL " + e + "\n"); finished = failed = true; }
                if (!finished) return;
                InputSystem.onBeforeUpdate -= inject; EditorApplication.update -= tick;
                action.RemoveAllBindingOverrides(); InputSystem.RemoveDevice(keyboard);
                if (reviewFixture && !failed)
                    typeof(DevelopmentPanel).GetField("scroll", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(UnityEngine.Object.FindFirstObjectByType<DevelopmentPanel>(), new Vector2(0, 10000));
                if (failed || SessionState.GetBool("EvolvingRPG.PanelExitCheck", false)) EditorApplication.ExitPlaymode();
                else Debug.Log("Development panel input checks passed. Panel remains open; exit Play mode after visual inspection.");
            };
            EditorApplication.update += tick;
        }
    }
}
