using UnityEngine;

public class SkeletonAI : EnemyAI
{
    [Header("Skeleton Specific Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 1.5f;
    
    private float lastAttackTime;
    private bool canAttack = true;
    
    protected override void OnEnemyStart()
    {
        // Skeleton özel ayarları
        moveSpeed = 2f;
        maxHealth = 10;
        damage = 5;
        xpReward = 10; // Skeleton XP ödülü
        itemDropChance = 0.4f; // %40 şans
        itemXPValue = 5; // Item XP değeri
        currentHealth = maxHealth;
    }
    
    protected override void OnEnemyUpdate()
    {
        // Skeleton hareket mantığı
        MoveTowardsPlayer();
        
        // Saldırı kontrolü
        CheckForAttack();
    }
    
    private void CheckForAttack()
    {
        if (!canAttack || player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }
    
    private void Attack()
    {
        lastAttackTime = Time.time;
        canAttack = false;
        
        // Saldırı animasyonu veya efekti burada eklenebilir
        Debug.Log("Skeleton attacks!");
        
        // Saldırı cooldown'ı
        StartCoroutine(AttackCooldown());
    }
    
    private System.Collections.IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    protected override void OnPlayerHit(Collider2D player)
    {
        // Skeleton özel saldırı mantığı
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Skeleton hit player for {damage} damage!");
        }
    }
    
    protected override string GetEnemyName()
    {
        return "Skeleton";
    }
    
    // Saldırı menzilini görselleştirmek için (debug)
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 