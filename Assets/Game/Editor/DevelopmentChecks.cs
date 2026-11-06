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
            Require(!a.Owns("") && !a.Owns(null), "empty identity reported owned");
            var boundPreview = a.PreviewPurchase(DevelopmentIds.Momentum);
            Require(!b.Purchase(boundPreview, out _) && b.Revision == 0 && b.Balance == 1, "cross-character preview accepted");
            Require(!a.Purchase(default(DevelopmentPurchasePreview), out _), "default preview accepted");
            Require(!a.Purchase(DevelopmentIds.Retaliation, 0, out _) && !a.Purchase("", 0, out _) && !a.Configure("", false, out _), "unknown/unrevealed option accepted");
            Require(a.Purchase(boundPreview, out _) && a.Balance == 0 && a.Heavy == DevelopmentIds.Momentum, "choice did not spend/activate");
            Require(!a.Purchase(boundPreview, out _), "bound preview replay accepted");
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
            Previews(owner, hud);
            hud.ResetEncounter();
            Require(owner.Purchase(DevelopmentIds.Retaliation, owner.State.Revision) && !owner.RetaliationActive && owner.State.Balance == 0, "actual learning cost/separation invalid");
            Require(owner.ConfigureHeavy(DevelopmentIds.Momentum, true) && owner.RetaliationActive, "owned effect activation failed");
            Require(!owner.GetComponent<ContextualDiscovery>().SetInvestments(true, true) && !owner.GetComponent<RetaliatoryMomentumTrial>().SetTrialEnabled(true), "debug fixture bypassed ownership");
            File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS live native discovery reveals only knowledge; separate purchase spends the evolution grant; explicit activation required; debug investment/trial bypass rejected.\n");
            RetaliationChecks.Begin();
        }
        public static void BeforeFresh() => CharacterSaveChecks.NativeRoundtrip();
        static void Previews(OwnedCharacter owner, EnemySandboxHUD hud)
        {
            var actor = owner.GetComponent<CombatActions>(); var stamina = owner.GetComponent<CombatStamina>();
            var trial = owner.GetComponent<RetaliatoryMomentumTrial>();
            var baseline = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Heavy.asset");
            var breaker = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Breaker.asset");
            var momentum = AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Game/Combat/Data/Techniques/Momentum.asset");
            var tuned = UnityEngine.Object.Instantiate(breaker);
            string state = JsonUtility.ToJson(owner.State.Export()); float resource = stamina.Current;
            var selected = actor.HeavyAttack; int consumed = trial.Consumed, observations = 0;
            Action<CombatObservation> observe = value => observations++;
            actor.Observed += observe;
            try
            {
                Require(owner.TryPreviewHeavy(DevelopmentIds.Baseline, out var foundation) && foundation.Damage == baseline.Damage,
                    "baseline preview not authored from assigned definition");
                Require(owner.TryPreviewHeavy(DevelopmentIds.Momentum, out var advance) && advance.AdvanceDistance == momentum.AdvanceDistance
                    && advance.MissRecovery == momentum.MissRecovery, "Momentum preview lost commitment/liability");
                Require(!owner.TryPreviewHeavy(DevelopmentIds.Retaliation, out _) && !owner.TryPreviewHeavy("", out _)
                    && !owner.TryPreviewHeavy(null, out _) && !owner.TryPreviewHeavy("future.heavy", out _), "unknown/effect interpreted as base technique");
                // Runtime clone only: never edit a shared authored asset or save a scene.
                tuned.Windup = .73f; tuned.PoiseDamage = 71; tuned.StaminaCost = 19;
                owner.Configure(baseline, tuned, momentum, hud);
                Require(owner.TryPreviewHeavy(DevelopmentIds.Breaker, out var first) && first.Windup == .73f
                    && first.PoiseDamage == 71 && first.StaminaCost == 19, "preview uses duplicated tuning constants");
                tuned.Windup = .91f;
                Require(first.Windup == .73f && owner.TryPreviewHeavy(DevelopmentIds.Breaker, out var next) && next.Windup == .91f,
                    "preview not a value snapshot of current authored data");
                Require(JsonUtility.ToJson(owner.State.Export()) == state && stamina.Current == resource && actor.HeavyAttack == selected
                    && trial.Consumed == consumed && observations == 0 && !actor.Busy, "inspection changed ownership, resource, action or opportunity consumption");
            }
            finally { actor.Observed -= observe; owner.Configure(baseline, breaker, momentum, hud); UnityEngine.Object.DestroyImmediate(tuned); }
            File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS authored technique previews: assigned data, immutable current snapshots, unknown/effect rejection and no ownership/resource/action/acceptance side effects. Runtime clone only; assets unchanged.\n");
        }
        public static void Finished()
            => File.AppendAllText("Logs/DevelopmentChecks.txt", "PASS all ownership checks including nine owned native retaliation cases and fresh-character reset.\n");
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
