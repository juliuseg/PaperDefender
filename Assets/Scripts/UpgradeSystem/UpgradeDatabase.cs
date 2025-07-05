using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Upgrade Database", menuName = "Upgrades/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    [Header("All Available Upgrades")]
    public List<Upgrade> allUpgrades = new List<Upgrade>();
    
    public Upgrade GetRandomUpgrade()
    {
        if (allUpgrades.Count == 0) return null;
        return allUpgrades[Random.Range(0, allUpgrades.Count)];
    }
    
    public List<Upgrade> GetRandomUpgrades(int count)
    {
        List<Upgrade> result = new List<Upgrade>();
        List<Upgrade> availableUpgrades = new List<Upgrade>(allUpgrades);
        
        // Remove already collected upgrades from available options
        // BUT: Keep one-time upgrades that can be collected multiple times
        if (UpgradeManager.Instance != null)
        {
            availableUpgrades.RemoveAll(upgrade => 
                UpgradeManager.Instance.collectedUpgrades.Contains(upgrade) && 
                upgrade.interactionType != InteractionType.one_time);
        }
        
        // Apply custom filters for specific upgrades
        ApplyCustomUpgradeFilters(ref availableUpgrades);
        
        for (int i = 0; i < count && availableUpgrades.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableUpgrades.Count);
            result.Add(availableUpgrades[randomIndex]);
            availableUpgrades.RemoveAt(randomIndex);
        }
        
        return result;
    }
    
    /// <summary>
    /// Applies custom filters for specific upgrades based on game state
    /// </summary>
    /// <param name="availableUpgrades">Reference to the list of available upgrades to filter</param>
    private void ApplyCustomUpgradeFilters(ref List<Upgrade> availableUpgrades)
    {
        // Filter out OrbitShield upgrade if player already has 3 orbit shields
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerUpgradeHandler playerUpgradeHandler = player.GetComponent<PlayerUpgradeHandler>();
            if (playerUpgradeHandler != null && !playerUpgradeHandler.CanHaveMoreOrbitShields())
            {
                availableUpgrades.RemoveAll(upgrade => upgrade.effect == UpgradeEffect.OrbitShield);
                Debug.Log($"OrbitShield upgrade filtered out - player has {playerUpgradeHandler.GetOrbitShieldCount()} orbit shields (max: 3)");
            }
        }
    }
    
    public List<Upgrade> GetRandomUpgradesByCategory(int count, UpgradeCategory category)
    {
        List<Upgrade> categoryUpgrades = allUpgrades.Where(u => u.category == category).ToList();
        List<Upgrade> result = new List<Upgrade>();
        List<Upgrade> availableUpgrades = new List<Upgrade>(categoryUpgrades);
        
        // Remove already collected upgrades from available options
        // BUT: Keep one-time upgrades that can be collected multiple times
        if (UpgradeManager.Instance != null)
        {
            availableUpgrades.RemoveAll(upgrade => 
                UpgradeManager.Instance.collectedUpgrades.Contains(upgrade) && 
                upgrade.interactionType != InteractionType.one_time);
        }
        
        // Apply custom filters for specific upgrades
        ApplyCustomUpgradeFilters(ref availableUpgrades);
        
        for (int i = 0; i < count && availableUpgrades.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableUpgrades.Count);
            result.Add(availableUpgrades[randomIndex]);
            availableUpgrades.RemoveAt(randomIndex);
        }
        
        return result;
    }
    
    public List<Upgrade> GetUpgradesByCategory(UpgradeCategory category)
    {
        return allUpgrades.Where(u => u.category == category).ToList();
    }
} 