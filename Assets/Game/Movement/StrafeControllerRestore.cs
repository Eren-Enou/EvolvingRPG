using UnityEngine;

namespace EvolvingRPG.Movement
{
    // Starter Assets has no public move/turn hook. Finish the adapter after its default-order Update.
    [DefaultExecutionOrder(50)]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class StrafeControllerRestore : MonoBehaviour
    {
        private DedicatedStrafe strafe;

        private void Awake() => strafe = GetComponent<DedicatedStrafe>();
        private void Update()
        {
            if (strafe != null)
                strafe.RestoreControllerState();
        }
    }
}
