using System;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    public static class DevelopmentChecks
    {
        [MenuItem("Tools/EvolvingRPG/Run Character Development Checks")]
        public static void Run() { Models(); CharacterSaveChecks.Run(); DiscoveryChecks.RunDevelopment(); }
        static void Models()
        {
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/DevelopmentChecks.txt", "Character development checks\n");
            var a = new CharacterDevelopmentState(); var b = new CharacterDevelopmentState();
            Require(a.CharacterId != b.CharacterId && a.Balance == 1 && a.Heavy == DevelopmentIds.Baseline, "fresh identity/foundation invalid");
            Require(!a.Purchase(DevelopmentIds.Retaliation, 0, out _) && !a.Purchase("", 0, out _) && !a.Configure("", false, out _), "unknown/unrevealed option accepted");
            Require(a.Purchase(DevelopmentIds.Momentum, 0, out _) && a.Balance == 0 && a.Heavy == DevelopmentIds.Momentum, "choice did not spend/activate");
            Require(!a.Purchase(DevelopmentIds.Momentum, 0, out _) && !a.Purchase(DevelopmentIds.Breaker, a.Revision, out _), "duplicate/competing primary bought");
            a.RecordKnowledge(1); Require(a.Knowledge == DiscoveryKnowledge.Foreshadowed && a.Balance == 0 && !a.RetaliationLearned, "hint granted power");
            int preview = a.Revision; a.RecordKnowledge(3); Require(a.Balance == 1 && !a.Purchase(DevelopmentIds.Retaliation, preview, out _), "reveal grant/stale preview invalid");
            a.RecordKnowledge(100); a.RecordKnowledge(3); Require(a.Balance == 1 && a.Exchanges == 3, "repeated reveal minted resources");
            Require(a.Purchase(DevelopmentIds.Retaliation, a.Revision, out _) && a.Balance == 0 && !a.RetaliationActive, "purchase auto-activated");
            Require(a.Configure(DevelopmentIds.Momentum, true, out _) && a.RetaliationActive, "learned activation failed");
            Require(a.Configure(DevelopmentIds.Baseline, false, out _) && !a.RetaliationActive && a.RetaliationLearned, "baseline selection erased ownership");
            Require(!a.Configure(DevelopmentIds.Baseline, true, out _), "incompatible effect active");
            Require(b.Purchase(DevelopmentIds.Breaker, b.Revision, out _) && b.Balance == 0 && !b.Configure(DevelopmentIds.Momentum, false, out _), "second character inherited power");
            File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS isolated ownership: fresh identity, costs, stale/duplicate commands, one discipline, reveal separate from learning/activation, once-only grant, compatibility and character isolation.\n");
        }
        public static void AfterDiscovery()
        {
            OwnedCharacter owner = null; EnemySandboxHUD hud = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            { if (owner == null) owner = root.GetComponentInChildren<OwnedCharacter>(true); if (hud == null) hud = root.GetComponentInChildren<EnemySandboxHUD>(true); }
            Require(owner != null && owner.State.Knowledge == DiscoveryKnowledge.Revealed && !owner.State.RetaliationLearned && !owner.RetaliationActive, "actual discovery auto-learned/activated");
            hud.ResetEncounter();
            Require(owner.Purchase(DevelopmentIds.Retaliation, owner.State.Revision) && !owner.RetaliationActive && owner.State.Balance == 0, "actual learning cost/separation invalid");
            Require(owner.ConfigureHeavy(DevelopmentIds.Momentum, true) && owner.RetaliationActive, "owned effect activation failed");
            Require(!owner.GetComponent<ContextualDiscovery>().SetInvestments(true, true) && !owner.GetComponent<RetaliatoryMomentumTrial>().SetTrialEnabled(true), "debug fixture bypassed ownership");
            File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS live native discovery reveals only knowledge; separate purchase spends the evolution grant; explicit activation required; debug investment/trial bypass rejected.\n");
            RetaliationChecks.Begin();
        }
        public static void BeforeFresh() => CharacterSaveChecks.NativeRoundtrip();
        public static void Finished()
            => File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS all ownership checks including nine owned native retaliation cases and fresh-character reset.\n");
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
