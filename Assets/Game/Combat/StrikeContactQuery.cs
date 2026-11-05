using UnityEngine;

namespace EvolvingRPG.Combat
{
    public readonly struct StrikeGeometry
    {
        public readonly Vector3 Origin, Forward;
        public readonly float Range, Arc;
        public StrikeGeometry(Vector3 origin, Vector3 forward, float range, float arc)
        { Origin = origin; Forward = forward; Range = range; Arc = arc; }
        public StrikeGeometry At(Vector3 origin) => new StrikeGeometry(origin, Forward, Range, Arc);
    }

    public readonly struct StrikeCapsule
    {
        public readonly Vector3 Center, SightPoint;
        public readonly float Radius, HalfSegment;
        public readonly bool Supported;
        public StrikeCapsule(Vector3 center, float radius, float halfSegment, Vector3 sightPoint, bool supported = true)
        { Center = center; Radius = radius; HalfSegment = halfSegment; SightPoint = sightPoint; Supported = supported; }

        public static StrikeCapsule Capture(CharacterController controller)
        {
            if (controller == null || !controller.enabled) return default;
            var t = controller.transform; var scale = t.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float half = Mathf.Max(radius, controller.height * Mathf.Abs(scale.y) * 0.5f);
            bool supported = radius > 0 && Vector3.Dot(t.up, Vector3.up) > 0.9999f
                && Mathf.Abs(Mathf.Abs(scale.x) - Mathf.Abs(scale.z)) < 0.001f;
            return new StrikeCapsule(t.TransformPoint(controller.center), radius, half - radius,
                t.position + Vector3.up, supported);
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            Vector3 axis = new Vector3(Center.x, Mathf.Clamp(point.y, Center.y - HalfSegment, Center.y + HalfSegment), Center.z);
            Vector3 offset = point - axis;
            return offset.sqrMagnitude <= Radius * Radius ? point : axis + offset.normalized * Radius;
        }

        public bool SameShape(StrikeCapsule other) => Supported && other.Supported
            && Mathf.Abs(Radius - other.Radius) < 0.001f && Mathf.Abs(HalfSegment - other.HalfSegment) < 0.001f
            && Mathf.Abs(Center.y - other.Center.y) < 0.05f;
    }

    // No damage, state changes, collider movement, or temporary proxy colliders.
    public static class StrikeContactQuery
    {
        public static bool GeometryContacts(StrikeGeometry strike, Vector3 closestPoint)
        {
            Vector3 offset = closestPoint - strike.Origin;
            return offset.magnitude <= strike.Range
                && Vector3.Angle(strike.Forward, Vector3.ProjectOnPlane(offset, Vector3.up)) <= strike.Arc * 0.5f;
        }

        public static bool Visible(Vector3 origin, Vector3 sightPoint, CombatVitality target, out bool dynamicBlocker)
        {
            Vector3 offset = sightPoint - origin;
            bool blocked = Physics.Raycast(origin, offset.normalized, out var hit, offset.magnitude, 1, QueryTriggerInteraction.Ignore)
                && hit.collider.GetComponentInParent<CombatVitality>() != target;
            dynamicBlocker = blocked && !hit.collider.gameObject.isStatic;
            return !blocked;
        }

        // Actual damage preserves Collider.ClosestPoint and the existing default-layer LOS contract.
        public static bool Contacts(StrikeGeometry strike, CharacterController body, CombatVitality target)
            => body != null && GeometryContacts(strike, body.ClosestPoint(strike.Origin))
                && Visible(strike.Origin, body.transform.position + Vector3.up, target, out _);

        public static bool Contacts(StrikeGeometry strike, StrikeCapsule pose, CombatVitality target, out bool dynamicBlocker)
        {
            dynamicBlocker = false;
            return pose.Supported && GeometryContacts(strike, pose.ClosestPoint(strike.Origin))
                && Visible(strike.Origin, pose.SightPoint, target, out dynamicBlocker);
        }

        // Translate the query into the live controller's frame, then translate its result back.
        // Unity's controller query capsule can be smaller than its authored dimensions. Using
        // that same native query preserves the actual strike boundary without moving a body.
        public static bool ContactsAtPose(StrikeGeometry strike, StrikeCapsule pose,
            CharacterController body, CombatVitality target, out bool dynamicBlocker)
        {
            dynamicBlocker = false;
            if (body == null || !pose.Supported) return false;
            var live = StrikeCapsule.Capture(body);
            if (!pose.SameShape(live)) return false;
            Vector3 translation = live.Center - pose.Center;
            Vector3 point = body.ClosestPoint(strike.Origin + translation) - translation;
            return GeometryContacts(strike, point)
                && Visible(strike.Origin, pose.SightPoint, target, out dynamicBlocker);
        }
        // Enclose ALL supported motion from the previous pose, rather than assuming a linear
        // endpoint interpolation. Ignoring LOS here prevents wall protection becoming evasion.
        public static bool ProvesClear(StrikeGeometry strike, StrikeCapsule previous, float maximumTravel)
        {
            float radius = previous.Radius + maximumTravel + 0.02f;
            var enclosure = new StrikeCapsule(previous.Center, radius, previous.HalfSegment, previous.SightPoint);
            if ((enclosure.ClosestPoint(strike.Origin) - strike.Origin).magnitude > strike.Range + 0.001f) return true;
            Vector3 horizontal = Vector3.ProjectOnPlane(previous.Center - strike.Origin, Vector3.up);
            if (horizontal.magnitude <= radius) return false;
            float angularRadius = Mathf.Asin(Mathf.Clamp01(radius / horizontal.magnitude)) * Mathf.Rad2Deg;
            return Vector3.Angle(strike.Forward, horizontal) > strike.Arc * 0.5f + angularRadius + 0.01f;
        }
    }
}
