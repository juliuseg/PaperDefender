using UnityEngine;
using UnityEngine.UI;

public class SpriteUVRemapper : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Image image;
    private MaterialPropertyBlock materialPropertyBlock;
    
    void Start()
    {
        Initialize();
        UpdateUVRemap();
    }
    
    void OnValidate()
    {
        // This runs in edit mode when values change in inspector
        if (Application.isPlaying)
        {
            UpdateUVRemap();
        }
        else
        {
            // In edit mode, we need to update the material directly
            UpdateUVRemapInEditMode();
        }
    }
    
    void Initialize()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (image == null)
            image = GetComponent<Image>();
        if (materialPropertyBlock == null)
            materialPropertyBlock = new MaterialPropertyBlock();
    }
    
    void UpdateUVRemap()
    {
        Initialize();
        
        // Try SpriteRenderer first
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            UpdateUVRemapForSpriteRenderer();
        }
        // Try Image if SpriteRenderer doesn't have a sprite
        else if (image != null && image.sprite != null)
        {
            UpdateUVRemapForImage();
        }
    }
    
    void UpdateUVRemapForSpriteRenderer()
    {
        var sprite = spriteRenderer.sprite;
        var rect = sprite.textureRect;
        var texelSize = sprite.texture.texelSize;
        
        // Calculate UV remap values
        Vector4 uvRemap = new Vector4(
            rect.x * texelSize.x,
            rect.y * texelSize.y,
            rect.width * texelSize.x,
            rect.height * texelSize.y
        );
        
        // Set the UV remap property on the material
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetVector("_UVRemap", uvRemap);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    
    void UpdateUVRemapForImage()
    {
        var sprite = image.sprite;
        var rect = sprite.textureRect;
        var texelSize = sprite.texture.texelSize;
        
        // Calculate UV remap values
        Vector4 uvRemap = new Vector4(
            rect.x * texelSize.x,
            rect.y * texelSize.y,
            rect.width * texelSize.x,
            rect.height * texelSize.y
        );
        
        // For Image components, we need to modify the material directly
        if (image.material != null)
        {
            image.material.SetVector("_UVRemap", uvRemap);
        }
    }
    
    void UpdateUVRemapInEditMode()
    {
        Initialize();
        
        // Try SpriteRenderer first
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            UpdateUVRemapForSpriteRendererInEditMode();
        }
        // Try Image if SpriteRenderer doesn't have a sprite
        else if (image != null && image.sprite != null)
        {
            UpdateUVRemapForImageInEditMode();
        }
    }
    
    void UpdateUVRemapForSpriteRendererInEditMode()
    {
        if (spriteRenderer.sprite == null) return;
        
        var sprite = spriteRenderer.sprite;
        var rect = sprite.textureRect;
        var texelSize = sprite.texture.texelSize;
        
        // Calculate UV remap values
        Vector4 uvRemap = new Vector4(
            rect.x * texelSize.x,
            rect.y * texelSize.y,
            rect.width * texelSize.x,
            rect.height * texelSize.y
        );
        
        // In edit mode, we need to modify the material directly
        if (spriteRenderer.material != null)
        {
            spriteRenderer.material.SetVector("_UVRemap", uvRemap);
        }
    }
    
    void UpdateUVRemapForImageInEditMode()
    {
        if (image.sprite == null) return;
        
        var sprite = image.sprite;
        var rect = sprite.textureRect;
        var texelSize = sprite.texture.texelSize;
        
        // Calculate UV remap values
        Vector4 uvRemap = new Vector4(
            rect.x * texelSize.x,
            rect.y * texelSize.y,
            rect.width * texelSize.x,
            rect.height * texelSize.y
        );
        
        // In edit mode, we need to modify the material directly
        if (image.material != null)
        {
            image.material.SetVector("_UVRemap", uvRemap);
        }
    }
    
    // Call this if the sprite changes at runtime
    public void RefreshUVRemap()
    {
        UpdateUVRemap();
    }
    
    // Editor-only method to update UV remap in edit mode
    [ContextMenu("Update UV Remap")]
    void UpdateUVRemapContextMenu()
    {
        UpdateUVRemapInEditMode();
    }
} 