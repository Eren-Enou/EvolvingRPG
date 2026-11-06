using System;
using System.Collections.Generic;
using EvolvingRPG.Combat;

namespace EvolvingRPG.Progression
{
    public static class DevelopmentIds
    {
        public const string Baseline = "warrior.heavy.baseline";
        public const string Breaker = "warrior.heavy.breaker";
        public const string Momentum = "warrior.heavy.momentum";
        public const string Retaliation = "warrior.heavy.retaliatory_momentum";
        public static string Label(string id) => id == Baseline ? "Baseline" : id == Breaker ? "Breaker" : id == Momentum ? "Momentum" : id == Retaliation ? "Retaliatory Momentum" : "Fresh / unresolved";
        public static bool IsPrimary(string id) => id == Breaker || id == Momentum;
        public static bool IsKnown(string id) => id == Baseline || IsPrimary(id) || id == Retaliation;
    }

    // Shared combat reads this narrow boundary; debug scenes have no provider.
    public interface ICharacterDevelopment
    {
        bool MomentumOwned { get; }
        bool RetaliationActive { get; }
        int BuildRevision { get; }
    }

    // Character-owned state. Definitions never contain purchases or mutable balances.
    public sealed class CharacterDevelopmentState
    {
        public string CharacterId { get; private set; } = Guid.NewGuid().ToString("N");
        public int Revision { get; private set; }
        public int BuildRevision { get; private set; }
        public string Primary { get; private set; } = "";
        public string Heavy { get; private set; } = DevelopmentIds.Baseline;
        public bool RetaliationLearned { get; private set; }
        public bool RetaliationActive { get; private set; }
        public int Exchanges { get; private set; }
        public DiscoveryKnowledge Knowledge => Exchanges >= 3 ? DiscoveryKnowledge.Revealed : Exchanges > 0 ? DiscoveryKnowledge.Foreshadowed : DiscoveryKnowledge.Undiscovered;
        // Once per character: one initial choice, plus one evolution fixture grant on reveal.
        public int Granted => 1 + (Knowledge == DiscoveryKnowledge.Revealed ? 1 : 0);
        public int Balance => Granted - (Primary.Length > 0 ? 1 : 0) - (RetaliationLearned ? 1 : 0);
        public bool HasUnresolvedContent { get; private set; }
        public string ContentWarning { get; private set; } = "";
        string retainedHeavy;
        readonly HashSet<string> heavyExperience = new HashSet<string>(StringComparer.Ordinal);
        public int ExperienceCount => heavyExperience.Count;
        public bool HasExperience(string profile, string outcome) => heavyExperience.Contains(HeavyExperienceCatalog.Id(profile, outcome));
        public string[] ExperienceFacts
        {
            get { var result = new string[heavyExperience.Count]; heavyExperience.CopyTo(result); Array.Sort(result, StringComparer.Ordinal); return result; }
        }
        public int RecordExperience(IEnumerable<string> facts)
        {
            if (HasUnresolvedContent || facts == null) return 0;
            var combined = new HashSet<string>(heavyExperience, StringComparer.Ordinal);
            foreach (string fact in facts) { if (!HeavyExperienceCatalog.Known(fact)) return 0; combined.Add(fact); }
            var all = new string[combined.Count]; combined.CopyTo(all);
            if (!HeavyExperienceCatalog.Validate(all, Primary, true)) return 0;
            int added = combined.Count - heavyExperience.Count;
            if (added == 0) return 0;
            heavyExperience.UnionWith(combined); Revision++; // History never changes the committed build.
            return added;
        }
        public bool Owns(string id) => !string.IsNullOrEmpty(id) && (id == DevelopmentIds.Baseline || id == Primary || id == DevelopmentIds.Retaliation && RetaliationLearned);
        public DevelopmentPurchasePreview PreviewPurchase(string id) => new DevelopmentPurchasePreview(CharacterId, Revision, id);
        public bool Purchase(DevelopmentPurchasePreview preview, out string message)
        {
            if (!string.Equals(preview.CharacterId, CharacterId, StringComparison.Ordinal))
            { message = "Character changed; inspect this character before confirming an investment."; return false; }
            return Purchase(preview.DevelopmentId, preview.Revision, out message);
        }
        public string Eligibility(string id)
        {
            if (!DevelopmentIds.IsKnown(id)) return "Unknown development.";
            if (Owns(id)) return "Already learned.";
            if (DevelopmentIds.IsPrimary(id) && Primary.Length > 0) return "This experimental character has committed its one Heavy discipline. Create a different character to compare.";
            if (id == DevelopmentIds.Retaliation)
            {
                if (Knowledge != DiscoveryKnowledge.Revealed) return "Not revealed: demonstrate verified evasive Momentum exchanges.";
                if (Primary != DevelopmentIds.Momentum) return "Requires owned Momentum.";
            }
            return Balance < 1 ? "Insufficient experimental points." : "Available: costs 1 experimental point.";
        }
        public bool Purchase(string id, int expectedRevision, out string message)
        {
            message = Eligibility(id);
            if (HasUnresolvedContent) { message = ContentWarning; return false; }
            if (expectedRevision != Revision) { message = "Preview changed; inspect the current character before confirming."; return false; }
            if (!DevelopmentIds.IsKnown(id) || Owns(id) || Balance < 1) return false;
            if (DevelopmentIds.IsPrimary(id))
            {
                if (Primary.Length > 0) return false;
                Primary = Heavy = id; // Explicit purchase preview says it becomes the selected Heavy.
            }
            else if (id == DevelopmentIds.Retaliation && Knowledge == DiscoveryKnowledge.Revealed && Primary == DevelopmentIds.Momentum)
                RetaliationLearned = true; // Learning never implicitly activates the effect.
            else return false;
            Revision++; BuildRevision++; message = "Learned " + DevelopmentIds.Label(id) + "."; return true;
        }
        public bool Configure(string heavy, bool retaliation, out string message)
        {
            if (HasUnresolvedContent) { message = ContentWarning; return false; }
            if ((!DevelopmentIds.IsKnown(heavy) || !Owns(heavy) || heavy == DevelopmentIds.Retaliation) || retaliation && (!RetaliationLearned || heavy != DevelopmentIds.Momentum))
            { message = "Configuration requires learned, compatible developments."; return false; }
            if (heavy == Heavy && retaliation == RetaliationActive) { message = "Configuration unchanged."; return true; }
            Heavy = heavy; RetaliationActive = retaliation; Revision++; BuildRevision++; message = "Configuration updated."; return true;
        }
        public CharacterSaveData Export() => new CharacterSaveData
        {
            schemaVersion = 2, heavyExperience = ExperienceFacts, discoveryId = DevelopmentIds.Retaliation, characterId = CharacterId, revision = Revision, buildRevision = BuildRevision,
            primary = Primary, heavy = retainedHeavy ?? Heavy, primaryPaid = Primary.Length > 0 ? 1 : 0,
            retaliationPaid = RetaliationLearned ? 1 : 0, exchanges = Exchanges,
            grantedPoints = Granted, pointBalance = Balance, retaliationLearned = RetaliationLearned, retaliationActive = RetaliationActive
        };
        public static bool TryRestore(CharacterSaveData data, out CharacterDevelopmentState state, out string message)
        {
            state = null; message = "Invalid or inconsistent character save; original preserved.";
            bool migrated = data != null && data.schemaVersion == 1;
            if (migrated)
            {
                if (data.heavyExperience != null && data.heavyExperience.Length != 0) return false;
                data = data.UpgradeLegacyExperience(); // Copy; never mutate a caller's original record.
            }
            if (data == null || data.schemaVersion != 2 || !Guid.TryParseExact(data.characterId, "N", out _)
                || data.revision < 0 || data.revision > int.MaxValue - 100 || data.buildRevision < 0 || data.buildRevision > data.revision
                || data.exchanges < 0 || data.exchanges > 3 || data.primary == null || data.heavy == null
                || data.primary.Length > 128 || data.heavy.Length > 128 || data.discoveryId != DevelopmentIds.Retaliation) return false;
            bool primaryKnown = data.primary.Length == 0 || DevelopmentIds.IsPrimary(data.primary);
            bool heavyKnown = data.heavy == DevelopmentIds.Baseline || DevelopmentIds.IsPrimary(data.heavy);
            if (!HeavyExperienceCatalog.Validate(data.heavyExperience, data.primary, primaryKnown)) return false;
            if (data.primary == DevelopmentIds.Baseline || data.primary == DevelopmentIds.Retaliation
                || data.primaryPaid != (data.primary.Length > 0 ? 1 : 0) || data.retaliationPaid != (data.retaliationLearned ? 1 : 0)
                || data.grantedPoints != 1 + (data.exchanges == 3 ? 1 : 0)
                || data.pointBalance != data.grantedPoints - data.primaryPaid - data.retaliationPaid || data.pointBalance < 0
                || primaryKnown && data.exchanges > 0 && data.primary != DevelopmentIds.Momentum
                || data.retaliationLearned && (data.exchanges != 3 || primaryKnown && data.primary != DevelopmentIds.Momentum)
                || heavyKnown && data.heavy != DevelopmentIds.Baseline && data.heavy != data.primary
                || data.retaliationActive && (!data.retaliationLearned || primaryKnown && heavyKnown && data.heavy != DevelopmentIds.Momentum)) return false;
            bool unresolved = !primaryKnown || !heavyKnown;
            state = new CharacterDevelopmentState
            {
                CharacterId = data.characterId, Revision = data.revision, BuildRevision = data.buildRevision,
                Primary = data.primary, Heavy = unresolved ? DevelopmentIds.Baseline : data.heavy,
                RetaliationLearned = data.retaliationLearned, RetaliationActive = !unresolved && data.retaliationActive,
                Exchanges = data.exchanges, HasUnresolvedContent = unresolved,
                retainedHeavy = unresolved ? data.heavy : null,
                ContentWarning = unresolved ? "Unknown development retained; baseline selected and evolution inactive. Original save is protected from overwrite." : ""
            };
            state.heavyExperience.UnionWith(data.heavyExperience);
            message = (unresolved ? state.ContentWarning : "Loaded legal character development; transient encounter state was not serialized.")
                + (migrated ? " Migrated schema 1 in memory with empty experience; explicit save writes schema 2. No past experience inferred." : "");
            return true;
        }
        public void RecordKnowledge(int exchanges)
        {
            int next = Math.Min(3, Math.Max(0, exchanges));
            if (next <= Exchanges || Primary != DevelopmentIds.Momentum) return;
            Exchanges = next; Revision++; // History changes cannot invalidate a running action's build snapshot.
        }
    }
}
