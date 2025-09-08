using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int damage = 5;
    [SerializeField] protected int xpReward = 10; // XP ödülü (artık kullanılmıyor)
    
    [Header("Item Drop Settings")]
    [SerializeField] protected GameObject xpItemPrefab;
    [SerializeField] protected float itemDropChance = 0.3f; // %30 şans
    [SerializeField] protected int itemXPValue = 5; // Item'dan alınacak XP
    [Space]
    [SerializeField] protected GameObject healthItemPrefab;
    [SerializeField] protected float healthDropChance = 0.1f; // %10 şans
    [SerializeField] protected int healthValue = 20; // Health item ile iyileşme
    
    [Header("References")]
    [SerializeField] protected Transform player;
    
    protected SpriteRenderer spriteRenderer;
    protected bool isDead = false;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Player'ı otomatik bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Alt sınıfların kendi başlangıç ayarlarını yapmasına izin ver
        OnEnemyStart();
    }
    
    protected virtual void Update()
    {
        if (isDead || player == null) return;
        
        // Alt sınıfların özel hareket mantığını kullanmasına izin ver
        OnEnemyUpdate();
        
        // Sprite yönünü ayarla
        UpdateSpriteDirection();
    }
    
    // Alt sınıfların override edebileceği metodlar
    protected virtual void OnEnemyStart()
    {
        // Base implementation - boş
    }
    
    protected virtual void OnEnemyUpdate()
    {
        // Base implementation - basit hareket
        MoveTowardsPlayer();
    }
    
    protected virtual void MoveTowardsPlayer()
    {
        // Spawn olur olmaz direkt oyuncuya doğru git
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }
    
    protected virtual void UpdateSpriteDirection()
    {
        if (player != null)
        {
            // Player'ın hangi tarafında olduğuna göre sprite'ı çevir
            if (transform.position.x < player.position.x)
            {
                spriteRenderer.flipX = false; // Sağa bak
            }
            else
            {
                spriteRenderer.flipX = true; // Sola bak
            }
        }
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Hasar efekti (kırmızı flash)
        StartCoroutine(DamageFlash());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual System.Collections.IEnumerator DamageFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    protected virtual void Die()
    {
        isDead = true;
        
        // Sadece item drop et
        DropItems();
        
        // Düşmanı yok et
        Destroy(gameObject, 0.5f);
    }
    
    private void DropItems()
    {
        // Tek bir zar at, aynı anda iki item düşmesin
        float roll = Random.value; // 0..1

        float xpThreshold = Mathf.Clamp01(itemDropChance);
        float healthThreshold = Mathf.Clamp01(itemDropChance + healthDropChance);

        if (xpItemPrefab != null && roll <= xpThreshold)
        {
            DropXPItem();
        }
        else if (healthItemPrefab != null && roll <= healthThreshold)
        {
            DropHealthItem();
        }
        // Aksi halde drop yok
    }

    void DropXPItem()
    {
        Vector3 dropPosition = transform.position + Random.insideUnitSphere * 0.5f;
        GameObject item = Instantiate(xpItemPrefab, dropPosition, Quaternion.identity);
        ItemDrop itemDrop = item.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            itemDrop.SetXPValue(itemXPValue);
            itemDrop.SetItemType(ItemDrop.ItemType.XP);
        }
        Debug.Log($"{GetEnemyName()} dropped XP item!");
    }

    void DropHealthItem()
    {
        Vector3 dropPosition = transform.position + Random.insideUnitSphere * 0.5f;
        GameObject item = Instantiate(healthItemPrefab, dropPosition, Quaternion.identity);
        ItemDrop itemDrop = item.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            itemDrop.SetXPValue(healthValue); // Health miktarı
            itemDrop.SetItemType(ItemDrop.ItemType.Health);
        }
        Debug.Log($"{GetEnemyName()} dropped Health item!");
    }
    
    // Player ile çarpışma kontrolü
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            OnPlayerHit(other);
        }
    }
    
    protected virtual void OnPlayerHit(Collider2D player)
    {
        // Player'a hasar ver
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"{GetEnemyName()} hit player for {damage} damage!");
        }
    }
    
    // Alt sınıfların override edebileceği metodlar
    protected virtual string GetEnemyName()
    {
        return "Enemy";
    }
    
    // Getter metodları
    public virtual int GetDamage() => damage;
    public virtual float GetMoveSpeed() => moveSpeed;
    public virtual int GetCurrentHealth() => currentHealth;
    public virtual int GetMaxHealth() => maxHealth;
    public virtual bool IsDead() => isDead;
    public virtual int GetXPReward() => xpReward;
} 