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
            a.RecordExperience(new[] { HeavyExperienceCatalog.Id(DevelopmentIds.Momentum, HeavyExperienceCatalog.Contact), HeavyExperienceCatalog.Id(DevelopmentIds.Momentum, HeavyExperienceCatalog.GuardBreak) });
            Require(store.Save("A", a, out _) && store.Save("A", a, out _), "real temp/replace/backup save failed");
            Require(store.Load("A", out var loaded, out _) && loaded.CharacterId == a.CharacterId && loaded.Balance == 0
                && loaded.RetaliationLearned && loaded.RetaliationActive && loaded.Knowledge == DiscoveryKnowledge.Revealed && loaded.ExperienceCount == 2, "roundtrip changed legal state");
            var b = new CharacterDevelopmentState(); b.Purchase(DevelopmentIds.Breaker, b.Revision, out _);
            Require(store.Save("B", b, out _) && store.Load("B", out var second, out _) && second.CharacterId != a.CharacterId && second.Primary == DevelopmentIds.Breaker
                && !second.RetaliationLearned && second.Exchanges == 0, "two characters leaked state");
            for (int i = 0; i < 3; i++) Require(store.Load("A", out loaded, out _) && loaded.Balance == 0 && !loaded.Purchase(DevelopmentIds.Retaliation, loaded.Revision, out _), "reload duplicated spend/grant");
            Pass("Real JSON roundtrip, File.Replace backup, two independent characters and repeated-load resource integrity");
            var legacy = a.Export(); legacy.schemaVersion = 1; legacy.initialPoints = 0; legacy.heavyExperience = null;
            string legacyRoot = Path.Combine(root, "Legacy"); Directory.CreateDirectory(legacyRoot);
            string legacyPath = Path.Combine(legacyRoot, "character-a.json"), legacyJson = JsonUtility.ToJson(legacy);
            File.WriteAllText(legacyPath, legacyJson); var legacyStore = new CharacterSaveStore(legacyRoot);
            for (int i = 0; i < 3; i++)
                Require(legacyStore.Load("A", out loaded, out string migration) && migration.Contains("Migrated schema 1")
                    && loaded.ExperienceCount == 0 && loaded.CharacterId == a.CharacterId && loaded.Revision == a.Revision
                    && loaded.BuildRevision == a.BuildRevision && loaded.Balance == 0 && loaded.RetaliationActive && loaded.Exchanges == 3
                    && File.ReadAllText(legacyPath) == legacyJson, "legacy migration fabricated history, changed power or wrote without consent");
            Require(legacyStore.Save("A", loaded, out _) && JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(legacyPath)).schemaVersion == 3
                && File.ReadAllText(legacyPath + ".bak") == legacyJson && legacy.schemaVersion == 1 && legacy.heavyExperience == null,
                "explicit migration write lost original backup or mutated source DTO");
            Pass("Schema 1 migrates without invented history or changed identity/build/balance; read is non-writing, explicit save retains legacy backup");
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
            var unknownFact = a.Export(); unknownFact.heavyExperience = new[] { "warrior.heavy.future.experience.v99" };
            File.WriteAllText(path, JsonUtility.ToJson(unknownFact)); original = File.ReadAllText(path);
            Require(!store.Load("A", out _, out _) && !store.Save("A", a, out _) && File.ReadAllText(path) == original, "unknown experience rolled back or overwritten");
            File.WriteAllText(path, legal); File.WriteAllText(path + ".bak", JsonUtility.ToJson(unknownFact));
            Require(store.Load("A", out _, out _) && !store.Save("A", a, out _), "protected unknown experience backup overwritten");
            File.WriteAllText(path + ".bak", legal);
            var invalidHistory = a.Export(); invalidHistory.heavyExperience = new[] { HeavyExperienceCatalog.Id(DevelopmentIds.Momentum, HeavyExperienceCatalog.GuardBreak) };
            Require(!CharacterDevelopmentState.TryRestore(invalidHistory, out _, out _), "advanced history without completed impact accepted");
            invalidHistory = a.Export(); invalidHistory.heavyExperience = new[] { a.ExperienceFacts[0], a.ExperienceFacts[0] };
            Require(!CharacterDevelopmentState.TryRestore(invalidHistory, out _, out _), "duplicate history accepted");
            invalidHistory = a.Export(); invalidHistory.schemaVersion = 1; invalidHistory.initialPoints = 0;
            File.WriteAllText(path, JsonUtility.ToJson(invalidHistory)); original = File.ReadAllText(path);
            Require(!store.Load("A", out _, out _) && !store.Save("A", a, out _) && File.ReadAllText(path) == original, "legacy populated history silently discarded");
            Pass("Unknown experience and legacy populated history protected; duplicate and inconsistent experience rejected");
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
            MissionEconomy(root);
            SlotPreviews(root, a, b);
            LockedRecords(root, a, b);
        }
        static void LockedRecords(string root, CharacterDevelopmentState momentum, CharacterDevelopmentState breaker)
        {
            var store = new CharacterSaveStore(Path.Combine(root, "LockedRecords"));
            Require(store.Save("A", breaker, out _) && store.Save("A", momentum, out _), "could not prepare locked slot");
            string latest = Path.Combine(store.Location, "character-a.json"), backup = latest + ".bak";
            string latestJson = File.ReadAllText(latest), backupJson = File.ReadAllText(backup);

            using (new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var blocked = store.Inspect("A");
                Require(blocked.HasLatest && !blocked.CanLoad && !blocked.CanSave && !blocked.UsesBackup,
                    "temporarily unreadable latest preview fell back or appeared writable");
                Require(!store.Load("A", out _, out _) && !store.Save("A", breaker, out _),
                    "temporarily unreadable latest was rolled back or overwritten");
            }
            using (new FileStream(backup, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var blocked = store.Inspect("A");
                Require(blocked.CanLoad && !blocked.CanSave && blocked.CharacterId == momentum.CharacterId
                    && store.Load("A", out var readable, out _) && readable.CharacterId == momentum.CharacterId
                    && !store.Save("A", breaker, out _), "temporarily unreadable backup was offered for overwrite");
            }
            using (new FileStream(latest, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Require(store.Inspect("A").CanLoad && !store.Save("A", breaker, out _),
                    "replacement of a read-only shared handle did not fail");
                Require(Directory.GetFiles(store.Location, "*.tmp").Length == 0, "failed replacement left a temporary save");
            }
            Require(File.ReadAllText(latest) == latestJson && File.ReadAllText(backup) == backupJson
                && store.Load("A", out var reloaded, out _) && reloaded.CharacterId == momentum.CharacterId,
                "a failed replacement changed the saved checkpoint or backup");
            Require(store.Save("A", breaker, out _) && File.ReadAllText(backup) == latestJson,
                "retry after the lock did not save and preserve the previous latest");
            Pass("Locked latest and backup protect the checkpoint; failed replacement cleans up its temp and a later retry succeeds");
        }
        static void SlotPreviews(string root, CharacterDevelopmentState momentum, CharacterDevelopmentState breaker)
        {
            var store = new CharacterSaveStore(Path.Combine(root, "SlotPreviews"));
            var empty = store.Inspect("A");
            Require(!empty.CanLoad && empty.CanSave && !empty.HasLatest && !Directory.Exists(store.Location),
                "inspecting an empty slot changed disk or offered a load");
            Require(store.Save("A", breaker, out _) && store.Save("A", momentum, out _) && store.Save("B", breaker, out _),
                "could not prepare slot previews");
            string latest = Path.Combine(store.Location, "character-a.json"), backup = latest + ".bak";
            string latestJson = File.ReadAllText(latest), backupJson = File.ReadAllText(backup);
            var a = store.Inspect("A"); var b = store.Inspect("B");
            Require(a.CanSave && a.CanLoad && !a.UsesBackup && a.HasLatest && a.CharacterId == momentum.CharacterId
                && a.Heavy == DevelopmentIds.Momentum && a.RetaliationActive && a.Revision == momentum.Revision
                && b.CanSave && b.CanLoad && b.CharacterId == breaker.CharacterId && b.Heavy == DevelopmentIds.Breaker && !b.RetaliationActive,
                "slot summaries did not describe independent saved Warriors");
            Require(File.ReadAllText(latest) == latestJson && File.ReadAllText(backup) == backupJson,
                "preview wrote a primary or backup");
            File.AppendAllText(backup, "\n");
            var changedBackup = store.Inspect("A");
            Require(changedBackup.CanSave && changedBackup.CanLoad && changedBackup.CharacterId == a.CharacterId
                && changedBackup.Revision == a.Revision && !changedBackup.Equals(a),
                "review identity ignored a changed backup when the visible latest stayed the same");
            File.WriteAllText(backup, backupJson);
            File.WriteAllText(latest, "{broken");
            var recovered = store.Inspect("A");
            Require(recovered.CanLoad && !recovered.CanSave && recovered.UsesBackup && recovered.CharacterId == breaker.CharacterId
                && !recovered.Equals(a) && File.ReadAllText(latest) == "{broken" && File.ReadAllText(backup) == backupJson,
                "damaged latest did not preview read-only recovery of the previous Warrior");
            File.Delete(latest); // This fixture owns both records in an isolated Logs directory.
            var backupOnly = store.Inspect("A");
            Require(backupOnly.CanLoad && backupOnly.CanSave && backupOnly.UsesBackup && !backupOnly.HasLatest,
                "backup-only slot should allow a new latest file without replacing its backup");
            var future = momentum.Export(); future.schemaVersion = 99;
            string futureJson = JsonUtility.ToJson(future);
            File.WriteAllText(latest, futureJson);
            var protectedLatest = store.Inspect("A");
            Require(!protectedLatest.CanLoad && !protectedLatest.CanSave && !protectedLatest.UsesBackup
                && File.ReadAllText(latest) == futureJson, "future latest preview fell back or offered overwrite");
            File.WriteAllText(latest, latestJson); File.WriteAllText(backup, futureJson);
            var protectedBackup = store.Inspect("A");
            Require(protectedBackup.CanLoad && !protectedBackup.CanSave && !protectedBackup.UsesBackup
                && protectedBackup.CharacterId == momentum.CharacterId && File.ReadAllText(backup) == futureJson,
                "protected backup did not prevent overwrite or blocked healthy latest load");
            Pass("Read-only slot previews: empty and independent builds, backup-only and damaged recovery, future latest and protected backup");
        }
        static void MissionEconomy(string root)
        {
            var warrior = new CharacterDevelopmentState(0);
            var store = new CharacterSaveStore(Path.Combine(root,"MissionEconomy"));
            Require(store.Save("A",warrior,out _),"pre-mission zero-point save failed");
            string path=Path.Combine(store.Location,"character-a.json");
            string before=File.ReadAllText(path);
            Require(warrior.GrantRelayMilestone() && warrior.Purchase(warrior.PreviewPurchase(DevelopmentIds.Momentum),out _)
                && store.Save("A",warrior,out _),"mission-funded development save failed");
            string completed=File.ReadAllText(path);
            for(int i=0;i<3;i++)
                Require(store.Load("A",out var loaded,out _) && loaded.CharacterId==warrior.CharacterId
                    && loaded.InitialPoints==0 && loaded.RelayMilestone && loaded.Granted==1 && loaded.Balance==0
                    && loaded.Primary==DevelopmentIds.Momentum && !loaded.GrantRelayMilestone()
                    && File.ReadAllText(path)==completed,"schema-3 completion flag/wallet not atomically restored or read rewrote file");
            Require(File.ReadAllText(path+".bak")==before,"mission save backup lost full prior character");
            File.WriteAllText(path,"{broken");
            Require(store.Load("A",out var recovered,out _) && recovered.CharacterId==warrior.CharacterId
                && !recovered.RelayMilestone && recovered.Balance==0 && recovered.Primary==""
                && !store.Save("A",recovered,out _) && File.ReadAllText(path)=="{broken","backup mixed old wallet with new milestone or overwrote damaged original");
            File.WriteAllText(path,completed);
            Pass("Schema 3: mission flag and spent wallet roundtrip together; repeated loads mint nothing; corrupt latest recovers whole pre-mission character without history merging or automatic repair");

            var legacy=new CharacterDevelopmentState(); legacy.Purchase(DevelopmentIds.Breaker,legacy.Revision,out _);
            var dto=legacy.Export(); dto.schemaVersion=2; dto.initialPoints=0;
            var legacyRoot=Path.Combine(root,"Schema2"); Directory.CreateDirectory(legacyRoot);
            string legacyPath=Path.Combine(legacyRoot,"character-a.json"), json=JsonUtility.ToJson(dto);
            File.WriteAllText(legacyPath,json); var oldStore=new CharacterSaveStore(legacyRoot);
            Require(oldStore.Load("A",out var upgraded,out string message) && message.Contains("Migrated schema 2")
                && upgraded.InitialPoints==1 && !upgraded.RelayMilestone && upgraded.Balance==0 && upgraded.Primary==DevelopmentIds.Breaker
                && File.ReadAllText(legacyPath)==json,"schema-2 migration changed prior economy or wrote during read");
            Require(oldStore.Save("A",upgraded,out _) && File.ReadAllText(legacyPath+".bak")==json
                && JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(legacyPath)).schemaVersion==3,"explicit schema-2 migration did not retain backup");
            dto.relayMilestone=true; File.WriteAllText(legacyPath,JsonUtility.ToJson(dto)); string protectedJson=File.ReadAllText(legacyPath);
            Require(!oldStore.Load("A",out _,out _) && !oldStore.Save("A",upgraded,out _)
                && File.ReadAllText(legacyPath)==protectedJson,"legacy schema silently ignored mission fields");
            Pass("Schema 2: in-memory migration preserves fixture funding/ownership; explicit save upgrades with original backup; unexpected legacy mission fields protect against rollback/overwrite");
        }
        public static void NativeRoundtrip()
        {
            OwnedCharacter owner = null;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) if (owner == null) owner = root.GetComponentInChildren<OwnedCharacter>(true);
            Require(owner != null && owner.CanDevelop && owner.State.RetaliationLearned, "native owner not ready for save");
            owner.UseTestSaveDirectory(Path.GetFullPath(Path.Combine("Logs", "CharacterSaveChecks", "Native-" + Guid.NewGuid().ToString("N"))));
            var actor = owner.GetComponent<CombatActions>(); var discovery = owner.GetComponent<ContextualDiscovery>(); var trial = owner.GetComponent<RetaliatoryMomentumTrial>();
            string originalId = owner.State.CharacterId;
            string originalExperience = string.Join("|", owner.State.ExperienceFacts);
            owner.GetComponent<StarterAssets.ThirdPersonController>().Grounded = true;
            Require(actor.TryAttack(actor.HeavyAttack) && !owner.SaveSlot("A") && !owner.LoadSlot("A"), "busy save/load accepted");
            actor.Interrupt("safe-boundary save fixture complete");
            Require(owner.SaveSlot("A") && owner.SaveSlot("A"), "native save failed");
            owner.ConfigureHeavy(DevelopmentIds.Baseline, false);
            Require(owner.LoadSlot("A") && owner.State.CharacterId == originalId && owner.State.RetaliationActive && owner.State.Balance == 0
                && actor.HeavyAttack == discovery.Momentum && discovery.Ledger.Knowledge == DiscoveryKnowledge.Revealed
                && !trial.OpportunityReady && !actor.CommittedContext.HasValue && !actor.Busy && owner.Review.Count == 0
                && string.Join("|", owner.State.ExperienceFacts) == originalExperience, "native load did not restore build safely");
            Require(owner.FreshCharacter() && owner.Review.Count == 0 && owner.SaveSlot("B"), "fresh second save failed"); string freshId = owner.State.CharacterId;
            Require(owner.LoadSlot("A") && owner.State.CharacterId == originalId && owner.LoadSlot("B") && owner.State.CharacterId == freshId
                && owner.State.Primary.Length == 0 && owner.State.Exchanges == 0 && !owner.State.RetaliationLearned && owner.State.Balance == 1
                && owner.State.ExperienceCount == 0, "native slots inherited state");
            Require(owner.LoadSlot("A"), "restore original for final reset test failed");
            Pass("Native owner: busy boundary rejects; legal build/knowledge/balance restore; fresh second slot remains independent; no action context or opportunity resumes. User save directory untouched.");
        }
        static void Require(bool condition, string message) { if (!condition) { File.AppendAllText(Report, "FAIL " + message + "\n"); throw new InvalidOperationException(message); } }
        static void Pass(string message) => File.AppendAllText(Report, "PASS " + message + "\n");
    }
}
