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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace EvolvingRPG.Editor
{
    [InitializeOnLoad]
    public static class TechniquePlayModeChecks
    {
        private const string Requested = "EvolvingRPG.TechniqueChecks";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> results = new List<string>();
        static TechniquePlayModeChecks() => EditorApplication.playModeStateChanged += ModeChanged;
        [MenuItem("Tools/EvolvingRPG/Run Technique Play Mode Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits first.");
            if (!File.Exists(TechniqueSandboxBuilder.ScenePath)) TechniqueSandboxBuilder.Create();
            EditorSceneManager.OpenScene(TechniqueSandboxBuilder.ScenePath);
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        private static void ModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false); results.Clear();
            CombatActions actor = Find<CombatActions>();
            var selector = actor.GetComponent<HeavyTechniqueSelector>();
            Vector3 originalPosition = actor.transform.position; Quaternion originalFacing = actor.transform.rotation;
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            AttackDefinition temporary = null;
            try
            {
                CheckBinding(selector, "chooseBaseline", Key.Digit1);
                CheckBinding(selector, "chooseBreaker", Key.Digit2);
                CheckBinding(selector, "chooseMomentum", Key.Digit3);
                CheckNumberEvents(selector);
                Pass("Number-row 1/2/3 bindings resolve to real keyboard controls");
                var baseline = actor.HeavyAttack; var light = actor.LightAttack;
                Require(selector.TrySelect(0) && selector.SelectedDefinition == baseline && baseline.AdvanceDistance == 0
                    && baseline.MissRecovery == 0 && baseline.StaminaCost == 20, "baseline changed");
                Require(selector.TrySelect(1), "could not select Breaker"); var breaker = actor.HeavyAttack;
                Require(breaker.Windup > baseline.Windup && breaker.Recovery > baseline.Recovery
                    && breaker.PoiseDamage > baseline.PoiseDamage && breaker.AdvanceDistance == 0 && breaker.StaminaCost == baseline.StaminaCost,
                    "Breaker has no commitment/poise tradeoff");
                Require(selector.TrySelect(2), "could not select Momentum"); var momentum = actor.HeavyAttack;
                Require(momentum.AdvanceDistance == 1.2f && momentum.MissRecovery == 0.55f && momentum.PoiseDamage < baseline.PoiseDamage
                    && momentum.Damage < baseline.Damage && momentum.StaminaCost == baseline.StaminaCost && actor.LightAttack == light,
                    "Momentum tradeoff or Light/cost preservation failed");
                Pass("Baseline preserved; two behavioral tradeoffs; Light and Heavy stamina costs unchanged");

                actor.GetComponent<ThirdPersonController>().Grounded = true;
                Require(actor.TryAttack(momentum), "could not commit Momentum");
                var committed = actor.CommittedAttack;
                Require(!selector.TrySelect(1) && selector.Selected == 2 && actor.CurrentAttack == momentum,
                    "selection altered committed action");
                TickAt(actor, committed.Duration + 0.02f);
                Require(actor.Busy && actor.Phase == CombatPhase.Recovery, "miss penalty was skipped");
                TickAt(actor, committed.Duration + committed.MissRecovery + 0.02f);
                Require(!actor.Busy && selector.Selected == 2 && actor.Diagnostics.Misses == 1, "miss completion or no-queue selection failed");
                Pass("Miss adds recovery; switching during commitment is rejected without queueing");

                // A real enemy receiver accepts the swing; guarded hits still count as hits.
                var guard = FindGuard(); guard.ResetEnemy();
                Teleport(actor, guard.transform.position + guard.transform.forward * 1.5f,
                    Quaternion.LookRotation(-guard.transform.forward));
                Require(actor.TryAttack(momentum), "could not start hit Momentum");
                TickAt(actor, committed.Windup + 0.01f);
                Require(actor.Diagnostics.Hits == 1, "committed hit did not resolve");
                TickAt(actor, committed.Duration + 0.02f);
                Require(!actor.Busy && actor.Diagnostics.Misses == 1, "successful hit inherited miss penalty");
                Pass("Resolved enemy hit uses ordinary recovery, including guarded contact");

                selector.TrySelect(1); guard.ResetEnemy();
                guard.GetComponent<EnemyVitality>().ReceiveHit(new HitData(new AttackSnapshot(breaker), actor.transform.position), out var receipt);
                Require(receipt.PoiseBroken && receipt.Guarded, "Breaker did not break fresh frontal guard in one hit");
                Pass("Breaker disrupts a fresh guarded fighter in one committed impact");

                temporary = UnityEngine.Object.Instantiate(momentum);
                temporary.StaminaCost = 0;
                actor.Interrupt("snapshot fixture");
                Require(actor.TryAttack(temporary), "could not start snapshot fixture");
                temporary.AdvanceDistance = 8; temporary.MissRecovery = 8;
                Require(actor.CommittedAttack.AdvanceDistance == 1.2f && actor.CommittedAttack.MissRecovery == 0.55f,
                    "new parameters were not snapshotted");
                actor.Interrupt("interrupted fixture");
                Require(actor.AttackAdvanceSpeedThisFrame == 0 && !actor.HasCommittedAttack,
                    "interruption retained advancement");
                Require(actor.TryDodge() && !selector.TrySelect(0) && actor.AttackAdvanceSpeedThisFrame == 0,
                    "dodge inherited technique advancement or allowed switching");
                actor.Interrupt("fixture complete");
                Pass("Advance/miss values commit immutably; interruption clears motion; dodge remains separate");
            }
            catch (Exception exception) { errors.Add(exception.ToString()); }
            finally
            {
                if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
                actor.Interrupt("checks complete"); Teleport(actor, originalPosition, originalFacing);
                Application.logMessageReceived -= capture;
                Directory.CreateDirectory("Logs");
                string report = "Unity " + Application.unityVersion + " technique Play-mode checks\n" + string.Join("\n", results)
                    + "\n" + (errors.Count == 0 ? "PASS: isolated groups; no runtime Console errors captured." : string.Join("\n", errors));
                File.WriteAllText("Logs/TechniquePlayModeChecks.txt", report); Debug.Log(report);
                if (errors.Count == 0) BeginMotorChecks(actor, selector); else EditorApplication.ExitPlaymode();
            }
        }
        private static void BeginMotorChecks(CombatActions actor, HeavyTechniqueSelector selector)
        {
            int test = 0, frame = -1; bool prepared = false;
            float started = Time.time, originalStep = Time.captureDeltaTime;
            bool background = Application.runInBackground;
            double deadline = EditorApplication.timeSinceStartup + 60;
            Vector3 position = default; GameObject wall = null;
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture; Application.runInBackground = true;
            var hud = Find<EnemySandboxHUD>();
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Motor checks interrupted or timed out.");
                    if (Time.frameCount == frame) return; frame = Time.frameCount;
                    if (!prepared)
                    {
                        if (actor.Busy || Time.time - started < 0.4f) return;
                        hud.ResetEncounter();
                        Teleport(actor, new Vector3(0, 0.05f, -12), Quaternion.identity);
                        actor.GetComponent<ThirdPersonController>().Grounded = true;
                        Require(selector.TrySelect(2), "live selector could not choose Momentum");
                        Time.captureDeltaTime = test == 1 ? 1f / 15 : 1f / 60;
                        if (test == 2)
                        {
                            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            wall.transform.position = actor.transform.position + new Vector3(0, 1, 0.9f);
                            wall.transform.localScale = new Vector3(3, 2, 0.15f); Physics.SyncTransforms();
                        }
                        position = actor.transform.position;
                        Require(actor.TryAttack(actor.HeavyAttack), "live Momentum could not start");
                        prepared = true; started = Time.time;
                    }
                    if (actor.Busy) return;
                    float elapsed = Time.time - started;
                    Vector3 displacement = Vector3.ProjectOnPlane(actor.transform.position - position, Vector3.up);
                    Require(Quaternion.Angle(actor.transform.rotation, Quaternion.identity) < 0.1f, "advancing attack rotated actor");
                    if (test < 2) Require(Mathf.Abs(displacement.z - 1.2f) < 0.08f && Mathf.Abs(displacement.x) < 0.03f,
                        $"advance distance at simulation step {Time.captureDeltaTime}: {displacement}");
                    else Require(displacement.z > 0.1f && displacement.z < 0.8f, "advance passed through wall: " + displacement);
                    Require(elapsed > 1.4f && elapsed < 1.8f && actor.Diagnostics.Misses == 1, "live miss recovery duration changed: " + elapsed);
                    File.AppendAllText("Logs/TechniquePlayModeChecks.txt", "\nPASS Native motor: " + (test == 2 ? "wall shortens advancement" : test == 1 ? "1.2m advance at 15Hz simulation" : "1.2m advance at 60Hz simulation") + "; fixed facing and miss recovery");
                    if (wall != null) { UnityEngine.Object.DestroyImmediate(wall); wall = null; }
                    if (++test < 3) { prepared = false; started = Time.time; return; }
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    File.AppendAllText("Logs/TechniquePlayModeChecks.txt", "\nPASS: all motor checks; no runtime Console errors captured.\n");
                }
                catch (Exception exception) { File.AppendAllText("Logs/TechniquePlayModeChecks.txt", "\nFAIL Motor checks: " + exception + "\n"); }
                if (wall != null) UnityEngine.Object.DestroyImmediate(wall);
                Application.logMessageReceived -= capture; EditorApplication.update -= tick;
                Time.captureDeltaTime = originalStep; Application.runInBackground = background; EditorApplication.ExitPlaymode();
            };
            EditorApplication.update += tick;
        }
        private static T Find<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var result = root.GetComponentInChildren<T>(); if (result != null) return result; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        private static SandboxEnemy FindGuard()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var enemy = root.GetComponent<SandboxEnemy>(); if (enemy != null && enemy.GuardedArchetype) return enemy; }
            throw new InvalidOperationException("Missing guarded fighter");
        }
        private static void CheckBinding(HeavyTechniqueSelector selector, string field, Key expected)
        {
            var action = (InputAction)typeof(HeavyTechniqueSelector).GetField(field, Private).GetValue(selector);
            foreach (var control in action.controls)
                if (control.device is Keyboard keyboard && control == keyboard[expected]) return;
            throw new InvalidOperationException("Unresolved number-row binding: " + expected);
        }
        private static void CheckNumberEvents(HeavyTechniqueSelector selector)
        {
            var keyboard = InputSystem.AddDevice<Keyboard>("TechniqueCheckKeyboard");
            string[] fields = { "chooseBaseline", "chooseBreaker", "chooseMomentum" };
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3 };
            try
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    var action = (InputAction)typeof(HeavyTechniqueSelector).GetField(fields[i], Private).GetValue(selector);
                    try
                    {
                        action.ApplyBindingOverride(0, keyboard.path + "/" + (i + 1));
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys[i])); InputSystem.Update();
                        Require(action.WasPressedThisFrame(), "number key did not generate action edge: " + keys[i]);
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                    }
                    finally { action.RemoveAllBindingOverrides(); }
                }
            }
            finally { InputSystem.RemoveDevice(keyboard); }
        }
        private static void Teleport(CombatActions actor, Vector3 position, Quaternion rotation)
        {
            var motor = actor.GetComponent<CharacterController>(); motor.enabled = false; actor.transform.SetPositionAndRotation(position, rotation); motor.enabled = true; Physics.SyncTransforms();
        }
        private static void TickAt(CombatActions actor, float elapsed)
        { typeof(CombatActions).GetField("elapsed", Private).SetValue(actor, elapsed); typeof(CombatActions).GetMethod("TickAction", Private).Invoke(actor, null); }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) => results.Add("PASS " + message);
    }
}
