using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Movement
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(CinemachineThirdPersonFollow))]
    public sealed class ThirdPersonCameraZoom : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float minimumDistance = 1.25f;
        [SerializeField, Min(0.1f)] private float maximumDistance = 7f;
        [SerializeField, Min(0.1f)] private float distance = 4f;
        [Tooltip("Metres per mouse-wheel notch (120 scroll units).")]
        [SerializeField, Min(0.01f)] private float zoomPerNotch = 0.5f;
        [SerializeField, Min(0f)] private float smoothTime = 0.08f;

        private CinemachineThirdPersonFollow follow;
        private float velocity;

        private void OnEnable()
        {
            follow = GetComponent<CinemachineThirdPersonFollow>();
            follow.CameraDistance = Mathf.Clamp(distance, minimumDistance, maximumDistance);
            velocity = 0f;
        }

        private void Update()
        {
            if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
                distance -= Mouse.current.scroll.ReadValue().y / 120f * zoomPerNotch;

            distance = Mathf.Clamp(distance, minimumDistance, maximumDistance);
            // Only set the requested rig distance. Cinemachine independently pulls in at obstacles.
            follow.CameraDistance = Mathf.SmoothDamp(follow.CameraDistance, distance, ref velocity, smoothTime);
        }

        private void OnValidate()
        {
            minimumDistance = Mathf.Max(0.1f, minimumDistance);
            maximumDistance = Mathf.Max(minimumDistance, maximumDistance);
            distance = Mathf.Clamp(distance, minimumDistance, maximumDistance);
        }
    }
}
