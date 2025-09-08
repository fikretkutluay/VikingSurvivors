using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.XP;
    [SerializeField] private int xpValue = 10;


    

    
    private Transform player;

    private bool isCollected = false;
    private SpriteRenderer spriteRenderer;
    
    public enum ItemType
    {
        XP,
        Health,
        PowerUp
    }
    
    void Start()
    {
        // Player'ı bul
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // XP item'ları için tag ayarla
        if (itemType == ItemType.XP)
        {
            gameObject.tag = "XPItem";
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Player'a yakınsa topla
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer < 0.5f)
            {
                CollectItem();
            }
        }
    }
    
    void CollectItem()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Item tipine göre farklı efektler
        switch (itemType)
        {
            case ItemType.XP:
                GiveXPToPlayer();
                break;
                
            case ItemType.Health:
                GiveHealthToPlayer();
                break;
                
            case ItemType.PowerUp:
                GivePowerUpToPlayer();
                break;
        }
        
        // Toplama efekti
        StartCoroutine(CollectEffect());
    }
    
    private void GiveXPToPlayer()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.GainXP(xpValue);
                Debug.Log($"Collected XP item! Gained {xpValue} XP");
            }
        }
    }
    
    private void GiveHealthToPlayer()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(20);
                Debug.Log("Collected Health item! Healed 20 HP");
            }
        }
    }
    
    private void GivePowerUpToPlayer()
    {
        // Gelecekte power-up sistemi eklenecek
        Debug.Log("Collected Power-Up item!");
    }
    
    private System.Collections.IEnumerator CollectEffect()
    {
        // Basit kaybolma efekti
        yield return new WaitForSeconds(0.1f);
        
        // Item'ı yok et
        Destroy(gameObject);
    }
    

    
    // Public metodlar
    public void SetXPValue(int value)
    {
        xpValue = value;
    }
    
    public void SetItemType(ItemType type)
    {
        itemType = type;
    }
} 