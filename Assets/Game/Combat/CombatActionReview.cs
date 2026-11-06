using System;
using System.Collections.Generic;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    // Presentation evidence only. No power, discovery, reward or save code reads this journal.
    public sealed class CombatActionReview : IDisposable
    {
        public sealed class Entry
        {
            public readonly int Sequence, Contacts;
            public readonly string Name, LinkedOpponent;
            public readonly bool Interrupted, Evolved, LinkedHit, Guarded, PoiseBroken;
            public readonly float Duration, PlannedAdvance, Displacement, Damage, Poise, MissRecovery, LinkedRecovery;
            public readonly MeleeMissReason MissReason;
            public readonly int ReceivedHits;
            public readonly float DamageTaken, PoiseTaken;
            public readonly bool Staggered, Defeated;
            public readonly CombatPhase? FirstImpactPhase;
            public readonly float BaseRecovery;
            public readonly string InterruptionReason;
            public Entry(int sequence, AttackSnapshot attack, string linkedOpponent, bool evolved, bool linkedHit,
                bool interrupted, float duration, float displacement, int contacts, float damage, float poise,
                bool guarded, bool broken, MeleeMissReason miss, float linkedRecovery,
                int receivedHits, float damageTaken, float poiseTaken, bool staggered, bool defeated, CombatPhase? firstImpactPhase, string interruptionReason)
            {
                Sequence = sequence; Name = attack.DisplayName; LinkedOpponent = linkedOpponent;
                Evolved = evolved; LinkedHit = linkedHit; Interrupted = interrupted; Duration = duration;
                PlannedAdvance = attack.AdvanceDistance; Displacement = displacement; Contacts = contacts;
                Damage = damage; Poise = poise; Guarded = guarded; PoiseBroken = broken; MissReason = miss;
                // Interrupted actions never claim that their remaining recovery was paid.
                MissRecovery = !interrupted && contacts == 0 ? attack.MissRecovery : 0;
                LinkedRecovery = !interrupted ? linkedRecovery : 0;
                ReceivedHits = receivedHits; DamageTaken = damageTaken; PoiseTaken = poiseTaken;
                Staggered = staggered; Defeated = defeated; FirstImpactPhase = firstImpactPhase;
                BaseRecovery = attack.Recovery;
                InterruptionReason = interrupted ? (string.IsNullOrEmpty(interruptionReason) ? "Cause not recorded" : interruptionReason) : "";
            }
            public string Summary => $"#{Sequence} {Name}: {(Interrupted ? "interrupted" : "completed")} in {Duration:0.00}s | {Contacts} contacts, {Damage:0.#} HP / {Poise:0.#} poise applied"
                + (Interrupted ? " | cause: " + InterruptionReason : "")
                + (Guarded ? " | guarded" : "") + (PoiseBroken ? " | poise break" : "")
                + $" | received {ReceivedHits} impacts: {DamageTaken:0.#} HP / {PoiseTaken:0.#} poise during commitment"
                + (FirstImpactPhase.HasValue ? " (first in " + FirstImpactPhase.Value + ")" : "")
                + (Staggered ? " | player staggered" : "") + (Defeated ? " | player defeated" : "")
                + $" | committed base recovery {BaseRecovery:0.000}s"
                + $" | committed advance {PlannedAdvance:0.00}m / net horizontal displacement {Displacement:0.00}m"
                + (!Interrupted && Contacts == 0 ? " | miss: " + MeleeMissFeedback.Describe(MissReason) : "")
                + (Evolved ? " | linked return " + (LinkedHit ? "landed" : "not satisfied") + " against " + LinkedOpponent : "")
                + (!Interrupted ? $" | added recovery: miss {MissRecovery:0.00}s + linked {LinkedRecovery:0.00}s" : " | recovery unfinished");
        }
        readonly CombatActions actions;
        readonly Transform actor;
        readonly List<Entry> history = new List<Entry>(6);
        bool attached, linkedHit, guarded, broken;
        int pending = -1, contacts;
        float started, damage, poise;
        Vector3 start;
        AttackSnapshot snapshot;
        AttackCommitment? context;
        string opponent;
        int receivedHits;
        float damageTaken, poiseTaken;
        bool staggered, defeated;
        CombatPhase? firstImpactPhase;
        public Entry Latest => history.Count == 0 ? null : history[history.Count - 1];
        public int Count => history.Count;
        public IEnumerable<Entry> NewestFirst { get { for (int i = history.Count - 1; i >= 0; i--) yield return history[i]; } }
        public CombatActionReview(CombatActions actions, Transform actor) { this.actions = actions; this.actor = actor; }
        public void Attach() { if (attached) return; actions.Observed += Observe; attached = true; }
        public void Dispose() { if (attached) actions.Observed -= Observe; attached = false; pending = -1; context = null; }
        public void Clear() { history.Clear(); pending = -1; context = null; }
        void Observe(CombatObservation value)
        {
            if (value.Kind == CombatObservationKind.Accepted)
            {
                pending = -1; context = null;
                if (value.ActionKind != CombatActionKind.Attack || !value.Attack.HasValue) return;
                pending = value.ActionSequence; snapshot = value.Attack.Value; context = actions.CommittedContext;
                opponent = context.HasValue ? context.Value.Opportunity.Label : "";
                started = value.Time; start = actor.position; contacts = 0; damage = poise = 0; linkedHit = guarded = broken = false;
                receivedHits = 0; damageTaken = poiseTaken = 0; staggered = defeated = false; firstImpactPhase = null;
                return;
            }
            if (pending != value.ActionSequence) return;
            if (value.Kind == CombatObservationKind.ImpactReceived)
            {
                if (value.Resolution.DamageApplied <= 0 && value.Resolution.PoiseApplied <= 0
                    && !value.Resolution.PoiseBroken && !value.Resolution.Defeated) return;
                receivedHits++; damageTaken += value.Resolution.DamageApplied; poiseTaken += value.Resolution.PoiseApplied;
                staggered |= value.Resolution.PoiseBroken; defeated |= value.Resolution.Defeated;
                if (!firstImpactPhase.HasValue) firstImpactPhase = value.Phase;
                return;
            }
            if (value.Kind == CombatObservationKind.HitResolved)
            {
                contacts++; damage += value.Resolution.DamageApplied; poise += value.Resolution.PoiseApplied;
                guarded |= value.Resolution.Guarded; broken |= value.Resolution.PoiseBroken;
                if (context.HasValue && context.Value.Matches(value.Target, value.Resolution)) linkedHit = true;
                return;
            }
            if (value.Kind != CombatObservationKind.Completed && value.Kind != CombatObservationKind.Interrupted) return;
            bool linkedSatisfied = context.HasValue && linkedHit && context.Value.SameGeneration;
            float penalty = context.HasValue && !linkedSatisfied ? context.Value.TargetMissRecovery : 0;
            var entry = new Entry(pending, snapshot, opponent, context.HasValue, linkedSatisfied,
                value.Kind == CombatObservationKind.Interrupted, Mathf.Max(0, value.Time - started),
                Vector3.ProjectOnPlane(actor.position - start, Vector3.up).magnitude,
                contacts, damage, poise, guarded, broken, value.MissReason, penalty,
                receivedHits, damageTaken, poiseTaken, staggered, defeated, firstImpactPhase, value.Reason);
            if (history.Count == 6) history.RemoveAt(0);
            history.Add(entry); pending = -1; context = null;
        }
    }
}
