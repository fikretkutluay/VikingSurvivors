using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    

    
    private PlayerInputActions playerInputActions;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    
    // Ice effect management
    private float currentSlowMultiplier = 1f;
    private float slowEffectEndTime = 0f;
    private Coroutine slowEffectCoroutine;
    private float baseMoveSpeed;
    private bool isIceEffectActive = false;
    
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        baseMoveSpeed = moveSpeed; // Store original speed
    }
    
    void OnEnable()
    {
        playerInputActions.Enable();
        playerInputActions.Newactionmap.Move.performed += OnMove;
        playerInputActions.Newactionmap.Move.canceled += OnMove;
    }
    
    void OnDisable()
    {
        playerInputActions.Disable();
        playerInputActions.Newactionmap.Move.performed -= OnMove;
        playerInputActions.Newactionmap.Move.canceled -= OnMove;
    }
    
    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    void Update()
    {
        // WASD hareketi
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
        
        // Sprite flip - hareket yönüne göre
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
        

    }

    // ---- modifiers for skills ----
    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;
        Debug.Log($"Move Speed increased to: {moveSpeed}");
    }
    
    // ---- temporary effects ----
    public void SetTemporarySpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public float GetCurrentSpeed()
    {
        return moveSpeed;
    }
    
    // ---- ice effect ----
    public void ApplyIceEffect(float duration)
    {
        StartCoroutine(IceEffectCoroutine(duration));
    }
    
    // Public getter for ice effect status
    public bool IsIceEffectActive()
    {
        return isIceEffectActive;
    }
    
    // New ice slow effect with override system
    public void ApplySlowEffect(float duration, float slowStrength)
    {
        // Override: Only apply if new effect is stronger or current effect expired
        if (slowStrength < currentSlowMultiplier || Time.time > slowEffectEndTime)
        {
            currentSlowMultiplier = slowStrength;
            slowEffectEndTime = Time.time + duration;
            
            // Stop previous coroutine if running
            if (slowEffectCoroutine != null)
            {
                StopCoroutine(slowEffectCoroutine);
            }
            
            slowEffectCoroutine = StartCoroutine(UpdateSlowEffect());
        }
    }
    
    private System.Collections.IEnumerator UpdateSlowEffect()
    {
        while (Time.time < slowEffectEndTime)
        {
            float effectiveSpeed = baseMoveSpeed * currentSlowMultiplier;
            moveSpeed = effectiveSpeed;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Effect ended, restore normal speed
        moveSpeed = baseMoveSpeed;
        currentSlowMultiplier = 1f;
        slowEffectCoroutine = null;
    }
    
    private System.Collections.IEnumerator IceEffectCoroutine(float duration)
    {
        isIceEffectActive = true; // Buz efekti başladı
        Color originalColor = spriteRenderer.color;
        Color iceColor = new Color(0.7f, 0.9f, 1f, 1f); // Açık mavi
        
        // Donma animasyonu (yanıp sönen efekt)
        float flashDuration = 0.2f;
        int flashCount = 0;
        int maxFlashes = (int)(duration / flashDuration);
        
        while (flashCount < maxFlashes)
        {
            // Buz rengi
            spriteRenderer.color = iceColor;
            yield return new WaitForSeconds(flashDuration);
            
            // Hafif şeffaf buz rengi
            spriteRenderer.color = new Color(iceColor.r, iceColor.g, iceColor.b, 0.5f);
            yield return new WaitForSeconds(flashDuration);
            
            flashCount++;
        }
        
        // Buz efekti bittiğinde flag'i kapat
        isIceEffectActive = false;
        
        // Kısa bir bekleme (hasar efekti varsa bitsin)
        yield return new WaitForSeconds(0.2f);
        
        // Son olarak orijinal rengi garanti et
        spriteRenderer.color = originalColor;
    }
    

}
