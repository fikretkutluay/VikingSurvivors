using UnityEngine;
using System.Collections;

public class IceGoblinAI : EnemyAI
{
    [Header("Ice Projectile Attack")]
    [SerializeField] private float iceProjectileCooldown = 2.5f;
    [SerializeField] private float iceProjectileRange = 10f; // Uzun menzil

    

    
    [Header("Ice Projectile")]
    [SerializeField] private GameObject iceProjectilePrefab; // Buz küresi prefab'ı
    [SerializeField] private float projectileSpeed = 4f;
    [SerializeField] private float projectileLifetime = 8f; // Uzun yaşam süresi
    
    private float lastIceProjectileTime;

    
    protected override void OnEnemyStart()
    {
        // Ice Goblin özel ayarları (Draugr'dan daha güçlü)
        moveSpeed = 2.2f; // Draugr'dan hızlı
        maxHealth = 35; // Draugr'dan daha fazla can
        damage = 12; // Draugr'dan daha fazla hasar
        xpReward = 0; // Direkt XP verme, sadece item düşür
        itemDropChance = 0.7f; // %70 şans
        itemXPValue = 35; // Daha fazla XP
        healthValue = 30; // Can itemi değeri
        currentHealth = maxHealth;
        

    }
    
    protected override void OnEnemyUpdate()
    {
        // Ice Goblin hareket mantığı
        MoveTowardsPlayer();
        
        // Buz küresi atma kontrolü
        CheckForIceProjectile();
    }
    
    private void CheckForIceProjectile()
    {
        if (player == null || Time.time < lastIceProjectileTime + iceProjectileCooldown) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= iceProjectileRange)
        {
            PerformIceProjectile();
        }
    }
    
    private void PerformIceProjectile()
    {
        lastIceProjectileTime = Time.time;
        
        // Buz küresi at
        if (iceProjectilePrefab != null && player != null)
        {
            // Buz küresini oluştur
            GameObject iceProjectile = Instantiate(iceProjectilePrefab, transform.position, Quaternion.identity);
            
            // Buz küresine takip sistemi ekle
            IceProjectile iceProjectileScript = iceProjectile.GetComponent<IceProjectile>();
            if (iceProjectileScript != null)
            {
                iceProjectileScript.Initialize(player, projectileSpeed);
            }
            
            // Buz küresini belirli süre sonra yok et
            Destroy(iceProjectile, projectileLifetime);
        }
    }
    

    

    
    public override void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // Normal hasar alma
        base.TakeDamage(damage);
    }
    
    protected override void OnPlayerHit(Collider2D player)
    {
        // Ice Goblin normal saldırısı (sadece hasar, yavaşlatma yok)
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
    
    protected override string GetEnemyName()
    {
        return "Ice Goblin";
    }
    
    // Gizmos ile menzilleri görselleştir
    protected virtual void OnDrawGizmosSelected()
    {
        // Buz küresi atma menzili
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, iceProjectileRange);
    }
    

}
