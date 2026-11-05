using System;
using System.Collections.Generic;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // Manual menu only. Importing these scripts never opens, creates, or saves a scene.
    public static class EvasionSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/EvasionEvidenceTest.unity";
        [MenuItem("Tools/EvolvingRPG/Create Evasion Evidence Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing EvasionEvidenceTest; no scene overwritten."); return; }
            const string source = "Assets/Game/Scenes/EnemyTest.unity";
            if (!File.Exists(source)) throw new FileNotFoundException("Saved EnemyTest is required; existing scenes will not be rebuilt.", source);
            if (!AssetDatabase.CopyAsset(source, ScenePath)) throw new IOException("Could not copy EnemyTest.");
            var previous = SceneManager.GetActiveScene(); Scene scene = default; bool saved = false;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                CombatActions actor = null; var enemies = new List<SandboxEnemy>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (actor == null) actor = root.GetComponentInChildren<CombatActions>(true);
                    enemies.AddRange(root.GetComponentsInChildren<SandboxEnemy>(true));
                }
                if (actor == null || enemies.Count != 2) throw new InvalidOperationException("Expected one player and the guarded/fast sandbox opponents.");
                if (actor.GetComponent<HeavyPractice>() != null) throw new InvalidOperationException("C1 must not contain the practice reward component.");
                var evidence = actor.gameObject.AddComponent<EvasionEvidence>(); evidence.Configure(enemies.ToArray());
                SandboxEnemy second = enemies.Find(enemy => !enemy.GuardedArchetype);
                var hud = new GameObject("Evasion evidence diagnostics").AddComponent<EvasionEvidenceHUD>();
                hud.Configure(evidence, second);
                if (second != null) second.gameObject.SetActive(false);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                saved = EditorSceneManager.SaveScene(scene, ScenePath);
                if (!saved) throw new IOException("Could not save the new evidence sandbox.");
                Debug.Log("Created EvasionEvidenceTest. Open it explicitly, Play, click Game view. F3 enables the second opponent. Prior scenes preserved.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (!saved) AssetDatabase.DeleteAsset(ScenePath); // Only the new copy created by this invocation.
            }
        }
    }
}
