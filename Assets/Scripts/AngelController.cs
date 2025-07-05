using UnityEngine;

public class AngelController : MonoBehaviour
{
    [Header("Angel Settings")]
    public int maxLives = 3;
    public float fadeOutTime = 0.5f;
    public Color angelColor;
    public Color flashColor;
    private int currentLives;
    private SpriteRenderer spriteRenderer;
    private bool isDestroying = false;
    
    void Start()
    {
        currentLives = maxLives;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = angelColor;
        }
        
        Debug.Log($"Angel shield spawned with {currentLives} lives");
    }
    
    /// <summary>
    /// Called when the angel shield is hit by an enemy projectile
    /// Returns true if the hit was blocked by the angel
    /// </summary>
    public bool Hit(Vector2 hitWorldPos, string tagToHit)
    {
        if (isDestroying) return false;
        
        // Only block projectiles targeting the player
        if (tagToHit == "Player")
        {
            currentLives--;
            Debug.Log($"Angel shield hit! Lives remaining: {currentLives}");
            
            // Award points to player
            PointSystem ps = GameObject.FindGameObjectWithTag("GameController").GetComponent<PointSystem>();
            if (ps != null)
            {
                ps.AddPoints(10, transform.position);
            }
            
            if (currentLives <= 0)
            {
                DestroyAngel();
            }
            else
            {
                // Visual feedback for hit
                StartCoroutine(FlashOnHit());
            }
            
            return true; // Hit was blocked
        }
        
        return false; // Hit was not blocked
    }
    
    private System.Collections.IEnumerator FlashOnHit()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }
    
    private void DestroyAngel()
    {
        if (isDestroying) return;
        isDestroying = true;
        
        Debug.Log("Angel shield destroyed");
        StartCoroutine(FadeOutAndDestroy());
    }
    
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float fadeTimer = 0f;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        while (fadeTimer < fadeOutTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, fadeTimer / fadeOutTime);
            
            if (spriteRenderer != null)
            {
                Color newColor = startColor;
                newColor.a = alpha;
                spriteRenderer.color = newColor;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    

} 