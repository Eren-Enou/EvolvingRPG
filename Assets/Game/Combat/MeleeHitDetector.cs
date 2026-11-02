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

        // Geometry queries only. Target health/poise resolution lives in TrainingTarget.
        public int ResolveSwing(AttackDefinition attack, Quaternion facing, HashSet<TrainingTarget> alreadyHit)
        {
            Vector3 origin = transform.position + Vector3.up * hitHeight;
            int count = Physics.OverlapSphereNonAlloc(origin, attack.Range, overlaps, targetLayers, QueryTriggerInteraction.Ignore);
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                var target = overlaps[i].GetComponentInParent<TrainingTarget>();
                if (target == null || target.Defeated || alreadyHit.Contains(target)) continue;
                Vector3 point = overlaps[i].ClosestPoint(origin);
                Vector3 offset = point - origin;
                Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
                if (horizontal.sqrMagnitude > 0.001f
                    && Vector3.Angle(facing * Vector3.forward, horizontal) > attack.Arc * 0.5f) continue;
                // The first solid object along the strike must be this target, not a wall or another target.
                if (Physics.Raycast(origin, offset.normalized, out RaycastHit obstruction, offset.magnitude + 0.05f,
                    obstructionLayers, QueryTriggerInteraction.Ignore)
                    && obstruction.collider.GetComponentInParent<TrainingTarget>() != target) continue;
                alreadyHit.Add(target);
                if (target.ReceiveHit(new HitData(attack))) hits++;
            }
            return hits;
        }
    }
}
