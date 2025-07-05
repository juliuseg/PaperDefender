using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class UpgradeInputHandler : MonoBehaviour
{
    public int currentUpgradeIndex = -1;
    public bool immediateActivationMode = false;
    
    private UpgradeCollection upgradeCollection;
    private UpgradeCooldownManager cooldownManager;
    private UpgradeEffectExecutor effectExecutor;
    
    public event Action<UpgradeEffect?> OnCurrentUpgradeChanged;
    
    public void Initialize(UpgradeCollection collection, UpgradeCooldownManager cooldown, UpgradeEffectExecutor executor)
    {
        upgradeCollection = collection;
        cooldownManager = cooldown;
        effectExecutor = executor;
    }
    
    void Update()
    {
        if (Time.timeScale <= 0f) return;
        
        HandleKeyboardInput();
    }
    
    private void HandleKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        for (int i = 1; i <= 9; i++)
        {
            Key key = (Key)((int)Key.Digit1 + (i - 1));
            if (keyboard[key].wasPressedThisFrame)
            {
                int newIndex = i - 1;
                List<UpgradeEffect> activeUpgradeList = upgradeCollection.GetActiveUpgrades();
                
                if (newIndex < activeUpgradeList.Count)
                {
                    UpgradeEffect upgradeEffect = activeUpgradeList[newIndex];
                    
                    if (immediateActivationMode)
                    {
                        TriggerUpgradeImmediately(upgradeEffect);
                    }
                    else
                    {
                        if (currentUpgradeIndex == newIndex)
                        {
                            SetCurrentUpgradeIndex(-1);
                        }
                        else
                        {
                            SetCurrentUpgradeIndex(newIndex);
                        }
                    }
                }
                else
                {
                    Debug.Log($"No active upgrade available at index {newIndex}. Active upgrades: {activeUpgradeList.Count}");
                }
                break;
            }
        }
    }
    
    public UpgradeEffect? GetCurrentUpgrade()
    {
        if (currentUpgradeIndex == -1)
        {
            return null;
        }
        
        List<UpgradeEffect> activeUpgradeList = upgradeCollection.GetActiveUpgrades();
        if (currentUpgradeIndex < activeUpgradeList.Count)
        {
            return activeUpgradeList[currentUpgradeIndex];
        }
        
        return null;
    }
    
    public void SetCurrentUpgradeIndex(int newIndex)
    {
        UpgradeEffect? oldCurrentUpgrade = GetCurrentUpgrade();
        currentUpgradeIndex = newIndex;
        UpgradeEffect? newCurrentUpgrade = GetCurrentUpgrade();
        
        if (oldCurrentUpgrade != newCurrentUpgrade)
        {
            OnCurrentUpgradeChanged?.Invoke(newCurrentUpgrade);
        }
        
        if (newIndex == -1)
        {
            Debug.Log("Deselected upgrade");
        }
        else
        {
            PrintCurrentUpgrade();
        }
    }
    
    private void PrintCurrentUpgrade()
    {
        if (currentUpgradeIndex == -1)
        {
            Debug.Log("No upgrade selected");
        }
        else
        {
            List<UpgradeEffect> activeUpgradeList = upgradeCollection.GetActiveUpgrades();
            if (currentUpgradeIndex < activeUpgradeList.Count)
            {
                UpgradeEffect selectedUpgrade = activeUpgradeList[currentUpgradeIndex];
                Debug.Log($"Selected upgrade: {selectedUpgrade} (key {currentUpgradeIndex + 1} pressed)");
            }
        }
    }
    
    public bool TriggerUpgradeImmediately(UpgradeEffect effect)
    {
        return UseActiveUpgrade(effect);
    }
    
    public bool UseActiveUpgrade(UpgradeEffect effect)
    {
        Upgrade upgrade = upgradeCollection.GetUpgrade(effect);
        if (upgrade == null || upgrade.interactionType != InteractionType.active)
        {
            Debug.LogWarning($"Cannot use upgrade {effect}: not found or not active");
            return false;
        }
        
        if (!cooldownManager.IsUpgradeReady(effect))
        {
            float remaining = cooldownManager.GetRemainingCooldown(effect);
            Debug.Log($"Upgrade {effect} not ready: {remaining:F2}s remaining");
            return false;
        }
        
        cooldownManager.StartCooldown(effect);
        effectExecutor.ExecuteEffect(effect);
        
        return true;
    }
    
    public void ToggleActivationMode()
    {
        immediateActivationMode = !immediateActivationMode;
        Debug.Log($"Switched to {(immediateActivationMode ? "immediate" : "select-then-click")} activation mode");
    }
} 