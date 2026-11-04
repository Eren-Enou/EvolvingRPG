using System;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class TechniqueSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/TechniqueTest.unity";
        public const string DataPath = "Assets/Game/Combat/Data/Techniques";
        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (!File.Exists(ScenePath) && !SessionState.GetBool("EvolvingRPG.TechniqueSceneAttempted", false)) EditorApplication.update += CreateWhenIdle;
        }
        private static void CreateWhenIdle()
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= CreateWhenIdle; SessionState.SetBool("EvolvingRPG.TechniqueSceneAttempted", true); Create();
        }
        [MenuItem("Tools/EvolvingRPG/Create Technique Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before creating TechniqueTest.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing TechniqueTest."); return; }
            var baseline = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Heavy.asset");
            if (baseline == null) throw new InvalidOperationException("Missing baseline Heavy.");
            if (!AssetDatabase.IsValidFolder(DataPath)) AssetDatabase.CreateFolder("Assets/Game/Combat/Data", "Techniques");
            var breaker = Definition("Breaker", baseline, false);
            var momentum = Definition("Momentum", baseline, true);
            if (!AssetDatabase.CopyAsset(EnemySandboxBuilder.ScenePath, ScenePath)) throw new IOException("Could not copy EnemyTest.");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene); CombatActions player = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (player == null) player = root.GetComponentInChildren<CombatActions>();
                    var hud = root.GetComponent<EnemySandboxHUD>();
                    if (hud != null) hud.SetOpponentDetails(false);
                }
                if (player == null) throw new InvalidOperationException("Missing combat actor.");
                player.gameObject.AddComponent<HeavyTechniqueSelector>().Configure(baseline, breaker, momentum);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save TechniqueTest.");
                Debug.Log("Created " + ScenePath + ". Keys 1/2/3 select Heavy experiments while ready; RMB attacks. Existing scenes preserved.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        private static AttackDefinition Definition(string name, AttackDefinition baseline, bool advancing)
        {
            string path = DataPath + "/" + name + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<AttackDefinition>(path);
            if (asset != null) return asset;
            asset = UnityEngine.Object.Instantiate(baseline); asset.name = name; asset.DisplayName = name;
            // Preserve the current Heavy stamina cost for interruption testing.
            if (advancing)
            {
                asset.Windup = 0.4f; asset.ActiveTime = 0.16f; asset.Recovery = 0.45f;
                asset.Damage = 40; asset.PoiseDamage = 25; asset.AdvanceDistance = 1.2f; asset.MissRecovery = 0.55f;
                asset.Color = new Color(0.2f, 0.9f, 0.7f);
            }
            else
            {
                asset.Windup = 0.65f; asset.ActiveTime = 0.18f; asset.Recovery = 0.85f;
                asset.PoiseDamage = 60; asset.AdvanceDistance = 0; asset.MissRecovery = 0;
                asset.Color = new Color(0.8f, 0.35f, 1f);
            }
            asset.MovementMultiplier = 0; AssetDatabase.CreateAsset(asset, path); return asset;
        }
    }
}
