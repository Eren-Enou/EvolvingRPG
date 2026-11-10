using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EvolvingRPG.Editor
{
    // Command-line entry for an interactive (non-batch) Editor that THIS process owns:
    //   Unity.exe -projectPath <p> -executeMethod EvolvingRPG.Editor.AutomationRunner.Run -evolvingStages all|0,14,15 [-evolvingKeepOpen]
    // Never call it against an Editor the user is working in. It opens the saved RelayOutpost scene, runs the
    // requested Extended Checks stages, writes Logs/AutomationRunner.txt and exits with 0 (pass) or 1 (fail).
    [InitializeOnLoad]
    public static class AutomationRunner
    {
        const string Active = "EvolvingRPG.Automation.Active", Started = "EvolvingRPG.Automation.Started", Keep = "EvolvingRPG.Automation.Keep";
        const string ChecksStarted = "EvolvingRPG.Automation.ChecksStarted", Stages = "EvolvingRPG.Automation.Stages";
        const string Report = "Logs/AutomationRunner.txt";
        static AutomationRunner() => EditorApplication.update += Poll;

        public static void Run()
        {
            string stages = Argument("-evolvingStages") ?? "all";
            bool keep = Array.IndexOf(Environment.GetCommandLineArgs(), "-evolvingKeepOpen") >= 0;
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Automation " + DateTime.UtcNow.ToString("O") + " stages=" + stages + "\n");
            SessionState.SetBool(Active, true); SessionState.SetBool(Keep, keep);
            SessionState.SetBool(ChecksStarted, false);
            SessionState.SetString(Started, DateTime.UtcNow.ToString("O"));
            SessionState.SetString(Stages, stages);
        }
        static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
        static void Poll()
        {
            if (!SessionState.GetBool(Active, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (DateTime.TryParse(SessionState.GetString(Started, ""), out var start) && (DateTime.UtcNow - start.ToUniversalTime()).TotalMinutes > 90)
            { Finish(false, "Automation wall-clock limit reached."); return; }
            if (!SessionState.GetBool(ChecksStarted, false))
            {
                if (EditorApplication.isUpdating || EditorApplication.isCompiling) return;
                try
                {
                    string stages = SessionState.GetString(Stages, "all");
                    EditorSceneManager.OpenScene(ExpeditionSceneBuilder.ScenePath);
                    ExtendedChecks.RunStages(stages == "all" ? null : stages);
                    SessionState.SetBool(ChecksStarted, true);
                }
                catch (Exception e) { Finish(false, e.ToString()); }
                return;
            }
            if (ExtendedChecks.Running || !File.Exists("Logs/ExtendedChecks.txt")) return;
            string[] lines = File.ReadAllLines("Logs/ExtendedChecks.txt");
            if (lines.Length < 2) return;
            string last = lines[lines.Length - 1];
            if (last.StartsWith("PASS All requested", StringComparison.Ordinal)) Finish(true, last);
            else if (last.StartsWith("FAIL", StringComparison.Ordinal)) Finish(false, last);
        }
        static void Finish(bool passed, string message)
        {
            SessionState.SetBool(Active, false);
            File.AppendAllText(Report, (passed ? "PASS " : "FAIL ") + message + "\n");
            if (!SessionState.GetBool(Keep, false)) EditorApplication.Exit(passed ? 0 : 1);
        }
    }
}
