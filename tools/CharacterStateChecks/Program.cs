using System;
using System.Text.Json;
using EvolvingRPG.Combat;
using EvolvingRPG.Progression;
using EvolvingRPG.Expeditions;

internal static class Program
{
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    static int assertions, failures, cases;
    static int Main()
    {
        Run("Fresh character identity and ownership", Fresh);
        Run("Mission lifecycle, first-time settlement and schema-2 migration", Missions);
        Run("Investment preview identity, revision and replay protection", Previews);
        Run("Deliberate ownership versus knowledge and activation", Choices);
        Run("Experience deduplication and atomic validation", Experience);
        Run("Schema migration preserves original and invents no history", Migration);
        Run("Illegal saved state is rejected", InvalidSaves);
        Run("Unknown content fallback remains frozen", UnknownContent);
        Run("DTO snapshots do not alias live history", SnapshotIsolation);
        Run("Portable JSON roundtrip (System.Text.Json, not Unity codec)", JsonRoundtrip);
        Run("Seeded command traces preserve invariants", Traces);
        Console.WriteLine($"RESULT: {cases - failures}/{cases} cases passed; {assertions} assertions. Production pure C# only; no Unity, stubs, file-store or gameplay tests.");
        return failures == 0 ? 0 : 1;
    }
    static void Run(string name, Action check)
    {
        cases++;
        try { check(); Console.WriteLine("PASS " + name); }
        catch (Exception e) { failures++; Console.Error.WriteLine("FAIL " + name + ": " + e); }
    }
    static void Check(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
    static string Fact(string profile, string outcome) => HeavyExperienceCatalog.Id(profile, outcome);
    static void Missions()
    {
        var a = new CharacterDevelopmentState(0); var b = new CharacterDevelopmentState(0);
        Check(a.Balance == 0 && !a.Purchase(DevelopmentIds.Breaker, a.Revision, out _), "unearned loop point");
        var run = new ExpeditionAttempt(a.CharacterId, ExpeditionAttempt.Relay);
        Check(!run.Finish(run.Id, ExpeditionPhase.Completed) && !run.Settle(run.Id, a), "accepted attempt settled");
        Check(run.Deploy(run.Id) && !run.Defeat("stale", "fast") && !run.Finish(run.Id, ExpeditionPhase.Completed), "stale/uncleared completion");
        Check(run.Defeat(run.Id, "fast") && !run.Defeat(run.Id, "fast") && !run.Finish(run.Id, ExpeditionPhase.Completed), "duplicate/partial defenders");
        Check(run.Defeat(run.Id, "guarded") && run.Finish(run.Id, ExpeditionPhase.Completed), "real objective model cannot finish");
        Check(!run.Settle(run.Id, b) && run.Settle(run.Id, a) && a.Balance == 1 && a.RelayMilestone && run.PointAwarded, "settlement identity/award");
        Check(!run.Settle(run.Id, a) && !run.Finish(run.Id, ExpeditionPhase.Completed) && a.Balance == 1, "duplicate settlement");
        Check(a.Purchase(a.PreviewPurchase(DevelopmentIds.Momentum), out _) && a.Balance == 0, "earned point cannot be invested deliberately");
        Check(CharacterDevelopmentState.TryRestore(a.Export(), out var loaded, out _) && loaded.RelayMilestone && !loaded.GrantRelayMilestone(), "reload reopens milestone");
        foreach (var outcome in new[] { ExpeditionPhase.Completed, ExpeditionPhase.Failed, ExpeditionPhase.Abandoned })
        {
            var next = new ExpeditionAttempt(a.CharacterId, ExpeditionAttempt.Relay);
            Check(next.Id != run.Id && next.Deploy(next.Id), "retry identity");
            next.Defeat(next.Id,"fast"); next.Defeat(next.Id,"guarded");
            Check(next.Finish(next.Id,outcome) && next.Settle(next.Id,a) && !next.PointAwarded && a.Balance == 0, "repeat/failure/abandon minted point");
        }
        var legacy = new CharacterDevelopmentState().Export(); legacy.schemaVersion=2; legacy.initialPoints=0;
        Check(CharacterDevelopmentState.TryRestore(legacy,out var upgraded,out _) && upgraded.InitialPoints==1 && !upgraded.RelayMilestone
            && upgraded.Balance==1 && upgraded.Export().schemaVersion==3 && legacy.schemaVersion==2 && legacy.initialPoints==0, "schema-2 migration changed or inferred progress");
        var corrupt=a.Export(); corrupt.initialPoints=2;
        Check(!CharacterDevelopmentState.TryRestore(corrupt,out _,out _), "invalid initial grant");
        foreach (var outcome in new[] { ExpeditionPhase.Failed, ExpeditionPhase.Abandoned })
        {
            var fresh = new CharacterDevelopmentState(0); var failed = new ExpeditionAttempt(fresh.CharacterId, ExpeditionAttempt.Relay);
            Check(failed.Deploy(failed.Id) && failed.Finish(failed.Id,outcome) && failed.Settle(failed.Id,fresh)
                && !failed.PointAwarded && fresh.Balance==0 && !fresh.RelayMilestone, "unsuccessful first attempt paid milestone");
        }
        var priorityOwner = new CharacterDevelopmentState(0); var priority = new ExpeditionAttempt(priorityOwner.CharacterId, ExpeditionAttempt.Priority);
        Check(priority.Deploy(priority.Id) && !priority.Defeat(priority.Id,"unknown") && priority.Defeat(priority.Id,"fast")
            && priority.ObjectivesReady && priority.Finish(priority.Id,ExpeditionPhase.Completed) && priority.Settle(priority.Id,priorityOwner)
            && !priority.PointAwarded && !priorityOwner.RelayMilestone && priorityOwner.Balance==0, "optional guard or priority reward policy wrong");
        CharacterDevelopmentState breaker = null, momentum = null;
        foreach (var discipline in new[] { DevelopmentIds.Breaker, DevelopmentIds.Momentum })
        {
            var warrior = new CharacterDevelopmentState(0); var mission = new ExpeditionAttempt(warrior.CharacterId,ExpeditionAttempt.Relay);
            var stale = warrior.PreviewPurchase(discipline); int build = warrior.BuildRevision;
            Check(mission.Deploy(mission.Id) && mission.Defeat(mission.Id,"guarded") && mission.Defeat(mission.Id,"fast")
                && mission.Finish(mission.Id,ExpeditionPhase.Completed) && mission.Settle(mission.Id,warrior), "equal-resource completion setup");
            Check(warrior.BuildRevision==build && !warrior.Purchase(stale,out _) && warrior.Primary=="", "reward changed build or accepted stale investment");
            Check(warrior.Purchase(warrior.PreviewPurchase(discipline),out _) && warrior.Granted==1 && warrior.Balance==0
                && warrior.ExperienceCount==0 && warrior.Exchanges==0 && !warrior.RetaliationLearned, "mission fabricated experience or investment economy");
            Check(CharacterDevelopmentState.TryRestore(warrior.Export(),out var restored,out _) && restored.Primary==discipline
                && restored.RelayMilestone && restored.Balance==0, "mission-funded specialization restore");
            if(discipline==DevelopmentIds.Breaker) breaker=warrior; else momentum=warrior;
        }
        Check(breaker.CharacterId!=momentum.CharacterId && breaker.InitialPoints==momentum.InitialPoints
            && breaker.Granted==momentum.Granted && breaker.BuildRevision==momentum.BuildRevision
            && !breaker.Owns(DevelopmentIds.Momentum) && !momentum.Owns(DevelopmentIds.Breaker), "equal-resource identities leaked or received unequal budgets");
    }
    static void Previews()
    {
        var a = new CharacterDevelopmentState(); var b = new CharacterDevelopmentState();
        var preview = a.PreviewPurchase(DevelopmentIds.Momentum);
        Check(preview.CharacterId == a.CharacterId && preview.Revision == 0 && preview.DevelopmentId == DevelopmentIds.Momentum, "preview snapshot");
        Check(!b.Purchase(preview, out _) && b.Revision == 0 && b.Balance == 1 && b.Primary == "", "cross-character preview mutated target at same revision");
        Check(!a.Purchase(default(DevelopmentPurchasePreview), out _) && a.Revision == 0, "default preview accepted");
        Check(!a.Purchase(a.PreviewPurchase(""), out _) && !a.Purchase(a.PreviewPurchase(null), out _)
            && !a.Purchase(a.PreviewPurchase("future.option"), out _) && a.Revision == 0, "unknown preview accepted");
        Check(a.RecordExperience(new[] { Fact(DevelopmentIds.Baseline, HeavyExperienceCatalog.Contact) }) == 1, "history fixture");
        Check(!a.Purchase(preview, out _) && a.Balance == 1 && a.Primary == "", "stale history revision accepted");
        preview = a.PreviewPurchase(DevelopmentIds.Momentum);
        Check(a.Purchase(preview, out _) && a.Balance == 0 && a.Primary == DevelopmentIds.Momentum, "fresh confirmation failed");
        int revision = a.Revision, build = a.BuildRevision;
        Check(!a.Purchase(preview, out _) && a.Revision == revision && a.BuildRevision == build && a.Balance == 0, "replay spent twice");
        a.RecordKnowledge(2); preview = a.PreviewPurchase(DevelopmentIds.Retaliation); a.RecordKnowledge(3);
        Check(!a.Purchase(preview, out _) && !a.RetaliationLearned && a.Balance == 1, "knowledge-stale preview accepted");
        preview = a.PreviewPurchase(DevelopmentIds.Retaliation);
        Check(CharacterDevelopmentState.TryRestore(a.Export(), out var loaded, out _), "restore preview fixture");
        Check(loaded.Purchase(preview, out _) && loaded.RetaliationLearned && !loaded.RetaliationActive,
            "unchanged identity/revision no longer represents same intent");
        Check(!a.RetaliationLearned && a.Balance == 1, "loaded command mutated original");
    }
    static CharacterDevelopmentState Developed()
    {
        var state = new CharacterDevelopmentState();
        Check(state.Purchase(DevelopmentIds.Momentum, state.Revision, out _), "Momentum purchase");
        state.RecordKnowledge(3);
        Check(state.Purchase(DevelopmentIds.Retaliation, state.Revision, out _), "return purchase");
        Check(state.Configure(DevelopmentIds.Momentum, true, out _), "return activation");
        state.RecordExperience(new[] { Fact(DevelopmentIds.Momentum, HeavyExperienceCatalog.Contact) });
        return state;
    }
    static void Fresh()
    {
        var a = new CharacterDevelopmentState(); var b = new CharacterDevelopmentState();
        Check(a.CharacterId != b.CharacterId && Guid.TryParseExact(a.CharacterId, "N", out _), "unique identity");
        Check(a.Balance == 1 && a.Heavy == DevelopmentIds.Baseline && a.ExperienceCount == 0, "fresh foundation");
        Check(a.Owns(DevelopmentIds.Baseline) && !a.Owns(null) && !a.Owns("") && !a.Owns("unknown"), "empty/unknown ID reported as owned");
    }
    static void Choices()
    {
        var a = new CharacterDevelopmentState();
        Check(!a.Purchase(DevelopmentIds.Retaliation, 0, out _), "unrevealed purchase");
        Check(a.Purchase(DevelopmentIds.Momentum, 0, out _) && a.Balance == 0, "primary cost");
        Check(!a.Purchase(DevelopmentIds.Breaker, a.Revision, out _) && !a.Purchase(DevelopmentIds.Momentum, a.Revision, out _), "competing/duplicate choice");
        a.RecordKnowledge(1); int stale = a.Revision; a.RecordKnowledge(3);
        Check(a.Knowledge == DiscoveryKnowledge.Revealed && a.Balance == 1 && !a.RetaliationLearned, "reveal granted power");
        Check(!a.Purchase(DevelopmentIds.Retaliation, stale, out _), "stale preview accepted");
        Check(a.Purchase(DevelopmentIds.Retaliation, a.Revision, out _) && !a.RetaliationActive, "learning auto-activated");
        Check(!a.Configure(DevelopmentIds.Baseline, true, out _) && a.Configure(DevelopmentIds.Momentum, true, out _), "compatibility");
        a.RecordKnowledge(100); Check(a.Balance == 0, "repeat reveal minted points");
    }
    static void Experience()
    {
        var a = new CharacterDevelopmentState(); string contact = Fact(DevelopmentIds.Baseline, HeavyExperienceCatalog.Contact);
        int build = a.BuildRevision;
        Check(a.RecordExperience(new[] { Fact(DevelopmentIds.Baseline, HeavyExperienceCatalog.GuardBreak) }) == 0, "advanced without contact");
        Check(a.RecordExperience(new[] { contact, "unknown" }) == 0 && a.ExperienceCount == 0, "partial mutation on invalid batch");
        Check(a.RecordExperience(new[] { contact }) == 1, "contact missing"); int revision = a.Revision;
        Check(a.RecordExperience(new[] { contact, contact }) == 0 && a.Revision == revision, "duplicate progress");
        Check(a.RecordExperience(new[] { Fact(DevelopmentIds.Breaker, HeavyExperienceCatalog.Contact) }) == 0, "unowned profile history");
        Check(a.BuildRevision == build && a.Balance == 1 && a.Knowledge == DiscoveryKnowledge.Undiscovered, "history granted build, points or knowledge");
    }
    static void Migration()
    {
        var a = Developed(); var legacy = a.Export(); legacy.schemaVersion = 1; legacy.initialPoints = 0; legacy.heavyExperience = null;
        string original = JsonSerializer.Serialize(legacy, Json);
        for (int i = 0; i < 3; i++)
        {
            Check(CharacterDevelopmentState.TryRestore(legacy, out var loaded, out string message) && message.Contains("Migrated schema 1"), "migration failed");
            Check(loaded.CharacterId == a.CharacterId && loaded.Revision == a.Revision && loaded.BuildRevision == a.BuildRevision
                && loaded.Primary == a.Primary && loaded.Heavy == a.Heavy && loaded.RetaliationActive && loaded.Exchanges == a.Exchanges
                && loaded.Balance == a.Balance && loaded.ExperienceCount == 0, "migration changed power or invented history");
            Check(JsonSerializer.Serialize(legacy, Json) == original && loaded.Export().schemaVersion == 3, "source mutated or version not upgraded");
        }
        legacy.heavyExperience = a.ExperienceFacts;
        Check(!CharacterDevelopmentState.TryRestore(legacy, out _, out _), "populated legacy history dropped");
    }
    static void InvalidSaves()
    {
        Action<CharacterSaveData>[] mutations = {
            d => d.schemaVersion = 99, d => d.characterId = "bad", d => d.revision = -1,
            d => d.buildRevision = d.revision + 1, d => d.exchanges = 4, d => d.primary = null,
            d => d.heavy = null, d => d.discoveryId = "unknown", d => d.primaryPaid = 0,
            d => d.pointBalance = 99, d => d.grantedPoints = 99, d => d.retaliationPaid = 0,
            d => d.retaliationLearned = false, d => d.heavy = DevelopmentIds.Breaker,
            d => d.heavyExperience = null, d => d.heavyExperience = new[] { "unknown" },
            d => d.heavyExperience = new[] { d.heavyExperience[0], d.heavyExperience[0] },
            d => d.heavyExperience = new[] { Fact(DevelopmentIds.Momentum, HeavyExperienceCatalog.GuardBreak) }
        };
        var a = Developed();
        foreach (var mutate in mutations)
        { var data = a.Export(); mutate(data); Check(!CharacterDevelopmentState.TryRestore(data, out var invalid, out _) && invalid == null, "illegal save accepted"); }
        Check(!CharacterDevelopmentState.TryRestore(null, out _, out _), "null save accepted");
    }
    static void UnknownContent()
    {
        var data = Developed().Export(); data.primary = data.heavy = "warrior.heavy.missing_momentum";
        Check(CharacterDevelopmentState.TryRestore(data, out var state, out _) && state.HasUnresolvedContent, "missing ownership erased");
        Check(state.Primary == data.primary && state.Heavy == DevelopmentIds.Baseline && !state.RetaliationActive && state.RetaliationLearned, "unsafe fallback");
        int revision = state.Revision;
        Check(!state.Purchase(DevelopmentIds.Breaker, revision, out _) && !state.Configure(DevelopmentIds.Momentum, true, out _)
            && state.RecordExperience(new[] { Fact(DevelopmentIds.Baseline, HeavyExperienceCatalog.Contact) }) == 0 && state.Revision == revision, "unresolved build mutated");
    }
    static void SnapshotIsolation()
    {
        var a = Developed(); var snapshot = a.Export(); snapshot.heavyExperience[0] = "changed";
        Check(a.ExperienceFacts[0] != "changed", "export aliased live state");
        var valid = a.Export(); Check(CharacterDevelopmentState.TryRestore(valid, out var loaded, out _), "restore");
        valid.heavyExperience[0] = "changed"; Check(loaded.ExperienceFacts[0] != "changed", "restore aliased input");
        Check(loaded.RecordExperience(loaded.ExperienceFacts) == 0 && loaded.Balance == a.Balance, "restore reopened credit");
    }
    static void JsonRoundtrip()
    {
        var a = Developed(); string json = JsonSerializer.Serialize(a.Export(), Json);
        Check(CharacterDevelopmentState.TryRestore(JsonSerializer.Deserialize<CharacterSaveData>(json, Json), out var loaded, out _), "portable DTO roundtrip");
        Check(JsonSerializer.Serialize(loaded.Export(), Json) == json, "supported DTO changed on portable roundtrip");
    }
    static void Traces()
    {
        var random = new Random(71051);
        for (int run = 0; run < 30; run++)
        {
            var a = new CharacterDevelopmentState();
            for (int step = 0; step < 100; step++)
            {
                switch (random.Next(7))
                {
                    case 0: a.Purchase(DevelopmentIds.Momentum, a.Revision, out _); break;
                    case 1: a.Purchase(DevelopmentIds.Breaker, a.Revision - random.Next(2), out _); break;
                    case 2: a.RecordKnowledge(random.Next(-1, 6)); break;
                    case 3: a.Purchase(DevelopmentIds.Retaliation, a.Revision, out _); break;
                    case 4: a.Configure(DevelopmentIds.Baseline, random.Next(2) == 0, out _); break;
                    case 5: a.Configure(DevelopmentIds.Momentum, random.Next(2) == 0, out _); break;
                    case 6: a.RecordExperience(new[] { Fact(a.Heavy, HeavyExperienceCatalog.Contact) }); break;
                }
                Check(a.Balance >= 0 && a.Balance <= 2 && a.ExperienceCount <= 6 && a.BuildRevision <= a.Revision, "resource/history/revision invariant");
                Check(!a.RetaliationActive || a.RetaliationLearned && a.Heavy == DevelopmentIds.Momentum, "active without legal ownership");
                Check(CharacterDevelopmentState.TryRestore(a.Export(), out var restored, out _) && restored.CharacterId == a.CharacterId
                    && restored.Balance == a.Balance && restored.Revision == a.Revision, "legal command trace cannot roundtrip");
            }
        }
    }
}
