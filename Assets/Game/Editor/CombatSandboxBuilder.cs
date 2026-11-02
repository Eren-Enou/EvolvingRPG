using System;
using System.IO;
using EvolvingRPG.Combat;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class CombatSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/CombatTest.unity";
        private const string DataPath = "Assets/Game/Combat/Data";

        [InitializeOnLoadMethod]
        private static void ScheduleCreation()
        {
            if (!File.Exists(ScenePath) && !SessionState.GetBool("EvolvingRPG.CombatSceneAttempted", false))
                EditorApplication.update += CreateWhenIdle;
        }
        private static void CreateWhenIdle()
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= CreateWhenIdle;
            SessionState.SetBool("EvolvingRPG.CombatSceneAttempted", true);
            Create();
        }

        [MenuItem("Tools/EvolvingRPG/Create Combat Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before creating the combat sandbox.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing combat sandbox: " + ScenePath); return; }
            if (!File.Exists(MovementPlaygroundBuilder.ScenePath))
                throw new InvalidOperationException("Create the movement playground first.");
            Directory.CreateDirectory(DataPath);
            AssetDatabase.Refresh();
            var light = Attack("Light", false);
            var heavy = Attack("Heavy", true);
            var bodyMaterial = Material("PracticeBody", "Universal Render Pipeline/Lit", new Color(0.4f, 0.65f, 0.45f));
            var bladeMaterial = Material("PracticeBlade", "Universal Render Pipeline/Lit", Color.gray);
            var guideMaterial = Material("RangeGuide", "Sprites/Default", Color.white);
            if (!AssetDatabase.CopyAsset(MovementPlaygroundBuilder.ScenePath, ScenePath))
                throw new IOException("Could not copy MovementTest to CombatTest.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                ThirdPersonController controller = null;
                foreach (var root in scene.GetRootGameObjects())
                    if (controller == null) controller = root.GetComponentInChildren<ThirdPersonController>();
                if (controller == null) throw new InvalidOperationException("Movement scene has no player.");
                var player = controller.gameObject;
                player.transform.position = new Vector3(0, 0.05f, -2f);
                var actions = player.AddComponent<CombatActions>();
                actions.Configure(light, heavy);
                var pivot = new GameObject("Practice blade pivot").transform;
                pivot.SetParent(player.transform, false);
                pivot.localPosition = new Vector3(0, 1.1f, 0);
                var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "Practice blade (visual only)";
                UnityEngine.Object.DestroyImmediate(blade.GetComponent<Collider>());
                blade.transform.SetParent(pivot, false);
                blade.transform.localPosition = new Vector3(0, 0, 0.9f);
                blade.transform.localScale = new Vector3(0.12f, 0.12f, 1.4f);
                blade.GetComponent<Renderer>().sharedMaterial = bladeMaterial;
                var guideObject = new GameObject("Attack reach guide");
                guideObject.transform.SetParent(player.transform, false);
                var guide = guideObject.AddComponent<LineRenderer>();
                guide.useWorldSpace = false;
                guide.widthMultiplier = 0.035f;
                guide.sharedMaterial = guideMaterial;
                guide.enabled = false;
                player.AddComponent<CombatActionFeedback>().Configure(pivot, blade.GetComponent<Renderer>(), guide);

                var targetObject = new GameObject("Training Target");
                var collider = targetObject.AddComponent<CapsuleCollider>();
                collider.center = Vector3.up;
                collider.height = 2f;
                collider.radius = 0.5f;
                var target = targetObject.AddComponent<TrainingTarget>();
                var visualPivot = new GameObject("Target reaction pivot").transform;
                visualPivot.SetParent(targetObject.transform, false);
                var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.name = "Target body (visual only)";
                UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
                visual.transform.SetParent(visualPivot, false);
                visual.transform.localPosition = Vector3.up;
                visual.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
                targetObject.AddComponent<TrainingTargetFeedback>().Configure(visual.GetComponent<Renderer>(), visualPivot);
                new GameObject("Combat debug HUD").AddComponent<CombatSandboxHUD>().Configure(actions, target);

                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component))
                            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save CombatTest.");
                AssetDatabase.SaveAssets();
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/CombatSandboxCreation.txt", "Unity " + Application.unityVersion
                    + " compiled and created " + ScenePath + " with attack assets, target, feedback and input components.\n");
                Debug.Log("Combat sandbox created. Open " + ScenePath + " and press Play.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static AttackDefinition Attack(string name, bool heavy)
        {
            string path = DataPath + "/" + name + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<AttackDefinition>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<AttackDefinition>();
            asset.DisplayName = name;
            if (heavy)
            {
                asset.Windup = 0.5f; asset.ActiveTime = 0.18f; asset.Recovery = 0.65f;
                asset.Damage = 45f; asset.PoiseDamage = 40f; asset.StaminaCost = 20f;
                asset.Range = 2.5f; asset.Arc = 70f; asset.MovementMultiplier = 0f;
                asset.Color = new Color(1f, 0.4f, 0.12f);
            }
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material Material(string name, string shader, Color color)
        {
            string path = DataPath + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var found = Shader.Find(shader);
            if (found == null) throw new InvalidOperationException("Missing shader: " + shader);
            material = new Material(found) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
