using System;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class EnemySandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/EnemyTest.unity";
        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (!File.Exists(ScenePath) && !SessionState.GetBool("EvolvingRPG.EnemySceneAttempted", false)) EditorApplication.update += CreateWhenIdle;
        }
        private static void CreateWhenIdle()
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= CreateWhenIdle;
            SessionState.SetBool("EvolvingRPG.EnemySceneAttempted", true); Create();
        }
        [MenuItem("Tools/EvolvingRPG/Create Enemy Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before creating EnemyTest.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing EnemyTest."); return; }
            if (!AssetDatabase.CopyAsset(CombatSandboxBuilder.ScenePath, ScenePath)) throw new IOException("Could not copy CombatTest.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                CombatActions player = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.GetComponent<TrainingTarget>() != null || root.GetComponent<CombatSandboxHUD>() != null)
                    { UnityEngine.Object.DestroyImmediate(root); continue; }
                    if (player == null) player = root.GetComponentInChildren<CombatActions>();
                    if (root.name == "Graybox")
                    {
                        for (int i = root.transform.childCount - 1; i >= 0; i--)
                        {
                            var child = root.transform.GetChild(i);
                            if (child.name == "Floor" || child.name.EndsWith("boundary")) continue;
                            UnityEngine.Object.DestroyImmediate(child.gameObject);
                        }
                    }
                }
                if (player == null) throw new InvalidOperationException("CombatTest has no actor.");
                player.transform.SetPositionAndRotation(new Vector3(0, 0.05f, -7), Quaternion.identity);
                var health = player.gameObject.AddComponent<CombatVitality>();
                health.Configure(150, 40, 0.65f);
                var guarded = Enemy("Guarded fighter (left)", new Vector3(-7, 0.05f, 3), false, health);
                var fast = Enemy("Fast fighter (right)", new Vector3(7, 0.05f, 3), true, health);
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Line of sight test wall"; wall.transform.position = new Vector3(0, 1.5f, 6); wall.transform.localScale = new Vector3(2, 3, 3);
                new GameObject("Enemy encounter HUD").AddComponent<EnemySandboxHUD>().Configure(player, new[] { guarded, fast });
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save EnemyTest.");
                AssetDatabase.SaveAssets();
                Debug.Log("Created " + ScenePath + ". Open and press Play. Existing scenes preserved.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        private static SandboxEnemy Enemy(string name, Vector3 position, bool fast, CombatVitality player)
        {
            var root = new GameObject(name); root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, 180, 0));
            var motor = root.AddComponent<CharacterController>(); motor.center = Vector3.up; motor.height = 2; motor.radius = 0.4f;
            motor.stepOffset = 0.3f; motor.skinWidth = 0.04f;
            var brain = root.AddComponent<SandboxEnemy>(); brain.Configure(player, fast);
            root.AddComponent<EnemyVitality>().Configure(fast ? 160 : 240, fast ? 30 : 60, fast ? 0.85f : 1.2f);
            var body = Visual(root.transform, "Body", PrimitiveType.Capsule, Vector3.up, Vector3.one);
            var pivot = new GameObject("Weapon pivot").transform; pivot.SetParent(root.transform, false); pivot.localPosition = Vector3.up * 1.1f;
            Visual(pivot, "Weapon", PrimitiveType.Cube, Vector3.forward * 0.8f, new Vector3(0.14f, 0.14f, 1.3f));
            var shield = Visual(root.transform, "Guard indicator", PrimitiveType.Cube, new Vector3(0, 1.1f, 0.55f), new Vector3(1f, 0.8f, 0.12f));
            var line = new GameObject("Strike telegraph").AddComponent<LineRenderer>();
            line.transform.SetParent(root.transform, false); line.useWorldSpace = false; line.widthMultiplier = 0.055f;
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Combat/Data/RangeGuide.mat"); line.enabled = false;
            root.AddComponent<EnemyFeedback>().Configure(body.GetComponent<Renderer>(), pivot, shield, line);
            return brain;
        }
        private static GameObject Visual(Transform parent, string name, PrimitiveType shape, Vector3 position, Vector3 scale)
        {
            var visual = GameObject.CreatePrimitive(shape); visual.name = name;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(parent, false); visual.transform.localPosition = position; visual.transform.localScale = scale;
            visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Combat/Data/PracticeBody.mat");
            return visual;
        }
    }
}
