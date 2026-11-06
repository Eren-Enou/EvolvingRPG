using System;

namespace EvolvingRPG.Progression
{
    // Pure save envelope. Unity's codec and file store remain separate consumers.
    [Serializable]
    public sealed class CharacterSaveData
    {
        public int schemaVersion;
        public string characterId;
        public int revision, buildRevision;
        public string primary, heavy, discoveryId;
        public int primaryPaid, retaliationPaid, exchanges, grantedPoints, pointBalance;
        public bool retaliationLearned, retaliationActive;
        public string[] heavyExperience;
        public int initialPoints;
        public bool relayMilestone;
        internal CharacterSaveData UpgradeMissionEconomy()
        {
            var upgraded = (CharacterSaveData)MemberwiseClone();
            upgraded.schemaVersion = 3; upgraded.initialPoints = 1; upgraded.relayMilestone = false; return upgraded;
        }
        internal CharacterSaveData UpgradeLegacyExperience()
        {
            var upgraded = (CharacterSaveData)MemberwiseClone();
            upgraded.schemaVersion = 2; upgraded.heavyExperience = Array.Empty<string>(); return upgraded;
        }
    }
}
