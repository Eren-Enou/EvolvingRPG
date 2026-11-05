using System;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    public static class DevelopmentSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/DevelopmentTest.unity";
        [MenuItem("Tools/EvolvingRPG/Create Character Development Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath)) return;
            RetaliationSandboxBuilder.Create();
            if (!AssetDatabase.CopyAsset(RetaliationSandboxBuilder.ScenePath, ScenePath)) throw new IOException("Cannot copy retaliation scene.");
            var previous = SceneManager.GetActiveScene(); Scene scene = default; bool saved = false;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); SceneManager.SetActiveScene(scene);
                CombatActions actor = null; EnemySandboxHUD encounter = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (actor == null) actor = root.GetComponentInChildren<CombatActions>(true);
                    if (encounter == null) encounter = root.GetComponentInChildren<EnemySandboxHUD>(true);
                    var hud = root.GetComponent<DiscoveryHUD>(); if (hud != null) UnityEngine.Object.DestroyImmediate(hud);
                }
                if (actor == null || encounter == null || actor.GetComponent<HeavyPractice>() != null) throw new InvalidOperationException("Invalid comparison source.");
                var selector = actor.GetComponent<HeavyTechniqueSelector>(); if (selector != null) UnityEngine.Object.DestroyImmediate(selector);
                var owner = actor.gameObject.AddComponent<OwnedCharacter>();
                owner.Configure(AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Heavy.asset"),
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Breaker.asset"),
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Momentum.asset"), encounter);
                actor.GetComponent<ContextualDiscovery>().ConfigureDevelopment(owner);
                new GameObject("Character development panel").AddComponent<DevelopmentPanel>().Configure(owner);
                foreach (var root in scene.GetRootGameObjects()) foreach (var component in root.GetComponentsInChildren<Component>(true))
                    if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                saved = EditorSceneManager.SaveScene(scene, ScenePath); if (!saved) throw new IOException("Could not save development scene.");
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
