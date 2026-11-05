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
    public static class EvasionEvidenceChecks
    {
        private const string Requested = "EvolvingRPG.EvasionChecksRequested";
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private const string Report = "Logs/EvasionEvidenceChecks.txt";
        private static readonly List<string> results = new List<string>();
        static EvasionEvidenceChecks() => EditorApplication.playModeStateChanged += ModeChanged;

        [MenuItem("Tools/EvolvingRPG/Run Evasion Evidence Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before running checks.");
            if (!File.Exists(EvasionSandboxBuilder.ScenePath)) EvasionSandboxBuilder.Create();
            EditorSceneManager.OpenScene(EvasionSandboxBuilder.ScenePath);
            SessionState.SetBool(Requested, true); EditorApplication.EnterPlaymode();
        }
        private static void ModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false); results.Clear(); var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture;
            try { ModelChecks(); QueryChecks(); LifecycleChecks(); }
            catch (Exception exception) { errors.Add(exception.ToString()); }
            finally
            {
                Application.logMessageReceived -= capture; Directory.CreateDirectory("Logs");
                string report = "Unity " + Application.unityVersion + " / C1 evasion checks\n" + string.Join("\n", results)
                    + "\n" + (errors.Count == 0 ? "PASS isolated checks; runtime motor checks follow." : "FAIL\n" + string.Join("\n", errors));
                File.WriteAllText(Report, report); Debug.Log(report);
            }
            if (errors.Count == 0) BeginMotorChecks(); else EditorApplication.ExitPlaymode();
        }

        private static void ModelChecks()
        {
            Check(Fixture().Finish(), EvasionClassification.Verified, EvasionReason.Verified);
            Check(Fixture(includeThreat: false).Finish(), EvasionClassification.Rejected, EvasionReason.NoThreat);
            Check(Fixture(includeThreat: false, issue: EvasionReason.BeforeCommitment).Finish(), EvasionClassification.Rejected, EvasionReason.BeforeCommitment);
            Check(Fixture(startZ: 5).Finish(), EvasionClassification.Rejected, EvasionReason.StartingNotThreatened);
            Pass("Verified threatened-start evasion; no threat; dodge before commitment; harmless enemy miss");

            Check(Fixture(blocked: true).Finish(), EvasionClassification.Rejected, EvasionReason.Obstructed);
            Check(Fixture(incoming: true).Finish(), EvasionClassification.Rejected, EvasionReason.IncomingHit);
            Check(Fixture(completed: false).Finish(), EvasionClassification.Rejected, EvasionReason.DodgeInterrupted);
            Check(Fixture(issue: EvasionReason.PlayerReset).Finish(), EvasionClassification.Unverified, EvasionReason.PlayerReset);
            Pass("Wall-blocked travel, incoming contact, interrupted dodge and player reset never verify");

            Check(Fixture(end: ThreatEnd.Interrupted).Finish(), EvasionClassification.Unverified, EvasionReason.EnemyInterrupted);
            Check(Fixture(end: ThreatEnd.Reset).Finish(), EvasionClassification.Unverified, EvasionReason.EnemyReset);
            Check(Fixture(issue: EvasionReason.OverlappingThreats).Finish(), EvasionClassification.Unverified, EvasionReason.OverlappingThreats);
            Check(Fixture(step: 1f / 15).Finish(), EvasionClassification.Unverified, EvasionReason.IncompleteSamples);
            Check(Fixture(issue: EvasionReason.IncompleteSamples).Finish(), EvasionClassification.Unverified, EvasionReason.IncompleteSamples);
            Check(Fixture(issue: EvasionReason.UnsupportedGeometry).Finish(), EvasionClassification.Unverified, EvasionReason.UnsupportedGeometry);
            Pass("Interrupted/reset enemy, overlapping threats, coarse/missing samples and unsupported geometry stay unverified");

            var once = Fixture(); var first = once.Finish(); int count = once.Samples;
            once.IncomingHit(); once.Sample(10, default, true, Vector3.zero, false, true);
            var duplicate = once.Finish();
            Require(duplicate.Classification == first.Classification && once.Samples == count, "finished evidence changed after replay");
            var key = first.Threat.Value;
            Require(!key.Equals(new EnemyThreat(key.Source, key.Target, key.Revision + 1, key.Sequence,
                key.CommittedAt, 0.7f, 0.16f, key.Geometry)), "reset generation reused threat identity");
            Pass("Finished reducer is immutable under duplicate events; encounter revision changes identity");
        }

        private static EvasionAttempt Fixture(bool includeThreat = true, float startZ = 1.5f, bool blocked = false,
            bool incoming = false, bool completed = true, ThreatEnd end = ThreatEnd.Completed,
            float step = 0.01f, EvasionReason? issue = null)
        {
            var geometry = new StrikeGeometry(Vector3.up, Vector3.forward, 1.9f, 80);
            var start = new StrikeCapsule(new Vector3(0, 1, startZ), 0.3f, 0.6f, new Vector3(0, 1, startZ));
            var threat = new EnemyThreat(null, null, 0, 1, 0, 0.7f, 0.16f, geometry);
            var attempt = new EvasionAttempt(1, 0, 0, start, Vector3.right, 3, 0.25f, includeThreat ? threat : (EnemyThreat?)null);
            if (issue.HasValue) attempt.Mark(issue.Value);
            int frames = Mathf.CeilToInt(0.9f / step);
            for (int i = 1; i <= frames; i++)
            {
                float time = i * step;
                float x = blocked ? 0 : Mathf.Min(1, time / 0.25f) * 3;
                var pose = new StrikeCapsule(new Vector3(x, 1, startZ), 0.3f, 0.6f, new Vector3(x, 1, startZ));
                if (time >= 0.6f) attempt.EndDodge(completed);
                if (time >= 0.86f && includeThreat) attempt.Resolve(new ThreatResolution(threat, end, 0.86f, incoming));
                attempt.Sample(time, pose, blocked, Vector3.up,
                    StrikeContactQuery.GeometryContacts(geometry, start.ClosestPoint(geometry.Origin)),
                    StrikeContactQuery.GeometryContacts(geometry, pose.ClosestPoint(geometry.Origin)));
            }
            if (incoming) attempt.IncomingHit();
            return attempt;
        }

        private static void QueryChecks()
        {
            var targetObject = new GameObject("Temporary contact-query target"); GameObject wall = null;
            try
            {
                targetObject.transform.position = new Vector3(1000, 0, 1001.5f);
                var body = targetObject.AddComponent<CharacterController>(); body.center = Vector3.up; body.height = 1.8f; body.radius = 0.3f;
                var target = targetObject.AddComponent<CombatVitality>(); Physics.SyncTransforms();
                var geometry = new StrikeGeometry(new Vector3(1000, 1, 1000), Vector3.forward, 1.9f, 80);
                var pose = StrikeCapsule.Capture(body);
                Require((pose.ClosestPoint(body.ClosestPoint(geometry.Origin)) - body.ClosestPoint(geometry.Origin)).magnitude < 0.015f,
                    "authored capsule does not enclose native query");
                Require(StrikeContactQuery.Contacts(geometry, body, target) && StrikeContactQuery.ContactsAtPose(geometry, pose, body, target, out _), "actual/proxy contact differ");
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.isStatic = true;
                wall.transform.position = new Vector3(1000, 1, 1000.7f); wall.transform.localScale = new Vector3(2, 2, 0.1f); Physics.SyncTransforms();
                Require(!StrikeContactQuery.Contacts(geometry, body, target) && !StrikeContactQuery.ContactsAtPose(geometry, pose, body, target, out _), "static wall did not block both queries");
                wall.isStatic = false;
                StrikeContactQuery.ContactsAtPose(geometry, pose, body, target, out bool ambiguous);
                Require(ambiguous, "dynamic blocker was not flagged");
                Require(target.Health == target.MaximumHealth, "contact query applied damage");
                Pass("Real collider/proxy share geometry and LOS; wall blocks; dynamic blocker flags ambiguity; query has no damage side effect");
            }
            finally { if (wall != null) UnityEngine.Object.DestroyImmediate(wall); UnityEngine.Object.DestroyImmediate(targetObject); }
        }

        private static void LifecycleChecks()
        {
            var actor = Find<CombatActions>(); var player = actor.GetComponent<CombatVitality>();
            var evidence = actor.GetComponent<EvasionEvidence>(); var guard = FindGuard();
            var health = guard.GetComponent<EnemyVitality>(); var native = actor.GetComponent<ThirdPersonController>();
            Vector3 original = actor.transform.position; Quaternion facing = actor.transform.rotation;
            var ended = new List<ThreatResolution>(); EnemyThreat? incoming = null;
            Action<ThreatResolution> listen = value => ended.Add(value);
            Action<HitData, HitResolution> hit = (data, receipt) => incoming = data.Threat;
            guard.ThreatFinished += listen; player.HitReceived += hit;
            try
            {
                guard.ResetEnemy(); player.ResetVitality(); Teleport(actor.gameObject, guard.transform.position + guard.transform.forward * 1.5f, Quaternion.identity);
                Step(guard, 0); var threat = guard.CurrentThreat.Value;
                Require(threat.Source == guard && threat.Target == player && threat.Revision == guard.EncounterRevision, "committed identity/target missing");
                float before = player.Health;
                Step(guard, threat.ActiveAt - threat.CommittedAt + 0.01f); Step(guard, 0.01f);
                Require(player.Health < before && guard.HitsLanded == 1 && incoming.HasValue && incoming.Value.Equals(threat), "actual damage/once-only/source identity changed");
                Step(guard, threat.ActiveUntil - threat.ActiveAt + 0.02f);
                Require(ended.Count == 1 && ended[0].End == ThreatEnd.Completed && ended[0].Connected, "normal completion missing or duplicated");
                Pass("Real enemy commits, shared query applies damage once, hit carries identity, and normal active interval completes once");

                guard.ResetEnemy(); player.ResetVitality(); ended.Clear(); Step(guard, 0);
                health.ReceiveHit(new HitData("Fixture stagger", 0, 1000, Color.white, actor.transform.position), out _);
                Require(ended.Count == 1 && ended[0].End == ThreatEnd.Interrupted, "poise interruption not observed");
                guard.ResetEnemy(); ended.Clear(); Step(guard, 0); guard.ResetEnemy();
                Require(ended.Count == 1 && ended[0].End == ThreatEnd.Reset, "reset not observed exactly once");
                ended.Clear(); Step(guard, 0); player.ResetVitality();
                Require(ended.Count == 1 && ended[0].End == ThreatEnd.TargetChanged, "player reset did not terminate threat");
                Pass("Real poise break, opponent reset and player reset terminate threats with explicit outcomes");

                guard.ResetEnemy(); player.ResetVitality(); Step(guard, 0); native.Grounded = true;
                Require(actor.TryDodge(), "fixture dodge did not start");
                var pending = evidence.Pending;
                var accepted = new CombatObservation(actor.AcceptedActions, CombatObservationKind.Accepted, CombatActionKind.Dodge, null, null, default, 0);
                typeof(EvasionEvidence).GetMethod("OnAction", Private).Invoke(evidence, new object[] { accepted });
                Require(ReferenceEquals(pending, evidence.Pending), "duplicate acceptance replaced/ended evidence");
                SandboxEnemy fast = null;
                foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                    foreach (var enemy in root.GetComponentsInChildren<SandboxEnemy>(true)) if (!enemy.GuardedArchetype) fast = enemy;
                Require(fast != null, "second opponent missing");
                bool wasActive = fast.gameObject.activeSelf;
                var homeField = typeof(SandboxEnemy).GetField("home", Private);
                Vector3 fastHome = default;
                try
                {
                    fast.gameObject.SetActive(true); fastHome = (Vector3)homeField.GetValue(fast);
                    Teleport(fast.gameObject, actor.transform.position + Vector3.right * 1.5f, Quaternion.LookRotation(Vector3.left));
                    homeField.SetValue(fast, fast.transform.position);
                    Step(fast, 0);
                    Require(fast.CurrentThreat.HasValue, "second opponent did not commit: " + fast.Phase + " pos=" + fast.transform.position + " actor=" + actor.transform.position + " hp=" + fast.GetComponent<EnemyVitality>().Health + " target=" + fast.EngagedWith(player));
                    pending.EndDodge(true, StrikeCapsule.Capture(actor.GetComponent<CharacterController>()));
                    typeof(EvasionEvidence).GetMethod("LateUpdate", Private).Invoke(evidence, null);
                    Require(evidence.Latest.HasValue && evidence.Latest.Value.Reason == EvasionReason.OverlappingThreats,
                        "second real committed threat was not ambiguous");
                }
                finally { homeField.SetValue(fast, fastHome); fast.ResetEnemy(); fast.gameObject.SetActive(wasActive); }
                actor.Interrupt("overlap fixture done"); guard.ResetEnemy(); player.ResetVitality(); Step(guard, 0); native.Grounded = true;
                Require(actor.TryDodge(), "reset fixture dodge did not start");
                player.ResetVitality();
                Require(evidence.Latest.HasValue && evidence.Latest.Value.Reason == EvasionReason.PlayerReset, "collector player reset left stale evidence");
                actor.Interrupt("fixture cleanup"); guard.ResetEnemy();
                native.Grounded = true; Require(actor.TryDodge(), "pre-commit fixture dodge did not start");
                Require(evidence.Pending != null && !evidence.Pending.Threat.HasValue, "idle enemy was adopted as a threat");
                Step(guard, 0);
                evidence.Pending.EndDodge(true, StrikeCapsule.Capture(actor.GetComponent<CharacterController>()));
                typeof(EvasionEvidence).GetMethod("LateUpdate", Private).Invoke(evidence, null);
                Require(evidence.Latest.HasValue && evidence.Latest.Value.Reason == EvasionReason.BeforeCommitment,
                    "collector adopted a threat committed after accepted dodge");
                actor.Interrupt("pre-commit fixture done"); guard.ResetEnemy();
                Pass("Real accepted dodge/second-threat callbacks reach collector; duplicate acceptance ignored; player reset invalidates pending evidence");
                Pass("Real pre-commit dodge does not adopt a later committed enemy strike");
            }
            finally
            {
                guard.ThreatFinished -= listen; player.HitReceived -= hit;
                actor.Interrupt("isolated checks done"); player.ResetVitality(); guard.ResetEnemy(); Teleport(actor.gameObject, original, facing);
            }
        }

        private static void BeginMotorChecks()
        {
            var actor = Find<CombatActions>(); var collector = actor.GetComponent<EvasionEvidence>(); var guard = FindGuard();
            var player = actor.GetComponent<CombatVitality>(); var inputs = actor.GetComponent<StarterAssetsInputs>();
            var native = actor.GetComponent<ThirdPersonController>(); var hud = Find<EnemySandboxHUD>();
            int test = 0, frame = -1, sequence = -1; bool prepared = false;
            var homeField = typeof(SandboxEnemy).GetField("home", Private);
            Vector3 originalHome = (Vector3)homeField.GetValue(guard);
            float started = Time.time, originalStep = Time.captureDeltaTime; bool background = Application.runInBackground;
            double deadline = EditorApplication.timeSinceStartup + 60; GameObject wall = null;
            Vector3 start = default; var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); };
            Application.logMessageReceived += capture; Application.runInBackground = true; Time.captureDeltaTime = 1f / 60;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                try
                {
                    if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Native evidence checks interrupted or timed out.");
                    if (Time.frameCount == frame) return; frame = Time.frameCount;
                    inputs.move = Vector2.zero; inputs.jump = false; inputs.sprint = false;
                    if (!prepared)
                    {
                        if (actor.Busy || Time.time - started < 0.5f) return;
                        hud.ResetEncounter();
                        Teleport(actor.gameObject, new Vector3(0, 0.05f, -12), Quaternion.identity);
                        Teleport(guard.gameObject, actor.transform.position + Vector3.forward * 1.5f, Quaternion.LookRotation(Vector3.back));
                        // Settle fixture bodies before freezing source geometry. This is placement,
                        // not simulated dodge movement; the vendor alone moves the actual dodge.
                        actor.GetComponent<CharacterController>().Move(Vector3.down);
                        guard.GetComponent<CharacterController>().Move(Vector3.down); Physics.SyncTransforms();
                        if (test == 1)
                        {
                            wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.isStatic = true;
                            wall.transform.position = actor.transform.position + new Vector3(0, 1, -0.7f);
                            wall.transform.localScale = new Vector3(3, 2, 0.1f); Physics.SyncTransforms();
                        }
                        homeField.SetValue(guard, guard.transform.position);
                        native.Grounded = true; Step(guard, 0); start = actor.transform.position;
                        Require(guard.CurrentThreat.HasValue, "native fixture did not establish a threat");
                        Require(actor.TryDodge(), "native dodge did not start"); sequence = actor.AcceptedActions;
                        // Editor callback runs after game updates. Seed this same-time sample so the
                        // first normal frame does not falsely look like two missing sample intervals.
                        typeof(EvasionEvidence).GetMethod("LateUpdate", Private).Invoke(collector, null);
                        prepared = true; started = Time.time;
                    }
                    if (Time.time - started > 5) throw new TimeoutException("No result for accepted dodge " + sequence);
                    if (actor.Busy || !collector.Latest.HasValue || collector.Latest.Value.DodgeSequence != sequence) return;
                    var result = collector.Latest.Value;
                    float moved = Vector3.ProjectOnPlane(actor.transform.position - start, Vector3.up).magnitude;
                    if (test == 0)
                    {
                        Require(result.Classification == EvasionClassification.Verified && moved > 2.85f && moved < 3.15f,
                            "normal native evasion did not verify: " + result.Explanation + " / " + moved);
                        Require(Quaternion.Angle(actor.transform.rotation, Quaternion.identity) < 0.1f, "dodge changed facing");
                    }
                    else if (test == 1) Require(moved < 0.8f && result.Classification != EvasionClassification.Verified,
                        "real wall movement incorrectly verified: " + result.Explanation + " / " + moved);
                    else Require(result.Classification == EvasionClassification.Unverified && result.Reason == EvasionReason.IncompleteSamples,
                        "15Hz evidence incorrectly accepted: " + result.Explanation);
                    File.AppendAllText(Report, "\nPASS Native motor: " + (test == 0 ? "actual dodge verifies; distance and facing preserved" : test == 1 ? "wall clips actual travel; no false verification" : "15Hz samples remain unverified"));
                    if (wall != null) { UnityEngine.Object.DestroyImmediate(wall); wall = null; }
                    if (++test < 3)
                    {
                        prepared = false; started = Time.time;
                        Time.captureDeltaTime = test == 2 ? 1f / 15 : 1f / 60; return;
                    }
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
                    File.AppendAllText(Report, "\nPASS all C1 checks; no runtime Console errors captured. Run baseline combat/technique/enemy/practice regressions separately.\n");
                }
                catch (Exception exception) { File.AppendAllText(Report, "\nFAIL Native evidence: " + exception + "\n"); }
                if (wall != null) UnityEngine.Object.DestroyImmediate(wall);
                homeField.SetValue(guard, originalHome);
                hud.ResetEncounter(); Application.logMessageReceived -= capture; EditorApplication.update -= tick;
                Time.captureDeltaTime = originalStep; Application.runInBackground = background; EditorApplication.ExitPlaymode();
            };
            EditorApplication.update += tick;
        }

        private static T Find<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var value = root.GetComponentInChildren<T>(true); if (value != null) return value; }
            throw new InvalidOperationException("Missing " + typeof(T).Name);
        }
        private static SandboxEnemy FindGuard()
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var enemy in root.GetComponentsInChildren<SandboxEnemy>(true)) if (enemy.GuardedArchetype) return enemy;
            throw new InvalidOperationException("Missing guarded opponent");
        }
        private static void Step(SandboxEnemy enemy, float delta) => typeof(SandboxEnemy).GetMethod("Simulate", Private).Invoke(enemy, new object[] { delta });
        private static void Teleport(GameObject actor, Vector3 position, Quaternion rotation)
        {
            var body = actor.GetComponent<CharacterController>(); body.enabled = false;
            actor.transform.SetPositionAndRotation(position, rotation); body.enabled = true; Physics.SyncTransforms();
        }
        private static void Check(EvasionResult result, EvasionClassification classification, EvasionReason reason)
            => Require(result.Classification == classification && result.Reason == reason, "Expected " + reason + "; got " + result.Classification + ": " + result.Explanation);
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) => results.Add("PASS " + message);
    }
}
