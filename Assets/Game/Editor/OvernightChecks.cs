using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EvolvingRPG.Editor
{
    // Exclusive batch runner. Never invoked automatically on script import.
    [InitializeOnLoad]
    public static class OvernightChecks
    {
        const string Active = "EvolvingRPG.Overnight.Active";
        const string Stage = "EvolvingRPG.Overnight.Stage";
        const string Seen = "EvolvingRPG.Overnight.SeenPlay";
        static readonly string[] Reports = { "EvasionEvidenceChecks", "CombatPlayModeChecks", "TechniquePlayModeChecks", "EnemyPlayModeChecks", "PracticePlayModeChecks", "AttackSnapshotChecks" };
        static OvernightChecks() { EditorApplication.playModeStateChanged += Changed; EditorApplication.update += Tick; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void PrepareBatchDevices()
        {
            if (!SessionState.GetBool(Active, false)) return;
            Time.captureDeltaTime = 1f / 60;
            Application.runInBackground = true;
            if (UnityEngine.InputSystem.Keyboard.current == null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            if (UnityEngine.InputSystem.Mouse.current == null) UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();
        }
        public static void RunHeadless() { SessionState.SetBool("EvolvingRPG.Overnight.SkipGUI", true); Run(); }
        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Use individual menus in an interactive Editor.");
            Directory.CreateDirectory("Logs");
            SessionState.SetBool(Active, true); SessionState.SetInt(Stage, 0);
            Next();
        }
        static void Next()
        {
            int i = SessionState.GetInt(Stage, 0);
            if (i == Reports.Length) { SessionState.SetBool(Active, false); EditorApplication.Exit(0); return; }
            File.Delete("Logs/" + Reports[i] + ".txt");
            SessionState.SetBool(Seen, false);
            SessionState.SetString("EvolvingRPG.Overnight.Started", DateTime.UtcNow.ToString("O"));
            try
            {
                switch (i)
                {
                    case 0: EvasionEvidenceChecks.Run(); break;
                    case 1: CombatPlayModeChecks.Run(); break;
                    case 2: TechniquePlayModeChecks.Run(); break;
                    case 3: EnemyPlayModeChecks.Run(); break;
                    case 4: PracticePlayModeChecks.Run(); break;
                    case 5: AttackSnapshotChecks.Run(); break;
                }
            }
            catch (Exception e) { Debug.LogException(e); Stop(2); }
        }
        static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Active, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode) SessionState.SetBool(Seen, true);
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Seen, false)) return;
            int i = SessionState.GetInt(Stage, 0);
            string path = "Logs/" + Reports[i] + ".txt";
            if (!File.Exists(path) || File.ReadAllText(path).Contains("FAIL") || File.ReadAllText(path).Contains("Exception:") || !File.ReadAllText(path).Contains("PASS")) { Stop(3); return; }
            SessionState.SetInt(Stage, i == 0 && SessionState.GetBool("EvolvingRPG.Overnight.SkipGUI", false) ? 2 : i + 1);
            EditorApplication.delayCall += Next;
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Active, false)) return;
            if (DateTime.TryParse(SessionState.GetString("EvolvingRPG.Overnight.Started", ""), out var started)
                && (DateTime.UtcNow - started.ToUniversalTime()).TotalSeconds > 240)
            { Debug.LogError("Overnight check stage timed out"); Stop(4); }
        }
        static void Stop(int code) { SessionState.SetBool(Active, false); EditorApplication.Exit(code); }
    }
}
