using UnityEngine;

namespace EvolvingRPG.Combat
{
    // Receiver owns impact resolution. Geometry queries know nothing about health or guard rules.
    public abstract class HitReceiver : MonoBehaviour
    {
        public abstract bool Defeated { get; }
        public abstract bool ReceiveHit(HitData hit, out HitResolution resolution);
    }
}
