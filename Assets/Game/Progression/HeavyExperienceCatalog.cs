using System;
using System.Collections.Generic;

namespace EvolvingRPG.Progression
{
    // Finite qualitative history, not mastery ranks or a resource economy.
    public static class HeavyExperienceCatalog
    {
        public const string Contact = "contact", GuardBreak = "guard_break", Interception = "interception";
        public const int MaximumFacts = 9;
        public static IEnumerable<string> Profiles { get { yield return DevelopmentIds.Baseline; yield return DevelopmentIds.Breaker; yield return DevelopmentIds.Momentum; } }
        public static IEnumerable<string> Outcomes { get { yield return Contact; yield return GuardBreak; yield return Interception; } }
        public static string Id(string profile, string outcome) => profile + ".experience." + outcome + ".v1";
        public static string Profile(string id)
        {
            foreach (var profile in Profiles) foreach (var outcome in Outcomes)
                if (id == Id(profile, outcome)) return profile;
            return null;
        }
        public static bool Known(string id) => Profile(id) != null;
        public static string Label(string outcome) => outcome == Contact ? "completed live impact" : outcome == GuardBreak ? "guard break" : "windup interruption";
        public static bool Validate(string[] facts, string primary, bool primaryKnown)
        {
            if (facts == null || facts.Length > MaximumFacts) return false;
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in facts)
            {
                string profile = Profile(id);
                if (profile == null || !unique.Add(id) || primaryKnown && profile != DevelopmentIds.Baseline && profile != primary) return false;
            }
            foreach (string profile in Profiles)
                if ((unique.Contains(Id(profile, GuardBreak)) || unique.Contains(Id(profile, Interception))) && !unique.Contains(Id(profile, Contact))) return false;
            return true;
        }
    }
}
