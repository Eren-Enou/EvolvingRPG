using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Movement
{
    // Supply scaled mouse input before the vendor controller updates its camera target.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(StarterAssetsInputs))]
    public sealed class PlaygroundMouseLook : MonoBehaviour
    {
        [SerializeField, Min(0.001f)] private float sensitivity = 0.12f;
        [SerializeField] private bool invertY;

        private StarterAssetsInputs inputs;

        private void Awake()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            inputs.cursorInputForLook = false;
            inputs.lookFromTouch = false;
        }

        private void Start() => Capture(Application.isFocused);

        private void Update()
        {
            inputs.look = Vector2.zero;
            if (!Application.isFocused)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Capture(false);
            else if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null
                     && Mouse.current.leftButton.wasPressedThisFrame)
                Capture(true);

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                inputs.move = Vector2.zero;
                inputs.jump = false;
                inputs.sprint = false;
                return;
            }

            if (Mouse.current == null)
                return;

            Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;
            if (invertY)
                delta.y = -delta.y;
            inputs.LookInput(delta);
        }

        private void Capture(bool capture)
        {
            // Keep the vendor focus handler consistent with our cursor state.
            inputs.cursorLocked = capture;
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
            inputs.look = Vector2.zero;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && inputs != null)
            {
                Capture(false);
                inputs.move = Vector2.zero;
                inputs.jump = false;
                inputs.sprint = false;
            }
        }

        private void OnDisable()
        {
            if (inputs != null)
                Capture(false);
        }
    }
}
