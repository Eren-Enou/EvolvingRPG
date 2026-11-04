using UnityEngine;

namespace EvolvingRPG.Combat
{
    [RequireComponent(typeof(SandboxEnemy), typeof(EnemyVitality))]
    public sealed class EnemyFeedback : MonoBehaviour
    {
        [SerializeField] private Renderer body;
        [SerializeField] private Transform weapon;
        [SerializeField] private GameObject shield;
        [SerializeField] private LineRenderer guide;
        private SandboxEnemy enemy;
        private EnemyVitality vitality;
        private MaterialPropertyBlock block;
        private float flashUntil;
        private void Awake()
        {
            enemy = GetComponent<SandboxEnemy>(); vitality = GetComponent<EnemyVitality>();
            block = new MaterialPropertyBlock(); vitality.HitReceived += OnHit;
        }
        private void OnDestroy() { if (vitality != null) vitality.HitReceived -= OnHit; }
        private void OnHit(HitData hit, HitResolution result) => flashUntil = Time.time + (result.PoiseBroken ? 0.3f : 0.12f);
        public void Configure(Renderer renderer, Transform blade, GameObject guard, LineRenderer line)
        { body = renderer; weapon = blade; shield = guard; guide = line; }
        private void LateUpdate()
        {
            if (body == null || guide == null || weapon == null) return;
            Color color = vitality.Defeated ? Color.gray : vitality.Staggered ? new Color(0.8f, 0.35f, 1f)
                : enemy.Phase == EnemyPhase.Windup ? Color.yellow : enemy.Phase == EnemyPhase.Active ? Color.red
                : enemy.Phase == EnemyPhase.Recovery ? Color.green : enemy.GuardedArchetype ? new Color(0.25f, 0.55f, 0.85f) : new Color(0.85f, 0.45f, 0.25f);
            body.GetPropertyBlock(block); block.SetColor("_BaseColor", Time.time < flashUntil ? Color.white : color); body.SetPropertyBlock(block);
            shield.SetActive(enemy.CanGuard);
            float angle = enemy.Phase == EnemyPhase.Windup ? Mathf.Lerp(-30, -110, enemy.PhaseProgress)
                : enemy.Phase == EnemyPhase.Active ? Mathf.Lerp(-110, 75, enemy.PhaseProgress) : 90;
            weapon.localRotation = Quaternion.Euler(0, angle, 0);
            guide.enabled = enemy.Phase == EnemyPhase.Windup || enemy.Phase == EnemyPhase.Active;
            guide.startColor = guide.endColor = color;
            if (!guide.enabled) return;
            guide.positionCount = 27; guide.SetPosition(0, Vector3.up * 0.08f);
            for (int i = 0; i <= 24; i++) guide.SetPosition(i + 1, Quaternion.Euler(0, Mathf.Lerp(-enemy.AttackArc / 2, enemy.AttackArc / 2, i / 24f), 0)
                * Vector3.forward * enemy.AttackRange + Vector3.up * 0.08f);
            guide.SetPosition(26, Vector3.up * 0.08f);
        }
    }
}
