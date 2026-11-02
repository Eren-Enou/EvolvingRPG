using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolvingRPG.Combat
{
    public sealed class CombatSandboxHUD : MonoBehaviour
    {
        [SerializeField] private CombatActions player;
        [SerializeField] private TrainingTarget target;
        [SerializeField] private InputAction reset = new InputAction("ResetTarget", InputActionType.Button, "<Keyboard>/r");
        private CombatStamina stamina;
        private GUIStyle title;
        private GUIStyle text;
        public void Configure(CombatActions actions, TrainingTarget trainingTarget) { player = actions; target = trainingTarget; }
        private void Start() { if (player != null) stamina = player.GetComponent<CombatStamina>(); }
        private void OnEnable() => reset.Enable();
        private void OnDisable() => reset.Disable();
        private void Update()
        {
            if (Application.isFocused && Cursor.lockState == CursorLockMode.Locked && reset.WasPressedThisFrame() && target != null)
                target.ResetTarget();
        }

        private void OnGUI()
        {
            if (player == null || target == null || stamina == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 13 };
                title.normal.textColor = text.normal.textColor = Color.white;
            }
            GUI.Box(new Rect(12, 12, 455, 204), GUIContent.none);
            GUI.Label(new Rect(24, 20, 430, 26), "EVOLVING RPG  /  COMBAT SANDBOX", title);
            GUI.Label(new Rect(24, 50, 430, 22), "LMB Light   |   RMB Heavy   |   Left Shift Dodge   |   R Reset", text);
            GUI.Label(new Rect(24, 73, 430, 22), "WASD / Q-E move   |   Space jump   |   Mouse / wheel camera", text);
            GUI.Label(new Rect(24, 97, 430, 22), $"{player.Phase}  {player.PhaseProgress:P0}   |   {player.Message}", text);
            Bar(new Rect(24, 125, 430, 18), stamina.Current / stamina.Maximum, new Color(0.3f, 0.75f, 1f), $"Stamina {stamina.Current:0}/{stamina.Maximum:0}");
            Bar(new Rect(24, 150, 430, 18), target.Health / target.MaximumHealth, new Color(0.3f, 0.8f, 0.4f), $"Target HP {target.Health:0}/{target.MaximumHealth:0}");
            Bar(new Rect(24, 175, 430, 18), target.Poise / target.PoiseThreshold, new Color(1f, 0.65f, 0.15f), $"Poise {target.Poise:0}/{target.PoiseThreshold:0}  {(target.Staggered ? "STAGGERED" : target.Defeated ? "DEFEATED" : "")}");
            GUI.Label(new Rect(24, 219, 600, 24), target.LastHit + "   |   Escape releases cursor", text);
        }

        private void Bar(Rect rect, float fraction, Color color, string label)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.1f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(fraction), rect.height), Texture2D.whiteTexture);
            GUI.color = previous;
            GUI.Label(new Rect(rect.x + 5, rect.y - 2, rect.width, rect.height + 4), label, text);
        }
    }
}
