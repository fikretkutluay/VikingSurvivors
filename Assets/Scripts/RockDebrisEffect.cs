using UnityEngine;

public class RockDebrisEffect : MonoBehaviour
{
    [Header("Debris Settings")]
    [SerializeField] private Sprite rockSprite; // Kaya parçası sprite'ı
    [SerializeField] private int debrisCount = 12; // Kaç parça oluşturulacak
    [SerializeField] private float explosionForce = 8f; // Patlama gücü
    [SerializeField] private float explosionRadius = 3.5f; // Patlama yarıçapı
    [SerializeField] private float minLifetime = 2f; // Minimum yaşam süresi
    [SerializeField] private float maxLifetime = 4f; // Maksimum yaşam süresi
    [SerializeField] private float minSize = 0.3f; // Minimum boyut
    [SerializeField] private float maxSize = 0.8f; // Maksimum boyut
    
    void Start()
    {
        CreateRockDebris();
    }
    
    void CreateRockDebris()
    {
        for (int i = 0; i < debrisCount; i++)
        {
            // Kaya parçası oluştur
            GameObject debris = new GameObject($"RockDebris_{i}");
            debris.transform.SetParent(transform);
            
            // SpriteRenderer ekle
            SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
            sr.sprite = rockSprite;
            sr.sortingOrder = 1; // Düşmanların üstünde görünsün
            
            // Rastgele boyut
            float randomSize = Random.Range(minSize, maxSize);
            debris.transform.localScale = Vector3.one * randomSize;
            
            // Rastgele pozisyon (patlama merkezinde)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 startPos = transform.position + (Vector3)randomDirection * 0.2f;
            debris.transform.position = startPos;
            
            // Rigidbody2D ekle (fizik için)
            Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
            rb.drag = 0.5f; // Hava direnci
            
            // Rastgele patlama yönü ve gücü
            Vector2 explosionDirection = randomDirection;
            float explosionPower = Random.Range(explosionForce * 0.7f, explosionForce * 1.3f);
            rb.AddForce(explosionDirection * explosionPower, ForceMode2D.Impulse);
            
            // Rastgele dönme
            float randomTorque = Random.Range(-200f, 200f);
            rb.AddTorque(randomTorque);
            
            // Yaşam süresi
            float lifetime = Random.Range(minLifetime, maxLifetime);
            Destroy(debris, lifetime);
        }
        
        // Bu obje de kendini yok et (parçacıklar ayrı yok olacak)
        Destroy(gameObject, maxLifetime + 1f);
    }
    
    // Debug için patlama alanını göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
