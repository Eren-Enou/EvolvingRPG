using System;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    public static class DiscoverySandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/DiscoveryTest.unity";
        [MenuItem("Tools/EvolvingRPG/Create Discovery Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath)) return;
            if (!File.Exists(EvasionSandboxBuilder.ScenePath)) EvasionSandboxBuilder.Create();
            if (!AssetDatabase.CopyAsset(EvasionSandboxBuilder.ScenePath, ScenePath)) throw new IOException("Could not copy evasion scene.");
            var previous = SceneManager.GetActiveScene(); Scene scene = default; bool saved = false;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); SceneManager.SetActiveScene(scene);
                CombatActions actor = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (actor == null) actor = root.GetComponentInChildren<CombatActions>(true);
                    var hud = root.GetComponent<EnemySandboxHUD>(); if (hud != null) hud.SetOpponentDetails(false);
                }
                if (actor == null || actor.GetComponent<HeavyPractice>() != null) throw new InvalidOperationException("Expected actor without practice effect.");
                var baseline = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Heavy.asset");
                var breaker = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Breaker.asset");
                var momentum = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Momentum.asset");
                if (baseline == null || breaker == null || momentum == null) throw new InvalidOperationException("Missing existing technique definitions.");
                actor.gameObject.AddComponent<HeavyTechniqueSelector>().Configure(baseline, breaker, momentum);
                var discovery = actor.gameObject.AddComponent<ContextualDiscovery>(); discovery.Configure(momentum);
                new GameObject("Discovery journal").AddComponent<DiscoveryHUD>().Configure(discovery);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                saved = EditorSceneManager.SaveScene(scene, ScenePath);
                if (!saved) throw new IOException("Could not save DiscoveryTest.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (!saved) AssetDatabase.DeleteAsset(ScenePath);
            }
        }
    }
}
