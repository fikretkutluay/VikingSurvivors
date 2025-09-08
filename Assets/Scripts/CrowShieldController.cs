using UnityEngine;

public class CrowShieldController : MonoBehaviour
{
    [Header("Crow Shield Settings")]
    [SerializeField] private float rotationSpeed = 360f; // Z rotasyonu
    [SerializeField] private int damage = 30; // Base damage (Lv7'de açılır)
    [SerializeField] private float knockbackForce = 6f; // Küçük itme
    [SerializeField] private float hitCooldown = 0.35f; // Global vuruş periyodu
    [SerializeField] private float flickerFrequency = 12f; // Cooldown sırasında yanıp sönme hızı
    [SerializeField] private int sortingOrder = 10; // Görünürlük için
    
    private Transform player;
    private bool isActive = false;
    private float baseScale = 0.75f;
    private float currentScale;
    
    // Cooldown & görsel
    private float nextHitAllowedTime = 0f;
    private bool isOnCooldown = false;
    private SpriteRenderer spriteRenderer;
    private System.Collections.Generic.HashSet<Collider2D> enemiesInside = new System.Collections.Generic.HashSet<Collider2D>();
    
    void Start()
    {
        // Player'ı bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Başlangıç scale'i ayarla
        currentScale = baseScale;
        transform.localScale = Vector3.one * currentScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
        
        // Başlangıçta pasif; SkillManager üzerinden Lv7'de açılacak
        isActive = false;
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (isActive && player != null)
        {
            // Player'ı takip et
            transform.position = player.position;
            
            // Z ekseni etrafında sürekli döndür
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Cooldown flicker
            if (isOnCooldown)
            {
                if (Time.time >= nextHitAllowedTime)
                {
                    isOnCooldown = false;
                    if (spriteRenderer != null)
                    {
                        var c = spriteRenderer.color; c.a = 1f; spriteRenderer.color = c;
                    }
                }
                else if (spriteRenderer != null)
                {
                    float alpha = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(Time.time * flickerFrequency));
                    var c = spriteRenderer.color; c.a = alpha; spriteRenderer.color = c;
                }
            }

            // Cooldown bittiğinde içerideki tüm düşmanlara aynı anda hasar uygula
            if (!isOnCooldown && Time.time >= nextHitAllowedTime && enemiesInside.Count > 0)
            {
                DealDamageToAllInside();
            }
        }
    }
    
    
    // Düşman kalkan içindeyken sürekli it
    void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Enemy"))
        {
            // Düşmanı sürekli it
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Düşmanı oyuncudan uzaklaştır
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
    
    // Skill sistemi için metodlar
    public void ActivateShield()
    {
        isActive = true;
        gameObject.SetActive(true);
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            var c = spriteRenderer.color; c.a = 1f; spriteRenderer.color = c;
            spriteRenderer.sortingOrder = sortingOrder;
        }
        // Cooldown resetle
        isOnCooldown = false;
        nextHitAllowedTime = 0f;
    }
    
    public void DeactivateShield()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
    
    public void UpgradeShield(int level)
    {
        // Scale hesapla: 0.75 + (level * 0.1)
        currentScale = baseScale + (level * 0.1f);
        transform.localScale = Vector3.one * currentScale;
        
        // Maksimum 5 level (1.25 scale)
        if (level >= 5)
        {
            currentScale = 1.25f;
            transform.localScale = Vector3.one * currentScale;
        }
    }
    
    // Debug için görselleştirme
    void OnDrawGizmosSelected()
    {
        if (isActive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, currentScale * 1f);
        }
    }
    
    // Kalkan alanına bir düşman girdiğinde set'e ekle ve uygunsa toplu hasar ver
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Enemy")) return;

        enemiesInside.Add(other);

        // Eğer cooldown bitmişse anında içerideki herkese uygula
        if (!isOnCooldown && Time.time >= nextHitAllowedTime)
        {
            DealDamageToAllInside();
        }
        
        // İlk temas knockback
        Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInside.Remove(other);
        }
    }

    private void DealDamageToAllInside()
    {
        foreach (var col in enemiesInside)
        {
            if (col == null) continue;
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.TakeDamage(damage);
            }
        }
        nextHitAllowedTime = Time.time + hitCooldown;
        isOnCooldown = true;
    }
}
