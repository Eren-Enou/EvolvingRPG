namespace EvolvingRPG.Combat
{
    // Optional actor-owned resolution before commitment. Execution consumes only the returned snapshot.
    public interface IAttackResolver
    {
        AttackSnapshot Resolve(AttackDefinition definition, AttackSnapshot authored);
    }
}
