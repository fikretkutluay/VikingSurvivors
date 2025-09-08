using UnityEngine;

public class RockGolemAI : EnemyAI
{
    [Header("Elite Settings")]
    [SerializeField] private bool isElite = true; // Görsel/etiket amaçlı

    [Header("Death Explosion")]
    [SerializeField] private float explosionRadius = 3.5f;
    [SerializeField] private int explosionDamageToPlayer = 20;
    [SerializeField] private int explosionDamageToEnemies = 50;
    [SerializeField] private GameObject rockDebrisPrefab; // Kaya parçacıkları efekti (ana VFX)

    protected override void OnEnemyStart()
    {
        // Hantal ama tank
        moveSpeed = 0.8f;
        maxHealth = 300;
        currentHealth = maxHealth;
        damage = 10; // temas hasarı
    }

    protected override void OnEnemyUpdate()
    {
        // Basit: sürekli oyuncuya yürü
        MoveTowardsPlayer();
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Önce patlama uygula, sonra base.Die() ile drop ve yok etme
        PerformDeathExplosion();

        base.Die();
    }

    private void PerformDeathExplosion()
    {
        Debug.Log($"[RockGolem] Death explosion triggered at {transform.position} | radius={explosionRadius}");
        
        // Kaya parçacıkları efekti (ana VFX)
        if (rockDebrisPrefab != null)
        {
            Instantiate(rockDebrisPrefab, transform.position, Quaternion.identity);
            Debug.Log($"[RockGolem] Rock debris effect spawned");
        }

        // Player'a zarar ver
        if (player != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer <= explosionRadius)
            {
                var playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(explosionDamageToPlayer);
                    Debug.Log($"[RockGolem] Player hit by explosion. distance={distToPlayer:F2}, damage={explosionDamageToPlayer}");
                }
            }
            else
            {
                Debug.Log($"[RockGolem] Player outside explosion radius. distance={distToPlayer:F2}");
            }
        }

        // Yakındaki düşmanlara zarar ver (kendisi hariç)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        int enemiesHit = 0;
        foreach (var hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject) continue;
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(explosionDamageToEnemies);
                enemiesHit++;
            }
        }
        Debug.Log($"[RockGolem] Explosion damaged {enemiesHit} enemies.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    protected override string GetEnemyName()
    {
        return "Rock Golem (Elite)";
    }
}


