using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
// Using universal render pipeline

public class ShinyEnemyCont : MonoBehaviour
{
    private Light2D shinyLight;
    public SpriteRenderer enemySprite;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shinyLight = GetComponent<Light2D>();
        if (shinyLight == null)
        {
            Debug.LogError("ShinyEnemyCont: Shiny light is null");
            return;
        }
        
        // Make sure the light is in Sprite mode
        if (shinyLight.lightType != Light2D.LightType.Sprite)
        {
            shinyLight.lightType = Light2D.LightType.Sprite;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (shinyLight != null && enemySprite != null)
        {
            // Make the light sprite always match the enemy sprite
            if (enemySprite.sprite != null && shinyLight.lightCookieSprite != enemySprite.sprite)
            {
                shinyLight.lightCookieSprite = enemySprite.sprite;
            }
            
            // Flip the light scale to match enemy sprite flip
            Vector3 lightScale = transform.localScale;
            if (enemySprite.flipX)
            {
                lightScale.x = Mathf.Abs(lightScale.x) * -1f;
            }
            else
            {
                lightScale.x = Mathf.Abs(lightScale.x);
            }
            transform.localScale = lightScale;
            
            // You can also change other light properties
            // shinyLight.intensity = 0.5f + Mathf.Sin(Time.time * 2f) * 0.3f; // Pulsing intensity
            // shinyLight.color = Color.Lerp(Color.yellow, Color.white, Mathf.Sin(Time.time * 1.5f) * 0.5f + 0.5f); // Color cycling
        }
    }
    
    
}
