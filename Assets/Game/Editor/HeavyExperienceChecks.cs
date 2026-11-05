using System;
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
    public static class HeavyExperienceChecks
    {
        const string Requested = "EvolvingRPG.HeavyExperienceChecks", Report = "Logs/HeavyExperienceChecks.txt";
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        static HeavyExperienceChecks() => EditorApplication.playModeStateChanged += Changed;
        [MenuItem("Tools/EvolvingRPG/Run Heavy Experience Checks %#&F7")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits first.");
            EditorSceneManager.OpenScene(DevelopmentSandboxBuilder.ScenePath);
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report, "Unity " + Application.unityVersion + " / Heavy experience history\n");
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false);
            try { Models(); Native(); }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); EditorApplication.ExitPlaymode(); }
        }
        static void Models()
        {
            var owner = Find<OwnedCharacter>(); var actor = owner.GetComponent<CombatActions>();
            var vitality = owner.GetComponent<CombatVitality>(); var enemy = Guard(); var receiver = enemy.GetComponent<EnemyVitality>();
            var heavy = actor.HeavyAttack; var state = new CharacterDevelopmentState(); int sequence = 0;
            using (var collector = new HeavyExperienceCollector(actor, vitality, () => state, definition => definition == heavy ? DevelopmentIds.Baseline : null))
            {
                var observe = typeof(HeavyExperienceCollector).GetMethod("Observe", Private);
                void Send(CombatObservationKind kind, HitResolution receipt = default, HitReceiver target = null, AttackDefinition definition = null)
                    => observe.Invoke(collector, new object[] { new CombatObservation(sequence, kind, CombatActionKind.Attack, definition ?? heavy, target, receipt, 0) });
                HitResolution Receipt(EnemyPhase phase = EnemyPhase.Approach, bool engaged = true, bool broken = false, bool guarded = false, float damage = 1)
                    => new HitResolution(damage, 0, broken, false, guarded, false, new EnemyImpactContext(phase, enemy.EncounterRevision, 0, float.NegativeInfinity, engaged));
                void Complete(HitResolution receipt, HitReceiver target = null, AttackDefinition definition = null)
                { sequence++; Send(CombatObservationKind.Accepted, definition: definition); Send(CombatObservationKind.HitResolved, receipt, target ?? receiver, definition); Send(CombatObservationKind.Completed, definition: definition); }
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver);
                Require(state.ExperienceCount == 0, "impact committed experience before recovery");
                Send(CombatObservationKind.Completed); Require(state.ExperienceCount == 1, "initiative without enemy strike did not count");
                Send(CombatObservationKind.Completed); int revision = state.Revision;
                Complete(Receipt()); Require(state.ExperienceCount == 1 && state.Revision == revision, "duplicate or repeated contact grew history");
                Complete(Receipt(EnemyPhase.Windup, broken: true, guarded: true));
                Require(state.ExperienceCount == 3 && state.Balance == 1 && state.BuildRevision == 0 && state.Knowledge == DiscoveryKnowledge.Undiscovered,
                    "qualitative facts changed power/resources/knowledge or lost pre-impact context");
                Pass("Completion, initiative with no prior strike, frozen guard/interception facts, lifetime deduplication and no power/resource/discovery mutation");

                state = new CharacterDevelopmentState();
                Complete(Receipt(EnemyPhase.Idle)); Complete(Receipt(EnemyPhase.Returning)); Complete(Receipt(engaged: false)); Complete(Receipt(damage: 0));
                Complete(new HitResolution(10, 10, true, false)); Complete(Receipt(), definition: actor.LightAttack);
                var dummy = new GameObject("Temporary experience non-enemy");
                try { Complete(Receipt(), dummy.AddComponent<TrainingTarget>()); } finally { UnityEngine.Object.DestroyImmediate(dummy); }
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.Completed);
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver); Send(CombatObservationKind.Interrupted); Send(CombatObservationKind.Completed);
                Require(state.ExperienceCount == 0, "inert/foreign/zero/missing-context/Light/dummy/miss/interruption awarded history");
                Pass("Idle/returning/foreign/zero/missing-context/Light/dummy/miss/interrupted outcomes reject");

                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver); enemy.ResetEnemy(); Send(CombatObservationKind.Completed);
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver);
                enemy.gameObject.SetActive(false); Send(CombatObservationKind.Completed); enemy.gameObject.SetActive(true);
                Require(state.ExperienceCount == 0, "stale or disabled opponent gave credit");
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver); state = new CharacterDevelopmentState(); Send(CombatObservationKind.Completed);
                Require(state.ExperienceCount == 0, "pending evidence crossed character replacement");
                sequence++; Send(CombatObservationKind.Accepted); Send(CombatObservationKind.HitResolved, Receipt(), receiver);
                state.Purchase(DevelopmentIds.Momentum, state.Revision, out _); Send(CombatObservationKind.Completed);
                Require(state.ExperienceCount == 0, "changed build accepted pending history");
                Require(state.RecordExperience(new[] { HeavyExperienceCatalog.Id(DevelopmentIds.Breaker, HeavyExperienceCatalog.Contact) }) == 0
                    && state.RecordExperience(new[] { "unknown" }) == 0, "unowned/unknown history accepted");
                Pass("Reset/disable/character/build invalidation; unknown and unowned facts reject atomically");
            }
        }
        static void Native()
        {
            var owner = Find<OwnedCharacter>(); var actor = owner.GetComponent<CombatActions>(); var enemy = Guard(); var hud = Find<EnemySandboxHUD>();
            var native = owner.GetComponent<ThirdPersonController>(); var inputs = owner.GetComponent<StarterAssetsInputs>();
            var home = typeof(SandboxEnemy).GetField("home", Private); var originalHome = (Vector3)home.GetValue(enemy);
            float oldStep = Time.captureDeltaTime; bool oldBackground = Application.runInBackground;
            Time.captureDeltaTime = 1f / 60; Application.runInBackground = true;
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs", "HeavyExperienceChecks", Guid.NewGuid().ToString("N"))));
            int test = 0, stage = 0, frame = -1; float wait = Time.time; bool hadHit = false; CombatObservation firstHit = default;
            GameObject dummy = null; double deadline = EditorApplication.timeSinceStartup + 120;
            var errors = new System.Collections.Generic.List<string>();
            Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Action<CombatObservation> receipt = value => { if (value.Kind == CombatObservationKind.HitResolved && !hadHit) { firstHit = value; hadHit = true; } };
            actor.Observed += receipt; Application.logMessageReceived += capture;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("History case " + test + " stage " + stage);
                    if (frame == Time.frameCount) return; frame = Time.frameCount;
                    inputs.move = Vector2.zero; inputs.jump = inputs.sprint = false;
                    if (stage == 0)
                    {
                        if (Time.time - wait < .3f) return;
                        hud.ResetEncounter();
                        if (test != 1 && test != 7) Require(owner.FreshCharacter(), "fresh fixture rejected");
                        if (test == 6) Require(owner.Purchase(DevelopmentIds.Breaker, owner.State.Revision), "Breaker ownership rejected");
                        Teleport(owner.gameObject, new Vector3(0, .05f, -12), Quaternion.identity);
                        float distance = test >= 6 ? 1.5f : test == 4 || test == 5 ? 9f : 3.3f;
                        Teleport(enemy.gameObject, actor.transform.position + Vector3.forward * distance, Quaternion.LookRotation(Vector3.back));
                        home.SetValue(enemy, enemy.transform.position); Physics.SyncTransforms();
                        typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { 0f });
                        if (test == 5)
                        {
                            dummy = GameObject.CreatePrimitive(PrimitiveType.Cube); dummy.name = "Temporary native history dummy";
                            dummy.transform.position = actor.transform.position + new Vector3(0, 1, 1.5f); dummy.AddComponent<TrainingTarget>(); Physics.SyncTransforms();
                        }
                        native.Grounded = true; hadHit = false;
                        Require(actor.TryAttack(actor.HeavyAttack), "native Heavy rejected: " + actor.Message);
                        stage = 1; return;
                    }
                    if (stage == 1 && hadHit && (test == 2 || test == 3))
                    {
                        Require(owner.State.ExperienceCount == 0, "pending contact already awarded experience");
                        if (test == 2) actor.Interrupt("history interruption fixture"); else enemy.ResetEnemy();
                        stage = 2;
                    }
                    if (actor.Busy) return;
                    int expected = test == 0 || test == 1 ? 1 : test >= 6 ? 3 : 0;
                    Require(owner.State.ExperienceCount == expected, "native history case " + test + " count " + owner.State.ExperienceCount + ": " + owner.Experience.Latest);
                    if (test == 0) Require(hadHit && firstHit.Resolution.EnemyContext.HasValue
                        && firstHit.Resolution.EnemyContext.Value.PhaseBeforeHit == EnemyPhase.Approach
                        && firstHit.Resolution.EnemyContext.Value.ThreatSequence == 0, "initiative fixture was not truly before first enemy strike");
                    if (test == 5) Require(hadHit && firstHit.Target is TrainingTarget, "dummy exclusion fixture never hit dummy");
                    if (test == 6)
                    {
                        Require(hadHit && firstHit.Resolution.Guarded && firstHit.Resolution.PoiseBroken
                            && firstHit.Resolution.EnemyContext.Value.PhaseBeforeHit == EnemyPhase.Windup, "native Breaker did not intercept actual guarded windup");
                        hud.ResetEncounter(); string id = owner.State.CharacterId;
                        Require(owner.SaveSlot("A") && owner.FreshCharacter() && owner.State.ExperienceCount == 0 && owner.SaveSlot("B")
                            && owner.LoadSlot("A") && owner.State.CharacterId == id && owner.State.ExperienceCount == 3
                            && !actor.Busy && !actor.CommittedContext.HasValue && !owner.GetComponent<RetaliatoryMomentumTrial>().OpportunityReady,
                            "native save/load/fresh history isolation failed");
                    }
                    Pass("Native case " + test + ": " + new[] { "initiative before first enemy strike", "R retains fact and repeated contact adds none", "interrupted impact gives no fact", "target reset invalidates pending impact", "miss gives no fact", "real dummy contact gives no fact", "guard/interception facts plus isolated A/B save/load", "repeated guard/interception after reload adds none" }[test]);
                    if (dummy != null) { UnityEngine.Object.DestroyImmediate(dummy); dummy = null; }
                    if (++test < 8) { stage = 0; wait = Time.time; return; }
                    hud.ResetEncounter(); Require(owner.LoadSlot("B") && owner.State.ExperienceCount == 0, "second character inherited history");
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    Pass("All Heavy experience model/native checks; no runtime Console errors. History is not a gameplay-feel result.");
                }
                catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); }
                if (dummy != null) UnityEngine.Object.DestroyImmediate(dummy);
                actor.Observed -= receipt; Application.logMessageReceived -= capture; EditorApplication.update -= tick;
                home.SetValue(enemy, originalHome); hud.ResetEncounter(); Time.captureDeltaTime = oldStep; Application.runInBackground = oldBackground;
                EditorApplication.ExitPlaymode();
            };
            EditorApplication.update += tick;
        }
        static void Teleport(GameObject obj, Vector3 position, Quaternion rotation)
        { var motor = obj.GetComponent<CharacterController>(); motor.enabled = false; obj.transform.SetPositionAndRotation(position, rotation); motor.enabled = true; }
        static T Find<T>() where T : Component
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var found = root.GetComponentInChildren<T>(true); if (found != null) return found; } return null; }
        static SandboxEnemy Guard()
        { foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) { var found = root.GetComponent<SandboxEnemy>(); if (found != null && found.GuardedArchetype) return found; } throw new InvalidOperationException("Missing guard"); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        static void Pass(string message) => File.AppendAllText(Report, "PASS " + message + "\n");
    }
}
