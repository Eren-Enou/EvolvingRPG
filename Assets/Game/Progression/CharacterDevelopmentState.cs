using System;
using EvolvingRPG.Combat;

namespace EvolvingRPG.Progression
{
    public static class DevelopmentIds
    {
        public const string Baseline = "warrior.heavy.baseline";
        public const string Breaker = "warrior.heavy.breaker";
        public const string Momentum = "warrior.heavy.momentum";
        public const string Retaliation = "warrior.heavy.retaliatory_momentum";
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
        public bool Owns(string id) => id == DevelopmentIds.Baseline || id == Primary || id == DevelopmentIds.Retaliation && RetaliationLearned;
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
            Revision++; BuildRevision++; message = "Learned " + id + "."; return true;
        }
        public bool Configure(string heavy, bool retaliation, out string message)
        {
            if ((!DevelopmentIds.IsKnown(heavy) || !Owns(heavy) || heavy == DevelopmentIds.Retaliation) || retaliation && (!RetaliationLearned || heavy != DevelopmentIds.Momentum))
            { message = "Configuration requires learned, compatible developments."; return false; }
            if (heavy == Heavy && retaliation == RetaliationActive) { message = "Configuration unchanged."; return true; }
            Heavy = heavy; RetaliationActive = retaliation; Revision++; BuildRevision++; message = "Configuration updated."; return true;
        }
        public void RecordKnowledge(int exchanges)
        {
            int next = Math.Min(3, Math.Max(0, exchanges));
            if (next <= Exchanges || Primary != DevelopmentIds.Momentum) return;
            Exchanges = next; Revision++; // History changes cannot invalidate a running action's build snapshot.
        }
    }
}
