using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeCooldownManager : MonoBehaviour
{
    private Dictionary<UpgradeEffect, float> cooldownTimers = new Dictionary<UpgradeEffect, float>();
    private UpgradeCollection upgradeCollection;
    
    public event Action<UpgradeEffect, float> OnCooldownChanged;
    
    public void Initialize(UpgradeCollection collection)
    {
        upgradeCollection = collection;
    }
    
    void Update()
    {
        UpdateCooldowns();
    }
    
    private void UpdateCooldowns()
    {
        List<UpgradeEffect> effectsToUpdate = new List<UpgradeEffect>();
        
        var cooldownTimersCopy = new Dictionary<UpgradeEffect, float>(cooldownTimers);
        
        foreach (var kvp in cooldownTimersCopy)
        {
            UpgradeEffect effect = kvp.Key;
            float currentTime = kvp.Value;
            
            if (currentTime > 0f)
            {
                cooldownTimers[effect] = currentTime - Time.deltaTime;
                effectsToUpdate.Add(effect);
            }
        }
        
        foreach (UpgradeEffect effect in effectsToUpdate)
        {
            float progress = GetCooldownProgress(effect);
            OnCooldownChanged?.Invoke(effect, progress);
        }
    }
    
    public void StartCooldown(UpgradeEffect effect)
    {
        Upgrade upgrade = upgradeCollection.GetUpgrade(effect);
        if (upgrade != null && upgrade.interactionType == InteractionType.active)
        {
            cooldownTimers[effect] = upgrade.cooldownDuration;
            float progress = GetCooldownProgress(effect);
            OnCooldownChanged?.Invoke(effect, progress);
        }
        else
        {
            Debug.LogWarning($"Could not start cooldown for {effect}: upgrade not found or not active");
        }
    }
    
    public float GetCooldownProgress(UpgradeEffect effect)
    {
        if (!cooldownTimers.ContainsKey(effect))
        {
            return 0f;
        }
        
        if (cooldownTimers[effect] <= 0f)
        {
            return 0f;
        }
        
        Upgrade upgrade = upgradeCollection.GetUpgrade(effect);
        if (upgrade == null)
        {
            return 0f;
        }
        
        return cooldownTimers[effect] / upgrade.cooldownDuration;
    }
    
    public bool IsUpgradeReady(UpgradeEffect effect)
    {
        bool ready = GetCooldownProgress(effect) <= 0f;
        if (!ready)
        {
            float remaining = GetRemainingCooldown(effect);
            Debug.Log($"Upgrade {effect} not ready: {remaining:F2}s remaining");
        }
        return ready;
    }
    
    public float GetRemainingCooldown(UpgradeEffect effect)
    {
        if (cooldownTimers.ContainsKey(effect))
        {
            return Mathf.Max(0f, cooldownTimers[effect]);
        }
        return 0f;
    }
    
    public void ClearAllCooldowns()
    {
        cooldownTimers.Clear();
    }
    
    public void InitializeCooldownForUpgrade(UpgradeEffect effect)
    {
        if (!cooldownTimers.ContainsKey(effect))
        {
            cooldownTimers[effect] = 0f;
            float progress = GetCooldownProgress(effect);
            OnCooldownChanged?.Invoke(effect, progress);
        }
    }
    
    public void ForceRefreshAllCooldowns()
    {
        foreach (Upgrade upgrade in upgradeCollection.collectedUpgrades)
        {
            if (upgrade.interactionType == InteractionType.active)
            {
                float progress = GetCooldownProgress(upgrade.effect);
                OnCooldownChanged?.Invoke(upgrade.effect, progress);
            }
        }
    }
} 