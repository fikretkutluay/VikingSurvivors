using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float lifetime = 10f;
    
    private Vector2 direction;
    private Vector3 startPosition;
    private bool isInitialized = false;
    private int actualDamage; // Gerçek hasar değeri
    private float actualSpeed; // Gerçek hız değeri
    private float actualRange; // Gerçek menzil değeri
    
    void Start()
    {
        startPosition = transform.position;
        
        // Belirli süre sonra otomatik yok ol
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Hareket
        Vector3 movement = direction * actualSpeed * Time.deltaTime;
        transform.Translate(movement);
        
        // Maksimum mesafeyi kontrol et
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= actualRange)
        {
            Destroy(gameObject);
        }
    }
    
    public void Initialize(Vector2 dir, float speed, int dmg, float range)
    {
        direction = dir.normalized;
        actualSpeed = speed;
        actualDamage = dmg;
        actualRange = range;
        isInitialized = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Düşmana hasar ver
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(actualDamage);
            }
            
            // Alev topunu yok et
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // Duvara çarpınca yok ol
            Destroy(gameObject);
        }
    }
    
    // Backup collision detection - eğer trigger çalışmıyorsa
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemyAI = collision.gameObject.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(actualDamage);
            }
            
            // Alev topunu yok et
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            // Duvara çarpınca yok ol
            Destroy(gameObject);
        }
    }
    
    // Debug için menzili görselleştir
    void OnDrawGizmosSelected()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(startPosition, actualRange);
            
            // Hareket yönünü göster
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
