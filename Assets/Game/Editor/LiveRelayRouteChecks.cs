using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Expeditions;
using EvolvingRPG.Progression;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvolvingRPG.Editor
{
    // A scripted test player supplies movement/action intents; production motor, AI and combat resolve them.
    // No opponent disabling/repositioning, health edits, synthetic damage or combat-parameter changes.
    [InitializeOnLoad]
    public static class LiveRelayRouteChecks
    {
        const string Requested="EvolvingRPG.LiveRoute.Requested", Active="EvolvingRPG.LiveRoute.Active";
        const string Original="EvolvingRPG.LiveRoute.Scene", Report="Logs/LiveRelayRouteChecks.txt";
        static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
        static readonly List<string> errors=new List<string>();
        static ExpeditionLoop loop;
        static OwnedCharacter owner;
        static CombatActions actor;
        static CombatVitality vitality;
        static StarterAssetsInputs inputs;
        static int frame;
        static float resumeAt,oldStep,damageTaken;
        static bool passed,oldBackground;
        static double deadline;
        static LiveRelayRouteChecks() => EditorApplication.playModeStateChanged+=Changed;

        [MenuItem("Tools/EvolvingRPG/Run Live Relay Route Check %#&F3")]
        public static void Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(Active,false)) throw new InvalidOperationException("Stop Play first.");
            if(SceneManager.sceneCount!=1 || SceneManager.GetActiveScene().isDirty || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                throw new InvalidOperationException("Use one saved scene; no editing context is replaced.");
            SessionState.SetString(Original,SceneManager.GetActiveScene().path);
            ExpeditionSceneBuilder.Create(); EditorSceneManager.OpenScene(ExpeditionSceneBuilder.ScenePath);
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report,"Unity "+Application.unityVersion+" / live controller route / "+DateTime.UtcNow.ToString("O")+"\n");
            SessionState.SetBool(Requested,true);
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Requested,false))
            {
                SessionState.SetBool(Requested,false); SessionState.SetBool(Active,true);
                errors.Clear(); steps.Clear(); passed=false; frame=-1; resumeAt=Time.time; damageTaken=0;
                oldStep=Time.captureDeltaTime; oldBackground=Application.runInBackground;
                Time.captureDeltaTime=1f/60; Application.runInBackground=true;
                deadline=EditorApplication.timeSinceStartup+240;
                Application.logMessageReceived+=Capture; steps.Push(Exercise()); EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.ExitingPlayMode && SessionState.GetBool(Active,false))
            {
                EditorApplication.update-=Tick; steps.Clear();
                if(vitality!=null) vitality.HitReceived-=Incoming;
                Time.captureDeltaTime=oldStep; Application.runInBackground=oldBackground;
            }
            if(state==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Active,false))
            {
                SessionState.SetBool(Active,false); Application.logMessageReceived-=Capture;
                bool success=passed && errors.Count==0;
                File.AppendAllText(Report,success ? "PASS Live route and progression roundtrip; no runtime/teardown errors.\n"
                    : "FAIL Live route incomplete/interrupted.\n"+string.Join("\n",errors)+"\n");
                if(success) { EditorSceneManager.OpenScene(SessionState.GetString(Original,ExpeditionSceneBuilder.ScenePath)); Debug.Log("Live relay route passed; scripted intent does not establish human feel."); }
                else Debug.LogError("Live relay route failed; inspect "+Report);
            }
        }
        static void Capture(string text,string stack,LogType type)
        { if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) errors.Add(text+"\n"+stack); }
        static void Incoming(HitData _,HitResolution result) => damageTaken+=result.DamageApplied;
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying) return;
                if(EditorApplication.timeSinceStartup>deadline) throw new TimeoutException("Live route wall-clock deadline.");
                if(errors.Count>0) throw new InvalidOperationException("Console error captured.");
                if(frame==Time.frameCount || Time.time<resumeAt) return; frame=Time.frameCount;
                while(steps.Count>0)
                {
                    var step=steps.Peek();
                    if(!step.MoveNext()) { steps.Pop(); continue; }
                    if(step.Current is IEnumerator nested) { steps.Push(nested); continue; }
                    resumeAt=Time.time+(step.Current is float wait ? wait : 0); return;
                }
                passed=true; EditorApplication.ExitPlaymode();
            }
            catch(Exception e) { File.AppendAllText(Report,"FAIL "+e+"\n"); EditorApplication.ExitPlaymode(); }
        }
        static IEnumerator Exercise()
        {
            yield return .5f;
            loop=UnityEngine.Object.FindAnyObjectByType<ExpeditionLoop>(); Require(loop!=null,"loop missing");
            owner=loop.Character; actor=owner.GetComponent<CombatActions>(); vitality=owner.GetComponent<CombatVitality>();
            inputs=owner.GetComponent<StarterAssetsInputs>(); vitality.HitReceived+=Incoming;
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs","LiveRouteSaves",Guid.NewGuid().ToString("N"))));
            Require(owner.State.Balance==0 && owner.State.Primary=="","fresh Warrior unexpectedly funded/specialized");
            yield return Walk(new Vector3(55,.05f,2.5f),.3f);
            Require(loop.OpenBoard() && loop.Deploy(ExpeditionAttempt.Relay),"walked board/deployment failed");
            string identity=owner.State.CharacterId,attempt=loop.Attempt.Id;
            File.AppendAllText(Report,"DEPLOY "+attempt+" / Warrior "+identity+"\n");
            yield return ClearRelay();
            Require(loop.BoardOpen && owner.State.RelayMilestone && owner.State.Balance==1 && owner.State.CharacterId==identity,"live completion lost identity/reward");
            Require(owner.Purchase(owner.State.PreviewPurchase(DevelopmentIds.Momentum)) && owner.SaveSlot("A"),"earned investment/save failed");
            Require(owner.FreshCharacter() && owner.LoadSlot("A") && owner.State.CharacterId==identity
                && owner.State.Primary==DevelopmentIds.Momentum && owner.State.Balance==0 && owner.State.RelayMilestone,"live-earned character roundtrip failed");
            yield return null;
            File.AppendAllText(Report,"RESTORE attempt="+(loop.Attempt==null ? "none" : loop.Attempt.Phase.ToString())
                +" actorBusy="+actor.Busy+" context="+actor.CommittedContext.HasValue+" identity="+owner.State.CharacterId+"\n");
            Require(loop.Attempt==null && !actor.Busy && !actor.CommittedContext.HasValue,"load retained an attempt or action");
            Require(owner.State.ExperienceCount>0,"live fights produced no retained Heavy experience");
            int baselineFacts=owner.State.ExperienceCount;
            Require(loop.Deploy(ExpeditionAttempt.Relay) && loop.Attempt.Id!=attempt && owner.State.Heavy==DevelopmentIds.Momentum,
                "repeat deployment did not use the restored Warrior and a new attempt");
            Require(owner.TryPreviewHeavy(DevelopmentIds.Momentum,out var momentum) && momentum.AdvanceDistance>0
                && actor.HeavyAttack.DisplayName==momentum.DisplayName,"repeat did not equip the owned advancing Heavy");
            File.AppendAllText(Report,"REPEAT "+loop.Attempt.Id+" / "+momentum.DisplayName+"\n");
            yield return ClearRelay();
            Require(loop.Attempt.Outcome==ExpeditionPhase.Completed && !loop.Attempt.PointAwarded && owner.State.Balance==0
                && owner.State.RelayMilestone && owner.State.CharacterId==identity && owner.State.ExperienceCount>baselineFacts,
                "live repeat duplicated the reward, lost identity, or did not record specialized practice");
            Require(owner.SaveSlot("A") && owner.LoadSlot("A") && owner.State.ExperienceCount>baselineFacts,
                "repeat experience did not survive explicit save and reload");
            Require(loop.Deploy(ExpeditionAttempt.Priority) && loop.Return(ExpeditionPhase.Abandoned),"post-load retry fixture failed");
            Require(owner.LoadSlot("A") && loop.Attempt==null,"same-ID reload retained settled attempt");
            Require(loop.Deploy(ExpeditionAttempt.Priority),"immediate deployment after same-ID load failed");
            yield return null;
            Require(loop.InMission,"next Update discarded a legitimate post-load deployment");
            Require(loop.Return(ExpeditionPhase.Abandoned) && owner.LoadSlot("A") && loop.Attempt==null,"final safe-hub reload failed");
            File.AppendAllText(Report,"PASS Two live relay clears; total damage received "+damageTaken.ToString("0.#")
                +"; Heavy facts "+owner.State.ExperienceCount+"; earned Momentum restored; repeat paid no point. No tuning, health, position or opponent-state overrides.\n");

            yield return ClearPriority(true);
            int momentumFacts=owner.State.ExperienceCount;
            Require(owner.SaveSlot("A") && owner.FreshCharacter(),"could not save Momentum and start the comparison Warrior");
            string breakerId=owner.State.CharacterId;
            Require(breakerId!=identity && owner.State.Balance==0 && owner.State.ExperienceCount==0 && owner.State.Primary=="",
                "second Warrior inherited the first Warrior's identity, points, or history");
            yield return ClearPriority(true);
            Require(owner.State.Granted==0 && loop.Deploy(ExpeditionAttempt.Relay),"Baseline priority earned funding or could not deploy for restoration");
            yield return ClearRelay();
            Require(owner.State.Granted==1 && owner.State.Balance==1 && owner.Purchase(owner.State.PreviewPurchase(DevelopmentIds.Breaker))
                && owner.State.Balance==0 && owner.SaveSlot("B") && owner.LoadSlot("B"),"second Warrior did not earn and reload the equal-cost Breaker choice");
            Require(loop.Deploy(ExpeditionAttempt.Relay),"restored Breaker could not deploy for restoration");
            yield return ClearRelay();
            Require(!loop.Attempt.PointAwarded && owner.State.Granted==1,"Breaker replay duplicated funding");
            yield return ClearPriority(false);
            int breakerFacts=owner.State.ExperienceCount;
            Require(owner.State.HasExperience(DevelopmentIds.Breaker, HeavyExperienceCatalog.Contact) && owner.SaveSlot("B"),"live Breaker practice did not record/save");
            Require(owner.LoadSlot("A") && owner.State.CharacterId==identity && owner.State.Primary==DevelopmentIds.Momentum
                && owner.State.ExperienceCount==momentumFacts && !owner.State.Owns(DevelopmentIds.Breaker)
                && !owner.State.HasExperience(DevelopmentIds.Breaker, HeavyExperienceCatalog.Contact),"slot A inherited Breaker practice or ownership");
            Require(owner.LoadSlot("B") && owner.State.CharacterId==breakerId && owner.State.Primary==DevelopmentIds.Breaker
                && owner.State.ExperienceCount==breakerFacts && !owner.State.Owns(DevelopmentIds.Momentum)
                && !owner.State.HasExperience(DevelopmentIds.Momentum, HeavyExperienceCatalog.Contact),"slot B inherited Momentum practice or ownership");
            File.AppendAllText(Report,"PASS Both contracts cleared with Baseline, earned Momentum and earned Breaker; independent A/B ownership and histories restored.\n");
            yield return DefeatAfterPartialClear();
        }
        static IEnumerator DefeatAfterPartialClear()
        {
            int previousFacts=owner.State.ExperienceCount, previousPoints=owner.State.Balance;
            Require(loop.Deploy(ExpeditionAttempt.Relay),"live failure deployment rejected");
            string failedId=loop.Attempt.Id;
            yield return .25f;
            yield return Walk(new Vector3(-7,.05f,-3),.5f);
            var (guard,fast)=Defenders();
            yield return Fight(guard);
            Require(!loop.Attempt.ObjectivesReady && !fast.GetComponent<EnemyVitality>().Defeated,"partial clear prematurely completed the objective");
            yield return Walk(new Vector3(0,.05f,-3),.6f);
            yield return Walk(new Vector3(7,.05f,-2),.6f);
            Intent(Vector3.zero);
            float stop=Time.time+45;
            while(loop.InMission && Time.time<stop) yield return null;
            Require(!loop.InMission && loop.Attempt.Outcome==ExpeditionPhase.Failed && !loop.Attempt.PointAwarded && loop.BoardOpen
                && loop.Report.StartsWith("Defeated") && vitality.Health==vitality.MaximumHealth,
                "actual enemy strikes did not return the defeated Warrior safely");
            Require(owner.State.ExperienceCount>=previousFacts && owner.State.Balance==previousPoints && !actor.CommittedContext.HasValue,
                "defeat lost retained experience, minted points, or kept a temporary action");
            yield return .25f;
            CheckCamera();
            Require(loop.Deploy(ExpeditionAttempt.Relay) && loop.Attempt.Id!=failedId && !loop.Attempt.ObjectivesReady
                && !guard.GetComponent<EnemyVitality>().Defeated && loop.Return(ExpeditionPhase.Abandoned),
                "retry inherited defeated defenders or could not abandon at arrival");
            File.AppendAllText(Report,"PASS Live defeat after defeating the guard; experience retained, no reward or inherited objective; retry and abandon succeeded.\n");
        }
        static IEnumerator ClearRelay()
        {
            yield return .25f;
            CheckCamera();
            yield return Walk(new Vector3(-7,.05f,-3),.5f);
            var (guard,fast)=Defenders();
            yield return Fight(guard);
            yield return Walk(new Vector3(0,.05f,-3),.6f);
            yield return Walk(new Vector3(7,.05f,-2),.6f);
            yield return Fight(fast);
            yield return CompleteAtRelay();
        }
        static IEnumerator ClearPriority(bool wideRoute)
        {
            Require(loop.Deploy(ExpeditionAttempt.Priority),"live priority deployment rejected");
            File.AppendAllText(Report,$"PRIORITY {actor.HeavyAttack.DisplayName} / {(wideRoute ? "wide route" : "center route")}\n");
            yield return .25f;
            CheckCamera();
            var (guard,fast)=Defenders();
            if(wideRoute)
            {
                yield return Walk(new Vector3(10.5f,.05f,-7),.45f);
                yield return Walk(new Vector3(10.5f,.05f,1),.55f);
                Require(guard.Phase==EnemyPhase.Idle && fast.Phase!=EnemyPhase.Idle,"wide approach did not isolate the fast fighter");
            }
            else
            {
                yield return Walk(new Vector3(0,.05f,-4),.45f);
                Require(guard.Phase!=EnemyPhase.Idle,"center approach did not draw the guard");
                yield return Fight(guard);
                yield return Walk(new Vector3(6,.05f,-2),.55f);
                yield return Walk(new Vector3(7,.05f,0),.55f);
            }
            yield return Fight(fast);
            Require(guard.GetComponent<EnemyVitality>().Defeated!=wideRoute,"priority route used the wrong target order");
            yield return CompleteAtRelay();
            Require(!loop.Attempt.PointAwarded && loop.Report.Contains(wideRoute ? "left the center guard standing" : "cleared the center guard"),
                "priority completion paid a point or reported the wrong route outcome");
        }
        static (SandboxEnemy guard,SandboxEnemy fast) Defenders()
        {
            SandboxEnemy guard=null,fast=null;
            foreach(var enemy in UnityEngine.Object.FindObjectsByType<SandboxEnemy>())
                if(enemy.GuardedArchetype) guard=enemy; else fast=enemy;
            Require(guard!=null && fast!=null,"authored opponents missing");
            return (guard,fast);
        }
        static IEnumerator CompleteAtRelay()
        {
            yield return Walk(new Vector3(5,.05f,10),.5f); // Approach relay east of the central obstruction.
            yield return Walk(new Vector3(1.5f,.05f,10),.4f);
            Require(loop.Attempt.ObjectivesReady && loop.Return(ExpeditionPhase.Completed),"walked relay completion rejected");
            yield return .25f;
            CheckCamera();
        }
        static void CheckCamera()
        {
            Require(Camera.main!=null && Vector3.Distance(Camera.main.transform.position,owner.transform.position)<15f,
                "camera did not follow the Warrior into the new zone");
        }
        static void Intent(Vector3 world)
        {
            Cursor.lockState=CursorLockMode.Locked;
            Vector3 local=Quaternion.Inverse(Quaternion.Euler(0,Camera.main.transform.eulerAngles.y,0))*world.normalized;
            inputs.move=new Vector2(local.x,local.z); inputs.look=Vector2.zero; inputs.jump=inputs.sprint=false;
        }
        static IEnumerator Walk(Vector3 destination,float radius)
        {
            float started=Time.time; Vector3 before=owner.transform.position; float distance=0; bool mission=loop.InMission;
            while(Vector3.ProjectOnPlane(destination-owner.transform.position,Vector3.up).magnitude>radius)
            {
                Require(!vitality.Defeated && loop.InMission==mission,"route changed mission state before waypoint");
                if(Time.time-started>30) throw new TimeoutException("Waypoint blocked: "+destination+" from "+owner.transform.position);
                float step=Vector3.Distance(before,owner.transform.position); Require(step<1,"unsanctioned displacement during walk"); distance+=step; before=owner.transform.position;
                Intent(destination-owner.transform.position); yield return null;
            }
            Intent(Vector3.zero); yield return .1f;
            File.AppendAllText(Report,"WALK "+destination+" / actual travel "+distance.ToString("0.00")+"m\n");
        }
        static IEnumerator Fight(SandboxEnemy enemy)
        {
            float started=Time.time;
            var target=enemy.GetComponent<EnemyVitality>(); int dodged=-1,attacks=0,dodges=0;
            float committedAdvance=0, committedPoise=0, committedRecovery=0;
            var last=owner.Review.Latest;
            int breaks=0, misses=0, interruptions=0;
            float displacement=0;
            void ReadOutcome()
            {
                var entry=owner.Review.Latest;
                if(entry==null || entry==last) return;
                last=entry;
                if(entry.PoiseBroken) breaks++;
                if(entry.Interrupted) interruptions++;
                else if(entry.Contacts==0) misses++;
                displacement=Mathf.Max(displacement,entry.Displacement);
            }
            while(!target.Defeated)
            {
                ReadOutcome();
                Require(loop.InMission && !vitality.Defeated,"scripted player was defeated; no restart/health override");
                Require(enemy.isActiveAndEnabled,"live opponent was disabled");
                if(Time.time-started>120) throw new TimeoutException("Fight stalled: "+enemy.name+" HP "+target.Health+" / player "+vitality.Health);
                if(actor.Busy || actor.MovementBlocked) { Intent(Vector3.zero); yield return null; continue; }
                Vector3 offset=Vector3.ProjectOnPlane(enemy.transform.position-owner.transform.position,Vector3.up);
                bool committed=enemy.CurrentThreat.HasValue && (enemy.Phase==EnemyPhase.Windup || enemy.Phase==EnemyPhase.Active);
                bool outside=false;
                if(committed)
                {
                    var threat=enemy.CurrentThreat.Value;
                    outside=Vector3.Angle(threat.Geometry.Forward,-offset)>enemy.AttackArc*.5f+10;
                    if(!outside && offset.magnitude<3 && threat.Sequence!=dodged && enemy.Phase==EnemyPhase.Windup)
                    {
                        Intent(Vector3.Cross(Vector3.up,threat.Geometry.Forward));
                        if(actor.TryDodge()) { dodged=threat.Sequence; dodges++; }
                        yield return null; continue;
                    }
                    if(!outside) { Intent(Vector3.zero); yield return null; continue; }
                }
                bool opening=outside || enemy.Phase==EnemyPhase.Recovery || target.Staggered;
                if(opening && offset.magnitude<2.6f && Vector3.Angle(owner.transform.forward,offset)<15)
                {
                    Intent(Vector3.zero);
                    if(actor.TryAttack(actor.HeavyAttack))
                    {
                        attacks++;
                        var snapshot=actor.CommittedAttack;
                        committedAdvance=snapshot.AdvanceDistance; committedPoise=snapshot.PoiseDamage; committedRecovery=snapshot.Recovery;
                    }
                }
                else Intent(offset);
                yield return null;
            }
            while(actor.Busy) { Intent(Vector3.zero); yield return null; }
            ReadOutcome();
            Intent(Vector3.zero);
            Require(enemy.AttacksStarted>0 && attacks>0 && dodges>0,"fight did not exercise live threat/action decisions");
            File.AppendAllText(Report,"DEFEATED "+enemy.name+" / native Heavies "+attacks+" / dodges "+dodges
                +" / enemy strikes "+enemy.AttacksStarted+" / player HP "+vitality.Health.ToString("0.#")
                +$" / committed {actor.HeavyAttack.DisplayName}: {committedAdvance:0.##}m advance, {committedPoise:0.#} poise, {committedRecovery:0.##}s base recovery"
                +$" / observed {breaks} poise breaks, {misses} misses, {interruptions} interruptions, {displacement:0.##}m max net attack movement\n");
        }
        static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
    }
}
