#if !UNITY_EDITOR
using System;
using System.Collections;
using EvolvingRPG.Combat;
using UnityEngine;

namespace EvolvingRPG.Expeditions
{
    // An explicit packaged-player smoke check. The normal game never invokes the route or accesses test saves.
    static class RelayPlayerSmoke
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (!Debug.isDebugBuild || Array.IndexOf(Environment.GetCommandLineArgs(), "-evolvingRelaySmoke") < 0) return;
            var loop = UnityEngine.Object.FindAnyObjectByType<ExpeditionLoop>();
            if (loop == null) { Fail("RelayOutpost is not the startup scene"); return; }
            Application.runInBackground = true;
            loop.StartCoroutine(Exercise(loop));
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
            Debug.Log($"PASS Relay player smoke: hub, board, deployment, {travel:0.00}m dodge, abandon and camera return. No save access.");
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
