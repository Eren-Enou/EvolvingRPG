using UnityEngine;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(TrainingTarget))]
    public sealed class TrainingTargetFeedback : MonoBehaviour
    {
        [SerializeField] private Renderer body;
        [SerializeField] private Transform visual;
        private TrainingTarget target;
        private MaterialPropertyBlock properties;
        private float flashUntil;
        private float reaction;
        private Color hitColor;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public void Configure(Renderer renderer, Transform model) { body = renderer; visual = model; }
        private void Awake()
        {
            target = GetComponent<TrainingTarget>();
            properties = new MaterialPropertyBlock();
        }
        private void OnEnable() { target.HitReceived += OnHit; target.ResetPerformed += OnReset; }
        private void OnDisable() { target.HitReceived -= OnHit; target.ResetPerformed -= OnReset; }
        private void OnHit(HitData hit, bool staggered)
        {
            flashUntil = Time.time + (staggered ? 0.4f : 0.18f);
            reaction = Mathf.Clamp(hit.PoiseDamage / target.PoiseThreshold, 0.1f, 0.9f);
            hitColor = staggered ? Color.white : hit.Color;
        }
        private void OnReset() { flashUntil = 0; reaction = 0; }
        private void LateUpdate()
        {
            if (body != null)
            {
                Color color = target.Defeated ? new Color(0.16f, 0.16f, 0.18f) : Time.time < flashUntil ? hitColor
                    : target.Staggered ? new Color(1f, 0.75f, 0.1f) : new Color(0.4f, 0.65f, 0.45f);
                body.GetPropertyBlock(properties);
                properties.SetColor(BaseColor, color);
                body.SetPropertyBlock(properties);
            }
            if (visual != null)
            {
                reaction = Mathf.MoveTowards(reaction, 0, Time.deltaTime * 3f);
                visual.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 35f) * reaction * 18f, 0, target.Defeated ? 65f : 0);
            }
        }
    }
}
