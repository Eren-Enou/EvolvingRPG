using System;
using System.Collections.Generic;
using EvolvingRPG.Progression;

namespace EvolvingRPG.Expeditions
{
    public enum ExpeditionPhase { Accepted, InProgress, Completed, Failed, Abandoned, RewardSettled }
    // One deliberate deployment; callbacks must carry this ID. No Unity references or reward counters.
    public sealed class ExpeditionAttempt
    {
        public const string Relay = "relay.restore.v1", Priority = "relay.priority.v1";
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public string CharacterId { get; }
        public string Contract { get; }
        public ExpeditionPhase Phase { get; private set; } = ExpeditionPhase.Accepted;
        public ExpeditionPhase Outcome { get; private set; } = ExpeditionPhase.Accepted;
        public bool PointAwarded { get; private set; }
        readonly HashSet<string> defeated = new HashSet<string>(StringComparer.Ordinal);
        public bool GuardDefeated => defeated.Contains("guarded");
        public bool FastDefeated => defeated.Contains("fast");
        public bool ObjectivesReady => FastDefeated && (Contract == Priority || GuardDefeated);
        public ExpeditionAttempt(string characterId, string contract)
        {
            if (!Guid.TryParseExact(characterId, "N", out _) || contract != Relay && contract != Priority)
                throw new ArgumentException("Unknown contract or character identity.");
            CharacterId = characterId; Contract = contract;
        }
        public bool Deploy(string id) { if (id != Id || Phase != ExpeditionPhase.Accepted) return false; Phase = ExpeditionPhase.InProgress; return true; }
        public bool Defeat(string id, string role)
        {
            return id == Id && Phase == ExpeditionPhase.InProgress && (role == "guarded" || role == "fast") && defeated.Add(role);
        }
        public bool Finish(string id, ExpeditionPhase outcome)
        {
            if (id != Id || Phase != ExpeditionPhase.InProgress || outcome != ExpeditionPhase.Completed && outcome != ExpeditionPhase.Failed && outcome != ExpeditionPhase.Abandoned) return false;
            if (outcome == ExpeditionPhase.Completed && !ObjectivesReady) return false;
            Phase = Outcome = outcome; return true;
        }
        public bool Settle(string id, CharacterDevelopmentState character)
        {
            if (id != Id || character == null || character.CharacterId != CharacterId || character.HasUnresolvedContent
                || Phase != ExpeditionPhase.Completed && Phase != ExpeditionPhase.Failed && Phase != ExpeditionPhase.Abandoned) return false;
            PointAwarded = Outcome == ExpeditionPhase.Completed && Contract == Relay && character.GrantRelayMilestone();
            Phase = ExpeditionPhase.RewardSettled; return true;
        }
    }
}
