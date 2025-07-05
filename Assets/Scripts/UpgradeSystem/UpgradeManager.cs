using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("Upgrade Database")]
    public UpgradeDatabase upgradeDatabase; 
    public UpgradeDatabase testUpgradeDatabase;
    public bool useTestUpgradeDatabase = false;
    
    [Header("Starting Upgrades")]
    public List<Upgrade> startingUpgrades = new List<Upgrade>();
    public bool applyStartingUpgradesOnStart = true;
    
    [Header("Input Settings")]
    public bool startInImmediateActivationMode = false;
    
    [Header("Subsystems")]
    private UpgradeCollection upgradeCollection;
    private UpgradeCooldownManager cooldownManager;
    private UpgradeInputHandler inputHandler;
    private UpgradeEffectExecutor effectExecutor;
    
    // Events
    public event Action<UpgradeEffect> OnUpgradeApplied;
    public event Action<Upgrade> OnUpgradeCollected;
    public event Action<UpgradeEffect?> OnCurrentUpgradeChanged;
    public event Action<UpgradeEffect, float> OnCooldownChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSubsystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartCoroutine(ApplySavedUpgradesAfterFrame());
    }
    
    private void InitializeSubsystems()
    {
        // Create and initialize subsystems
        upgradeCollection = new UpgradeCollection();
        
        cooldownManager = gameObject.AddComponent<UpgradeCooldownManager>();
        cooldownManager.Initialize(upgradeCollection);
        cooldownManager.OnCooldownChanged += (effect, progress) => OnCooldownChanged?.Invoke(effect, progress);
        
        inputHandler = gameObject.AddComponent<UpgradeInputHandler>();
        inputHandler.OnCurrentUpgradeChanged += (effect) => OnCurrentUpgradeChanged?.Invoke(effect);
        
        effectExecutor = gameObject.AddComponent<UpgradeEffectExecutor>();
        
        inputHandler.Initialize(upgradeCollection, cooldownManager, effectExecutor);
        
        // Set initial activation mode
        inputHandler.immediateActivationMode = startInImmediateActivationMode;
    }
    
    private System.Collections.IEnumerator ApplySavedUpgradesAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        
        // Save collected upgrades and clear them
        List<Upgrade> savedUpgrades = new List<Upgrade>(upgradeCollection.collectedUpgrades);
        upgradeCollection.Reset();
        
        // Manually apply each saved upgrade
        foreach (Upgrade upgrade in savedUpgrades)
        {
            ApplyUpgrade(upgrade);
        }
        
        // Apply starting upgrades if enabled
        if (applyStartingUpgradesOnStart)
        {
            ApplyStartingUpgrades();
        }
        
        // Ensure all active upgrades have cooldowns initialized
        InitializeAllActiveUpgradeCooldowns();
    }
    
    public UpgradeEffect? GetCurrentUpgrade()
    {
        return inputHandler?.GetCurrentUpgrade();
    }
    
    public void ApplyUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) {
            Debug.LogError("Upgrade is null");
            return;
        }
        
        upgradeCollection.AddUpgrade(upgrade);
        
        if (upgrade.interactionType == InteractionType.active)
        {
            cooldownManager.InitializeCooldownForUpgrade(upgrade.effect);
        }
        
        OnUpgradeApplied?.Invoke(upgrade.effect);
        if (upgrade.interactionType != InteractionType.one_time)
        {
            OnUpgradeCollected?.Invoke(upgrade);
        }
    }
    
    public bool HasUpgrade(UpgradeEffect effect)
    {
        return upgradeCollection.HasUpgrade(effect);
    }
    
    public List<Upgrade> GetRandomUpgrades(int count)
    {
        UpgradeDatabase databaseToUse = useTestUpgradeDatabase ? testUpgradeDatabase : upgradeDatabase;
        if (databaseToUse == null) {
            Debug.LogError("Upgrade database is null");
            return new List<Upgrade>();
        }
        return databaseToUse.GetRandomUpgrades(count);
    }
    
    public List<Upgrade> GetRandomUpgradesByCategory(int count, UpgradeCategory category)
    {
        UpgradeDatabase databaseToUse = useTestUpgradeDatabase ? testUpgradeDatabase : upgradeDatabase;
        if (databaseToUse == null) {
            Debug.LogError("Upgrade database is null");
            return new List<Upgrade>();
        }
        return databaseToUse.GetRandomUpgradesByCategory(count, category);
    }
    
    public void ResetUpgrades()
    {
        upgradeCollection.Reset();
        cooldownManager.ClearAllCooldowns();
    }
    
    public void ApplyStartingUpgrades()
    {
        foreach (Upgrade upgrade in startingUpgrades)
        {
            if (upgrade != null)
            {
                ApplyUpgrade(upgrade);
                Debug.Log($"Applied starting upgrade: {upgrade.upgradeName}");
            }
        }
    }
    
    public void AddStartingUpgrade(Upgrade upgrade)
    {
        if (upgrade != null && !startingUpgrades.Contains(upgrade))
        {
            startingUpgrades.Add(upgrade);
        }
    }
    
    public void RemoveStartingUpgrade(Upgrade upgrade)
    {
        if (upgrade != null)
        {
            startingUpgrades.Remove(upgrade);
        }
    }
    
    public void ClearStartingUpgrades()
    {
        startingUpgrades.Clear();
    }

    public void StartCooldown(UpgradeEffect effect)
    {
        cooldownManager.StartCooldown(effect);
    }
    
    public float GetCooldownProgress(UpgradeEffect effect)
    {
        return cooldownManager.GetCooldownProgress(effect);
    }
    
    public bool IsUpgradeReady(UpgradeEffect effect)
    {
        return cooldownManager.IsUpgradeReady(effect);
    }
    
    public float GetRemainingCooldown(UpgradeEffect effect)
    {
        return cooldownManager.GetRemainingCooldown(effect);
    }
    
    public void ClearAllCooldowns()
    {
        cooldownManager.ClearAllCooldowns();
    }
    
    public bool TriggerUpgradeImmediately(UpgradeEffect effect)
    {
        return inputHandler.TriggerUpgradeImmediately(effect);
    }
    
    public bool UseActiveUpgrade(UpgradeEffect effect)
    {
        return inputHandler.UseActiveUpgrade(effect);
    }
    
    private void InitializeAllActiveUpgradeCooldowns()
    {
        StartCoroutine(UpdateUIAfterInitialization());
    }
    
    private System.Collections.IEnumerator UpdateUIAfterInitialization()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        cooldownManager.ForceRefreshAllCooldowns();
    }
    
    public void ForceRefreshAllCooldowns()
    {
        cooldownManager.ForceRefreshAllCooldowns();
    }
    
    public void SetCurrentUpgradeIndex(int newIndex)
    {
        inputHandler.SetCurrentUpgradeIndex(newIndex);
    }
    
    public void ToggleActivationMode()
    {
        inputHandler.ToggleActivationMode();
    }
    
    // Backward compatibility properties
    public bool immediateActivationMode => inputHandler?.immediateActivationMode ?? false;
    public int currentUpgradeIndex => inputHandler?.currentUpgradeIndex ?? -1;
    public List<Upgrade> collectedUpgrades => upgradeCollection?.collectedUpgrades ?? new List<Upgrade>();
    public HashSet<UpgradeEffect> activeUpgrades => upgradeCollection?.activeUpgrades ?? new HashSet<UpgradeEffect>();
} 