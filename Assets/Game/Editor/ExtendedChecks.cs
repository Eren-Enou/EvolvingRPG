using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // Explicit interactive runner: no import-time tests and no second Editor process.
    [InitializeOnLoad]
    public static class ExtendedChecks
    {
        const string Active = "EvolvingRPG.Extended.Active", Stage = "EvolvingRPG.Extended.Stage";
        const string Report = "Logs/ExtendedChecks.txt";
        static readonly string[] Reports = { "CombatPlayModeChecks", "TechniquePlayModeChecks", "EvasionEvidenceChecks", "EnemyPlayModeChecks", "PracticePlayModeChecks", "AttackSnapshotChecks", "DiscoveryChecks", "RetaliationChecks", "DevelopmentChecks", "DevelopmentPanelInputChecks", "StrafingPlayModeChecks", "HeavyExperienceChecks", "CombatTradeChecks", "ExpeditionChecks" };
        static readonly string[] Completed = {
            "PASS: no runtime Console errors captured.", "PASS: all motor checks; no runtime Console errors captured.",
            "PASS all C1 checks; no runtime Console errors captured.", "PASS: live smoke; no runtime Console errors captured.",
            "PASS: live practice; no runtime Console errors captured.", "PASS: all checks; no runtime Console errors captured.",
            "PASS All C2 checks; no runtime Console errors", "PASS all C3 native checks; no runtime Console errors.",
            "PASS all ownership checks including nine owned native retaliation cases and fresh-character reset.",
            "PASS Exit Play mode with panel open: no runtime teardown errors.", "PASS: no runtime Console errors captured.",
            "PASS All Heavy experience model/native checks; no runtime Console errors.", "PASS All combat trade checks; no runtime Console errors.",
            "PASS All relay native checks including teardown; no runtime Console errors." };
        static ExtendedChecks() { EditorApplication.playModeStateChanged += Changed; EditorApplication.update += Tick; }

        [MenuItem("Tools/EvolvingRPG/Run Extended Regression Checks %#F11")]
        public static void Run()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Active, false))
                throw new InvalidOperationException("Use a stopped interactive Editor with no check already running.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty || string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                    throw new InvalidOperationException("Save scene edits before running extended checks.");
            SessionState.SetString("EvolvingRPG.Extended.Scene", SceneManager.GetActiveScene().path);
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Unity " + Application.unityVersion + " / interactive regression run " + DateTime.UtcNow.ToString("O") + "\n");
            SessionState.SetInt(Stage, 0); SessionState.SetBool(Active, true); Next();
        }
        static void Next()
        {
            int stage = SessionState.GetInt(Stage, 0);
            if (stage == Reports.Length) { Finish(true, "All requested stages passed. Technical checks do not validate feel."); return; }
            SessionState.SetBool("EvolvingRPG.Extended.Seen", false);
            SessionState.SetString("EvolvingRPG.Extended.Start", DateTime.UtcNow.ToString("O"));
            File.Delete("Logs/" + Reports[stage] + ".txt");
            try
            {
                // Focus before EnteredPlayMode callbacks that verify real Input System event edges.
                EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
                switch (stage)
                {
                    case 0: CombatPlayModeChecks.Run(); break;
                    case 1: TechniquePlayModeChecks.Run(); break;
                    case 2: EvasionEvidenceChecks.Run(); break;
                    case 3: EnemyPlayModeChecks.Run(); break;
                    case 4: PracticePlayModeChecks.Run(); break;
                    case 5: AttackSnapshotChecks.Run(); break;
                    case 6: DiscoveryChecks.Run(); break;
                    case 7: RetaliationChecks.Run(); break;
                    case 8: DevelopmentChecks.Run(); break;
                    case 9: DevelopmentPanelInputChecks.RunExitChecks(); break;
                    case 10:
                        EditorSceneManager.OpenScene(MovementPlaygroundBuilder.ScenePath);
                        StrafingPlayModeChecks.Run(); break;
                    case 11: HeavyExperienceChecks.Run(); break;
                    case 12: CombatTradeChecks.Run(); break;
                    case 13: ExpeditionChecks.Run(); break;
                }
            }
            catch (Exception e) { Finish(false, e.ToString()); }
        }
        static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Active, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) SessionState.SetBool("EvolvingRPG.Extended.Seen", true);
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool("EvolvingRPG.Extended.Seen", false)) return;
            // Let each suite finish writing its EnteredEditMode teardown result first.
            EditorApplication.delayCall += CompleteStage;
        }
        static void CompleteStage()
        {
            if (!SessionState.GetBool(Active, false)) return;
            int stage = SessionState.GetInt(Stage, 0); string path = "Logs/" + Reports[stage] + ".txt";
            string result = File.Exists(path) ? File.ReadAllText(path) : "Missing report";
            if (!result.Contains(Completed[stage]) || result.Contains("FAIL") || result.Contains("Exception:") || result.Contains("SKIP"))
            { Finish(false, Reports[stage] + ": incomplete, failed or skipped; inspect " + path); return; }
            File.AppendAllText(Report, "PASS " + Reports[stage] + "\n");
            SessionState.SetInt(Stage, stage + 1); EditorApplication.delayCall += Next;
        }
        static void Tick()
        {
            if (SessionState.GetBool(Active, false) && DateTime.TryParse(SessionState.GetString("EvolvingRPG.Extended.Start", ""), out var start)
                && (DateTime.UtcNow - start.ToUniversalTime()).TotalSeconds > 240)
            { Finish(false, "Stage timed out; inspect individual report."); if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); }
        }
        static void Finish(bool passed, string message)
        {
            SessionState.SetBool(Active, false);
            File.AppendAllText(Report, (passed ? "PASS " : "FAIL ") + message + "\n");
            if (passed)
            {
                string original = SessionState.GetString("EvolvingRPG.Extended.Scene", "");
                if (File.Exists(original)) EditorSceneManager.OpenScene(original);
                Debug.Log("Extended regression checks passed; original active scene restored.");
            }
            else Debug.LogError("Extended regression checks stopped: " + message);
        }
    }
}
