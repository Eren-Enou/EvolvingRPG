using System;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Expeditions;
using EvolvingRPG.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    public static class ExpeditionSceneBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/RelayOutpost.unity";
        [MenuItem("Tools/EvolvingRPG/Create Relay Outpost Loop %#&F5")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play first.");
            if (File.Exists(ScenePath)) { Debug.Log("Keeping existing RelayOutpost scene."); return; }
            DevelopmentSandboxBuilder.Create();
            if (!AssetDatabase.CopyAsset(DevelopmentSandboxBuilder.ScenePath, ScenePath)) throw new IOException("Cannot copy development foundation.");
            var previous = SceneManager.GetActiveScene(); Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); SceneManager.SetActiveScene(scene);
                OwnedCharacter owner = null; EnemySandboxHUD hud = null; SandboxEnemy guard = null, fast = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (owner == null) owner = root.GetComponentInChildren<OwnedCharacter>(true);
                    if (hud == null) hud = root.GetComponentInChildren<EnemySandboxHUD>(true);
                    var enemy = root.GetComponent<SandboxEnemy>(); if (enemy != null) { if (enemy.GuardedArchetype) guard = enemy; else fast = enemy; }
                    var evidenceHUD = root.GetComponent<EvasionEvidenceHUD>(); if (evidenceHUD != null) evidenceHUD.enabled = false;
                }
                if (owner == null || hud == null || guard == null || fast == null) throw new InvalidOperationException("Missing loop foundation.");
                owner.transform.SetPositionAndRotation(new Vector3(55,.05f,0), Quaternion.identity);
                var settings = new SerializedObject(owner);
                settings.FindProperty("initialFixturePoints").intValue = 0;
                settings.FindProperty("saveFolder").stringValue = "RelayPrototype";
                settings.FindProperty("developmentRadius").floatValue = 6;
                settings.ApplyModifiedPropertiesWithoutUndo();
                var hub = new GameObject("Relay outpost graybox").transform;
                Box(hub,"Hub floor",new Vector3(55,-.5f,0),new Vector3(20,1,20));
                Box(hub,"Hub rear wall",new Vector3(55,1,-10),new Vector3(20,2,1));
                Box(hub,"Hub left wall",new Vector3(45,1,0),new Vector3(1,2,20));
                Box(hub,"Hub right wall",new Vector3(65,1,0),new Vector3(1,2,20));
                Box(hub,"Hub front wall",new Vector3(55,1,10),new Vector3(20,2,1));
                var board = Station(hub,"MISSION BOARD / F",new Vector3(55,.05f,4),new Color(.9f,.7f,.15f));
                var develop = Station(hub,"DEVELOPMENT / F or Tab",new Vector3(51,.05f,0),Color.green);
                var saves = Station(hub,"SAVE / LOAD / F",new Vector3(59,.05f,0),Color.white);
                var relay = Station(null,"RELAY / F after objective",new Vector3(0,.05f,10),Color.cyan);
                var arrival = Station(null,"ARRIVAL / F to abandon",new Vector3(0,.05f,-7),new Color(.6f,.6f,1));
                owner.gameObject.AddComponent<ExpeditionLoop>().Configure(owner,hud,guard,fast,board,develop,saves,relay,arrival);
                foreach(var root in scene.GetRootGameObjects()) foreach(var component in root.GetComponentsInChildren<Component>(true))
                    if(component!=null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                if (!EditorSceneManager.SaveScene(scene,ScenePath)) throw new IOException("Cannot save loop scene.");
                Debug.Log("Created RelayOutpost: new hub/expedition loop; comparison scenes unchanged.");
            }
            finally
            {
                if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if(scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene,true);
            }
        }
        static void Box(Transform parent,string name,Vector3 position,Vector3 scale)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent);obj.transform.position=position;obj.transform.localScale=scale;
        }
        static Transform Station(Transform parent,string label,Vector3 position,Color color)
        {
            var marker=new GameObject(label);marker.transform.SetParent(parent);marker.transform.position=position;
            var visual=GameObject.CreatePrimitive(PrimitiveType.Cube);visual.transform.SetParent(marker.transform,false);
            // Arrival is also the deployment position. Decorative beacons must never trap the capsule.
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.localPosition=Vector3.up*.35f;visual.transform.localScale=new Vector3(1,.7f,1);
            // Color is authored through a saved material, never an unsaved shared-material mutation.
            string path="Assets/Game/Expeditions/"+label.Split('/')[0].Trim().Replace(" ","_")+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(material,path);}
            visual.GetComponent<Renderer>().sharedMaterial=material;
            var textObject=new GameObject("Station label");textObject.transform.SetParent(marker.transform,false);textObject.transform.localPosition=Vector3.up*2.2f;
            var text=textObject.AddComponent<TextMesh>();text.text=label;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=48;text.characterSize=.055f;text.anchor=TextAnchor.MiddleCenter;
            textObject.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;textObject.AddComponent<ExpeditionSign>();
            return marker.transform;
        }
    }
}
