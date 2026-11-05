using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using StarterAssets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    // Begins only after DiscoveryChecks has revealed knowledge through three real native exchanges.
    public static class RetaliationChecks
    {
        const string Report = "Logs/RetaliationChecks.txt";
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [MenuItem("Tools/EvolvingRPG/Run Retaliation Trial Checks")]
        public static void Run() => DiscoveryChecks.RunTrial();
        public static void Begin()
        {
            File.WriteAllText(Report, "Unity " + Application.unityVersion + " / C3 checks; preceding live C2 chain passed\n");
            var actor = Find<CombatActions>(); var trial = actor.GetComponent<RetaliatoryMomentumTrial>(); var discovery = actor.GetComponent<ContextualDiscovery>();
            var owner = actor.GetComponent<OwnedCharacter>();
            var evidence = actor.GetComponent<EvasionEvidence>(); var hud = Find<EnemySandboxHUD>(); var enemy = Guard();
            var native = actor.GetComponent<ThirdPersonController>(); var inputs = actor.GetComponent<StarterAssetsInputs>();
            var home = typeof(SandboxEnemy).GetField("home", Private); var oldHome = (Vector3)home.GetValue(enemy);
            float oldStep = Time.captureDeltaTime; bool oldBackground = Application.runInBackground;
            Time.captureDeltaTime = 1f / 60; Application.runInBackground = true;
            int test = 0, stage = 0, frame = -1, dodge = -1, consumed = 0; float wait = Time.time, acceptedAt = 0;
            Vector3 start = default; Quaternion facing = default; GameObject temporary = null; TrainingTarget bystander = null;
            double deadline = EditorApplication.timeSinceStartup + 120;
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("C3 native case " + test + " stage " + stage + " timed out");
                    if (frame == Time.frameCount) return; frame = Time.frameCount;
                    inputs.move = Vector2.zero; inputs.jump = false; inputs.sprint = false;
                    if (stage == 0)
                    {
                        if (actor.Busy || Time.time - wait < .5f) return;
                        hud.ResetEncounter(); Require(owner != null ? owner.ConfigureHeavy(DevelopmentIds.Momentum, test != 0) : trial.SetTrialEnabled(test != 0), "effect configuration failed after reveal");
                        Teleport(actor.gameObject, new Vector3(0, .05f, -12), Quaternion.identity);
                        Teleport(enemy.gameObject, actor.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(Vector3.back));
                        actor.GetComponent<CharacterController>().Move(Vector3.down); enemy.GetComponent<CharacterController>().Move(Vector3.down);
                        home.SetValue(enemy, enemy.transform.position); Physics.SyncTransforms();
                        typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { 0f });
                        Require(enemy.CurrentThreat.HasValue, "fixture threat missing");
                        Camera.main.transform.rotation = Quaternion.identity; inputs.move = Vector2.right; native.Grounded = true;
                        Require(actor.TryDodge(), "native dodge failed"); inputs.move = Vector2.zero; dodge = actor.AcceptedActions;
                        typeof(EvasionEvidence).GetMethod("LateUpdate", Private).Invoke(evidence, null); stage = 1;
                    }
                    else if (stage == 1)
                    {
                        if (actor.Busy || !evidence.Latest.HasValue || evidence.Latest.Value.DodgeSequence != dodge) return;
                        Require(evidence.Latest.Value.Classification == EvasionClassification.Verified, "C3 received unverified evasion");
                        if (test != 0) Require(trial.OpportunityReady, "verified evasion did not arm opportunity");
                        stage = 2;
                    }
                    else if (stage == 2)
                    {
                        if (test == 6 && (trial.SecondsRemaining > 0 || trial.OpportunityReady)) return; // Natural event-time expiry; no test clock rewrite.
                        if (test == 7) enemy.ResetEnemy();
                        if (test == 8) enemy.GetComponent<EnemyVitality>().ReceiveHit(new HitData("Fixture defeat", 10000, 0, Color.white, actor.transform.position, actor.GetComponent<CombatVitality>()), out _);
                        bool away = test == 2 || test == 3 || test == 4 || test == 6 || test == 8;
                        actor.transform.rotation = away ? Quaternion.LookRotation(Vector3.back)
                            : Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.transform.position - actor.transform.position, Vector3.up));
                        facing = actor.transform.rotation; start = actor.transform.position;
                        Camera.main.transform.rotation = Quaternion.Euler(0, actor.transform.eulerAngles.y, 0);
                        var authored = new AttackSnapshot(discovery.Momentum); consumed = trial.Consumed;
                        if (test == 1)
                        {
                            // Neutral, backward and strafe preserve the ordinary form and do not consume.
                            foreach (var direction in new[] { Vector2.zero, Vector2.down, Vector2.right, Vector2.left })
                            { inputs.move = direction; Require(trial.Resolve(discovery.Momentum, authored).AdvanceDistance == authored.AdvanceDistance, "nonforward intent lunged"); }
                        }
                        inputs.move = Vector2.up;
                        var preview = trial.Resolve(discovery.Momentum, authored);
                        var previewAgain = trial.Resolve(discovery.Momentum, authored);
                        Require(trial.Consumed == consumed && preview.AdvanceDistance == previewAgain.AdvanceDistance, "preview consumed opportunity");
                        native.Grounded = false; Require(!actor.TryAttack(discovery.Momentum) && trial.Consumed == consumed, "rejected action consumed opportunity");
                        native.Grounded = true;
                        if (test == 2)
                        {
                            temporary = GameObject.CreatePrimitive(PrimitiveType.Cube); temporary.name = "Temporary bystander";
                            temporary.transform.position = start + actor.transform.forward * 3 + Vector3.up; temporary.transform.localScale = new Vector3(1, 2, 1);
                            bystander = temporary.AddComponent<TrainingTarget>(); Physics.SyncTransforms();
                        }
                        if (test == 4)
                        {
                            temporary = GameObject.CreatePrimitive(PrimitiveType.Cube); temporary.name = "Temporary return obstruction"; temporary.isStatic = true;
                            temporary.transform.position = start + actor.transform.forward * .8f + Vector3.up; temporary.transform.localScale = new Vector3(3, 2, .15f); Physics.SyncTransforms();
                        }
                        Require(actor.TryAttack(discovery.Momentum), "return rejected"); inputs.move = Vector2.zero; acceptedAt = Time.time;
                        bool evolved = test > 0 && test < 6;
                        Require(actor.CommittedContext.HasValue == evolved && Mathf.Abs(actor.CommittedAttack.AdvanceDistance - (evolved ? 1.6f : 1.2f)) < .001f, "committed form incorrect");
                        Require(trial.Consumed == consumed + (evolved ? 1 : 0), "consumption not acceptance-linked");
                        var frozen = actor.CommittedAttack;
                        Require(!actor.TryAttack(discovery.Momentum) && trial.Consumed == consumed + (evolved ? 1 : 0), "busy retry consumed again");
                        Require(!trial.SetTrialEnabled(false) && actor.CommittedAttack.AdvanceDistance == frozen.AdvanceDistance, "mid-action toggle rewrote commitment");
                        if (test == 5) { enemy.ResetEnemy(); Physics.SyncTransforms(); }
                        if (test == 3) { trial.enabled = false; Require(actor.CommittedContext.HasValue && actor.CommittedAttack.AdvanceDistance == frozen.AdvanceDistance, "disabling resolver rewrote committed action"); }
                        stage = 3;
                    }
                    else
                    {
                        if (actor.Busy) return;
                        float duration = Time.time - acceptedAt;
                        float distance = Vector3.ProjectOnPlane(actor.transform.position - start, Vector3.up).magnitude;
                        float expected = test == 0 || test == 1 || test == 7 ? 1.01f : test == 2 || test == 5 ? 1.26f : test == 6 || test == 8 ? 1.56f : 1.81f;
                        Require(Mathf.Abs(duration - expected) < .09f, "recovery policy incorrect: case " + test + " duration " + duration + " expected " + expected);
                        if (test == 4) Require(distance > .05f && distance < .65f, "trial passed through real wall: " + distance);
                        else if (test != 6) Require(Mathf.Abs(distance - (test == 0 || test >= 6 ? 1.2f : 1.6f)) < .09f, "native advance incorrect: " + distance);
                        Require(Quaternion.Angle(actor.transform.rotation, facing) < .1f, "return changed facing");
                        if (test == 2) Require(bystander.Health < bystander.MaximumHealth, "bystander-only fixture did not actually hit");
                        Require(Mathf.Approximately(discovery.Momentum.AdvanceDistance, 1.2f) && Mathf.Approximately(discovery.Momentum.Recovery, .45f)
                            && Mathf.Approximately(discovery.Momentum.StaminaCost, 20) && actor.GetComponent<HeavyPractice>() == null, "shared asset/stamina/practice changed");
                        if (owner != null)
                        {
                            var review = owner.Review.Latest;
                            bool evolved = test > 0 && test < 6;
                            float expectedMiss = test == 3 || test == 4 || test == 6 || test == 8 ? .55f : 0;
                            float expectedLinked = test >= 2 && test <= 5 ? .25f : 0;
                            Require(review != null && review.Sequence == actor.AcceptedActions && !review.Interrupted
                                && review.Evolved == evolved && Mathf.Abs(review.Displacement - distance) < .03f
                                && Mathf.Abs(review.Duration - duration) < .03f
                                && Mathf.Abs(review.PlannedAdvance - (evolved ? 1.6f : 1.2f)) < .001f
                                && Mathf.Abs(review.MissRecovery - expectedMiss) < .001f
                                && Mathf.Abs(review.LinkedRecovery - expectedLinked) < .001f,
                                "native action review disagrees with actual committed form, travel or recovery: case " + test);
                            Require(!evolved || review.LinkedHit == (test == 1), "review treated bystander or reset generation as linked success");
                        }
                        File.AppendAllText(Report, "PASS Native case " + test + " / " + new[] { "ordinary reference", "linked return; previews/rejections do not consume", "bystander hit retains target miss cost", "complete miss retains both drawbacks", "wall clips evolved travel", "post-acceptance target reset retains miss cost", "expired opportunity falls back to ordinary Momentum", "reset before acceptance falls back to ordinary Momentum", "defeated target falls back to ordinary Momentum" }[test]
                            + " / displacement=" + distance.ToString("0.000") + " m / duration=" + duration.ToString("0.000") + " s\n");
                        if (temporary != null) { UnityEngine.Object.DestroyImmediate(temporary); temporary = null; bystander = null; }
                        trial.enabled = true;
                        if (++test < 9) { stage = 0; wait = Time.time; return; }
                        if (owner != null) { hud.ResetEncounter(); DevelopmentChecks.BeforeFresh(); }
                        Require((owner != null ? owner.FreshCharacter() : discovery.FreshCharacter()) && !trial.TrialEnabled && !trial.OpportunityReady
                            && !discovery.MomentumInvestment && (owner != null || !discovery.EvasiveInvestment) && discovery.Ledger.Knowledge == DiscoveryKnowledge.Undiscovered, "fresh character inherited trial or knowledge");
                        File.AppendAllText(Report, "PASS Fresh Character clears knowledge, investments and trial activation.\n");
                        if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                        if (owner != null) DevelopmentChecks.Finished();
                        File.AppendAllText(Report, "PASS all C3 native checks; no runtime Console errors. Gameplay feel awaits player testing.\n");
                        Cleanup(); return;
                    }
                    return;
                }
                catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); Cleanup(); }
            };
            void Cleanup()
            {
                if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
                home.SetValue(enemy, oldHome);
                try { hud.ResetEncounter(); }
                finally
                {
                    Time.captureDeltaTime = oldStep; Application.runInBackground = oldBackground;
                    Application.logMessageReceived -= capture; EditorApplication.update -= tick; EditorApplication.ExitPlaymode();
                }
            }
            EditorApplication.update += tick;
        }
        static T Find<T>() where T : Component
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; } throw new InvalidOperationException("Missing " + typeof(T).Name); }
        static SandboxEnemy Guard()
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) foreach (var enemy in root.GetComponentsInChildren<SandboxEnemy>(true)) if (enemy.GuardedArchetype) return enemy; throw new InvalidOperationException("Missing guard"); }
        static void Teleport(GameObject actor, Vector3 position, Quaternion rotation)
        { var body = actor.GetComponent<CharacterController>(); body.enabled = false; actor.transform.SetPositionAndRotation(position, rotation); body.enabled = true; Physics.SyncTransforms(); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
