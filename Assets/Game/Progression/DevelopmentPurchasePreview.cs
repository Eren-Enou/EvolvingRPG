namespace EvolvingRPG.Progression
{
    // Transient intent, never persisted. Revision alone cannot identify a character.
    public readonly struct DevelopmentPurchasePreview
    {
        public string CharacterId { get; }
        public int Revision { get; }
        public string DevelopmentId { get; }
        internal DevelopmentPurchasePreview(string characterId, int revision, string developmentId)
        { CharacterId = characterId; Revision = revision; DevelopmentId = developmentId; }
    }
}
