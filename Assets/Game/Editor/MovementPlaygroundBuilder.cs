using System;
using System.IO;
using EvolvingRPG.Movement;
using StarterAssets;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class MovementPlaygroundBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/MovementTest.unity";
        private const string Vendor = "Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/";

        // Create the new scene once after import, without opening or saving the user's scenes.
        [InitializeOnLoadMethod]
        private static void ScheduleFirstCreation()
        {
            if (!File.Exists(ScenePath) && !SessionState.GetBool("EvolvingRPG.MovementSceneAttempted", false))
                EditorApplication.update += CreateWhenIdle;
        }

        private static void CreateWhenIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            EditorApplication.update -= CreateWhenIdle;
            SessionState.SetBool("EvolvingRPG.MovementSceneAttempted", true);
            Create();
        }

        [MenuItem("Tools/EvolvingRPG/Create Movement Playground")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before creating the playground.");
            if (File.Exists(ScenePath))
            {
                Debug.Log("MovementTest already exists; keeping all scene edits. Open " + ScenePath);
                return;
            }

            GameObject playerAsset = Load("PlayerArmature");
            GameObject cameraAsset = Load("MainCamera");
            GameObject followAsset = Load("PlayerFollowCamera");
            Directory.CreateDirectory("Assets/Game/Scenes");
            AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var cameraObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraAsset, scene);
                cameraObject.name = "MainCamera";
                cameraObject.GetComponent<Camera>().nearClipPlane = 0.1f;

                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset, scene);
                player.name = "Player";
                player.transform.position = new Vector3(0f, 0.05f, -10f);
                var controller = player.GetComponent<ThirdPersonController>();
                controller.MoveSpeed = 4f;
                controller.SprintSpeed = 6f;
                controller.RotationSmoothTime = 0.08f;
                controller.SpeedChangeRate = 18f;
                controller.GroundLayers = 1; // Graybox uses Default; vendor player uses Player layer.
                controller.TopClamp = 70f;
                controller.BottomClamp = -30f;
                player.AddComponent<PlaygroundMouseLook>();
                player.GetComponent<StarterAssetsInputs>().cursorInputForLook = false;

                var rig = (GameObject)PrefabUtility.InstantiatePrefab(followAsset, scene);
                rig.name = "PlayerFollowCamera";
                var virtualCamera = rig.GetComponent<CinemachineCamera>();
                virtualCamera.Follow = controller.CinemachineCameraTarget.transform;
                virtualCamera.Lens.FieldOfView = 55f;
                virtualCamera.Lens.NearClipPlane = 0.1f;
                var follow = rig.GetComponent<CinemachineThirdPersonFollow>();
                follow.CameraDistance = 4f;
                follow.ShoulderOffset = new Vector3(0.35f, 0f, 0f);
                follow.Damping = new Vector3(0.05f, 0.1f, 0.1f);
                follow.AvoidObstacles.Enabled = true;
                follow.AvoidObstacles.CollisionFilter = 1;
                follow.AvoidObstacles.IgnoreTag = "Player";
                follow.AvoidObstacles.CameraRadius = 0.25f;
                follow.AvoidObstacles.DampingIntoCollision = 0f;
                follow.AvoidObstacles.DampingFromCollision = 0.2f;
                var noise = rig.GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise != null)
                    noise.enabled = false;
                rig.AddComponent<ThirdPersonCameraZoom>();

                var graybox = new GameObject("Graybox").transform;
                Box(graybox, "Floor", new Vector3(0, -0.5f, 0), new Vector3(40, 1, 40));
                Box(graybox, "North boundary", new Vector3(0, 2, 20), new Vector3(41, 4, 1));
                Box(graybox, "South boundary", new Vector3(0, 2, -20), new Vector3(41, 4, 1));
                Box(graybox, "West boundary", new Vector3(-20, 2, 0), new Vector3(1, 4, 40));
                Box(graybox, "East boundary", new Vector3(20, 2, 0), new Vector3(1, 4, 40));
                Box(graybox, "Obstacle A", new Vector3(-4, 1.5f, -2), new Vector3(3, 3, 3));
                Box(graybox, "Obstacle B", new Vector3(4, 1, 2), new Vector3(3, 2, 4));
                Box(graybox, "Low jump block", new Vector3(-5, 0.35f, -9), new Vector3(3, 0.7f, 2));
                Box(graybox, "Corridor west", new Vector3(7, 2, 10), new Vector3(1, 4, 12));
                Box(graybox, "Corridor east", new Vector3(10.5f, 2, 10), new Vector3(1, 4, 12));
                Box(graybox, "Corridor ceiling", new Vector3(8.75f, 3.75f, 10), new Vector3(2.5f, 0.5f, 6));
                var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                dummy.name = "Circling marker (inert)";
                dummy.transform.SetParent(graybox);
                dummy.transform.position = new Vector3(0, 1, 5);
                var light = new GameObject("Sun").AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.65f);

                // Persist prefab overrides through Unity serialization, never hand-authored YAML.
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component))
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Could not save " + ScenePath);
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/MovementPlaygroundCreation.txt",
                    "Unity " + Application.unityVersion + " compiled and created " + ScenePath
                    + " using imported Starter Assets and Cinemachine. No Play mode test was run.\n");
                Debug.Log("Movement playground created. Open " + ScenePath + " and press Play.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded)
                    SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static GameObject Load(string name)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Vendor + name + ".prefab");
            if (asset == null)
                throw new InvalidOperationException("Missing imported Starter Assets prefab: " + Vendor + name);
            return asset;
        }

        private static void Box(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
        }
    }
}
