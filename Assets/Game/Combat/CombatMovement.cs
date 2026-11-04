using StarterAssets;
using UnityEngine;

namespace EvolvingRPG.Combat
{
    // Nested inside DedicatedStrafe's before/after update bracket; the vendor remains the only motor.
    [DefaultExecutionOrder(-25)]
    [RequireComponent(typeof(CombatMovementRestore))]
    public sealed class CombatMovement : MonoBehaviour
    {
        private CombatActions actions;
        private ThirdPersonController controller;
        private StarterAssetsInputs inputs;
        private Transform movementCamera;
        private bool prepared;
        private bool restoreFacing;
        private Vector2 move;
        private bool sprint;
        private float moveSpeed, sprintSpeed, response, rotationTime;

        private void Awake()
        {
            actions = GetComponent<CombatActions>();
            controller = GetComponent<ThirdPersonController>();
            inputs = GetComponent<StarterAssetsInputs>();
        }
        private void Start() { if (Camera.main != null) movementCamera = Camera.main.transform; }

        private void Update()
        {
            Restore();
            if (actions == null || !actions.enabled || !controller.enabled) return;
            prepared = true;
            move = inputs.move; sprint = inputs.sprint;
            moveSpeed = controller.MoveSpeed; sprintSpeed = controller.SprintSpeed;
            response = controller.SpeedChangeRate; rotationTime = controller.RotationSmoothTime;
            inputs.sprint = false; // Shift is dodge only in scenes with this component.
            restoreFacing = actions.Busy || actions.MovementBlocked || actions.DodgeSpeedThisFrame > 0f || actions.AttackAdvanceSpeedThisFrame > 0f;
            if (!restoreFacing) return;
            // Jump is a consumed request, not reversible movement state. Drop it during commitment;
            // otherwise let the vendor clear it in the air. Never restore an old true value afterward.
            inputs.jump = false;
            controller.RotationSmoothTime = 0f;
            // Eliminate the vendor motor's residual horizontal drift for committed stops and fixed-distance dodges.
            controller.SpeedChangeRate = Time.deltaTime > 0f ? 1f / Time.deltaTime + 1f : response;
            float speed = 0f;
            if (actions.MovementBlocked) inputs.move = Vector2.zero;
            else if ((actions.DodgeSpeedThisFrame > 0f || actions.AttackAdvanceSpeedThisFrame > 0f) && movementCamera != null)
            {
                Vector3 direction = actions.DodgeSpeedThisFrame > 0f ? actions.DodgeDirection : actions.Facing * Vector3.forward;
                Vector3 local = Quaternion.Inverse(Quaternion.Euler(0, movementCamera.eulerAngles.y, 0)) * direction;
                inputs.move = new Vector2(local.x, local.z);
                speed = actions.DodgeSpeedThisFrame > 0f ? actions.DodgeSpeedThisFrame : actions.AttackAdvanceSpeedThisFrame;
            }
            else if (actions.HasCommittedAttack)
                speed = moveSpeed * actions.CommittedAttack.MovementMultiplier;
            controller.MoveSpeed = controller.SprintSpeed = speed;
        }

        public void Restore()
        {
            if (!prepared) return;
            prepared = false;
            if (restoreFacing) transform.rotation = actions.Facing;
            inputs.move = move; inputs.sprint = sprint;
            controller.MoveSpeed = moveSpeed; controller.SprintSpeed = sprintSpeed;
            controller.SpeedChangeRate = response; controller.RotationSmoothTime = rotationTime;
        }
        private void LateUpdate() => Restore();
        private void OnDisable() => Restore();
    }
}
