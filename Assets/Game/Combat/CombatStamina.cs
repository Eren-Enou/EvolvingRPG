using UnityEngine;

namespace EvolvingRPG.Combat
{
    public sealed class CombatStamina : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximum = 100f;
        [SerializeField, Min(0f)] private float activeRegeneration = 12f;
        [SerializeField, Min(0f)] private float idleRegeneration = 30f;
        public float Current { get; private set; }
        public float Maximum => maximum;
        private CombatActions actions;

        private void Awake()
        {
            Current = maximum;
            actions = GetComponent<CombatActions>();
        }

        public bool TrySpend(float amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            return true;
        }

        private void Update()
        {
            Current = Mathf.Min(maximum, Current + (actions != null && actions.Busy
                ? activeRegeneration : idleRegeneration) * Time.deltaTime);
        }
    }
}
