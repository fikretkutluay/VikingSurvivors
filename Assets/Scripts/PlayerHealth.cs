using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    [Header("XP & Level Settings")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int previousLevel = 1;
    [SerializeField] private int baseXPRequired = 30; // İlk level için 30 XP
    [SerializeField] private float xpMultiplier = 1.35f; // Her level için 1.35x artış
    [SerializeField] private int xpToNextLevel = 30; // Başlangıç değeri
    
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image xpBarFill;
    [SerializeField] private Text levelText;
    [SerializeField] private Text healthText;
    [SerializeField] private Text xpText;
    
    [Header("Damage Settings")]
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Level Up UI")]
    [SerializeField] private LevelUpUI levelUpUI;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvincible = false;
    private bool isDead = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        if (levelUpUI == null) levelUpUI = FindObjectOfType<LevelUpUI>(includeInactive:true);
        
        // İlk level için XP gereksinimini hesapla
        xpToNextLevel = CalculateXPRequired(currentLevel);
        
        UpdateUI();
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Hasar efekti
        StartCoroutine(DamageFlash());
        
        // Invincibility
        StartCoroutine(InvincibilityFrames());
        
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        UpdateUI();
    }
    
    public void GainXP(int xpAmount)
    {
        if (isDead) return;
        
        currentXP += xpAmount;
        
        // Level up kontrolü
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
    }
    
    private void LevelUp()
    {
        previousLevel = currentLevel;
        currentXP -= xpToNextLevel;
        currentLevel++;
        
        // Level up bonusları
        maxHealth += 10;
        // currentHealth = maxHealth; // Artık level up'ta can dolmuyor
        
        // Level 4'te Fireball skills unlock
        if (currentLevel == 4)
        {
            Debug.Log("🔥 FIREBALL SKILLS UNLOCKED! 🔥");
            Debug.Log("Fireball Power, Fireball Speed, Fireball Count artık seçeneklerde çıkacak!");
            
            // Fireball sistemini aktif et
            var attackSystem = GetComponent<AttackSystem>();
            if (attackSystem != null)
            {
                attackSystem.ActivateFireball();
                Debug.Log("Fireball attack system aktif edildi!");
            }
            else
            {
                Debug.LogWarning("AttackSystem component bulunamadı!");
            }
        }
        
        // Level 5'te Magnet otomatik aktif
        if (currentLevel == 5)
        {
            Debug.Log("🧲 MAGNET ACTIVATED! 🧲");
            Debug.Log("Magnet otomatik olarak aktif edildi!");
            
            // Magnet'i otomatik aktif et
            var magnetAttractor = GetComponent<MagnetAttractor>();
            if (magnetAttractor != null)
            {
                magnetAttractor.SetActive(true);
                Debug.Log("Magnet component aktif edildi!");
            }
            else
            {
                Debug.LogWarning("MagnetAttractor component bulunamadı!");
            }
        }
        
        // Sonraki level için XP gereksinimi hesapla
        xpToNextLevel = CalculateXPRequired(currentLevel + 1);
        
        // Level-up seçim ekranını aç
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpOptions();
        }
        
        UpdateUI();
    }

    // --- Debug helpers ---
    [ContextMenu("Set Level 5 and Open LevelUpUI")]
    public void Debug_OpenLevelUpAt5()
    {
        currentLevel = 5;
        previousLevel = 4;
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpOptions();
        }
        else
        {
            Debug.LogWarning("LevelUpUI reference missing on PlayerHealth.");
        }
    }
    
    
    private System.Collections.IEnumerator DamageFlash()
    {
        // Kısa bir bekleme (buz efekti başlaması için)
        yield return new WaitForSeconds(0.01f);
        
        // Eğer buz efekti aktifse, hasar efektini atla
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null && playerMovement.IsIceEffectActive())
        {
            yield break;
        }
        
        spriteRenderer.color = Color.red; // Changed from damageColor to Color.red
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        
        // Yanıp sönen efekt
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    
    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");
    }
    
    private void UpdateUI()
    {
        // Health Bar (Image fill)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        // XP Bar (Image fill)
        if (xpBarFill != null)
        {
            xpBarFill.fillAmount = (float)currentXP / xpToNextLevel;
        }
        // Text'ler
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        if (xpText != null)
        {
            xpText.text = $"{currentXP}/{xpToNextLevel} XP";
        }
    }
    
    // Modifiers for skills
    public void AddMaxHealth(int delta)
    {
        maxHealth = Mathf.Max(1, maxHealth + delta);
        UpdateUI();
    }
    
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Ensure current health doesn't exceed new max
        UpdateUI();
        Debug.Log($"Max Health increased to: {maxHealth}");
    }
    
    // Getter metodları
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentXP() => currentXP;
    public int GetCurrentLevel() => currentLevel;
    
    // Level için gerekli XP'yi hesapla (exponential growth)
    private int CalculateXPRequired(int level)
    {
        if (level <= 1) return baseXPRequired;
        
        // Formula: baseXP * (multiplier ^ (level - 1))
        float xpRequired = baseXPRequired * Mathf.Pow(xpMultiplier, level - 1);
        return Mathf.RoundToInt(xpRequired);
    }
    public int GetPreviousLevel() => previousLevel;
    public bool IsDead() => isDead;
    public bool IsInvincible() => isInvincible;
    
    // Test metodları (debug için)
    [ContextMenu("Take 20 Damage")]
    public void TestTakeDamage()
    {
        TakeDamage(20);
    }
    
    [ContextMenu("Heal 20 HP")]
    public void TestHeal()
    {
        Heal(20);
    }
    
    [ContextMenu("Gain 50 XP")]
    public void TestGainXP()
    {
        GainXP(50);
    }
    
    [ContextMenu("Show XP Requirements")]
    public void Debug_ShowXPRequirements()
    {
        Debug.Log("=== XP Requirements ===");
        for (int i = 1; i <= 10; i++)
        {
            int xpReq = CalculateXPRequired(i);
            Debug.Log($"Level {i}: {xpReq} XP required");
        }
    }
} 