using System;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    public static class RetaliationSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/RetaliationTest.unity";
        [MenuItem("Tools/EvolvingRPG/Create Retaliation Trial Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath)) return;
            DiscoverySandboxBuilder.Create();
            if (!AssetDatabase.CopyAsset(DiscoverySandboxBuilder.ScenePath, ScenePath)) throw new IOException("Could not copy DiscoveryTest.");
            var previous = SceneManager.GetActiveScene(); Scene scene = default; bool saved = false;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); SceneManager.SetActiveScene(scene);
                CombatActions actor = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (actor == null) actor = root.GetComponentInChildren<CombatActions>(true);
                    var oldHud = root.GetComponent<EnemySandboxHUD>(); if (oldHud != null) oldHud.SetInterfaceVisible(false);
                }
                if (actor == null || actor.GetComponent<HeavyPractice>() != null) throw new InvalidOperationException("Expected discovery actor without practice.");
                actor.ConfigureResolver(actor.gameObject.AddComponent<RetaliatoryMomentumTrial>());
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                saved = EditorSceneManager.SaveScene(scene, ScenePath);
                if (!saved) throw new IOException("Could not save trial scene.");
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
