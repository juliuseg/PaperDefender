using UnityEngine;

public class WormHoleController : MonoBehaviour
{
    [Header("Wormhole Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    public Color wormholeColor = new Color(0.5f, 0.2f, 1f, 0.8f); // Purple with transparency

    public float rotationSpeed = 30f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private float pulseTimer = 0f;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = wormholeColor;
        }
        
        originalScale = transform.localScale;
        
        Debug.Log($"Wormhole spawned at position: {transform.position}");
    }
    
    void Update()
    {
        // Pulse animation
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
        transform.localScale = originalScale * pulse;
        
        // Rotate slowly
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Called when the wormhole is destroyed (e.g., when player dies or game resets)
    /// </summary>
    public void DestroyWormhole()
    {
        Debug.Log("Wormhole destroyed");
        Destroy(gameObject);
    }
} 