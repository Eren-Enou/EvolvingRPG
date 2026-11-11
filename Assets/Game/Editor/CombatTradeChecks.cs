using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // Explicit runtime-only comparison. No scene authoring, personal saves or new combat rules.
    [InitializeOnLoad]
    public static class CombatTradeChecks
    {
        const string Requested = "EvolvingRPG.CombatTradeChecks", Inspect = "EvolvingRPG.CombatTradeInspect";
        const string Report = "Logs/CombatTradeChecks.txt";
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static CombatTradeChecks() => EditorApplication.playModeStateChanged += Changed;
        [MenuItem("Tools/EvolvingRPG/Run Combat Trade Checks %#&F8")]
        public static void Run() => RunMode(false);
        [MenuItem("Tools/EvolvingRPG/Inspect Combat Trade Fixture %#&F9")]
        public static void InspectFixture() => RunMode(true);
        static void RunMode(bool inspect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty || string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                    throw new InvalidOperationException("Save scene edits first.");
            EditorSceneManager.OpenScene(DevelopmentSandboxBuilder.ScenePath);
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report, "Unity " + Application.unityVersion + " / native trade comparison " + DateTime.UtcNow.ToString("O") + "\n");
            SessionState.SetBool(Inspect, inspect); SessionState.SetBool(Requested, true);
            EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false);
            try { Begin(); }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); EditorApplication.ExitPlaymode(); }
        }
        static void Begin()
        {
            var owner = Find<OwnedCharacter>(); var actions = owner.GetComponent<CombatActions>();
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs", "CombatTradeSaves", Guid.NewGuid().ToString("N"))));
            var vitality = owner.GetComponent<CombatVitality>(); var inputs = owner.GetComponent<StarterAssetsInputs>();
            var motor = owner.GetComponent<ThirdPersonController>(); var hud = Find<EnemySandboxHUD>();
            SandboxEnemy enemy = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var candidate = root.GetComponent<SandboxEnemy>(); if (candidate != null && candidate.GuardedArchetype) enemy = candidate; }
            Require(enemy != null, "guarded opponent missing");
            var home = typeof(SandboxEnemy).GetField("home", Private); var originalHome = (Vector3)home.GetValue(enemy);
            float enemyPoise = (float)typeof(SandboxEnemy).GetField("poiseDamage", Private).GetValue(enemy);
            Require(enemyPoise > 0 && enemyPoise < vitality.PoiseThreshold, "fixture needs a non-staggering first strike");
            Require(actions.HeavyAttack.PoiseDamage < enemy.GetComponent<EnemyVitality>().PoiseThreshold,
                "fixture Heavy would break guard before its strike; review comparison parameters");
            float oldStep = Time.captureDeltaTime; bool oldBackground = Application.runInBackground;
            Time.captureDeltaTime = 1f / 60; Application.runInBackground = true;
            bool inspect = SessionState.GetBool(Inspect, false);
            int stage = 0, test = 0, frame = -1; float wait = Time.time;
            double deadline = EditorApplication.timeSinceStartup + 60;
            var errors = new List<string>(); var entries = new List<CombatActionReview.Entry>(2);
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                bool finished = false, failed = false;
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
                        throw new TimeoutException("Trade case " + test + " stage " + stage);
                    if (frame == Time.frameCount) return; frame = Time.frameCount;
                    inputs.move = inputs.look = Vector2.zero; inputs.jump = inputs.sprint = false;
                    if (stage == 3)
                    {
                        Require(owner.MenuOpen, "inspection panel closed after opening: focused=" + Application.isFocused
                            + ", safe=" + owner.CanDevelop + ", phase=" + actions.Phase);
                        if (Time.time - wait < .1f) return;
                        if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                        File.AppendAllText(Report, "PASS Inspection panel remains open across runtime frames at safe spawn.\n");
                        File.AppendAllText(Report, "PASS All combat trade checks; no runtime Console errors. Results do not judge whether trading was worthwhile.\n");
                        finished = true;
                    }
                    else
                    {
                    if (stage == 0)
                    {
                        if (Time.time - wait < .3f) return;
                        home.SetValue(enemy, originalHome); hud.ResetEncounter();
                        Require(owner.State.Heavy == DevelopmentIds.Baseline && owner.State.Primary == "", "fixture inherited a discipline");
                        Teleport(owner.gameObject, new Vector3(0, .05f, -12), Quaternion.identity);
                        Teleport(enemy.gameObject, owner.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(Vector3.back));
                        home.SetValue(enemy, enemy.transform.position); Physics.SyncTransforms();
                        if (test == 1)
                        {
                            float preload = vitality.PoiseThreshold - enemyPoise;
                            vitality.ReceiveHit(new HitData("Initial poise fixture", 0, preload, Color.gray, enemy.transform.position), out _);
                            Require(!vitality.Staggered && vitality.Health == vitality.MaximumHealth, "preload damaged or staggered player");
                            File.AppendAllText(Report, "SETUP second case preloads " + preload + " poise before acceptance; not a natural-play observation.\n");
                        }
                        typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { 0f });
                        stage = 1; return;
                    }
                    if (stage == 1)
                    {
                        if (!enemy.CurrentThreat.HasValue || Time.time - enemy.CurrentThreat.Value.CommittedAt < .1f) return;
                        Require(enemy.Phase == EnemyPhase.Windup, "comparison missed committed windup");
                        motor.Grounded = true; Require(actions.TryAttack(actions.HeavyAttack), "native Heavy rejected: " + actions.Message);
                        stage = 2; return;
                    }
                    if (actions.Busy) return;
                    var entry = owner.Review.Latest;
                    Require(entry != null && entry.Contacts > 0 && entry.Damage > 0 && entry.ReceivedHits == 1
                        && entry.DamageTaken > 0 && entry.PoiseTaken > 0 && entry.FirstImpactPhase == CombatPhase.Recovery,
                        "native trade did not include landed Heavy plus actual recovery impact: " + entry?.Summary);
                    Require(entry.Interrupted == (test == 1) && entry.Staggered == (test == 1) && !entry.Defeated,
                        "fresh versus preloaded poise did not separate completion and interruption");
                    Require(entry.InterruptionReason == (test == 1 ? "Staggered" : ""), "native trade lost actual interruption cause");
                    Require(owner.State.ExperienceCount == 1 && owner.State.Balance == 1 && owner.State.BuildRevision == 0,
                        "trade changed progression beyond the one completed Baseline contact fact");
                    string explanation = owner.Experience.ExplanationFor(entry.Sequence);
                    Require(explanation != null && explanation.Contains(test == 0 ? "demonstrated" : "interrupted"),
                        "experience explanation does not describe trade completion rule");
                    entries.Add(entry);
                    File.AppendAllText(Report, "PASS Native " + (test == 0 ? "completed trade" : "landed but interrupted trade") + ": " + entry.Summary + " | " + explanation + "\n");
                    if (++test < 2) { stage = 0; wait = Time.time; return; }
                    home.SetValue(enemy, originalHome); hud.ResetEncounter();
                    Require(owner.Review.Count == 2 && owner.Review.Latest == entries[1]
                        && entries[0].ReceivedHits == 1 && owner.Experience.ExplanationFor(entries[0].Sequence) != null,
                        "R lost either trade's immutable evidence");
                    if (inspect)
                    {
                        EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
                        Require(owner.TryOpenMenu(), "inspection panel failed safe-spawn gate");
                        typeof(DevelopmentPanel).GetField("scroll", Private).SetValue(Find<DevelopmentPanel>(), new Vector2(0, 10000));
                        stage = 3; wait = Time.time; return;
                    }
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    File.AppendAllText(Report, "PASS All combat trade checks; no runtime Console errors. Results do not judge whether trading was worthwhile.\n");
                    finished = true;
                    }
                }
                catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); finished = failed = true; }
                if (!finished) return;
                EditorApplication.update -= tick; Application.logMessageReceived -= capture;
                home.SetValue(enemy, originalHome); Time.captureDeltaTime = oldStep; Application.runInBackground = oldBackground;
                if (failed) hud.ResetEncounter();
                if (!inspect || failed) EditorApplication.ExitPlaymode();
                else Debug.Log("Trade comparison passed. Two real attack results remain inspectable; stop Play when finished. No saves written.");
            };
            EditorApplication.update += tick;
        }
        static void Teleport(GameObject actor, Vector3 position, Quaternion facing)
        { var body = actor.GetComponent<CharacterController>(); body.enabled = false; actor.transform.SetPositionAndRotation(position, facing); body.enabled = true; }
        static T Find<T>() where T : Component
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; } throw new InvalidOperationException("Missing " + typeof(T).Name); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
