using UnityEngine;
using TMPro;
using System.Collections;

public class PointFloater : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI pointText;
    public float floatSpeed = 100f;
    public float lifetime = 1.5f;
    
    [Header("Animation")]
    public Vector2 floatDirection = Vector2.up;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera mainCamera;
    private float currentLifetime = 0f;
    
    
    public void Setup(int points, Vector3 worldPosition)
    {
        StartStuff();
        // Set the point text
        SetPoints(points);
        
        // Convert world position to UI position
        SetUIPosition(worldPosition);
        
        // Start the floating animation
        StartCoroutine(FloatAndFade());
    }
    
    void StartStuff(){
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
        
        if (pointText == null)
        {
            pointText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (pointText == null)
        {
            Debug.LogError("PointFloater requires a TextMeshProUGUI component!");
            return;
        }
        
        if (canvas == null)
        {
            Debug.LogError("PointFloater must be a child of a Canvas!");
            return;
        }
    }
    
    public void SetPoints(int points)
    {
        if (pointText != null)
        {
            pointText.text = "+" + points.ToString();
        }
    }
    
    public void SetUIPosition(Vector3 worldPosition)
    {
        if (mainCamera == null || canvas == null) return;
        
        // Convert world position to screen position
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Check if the point is behind the camera
        if (screenPosition.z < 0)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            return;
        }
        
        // Handle different canvas render modes
        Camera canvasCamera = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvasCamera = canvas.worldCamera;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For Screen Space Overlay, we don't need a camera
            canvasCamera = null;
        }
        
        // Convert screen position to UI position
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvasCamera,
            out Vector2 localPoint))
        {
            // Set the UI position
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            // Fallback: if conversion fails, use screen center 
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    IEnumerator FloatAndFade()
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Color startColor = pointText.color;
        
        while (currentLifetime < lifetime)
        {
            currentLifetime += Time.unscaledDeltaTime;
            float progress = currentLifetime / lifetime;
            
            // Float upward
            Vector2 newPosition = startPosition + (floatDirection * floatSpeed * currentLifetime);
            rectTransform.anchoredPosition = newPosition;
            
            // Fade out
            Color newColor = startColor;
            newColor.a = 1f - progress;
            pointText.color = newColor;
            
            yield return null;
        }
        
        // Destroy when animation is complete
        Destroy(gameObject);
    }
} 