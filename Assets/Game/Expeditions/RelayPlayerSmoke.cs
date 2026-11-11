#if !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEngine;

namespace EvolvingRPG.Expeditions
{
    // An explicit packaged-player smoke check. The normal game never invokes the route or accesses test saves.
    static class RelayPlayerSmoke
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (!Debug.isDebugBuild) return;
            string[] args = Environment.GetCommandLineArgs();
            int restart = Array.IndexOf(args, "-evolvingRelayRestart");
            if (restart < 0 && Array.IndexOf(args, "-evolvingRelaySmoke") < 0) return;
            var loop = UnityEngine.Object.FindAnyObjectByType<ExpeditionLoop>();
            if (loop == null) { Fail("RelayOutpost is not the startup scene"); return; }
            Application.runInBackground = true;
            if (restart < 0) { loop.StartCoroutine(Exercise(loop)); return; }
            if (restart + 2 >= args.Length || !Guid.TryParseExact(args[restart + 2], "N", out var token)
                || (args[restart + 1] != "write" && args[restart + 1] != "reload" && args[restart + 1] != "recover"))
            { Fail("expected -evolvingRelayRestart write|reload|recover <test GUID>"); return; }
            string root = Path.Combine(Path.GetTempPath(), "EvolvingRPG-RelayRestart-" + token.ToString("N"));
            loop.StartCoroutine(RestartExercise(loop, args[restart + 1], root));
        }

        static IEnumerator RestartExercise(ExpeditionLoop loop, string mode, string root)
        {
            yield return new WaitForSecondsRealtime(.6f);
            var owner = loop.Character;
            string latest = Path.Combine(root, "character-a.json");
            if (!owner.CanDevelop || loop.Attempt != null || owner.State.Balance != 0 || owner.State.Heavy != DevelopmentIds.Baseline)
            { Fail("new process did not start as a fresh Warrior in the hub"); yield break; }
            if (mode == "write" ? Directory.Exists(root) || File.Exists(root) : !File.Exists(latest) || !File.Exists(latest + ".bak"))
            { Fail("unexpected isolated restart fixture files"); yield break; }
            owner.UseTestSaveDirectory(root);
            if (mode == "write")
            {
                if (!loop.Deploy(ExpeditionAttempt.Relay) || !PrepareRelayCompletion(loop))
                { Fail("could not prepare first restoration fixture"); yield break; }
                yield return null; yield return null;
                if (!loop.Return(ExpeditionPhase.Completed) || !loop.Attempt.PointAwarded || owner.State.Balance != 1
                    || !owner.Purchase(owner.State.PreviewPurchase(DevelopmentIds.Momentum)) || !owner.SaveSlot("A")
                    || !owner.ConfigureHeavy(DevelopmentIds.Baseline, false) || !owner.SaveSlot("A") || owner.HasUnsavedProgress
                    || !owner.ConfigureHeavy(DevelopmentIds.Momentum, false) || !owner.HasUnsavedProgress)
                { Fail("first restoration, deliberate Momentum purchase or save failed: " + owner.Message); yield break; }
                if (!loop.Deploy(ExpeditionAttempt.Priority) || !loop.InMission || owner.SaveSlot("A") || !owner.HasUnsavedProgress)
                { Fail("interrupted-attempt fixture did not reject mid-mission saving"); yield break; }
                Debug.Log("PASS Relay restart write: " + owner.State.CharacterId + " saved; exiting during an active Priority attempt.");
            }
            else
            {
                var slot = owner.InspectSaveSlot("A");
                string expectedHeavy = mode == "recover" ? DevelopmentIds.Momentum : DevelopmentIds.Baseline;
                if (!slot.CanLoad || slot.UsesBackup != (mode == "recover") || !owner.LoadSlot("A")
                    || owner.State.CharacterId != slot.CharacterId || !owner.State.RelayMilestone
                    || owner.State.Heavy != expectedHeavy || !owner.MomentumOwned || owner.State.Balance != 0 || owner.State.Granted != 1
                    || owner.HasUnsavedProgress || !owner.SaveStatus.Contains("slot A")
                    || loop.Attempt != null || owner.GetComponent<RetaliatoryMomentumTrial>().Opportunity.HasValue
                    || owner.GetComponent<CombatActions>().CommittedContext.HasValue)
                { Fail("reload did not restore the saved milestone and owned build without a mission or commitment"); yield break; }
                if (mode == "recover")
                {
                    if (slot.CanSave || owner.SaveSlot("A"))
                    { Fail("backup recovery allowed an overwrite of the damaged latest file"); yield break; }
                    if (!owner.SaveSlot("B") || owner.InspectSaveSlot("B").CharacterId != owner.State.CharacterId
                        || owner.HasUnsavedProgress || !owner.SaveStatus.Contains("slot B"))
                    { Fail("backup recovery could not save the restored Warrior to another slot"); yield break; }
                    Debug.Log("PASS Relay restart recover: " + owner.State.CharacterId + " restored from backup and saved to B; damaged latest protected.");
                }
                else
                {
                    if (!owner.ConfigureHeavy(DevelopmentIds.Momentum, false)
                        || !loop.Deploy(ExpeditionAttempt.Relay) || !PrepareRelayCompletion(loop))
                    { Fail("could not prepare repeat restoration fixture"); yield break; }
                    yield return null; yield return null;
                    if (!loop.Return(ExpeditionPhase.Completed) || loop.Attempt.PointAwarded || owner.State.Granted != 1 || owner.State.Balance != 0)
                    { Fail("repeat restoration paid the milestone again after restart"); yield break; }
                    Debug.Log("PASS Relay restart reload: " + owner.State.CharacterId + " latest restored; unsaved equip and interrupted attempt absent; repeat paid no point.");
                }
            }
            Application.Quit(0);
        }

        // Only persistence is under test here. Real combat and traversal are covered by the native live route.
        static bool PrepareRelayCompletion(ExpeditionLoop loop)
        {
            var owner = loop.Character;
            var relay = GameObject.Find("RELAY / F after objective");
            var defenders = UnityEngine.Object.FindObjectsByType<SandboxEnemy>();
            if (relay == null || defenders.Length != 2) return false;
            foreach (var defender in defenders)
                if (!defender.GetComponent<CombatVitality>().ReceiveHit(
                    new HitData("Restart lifecycle fixture", 100000, 0, Color.white, owner.transform.position, owner.GetComponent<CombatVitality>()), out _)) return false;
            var controller = owner.GetComponent<CharacterController>();
            controller.enabled = false;
            owner.transform.position = relay.transform.position + Vector3.back;
            controller.enabled = true;
            Physics.SyncTransforms();
            return true;
        }

        static IEnumerator Exercise(ExpeditionLoop loop)
        {
            yield return new WaitForSecondsRealtime(.6f);
            var owner = loop.Character;
            if (!owner.CanDevelop || owner.State.Balance != 0 || !CameraFollows(owner.transform))
            { Fail("fresh hub, camera or development state invalid"); yield break; }
            if (!loop.OpenBoard() || !loop.Deploy(ExpeditionAttempt.Relay))
            { Fail("board or deployment rejected"); yield break; }
            yield return new WaitForSecondsRealtime(.5f);
            var actor = owner.GetComponent<CombatActions>();
            Vector3 before = owner.transform.position;
            if (!loop.InMission || !CameraFollows(owner.transform) || !actor.TryDodge())
            { Fail("arena camera or dodge rejected: " + actor.Message); yield break; }
            float deadline = Time.realtimeSinceStartup + 5;
            while (actor.Busy && Time.realtimeSinceStartup < deadline) yield return null;
            float travel = Vector3.ProjectOnPlane(owner.transform.position - before, Vector3.up).magnitude;
            if (actor.Busy || travel < 1 || travel > 4)
            { Fail($"arena dodge did not complete through the motor: {travel:0.00}m"); yield break; }
            // Exercise the production return transition; the native suite separately checks focused station input.
            if (!loop.Return(ExpeditionPhase.Abandoned) || !loop.BoardOpen || owner.State.Balance != 0)
            { Fail("abandon did not return to a no-reward board"); yield break; }
            yield return new WaitForSecondsRealtime(.4f);
            if (!owner.CanDevelop || !CameraFollows(owner.transform))
            { Fail("return did not restore safe hub and camera"); yield break; }
            if (!loop.Deploy(ExpeditionAttempt.Priority))
            { Fail("Priority deployment rejected"); yield break; }
            yield return new WaitForSecondsRealtime(1f);
            var defenders = UnityEngine.Object.FindObjectsByType<SandboxEnemy>();
            if (!loop.InMission || defenders.Length != 2 || Array.Exists(defenders, defender => defender.Phase != EnemyPhase.Idle))
            {
                string states = string.Join("; ", Array.ConvertAll(defenders, defender => $"{defender.name}: {defender.Phase} at {defender.transform.position}"));
                Fail($"Priority arrival was not calm: mission={loop.InMission}, Warrior={owner.transform.position}, defenders={defenders.Length}: {states}");
                yield break;
            }
            if (!loop.Return(ExpeditionPhase.Abandoned) || !loop.BoardOpen || owner.State.Balance != 0)
            { Fail("Priority abandon did not restore the no-reward board"); yield break; }
            Debug.Log($"PASS Relay player smoke: hub, board, deployment, {travel:0.00}m dodge, abandon and camera return; Priority arrival stayed calm. No save access.");
            Application.Quit(0);
        }

        static bool CameraFollows(Transform actor) => Camera.main != null && Vector3.Distance(Camera.main.transform.position, actor.position) < 15;
        static void Fail(string reason)
        {
            Debug.LogError("FAIL Relay player smoke: " + reason);
            Application.Quit(1);
        }
    }
}
#endif
