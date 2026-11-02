using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Movement
{
    // Bracket the vendor Update: translate input before it, restore facing before animation/camera evaluation.
    // The vendor still owns horizontal speed, CharacterController.Move, gravity, jumping and animation.
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ThirdPersonController), typeof(StarterAssetsInputs))]
    [RequireComponent(typeof(StrafeControllerRestore))]
    public sealed class DedicatedStrafe : MonoBehaviour
    {
        [SerializeField] private InputAction strafeLeft = new InputAction("StrafeLeft", InputActionType.Button, "<Keyboard>/q");
        [SerializeField] private InputAction strafeRight = new InputAction("StrafeRight", InputActionType.Button, "<Keyboard>/e");
        [Tooltip("Maximum movement speed while Q/E is held, including diagonal movement, in metres/second.")]
        [SerializeField, Min(0f)] private float strafeSpeed = 4f;
        [Tooltip("Acceleration/deceleration rate passed to Starter Assets while strafing. Higher responds faster.")]
        [SerializeField, Min(0.01f)] private float responsiveness = 18f;

        private ThirdPersonController controller;
        private StarterAssetsInputs inputs;
        private Transform movementCamera;
        private bool prepared;
        private Quaternion facing;
        private Vector2 originalMove;
        private float originalMoveSpeed;
        private float originalSprintSpeed;
        private float originalResponsiveness;
        private float originalRotationSmoothTime;

        private void Awake()
        {
            controller = GetComponent<ThirdPersonController>();
            inputs = GetComponent<StarterAssetsInputs>();
        }

        private void OnEnable()
        {
            strafeLeft.Enable();
            strafeRight.Enable();
        }

        private void Start()
        {
            var camera = Camera.main;
            if (camera != null)
                movementCamera = camera.transform;
        }

        private void Update()
        {
            RestoreControllerState();
            if (!controller.enabled || movementCamera == null || !Application.isFocused
                || Cursor.lockState != CursorLockMode.Locked)
                return;

            bool left = strafeLeft.IsPressed();
            bool right = strafeRight.IsPressed();
            // Both held is still facing-locked, with exactly zero Q/E lateral input.
            if (!left && !right)
                return;

            facing = transform.rotation;
            originalMove = inputs.move;
            originalMoveSpeed = controller.MoveSpeed;
            originalSprintSpeed = controller.SprintSpeed;
            originalResponsiveness = controller.SpeedChangeRate;
            originalRotationSmoothTime = controller.RotationSmoothTime;
            prepared = true;

            float lateral = (right ? 1f : 0f) - (left ? 1f : 0f);
            Vector2 localMove = CombineInput(originalMove, lateral);
            Vector3 worldMove = facing * new Vector3(localMove.x, 0f, localMove.y);
            Quaternion cameraYaw = Quaternion.Euler(0f, movementCamera.eulerAngles.y, 0f);
            Vector3 cameraMove = Quaternion.Inverse(cameraYaw) * worldMove;
            inputs.move = new Vector2(cameraMove.x, cameraMove.z);

            controller.MoveSpeed = strafeSpeed;
            // Preserve the existing sprint ratio without letting diagonal input add speed.
            controller.SprintSpeed = originalMoveSpeed > 0f
                ? strafeSpeed * originalSprintSpeed / originalMoveSpeed : strafeSpeed;
            controller.SpeedChangeRate = responsiveness;
            // Avoid carrying the vendor's turn-smoothing momentum out of facing-locked movement.
            controller.RotationSmoothTime = 0f;
        }

        public static Vector2 CombineInput(Vector2 movement, float lateral)
        {
            return Vector2.ClampMagnitude(movement + Vector2.right * lateral, 1f);
        }

        // Called by StrafeControllerRestore.Update after the vendor Update, before Animator evaluation.
        public void RestoreControllerState()
        {
            if (!prepared)
                return;
            prepared = false;
            transform.rotation = facing;
            inputs.move = originalMove;
            controller.MoveSpeed = originalMoveSpeed;
            controller.SprintSpeed = originalSprintSpeed;
            controller.SpeedChangeRate = originalResponsiveness;
            controller.RotationSmoothTime = originalRotationSmoothTime;
        }

        private void LateUpdate() => RestoreControllerState();

        private void OnDisable()
        {
            RestoreControllerState();
            strafeLeft.Disable();
            strafeRight.Disable();
        }
    }
}
