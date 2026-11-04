namespace EvolvingRPG.Combat
{
    // Most specific failed geometry gate observed during the active window.
    // This is combat feedback, not an assessment of player skill or progression eligibility.
    public enum MeleeMissReason
    {
        None, NoTargetInReach, TargetUnavailable, OutsideArc, Obstructed, QueryCapacityReached
    }

    public static class MeleeMissFeedback
    {
        public static string Describe(MeleeMissReason reason)
        {
            switch (reason)
            {
                case MeleeMissReason.NoTargetInReach: return "no target in reach";
                case MeleeMissReason.TargetUnavailable: return "target unavailable";
                case MeleeMissReason.OutsideArc: return "target outside attack arc";
                case MeleeMissReason.Obstructed: return "strike obstructed";
                case MeleeMissReason.QueryCapacityReached: return "query capacity reached; result incomplete";
                default: return "no resolved hit";
            }
        }
    }
}
