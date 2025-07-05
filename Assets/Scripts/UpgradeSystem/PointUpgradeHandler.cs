using UnityEngine;

// Attach this to the GameController GameObject to handle point-related upgrades
public class PointUpgradeHandler : MonoBehaviour
{
    private PointSystem pointSystem;
    
    [Header("Upgrade Values")]
    public float pointMultiplierValue = 2f;
    
    void Start()
    {
        pointSystem = GetComponent<PointSystem>();
        if (pointSystem == null)
        {
            Debug.LogError($"PointUpgradeHandler on {gameObject.name} requires a PointSystem component!");
            return;
        }
        
        // Subscribe to upgrade events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied += HandleUpgrade;
        }
    }
    
    void HandleUpgrade(UpgradeEffect effect)
    {
        // if (effect == UpgradeEffect.PointMultiplier)
        // {
        //     if (UpgradeManager.Instance.HasUpgrade(UpgradeEffect.PointMultiplier))
        //     {
        //         Debug.Log($"Point multiplier applied: {pointMultiplierValue}x");
        //     }
        // }
    }
    
    // Call this method from PointSystem.AddPoints() to apply multiplier
    public int ApplyPointMultiplier(int basePoints)
    {
        // if (UpgradeManager.Instance.HasUpgrade(UpgradeEffect.PointMultiplier))
        // {
        //     return Mathf.RoundToInt(basePoints * pointMultiplierValue);
        // }
        return basePoints;
    }
    
    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied -= HandleUpgrade;
        }
    }
} 