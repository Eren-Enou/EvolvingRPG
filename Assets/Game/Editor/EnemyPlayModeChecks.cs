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
    public static class EnemyPlayModeChecks
    {
        private const string Requested = "EvolvingRPG.EnemyChecksRequested";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> results = new List<string>();
        static EnemyPlayModeChecks() => EditorApplication.playModeStateChanged += ModeChanged;
        [MenuItem("Tools/EvolvingRPG/Run Enemy Play Mode Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before running enemy checks.");
            if (!File.Exists(EnemySandboxBuilder.ScenePath)) EnemySandboxBuilder.Create();
            EditorSceneManager.OpenScene(EnemySandboxBuilder.ScenePath);
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
            GameObject wall = null;
            CombatActions checkedActor = null;
            Vector3 originalPosition = default;
            Quaternion originalFacing = Quaternion.identity;
            try
            {
                CombatActions actions = null; SandboxEnemy guard = null, fast = null;
                foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (actions == null) actions = root.GetComponentInChildren<CombatActions>();
                    var enemy = root.GetComponent<SandboxEnemy>();
                    if (enemy != null) { if (enemy.GuardedArchetype) guard = enemy; else fast = enemy; }
                }
                Require(actions != null && guard != null && fast != null, "missing encounter components");
                checkedActor = actions; originalPosition = actions.transform.position; originalFacing = actions.transform.rotation;
                var player = actions.GetComponent<CombatVitality>();
                var health = guard.GetComponent<EnemyVitality>();
                var light = new AttackSnapshot(actions.LightAttack); var heavy = new AttackSnapshot(actions.HeavyAttack);
                Vector3 front = guard.transform.position + guard.transform.forward * 1.5f;
                Teleport(player, front); actions.transform.rotation = Quaternion.LookRotation(guard.transform.position - front);
                var targets = new HashSet<HitReceiver>();
                var detector = actions.GetComponent<MeleeHitDetector>();
                HitResolution receipt = default;
                Require(detector.ResolveSwing(heavy, actions.transform.rotation, targets, (target, result) => receipt = result) == 1,
                    "generic receiver detection did not hit enemy");
                Require(receipt.Guarded && Near(receipt.DamageApplied, 9f) && Near(health.Poise, 40f), "frontal guard or poise impact incorrect");
                Require(player.Health == player.MaximumHealth, "attacker struck its own receiver");
                Require(detector.ResolveSwing(heavy, actions.transform.rotation, targets) == 0, "enemy hit twice per swing");
                Pass("Generic enemy receiver, frontal guard, Heavy poise and per-swing deduplication");

                health.ReceiveHit(new HitData(heavy, front), out receipt);
                Require(receipt.PoiseBroken && receipt.PoiseApplied == 20 && health.Staggered && guard.Phase == EnemyPhase.Staggered && !guard.CanGuard,
                    "Heavy did not break guard and interrupt enemy");
                health.ReceiveHit(new HitData(light, front), out receipt);
                Require(receipt.Vulnerable && Near(receipt.DamageApplied, 18.75f) && receipt.PoiseApplied == 0,
                    "stagger opening bonus or applied poise incorrect");
                Pass("Guard break interrupts enemy and creates a vulnerable stagger window");

                guard.ResetEnemy();
                health.ReceiveHit(new HitData(heavy, guard.transform.position - guard.transform.forward * 1.5f), out receipt);
                Require(!receipt.Guarded && Near(receipt.DamageApplied, 45), "rear hit was guarded");
                Pass("Positioning bypasses frontal guard");

                guard.ResetEnemy(); player.ResetVitality(); Teleport(player, front);
                Step(guard, 0); Require(guard.Phase == EnemyPhase.Windup && guard.AttacksStarted == 1, "guard did not commit");
                Step(guard, 0.8f); Require(player.Health == player.MaximumHealth && guard.Phase == EnemyPhase.Windup, "guard hit before windup ended");
                Step(guard, 0.06f); Require(player.Health == player.MaximumHealth - 18 && guard.HitsLanded == 1, "active strike failed");
                Step(guard, 0.05f); Require(guard.HitsLanded == 1, "active strike hit repeatedly");
                Step(guard, 0.2f); Require(guard.Phase == EnemyPhase.Recovery && !guard.CanGuard, "no recovery opening");
                health.ReceiveHit(new HitData(heavy, front), out receipt);
                Require(receipt.Vulnerable && Near(receipt.DamageApplied, 56.25f), "recovery hit did not exploit opening");
                Pass("Telegraph, delayed single strike, recovery and opening damage");

                guard.ResetEnemy(); player.ResetVitality(); Teleport(player, front); Step(guard, 0);
                Quaternion facing = guard.transform.rotation;
                Teleport(player, guard.transform.position - guard.transform.forward * 1.5f); Step(guard, 0.9f);
                Require(player.Health == player.MaximumHealth && Quaternion.Angle(facing, guard.transform.rotation) < 0.01f, "committed strike tracked player behind it");
                Pass("Enemy facing commits; moving behind the telegraph avoids its strike");

                guard.ResetEnemy(); Teleport(player, front); Step(guard, 0);
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = guard.transform.position + guard.transform.forward * 0.75f + Vector3.up;
                wall.transform.localScale = new Vector3(2, 2, 0.15f); Physics.SyncTransforms();
                Step(guard, 0.9f); Require(player.Health == player.MaximumHealth, "enemy hit through solid wall");
                UnityEngine.Object.DestroyImmediate(wall); wall = null;
                Pass("Solid geometry blocks committed enemy strikes");

                fast.ResetEnemy(); guard.ResetEnemy(); player.ResetVitality();
                Teleport(player, fast.transform.position + fast.transform.forward * 1.5f); Step(fast, 0); Step(fast, 0.5f);
                Require(fast.Phase == EnemyPhase.Active && player.Health == player.MaximumHealth - 12, "fast fighter has no distinct quick opening");
                Pass("Fast fighter commits and hits sooner for less impact");

                player.ResetVitality(); actions.Interrupt("fixture ready"); actions.Diagnostics.Clear();
                actions.GetComponent<ThirdPersonController>().Grounded = true;
                Require(actions.TryAttack(actions.HeavyAttack), "could not start player Heavy");
                player.ReceiveHit(new HitData("Impact", 1, 20, Color.red, front), out _);
                Require(actions.Busy, "ordinary damage cancelled committed attack");
                player.ReceiveHit(new HitData("Impact", 1, 20, Color.red, front), out receipt);
                Require(receipt.PoiseBroken && !actions.Busy && actions.MovementBlocked && actions.Diagnostics.Interrupted == 1,
                    "player poise break failed to interrupt/block");
                Require(!actions.TryAttack(actions.LightAttack) && !actions.TryDodge(), "staggered player accepted action");
                var movement = actions.GetComponent<CombatMovement>();
                typeof(CombatMovement).GetMethod("Update", Private).Invoke(movement, null);
                Require(actions.GetComponent<ThirdPersonController>().MoveSpeed == 0, "staggered player could move"); movement.Restore();
                Pass("Player poise break interrupts once and blocks movement/actions; ordinary hits retain commitment");

                player.ReceiveHit(new HitData("Defeat", 1000, 0, Color.red, front), out receipt);
                Require(player.Defeated && receipt.DamageApplied < 1000 && !player.ReceiveHit(new HitData(light), out _), "defeat/overkill invalid");
                player.ResetVitality(); Require(!actions.MovementBlocked && player.Health == player.MaximumHealth, "reset failed to restore player");
                Pass("Player defeat, overkill cap, dead receiver rejection and reset");

                guard.ResetEnemy();
                for (int i = 0; i < 3; i++) health.ReceiveHit(new HitData(light, front), out _);
                Require(guard.Bracing, "repeated light pressure was not observed");
                health.ReceiveHit(new HitData(light, front), out receipt);
                Require(receipt.Guarded && Near(receipt.DamageApplied, 1.5f), "guard did not brace after repeated pressure");
                guard.ResetEnemy();
                Vector3 flank = guard.transform.position + guard.transform.right * 1.5f;
                health.ReceiveHit(new HitData(light, flank), out _); health.ReceiveHit(new HitData(light, flank), out _);
                Require(guard.AlertToFlank, "repeated same-side flank was not observed");
                typeof(SandboxEnemy).GetField("lastPatternAt", Private).SetValue(guard, Time.time - 10);
                Step(guard, 0); Require(!guard.AlertToFlank && !guard.Bracing, "pattern memory did not expire");
                guard.ResetEnemy(); Require(guard.Phase == EnemyPhase.Idle && health.Health == health.MaximumHealth && guard.AttacksStarted == 0,
                    "encounter reset retained behavior state");
                Pass("Repeated light/flank reactions, expiring encounter memory and reset isolation");

                fast.ResetEnemy(); var fastHealth = fast.GetComponent<EnemyVitality>();
                Vector3 fastFront = fast.transform.position + fast.transform.forward * 1.5f;
                for (int i = 0; i < 3; i++) fastHealth.ReceiveHit(new HitData(light, fastFront), out _);
                typeof(CombatVitality).GetField("staggerUntil", Private).SetValue(fastHealth, Time.time - 1);
                Step(fast, 0); Vector3 before = fast.transform.position; Step(fast, 0.1f);
                Require(Vector3.Dot(fast.transform.position - before, -fast.transform.forward) > 0.1f, "fast fighter did not retreat after repeated light pressure");
                Pass("Fast fighter retreats during recovery after repeated light pressure");
            }
            catch (Exception exception) { errors.Add(exception.ToString()); }
            finally
            {
                if (wall != null) UnityEngine.Object.DestroyImmediate(wall);
                if (checkedActor != null)
                {
                    Teleport(checkedActor.GetComponent<CombatVitality>(), originalPosition);
                    checkedActor.transform.rotation = originalFacing;
                }
                Application.logMessageReceived -= capture;
                Directory.CreateDirectory("Logs");
                string report = "Unity " + Application.unityVersion + " isolated enemy Play-mode checks\n" + string.Join("\n", results)
                    + "\n" + (errors.Count == 0 ? "PASS: all checks; no runtime Console errors captured." : string.Join("\n", errors));
                File.WriteAllText("Logs/EnemyPlayModeChecks.txt", report); Debug.Log(report);
                if (errors.Count == 0) BeginLiveSmoke(); else EditorApplication.ExitPlaymode();
            }
        }
        private static void BeginLiveSmoke()
        {
            // Exercise normal Start/Update/LateUpdate and physics as well as the isolated phase assertions.
            CombatActions player = null; SandboxEnemy guard = null; EnemySandboxHUD hud = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (player == null) player = root.GetComponentInChildren<CombatActions>();
                var enemy = root.GetComponent<SandboxEnemy>();
                if (enemy != null && enemy.GuardedArchetype) guard = enemy;
                if (hud == null) hud = root.GetComponent<EnemySandboxHUD>();
            }
            float started = Time.time;
            double deadline = EditorApplication.timeSinceStartup + 45;
            bool prepared = false, background = Application.runInBackground;
            int firstFrame = Time.frameCount;
            Application.runInBackground = true;
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying) throw new InvalidOperationException("Live smoke interrupted.");
                    if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Live smoke did not receive enough game frames.");
                    if (Time.frameCount <= firstFrame) return;
                    if (!prepared)
                    {
                        hud.ResetEncounter();
                        Teleport(player.GetComponent<CombatVitality>(), guard.transform.position + guard.transform.forward * 3.5f);
                        started = Time.time; prepared = true;
                    }
                    if (Time.time - started < 4f) return;
                    var health = player.GetComponent<CombatVitality>();
                    Require(guard.AttacksStarted > 0 && guard.HitsLanded > 0 && health.Health < health.MaximumHealth,
                        "normal frame loop did not approach, telegraph and hit player: " + guard.Phase + " attacks=" + guard.AttacksStarted + " hits=" + guard.HitsLanded + " actor=" + player.transform.position + " enemy=" + guard.transform.position);
                    hud.ResetEncounter();
                    Require(health.Health == health.MaximumHealth && guard.Phase == EnemyPhase.Idle && guard.AttacksStarted == 0,
                        "live encounter reset failed");
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    File.AppendAllText("Logs/EnemyPlayModeChecks.txt", "\nPASS Normal frame loop: approach, telegraph, strike, player damage, visuals/HUD and full encounter reset\nPASS: live smoke; no runtime Console errors captured.\n");
                }
                catch (Exception exception)
                { File.AppendAllText("Logs/EnemyPlayModeChecks.txt", "\nFAIL Live smoke: " + exception + "\n"); }
                EditorApplication.update -= tick; Application.logMessageReceived -= capture;
                Application.runInBackground = background; EditorApplication.ExitPlaymode();
            };
            EditorApplication.update += tick;
        }
        private static void Step(SandboxEnemy enemy, float delta) => typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { delta });
        private static void Teleport(CombatVitality player, Vector3 position)
        {
            var motor = player.GetComponent<CharacterController>(); motor.enabled = false; player.transform.position = position; motor.enabled = true; Physics.SyncTransforms();
        }
        private static bool Near(float a, float b) => Mathf.Abs(a - b) < 0.01f;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) => results.Add("PASS " + message);
    }
}
