using UnityEngine;

public class DraugrAI : EnemyAI
{
    [Header("Draugr Specific Settings")]
    [SerializeField] private float chargeSpeed = 4f;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float chargeRange = 3f;
    [SerializeField] private float stunDuration = 1f;
    
    private float lastChargeTime;
    private bool isCharging = false;
    private bool isStunned = false;
    private Vector3 chargeTarget;
    
    protected override void OnEnemyStart()
    {
        // Draugr özel ayarları
        moveSpeed = 1.5f;
        maxHealth = 20;
        damage = 8;
        xpReward = 25; // Draugr daha fazla XP verir
        itemDropChance = 0.6f; // %60 şans (daha yüksek)
        itemXPValue = 10; // Daha fazla XP
        currentHealth = maxHealth;
    }
    
    protected override void OnEnemyUpdate()
    {
        if (isStunned) return;
        
        // Draugr hareket mantığı
        if (!isCharging)
        {
            MoveTowardsPlayer();
            CheckForCharge();
        }
        else
        {
            ChargeAttack();
        }
    }
    
    private void CheckForCharge()
    {
        if (player == null || Time.time < lastChargeTime + chargeCooldown) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= chargeRange)
        {
            StartCharge();
        }
    }
    
    private void StartCharge()
    {
        isCharging = true;
        chargeTarget = player.position;
        lastChargeTime = Time.time;
        
        Debug.Log("Draugr starts charging!");
    }
    
    private void ChargeAttack()
    {
        // Charge hedefine doğru hızlı hareket
        Vector3 direction = (chargeTarget - transform.position).normalized;
        transform.Translate(direction * chargeSpeed * Time.deltaTime);
        
        // Charge mesafesini kontrol et
        float distanceToTarget = Vector2.Distance(transform.position, chargeTarget);
        
        if (distanceToTarget < 0.5f)
        {
            EndCharge();
        }
    }
    
    private void EndCharge()
    {
        isCharging = false;
        
        // Charge sonrası stun
        StartCoroutine(StunAfterCharge());
    }
    
    private System.Collections.IEnumerator StunAfterCharge()
    {
        isStunned = true;
        Debug.Log("Draugr is stunned after charge!");
        
        yield return new WaitForSeconds(stunDuration);
        
        isStunned = false;
        Debug.Log("Draugr recovered from stun!");
    }
    
    protected override void OnPlayerHit(Collider2D player)
    {
        // Draugr özel saldırı mantığı
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            int finalDamage = isCharging ? damage * 2 : damage;
            playerHealth.TakeDamage(finalDamage);
            
            if (isCharging)
            {
                Debug.Log($"Draugr charged attack hit player for {finalDamage} damage!");
            }
            else
            {
                Debug.Log($"Draugr hit player for {finalDamage} damage!");
            }
        }
    }
    
    protected override string GetEnemyName()
    {
        return "Draugr";
    }
    
    // Charge menzilini görselleştirmek için (debug)
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        
        if (isCharging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, chargeTarget);
        }
    }
} 