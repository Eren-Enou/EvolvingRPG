using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EvolvingRPG.Combat;
using EvolvingRPG.Expeditions;
using EvolvingRPG.Progression;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace EvolvingRPG.Editor
{
    // Explicit native integration check. Never runs on import; all saves are isolated under Logs.
    [InitializeOnLoad]
    public static class ExpeditionChecks
    {
        const string Requested = "EvolvingRPG.ExpeditionChecks.Requested", Active = "EvolvingRPG.ExpeditionChecks.Active";
        const string Report = "Logs/ExpeditionChecks.txt", Original = "EvolvingRPG.ExpeditionChecks.Original";
        static IEnumerator exercise;
        static readonly List<string> errors = new List<string>();
        static int frame;
        static float resumeAt, oldStep;
        static bool passed, oldBackground;
        static double deadline;
        static Keyboard testKeyboard;
        static InputAction testInteraction;
        static ExpeditionChecks() => EditorApplication.playModeStateChanged += Changed;

        [MenuItem("Tools/EvolvingRPG/Run Relay Expedition Checks")]
        public static void Run()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Active,false))
                throw new InvalidOperationException("Use a stopped interactive Editor with no relay check already running.");
            if (SceneManager.sceneCount != 1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                throw new InvalidOperationException("Use one saved scene first; no unsaved or additive scene edits are replaced.");
            SessionState.SetString(Original,SceneManager.GetActiveScene().path);
            ExpeditionSceneBuilder.Create();
            EditorSceneManager.OpenScene(ExpeditionSceneBuilder.ScenePath);
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Report,"Unity "+Application.unityVersion+" / relay native checks / "+DateTime.UtcNow.ToString("O")+"\n");
            SessionState.SetBool(Requested,true);
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Requested,false))
            {
                SessionState.SetBool(Requested,false); SessionState.SetBool(Active,true);
                errors.Clear(); passed=false; frame=-1; resumeAt=Time.time;
                oldStep=Time.captureDeltaTime; oldBackground=Application.runInBackground;
                Time.captureDeltaTime=1f/60; Application.runInBackground=true;
                deadline=EditorApplication.timeSinceStartup+240;
                Application.logMessageReceived+=Capture;
                exercise=Exercise(); EditorApplication.update+=Tick;
            }
            if (state == PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Active,false))
            {
                EditorApplication.update-=Tick; exercise=null;
                testInteraction?.RemoveBindingOverride(0); testInteraction=null;
                if(testKeyboard!=null) InputSystem.RemoveDevice(testKeyboard); testKeyboard=null;
                Time.captureDeltaTime=oldStep; Application.runInBackground=oldBackground;
            }
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Active,false))
            {
                SessionState.SetBool(Active,false); Application.logMessageReceived-=Capture;
                bool success=passed && errors.Count==0;
                File.AppendAllText(Report, success ? "PASS All relay native checks including teardown; no runtime Console errors.\n"
                    : "FAIL Relay checks incomplete, interrupted or errored.\n"+string.Join("\n",errors)+"\n");
                if(success)
                {
                    EditorSceneManager.OpenScene(SessionState.GetString(Original,DevelopmentSandboxBuilder.ScenePath));
                    Debug.Log("Relay native checks passed. This does not establish route difficulty or gameplay feel.");
                }
                else Debug.LogError("Relay checks failed or were interrupted; inspect "+Report);
            }
        }
        static void Capture(string message,string stack,LogType type)
        { if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) errors.Add(message+"\n"+stack); }
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying) return;
                if(EditorApplication.timeSinceStartup>deadline) throw new TimeoutException("Relay integration check timed out.");
                if(errors.Count>0) throw new InvalidOperationException("Runtime Console error captured.");
                if(frame==Time.frameCount || Time.time<resumeAt) return;
                frame=Time.frameCount;
                if(exercise.MoveNext()) { resumeAt=Time.time+(exercise.Current is float delay ? delay : 0f); return; }
                passed=true; EditorApplication.ExitPlaymode();
            }
            catch(Exception e)
            { File.AppendAllText(Report,"FAIL "+e+"\n"); EditorApplication.ExitPlaymode(); }
        }
        static IEnumerator Exercise()
        {
            yield return .5f; // Awake/Start, physics grounding and input ownership must all run natively.
            var loop=Find<ExpeditionLoop>(); var owner=loop.Character;
            var actor=owner.GetComponent<CombatActions>(); var hp=owner.GetComponent<CombatVitality>();
            var hud=Find<EnemySandboxHUD>(); var enemies=new List<SandboxEnemy>();
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var enemy=root.GetComponent<SandboxEnemy>(); if(enemy!=null) enemies.Add(enemy); }
            Require(enemies.Count==2,"expected two authored defenders");
            var refs=new SerializedObject(loop);
            var relay=(Transform)refs.FindProperty("relay").objectReferenceValue;
            var board=(Transform)refs.FindProperty("board").objectReferenceValue;
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs","ExpeditionChecks",Guid.NewGuid().ToString("N"))));
            Require(owner.CanDevelop && owner.State.InitialPoints==0 && owner.State.Balance==0 && !hud.AllowEncounterReset,"fresh hub economy/reset policy");
            foreach(var enemy in enemies) Require(!enemy.gameObject.activeSelf,"hub defender remained active");
            string momentumId=owner.State.CharacterId;
            var stale=owner.State.PreviewPurchase(DevelopmentIds.Momentum);
            testInteraction=(InputAction)typeof(ExpeditionLoop).GetField("interact",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(loop);
            testKeyboard=InputSystem.AddDevice<Keyboard>("RelayCheckKeyboard");
            testInteraction.ApplyBindingOverride(0,testKeyboard.path+"/f");
            Place(owner.gameObject,board.position+Vector3.back*1.5f);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.F)); yield return .05f;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState()); yield return .05f;
            Require(owner.MenuOpen && loop.BoardOpen,"focused native F interaction did not open board/menu");
            Require(!owner.GetComponent<UnityEngine.InputSystem.PlayerInput>().inputIsActive,"board did not suspend player input");
            Require(loop.Deploy(ExpeditionAttempt.Relay) && !owner.MenuOpen && loop.InMission,"deployment did not resume gameplay");
            Require(owner.GetComponent<UnityEngine.InputSystem.PlayerInput>().inputIsActive
                && owner.GetComponent<EvolvingRPG.Movement.PlaygroundMouseLook>().enabled,"deployment failed to restore movement/camera input ownership");
            string firstAttempt=loop.Attempt.Id;
            Require(!loop.Return(ExpeditionPhase.Completed) && !loop.Deploy(ExpeditionAttempt.Relay)
                && !owner.SaveSlot("A") && !owner.LoadSlot("A") && !owner.FreshCharacter(),"mission boundaries allowed reward/redeploy/save/load/fresh");
            Vector3 deployedPosition=owner.transform.position;
            // Remove opponent-phase restrictions so only the mission gate can reject this safe-position fixture.
            foreach(var enemy in enemies) enemy.enabled=false;
            Place(owner.gameObject,new Vector3(55,.05f,0));
            yield return null;
            Require(!owner.CanDevelop && !owner.TryOpenMenu() && !owner.SaveSlot("A") && !owner.LoadSlot("A")
                && !owner.FreshCharacter() && !owner.ConfigureHeavy(DevelopmentIds.Baseline,false),
                "mission lifecycle gate relied only on distance from hub");
            Place(owner.gameObject,deployedPosition);
            foreach(var enemy in enemies) enemy.enabled=true;
            yield return .2f;
            Require(actor.TryDodge(),"native mission dodge rejected: "+actor.Message);
            while(actor.Busy) yield return null;
            Pass("Fresh zero-point hub; menu input suspension; unique deployment; mission mutation boundaries; native dodge completed.");

            // First clear uses actual unmodified Heavy acceptance, collision query and hit resolution.
            // Freeze opponent decisions and place the actor behind each target: controlled integration, not AI/route difficulty validation.
            foreach(var enemy in enemies) enemy.enabled=false;
            foreach(var enemy in enemies)
            {
                Place(enemy.gameObject,new Vector3(0,.05f,-1));
                var target=enemy.GetComponent<EnemyVitality>();
                int swings=0;
                while(!target.Defeated)
                {
                    if(++swings>30) throw new InvalidOperationException("Heavy hit detection failed to clear controlled target.");
                    Place(owner.gameObject,new Vector3(0,.05f,-2.4f));
                    yield return .2f;
                    owner.GetComponent<ThirdPersonController>().Grounded=true;
                    float before=target.Health;
                    Require(actor.TryAttack(actor.HeavyAttack),"native Heavy rejected: "+actor.Message);
                    while(actor.Busy) yield return null;
                    Require(target.Health<before,"native Heavy failed to apply impact to authored target");
                }
                // Move the defeated collider away before testing the other opponent.
                Place(enemy.gameObject,new Vector3(-15,.05f,-1));
            }
            yield return null;
            Require(loop.Attempt.ObjectivesReady,"actual defeated defenders did not satisfy objective");
            int completedFacts=owner.State.ExperienceCount, completedExchanges=owner.State.Exchanges;
            Require(!loop.Return(ExpeditionPhase.Completed),"completion ignored relay proximity");
            Place(owner.gameObject,relay.position+Vector3.back*1.5f);
            yield return null; // Let the motor and the Input System see the new interaction position first.
            Pass($"Relay interaction preflight: focus={Application.isFocused} phase={actor.Phase} blocked={actor.MovementBlocked} "
                +$"distance={Vector3.Distance(owner.transform.position,relay.position):0.00} ready={loop.Attempt.ObjectivesReady} "
                +$"action={testInteraction.enabled}/{testInteraction.controls.Count} keyboard={testKeyboard.enabled}.");
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.F)); yield return .05f;
            bool pressed=testKeyboard.fKey.isPressed;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState()); yield return .05f;
            Require(!loop.InMission && loop.BoardOpen && owner.CanDevelop,
                $"focused native F completion did not return to safe hub report: pressed={pressed} focus={Application.isFocused} "
                +$"phase={actor.Phase} attempt={loop.Attempt?.Phase} board={loop.BoardOpen} safe={owner.CanDevelop} position={owner.transform.position}");
            Require(owner.State.ExperienceCount==completedFacts && owner.State.Exchanges==completedExchanges
                && owner.GetComponent<EvasionEvidence>().Pending==null
                && !owner.GetComponent<RetaliatoryMomentumTrial>().Opportunity.HasValue && !actor.CommittedContext.HasValue,
                "return lost completed knowledge or retained temporary combat opportunity");
            Require(owner.State.RelayMilestone && owner.State.Balance==1 && owner.State.Primary=="" && owner.State.BuildRevision==0
                && !loop.Return(ExpeditionPhase.Completed) && !owner.Purchase(stale),"reward duplicated, auto-invested or accepted stale preview");
            Require(owner.Purchase(owner.State.PreviewPurchase(DevelopmentIds.Momentum)) && owner.State.Balance==0
                && owner.SaveSlot("A") && owner.SaveSlot("A"),"first earned point could not be invested and backed up");
            Pass("Actual native Heavy collision/impact clears both controlled defenders; relay proximity enforced; return/report awards exactly one point; deliberate Momentum investment and isolated backup save.");

            // Remaining cases inject explicit terminal hit fixtures to test mission lifecycle, not attack reach/AI.
            Require(loop.Deploy(ExpeditionAttempt.Relay) && loop.Attempt.Id!=firstAttempt,"retry reused attempt identity");
            foreach(var enemy in enemies) TerminalHit(enemy.GetComponent<CombatVitality>(),hp);
            yield return null;
            Place(owner.gameObject,relay.position+Vector3.back*1.5f);
            Require(loop.Return(ExpeditionPhase.Completed) && !loop.Attempt.PointAwarded && owner.State.Balance==0,"repeat completion paid again");
            Require(loop.Deploy(ExpeditionAttempt.Priority),"priority deployment failed");
            foreach(var enemy in enemies) if(!enemy.GuardedArchetype) TerminalHit(enemy.GetComponent<CombatVitality>(),hp);
            yield return null;
            Require(loop.Attempt.ObjectivesReady,"priority contract incorrectly requires guarded fighter");
            Place(owner.gameObject,relay.position+Vector3.back*1.5f);
            Require(loop.Return(ExpeditionPhase.Completed) && !loop.Attempt.PointAwarded,"priority minted an unapproved point");
            Pass("Controlled terminal-hit fixtures: repeat restoration deduplicates reward; priority contract needs only fast defender and grants no currency.");

            Require(owner.FreshCharacter() && owner.State.Balance==0 && !owner.State.RelayMilestone,"fresh Warrior inherited milestone");
            yield return null;
            string breakerId=owner.State.CharacterId;
            Require(breakerId!=momentumId && loop.Attempt==null && loop.Deploy(ExpeditionAttempt.Relay),"new Warrior inherited attempt");
            TerminalHit(hp,enemies[0].GetComponent<CombatVitality>());
            yield return .1f;
            Require(!loop.InMission && loop.Attempt.Outcome==ExpeditionPhase.Failed && loop.BoardOpen
                && hp.Health==hp.MaximumHealth && owner.State.Balance==0 && !owner.State.RelayMilestone,"defeat did not recover to no-reward hub");
            string failedId=loop.Attempt.Id;
            Require(loop.Deploy(ExpeditionAttempt.Relay) && loop.Attempt.Id!=failedId && loop.Return(ExpeditionPhase.Abandoned)
                && owner.State.Balance==0 && !owner.State.RelayMilestone,"abandon/retry reward or attempt error");
            Require(loop.Deploy(ExpeditionAttempt.Relay),"post-abandon deployment failed");
            foreach(var enemy in enemies) TerminalHit(enemy.GetComponent<CombatVitality>(),hp);
            yield return null;
            Place(owner.gameObject,relay.position+Vector3.back*1.5f);
            Require(loop.Return(ExpeditionPhase.Completed) && owner.State.Balance==1
                && owner.Purchase(owner.State.PreviewPurchase(DevelopmentIds.Breaker)) && owner.SaveSlot("B") && owner.SaveSlot("B"),"second Warrior cannot earn equal-resource Breaker");
            Require(owner.LoadSlot("A") && owner.State.CharacterId==momentumId && owner.State.Primary==DevelopmentIds.Momentum
                && owner.State.RelayMilestone && owner.State.Granted==1 && owner.State.Balance==0,"Momentum restore lost identity/economy");
            yield return null;
            Require(loop.Attempt==null && !actor.Busy && !actor.CommittedContext.HasValue,"load resumed transient mission/action");
            Require(owner.LoadSlot("B") && owner.State.CharacterId==breakerId && owner.State.Primary==DevelopmentIds.Breaker
                && owner.State.RelayMilestone && owner.State.Granted==1 && owner.State.Balance==0,"Breaker restore leaked identity/economy");
            yield return null;
            Require(loop.Attempt==null,"second load inherited attempt");
            Place(owner.gameObject,new Vector3(55,-6,0));
            yield return .1f;
            Require(owner.CanDevelop && owner.State.Balance==0,"hub fall recovery failed or minted reward");
            Require(loop.OpenBoard(),"final board did not open");
            Pass("Controlled lifecycle: defeat, abandon, retry, second first-time completion; equal-resource A/B identities; no attempt/action resume on load; hub fall recovery. Exiting with board open tests teardown.");
        }
        static void TerminalHit(CombatVitality target,CombatVitality source)
        { Require(target.ReceiveHit(new HitData("Controlled lifecycle fixture",100000,0,Color.white,source.transform.position,source),out _),"terminal fixture rejected"); }
        static void Place(GameObject actor,Vector3 position)
        {
            var capsule=actor.GetComponent<CharacterController>(); capsule.enabled=false;
            actor.transform.SetPositionAndRotation(position,Quaternion.identity); capsule.enabled=true; Physics.SyncTransforms();
        }
        static T Find<T>() where T:Component
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { var found=root.GetComponentInChildren<T>(true); if(found!=null) return found; }
            throw new InvalidOperationException("Missing "+typeof(T).Name);
        }
        static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
        static void Pass(string message) => File.AppendAllText(Report,"PASS "+message+"\n");
    }
}
