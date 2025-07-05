using UnityEngine;

// Attach this to any GameObject to handle shield-related upgrades
public class ShieldUpgradeHandler : MonoBehaviour
{
    private ShieldDeflector shieldDeflector;
    
    [Header("Upgrade Values")]
    public float shieldDurationValue = 5f;
    public float shieldRegenValue = 0.5f;
    
    void Start()
    {
        shieldDeflector = GetComponent<ShieldDeflector>();
        if (shieldDeflector == null)
        {
            Debug.LogError($"ShieldUpgradeHandler on {gameObject.name} requires a ShieldDeflector component!");
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
        
        if (UpgradeManager.Instance.HasUpgrade(UpgradeEffect.SecondShield))
        {
            Debug.Log($"Second shield upgrade applied");
        }
        
    }
    
    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied -= HandleUpgrade;
        }
    }
} 