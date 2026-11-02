using System;
using System.Collections.Generic;
using System.IO;
using EvolvingRPG.Movement;
using StarterAssets;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // Bounded integration checks in the existing Editor, using virtual input devices.
    // Scene edits are never saved; the runtime arena and devices disappear on exiting Play mode.
    [InitializeOnLoad]
    public static class StrafingPlayModeChecks
    {
        private const string Requested = "EvolvingRPG.StrafingChecksRequested";
        private static readonly List<string> results = new List<string>();
        private static readonly List<string> errors = new List<string>();
        private static readonly Key[][] keys =
        {
            new[] { Key.Q }, new[] { Key.E }, new[] { Key.Q, Key.W }, new[] { Key.E, Key.W },
            new[] { Key.Q, Key.S }, new[] { Key.E, Key.S }, new[] { Key.Q, Key.E },
            new[] { Key.Q, Key.E, Key.W }, new[] { Key.W }, new[] { Key.A }, new[] { Key.D },
            new[] { Key.Q, Key.Space }, Array.Empty<Key>()
        };
        private static readonly Vector2[] directions =
        {
            Vector2.left, Vector2.right, new Vector2(-1, 1), new Vector2(1, 1),
            new Vector2(-1, -1), new Vector2(1, -1), Vector2.zero, Vector2.up,
            Vector2.up, Vector2.left, Vector2.right, Vector2.left, Vector2.zero
        };
        private static readonly string[] names =
        {
            "Q", "E", "Q+W", "E+W", "Q+S", "E+S", "Q+E", "Q+E+W",
            "W regression", "A regression", "D regression", "Q+Space jump", "Mouse-look and zoom"
        };
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static ThirdPersonController controller;
        private static CinemachineThirdPersonFollow follow;
        private static Camera camera;
        private static bool running;
        private static bool sampling;
        private static int step;
        private static int lastFrame;
        private static float started;
        private static float deadline;
        private static float peakHeight;
        private static float distanceBefore;
        private static float referenceSpeed;
        private static Vector3 startPosition;
        private static Quaternion facing;
        private static Quaternion cameraRotation;
        private static Quaternion targetRotation;
        private static Vector3 expectedDirection;

        static StrafingPlayModeChecks()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Tools/EvolvingRPG/Run Q-E Strafing Play Mode Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before running checks.");
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().path != MovementPlaygroundBuilder.ScenePath)
                throw new InvalidOperationException("Open only MovementTest before running checks.");
            SessionState.SetBool(Requested, true);
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Requested, false))
            {
                SessionState.SetBool(Requested, false);
                try { Begin(); }
                catch (Exception exception) { errors.Add(exception.ToString()); Finish(); }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode && running)
            {
                errors.Add("Checks interrupted before completion.");
                Finish(false);
            }
        }

        private static void Begin()
        {
            results.Clear();
            errors.Clear();
            running = true;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (controller == null) controller = root.GetComponentInChildren<ThirdPersonController>();
                if (follow == null) follow = root.GetComponentInChildren<CinemachineThirdPersonFollow>();
            }
            if (controller == null || controller.GetComponent<DedicatedStrafe>() == null)
                throw new InvalidOperationException("Install Q/E strafing before running checks.");
            camera = Camera.main;
            keyboard = InputSystem.AddDevice<Keyboard>("StrafingCheckKeyboard");
            mouse = InputSystem.AddDevice<Mouse>("StrafingCheckMouse");
            var playerInput = controller.GetComponent<PlayerInput>();
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.SwitchCurrentControlScheme("KeyboardMouse", keyboard, mouse);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Temporary strafing check floor";
            floor.transform.position = new Vector3(500, -0.5f, 500);
            floor.transform.localScale = new Vector3(80, 1, 80);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Application.logMessageReceived += RecordError;
            step = 0;
            lastFrame = -1;
            sampling = false;
            started = Time.unscaledTime;
            deadline = (float)EditorApplication.timeSinceStartup + 45f;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("Checks timed out. Keep Unity focused while they run.");
                if (!EditorApplication.isPlaying || Time.frameCount == lastFrame)
                    return;
                lastFrame = Time.frameCount;
                if (!sampling)
                {
                    Queue(Array.Empty<Key>());
                    if (Time.unscaledTime - started < 0.4f)
                        return;
                    var character = controller.GetComponent<CharacterController>();
                    character.enabled = false;
                    controller.transform.SetPositionAndRotation(new Vector3(500, 0.03f, 500), Quaternion.Euler(0, 37, 0));
                    character.enabled = true;
                    Physics.SyncTransforms();
                    startPosition = controller.transform.position;
                    peakHeight = startPosition.y;
                    facing = controller.transform.rotation;
                    cameraRotation = camera.transform.rotation;
                    targetRotation = controller.CinemachineCameraTarget.transform.rotation;
                    distanceBefore = follow.CameraDistance;
                    Quaternion basis = step >= 8 && step <= 10
                        ? Quaternion.Euler(0, camera.transform.eulerAngles.y, 0) : facing;
                    expectedDirection = basis * new Vector3(directions[step].x, 0, directions[step].y).normalized;
                    sampling = true;
                    started = Time.unscaledTime;
                }
                Queue(keys[step]);
                if (step == 12 && Time.unscaledTime - started < 0.08f)
                    InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(20, 0), scroll = new Vector2(0, 120) });
                peakHeight = Mathf.Max(peakHeight, controller.transform.position.y);
                if (Time.unscaledTime - started < 0.8f)
                    return;
                Evaluate();
                if (++step == names.Length)
                {
                    Finish();
                    return;
                }
                sampling = false;
                started = Time.unscaledTime;
            }
            catch (Exception exception)
            {
                errors.Add(exception.ToString());
                Finish();
            }
        }

        private static void Evaluate()
        {
            if (step == 12)
            {
                Require(Quaternion.Angle(targetRotation, controller.CinemachineCameraTarget.transform.rotation) > 0.1f,
                    "mouse-look did not rotate the camera target");
                Require(follow.CameraDistance < distanceBefore - 0.1f, "mouse wheel did not zoom inward");
            }
            else
            {
                Vector3 delta = controller.transform.position - startPosition;
                delta.y = 0;
                float meanSpeed = delta.magnitude / (Time.unscaledTime - started);
                if (expectedDirection == Vector3.zero)
                    Require(delta.magnitude < 0.03f, "opposite keys failed to cancel");
                else
                {
                    Require(delta.magnitude > 0.5f && Vector3.Dot(delta.normalized, expectedDirection) > 0.995f,
                        "movement direction differs from expected");
                    Require(meanSpeed <= controller.MoveSpeed + 0.1f, "movement exceeded configured speed");
                    if (step == 0)
                        referenceSpeed = meanSpeed;
                    if (step >= 1 && step <= 5)
                        Require(Mathf.Abs(meanSpeed - referenceSpeed) < 0.15f, "cardinal/diagonal speeds differ");
                }
                if (step < 8 || step == 11)
                    Require(Quaternion.Angle(facing, controller.transform.rotation) < 0.1f, "strafing changed facing");
                else
                    Require(Vector3.Dot(controller.transform.forward, expectedDirection) > 0.995f, "WASD no longer turns toward movement");
                Require(Quaternion.Angle(cameraRotation, camera.transform.rotation) < 0.1f, "movement rotated camera");
                if (step == 11)
                    Require(peakHeight > startPosition.y + 0.3f, "jump did not leave the ground");
            }
            results.Add("PASS " + names[step]);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(names[step] + ": " + message);
        }

        private static void Queue(Key[] pressed)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressed));
            InputSystem.QueueStateEvent(mouse, new MouseState());
        }

        private static void RecordError(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                errors.Add(message + "\n" + stack);
        }

        private static void Finish(bool exitPlay = true)
        {
            running = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= RecordError;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            Directory.CreateDirectory("Logs");
            string report = "Unity " + Application.unityVersion + " automated Play-mode checks\n"
                + string.Join("\n", results) + "\n" + (errors.Count == 0 ? "PASS: no runtime Console errors captured." : string.Join("\n", errors));
            File.WriteAllText("Logs/StrafingPlayModeChecks.txt", report);
            Debug.Log(report);
            if (exitPlay) EditorApplication.ExitPlaymode();
        }
    }
}
