using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using StarterAssets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // Synchronous, isolated Play-mode checks: no physical input, focus, or saved scene changes.
    [InitializeOnLoad]
    public static class AttackSnapshotChecks
    {
        private const string Requested = "EvolvingRPG.AttackSnapshotChecks";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> results = new List<string>();

        static AttackSnapshotChecks() => EditorApplication.playModeStateChanged += ModeChanged;

        [MenuItem("Tools/EvolvingRPG/Run Attack Snapshot Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before running snapshot checks.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save scene edits before running snapshot checks.");
            SessionState.SetBool(Requested, true);
            EditorApplication.EnterPlaymode();
        }

        private static void ModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Requested, false)) return;
            SessionState.SetBool(Requested, false);
            var errors = new List<string>();
            Application.LogCallback capture = (message, stack, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                    errors.Add(message + "\n" + stack);
            };
            Application.logMessageReceived += capture;
            results.Clear();
            GameObject actor = null, dummy = null;
            AttackDefinition definition = null;
            try
            {
                actor = new GameObject("Temporary snapshot check actor");
                actor.SetActive(false);
                actor.transform.position = Vector3.right * 1000f;
                actor.AddComponent<StarterAssetsInputs>();
                var controller = actor.AddComponent<ThirdPersonController>();
                controller.enabled = false; // Never run the native motor in this synchronous fixture.
                var actions = actor.AddComponent<CombatActions>();
                actor.SetActive(true);
                var stamina = actor.GetComponent<CombatStamina>();
                definition = ScriptableObject.CreateInstance<AttackDefinition>();
                definition.DisplayName = "Committed test strike";
                definition.Windup = definition.ActiveTime = definition.Recovery = 1f;
                definition.Damage = 15f; definition.PoiseDamage = 12f; definition.StaminaCost = 8f;
                definition.Range = 2.2f; definition.Arc = 90f;
                definition.MovementMultiplier = 0.25f; definition.Color = Color.cyan;
                dummy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dummy.name = "Temporary snapshot check target";
                dummy.transform.position = actor.transform.position + new Vector3(0.5f, 1f, 1.5f);
                var target = dummy.AddComponent<TrainingTarget>();
                Physics.SyncTransforms();

                Require(actions.TryAttack(definition), "could not commit fixture attack");
                Require(stamina.Current == stamina.Maximum - 8f && actions.HasCommittedAttack,
                    "commit did not spend original cost or create snapshot");
                var committed = actions.CommittedAttack;
                definition.DisplayName = "Changed next strike";
                definition.Windup = definition.ActiveTime = definition.Recovery = 10f;
                definition.Damage = 100f; definition.PoiseDamage = 60f; definition.StaminaCost = 30f;
                definition.Range = 0.1f; definition.Arc = 1f;
                definition.MovementMultiplier = 1f; definition.Color = Color.red;
                Require(committed.DisplayName == "Committed test strike" && committed.Duration == 3f
                    && committed.Damage == 15f && committed.PoiseDamage == 12f && committed.StaminaCost == 8f
                    && committed.Range == 2.2f && committed.Arc == 90f && committed.Color == Color.cyan,
                    "authoring changes leaked into snapshot");
                Pass("All committed values survive authoring edits; original stamina cost charged");

                TickAt(actions, 1.1f);
                Require(actions.Phase == CombatPhase.Active && target.Health == target.MaximumHealth - 15f
                    && target.Poise == 12f, "timing, geometry or applied impact used changed definition");
                TickAt(actions, 1.2f);
                Require(target.Health == target.MaximumHealth - 15f && actions.Diagnostics.Hits == 1,
                    "snapshot swing hit same target twice");
                Pass("Original active window, range, arc and damage/poise; one hit per target");

                var movement = actor.GetComponent<CombatMovement>();
                controller.MoveSpeed = 4f;
                controller.enabled = true;
                try
                {
                    Invoke(movement, "Update");
                    Require(controller.MoveSpeed == 1f, "movement restriction reread definition");
                }
                finally { movement.Restore(); controller.enabled = false; }
                Require(controller.MoveSpeed == 4f, "movement adapter did not restore motor settings");
                Pass("Committed movement multiplier and motor restoration");

                var pivot = new GameObject("Temporary blade pivot").transform;
                pivot.SetParent(actor.transform, false);
                var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.transform.SetParent(pivot, false);
                var guide = actor.AddComponent<LineRenderer>();
                var feedback = actor.AddComponent<CombatActionFeedback>();
                feedback.Configure(pivot, blade.GetComponent<Renderer>(), guide);
                Invoke(feedback, "LateUpdate");
                var block = new MaterialPropertyBlock();
                blade.GetComponent<Renderer>().GetPropertyBlock(block);
                Require(block.GetColor(Shader.PropertyToID("_BaseColor")) == Color.cyan
                    && guide.enabled && Mathf.Abs(Vector3.ProjectOnPlane(guide.GetPosition(13), Vector3.up).magnitude - 2.2f) < 0.001f,
                    "feedback changed during committed action");
                Pass("Committed blade color and reach guide");

                TickAt(actions, 2.1f);
                Require(actions.Phase == CombatPhase.Recovery, "original recovery window changed");
                TickAt(actions, 3.1f);
                Require(!actions.Busy && !actions.HasCommittedAttack && actions.CurrentAttack == null,
                    "original completion or snapshot cleanup failed");
                foreach (var record in actions.Diagnostics.Records)
                    Require(record.Attack.HasValue && record.Attack.Value.Damage == 15f
                        && record.Summary.Contains("Committed test strike"), "historical evidence changed with authoring asset");
                Pass("Original recovery/completion; stable historical action values and name");

                Require(actions.TryAttack(definition) && actions.CommittedAttack.Damage == 100f
                    && actions.CommittedAttack.Windup == 10f && actions.CommittedAttack.StaminaCost == 30f,
                    "next attack did not capture updated definition");
                actions.enabled = false;
                Require(!actions.HasCommittedAttack && actions.Diagnostics.Interrupted == 1
                    && actions.Diagnostics.Records[actions.Diagnostics.Records.Count - 1].Attack.Value.Damage == 100f,
                    "interruption lost or retained active snapshot");
                actions.enabled = true;
                Require(actions.TryDodge() && !actions.HasCommittedAttack, "dodge inherited attack snapshot");
                Pass("Next attack captures edits; interruption clears snapshot; dodge has no attack snapshot");

                actions.enabled = false;
                actions.enabled = true;
                UnityEngine.Object.DestroyImmediate(pivot.gameObject);
                CheckMissFeedback(actor, actions, target, committed);
            }
            catch (Exception exception) { errors.Add(exception.ToString()); }
            finally
            {
                if (actor != null) UnityEngine.Object.DestroyImmediate(actor);
                if (dummy != null) UnityEngine.Object.DestroyImmediate(dummy);
                if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
                Application.logMessageReceived -= capture;
                Directory.CreateDirectory("Logs");
                string report = "Unity " + Application.unityVersion + " isolated attack snapshot Play-mode checks\n"
                    + string.Join("\n", results) + "\n"
                    + (errors.Count == 0 ? "PASS: all checks; no runtime Console errors captured." : string.Join("\n", errors));
                File.WriteAllText("Logs/AttackSnapshotChecks.txt", report);
                Debug.Log(report);
                EditorApplication.ExitPlaymode();
            }
        }

        private static void CheckMissFeedback(GameObject actor, CombatActions actions, TrainingTarget target, AttackSnapshot attack)
        {
            var detector = actor.GetComponent<MeleeHitDetector>();
            var hitTargets = new HashSet<HitReceiver>();
            Vector3 targetPosition = target.transform.position;
            target.ResetTarget();
            target.transform.position += Vector3.forward * 10f;
            Physics.SyncTransforms();
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out var reason) == 0
                && reason == MeleeMissReason.NoTargetInReach, "range miss explanation incorrect");
            target.transform.position = targetPosition;
            Physics.SyncTransforms();
            Require(detector.ResolveSwing(attack, Quaternion.Euler(0, 180, 0), hitTargets, out reason) == 0
                && reason == MeleeMissReason.OutsideArc, "facing miss explanation incorrect");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.SetParent(actor.transform, false);
            wall.transform.localPosition = new Vector3(0.25f, 1f, 0.75f);
            wall.transform.localScale = new Vector3(2f, 2f, 0.1f);
            Physics.SyncTransforms();
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out reason) == 0
                && reason == MeleeMissReason.Obstructed, "wall miss explanation incorrect");
            UnityEngine.Object.DestroyImmediate(wall);
            for (int i = 0; i < 20; i++) target.ReceiveHit(new HitData(attack));
            Physics.SyncTransforms();
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out reason) == 0
                && reason == MeleeMissReason.TargetUnavailable, "defeated target explanation incorrect");
            target.ResetTarget();
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out reason) == 1
                && reason == MeleeMissReason.None, "successful hit reported as miss");
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out reason) == 0
                && reason == MeleeMissReason.None, "deduplicated hit reported as miss");
            var crowded = new GameObject("Temporary crowded query");
            crowded.transform.SetParent(actor.transform, false);
            for (int i = 0; i < 65; i++)
            {
                var collider = new GameObject("Query collider");
                collider.transform.SetParent(crowded.transform, false);
                collider.transform.localPosition = new Vector3(0, 1, 0.4f);
                collider.AddComponent<BoxCollider>();
            }
            target.transform.position += Vector3.forward * 10f;
            hitTargets.Clear(); Physics.SyncTransforms();
            Require(detector.ResolveSwing(attack, Quaternion.identity, hitTargets, out reason) == 0
                && reason == MeleeMissReason.QueryCapacityReached, "saturated query claimed complete evidence");
            UnityEngine.Object.DestroyImmediate(crowded);
            Pass("Range, facing, wall, defeated-target and saturated-query explanations; hits are not misses");

            var profile = ScriptableObject.CreateInstance<AttackDefinition>();
            profile.Windup = profile.ActiveTime = profile.Recovery = 1f;
            try
            {
                // Far target, direct accepted action: verify explanation persists into its completed observation.
                Require(actions.TryAttack(profile), "could not start miss fixture");
                TickAt(actions, profile.Windup + 0.1f);
                TickAt(actions, profile.Duration + 0.1f);
                var completed = actions.Diagnostics.Records[actions.Diagnostics.Records.Count - 1];
                Require(completed.Kind == CombatObservationKind.Completed && completed.TargetsHit == 0
                    && completed.MissReason == MeleeMissReason.NoTargetInReach
                    && completed.Summary.Contains("no target in reach")
                    && actions.Message.Contains("no target in reach"), "completed miss lost geometry evidence");
                Require(actions.TryDodge(), "could not start dodge after completed miss");
                TickAt(actions, 2f);
                completed = actions.Diagnostics.Records[actions.Diagnostics.Records.Count - 1];
                Require(completed.ActionKind == CombatActionKind.Dodge && completed.MissReason == MeleeMissReason.None
                    && completed.Summary.Contains("evasion unverified"), "dodge inherited miss evidence");
            }
            finally { UnityEngine.Object.DestroyImmediate(profile); }
            Pass("Completed miss preserves reason in action/HUD history; dodge remains unverified evasion");
        }

        private static void TickAt(CombatActions actions, float elapsed)
        {
            typeof(CombatActions).GetField("elapsed", Private).SetValue(actions, elapsed);
            Invoke(actions, "TickAction");
        }
        private static void Invoke(object instance, string method)
            => instance.GetType().GetMethod(method, Private).Invoke(instance, null);
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
        private static void Pass(string message) => results.Add("PASS " + message);
    }
}
