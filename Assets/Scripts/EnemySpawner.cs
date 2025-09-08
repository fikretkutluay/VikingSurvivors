using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public string enemyName;
        [Range(0f, 1f)] public float spawnChance = 0.5f; // 0-1 arası spawn şansı
        public float minSpawnTime = 0f; // Bu düşmanın spawn olmaya başlayacağı minimum süre
        [Min(0)] public int maxConcurrent = 999; // Bu türden aynı anda sahnede bulunabilecek maksimum adet
    }
    
    [Header("Spawn Settings")]
    [SerializeField] private List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] private float spawnRate = 1f; // Her 1 saniyede bir düşman
    [SerializeField] private float spawnRadius = 10f; // Oyuncudan 10 birim uzakta spawn
    [SerializeField] private int maxEnemies = 20; // Maksimum düşman sayısı
    
    [Header("References")]
    [SerializeField] private Transform player;
    
    private float lastSpawnTime;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float gameStartTime;
    
    void Start()
    {
        gameStartTime = Time.time;
        
        // Player'ı otomatik bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    void Update()
    {
        // Aktif düşmanları temizle (ölmüş olanları listeden çıkar)
        CleanupDeadEnemies();
        
        // Spawn kontrolü
        if (CanSpawnEnemy())
        {
            SpawnRandomEnemy();
        }
    }
    
    bool CanSpawnEnemy()
    {
        return Time.time >= lastSpawnTime + spawnRate && activeEnemies.Count < maxEnemies;
    }
    
    void SpawnRandomEnemy()
    {
        if (player == null || enemyTypes.Count == 0) return;
        
        // Spawn edilebilecek düşmanları filtrele
        List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();
        float currentGameTime = Time.time - gameStartTime;
        
        foreach (EnemySpawnData enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null && currentGameTime >= enemyData.minSpawnTime)
            {
                // Mevcut sayıyı kontrol et, limiti aşma
                int currentCount = GetActiveCountForName(enemyData.enemyName);
                if (currentCount < enemyData.maxConcurrent)
                {
                    availableEnemies.Add(enemyData);
                }
            }
        }
        
        if (availableEnemies.Count == 0) return;
        
        // Rastgele düşman seç
        EnemySpawnData selectedEnemy = GetRandomEnemy(availableEnemies);
        
        // Rastgele spawn pozisyonu hesapla (oyuncunun etrafında)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = player.position + new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;
        
        // Düşmanı oluştur
        GameObject enemy = Instantiate(selectedEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
        // Meta ekle ve isim ata
        var meta = enemy.GetComponent<SpawnedEnemyMeta>();
        if (meta == null) meta = enemy.AddComponent<SpawnedEnemyMeta>();
        meta.enemyName = selectedEnemy.enemyName;
        
        // Aktif düşmanlar listesine ekle
        activeEnemies.Add(enemy);
        
        // Spawn zamanını güncelle
        lastSpawnTime = Time.time;
        
        Debug.Log($"{selectedEnemy.enemyName} spawned at {spawnPosition}");
    }
    
    EnemySpawnData GetRandomEnemy(List<EnemySpawnData> availableEnemies)
    {
        // Toplam spawn şansını hesapla
        float totalChance = 0f;
        foreach (EnemySpawnData enemy in availableEnemies)
        {
            totalChance += enemy.spawnChance;
        }
        
        // Rastgele sayı üret
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;
        
        // Hangi düşmanın seçileceğini belirle
        foreach (EnemySpawnData enemy in availableEnemies)
        {
            currentChance += enemy.spawnChance;
            if (randomValue <= currentChance)
            {
                return enemy;
            }
        }
        
        // Fallback - ilk düşmanı döndür
        return availableEnemies[0];
    }
    
    void CleanupDeadEnemies()
    {
        // Ölü düşmanları listeden çıkar
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    int GetActiveCountForName(string name)
    {
        int count = 0;
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            GameObject obj = activeEnemies[i];
            if (obj == null) continue;
            // Önce meta üzerinden isim eşleştir
            var meta = obj.GetComponent<SpawnedEnemyMeta>();
            if (meta != null && meta.enemyName == name)
            {
                var ai = obj.GetComponent<EnemyAI>();
                if (ai != null && !ai.IsDead()) count++;
                continue;
            }

            // Fallback: tip adına göre karşılaştır (eski davranış)
            EnemyAI fallbackAi = obj.GetComponent<EnemyAI>();
            if (fallbackAi != null && !fallbackAi.IsDead())
            {
                string typeName = fallbackAi.GetType().Name;
                if (typeName == name) count++;
            }
        }
        return count;
    }
    
    // Spawn alanını görselleştirmek için (debug)
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, spawnRadius);
        }
    }
} 