using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using StarterAssets;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    [InitializeOnLoad]
    public static class CombatPlayModeChecks
    {
        private const string Requested = "EvolvingRPG.CombatChecksRequested";
        private sealed class Check
        {
            public string Name;
            public float Duration;
            public Action Begin;
            public Action<float> Frame;
            public Action Verify;
        }
        private static readonly List<Check> checks = new List<Check>();
        private static readonly List<string> results = new List<string>();
        private static readonly List<string> errors = new List<string>();
        private static CombatActions actions;
        private static TrainingTarget target;
        private static CombatStamina stamina;
        private static ThirdPersonController controller;
        private static CinemachineThirdPersonFollow follow;
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static GameObject temporary;
        private static bool running, sampling;
        private static int step, frame, accepted;
        private static int jumpTakeoffs;
        private static bool aboveJumpThreshold;
        private static float started, deadline, peakHeight, cameraDistance;
        private static Vector3 startPosition;
        private static Quaternion cameraRotation;
        private static KeyboardState pendingKeys;
        private static MouseState pendingMouse;

        static CombatPlayModeChecks() => EditorApplication.playModeStateChanged += ModeChanged;

        [MenuItem("Tools/EvolvingRPG/Run Combat Play Mode Checks %#F8")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save your scene edits before running combat checks.");
            EditorSceneManager.OpenScene(CombatSandboxBuilder.ScenePath);
            SessionState.SetBool(Requested, true);
            EditorApplication.EnterPlaymode();
        }

        private static void ModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Requested, false))
            {
                SessionState.SetBool(Requested, false);
                try { Begin(); } catch (Exception exception) { errors.Add(exception.ToString()); Finish(); }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode && running)
            { errors.Add("Checks interrupted."); Finish(false); }
        }

        private static void Begin()
        {
            results.Clear(); errors.Clear(); checks.Clear();
            actions = null; target = null; follow = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (actions == null) actions = root.GetComponentInChildren<CombatActions>();
                if (target == null) target = root.GetComponentInChildren<TrainingTarget>();
                if (follow == null) follow = root.GetComponentInChildren<CinemachineThirdPersonFollow>();
            }
            if (actions == null || target == null) throw new InvalidOperationException("Missing combat scene components.");
            controller = actions.GetComponent<ThirdPersonController>();
            stamina = actions.GetComponent<CombatStamina>();
            keyboard = InputSystem.AddDevice<Keyboard>("CombatCheckKeyboard");
            mouse = InputSystem.AddDevice<Mouse>("CombatCheckMouse");
            var playerInput = actions.GetComponent<PlayerInput>();
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);
            Queue(Array.Empty<Key>());
            InputSystem.onBeforeUpdate += InjectInput;
            // Standalone game actions are not device-paired by PlayerInput. Bind these test-only
            // instances explicitly, so the physical click that launched the menu cannot hold them down.
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var component in root.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (component == null || component.GetType().Namespace == null
                        || !component.GetType().Namespace.StartsWith("EvolvingRPG")) continue;
                    foreach (var fieldInfo in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (fieldInfo.FieldType != typeof(InputAction)) continue;
                        var action = (InputAction)fieldInfo.GetValue(component);
                        for (int i = 0; i < action.bindings.Count; i++)
                            action.ApplyBindingOverride(i, action.bindings[i].effectivePath
                                .Replace("<Mouse>", mouse.path).Replace("<Keyboard>", keyboard.path));
                    }
                }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            BuildChecks();
            running = true; sampling = false; step = 0; frame = -1;
            started = Time.time;
            deadline = (float)EditorApplication.timeSinceStartup + 90f;
            Application.logMessageReceived += RecordError;
            EditorApplication.update += Tick;
        }

        private static void Add(string name, float duration, Action begin, Action<float> input, Action verify)
        { checks.Add(new Check { Name = name, Duration = duration, Begin = begin, Frame = input, Verify = verify }); }
        private static void AttackInput(bool heavy, bool held, float time)
        { Queue(Array.Empty<Key>(), time < 0.05f || held ? (ushort)(heavy ? 2 : 1) : (ushort)0); }
        private static void HitOnce(AttackDefinition attack)
        {
            Require(Mathf.Abs(target.Health - (target.MaximumHealth - attack.Damage)) < 0.01f,
                $"expected HP {target.MaximumHealth - attack.Damage}, actual {target.Health}, accepted {actions.AcceptedActions - accepted}, phase {actions.Phase}, message {actions.Message}, position {actions.transform.position}");
            Require(actions.AcceptedActions == accepted + 1, "unexpected action repeat/overlap");
            var log = actions.Diagnostics;
            Require(log.Accepted == 1 && log.Hits == 1 && log.Misses == 0, "incorrect diagnostic action/hit counts");
            Require(log.Records.Count == 3 && log.Records[0].Kind == CombatObservationKind.Accepted
                && log.Records[1].Kind == CombatObservationKind.HitResolved
                && log.Records[2].Kind == CombatObservationKind.Completed, "incorrect diagnostic lifecycle");
            foreach (var observation in log.Records)
                Require(observation.ActionSequence == actions.AcceptedActions && observation.Definition == attack,
                    "diagnostic action identity mismatch");
            Require(log.Records[1].Target == target && Mathf.Abs(log.Records[1].Resolution.DamageApplied - attack.Damage) < 0.01f,
                "resolved damage evidence mismatch");
        }

        private static void BuildChecks()
        {
            Add("Light damage, poise and single hit", 0.7f, null, t => AttackInput(false, false, t), () =>
            { HitOnce(actions.LightAttack); Require(target.Poise > 0, "no poise impact"); });
            Add("Heavy timing, damage and commitment", 1.5f, null, t =>
            {
                Queue(t < 1.25f ? new[] { Key.W } : Array.Empty<Key>(), t < 0.05f ? (ushort)2 : (ushort)0);
                if (t > 0.15f && t < 0.3f) Require(target.Health == target.MaximumHealth, "heavy hit during windup");
            }, () =>
            {
                HitOnce(actions.HeavyAttack);
                Require(actions.HeavyAttack.Damage > actions.LightAttack.Damage && actions.HeavyAttack.PoiseDamage > actions.LightAttack.PoiseDamage, "heavy not distinct");
                Require(Vector3.Distance(startPosition, actions.transform.position) < 0.1f, "heavy did not restrict movement");
            });
            Add("Holding Light does not auto-repeat", 1.2f, null, t => AttackInput(false, true, t), () => HitOnce(actions.LightAttack));
            Add("Heavy rejects attack/dodge cancel attempts", 1.5f, null, t =>
                Queue(t > 0.75f && t < 0.82f ? new[] { Key.LeftShift } : Array.Empty<Key>(),
                    t < 0.05f ? (ushort)2 : t > 0.65f && t < 0.72f ? (ushort)1 : (ushort)0), () => HitOnce(actions.HeavyAttack));
            Add("Out-of-range attack misses", 0.7f, () => Teleport(new Vector3(0, 0.03f, -8), Quaternion.identity),
                t => AttackInput(false, false, t), () => VerifyMiss(MeleeMissReason.NoTargetInReach));
            Add("Wrong-facing attack misses", 0.7f, () => Teleport(startPosition, Quaternion.Euler(0, 180, 0)),
                t => AttackInput(false, false, t), () => VerifyMiss(MeleeMissReason.OutsideArc));
            Add("Walls obstruct melee hits", 0.7f, () =>
            {
                temporary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                temporary.transform.position = new Vector3(0, 1, -1);
                temporary.transform.localScale = new Vector3(2, 2, 0.2f);
                Physics.SyncTransforms();
            }, t => AttackInput(false, false, t), () => VerifyMiss(MeleeMissReason.Obstructed));
            Add("Multiple target colliders count once", 0.7f, () =>
            {
                temporary = new GameObject("Temporary target collider");
                temporary.transform.SetParent(target.transform, false);
                temporary.transform.localPosition = Vector3.up;
                temporary.AddComponent<SphereCollider>();
                Physics.SyncTransforms();
            }, t => AttackInput(false, false, t), () => HitOnce(actions.LightAttack));
            Add("Poise threshold produces stagger", 0.1f, () =>
            { target.ReceiveHit(new HitData(actions.HeavyAttack)); target.ReceiveHit(new HitData(actions.HeavyAttack)); }, null,
                () => Require(target.Staggered && target.Poise == 0f, "poise did not break"));
            AddDodge("Q + Shift dodge left", new[] { Key.Q, Key.LeftShift }, Vector3.left, false);
            AddDodge("Q + W + Shift diagonal dodge", new[] { Key.Q, Key.W, Key.LeftShift }, new Vector3(-1, 0, 1).normalized, false);
            AddDodge("Held Shift cannot chain dodges", new[] { Key.LeftShift }, Vector3.back, true);
            Add("Dodge stops at solid wall", 0.9f, () =>
            {
                temporary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                temporary.transform.position = new Vector3(-1, 1, -2);
                temporary.transform.localScale = new Vector3(0.2f, 2, 4);
                Physics.SyncTransforms();
            }, t => Queue(t < 0.05f ? new[] { Key.Q, Key.LeftShift } : Array.Empty<Key>()), () =>
                Require(Vector3.Distance(startPosition, actions.transform.position) < 1f, "dodge passed through wall"));
            Add("Insufficient stamina rejects actions", 0.1f, () =>
            {
                stamina.TrySpend(stamina.Current);
                Require(!actions.TryDodge() && !actions.TryAttack(actions.HeavyAttack), "zero stamina action accepted");
            }, null, () => Require(actions.AcceptedActions == accepted, "empty stamina accepted action"));
            Add("One Space tap produces one jump then settles", 3f, null,
                t => Queue(t < 0.05f ? new[] { Key.Space } : Array.Empty<Key>()), VerifySingleJump);
            Add("Holding Space does not repeat after landing", 3f, null,
                t => Queue(new[] { Key.Space }), VerifySingleJump);
            Add("A fresh Space press allows a second jump", 3.6f, null,
                t => Queue(t < 0.05f || (t > 1.6f && t < 1.7f) ? new[] { Key.Space } : Array.Empty<Key>()), () =>
                Require(jumpTakeoffs == 2 && controller.Grounded, $"expected two separate jumps and landing, got {jumpTakeoffs}"));
            Add("Space during Heavy is discarded without a delayed jump", 2f,
                () => Require(actions.TryAttack(actions.HeavyAttack), "could not start Heavy for blocked jump"),
                t => Queue(t > 0.15f && t < 0.25f ? new[] { Key.Space } : Array.Empty<Key>()), () =>
                Require(jumpTakeoffs == 0 && controller.Grounded && !actions.GetComponent<StarterAssetsInputs>().jump,
                    "blocked Space leaked past commitment"));
            Add("WASD and Space jump regression", 0.8f, null, t => Queue(t < 0.05f ? new[] { Key.W, Key.Space } : new[] { Key.W }), () =>
                Require(peakHeight > startPosition.y + 0.3f && actions.transform.position.z > startPosition.z + 0.3f, "movement/jump failed"));
            Add("Mouse-look and zoom during Heavy", 1.5f, null, t =>
            {
                AttackInput(true, false, t);
                if (t > 0.15f && t < 0.2f) pendingMouse = new MouseState { delta = new Vector2(20, 0), scroll = new Vector2(0, 120) };
            }, () =>
            {
                Require(Quaternion.Angle(cameraRotation, controller.CinemachineCameraTarget.transform.rotation) > 0.1f, "camera look blocked");
                Require(follow.CameraDistance < cameraDistance - 0.1f, "zoom blocked");
                Require(Quaternion.Angle(actions.transform.rotation, Quaternion.identity) < 0.1f, "camera turned committed character");
            });
            Add("R resets target", 0.2f, () => target.ReceiveHit(new HitData(actions.HeavyAttack)), t => Queue(new[] { Key.R }),
                () => Require(target.Health == target.MaximumHealth && target.Poise == 0, "target reset failed"));
            Add("Defeated target rejects hits then resets", 3.3f, () =>
            {
                for (int i = 0; i < 10; i++) target.ReceiveHit(new HitData(actions.HeavyAttack));
                Require(target.Defeated && !target.ReceiveHit(new HitData(actions.LightAttack)), "dead target accepted hit");
            }, null, () => Require(target.Health == target.MaximumHealth, "automatic target reset failed"));
            Add("Stamina regenerates during committed action", 0.3f, () =>
            { Require(actions.TryAttack(actions.HeavyAttack), "could not start heavy"); }, null, () =>
                Require(actions.Busy && stamina.Current > stamina.Maximum - actions.HeavyAttack.StaminaCost, "active stamina regeneration failed"));
            Add("Resolved receipts cap overkill and staggered poise", 0.1f, () =>
            {
                var hit = new HitData(actions.HeavyAttack);
                target.ReceiveHit(hit, out HitResolution first);
                target.ReceiveHit(hit, out HitResolution second);
                target.ReceiveHit(hit, out HitResolution third);
                Require(first.PoiseApplied == 40f && second.PoiseApplied == 20f && second.PoiseBroken
                    && third.PoiseApplied == 0f, "poise receipt does not reflect applied outcome");
                for (int i = 0; i < 20 && target.Health > hit.Damage; i++) target.ReceiveHit(hit);
                float remaining = target.Health;
                Require(target.ReceiveHit(hit, out HitResolution final) && final.DamageApplied == remaining && final.Defeated,
                    "overkill receipt exceeded remaining health");
                Require(!target.ReceiveHit(hit, out _), "dead target produced a new receipt");
            }, null, () => Require(actions.Diagnostics.Records.Count == 0, "external training hits attributed to player actions"));
            Add("Observation history is bounded and clearable", 0.1f, () =>
            {
                var log = new CombatObservationLog();
                var record = (Action<CombatObservation>)Delegate.CreateDelegate(typeof(Action<CombatObservation>), log,
                    typeof(CombatObservationLog).GetMethod("Record", BindingFlags.Instance | BindingFlags.NonPublic));
                for (int i = 1; i <= 70; i++)
                {
                    record(new CombatObservation(i, CombatObservationKind.Accepted, CombatActionKind.Attack,
                        actions.LightAttack, null, default, 0));
                    record(new CombatObservation(i, CombatObservationKind.Interrupted, CombatActionKind.Attack,
                        actions.LightAttack, null, default, 0));
                }
                Require(log.Records.Count == CombatObservationLog.Capacity && log.Accepted == 70 && log.Interrupted == 70
                    && log.Records[0].ActionSequence == 39 && log.Records[63].ActionSequence == 70,
                    "history eviction/order or totals incorrect");
                log.Clear();
                Require(log.Records.Count == 0 && log.Accepted == 0 && log.Interrupted == 0, "clear failed");
            }, null, () => Require(!actions.Busy, "diagnostic clear changed action state"));
            Add("Interrupted action never reports completion", 0.1f, () =>
            {
                // Isolate lifecycle behavior from the previous movement tests' teleport/vertical state.
                // Grounding and real input remain covered by the earlier integration checks.
                bool grounded = controller.Grounded;
                controller.Grounded = true;
                try
                {
                    Require(actions.TryAttack(actions.HeavyAttack), "could not start interrupted attack: " + actions.Message);
                    actions.enabled = false;
                }
                finally { controller.Grounded = grounded; }
            }, null, () =>
            {
                var records = actions.Diagnostics.Records;
                Require(records.Count == 2 && records[0].Kind == CombatObservationKind.Accepted
                    && records[1].Kind == CombatObservationKind.Interrupted && actions.Diagnostics.Hits == 0,
                    "interruption incorrectly counted as success");
                actions.enabled = true;
            });
        }

        private static void AddDodge(string name, Key[] pressed, Vector3 expected, bool held)
        {
            Add(name, held ? 1.4f : 0.9f, () => Teleport(new Vector3(0, 0.03f, -12), Quaternion.identity),
                t => Queue(t < 0.05f || held ? pressed : Array.Empty<Key>()), () =>
            {
                Vector3 displacement = Vector3.ProjectOnPlane(actions.transform.position - startPosition, Vector3.up);
                Require(Mathf.Abs(displacement.magnitude - 3f) < 0.12f && Vector3.Dot(displacement.normalized, expected) > 0.995f,
                    $"incorrect dodge direction/distance: {displacement}, length {displacement.magnitude}");
                Require(actions.AcceptedActions == accepted + 1 && !actions.Busy, "dodge repeated or never recovered");
                Require(actions.Diagnostics.DodgesCompleted == 1 && actions.Diagnostics.Hits == 0
                    && actions.Diagnostics.Latest.Contains("evasion unverified"), "dodge misclassified as successful evasion");
                Require(Quaternion.Angle(actions.transform.rotation, Quaternion.identity) < 0.1f, "dodge changed facing");
                Require(Quaternion.Angle(cameraRotation, controller.CinemachineCameraTarget.transform.rotation) < 0.1f, "dodge rotated camera");
            });
        }

        private static void VerifyMiss(MeleeMissReason expected)
        {
            Require(target.Health == target.MaximumHealth, "miss damaged target");
            var records = actions.Diagnostics.Records;
            Require(records.Count == 2 && records[1].Kind == CombatObservationKind.Completed
                && records[1].MissReason == expected && actions.Diagnostics.Misses == 1,
                "miss did not retain expected geometry explanation: " + expected);
        }

        private static void VerifySingleJump()
        {
            Require(jumpTakeoffs == 1 && peakHeight > startPosition.y + 0.3f,
                $"expected one jump, got {jumpTakeoffs}, peak {peakHeight}");
            Require(controller.Grounded && Mathf.Abs(actions.transform.position.y - startPosition.y) < 0.12f
                && !actions.GetComponent<StarterAssetsInputs>().jump, "jump did not settle or consumed input was restored");
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Keep Unity focused while checks run.");
                if (!EditorApplication.isPlaying || frame == Time.frameCount) return;
                frame = Time.frameCount;
                if (!sampling)
                {
                    Queue(Array.Empty<Key>());
                    if (actions.Busy || stamina.Current < stamina.Maximum - 0.1f || Time.time - started < 0.4f) return;
                    target.ResetTarget();
                    Teleport(new Vector3(0, 0.03f, -2), Quaternion.identity);
                    startPosition = actions.transform.position; peakHeight = startPosition.y;
                    cameraRotation = controller.CinemachineCameraTarget.transform.rotation;
                    cameraDistance = follow.CameraDistance;
                    accepted = actions.AcceptedActions;
                    actions.Diagnostics.Clear();
                    checks[step].Begin?.Invoke();
                    startPosition = actions.transform.position;
                    peakHeight = startPosition.y;
                    jumpTakeoffs = 0; aboveJumpThreshold = false;
                    sampling = true; started = Time.time;
                }
                float elapsed = Time.time - started;
                if (checks[step].Frame != null) checks[step].Frame(elapsed);
                else Queue(Array.Empty<Key>());
                peakHeight = Mathf.Max(peakHeight, actions.transform.position.y);
                bool above = actions.transform.position.y > startPosition.y + 0.2f;
                if (above && !aboveJumpThreshold) jumpTakeoffs++;
                aboveJumpThreshold = above;
                if (elapsed < checks[step].Duration) return;
                checks[step].Verify();
                results.Add("PASS " + checks[step].Name);
                if (temporary != null) UnityEngine.Object.Destroy(temporary);
                if (++step == checks.Count) { Finish(); return; }
                sampling = false; started = Time.time;
            }
            catch (Exception exception) { errors.Add((step < checks.Count ? checks[step].Name + ": " : "") + exception); Finish(); }
        }

        private static void Teleport(Vector3 position, Quaternion rotation)
        {
            var character = actions.GetComponent<CharacterController>();
            character.enabled = false;
            actions.transform.SetPositionAndRotation(position, rotation);
            character.enabled = true;
            Physics.SyncTransforms();
        }
        private static void Queue(Key[] pressed, ushort buttons = 0)
        {
            pendingKeys = new KeyboardState(pressed);
            pendingMouse = new MouseState { buttons = buttons };
        }
        private static void InjectInput()
        {
            // Editor input updates must not consume the button edge before the game's Update.
            if (InputState.currentUpdateType != InputUpdateType.Dynamic) return;
            InputSystem.QueueStateEvent(keyboard, pendingKeys);
            InputSystem.QueueStateEvent(mouse, pendingMouse);
            pendingMouse.delta = Vector2.zero;
            pendingMouse.scroll = Vector2.zero;
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void RecordError(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message + "\n" + stack); }

        private static void Finish(bool exit = true)
        {
            running = false;
            EditorApplication.update -= Tick;
            InputSystem.onBeforeUpdate -= InjectInput;
            Application.logMessageReceived -= RecordError;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            Directory.CreateDirectory("Logs");
            string report = "Unity " + Application.unityVersion + " automated combat Play-mode checks\n" + string.Join("\n", results)
                + "\n" + (errors.Count == 0 ? "PASS: no runtime Console errors captured." : "FAIL\n" + string.Join("\n", errors));
            File.WriteAllText("Logs/CombatPlayModeChecks.txt", report);
            Debug.Log(report);
            if (exit) EditorApplication.ExitPlaymode();
        }
    }
}
