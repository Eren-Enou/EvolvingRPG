using System;
using System.IO;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace EvolvingRPG.Editor
{
    public static class CharacterSaveChecks
    {
        const string Report = "Logs/CharacterSaveChecks.txt";
        [MenuItem("Tools/EvolvingRPG/Run Character Save Checks")]
        public static void Run()
        {
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report, "Unity " + Application.unityVersion + " / versioned character saves\n");
            string root = Path.GetFullPath(Path.Combine("Logs", "CharacterSaveChecks", Guid.NewGuid().ToString("N")));
            var store = new CharacterSaveStore(root); var a = new CharacterDevelopmentState();
            Require(a.Purchase(DevelopmentIds.Momentum, a.Revision, out _), "setup purchase failed");
            a.RecordKnowledge(3); a.Purchase(DevelopmentIds.Retaliation, a.Revision, out _); a.Configure(DevelopmentIds.Momentum, true, out _);
            Require(store.Save("A", a, out _) && store.Save("A", a, out _), "real temp/replace/backup save failed");
            Require(store.Load("A", out var loaded, out _) && loaded.CharacterId == a.CharacterId && loaded.Balance == 0
                && loaded.RetaliationLearned && loaded.RetaliationActive && loaded.Knowledge == DiscoveryKnowledge.Revealed, "roundtrip changed legal state");
            var b = new CharacterDevelopmentState(); b.Purchase(DevelopmentIds.Breaker, b.Revision, out _);
            Require(store.Save("B", b, out _) && store.Load("B", out var second, out _) && second.CharacterId != a.CharacterId && second.Primary == DevelopmentIds.Breaker
                && !second.RetaliationLearned && second.Exchanges == 0, "two characters leaked state");
            for (int i = 0; i < 3; i++) Require(store.Load("A", out loaded, out _) && loaded.Balance == 0 && !loaded.Purchase(DevelopmentIds.Retaliation, loaded.Revision, out _), "reload duplicated spend/grant");
            Pass("Real JSON roundtrip, File.Replace backup, two independent characters and repeated-load resource integrity");
            string path = Path.Combine(root, "character-a.json"); string legal = File.ReadAllText(path);
            File.WriteAllText(path, "{broken");
            Require(store.Load("A", out loaded, out string recovered) && recovered.Contains("Recovered") && loaded.CharacterId == a.CharacterId, "corrupt latest did not recover backup");
            Require(!store.Save("A", loaded, out _) && File.ReadAllText(path) == "{broken", "save overwrote damaged original without repair");
            File.WriteAllText(path, legal);
            var future = a.Export(); future.schemaVersion = 99; File.WriteAllText(path, JsonUtility.ToJson(future)); string original = File.ReadAllText(path);
            Require(!store.Load("A", out _, out _) && !store.Save("A", a, out _) && File.ReadAllText(path) == original, "future schema downgraded/overwritten");
            Pass("Corrupt latest recovers complete prior state; damaged originals and unsupported future schemas remain untouched");
            var unknownRule = a.Export(); unknownRule.discoveryId = "warrior.heavy.future_return";
            File.WriteAllText(path, JsonUtility.ToJson(unknownRule)); original = File.ReadAllText(path);
            Require(!store.Load("A", out _, out string ruleReason) && ruleReason.Contains("Unknown discovery rule")
                && !store.Save("A", a, out _) && File.ReadAllText(path) == original,
                "unknown discovery silently downgraded to backup or was overwritten");
            File.WriteAllText(path, legal); File.WriteAllText(path + ".bak", JsonUtility.ToJson(unknownRule));
            Require(store.Load("A", out _, out _) && !store.Save("A", a, out _) && File.ReadAllText(path) == legal,
                "unknown discovery backup was overwritten");
            File.WriteAllText(path + ".bak", legal);
            Pass("Unknown discovery IDs protect latest and backup records without silent rollback or overwrite");
            var missing = a.Export(); missing.primary = "warrior.heavy.missing_momentum"; missing.heavy = missing.primary;
            File.WriteAllText(path, JsonUtility.ToJson(missing)); original = File.ReadAllText(path);
            Require(store.Load("A", out var unresolved, out _) && unresolved.HasUnresolvedContent && unresolved.Primary == missing.primary
                && unresolved.Heavy == DevelopmentIds.Baseline && !unresolved.RetaliationActive && unresolved.RetaliationLearned, "unknown definition history/fallback failed");
            Require(!store.Save("A", unresolved, out _) && !store.Save("A", a, out _) && File.ReadAllText(path) == original, "unknown content overwritten");
            var inconsistent = a.Export(); inconsistent.pointBalance = 100;
            Require(!CharacterDevelopmentState.TryRestore(inconsistent, out _, out _), "invalid budget accepted");
            inconsistent = a.Export(); inconsistent.retaliationLearned = false;
            Require(!CharacterDevelopmentState.TryRestore(inconsistent, out _, out _), "active unlearned effect accepted");
            Require(!store.Save("../escape", a, out _) && !store.Load("../escape", out _, out _), "slot traversal accepted");
            var blocked = Path.Combine(root, "not-a-directory"); File.WriteAllText(blocked, "fixture");
            Require(!new CharacterSaveStore(blocked).Save("A", a, out _) && a.Balance == 0 && a.RetaliationActive, "failed I/O changed memory");
            Pass("Missing definition retained/inactive with protected original; inconsistent economy/configuration, slot traversal and failed I/O reject safely");
        }
        public static void NativeRoundtrip()
        {
            OwnedCharacter owner = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) if (owner == null) owner = root.GetComponentInChildren<OwnedCharacter>(true);
            Require(owner != null && owner.CanDevelop && owner.State.RetaliationLearned, "native owner not ready for save");
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs", "CharacterSaveChecks", "Native-" + Guid.NewGuid().ToString("N"))));
            var actor = owner.GetComponent<CombatActions>(); var discovery = owner.GetComponent<ContextualDiscovery>(); var trial = owner.GetComponent<RetaliatoryMomentumTrial>();
            string originalId = owner.State.CharacterId;
            owner.GetComponent<StarterAssets.ThirdPersonController>().Grounded = true;
            Require(actor.TryAttack(actor.HeavyAttack) && !owner.SaveSlot("A") && !owner.LoadSlot("A"), "busy save/load accepted");
            actor.Interrupt("safe-boundary save fixture complete");
            Require(owner.SaveSlot("A") && owner.SaveSlot("A"), "native save failed");
            owner.ConfigureHeavy(DevelopmentIds.Baseline, false);
            Require(owner.LoadSlot("A") && owner.State.CharacterId == originalId && owner.State.RetaliationActive && owner.State.Balance == 0
                && actor.HeavyAttack == discovery.Momentum && discovery.Ledger.Knowledge == DiscoveryKnowledge.Revealed
                && !trial.OpportunityReady && !actor.CommittedContext.HasValue && !actor.Busy && owner.Review.Count == 0, "native load did not restore build safely");
            Require(owner.FreshCharacter() && owner.Review.Count == 0 && owner.SaveSlot("B"), "fresh second save failed"); string freshId = owner.State.CharacterId;
            Require(owner.LoadSlot("A") && owner.State.CharacterId == originalId && owner.LoadSlot("B") && owner.State.CharacterId == freshId
                && owner.State.Primary.Length == 0 && owner.State.Exchanges == 0 && !owner.State.RetaliationLearned && owner.State.Balance == 1, "native slots inherited state");
            Require(owner.LoadSlot("A"), "restore original for final reset test failed");
            Pass("Native owner: busy boundary rejects; legal build/knowledge/balance restore; fresh second slot remains independent; no action context or opportunity resumes. User save directory untouched.");
        }
        static void Require(bool condition, string message) { if (!condition) { File.AppendAllText(Report, "FAIL " + message + "\n"); throw new InvalidOperationException(message); } }
        static void Pass(string message) => File.AppendAllText(Report, "PASS " + message + "\n");
    }
}
