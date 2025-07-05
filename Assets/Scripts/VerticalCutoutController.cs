using UnityEngine;
using UnityEngine.UI;

public class VerticalCutoutController : MonoBehaviour
{
    [Header("Vertical Cutout Settings")]
    [Range(0f, 1f)]
    public float startY = 0f;
    
    [Range(0f, 1f)]
    public float endY = 1f;
    
    private SpriteRenderer spriteRenderer;
    private Image image;
    private MaterialPropertyBlock propertyBlock;
    
    void Start()
    {
        Initialize();
        UpdateCutoutValues();
    }
    
    void OnValidate()
    {
        // This runs in edit mode when values change in inspector
        if (Application.isPlaying)
        {
            UpdateCutoutValues();
        }
        else
        {
            // In edit mode, we need to update the material directly for preview
            UpdateCutoutValuesInEditMode();
        }
    }
    
    void Initialize()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (image == null)
            image = GetComponent<Image>();
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }
    
    void UpdateCutoutValues()
    {
        Initialize();
        
        // Try SpriteRenderer first
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            UpdateCutoutValuesForSpriteRenderer();
        }
        // Try Image if SpriteRenderer doesn't have a material
        else if (image != null && image.material != null)
        {
            UpdateCutoutValuesForImage();
        }
    }
    
    void UpdateCutoutValuesForSpriteRenderer()
    {
        // Get the current property block
        spriteRenderer.GetPropertyBlock(propertyBlock);
        
        // Set the cutout values
        propertyBlock.SetFloat("_StartY", startY);
        propertyBlock.SetFloat("_EndY", endY);
        
        // Apply the property block back to the renderer
        spriteRenderer.SetPropertyBlock(propertyBlock);

        Debug.Log($"Updated cutout values for SpriteRenderer: {startY} - {endY}");
    }
    
    void UpdateCutoutValuesForImage()
    {
        // For Image components, we need to create a material instance to avoid affecting shared material
        if (image.materialForRendering == null) 
        {
            Debug.LogWarning("VerticalCutoutController: materialForRendering is null!");
            return;
        }
        
        // Create a material instance if we don't have one
        if (image.material == image.materialForRendering)
        {
            image.material = new Material(image.materialForRendering);
        }
        
        // Set the cutout values on the instance
        image.material.SetFloat("_StartY", startY);
        image.material.SetFloat("_EndY", endY);
    }
    
    void UpdateCutoutValuesInEditMode()
    {
        Initialize();
        
        // Try SpriteRenderer first
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            UpdateCutoutValuesForSpriteRendererInEditMode();
        }
        // Try Image if SpriteRenderer doesn't have a material
        else if (image != null && image.material != null)
        {
            UpdateCutoutValuesForImageInEditMode();
        }
    }
    
    void UpdateCutoutValuesForSpriteRendererInEditMode()
    {
        // In edit mode, we need to modify the material directly for preview
        spriteRenderer.material.SetFloat("_StartY", startY);
        spriteRenderer.material.SetFloat("_EndY", endY);

        Debug.Log($"Updated cutout values for SpriteRenderer in edit mode: {startY} - {endY}");
    }
    
    void UpdateCutoutValuesForImageInEditMode()
    {
        // In edit mode, we need to modify the material directly for preview
        if (image.material != null)
        {
            image.material.SetFloat("_StartY", startY);
            image.material.SetFloat("_EndY", endY);
        }

        Debug.Log($"Updated cutout values for Image in edit mode: {startY} - {endY}");
    }
    
    // Public methods to change values at runtime
    public void SetCutoutRange(float start, float end)
    {
        startY = Mathf.Clamp01(start);
        endY = Mathf.Clamp01(end);
        UpdateCutoutValues();
    }
    
    public void SetStartY(float start)
    {
        startY = Mathf.Clamp01(start);
        UpdateCutoutValues();
    }
    
    public void SetEndY(float end)
    {
        endY = Mathf.Clamp01(end);
        UpdateCutoutValues();
    }
    
    // Editor-only method to update cutout values in edit mode
    [ContextMenu("Update Cutout Values")]
    void UpdateCutoutValuesContextMenu()
    {
        UpdateCutoutValuesInEditMode();
    }
} 