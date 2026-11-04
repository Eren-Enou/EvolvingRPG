using UnityEngine;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(CombatActions))]
    public sealed class CombatActionFeedback : MonoBehaviour
    {
        [SerializeField] private Transform bladePivot;
        [SerializeField] private Renderer blade;
        [SerializeField] private LineRenderer reachGuide;
        private CombatActions actions;
        private MaterialPropertyBlock properties;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public void Configure(Transform pivot, Renderer renderer, LineRenderer guide)
        { bladePivot = pivot; blade = renderer; reachGuide = guide; }
        private void Awake() { actions = GetComponent<CombatActions>(); properties = new MaterialPropertyBlock(); }

        private void LateUpdate()
        {
            if (bladePivot == null || blade == null || reachGuide == null) return;
            var attack = actions.CommittedAttack;
            bool attacking = actions.HasCommittedAttack;
            Color color = attacking ? attack.Color : actions.Busy ? new Color(0.7f, 0.4f, 1f) : Color.gray;
            float angle = 100f;
            if (attacking)
            {
                angle = actions.Phase == CombatPhase.Windup ? Mathf.Lerp(-45f, -110f, actions.PhaseProgress)
                    : actions.Phase == CombatPhase.Active ? Mathf.Lerp(-110f, 75f, actions.PhaseProgress)
                    : Mathf.Lerp(75f, 100f, actions.PhaseProgress);
            }
            bladePivot.localRotation = Quaternion.Euler(attacking && actions.Phase == CombatPhase.Windup ? -25f : 0, angle, 0);
            blade.GetPropertyBlock(properties);
            properties.SetColor(BaseColor, color);
            blade.SetPropertyBlock(properties);
            reachGuide.enabled = attacking;
            if (!attacking) return;
            color.a = actions.Phase == CombatPhase.Active ? 1f : 0.45f;
            reachGuide.startColor = reachGuide.endColor = color;
            reachGuide.positionCount = 27;
            reachGuide.SetPosition(0, Vector3.up * 0.05f);
            for (int i = 0; i <= 24; i++)
            {
                float yaw = Mathf.Lerp(-attack.Arc * 0.5f, attack.Arc * 0.5f, i / 24f);
                reachGuide.SetPosition(i + 1, Quaternion.Euler(0, yaw, 0) * Vector3.forward * attack.Range + Vector3.up * 0.05f);
            }
            reachGuide.SetPosition(26, Vector3.up * 0.05f);
        }
    }
}
