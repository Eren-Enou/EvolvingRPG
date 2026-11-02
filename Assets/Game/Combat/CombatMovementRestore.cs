using UnityEngine;

namespace EvolvingRPG.Combat
{
    [DefaultExecutionOrder(25)]
    [AddComponentMenu("")]
    public sealed class CombatMovementRestore : MonoBehaviour
    {
        private CombatMovement movement;
        private void Awake() => movement = GetComponent<CombatMovement>();
        private void Update() { if (movement != null) movement.Restore(); }
    }
}
