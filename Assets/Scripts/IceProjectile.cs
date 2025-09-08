using UnityEngine;

public class IceProjectile : MonoBehaviour
{
    [Header("Ice Projectile Settings")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float slowEffectDuration = 3f; // Uzun süre
    [SerializeField] private float slowEffectStrength = 0.75f; // Oyuncu hızını %25 azaltır (0.75 = %75 hız)
    
    private bool hasHitPlayer = false;
    private Transform target;
    private float speed;
    private Rigidbody2D rb;
    
    public void Initialize(Transform player, float projectileSpeed)
    {
        target = player;
        speed = projectileSpeed;
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        if (target == null || rb == null) return;
        
        // Başlangıçta oyuncuya doğru yön hesapla
        Vector2 direction = (target.position - transform.position).normalized;
        
        // Hızı ayarla (sadece başlangıçta)
        rb.velocity = direction * speed;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer) return;
        
        if (other.CompareTag("Player"))
        {
            hasHitPlayer = true;
            
            // Oyuncuya hasar ver
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            
            // Yavaşlatma ve buz efekti uygula
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplySlowEffect(slowEffectDuration, slowEffectStrength);
                playerMovement.ApplyIceEffect(slowEffectDuration);
            }
            
            // Buz küresini yok et
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Duvara çarptığında yok ol
            Destroy(gameObject);
        }
    }
    
    

}
