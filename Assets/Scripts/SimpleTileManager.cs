using UnityEngine;

public class SimpleTileManager : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private Sprite[] tileSprites; // 5 farklı tile sprite'ı
    [SerializeField] private int renderRadius = 3; // Oyuncunun etrafında kaç tile render edilecek (3 = 7x7)
    [SerializeField] private float updateThreshold = 0.5f; // Oyuncu ne kadar hareket edince güncelle
    [SerializeField] private int tileSize = 6; // Tile boyutu (6 birim aralıklarla)
    [SerializeField] private float tileScale = 1.1f; // Tile'ları biraz büyüt (çizgileri kapatmak için)
    
    [Header("References")]
    [SerializeField] private Transform player;
    
    private Vector2Int lastPlayerTile = Vector2Int.zero;
    
    void Start()
    {
        // Player'ı otomatik bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // İlk tile'ları oluştur
        UpdateTiles();
    }
    
    void Update()
    {
        // Oyuncu yeterince hareket ettiyse güncelle
        if (player != null)
        {
            float distance = Vector2.Distance(player.position, new Vector2(lastPlayerTile.x * tileSize, lastPlayerTile.y * tileSize));
            if (distance > updateThreshold)
            {
                Vector2Int currentPlayerTile = GetPlayerTile();
                UpdateTiles();
                lastPlayerTile = currentPlayerTile;
            }
        }
    }
    
    Vector2Int GetPlayerTile()
    {
        if (player == null) return Vector2Int.zero;
        
        // 6 birim aralıklarla tile pozisyonu hesapla
        int tileX = Mathf.FloorToInt(player.position.x / tileSize);
        int tileY = Mathf.FloorToInt(player.position.y / tileSize);
        
        return new Vector2Int(tileX, tileY);
    }
    
    void UpdateTiles()
    {
        Vector2Int playerTile = GetPlayerTile();
        
        // Uzaktaki tile'ları temizle
        CleanupDistantTiles(playerTile);
        
        // Sadece eksik tile'ları oluştur
        for (int x = playerTile.x - renderRadius; x <= playerTile.x + renderRadius; x++)
        {
            for (int y = playerTile.y - renderRadius; y <= playerTile.y + renderRadius; y++)
            {
                // Sadece daha önce oluşturulmamış tile'ları oluştur
                if (!TileExists(x, y))
                {
                    CreateTile(x, y);
                }
            }
        }
    }
    
    bool TileExists(int x, int y)
    {
        // Bu pozisyonda tile var mı kontrol et
        Transform existingTile = transform.Find($"Tile_{x}_{y}");
        return existingTile != null;
    }
    
    void CleanupDistantTiles(Vector2Int playerTile)
    {
        // Tüm tile'ları kontrol et ve uzaktaki olanları sil
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Tile_"))
            {
                // Tile pozisyonunu parse et
                string[] parts = child.name.Split('_');
                if (parts.Length >= 3 && int.TryParse(parts[1], out int tileX) && int.TryParse(parts[2], out int tileY))
                {
                    // Oyuncudan uzak mı kontrol et
                    int distanceX = Mathf.Abs(tileX - playerTile.x);
                    int distanceY = Mathf.Abs(tileY - playerTile.y);
                    
                    if (distanceX > renderRadius || distanceY > renderRadius)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
        }
    }
    
    void ClearTiles()
    {
        // Tüm tile'ları sil (tag yerine parent kontrolü)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name.StartsWith("Tile_"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    void CreateTile(int x, int y)
    {
        // Tile'ı oluştur
        GameObject tile = new GameObject($"Tile_{x}_{y}");
        tile.transform.SetParent(transform);
        
        // SpriteRenderer ekle
        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        
        // Rastgele sprite seç
        if (tileSprites != null && tileSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, tileSprites.Length);
            sr.sprite = tileSprites[randomIndex];
        }
        
        // Tile pozisyonu (6 birim aralıklarla)
        tile.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
        
        // Tile'ı biraz büyüt (çizgileri kapatmak için)
        tile.transform.localScale = Vector3.one * tileScale;
    }
    
    // Debug için render alanını görselleştir
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        Vector2Int playerTile = GetPlayerTile();
        
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(playerTile.x * tileSize, playerTile.y * tileSize, 0);
        Vector3 size = new Vector3((renderRadius * 2 + 1) * tileSize, (renderRadius * 2 + 1) * tileSize, 0);
        Gizmos.DrawWireCube(center, size);
    }
}
