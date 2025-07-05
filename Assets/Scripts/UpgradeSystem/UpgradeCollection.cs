using UnityEngine;
using System.Collections.Generic;

public class UpgradeCollection
{
    public HashSet<UpgradeEffect> activeUpgrades = new HashSet<UpgradeEffect>();
    public List<Upgrade> collectedUpgrades = new List<Upgrade>();
    
    public void AddUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return;
        
        if (!collectedUpgrades.Contains(upgrade))
        {
            if (upgrade.interactionType != InteractionType.one_time)
            {
                collectedUpgrades.Add(upgrade);
                activeUpgrades.Add(upgrade.effect);
            }
        }
    }
    
    public bool HasUpgrade(UpgradeEffect effect)
    {
        return activeUpgrades.Contains(effect);
    }
    
    public List<UpgradeEffect> GetActiveUpgrades()
    {
        List<UpgradeEffect> activeUpgradeList = new List<UpgradeEffect>();
        
        foreach (Upgrade upgrade in collectedUpgrades)
        {
            if (upgrade.interactionType == InteractionType.active)
            {
                activeUpgradeList.Add(upgrade.effect);
            }
        }
        
        return activeUpgradeList;
    }
    
    public Upgrade GetUpgrade(UpgradeEffect effect)
    {
        return collectedUpgrades.Find(u => u.effect == effect);
    }
    
    public void Reset()
    {
        activeUpgrades.Clear();
        collectedUpgrades.Clear();
    }
    
    public int GetActiveUpgradeCount()
    {
        return GetActiveUpgrades().Count;
    }
} 