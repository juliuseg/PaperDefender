using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Add this for hover detection

public class UpgradeUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject upgradeBoxPrefab;
    public Transform upgradeContainer;
    public int upgradesToShow = 3;

    public float spacing;

    public Material oneTimeUpgradeMaterial;
    public Material passiveUpgradeMaterial;
    public Material activeUpgradeMaterial; 

    [Header("Collected Upgrades UI")]
    public GameObject upgradeUIBoxPrefab; // The new upgrade_ui_box prefab
    public Transform collectedUpgradesContainer; // Container for displaying collected upgrades
    public float uiBoxSpacing; // Spacing between UI boxes

    public Color UIBoxHighlightColor; // Box highlight color (when selected)
    public Color UIBoxNormalColor; // Box normal color (when not selected)
    public Color UIFillPassiveColor; // Fill color for passive upgrades
    public Color UIFillActiveColor; // Fill color for active upgrades

    public float UIBoxHighlightScale; // Scale up when selected
    
    [Header("Cutout Shader")]
    public Shader cutoutShader; // Assign your cutout shader here in the inspector
    
    [Header("Tooltip System")]
    public GameObject tooltipPrefab; // Prefab for the tooltip (same as upgradeBoxPrefab)
    public Transform tooltipContainer; // Container for the tooltip
    public float tooltipScale = 1.0f; // Scale of the tooltip box
    public float tooltipHideDelay = 1.0f; // Seconds to wait before hiding tooltip after hover ends
    
    [Header("Current Display")]
    private List<GameObject> currentUpgradeBoxes = new List<GameObject>();
    private List<Upgrade> currentUpgrades = new List<Upgrade>();
    
    [Header("Collected Upgrades Display")]
    private List<GameObject> collectedUpgradeBoxes = new List<GameObject>();
    private Dictionary<UpgradeEffect, GameObject> upgradeBoxesByEffect = new Dictionary<UpgradeEffect, GameObject>();
    private Dictionary<UpgradeEffect, VerticalCutoutController> cutoutControllersByEffect = new Dictionary<UpgradeEffect, VerticalCutoutController>();
    
    [Header("Tooltip Management")]
    private GameObject currentTooltip;
    private Dictionary<UpgradeEffect, Upgrade> upgradesByEffect = new Dictionary<UpgradeEffect, Upgrade>();
    private Coroutine hideTooltipCoroutine;
    private Coroutine fadeOutAndDestroyCoroutine;
    
    void Start()
    {
        // Subscribe to upgrade manager events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeCollected += OnUpgradeCollected;
            UpgradeManager.Instance.OnCurrentUpgradeChanged += OnCurrentUpgradeChanged;
            
            // Show any existing collected upgrades
            ShowCollectedUpgrades();
        } else {
            Debug.LogWarning("UpgradeManager.Instance == null");
        }
        
        // Subscribe to cooldown manager events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnCooldownChanged += OnCooldownChanged;
        }
        
        // Force refresh all cooldowns to ensure proper UI display
        StartCoroutine(ForceRefreshCooldownsAfterFrame());
    }
    
    private System.Collections.IEnumerator ForceRefreshCooldownsAfterFrame()
    {
        // Wait multiple frames to ensure UI is fully ready
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ForceRefreshAllCooldowns();
        }
    }
    
    public void ShowUpgradeSelection()
    {
        ClearCurrentUpgrades();
        
        // Get random upgrades (will exclude already collected ones)
        currentUpgrades = UpgradeManager.Instance.GetRandomUpgrades(upgradesToShow);
        
        // Create UI boxes for each upgrade (may be fewer than upgradesToShow)
        for (int i = 0; i < currentUpgrades.Count; i++)
        {
            CreateUpgradeBox(currentUpgrades[i], i);
        }
        
        // Debug.Log($"Showing {currentUpgrades.Count} upgrade choices (requested {upgradesToShow}, but some may have been excluded as already collected)");
    }
    
    private void CreateUpgradeBox(Upgrade upgrade, int index)
    {
        if (upgradeBoxPrefab == null || upgradeContainer == null) return;
        
        GameObject upgradeBox = Instantiate(upgradeBoxPrefab, upgradeContainer);
        currentUpgradeBoxes.Add(upgradeBox);
        
        // Set up the upgrade box UI
        SetupUpgradeBoxUI(upgradeBox, upgrade, index);
    }
    
    private void SetupUpgradeBoxUI(GameObject upgradeBox, Upgrade upgrade, int index)
    {
        // Find UI components
        Image iconImage = upgradeBox.transform.Find("PostItNote/Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = upgradeBox.transform.Find("PostItNote/Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = upgradeBox.transform.Find("PostItNote/Description")?.GetComponent<TextMeshProUGUI>();
        Button selectButton = upgradeBox.transform.Find("PostItNote")?.GetComponent<Button>();
        Image fillImage = upgradeBox.transform.Find("PostItNote/Fill")?.GetComponent<Image>();
        
        // Ensure animation starts properly
        Animator postItAnimator = upgradeBox.transform.Find("PostItNote/PostIt")?.GetComponent<Animator>();
        if (postItAnimator != null)
        {
            // Get the actual animation duration from the Animator
            AnimationClip[] clips = postItAnimator.runtimeAnimatorController.animationClips;
            float animationDuration = 1.0f; // Default fallback
            
            // Find the PostItAnim clip and get its duration
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "PostItAnim")
                {
                    animationDuration = clip.length;
                    // print ("Animation duration: " + animationDuration);
                    break;
                }
            }
            
            // Offset each box by 1/3 of the animation duration to show different frames
            float offsetTime = index * (animationDuration / upgradesToShow);
            postItAnimator.Play("PostItAnim", 0, offsetTime);
            
            // Debug.Log($"UpgradeBox {index}: Started PostItAnim animation (duration: {animationDuration:F2}s) with offset {offsetTime:F2}s");
        }
        
        // Debug component finding
        if (iconImage == null)
            Debug.LogWarning($"UpgradeBox {index}: Could not find Icon Image component");
        if (nameText == null)
            Debug.LogWarning($"UpgradeBox {index}: Could not find Name TextMeshPro component");
        if (descriptionText == null)
            Debug.LogWarning($"UpgradeBox {index}: Could not find Description TextMeshPro component");
        if (selectButton == null)
            Debug.LogWarning($"UpgradeBox {index}: Could not find Button component");
        if (fillImage == null)
            Debug.LogWarning($"UpgradeBox {index}: Could not find Fill Image component");
        
        // Set up the content
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
            // Debug.Log($"UpgradeBox {index}: Set icon for {upgrade.upgradeName}");
        }
        else if (iconImage != null && upgrade.icon == null)
        {
            Debug.LogWarning($"UpgradeBox {index}: Upgrade {upgrade.upgradeName} has no icon assigned");
        }

        if (fillImage != null)
        {
            if (upgrade.interactionType == InteractionType.one_time)
            {
                fillImage.material = oneTimeUpgradeMaterial;
            }
            else if (upgrade.interactionType == InteractionType.passive)
            {
                fillImage.material = passiveUpgradeMaterial;

            }
            else if (upgrade.interactionType == InteractionType.active)
            {
                fillImage.material = activeUpgradeMaterial;

            }
        }

        if (nameText != null)
        {
            string typeText = upgrade.interactionType switch
            {
                InteractionType.passive => "Passive",
                InteractionType.one_time => "One Time",
                InteractionType.active => "Active",
                _ => "Unknown"
            };
            nameText.text = $"{upgrade.upgradeName}\n<size=70%>({typeText})</size>";
            // Debug.Log($"UpgradeBox {index}: Set name to '{upgrade.upgradeName}'");
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
            // Debug.Log($"UpgradeBox {index}: Set description to '{upgrade.description}'");
        }

        
        // Set up the button
        if (selectButton != null)
        {

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => OnUpgradeSelected(index));
            // Debug.Log($"UpgradeBox {index}: Button click listener set up");
        }
        
        // Position the upgrade box with centered spacing
        PositionUpgradeBox(upgradeBox, index);
    }
    
    private void PositionUpgradeBox(GameObject upgradeBox, int index)
    {
        // Calculate centered positions based on number of upgrades
        float xPosition = 0f;
        
        if (currentUpgrades.Count == 3)
        {
            // 3 upgrades: -spacing, 0, spacing
            xPosition = (index - 1) * spacing;
        }
        else if (currentUpgrades.Count == 2)
        {
            // 2 upgrades: -spacing/2, spacing/2
            xPosition = (index - 0.5f) * spacing;
        }
        else if (currentUpgrades.Count == 1)
        {
            // 1 upgrade: 0 (centered)
            xPosition = 0f;
        }
        else
        {
            // Fallback for other numbers: distribute evenly
            float totalWidth = (currentUpgrades.Count - 1) * spacing;
            float startX = -totalWidth / 2f;
            xPosition = startX + (index * spacing);
        }
        
        RectTransform rectTransform = upgradeBox.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(xPosition, 0);
            // Debug.Log($"UpgradeBox {index}: Positioned at x = {xPosition} (total upgrades: {currentUpgrades.Count})");
        }
        else
        {
            Debug.LogWarning($"UpgradeBox {index}: No RectTransform found for positioning");
        }
    }
    
    public void OnUpgradeSelected(int upgradeIndex)
    {
        GameFlowManager gmf = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameFlowManager>();
        gmf.round++;

        if (upgradeIndex >= 0 && upgradeIndex < currentUpgrades.Count)
        {
            Upgrade selectedUpgrade = currentUpgrades[upgradeIndex];
            UpgradeManager.Instance.ApplyUpgrade(selectedUpgrade);
            
            // Hide the upgrade selection UI
            ClearCurrentUpgrades();
            
            Debug.Log($"Upgrade selected: {selectedUpgrade.upgradeName} (index: {upgradeIndex})");

            gmf.CloseWinMenusAndRestart();
        }
        else if (upgradeIndex == -1)
        {
            // Hide the upgrade selection UI
            ClearCurrentUpgrades();
            
            Debug.Log("Upgrade selected: No upgrade");

            gmf.CloseWinMenusAndRestart();
        } 
        else
        {
            Debug.LogError($"Invalid upgrade index: {upgradeIndex}");
        }
    }
    
    private void ClearCurrentUpgrades()
    {
        // Destroy all current upgrade boxes
        foreach (GameObject box in currentUpgradeBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }
        currentUpgradeBoxes.Clear();
        currentUpgrades.Clear();
    }
    
    private void OnUpgradeCollected(Upgrade upgrade)
    {
        // You can add visual feedback here when an upgrade is collected
        // Debug.Log($"Upgrade collected: {upgrade.upgradeName}");
        
        // Only recreate the display when a new upgrade is collected
        ShowCollectedUpgrades();
    }
    
    private void OnCurrentUpgradeChanged(UpgradeEffect? newCurrentUpgrade)
    {
        // Only update the fill colors when current upgrade changes
        UpdateCollectedUpgradesFillColors(newCurrentUpgrade);
        // Debug.Log($"Current upgrade changed to: {newCurrentUpgrade}");
    }
    
    private void OnCooldownChanged(UpgradeEffect effect, float cooldownProgress)
    {
        // Update the cutout controller for this upgrade
        if (cutoutControllersByEffect.ContainsKey(effect))
        {
            VerticalCutoutController cutoutController = cutoutControllersByEffect[effect];
            if (cutoutController != null)
            {
                // For active upgrades, use cooldown progress (0 = ready, 1 = just used)
                // We want to show the fill from 0 to (1 - cooldownProgress) so it fills up as cooldown decreases
                cutoutController.SetCutoutRange(0.0f, 1.0f - cooldownProgress);
            }
            else
            {
                Debug.LogWarning($"UI: Cutout controller is null for {effect}");
            }
        }
        else
        {
            // This is expected during initialization - the UI might not be ready yet
            // We'll retry this update later when the UI is fully initialized
            Debug.Log($"UI: No cutout controller found for {effect} (UI may not be ready yet)");
        }
        
        // Update the icon color based on cooldown progress
        if (upgradeBoxesByEffect.ContainsKey(effect))
        {
            GameObject upgradeBox = upgradeBoxesByEffect[effect];
            if (upgradeBox != null)
            {
                Image iconImage = upgradeBox.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    // When cooldown progress is 1.0 (just used), icon is at 0.5 alpha
                    // When cooldown progress is 0.0 (ready), icon is fully opaque (1.0 alpha)
                    Color iconColor = iconImage.color;
                    iconColor.a = cooldownProgress > 0f ? 0.5f : 1.0f; // 0.5 during cooldown, 1.0 when ready
                    iconImage.color = iconColor;
                    
                    Debug.Log($"UI: Updated icon alpha for {effect} to {iconColor.a:F2}");
                }
                else
                {
                    Debug.LogWarning($"UI: Icon Image not found for {effect}");
                }
            }
        }
    }
    
    /// <summary>
    /// Displays all collected upgrades in the UI using the new upgrade_ui_box prefab
    /// </summary>
    public void ShowCollectedUpgrades()
    {
        ClearCollectedUpgrades();
        
        if (UpgradeManager.Instance == null || collectedUpgradesContainer == null)
        {
            Debug.LogError("UpgradeManager or collectedUpgradesContainer is null");
            return;
        }
        
        // Get all collected upgrades
        List<Upgrade> allUpgrades = new List<Upgrade>(UpgradeManager.Instance.collectedUpgrades);
        
        // Separate active and passive upgrades
        List<Upgrade> activeUpgrades = new List<Upgrade>();
        List<Upgrade> passiveUpgrades = new List<Upgrade>();
        
        foreach (Upgrade upgrade in allUpgrades)
        {
            if (upgrade.interactionType == InteractionType.active)
            {
                activeUpgrades.Add(upgrade);
            }
            else
            {
                passiveUpgrades.Add(upgrade);
            }
        }
        
        // Get current upgrade for highlighting
        UpgradeEffect? currentUpgradeEffect = UpgradeManager.Instance.GetCurrentUpgrade();
        
        int boxIndex = 0;
        
        // Display active upgrades first (with key numbers)
        for (int i = 0; i < activeUpgrades.Count; i++)
        {
            Upgrade upgrade = activeUpgrades[i];
            bool isCurrentUpgrade = currentUpgradeEffect.HasValue && upgrade.effect == currentUpgradeEffect.Value;
            
            CreateCollectedUpgradeBox(upgrade, boxIndex, i + 1, isCurrentUpgrade); // Key 1, 2, 3, etc.
            boxIndex++;
        }
        
        // Display passive upgrades (no key numbers)
        for (int i = 0; i < passiveUpgrades.Count; i++)
        {
            Upgrade upgrade = passiveUpgrades[i];
            bool isCurrentUpgrade = false; // Passive upgrades can't be current
            
            CreateCollectedUpgradeBox(upgrade, boxIndex, -1, isCurrentUpgrade); // No key for passive
            boxIndex++;
        }
        
        // Debug.Log($"Displayed {activeUpgrades.Count} active and {passiveUpgrades.Count} passive upgrades");
    }
    
    /// <summary>
    /// Creates a UI box for a collected upgrade
    /// </summary>
    private void CreateCollectedUpgradeBox(Upgrade upgrade, int index, int keyNumber, bool isCurrentUpgrade)
    {
        if (upgradeUIBoxPrefab == null || collectedUpgradesContainer == null)
        {
            Debug.LogError("upgradeUIBoxPrefab or collectedUpgradesContainer is null");
            return;
        }
        
        GameObject upgradeBox = Instantiate(upgradeUIBoxPrefab, collectedUpgradesContainer);
        collectedUpgradeBoxes.Add(upgradeBox);
        
        // Track the upgrade box by its effect for easy access
        upgradeBoxesByEffect[upgrade.effect] = upgradeBox;
        
        // Set up the upgrade box UI
        SetupCollectedUpgradeBoxUI(upgradeBox, upgrade, index, keyNumber, isCurrentUpgrade);
    }
    
    /// <summary>
    /// Sets up the UI elements for a collected upgrade box
    /// </summary>
    private void SetupCollectedUpgradeBoxUI(GameObject upgradeBox, Upgrade upgrade, int index, int keyNumber, bool isCurrentUpgrade)
    {
        // Find UI components in the upgrade_ui_box prefab
        TextMeshProUGUI keyText = upgradeBox.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = upgradeBox.transform.Find("Icon")?.GetComponent<Image>();
        Image fillImage = upgradeBox.transform.Find("Fill")?.GetComponent<Image>();
        
        // Debug component finding
        if (keyText == null)
            Debug.LogWarning($"CollectedUpgradeBox {index}: Could not find KeyText component");
        if (iconImage == null)
            Debug.LogWarning($"CollectedUpgradeBox {index}: Could not find Icon Image component");
        if (fillImage == null)
            Debug.LogWarning($"CollectedUpgradeBox {index}: Could not find Fill Image component");
        
        // Set up key text (only for active upgrades)
        if (keyText != null)
        {
            if (keyNumber > 0)
            {
                keyText.text = $"({keyNumber})";
                keyText.gameObject.SetActive(true);
                // Debug.Log($"CollectedUpgradeBox {index}: Set key text to '({keyNumber})' for {upgrade.upgradeName}");
            }
            else
            {
                keyText.gameObject.SetActive(false);
                // Debug.Log($"CollectedUpgradeBox {index}: Disabled key text for passive upgrade {upgrade.upgradeName}");
            }
        }
        
        // Set up icon
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
            // Debug.Log($"CollectedUpgradeBox {index}: Set icon for {upgrade.upgradeName}");
        }
        else if (iconImage != null && upgrade.icon == null)
        {
            Debug.LogWarning($"CollectedUpgradeBox {index}: Upgrade {upgrade.upgradeName} has no icon assigned");
        }
        
        // Set up fill color based on interaction type only
        if (fillImage != null)
        {
            // Assign custom material with cutout shader if available
            if (cutoutShader != null)
            {
                // Create a new material instance with the cutout shader
                Material customMaterial = new Material(cutoutShader);
                fillImage.material = customMaterial;
            }
            else
            {
                Debug.LogWarning($"UI: No cutout shader assigned! Cooldown visualization won't work for {upgrade.effect}");
            }
            
            if (upgrade.interactionType == InteractionType.passive)
            {
                // Passive upgrades get their own color (can't be toggled)
                fillImage.color = UIFillPassiveColor;
                
                // Add VerticalCutoutController for passive upgrades (always show full)
                VerticalCutoutController cutoutController = fillImage.gameObject.GetComponent<VerticalCutoutController>();
                if (cutoutController == null)
                {
                    cutoutController = fillImage.gameObject.AddComponent<VerticalCutoutController>();
                }
                cutoutController.SetCutoutRange(0.0f, 1.0f);
                cutoutControllersByEffect[upgrade.effect] = cutoutController;
            }
            else
            {
                // Active upgrades - use active color for fill
                fillImage.color = UIFillActiveColor;
                
                // Add VerticalCutoutController for active upgrades
                VerticalCutoutController cutoutController = fillImage.gameObject.GetComponent<VerticalCutoutController>();
                if (cutoutController == null)
                {
                    cutoutController = fillImage.gameObject.AddComponent<VerticalCutoutController>();
                }
                
                // Set initial cooldown state
                float cooldownProgress = 0f; // Ready to use initially
                if (UpgradeManager.Instance != null)
                {
                    cooldownProgress = UpgradeManager.Instance.GetCooldownProgress(upgrade.effect);
                }
                cutoutController.SetCutoutRange(0.0f, 1.0f - cooldownProgress);
                cutoutControllersByEffect[upgrade.effect] = cutoutController;
            }
        }
        
        // Set up Box color and scale based on selection
        Image boxImage = upgradeBox.transform.Find("Box")?.GetComponent<Image>();
        if (boxImage != null)
        {
            if (isCurrentUpgrade)
            {
                boxImage.color = UIBoxHighlightColor; // Highlight selected upgrade
                upgradeBox.transform.localScale = new Vector3(UIBoxHighlightScale, UIBoxHighlightScale, 1.0f); // Scale up when selected    
                // Debug.Log($"UI: Box highlighted and scaled up for selected upgrade {upgrade.effect}");
            }
            else
            {
                boxImage.color = UIBoxNormalColor; // Normal color for unselected
                upgradeBox.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Normal scale when not selected
                // Debug.Log($"UI: Box normal color and scale for unselected upgrade {upgrade.effect}");
            }
        }
        else
        {
            Debug.LogWarning($"UI: Box Image not found for {upgrade.effect}");
        }
        
        // Add hover functionality to the upgrade box
        AddHoverFunctionality(upgradeBox, upgrade);
        
        // Store the upgrade for tooltip access
        upgradesByEffect[upgrade.effect] = upgrade;
        
        // Position the upgrade box
        PositionCollectedUpgradeBox(upgradeBox, index);
    }
    
    /// <summary>
    /// Positions a collected upgrade box in the UI
    /// </summary>
    private void PositionCollectedUpgradeBox(GameObject upgradeBox, int index)
    {
        RectTransform rectTransform = upgradeBox.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Simple horizontal layout - you can adjust this as needed
            float yPosition = index * uiBoxSpacing; // 150 pixels spacing between boxes
            rectTransform.anchoredPosition = new Vector2(0, yPosition);
            // Debug.Log($"CollectedUpgradeBox {index}: Positioned at y = {yPosition}");
        }
        else
        {
            Debug.LogWarning($"CollectedUpgradeBox {index}: No RectTransform found for positioning");
        }
    }
    
    /// <summary>
    /// Clears all displayed collected upgrade boxes
    /// </summary>
    private void ClearCollectedUpgrades()
    {
        foreach (GameObject box in collectedUpgradeBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }
        collectedUpgradeBoxes.Clear();
        upgradeBoxesByEffect.Clear(); // Clear the tracking dictionary
        cutoutControllersByEffect.Clear(); // Clear the cutout controllers dictionary
        upgradesByEffect.Clear(); // Clear the upgrades dictionary for tooltips
        HideTooltip(); // Hide any active tooltip
    }
    
    /// <summary>
    /// Updates the display of collected upgrades (call this when current upgrade changes)
    /// </summary>
    public void UpdateCollectedUpgradesDisplay()
    {
        ShowCollectedUpgrades();
    }
    
    /// <summary>
    /// Updates the box colors of collected upgrades when current upgrade changes
    /// </summary>
    private void UpdateCollectedUpgradesFillColors(UpgradeEffect? newCurrentUpgrade)
    {
        if (UpgradeManager.Instance == null) return;
        
        // Update box colors for all tracked upgrade boxes
        foreach (var kvp in upgradeBoxesByEffect)
        {
            UpgradeEffect effect = kvp.Key;
            GameObject upgradeBox = kvp.Value;
            
            if (upgradeBox != null)
            {
                bool isCurrentUpgrade = newCurrentUpgrade.HasValue && effect == newCurrentUpgrade.Value;
                
                Image boxImage = upgradeBox.transform.Find("Box")?.GetComponent<Image>();
                if (boxImage != null)
                {
                    if (isCurrentUpgrade)
                    {
                        boxImage.color = UIBoxHighlightColor; // Highlight selected upgrade
                        upgradeBox.transform.localScale = new Vector3(UIBoxHighlightScale, UIBoxHighlightScale, 1.0f); // Scale up when selected
                        Debug.Log($"UI: Box highlighted and scaled up for selected upgrade {effect}");
                    }
                    else
                    {
                        boxImage.color = UIBoxNormalColor; // Normal color for unselected
                        upgradeBox.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Normal scale when not selected
                        Debug.Log($"UI: Box normal color and scale for unselected upgrade {effect}");
                    }
                }
                else
                {
                    Debug.LogWarning($"UI: Box Image not found for {effect}");
                }
            }
        }
    }
    
    /// <summary>
    /// Adds hover functionality to an upgrade box
    /// </summary>
    private void AddHoverFunctionality(GameObject upgradeBox, Upgrade upgrade)
    {
        // Add EventTrigger component if it doesn't exist
        EventTrigger eventTrigger = upgradeBox.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = upgradeBox.AddComponent<EventTrigger>();
        }
        
        // Add PointerEnter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnUpgradeBoxHoverEnter(upgrade.effect); });
        eventTrigger.triggers.Add(enterEntry);
        
        // Add PointerExit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnUpgradeBoxHoverExit(); });
        eventTrigger.triggers.Add(exitEntry);
    }
    
    /// <summary>
    /// Called when hovering over an upgrade box
    /// </summary>
    private void OnUpgradeBoxHoverEnter(UpgradeEffect effect)
    {
        if (upgradesByEffect.ContainsKey(effect))
        {
            ShowTooltip(upgradesByEffect[effect]);
        }
    }
    
    /// <summary>
    /// Called when exiting hover over an upgrade box
    /// </summary>
    private void OnUpgradeBoxHoverExit()
    {
        // // Stop any existing hide coroutine
        // if (hideTooltipCoroutine != null)
        // {
        //     StopCoroutine(hideTooltipCoroutine);
        // }
        
        // Start delayed hide coroutine
        HideTooltip();
    }
    
    /// <summary>
    /// Shows a tooltip for the specified upgrade
    /// </summary>
    private void ShowTooltip(Upgrade upgrade)
    {
        if (tooltipPrefab == null || tooltipContainer == null)
        {
            Debug.LogWarning("Tooltip prefab or container is null");
            return;
        }
        
        // Stop any existing hide coroutine
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        // Stop any existing fade out coroutine to prevent it from deactivating the tooltip
        if (fadeOutAndDestroyCoroutine != null)
        {
            StopCoroutine(fadeOutAndDestroyCoroutine);
            fadeOutAndDestroyCoroutine = null;
        }
        
        // Create tooltip if it doesn't exist
        if (currentTooltip == null)
        {
            currentTooltip = Instantiate(tooltipPrefab, tooltipContainer);
            currentTooltip.transform.localScale = new Vector3(tooltipScale, tooltipScale, 1.0f);
            
            // Get the FadeEverythingInAndOut component and initialize it once
            FadeEverythingInAndOut fadeController = currentTooltip.GetComponent<FadeEverythingInAndOut>();
            if (fadeController != null)
            {
                fadeController.InitializeUIElements();
            }
            else
            {
                Debug.LogWarning("Tooltip prefab does not have FadeEverythingInAndOut component");
            }
        }
        
        // Update the tooltip content
        SetupTooltipContent(currentTooltip, upgrade);
        
        // Position the tooltip at the container position
        PositionTooltip();
        
        // Make sure the tooltip is visible and fade in
        currentTooltip.SetActive(true);
        
        // Get the FadeEverythingInAndOut component and fade in
        FadeEverythingInAndOut fadeControllerForFade = currentTooltip.GetComponent<FadeEverythingInAndOut>();
        if (fadeControllerForFade != null)
        {
            fadeControllerForFade.FadeIn();
        }
    }
    
    /// <summary>
    /// Sets up the content of the tooltip
    /// </summary>
    private void SetupTooltipContent(GameObject tooltip, Upgrade upgrade)
    {
        // Find UI components (same structure as upgradeBoxPrefab)
        Image iconImage = tooltip.transform.Find("PostItNote/Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = tooltip.transform.Find("PostItNote/Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = tooltip.transform.Find("PostItNote/Description")?.GetComponent<TextMeshProUGUI>();
        Image fillImage = tooltip.transform.Find("PostItNote/Fill")?.GetComponent<Image>();
        
        // Set up the content
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
        }
        
        if (fillImage != null)
        {
            if (upgrade.interactionType == InteractionType.one_time)
            {
                fillImage.material = oneTimeUpgradeMaterial;
            }
            else if (upgrade.interactionType == InteractionType.passive)
            {
                fillImage.material = passiveUpgradeMaterial;
            }
            else if (upgrade.interactionType == InteractionType.active)
            {
                fillImage.material = activeUpgradeMaterial;
            }
        }
        
        if (nameText != null)
        {
            string typeText = upgrade.interactionType switch
            {
                InteractionType.passive => "Passive",
                InteractionType.one_time => "One Time",
                InteractionType.active => "Active",
                _ => "Unknown"
            };
            nameText.text = $"{upgrade.upgradeName}\n<size=70%>({typeText})</size>";
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        
        // Disable the button component if it exists (tooltip shouldn't be clickable)
        Button tooltipButton = tooltip.transform.Find("PostItNote")?.GetComponent<Button>();
        if (tooltipButton != null)
        {
            tooltipButton.enabled = false;
        }
    }
    
    /// <summary>
    /// Positions the tooltip at the container position
    /// </summary>
    private void PositionTooltip()
    {
        if (currentTooltip == null || tooltipContainer == null) return;
        
        RectTransform rectTransform = currentTooltip.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Position the tooltip at the center of the container
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    
    /// <summary>
    /// Hides the current tooltip
    /// </summary>
    private void HideTooltip()
    {
        if (currentTooltip != null)
        {
            // Make sure the tooltip is active before trying to fade it
            if (!currentTooltip.activeSelf)
            {
                currentTooltip.SetActive(true);
            }
            
            // Get the FadeEverythingInAndOut component and fade out
            FadeEverythingInAndOut fadeController = currentTooltip.GetComponent<FadeEverythingInAndOut>();
            if (fadeController != null)
            {
                // Stop any existing fade out coroutine
                if (fadeOutAndDestroyCoroutine != null)
                {
                    StopCoroutine(fadeOutAndDestroyCoroutine);
                }
                
                // Start a coroutine to hide the tooltip after the fade out completes
                fadeOutAndDestroyCoroutine = StartCoroutine(FadeOutAndHideTooltip(fadeController));
            }
            else
            {
                // Fallback to instant hiding if no fade controller
                currentTooltip.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Coroutine to fade out the tooltip and then hide it
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndHideTooltip(FadeEverythingInAndOut fadeController)
    {
        // Start the fade out
        fadeController.FadeOut();
        
        // Wait for the fade time to complete (using unscaled time)
        yield return new WaitForSecondsRealtime(fadeController.fadeTime);
        
        // Hide the tooltip after fade out is complete
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }
        
        // Clear the coroutine reference when done
        fadeOutAndDestroyCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeCollected -= OnUpgradeCollected;
            UpgradeManager.Instance.OnCurrentUpgradeChanged -= OnCurrentUpgradeChanged;
        }
        
        // Unsubscribe from cooldown manager events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnCooldownChanged -= OnCooldownChanged;
        }
        
        // Stop any running coroutines
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        if (fadeOutAndDestroyCoroutine != null)
        {
            StopCoroutine(fadeOutAndDestroyCoroutine);
            fadeOutAndDestroyCoroutine = null;
        }
        
        // Clean up tooltip
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }
} 