using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    [InitializeOnLoad]
    public static class PracticePlayModeChecks
    {
        private const string Requested = "EvolvingRPG.PracticeChecks";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> results = new List<string>();
        static PracticePlayModeChecks() => EditorApplication.playModeStateChanged += ModeChanged;
        [MenuItem("Tools/EvolvingRPG/Run Practice Play Mode Checks")]
        public static void Run() => RunMode(false);
        [MenuItem("Tools/EvolvingRPG/Inspect Practice Review Fixture %#&F10")]
        public static void InspectReview() => RunMode(true);
        private static void RunMode(bool inspect)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits first.");
            if (!File.Exists(PracticeSandboxBuilder.ScenePath)) PracticeSandboxBuilder.Create();
            EditorSceneManager.OpenScene(PracticeSandboxBuilder.ScenePath);
            SessionState.SetBool("EvolvingRPG.PracticeReviewInspect", inspect);
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        private static void ModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false); results.Clear();
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            CombatActions actor = null; GameObject dummy = null;
            CombatActionReview snapshotReview = null;
            Vector3 originalPosition = default; Quaternion originalFacing = Quaternion.identity;
            try
            {
                actor = Find<CombatActions>(); originalPosition = actor.transform.position; originalFacing = actor.transform.rotation;
                snapshotReview = new CombatActionReview(actor, actor.transform); snapshotReview.Attach();
                var practice = actor.GetComponent<HeavyPractice>(); var player = actor.GetComponent<CombatVitality>();
                var selector = actor.GetComponent<HeavyTechniqueSelector>();
                var heavy = actor.HeavyAttack; var light = actor.LightAttack; var guard = Guard(); var enemy = guard.GetComponent<EnemyVitality>();
                dummy = new GameObject("Temporary practice dummy"); var training = dummy.AddComponent<TrainingTarget>();
                practice.ResetPractice();
                for (int i = 1; i <= 100; i++) Complete(practice, i, heavy, training, new HitResolution(45, 40, false, false));
                Require(practice.Points == 0 && practice.QualifiedActions == 0, "dummy repetition awarded practice");
                int history = 0; foreach (var entry in practice.History) history++;
                Require(history == 8, "decision history is not bounded");
                Complete(practice, 101, light, enemy, Receipt(guard, 1));
                Require(practice.Points == 0, "Light awarded Heavy practice");
                Pass("Dummy repetition and Light give no Heavy credit; decision history stays bounded");

                practice.ResetPractice();
                var ordinary = Receipt(guard, 1);
                Begin(practice, 1, heavy); Hit(practice, 1, heavy, enemy, ordinary);
                Require(practice.Points == 0, "hit awarded before recovery completion");
                Observe(practice, new CombatObservation(1, CombatObservationKind.Interrupted, CombatActionKind.Attack, heavy, null, default, 0));
                Require(practice.Points == 0, "interrupted action awarded credit");
                Complete(practice, 2, heavy, enemy, ordinary); Require(practice.Points == 1, "completed meaningful Heavy did not award one point");
                Complete(practice, 3, heavy, enemy, ordinary); Require(practice.Points == 1, "same enemy strike farmed twice");
                Observe(practice, new CombatObservation(2, CombatObservationKind.Completed, CombatActionKind.Attack, heavy, null, default, 1));
                Require(practice.Points == 1 && practice.QualifiedActions == 1, "replayed completion awarded twice");
                Pass("Completion required; interruption discards pending contact; enemy-strike and action deduplication");

                practice.ResetPractice();
                Complete(practice, 1, heavy, enemy, Receipt(guard, 0));
                Complete(practice, 2, heavy, enemy, Receipt(guard, 1, started: Time.time - 9));
                Complete(practice, 3, heavy, enemy, Receipt(guard, 1, engaged: false));
                Complete(practice, 4, heavy, enemy, Receipt(guard, 1, phase: EnemyPhase.Returning));
                Require(practice.Points == 0, "idle/stale/foreign-source/leashed enemy awarded credit");
                Pass("No committed threat, stale threat, foreign source and returning opponent are rejected");

                practice.ResetPractice(); guard.ResetEnemy();
                Vector3 front = guard.transform.position + guard.transform.forward * 1.5f;
                Teleport(actor, front, Quaternion.LookRotation(-guard.transform.forward));
                Step(guard, 0); Require(guard.Phase == EnemyPhase.Windup, "could not start real windup");
                selector.TrySelect(1); var breaker = actor.HeavyAttack;
                enemy.ReceiveHit(new HitData(new AttackSnapshot(breaker), front, player), out var receipt);
                Require(receipt.EnemyContext.HasValue && receipt.EnemyContext.Value.PhaseBeforeHit == EnemyPhase.Windup
                    && receipt.EnemyContext.Value.EngagedWithSource && receipt.PoiseBroken && guard.Phase == EnemyPhase.Staggered,
                    "receipt did not freeze pre-impact windup/source context");
                Complete(practice, 1, breaker, enemy, receipt);
                Require(practice.Points == 2 && practice.GuardBreaks == 1 && practice.WindupInterrupts == 1,
                    "guard break + windup bonus stacked incorrectly");
                Pass("Authoritative receipt freezes pre-hit phase/source; guard-break/windup bonus is bounded to two");

                practice.ResetPractice();
                Begin(practice, 1, heavy); Hit(practice, 1, heavy, enemy, Receipt(guard, 1)); guard.ResetEnemy();
                Observe(practice, new CombatObservation(1, CombatObservationKind.Completed, CombatActionKind.Attack, heavy, null, default, 1));
                Require(practice.Points == 0, "opponent reset before recovery still awarded credit");
                Complete(practice, 2, heavy, enemy, Receipt(guard, 1)); Require(practice.Points == 1, "fresh valid trial failed");
                guard.ResetEnemy(); Complete(practice, 3, heavy, enemy, Receipt(guard, 1));
                Require(practice.Points == 1, "individual enemy reset farmed progress");
                player.ResetVitality(); Require(practice.Points == 0 && !practice.TrialEnabled, "player/encounter reset retained practice");
                Pass("Opponent reset invalidates pending credit and cannot farm the same trial; encounter reset starts fresh");

                for (int i = 1; i <= practice.TrialThreshold; i++) Complete(practice, i, heavy, enemy, Receipt(guard, i));
                Require(practice.TrialReady && practice.Points == practice.TrialThreshold, "trial threshold failed");
                selector.TrySelect(0); actor.GetComponent<ThirdPersonController>().Grounded = true;
                Require(actor.TryAttack(heavy), "could not start authored Heavy");
                Require(Near(actor.CommittedAttack.Recovery, heavy.Recovery), "trial applied while off");
                practice.ToggleTrial(); Require(Near(actor.CommittedAttack.Recovery, heavy.Recovery), "toggle rewrote committed attack");
                actor.Interrupt("next snapshot");
                Require(Near(snapshotReview.Latest.BaseRecovery, heavy.Recovery), "review rewrote pre-toggle committed recovery");
                Require(actor.TryAttack(heavy), "could not start resolved Heavy");
                Require(Near(actor.CommittedAttack.Recovery, heavy.Recovery * 0.92f) && Near(heavy.Recovery, 0.65f), "per-actor trial or asset isolation failed");
                Require(Near(practice.Resolve(light, new AttackSnapshot(light)).Recovery, light.Recovery), "trial affected Light");
                selector.TrySelect(2); // Busy selection remains rejected; get the authored definition directly for resolution testing.
                var momentum = AssetDatabase.LoadAssetAtPath<AttackDefinition>(TechniqueSandboxBuilder.DataPath + "/Momentum.asset");
                var resolved = practice.Resolve(momentum, new AttackSnapshot(momentum));
                Require(Near(resolved.MissRecovery, 0.55f) && Near(resolved.AdvanceDistance, 1.2f) && Near(resolved.StaminaCost, 20),
                    "proficiency changed miss tradeoff, advance or stamina");
                actor.Interrupt("disable trial component");
                Require(Near(snapshotReview.Latest.BaseRecovery, heavy.Recovery * .92f), "review did not retain resolved trial recovery");
                practice.enabled = false;
                Require(actor.TryAttack(heavy) && Near(actor.CommittedAttack.Recovery, heavy.Recovery), "disabled resolver still applied effect");
                actor.Interrupt("fixture done"); practice.enabled = true;
                Pass("Threshold and optional bounded recovery trial; snapshot isolation, untouched assets/Light/stamina/miss drawback, disabled-resolver fallback");
            }
            catch (Exception exception) { errors.Add(exception.ToString()); }
            finally
            {
                snapshotReview?.Dispose();
                if (dummy != null) UnityEngine.Object.DestroyImmediate(dummy);
                if (actor != null) { actor.Interrupt("checks finished"); Teleport(actor, originalPosition, originalFacing); }
                Application.logMessageReceived -= capture; Directory.CreateDirectory("Logs");
                string report = "Unity " + Application.unityVersion + " Heavy practice checks\n" + string.Join("\n", results)
                    + "\n" + (errors.Count == 0 ? "PASS: isolated groups; no runtime Console errors captured." : string.Join("\n", errors));
                File.WriteAllText("Logs/PracticePlayModeChecks.txt", report); Debug.Log(report);
                if (errors.Count == 0) BeginLive(actor); else EditorApplication.ExitPlaymode();
            }
        }
        private static void BeginLive(CombatActions actor)
        {
            var practice = actor.GetComponent<HeavyPractice>(); var selector = actor.GetComponent<HeavyTechniqueSelector>();
            var hud = Find<EnemySandboxHUD>(); var guard = Guard();
            bool prepared = false, attacked = false, background = Application.runInBackground, failed = false;
            int firstFrame = Time.frameCount; double deadline = EditorApplication.timeSinceStartup + 45;
            var errors = new List<string>(); Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture; Application.runInBackground = true;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Live practice check interrupted or timed out.");
                    if (Time.frameCount <= firstFrame) return;
                    if (!prepared)
                    {
                        hud.ResetEncounter(); Teleport(actor, guard.transform.position + guard.transform.forward * 3.5f, Quaternion.LookRotation(-guard.transform.forward));
                        Require(selector.TrySelect(1), "live Breaker selection failed"); prepared = true;
                    }
                    if (!attacked)
                    {
                        if (guard.Phase != EnemyPhase.Windup) return;
                        if (actor.Busy || !actor.GetComponent<ThirdPersonController>().Grounded) return;
                        Require(actor.TryAttack(actor.HeavyAttack), $"live Breaker could not start: {actor.Message}, stamina {actor.GetComponent<CombatStamina>().Current}, HP {actor.GetComponent<CombatVitality>().Health}"); attacked = true; return;
                    }
                    if (actor.Busy) return;
                    Require(practice.Points == 2 && practice.GuardBreaks == 1 && practice.WindupInterrupts == 1,
                        "real hit/completion did not yield bounded contextual credit: " + practice.Latest);
                    var entry = hud.PracticeReview?.Latest;
                    Require(entry != null && !entry.Interrupted && entry.PoiseBroken && entry.Contacts > 0
                        && Near(entry.BaseRecovery, actor.HeavyAttack.Recovery), "Practice HUD lost actual native attack commitment");
                    hud.ResetEncounter(); Require(practice.Points == 0 && !practice.TrialEnabled, "live reset did not clear practice");
                    Require(hud.PracticeReview.Latest == entry && hud.PracticeReview.Count <= 6, "practice reset erased review or history unbounded");
                    hud.enabled = false; hud.enabled = true;
                    Require(hud.PracticeReview.Latest == entry, "HUD disable/re-enable erased completed evidence");
                    if (SessionState.GetBool("EvolvingRPG.PracticeReviewInspect", false))
                    {
                        hud.SetOpponentDetails(false); hud.SetPracticeReviewVisible(true);
                        EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
                        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
                    }
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    File.AppendAllText("Logs/PracticePlayModeChecks.txt", "\nPASS Live native action: enemy commits, Breaker interrupts windup, recovery completes, two points awarded, R reset clears trial; HUD review retains actual commitment across reset/disable\nPASS: live practice; no runtime Console errors captured.\n");
                }
                catch (Exception exception) { failed = true; File.AppendAllText("Logs/PracticePlayModeChecks.txt", "\nFAIL Live practice: " + exception + "\n"); }
                Application.logMessageReceived -= capture; EditorApplication.update -= tick;
                Application.runInBackground = background;
                if (failed || !SessionState.GetBool("EvolvingRPG.PracticeReviewInspect", false)) EditorApplication.ExitPlaymode();
                else Debug.Log("Practice review fixture passed. World continues; stop Play when finished. No saves written.");
            };
            EditorApplication.update += tick;
        }
        private static HitResolution Receipt(SandboxEnemy enemy, int threat, EnemyPhase phase = EnemyPhase.Recovery,
            float? started = null, bool engaged = true)
            => new HitResolution(45, 40, false, false, false, true,
                new EnemyImpactContext(phase, enemy.EncounterRevision, threat, started ?? Time.time, engaged));
        private static void Begin(HeavyPractice practice, int sequence, AttackDefinition definition)
            => Observe(practice, new CombatObservation(sequence, CombatObservationKind.Accepted, CombatActionKind.Attack, definition, null, default, 0));
        private static void Hit(HeavyPractice practice, int sequence, AttackDefinition definition, HitReceiver target, HitResolution receipt)
            => Observe(practice, new CombatObservation(sequence, CombatObservationKind.HitResolved, CombatActionKind.Attack, definition, target, receipt, 1));
        private static void Complete(HeavyPractice practice, int sequence, AttackDefinition definition, HitReceiver target, HitResolution receipt)
        { Begin(practice, sequence, definition); Hit(practice, sequence, definition, target, receipt); Observe(practice, new CombatObservation(sequence, CombatObservationKind.Completed, CombatActionKind.Attack, definition, null, default, 1)); }
        private static void Observe(HeavyPractice practice, CombatObservation observation) => typeof(HeavyPractice).GetMethod("OnObservation", Private).Invoke(practice, new object[] { observation });
        private static void Step(SandboxEnemy enemy, float delta) => typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { delta });
        private static void Teleport(CombatActions actor, Vector3 position, Quaternion facing)
        { var motor = actor.GetComponent<CharacterController>(); motor.enabled = false; actor.transform.SetPositionAndRotation(position, facing); motor.enabled = true; Physics.SyncTransforms(); }
        private static T Find<T>() where T : Component
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var found = root.GetComponentInChildren<T>(); if (found != null) return found; } throw new InvalidOperationException("Missing " + typeof(T).Name); }
        private static SandboxEnemy Guard()
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var enemy = root.GetComponent<SandboxEnemy>(); if (enemy != null && enemy.GuardedArchetype) return enemy; } throw new InvalidOperationException("Missing guarded fighter"); }
        private static bool Near(float a, float b) => Mathf.Abs(a - b) < 0.001f;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) => results.Add("PASS " + message);
    }
}
