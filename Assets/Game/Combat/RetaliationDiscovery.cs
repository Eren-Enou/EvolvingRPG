using System.Collections.Generic;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public enum DiscoveryKnowledge { Undiscovered, Foreshadowed, Revealed }

    // One authored session experiment, independent of input, presentation and power ownership.
    public sealed class RetaliationDiscovery
    {
        sealed class Exchange
        {
            public int Dodge, Attack = -1, Version;
            public bool Eligible, DodgeComplete, AttackComplete;
            public float DodgeEnded, CompletedAt;
            public EvasionResult? Evidence;
            public readonly List<(SandboxEnemy source, int revision)> Hits = new List<(SandboxEnemy, int)>();
        }
        Exchange pending;
        int lastDodge = -1;
        readonly HashSet<EnemyThreat> used = new HashSet<EnemyThreat>();
        readonly Queue<string> journal = new Queue<string>();
        public DiscoveryKnowledge Knowledge { get; private set; }
        public int Exchanges { get; private set; }
        public string Diagnostic { get; private set; } = "Awaiting an invested evade-and-Momentum exchange.";
        public IEnumerable<string> Journal => journal.ToArray();
        public float Window { get; }
        public RetaliationDiscovery(float window = 1.5f) { Window = Mathf.Max(0.1f, window); }
        public void BeginDodge(int sequence, bool prerequisites, int investmentVersion)
        {
            if (sequence <= lastDodge) return;
            lastDodge = sequence;
            pending = new Exchange { Dodge = sequence, Eligible = prerequisites, Version = investmentVersion };
            Diagnostic = prerequisites ? "Collecting defensive evidence." : "Missing experimental Momentum / evasive investment.";
        }
        public void CompleteDodge(int sequence, float time)
        {
            if (pending == null || pending.Dodge != sequence || pending.DodgeComplete) return;
            pending.DodgeComplete = true; pending.DodgeEnded = time;
        }
        public void Evidence(EvasionResult receipt)
        {
            if (pending == null || pending.Dodge != receipt.DodgeSequence || pending.Evidence.HasValue) return;
            pending.Evidence = receipt;
            if (receipt.Classification != EvasionClassification.Verified) { Reject("No verified evasion: " + receipt.Reason); return; }
            Evaluate();
        }
        public void BeginAttack(int sequence, bool momentum, float time, int version)
        {
            if (pending == null || pending.Attack >= 0) return;
            if (!pending.Eligible || pending.Version != version) { Reject("Prerequisites absent or changed during exchange."); return; }
            if (!momentum) { Reject("This connection requires Momentum Heavy, not the selected action."); return; }
            if (!pending.DodgeComplete || time < pending.DodgeEnded || time > pending.DodgeEnded + Window)
            { Reject("Momentum was not accepted within 1.5 s of full dodge recovery."); return; }
            pending.Attack = sequence;
            Diagnostic = "Momentum accepted on time; waiting for same-opponent impact and full recovery.";
        }
        public void Hit(int sequence, HitReceiver target, HitResolution resolution)
        {
            if (pending == null || pending.Attack != sequence || !resolution.EnemyContext.HasValue
                || resolution.DamageApplied <= 0 && resolution.PoiseApplied <= 0) return;
            var source = target != null ? target.GetComponent<SandboxEnemy>() : null;
            var context = resolution.EnemyContext.Value;
            if (source != null && context.EngagedWithSource && pending.Hits.Count < 16)
                pending.Hits.Add((source, context.EncounterRevision));
        }
        public void CompleteAttack(int sequence, int version, float completedAt = 0)
        {
            if (pending == null || pending.Attack != sequence) return;
            if (pending.Version != version) { Reject("Investment fixture changed before recovery completed."); return; }
            pending.AttackComplete = true; pending.CompletedAt = completedAt; Evaluate();
        }
        public void Interrupt(int sequence)
        { if (pending != null && (pending.Dodge == sequence || pending.Attack == sequence)) Reject("Linked action interrupted."); }
        public void Invalidate(string reason) { Reject(reason); }
        public void ClearEncounter() { pending = null; Diagnostic = "Encounter reset; session knowledge and fixtures retained."; }
        public void FreshCharacter()
        {
            pending = null; used.Clear(); journal.Clear(); Exchanges = 0; Knowledge = DiscoveryKnowledge.Undiscovered;
            Diagnostic = "Fresh session character; knowledge and fixtures cleared.";
            // Action sequences remain monotonic on the live actor.
        }
        void Evaluate()
        {
            if (pending == null || !pending.AttackComplete || !pending.Evidence.HasValue) return;
            var receipt = pending.Evidence.Value;
            if (!receipt.Threat.HasValue) { Reject("Verified receipt has no source identity."); return; }
            var key = receipt.Threat.Value;
            if (key.Source == null || key.Source.EncounterRevision != key.Revision) { Reject("Opponent generation changed."); return; }
            bool hit = pending.Hits.Exists(value => ReferenceEquals(value.source, key.Source) && value.revision == key.Revision);
            if (!hit) { Reject("Momentum did not apply an impact to the evaded opponent generation."); return; }
            used.RemoveWhere(value => value.Source == null || value.Source.EncounterRevision != value.Revision);
            if (used.Contains(key) || used.Count >= 64) { Reject("Threat already contributed or evidence budget exhausted."); return; }
            used.Add(key); Exchanges++;
            Knowledge = Exchanges >= 3 ? DiscoveryKnowledge.Revealed : DiscoveryKnowledge.Foreshadowed;
            string message = Knowledge == DiscoveryKnowledge.Revealed
                ? "Retaliatory Momentum revealed: invested advancing strikes repeatedly followed verified evasions. Known only; not learned or equipped."
                : "Your evasive movement is beginning to shape your advancing strikes.";
            if (journal.Count == 8) journal.Dequeue();
            journal.Enqueue("At " + pending.CompletedAt.ToString("0.00") + "s: " + message + " Evidence: " + key.Label + ", dodge #" + pending.Dodge + ", Heavy #" + pending.Attack + ".");
            Diagnostic = "Qualified exchange " + Exchanges + ": same opponent, on-time acceptance, applied impact and completed recovery.";
            pending = null;
        }
        void Reject(string reason) { pending = null; Diagnostic = reason; }
    }
}
