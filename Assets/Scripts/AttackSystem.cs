using UnityEngine;
using System.Collections.Generic; // Added for List

public class AttackSystem : MonoBehaviour
{
    [Header("Sword Attack Settings")]
    [SerializeField] private float baseSwordCooldown = 1.5f; // Base cooldown
    [SerializeField] private int baseSwordDamage = 10; // Base damage
    [SerializeField] private float swordRange = 2f; // Kılıç menzili
    
    [Header("Fireball Attack Settings")]
    [SerializeField] private float baseFireballCooldown = 2.5f; // Base cooldown
    [SerializeField] private float fireballRange = 17.5f;
    [SerializeField] private int baseFireballDamage = 15; // Base damage
    [SerializeField] private float fireballSpeed = 10f;
    [SerializeField] private int baseFireballCount = 1; // Base count
    
    [Header("Current Values (Updated by Skills)")]
    [SerializeField] private float currentSwordCooldown;
    [SerializeField] private int currentSwordDamage;
    [SerializeField] private float currentFireballCooldown;
    [SerializeField] private int currentFireballDamage;
    [SerializeField] private int currentFireballCount;
    
    [Header("Skill Activation")]
    [SerializeField] private bool isFireballActive = false; // Level 4'te aktif olacak
    
    [Header("Attack Effects")]
    [SerializeField] private GameObject swordEffectPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform attackPoint;
    
    void Start()
    {
        // Initialize current values with base values
        currentSwordCooldown = baseSwordCooldown;
        currentSwordDamage = baseSwordDamage;
        currentFireballCooldown = baseFireballCooldown;
        currentFireballDamage = baseFireballDamage;
        currentFireballCount = baseFireballCount;
    }
    
    [Header("Fireball Spawn Points")]
    [SerializeField] private Transform fireballSpawnTopRight;    // Sağ üst çıkış noktası (45°)
    [SerializeField] private Transform fireballSpawnBottomLeft; // Sol alt çıkış noktası (225°)
    [SerializeField] private Transform fireballSpawnTopLeft;     // Sol üst çıkış noktası (135°)
    [SerializeField] private Transform fireballSpawnBottomRight;// Sağ alt çıkış noktası (315°)
    
    private float lastSwordAttackTime;
    private float lastFireballAttackTime;
    
    void Update()
    {
        // Kılıç saldırısı aktif
        if (CanUseSword())
        {
            UseSword();
        }
        
        // Otomatik alev topu saldırısı
        if (CanUseFireball())
        {
            UseFireball();
        }
    }
    
    // Kılıç saldırısı
    bool CanUseSword()
    {
        return Time.time >= lastSwordAttackTime + currentSwordCooldown;
    }
    
    void UseSword()
    {
        if (swordEffectPrefab != null)
        {
            // Attack point pozisyonunu sprite yönüne göre ayarla
            UpdateAttackPointPosition();
            
            // Kılıç efekti oluştur
            GameObject effect = Instantiate(swordEffectPrefab, attackPoint.position, attackPoint.rotation);
            effect.transform.SetParent(transform);
            
            // Efektin yönünü ayarla (sprite flip'e göre)
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite != null && playerSprite.flipX)
            {
                effect.transform.localScale = new Vector3(-1, 1, 1);
            }
            
            // Efekti belirli süre sonra yok et
            Destroy(effect, 0.5f);
            
            // Yakındaki düşmanlara hasar ver (range kontrolü yok, sürekli vurur)
            CheckForEnemies(swordRange, currentSwordDamage); // Sabit 2f range kullan
        }
        
        lastSwordAttackTime = Time.time;
    }
    
    // Alev topu saldırısı
    bool CanUseFireball()
    {
        return isFireballActive && Time.time >= lastFireballAttackTime + currentFireballCooldown;
    }
    
    void UseFireball()
    {
        if (fireballPrefab != null)
        {
            // Level'a göre kaç yönde alev topu atılacak
            int directionsToFire = Mathf.Min(currentFireballCount, 4); // Maksimum 4 yön
            
            // Karakterin yönünü kontrol et
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            bool isFacingLeft = playerSprite != null && playerSprite.flipX;
            
            // Her yönde alev topu at
            for (int i = 0; i < directionsToFire; i++)
            {
                // Manuel spawn noktalarını kullan (karakter yönüne göre flip)
                Vector3 spawnPosition;
                Vector2 direction;
                
                if (i == 0) // Sağ üst çapraz (45°)
                {
                    if (isFacingLeft)
                    {
                        spawnPosition = fireballSpawnTopLeft != null ? fireballSpawnTopLeft.position : transform.position + new Vector3(-0.8f, 0.8f, 0);
                        direction = new Vector2(-1f, 1f).normalized;
                    }
                    else
                    {
                        spawnPosition = fireballSpawnTopRight != null ? fireballSpawnTopRight.position : transform.position + new Vector3(0.8f, 0.8f, 0);
                        direction = new Vector2(1f, 1f).normalized;
                    }
                }
                else if (i == 1) // Sol alt çapraz (225°)
                {
                    if (isFacingLeft)
                    {
                        spawnPosition = fireballSpawnBottomRight != null ? fireballSpawnBottomRight.position : transform.position + new Vector3(0.8f, -0.8f, 0);
                        direction = new Vector2(1f, -1f).normalized;
                    }
                    else
                    {
                        spawnPosition = fireballSpawnBottomLeft != null ? fireballSpawnBottomLeft.position : transform.position + new Vector3(-0.8f, -0.8f, 0);
                        direction = new Vector2(-1f, -1f).normalized;
                    }
                }
                else if (i == 2) // Sol üst çapraz (135°)
                {
                    if (isFacingLeft)
                    {
                        spawnPosition = fireballSpawnTopRight != null ? fireballSpawnTopRight.position : transform.position + new Vector3(0.8f, 0.8f, 0);
                        direction = new Vector2(1f, 1f).normalized;
                    }
                    else
                    {
                        spawnPosition = fireballSpawnTopLeft != null ? fireballSpawnTopLeft.position : transform.position + new Vector3(-0.8f, 0.8f, 0);
                        direction = new Vector2(-1f, 1f).normalized;
                    }
                }
                else // Sağ alt çapraz (315°)
                {
                    if (isFacingLeft)
                    {
                        spawnPosition = fireballSpawnBottomLeft != null ? fireballSpawnBottomLeft.position : transform.position + new Vector3(-0.8f, -0.8f, 0);
                        direction = new Vector2(-1f, -1f).normalized;
                    }
                    else
                    {
                        spawnPosition = fireballSpawnBottomRight != null ? fireballSpawnBottomRight.position : transform.position + new Vector3(0.8f, -0.8f, 0);
                        direction = new Vector2(1f, -1f).normalized;
                    }
                }
                
                // Alev topunu oluştur
                GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
                
                // Alev topunun sprite'ını hareket yönüne göre çevir
                SpriteRenderer fireballSprite = fireball.GetComponent<SpriteRenderer>();
                if (fireballSprite != null)
                {
                    // Karakter yönüne göre sprite'ı ayarla
                    if (i == 0) // İlk çapraz
                    {
                        if (isFacingLeft)
                        {
                            fireballSprite.flipX = true;
                            fireballSprite.flipY = false;
                        }
                        else
                        {
                            fireballSprite.flipX = false;
                            fireballSprite.flipY = false;
                        }
                    }
                    else if (i == 1) // İkinci çapraz
                    {
                        if (isFacingLeft)
                        {
                            fireballSprite.flipX = false;
                            fireballSprite.flipY = true;
                        }
                        else
                        {
                            fireballSprite.flipX = true;
                            fireballSprite.flipY = true;
                        }
                    }
                    else if (i == 2) // Üçüncü çapraz
                    {
                        if (isFacingLeft)
                        {
                            fireballSprite.flipX = false;
                            fireballSprite.flipY = false;
                        }
                        else
                        {
                            fireballSprite.flipX = true;
                            fireballSprite.flipY = false;
                        }
                    }
                    else // Dördüncü çapraz
                    {
                        if (isFacingLeft)
                        {
                            fireballSprite.flipX = true;
                            fireballSprite.flipY = true;
                        }
                        else
                        {
                            fireballSprite.flipX = false;
                            fireballSprite.flipY = true;
                        }
                    }
                }
                
                // Alev topuna hareket ve hasar bilgilerini ver
                Projectile projectile = fireball.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(direction, fireballSpeed, currentFireballDamage, fireballRange);
                }
                
                // Alev topunu belirli süre sonra yok et
                Destroy(fireball, 10f);
            }
        }
        
        lastFireballAttackTime = Time.time;
    }
    
    void CheckForEnemies(float range, int damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, range);
        
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.TakeDamage(damage);
                }
            }
        }
    }
    
    void UpdateAttackPointPosition()
    {
        if (attackPoint == null) return;
        
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            // Sprite flip'e göre attack point pozisyonunu ayarla
            if (playerSprite.flipX)
            {
                // Sola bakıyorsa, sol tarafta kılıç ucu
                attackPoint.localPosition = new Vector3(-4.5f, 0.2f, 0);
            }
            else
            {
                // Sağa bakıyorsa, sağ tarafta kılıç ucu
                attackPoint.localPosition = new Vector3(4.5f, 0.2f, 0);
            }
        }
    }
    
    // Saldırı menzilini görselleştirmek için (debug)
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Kılıç menzili (sabit 2f)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, swordRange);
            
            // Alev topu menzili
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fireballRange);
        }
    }

    // Skill upgrade metodları - Kılıç
    public void IncreaseSwordSpeed(float amount)
    {
        currentSwordCooldown = Mathf.Max(0.1f, currentSwordCooldown - amount); // Mevcut değerden çıkar
        Debug.Log($"Sword Cooldown reduced to: {currentSwordCooldown}");
    }

    public void IncreaseSwordDamage(int amount)
    {
        currentSwordDamage = amount; // Skill bonusunu direkt ata (base 0'dan başlıyor)
        Debug.Log($"Sword Damage set to: {currentSwordDamage}");
    }
    
    // Skill upgrade metodları - Alev Topu
    public void IncreaseFireballSpeed(float amount)
    {
        currentFireballCooldown = Mathf.Max(0.3f, currentFireballCooldown - amount); // Mevcut değerden çıkar
        Debug.Log($"Fireball Cooldown reduced to: {currentFireballCooldown}");
    }

    public void IncreaseFireballDamage(int amount)
    {
        currentFireballDamage = amount; // Skill bonusunu direkt ata (base 15 dahil)
        Debug.Log($"Fireball Damage set to: {currentFireballDamage}");
    }

    public void IncreaseFireballRange(float amount)
    {
        fireballRange += amount;
        Debug.Log($"Fireball Range increased to: {fireballRange}");
    }
    
    public void IncreaseFireballCount(int amount)
    {
        currentFireballCount = baseFireballCount + amount; // Base + skill bonusu
        Debug.Log($"Fireball Count set to: {currentFireballCount}");
    }
    
    // Fireball'ı aktif et (Level 4'te çağrılacak)
    public void ActivateFireball()
    {
        isFireballActive = true;
        Debug.Log("🔥 FIREBALL SYSTEM ACTIVATED! 🔥");
    }
} 
 