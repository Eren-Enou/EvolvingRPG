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
    [InitializeOnLoad]
    public static class DiscoveryChecks
    {
        const string Requested = "EvolvingRPG.DiscoveryChecks";
        const string Report = "Logs/DiscoveryChecks.txt";
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static DiscoveryChecks() => EditorApplication.playModeStateChanged += Changed;
        [MenuItem("Tools/EvolvingRPG/Run Discovery Checks")]
        public static void Run() => RunScene(false);
        public static void RunDevelopment() => RunScene(false, true);
        public static void RunTrial() => RunScene(true);
        static void RunScene(bool trial, bool development = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save edits first.");
            if (development) DevelopmentSandboxBuilder.Create(); else if (trial) RetaliationSandboxBuilder.Create(); else DiscoverySandboxBuilder.Create();
            EditorSceneManager.OpenScene(development ? DevelopmentSandboxBuilder.ScenePath : trial ? RetaliationSandboxBuilder.ScenePath : DiscoverySandboxBuilder.ScenePath);
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false); Directory.CreateDirectory("Logs"); File.WriteAllText(Report, "Unity " + Application.unityVersion + " / C2 checks\n");
            try { Models(); BeginLive(); }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); EditorApplication.ExitPlaymode(); }
        }
        static EvasionResult Receipt(SandboxEnemy enemy, int sequence, int dodge, EvasionReason? issue = null)
        {
            var geometry = new StrikeGeometry(Vector3.up, Vector3.forward, 1.9f, 80);
            var start = new StrikeCapsule(new Vector3(0, 1, 1.5f), .3f, .6f, Vector3.up);
            var key = new EnemyThreat(enemy, null, enemy.EncounterRevision, sequence, 0, .7f, .16f, geometry);
            var attempt = new EvasionAttempt(dodge, 0, 0, start, Vector3.right, 3, .25f, key);
            if (issue.HasValue) attempt.Mark(issue.Value);
            for (int i = 1; i <= 90; i++)
            {
                float now = i * .01f;
                var pose = new StrikeCapsule(start.Center + Vector3.right * Mathf.Min(3, now * 12), .3f, .6f, Vector3.up);
                if (i >= 60) attempt.EndDodge(true);
                if (i >= 86) attempt.Resolve(new ThreatResolution(key, ThreatEnd.Completed, .86f, false));
                attempt.Sample(now, pose, false, Vector3.up, true, false);
            }
            return attempt.Finish();
        }
        static void Models()
        {
            var actor = Find<CombatActions>(); var enemy = Guard(); var other = actor.GetComponent<CombatVitality>();
            void Exchange(RetaliationDiscovery ledger, int dodge, int strike, bool prerequisites = true, bool momentum = true,
                float accepted = 1, bool correct = true, bool complete = true, bool delayed = false, bool interrupted = false, EvasionReason? issue = null)
            {
                var receipt = Receipt(enemy, strike, dodge, issue);
                ledger.BeginDodge(dodge, prerequisites, 1); ledger.CompleteDodge(dodge, .6f);
                if (!delayed) ledger.Evidence(receipt);
                ledger.BeginAttack(dodge + 1, momentum, accepted, 1);
                var context = new EnemyImpactContext(EnemyPhase.Recovery, enemy.EncounterRevision, strike, 0, true);
                ledger.Hit(dodge + 1, correct ? enemy.GetComponent<EnemyVitality>() : other, new HitResolution(40, 25, false, false, enemyContext: context));
                if (interrupted) ledger.Interrupt(dodge + 1);
                else if (complete) ledger.CompleteAttack(dodge + 1, 1);
                if (delayed) ledger.Evidence(receipt);
            }
            var positive = new RetaliationDiscovery();
            Exchange(positive, 1, 1); Require(positive.Knowledge == DiscoveryKnowledge.Foreshadowed && positive.Exchanges == 1, "first clue missing");
            Exchange(positive, 3, 1); Require(positive.Exchanges == 1, "same strike contributed twice");
            Exchange(positive, 5, 2, delayed: true); Exchange(positive, 7, 3);
            Require(positive.Knowledge == DiscoveryKnowledge.Revealed && positive.Exchanges == 3, "deterministic reveal failed");
            Pass("Clue then three-strike reveal; delayed evidence uses acceptance time; duplicate strike rejected");
            for (int i = 0; i < 7; i++)
            {
                var negative = new RetaliationDiscovery();
                Exchange(negative, 10, 1, prerequisites: i != 0, momentum: i != 1, accepted: i == 2 ? 2.2f : 1,
                    correct: i != 3, complete: i != 4, interrupted: i == 5, issue: i == 6 ? EvasionReason.IncompleteSamples : (EvasionReason?)null);
                Require(negative.Exchanges == 0 && negative.Knowledge == DiscoveryKnowledge.Undiscovered, "invalid exchange revealed: " + i);
            }
            var changed = new RetaliationDiscovery(); changed.BeginDodge(20, true, 1); changed.CompleteDodge(20, .6f);
            changed.Evidence(Receipt(enemy, 8, 20)); changed.BeginAttack(21, true, 1, 2);
            changed.CompleteAttack(21, 2); Require(changed.Exchanges == 0, "changed investments qualified");
            var reset = new RetaliationDiscovery(); var stale = Receipt(enemy, 9, 30); enemy.ResetEnemy();
            reset.BeginDodge(30, true, 1); reset.CompleteDodge(30, .6f); reset.Evidence(stale); reset.BeginAttack(31, true, 1, 1);
            reset.Hit(31, enemy.GetComponent<EnemyVitality>(), new HitResolution(40, 25, false, false,
                enemyContext: new EnemyImpactContext(EnemyPhase.Recovery, stale.Threat.Value.Revision, 9, 0, true)));
            reset.CompleteAttack(31, 1); Require(reset.Exchanges == 0, "reset generation qualified");
            positive.ClearEncounter(); Require(positive.Knowledge == DiscoveryKnowledge.Revealed, "R erased knowledge");
            positive.FreshCharacter(); Require(positive.Exchanges == 0 && positive.Knowledge == DiscoveryKnowledge.Undiscovered, "fresh character inherited knowledge");
            Pass("Missing/changed prerequisites, wrong technique/target, late/incomplete/interrupted actions, unverified evidence and stale generation reject; reset scopes differ");
        }
        static void BeginLive()
        {
            var actor = Find<CombatActions>(); var discovery = actor.GetComponent<ContextualDiscovery>();
            var evidence = actor.GetComponent<EvasionEvidence>(); var enemy = Guard(); var hud = Find<EnemySandboxHUD>();
            var native = actor.GetComponent<ThirdPersonController>(); var inputs = actor.GetComponent<StarterAssetsInputs>();
            var selector = actor.GetComponent<HeavyTechniqueSelector>(); var owner = actor.GetComponent<OwnedCharacter>();
            var home = typeof(SandboxEnemy).GetField("home", Private); Vector3 oldHome = (Vector3)home.GetValue(enemy);
            float oldStep = Time.captureDeltaTime; bool oldBackground = Application.runInBackground;
            Time.captureDeltaTime = 1f / 60; Application.runInBackground = true;
            int stage = 0, count = 0, frame = -1, dodge = -1;
            float wait = Time.time; double deadline = EditorApplication.timeSinceStartup + 90;
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("C2 native chain timed out at " + stage + " / " + discovery.Ledger.Diagnostic);
                    if (frame == Time.frameCount) return; frame = Time.frameCount;
                    inputs.move = Vector2.zero; inputs.jump = false; inputs.sprint = false;
                    if (stage == 0)
                    {
                        if (actor.Busy || Time.time - wait < .5f) return;
                        hud.ResetEncounter();
                        if (count == 0)
                        {
                            if (owner != null) { Require(owner.FreshCharacter(), "owned fresh rejected"); Require(owner.Purchase(DevelopmentIds.Momentum, owner.State.Revision), "owned Momentum purchase rejected"); }
                            else { Require(discovery.FreshCharacter(), "fresh rejected"); Require(discovery.SetInvestments(true, true), "fixture rejected"); }
                        }
                        if (owner == null) Require(selector.TrySelect(2), "Momentum selection failed");
                        Teleport(actor.gameObject, new Vector3(0, .05f, -12), Quaternion.identity);
                        Teleport(enemy.gameObject, actor.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(Vector3.back));
                        actor.GetComponent<CharacterController>().Move(Vector3.down); enemy.GetComponent<CharacterController>().Move(Vector3.down);
                        home.SetValue(enemy, enemy.transform.position); Physics.SyncTransforms();
                        typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { 0f });
                        Require(enemy.CurrentThreat.HasValue, "live fixture did not commit");
                        Camera.main.transform.rotation = Quaternion.identity; inputs.move = Vector2.right; native.Grounded = true;
                        Require(actor.TryDodge(), "live dodge rejected"); inputs.move = Vector2.zero; dodge = actor.AcceptedActions;
                        typeof(EvasionEvidence).GetMethod("LateUpdate", Private).Invoke(evidence, null); stage = 1;
                    }
                    else if (stage == 1)
                    {
                        if (actor.Busy) return;
                        if (count != 1 && (!evidence.Latest.HasValue || evidence.Latest.Value.DodgeSequence != dodge)) return;
                        if (count != 1) Require(evidence.Latest.Value.Classification == EvasionClassification.Verified, "real evasion did not verify: " + evidence.Latest.Value.Explanation);
                        actor.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(enemy.transform.position - actor.transform.position, Vector3.up));
                        native.Grounded = true; Require(actor.TryAttack(discovery.Momentum), "live Momentum rejected"); stage = 2;
                    }
                    else
                    {
                        if (actor.Busy) return;
                        Require(evidence.Latest.HasValue && evidence.Latest.Value.DodgeSequence == dodge && evidence.Latest.Value.Classification == EvasionClassification.Verified, "delayed real evasion did not verify");
                        Require(discovery.Ledger.Exchanges == count + 1, "real same-opponent Momentum did not qualify: " + discovery.Ledger.Diagnostic);
                        count++; Pass("Native exchange " + count + ": real C1 evasion -> accepted Momentum -> applied same-generation hit -> full recovery -> " + discovery.Ledger.Knowledge);
                        if (count < 3) { stage = 0; wait = Time.time; return; }
                        Require(discovery.Ledger.Knowledge == DiscoveryKnowledge.Revealed && actor.GetComponent<HeavyPractice>() == null, "reveal/effect separation failed");
                        if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                        Pass("All C2 checks; no runtime Console errors; reveal grants no attack modifier or ownership.");
                        if (owner != null) { Cleanup(false); DevelopmentChecks.AfterDiscovery(); }
                        else if (actor.GetComponent<RetaliatoryMomentumTrial>() != null) { Cleanup(false); RetaliationChecks.Begin(); }
                        else Cleanup();
                        return;
                    }
                    return;
                }
                catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); Cleanup(); }
            };
            void Cleanup(bool exit = true)
            {
                home.SetValue(enemy, oldHome);
                try { hud.ResetEncounter(); }
                finally
                {
                Time.captureDeltaTime = oldStep; Application.runInBackground = oldBackground;
                Application.logMessageReceived -= capture; EditorApplication.update -= tick; if (exit) EditorApplication.ExitPlaymode();
                }
            }
            EditorApplication.update += tick;
        }
        static T Find<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var result = root.GetComponentInChildren<T>(true); if (result != null) return result; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        static SandboxEnemy Guard()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) foreach (var enemy in root.GetComponentsInChildren<SandboxEnemy>(true)) if (enemy.GuardedArchetype) return enemy;
            throw new InvalidOperationException("Missing guard");
        }
        static void Teleport(GameObject actor, Vector3 position, Quaternion rotation)
        { var body = actor.GetComponent<CharacterController>(); body.enabled = false; actor.transform.SetPositionAndRotation(position, rotation); body.enabled = true; Physics.SyncTransforms(); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        static void Pass(string message) => File.AppendAllText(Report, "PASS " + message + "\n");
    }
}
