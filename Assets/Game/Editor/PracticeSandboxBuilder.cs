using System;
using System.IO;
using EvolvingRPG.Combat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class PracticeSandboxBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/PracticeTest.unity";
        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (!File.Exists(ScenePath) && !SessionState.GetBool("EvolvingRPG.PracticeSceneAttempted", false)) EditorApplication.update += CreateWhenIdle;
        }
        private static void CreateWhenIdle()
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= CreateWhenIdle; SessionState.SetBool("EvolvingRPG.PracticeSceneAttempted", true); Create();
        }
        [MenuItem("Tools/EvolvingRPG/Create Practice Sandbox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before creating PracticeTest.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing PracticeTest."); return; }
            if (!AssetDatabase.CopyAsset(TechniqueSandboxBuilder.ScenePath, ScenePath)) throw new IOException("Could not copy TechniqueTest.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene); CombatActions actor = null;
                foreach (var root in scene.GetRootGameObjects()) if (actor == null) actor = root.GetComponentInChildren<CombatActions>();
                if (actor == null) throw new InvalidOperationException("No actor in technique sandbox.");
                var definitions = new[]
                {
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Heavy.asset"),
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>(TechniqueSandboxBuilder.DataPath + "/Breaker.asset"),
                    AssetDatabase.LoadAssetAtPath<AttackDefinition>(TechniqueSandboxBuilder.DataPath + "/Momentum.asset")
                };
                foreach (var definition in definitions) if (definition == null) throw new InvalidOperationException("Missing Heavy definition.");
                var practice = actor.gameObject.AddComponent<HeavyPractice>(); practice.Configure(definitions); actor.ConfigureResolver(practice);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                        if (component != null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save PracticeTest.");
                Debug.Log("Created PracticeTest: Heavy practice and optional F2 recovery trial. Existing scenes/assets preserved.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
