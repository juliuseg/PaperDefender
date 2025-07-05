using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteController : MonoBehaviour
{
    [Header("Vignette Settings")]
    public float targetIntensity = 0.5f; // The intensity when upgrades are active
    public float fadeSpeed = 2f; // How fast the vignette fades in/out
    
    [Header("Vignette Colors")]
    public Color breakShieldsColor = new Color(0.5f, 0.1f, 0.1f, 1f); // Red for BreakShields
    public Color vengeanceColor = new Color(0.8f, 0.4f, 0.1f, 1f); // Orange for Vengeance
    
    private Volume volume;
    private Vignette vignette;
    private Coroutine fadeCoroutine;
    private bool isFading = false;
    
    void Start()
    {
        // Get the Volume component
        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("VignetteController requires a Volume component!");
            return;
        }
        
        // Get the Vignette effect from the volume profile
        if (volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            if (vignette == null)
            {
                Debug.LogError("Volume profile does not contain a Vignette effect!");
                return;
            }
        }
        else
        {
            Debug.LogError("Volume profile is null!");
            return;
        }
        
        // Subscribe to the player's shield broken state changes
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Player_Controller playerController = player.GetComponent<Player_Controller>();
            if (playerController != null)
            {
                // We'll need to add an event to Player_Controller to notify when shields are broken/restored
                // For now, we'll use a polling approach in Update
            }
        }
        
        // Subscribe to game flow events to reset vignette when level ends
        GameFlowManager gameFlowManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameFlowManager>();
        if (gameFlowManager != null)
        {
            // We'll need to add events to GameFlowManager for level completion
            // For now, we'll check in Update
        }
        
        // Initialize vignette to 0 intensity
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
    }
    
    void Update()
    {
        // Check if any upgrade is active by polling the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Player_Controller playerController = player.GetComponent<Player_Controller>();
            if (playerController != null)
            {
                bool shieldsBroken = playerController.AreShieldsBroken();
                bool vengeanceActive = playerController.IsVengeanceActive();
                
                // Determine if any upgrade is active
                bool anyUpgradeActive = shieldsBroken || vengeanceActive;
                
                // If any upgrade is active and we're not already fading in
                if (anyUpgradeActive && !isFading && vignette.intensity.value < targetIntensity)
                {
                    if (fadeCoroutine != null)
                    {
                        StopCoroutine(fadeCoroutine);
                    }
                    fadeCoroutine = StartCoroutine(FadeVignette(true, shieldsBroken, vengeanceActive));
                }
                // If no upgrade is active and we're not already fading out
                else if (!anyUpgradeActive && !isFading && vignette.intensity.value > 0f)
                {
                    if (fadeCoroutine != null)
                    {
                        StopCoroutine(fadeCoroutine);
                    }
                    fadeCoroutine = StartCoroutine(FadeVignette(false, false, false));
                }
                // If upgrades are active and vignette is at full intensity, update color continuously
                else if (anyUpgradeActive && !isFading && vignette.intensity.value >= targetIntensity)
                {
                    UpdateVignetteColor(shieldsBroken, vengeanceActive);
                }
            }
        }
        
        // Check if level has ended (game is paused or win menu is active)
        GameFlowManager gameFlowManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameFlowManager>();
        if (gameFlowManager != null)
        {
            // Check if the game is paused (which happens when level ends)
            if (GameFlowManager.IsPaused && vignette.intensity.value > 0f)
            {
                ResetVignetteImmediately();
            }
        }
    }
    
    /// <summary>
    /// Fades the vignette in or out with color blending
    /// </summary>
    /// <param name="fadeIn">True to fade in, false to fade out</param>
    /// <param name="shieldsBroken">Whether BreakShields is active</param>
    /// <param name="vengeanceActive">Whether Vengeance is active</param>
    private IEnumerator FadeVignette(bool fadeIn, bool shieldsBroken, bool vengeanceActive)
    {
        isFading = true;
        
        float startIntensity = vignette.intensity.value;
        float targetIntensityValue = fadeIn ? targetIntensity : 0f;
        float fadeTime = 0f;
        
        // Determine the target color based on active upgrades
        Color targetColor = Color.white; // Default
        if (fadeIn)
        {
            if (shieldsBroken && vengeanceActive)
            {
                // Blend both colors
                targetColor = Color.Lerp(breakShieldsColor, vengeanceColor, 0.5f);
            }
            else if (shieldsBroken)
            {
                targetColor = breakShieldsColor;
            }
            else if (vengeanceActive)
            {
                targetColor = vengeanceColor;
            }
        }
        
        Color startColor = vignette.color.value;
        
        while (fadeTime < 1f)
        {
            fadeTime += Time.deltaTime * fadeSpeed;
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensityValue, fadeTime);
            Color currentColor = Color.Lerp(startColor, targetColor, fadeTime);
            
            vignette.intensity.value = currentIntensity;
            vignette.color.value = currentColor;
            yield return null;
        }
        
        // Ensure we reach the exact target values
        vignette.intensity.value = targetIntensityValue;
        vignette.color.value = targetColor;
        
        isFading = false;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Immediately resets the vignette intensity to 0 (called when level ends)
    /// </summary>
    public void ResetVignetteImmediately()
    {
        // Stop any ongoing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // Immediately set vignette intensity to 0 and reset color
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
            vignette.color.value = Color.white; // Reset to default color
        }
        
        isFading = false;
    }
    
    /// <summary>
    /// Updates the vignette color based on active upgrades (called continuously when upgrades are active)
    /// </summary>
    /// <param name="shieldsBroken">Whether BreakShields is active</param>
    /// <param name="vengeanceActive">Whether Vengeance is active</param>
    private void UpdateVignetteColor(bool shieldsBroken, bool vengeanceActive)
    {
        if (vignette == null) return;
        
        Color targetColor = Color.black;
        
        if (shieldsBroken && vengeanceActive)
        {
            // Blend both colors when both upgrades are active
            targetColor = Color.Lerp(breakShieldsColor, vengeanceColor, 0.5f);
        }
        else if (shieldsBroken)
        {
            targetColor = breakShieldsColor;
        }
        else if (vengeanceActive)
        {
            targetColor = vengeanceColor;
        }
        
        vignette.color.value = targetColor;
    }
}
