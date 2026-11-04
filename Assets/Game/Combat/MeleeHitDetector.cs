using System;
using System.Collections.Generic;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    public sealed class MeleeHitDetector : MonoBehaviour
    {
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private LayerMask obstructionLayers = 1;
        [SerializeField, Min(0f)] private float hitHeight = 1f;
        private readonly Collider[] overlaps = new Collider[64];

        // Geometry queries only. Each receiver owns health/poise/guard resolution.
        public int ResolveSwing(AttackDefinition attack, Quaternion facing, HashSet<HitReceiver> alreadyHit,
            Action<HitReceiver, HitResolution> resolved = null)
            => ResolveSwing(new AttackSnapshot(attack), facing, alreadyHit, resolved);

        public int ResolveSwing(AttackSnapshot attack, Quaternion facing, HashSet<HitReceiver> alreadyHit,
            Action<HitReceiver, HitResolution> resolved = null)
            => ResolveSwing(attack, facing, alreadyHit, out _, resolved);

        public int ResolveSwing(AttackSnapshot attack, Quaternion facing, HashSet<HitReceiver> alreadyHit,
            out MeleeMissReason missReason, Action<HitReceiver, HitResolution> resolved = null)
        {
            Vector3 origin = transform.position + Vector3.up * hitHeight;
            int count = Physics.OverlapSphereNonAlloc(origin, attack.Range, overlaps, targetLayers, QueryTriggerInteraction.Ignore);
            int hits = 0;
            missReason = MeleeMissReason.NoTargetInReach;
            for (int i = 0; i < count; i++)
            {
                var target = overlaps[i].GetComponentInParent<HitReceiver>();
                if (target == null || transform == target.transform || transform.IsChildOf(target.transform) || alreadyHit.Contains(target)) continue;
                Promote(ref missReason, MeleeMissReason.TargetUnavailable);
                if (target.Defeated) continue;
                Promote(ref missReason, MeleeMissReason.OutsideArc);
                Vector3 point = overlaps[i].ClosestPoint(origin);
                Vector3 offset = point - origin;
                Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
                if (horizontal.sqrMagnitude > 0.001f
                    && Vector3.Angle(facing * Vector3.forward, horizontal) > attack.Arc * 0.5f) continue;
                Promote(ref missReason, MeleeMissReason.Obstructed);
                // The first solid object along the strike must be this target, not a wall or another target.
                if (Physics.Raycast(origin, offset.normalized, out RaycastHit obstruction, offset.magnitude + 0.05f,
                    obstructionLayers, QueryTriggerInteraction.Ignore)
                    && obstruction.collider.GetComponentInParent<HitReceiver>() != target) continue;
                alreadyHit.Add(target);
                if (target.ReceiveHit(new HitData(attack, transform.position, GetComponentInParent<HitReceiver>()), out HitResolution result))
                {
                    hits++;
                    resolved?.Invoke(target, result);
                }
            }
            if (hits > 0 || alreadyHit.Count > 0) missReason = MeleeMissReason.None;
            else if (count == overlaps.Length) missReason = MeleeMissReason.QueryCapacityReached;
            return hits;
        }

        private static void Promote(ref MeleeMissReason current, MeleeMissReason candidate)
        { if (candidate > current) current = candidate; }
    }
}
