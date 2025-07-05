using UnityEngine;
using System.Collections.Generic;

// Attach this to the Player GameObject to handle player-related upgrades
public class PlayerUpgradeHandler : MonoBehaviour
{
    private Player_Controller playerController;
    
    [Header("Upgrade Values")]
    public int shieldNewMaxHealth;
    public float shieldNewSize;
    
    [Header("Orbit Shield")]
    public GameObject orbitShieldPrefab;
    
    // Simple orbit shield count system
    private int orbitShieldCount = 0;
    private const int MAX_ORBIT_SHIELDS = 4;
    private List<GameObject> orbitShields = new List<GameObject>();
    
    void Start()
    {
        playerController = GetComponent<Player_Controller>();
        if (playerController == null)
        {
            Debug.LogError($"PlayerUpgradeHandler on {gameObject.name} requires a Player_Controller component!");
            return;
        }
        
        // Subscribe to upgrade events
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied += HandleUpgrade;
        } else {
            Debug.LogWarning("UpgradeManager.Instance == null");
        }
        
        // Subscribe to round start events
        GameFlowManager gameFlowManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameFlowManager>();
        if (gameFlowManager != null)
        {
            gameFlowManager.OnRoundStart += OnRoundStart;
        } else {
            Debug.LogWarning("GameFlowManager not found!");
        }
        
        // Spawn initial orbit shields
        SpawnOrbitShields();
    }
    
    void HandleUpgrade(UpgradeEffect effect)
    {
        switch (effect)
        {
            case UpgradeEffect.IncreaseShieldMaxHealth:
                playerController.shieldMaxLife = shieldNewMaxHealth;
                break;
                
            case UpgradeEffect.IncreaseShieldSize:
                playerController.shieldSize = shieldNewSize;
                break;
                
            case UpgradeEffect.SecondShield:
                playerController.maxShields = 2;
                break;
            
            case UpgradeEffect.ExtraLife:
                playerController.max_health++;
                break;
                
            case UpgradeEffect.OrbitShield:
                // Increment the count when upgrade is applied
                orbitShieldCount++;
                Debug.Log($"OrbitShield upgrade applied! Count: {orbitShieldCount}");
                break;
        }
    }
    
    /// <summary>
    /// Spawns orbit shields based on the current count
    /// </summary>
    private void SpawnOrbitShields()
    {
        // Clear existing shields first
        ClearOrbitShields();
        
        // Spawn new shields based on count
        for (int i = 0; i < orbitShieldCount && i < MAX_ORBIT_SHIELDS; i++)
        {
            CreateOrbitShield();
        }
        
        // Position all shields with proper spacing
        PositionOrbitShields();
        
        Debug.Log($"Spawned {orbitShieldCount} orbit shields");
    }
    
    /// <summary>
    /// Creates a single orbit shield
    /// </summary>
    private void CreateOrbitShield()
    {
        if (orbitShieldPrefab == null)
        {
            Debug.LogError("OrbitShieldPrefab is not assigned in PlayerUpgradeHandler!");
            return;
        }
        
        if (playerController.shieldContainer == null)
        {
            Debug.LogError("Player's shieldContainer is not assigned!");
            return;
        }
        
        // Create orbit shield at the shield container
        GameObject newOrbitShield = Instantiate(orbitShieldPrefab, playerController.shieldContainer);
        newOrbitShield.GetComponent<ShieldDeflector>().SetOrbitShield(true);
        orbitShields.Add(newOrbitShield);
    }
    
    /// <summary>
    /// Positions all orbit shields with proper spacing
    /// </summary>
    private void PositionOrbitShields()
    {
        if (orbitShields.Count == 0) return;
        
        float angleOffset;
        if (orbitShields.Count == 3)
        {
            angleOffset = 120f; // 360° / 3 = 120°
        }
        else if (orbitShields.Count == 2)
        {
            angleOffset = 180f; // 360° / 2 = 180°
        }
        else
        {
            angleOffset = 0f; // Single shield
        }
        
        for (int i = 0; i < orbitShields.Count; i++)
        {
            if (orbitShields[i] != null)
            {
                float angle = i * angleOffset;
                orbitShields[i].transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
    
    /// <summary>
    /// Clears all orbit shields
    /// </summary>
    private void ClearOrbitShields()
    {
        foreach (GameObject shield in orbitShields)
        {
            if (shield != null)
            {
                Destroy(shield);
            }
        }
        
        orbitShields.Clear();
    }
    
    /// <summary>
    /// Called when the game resets to clean up orbit shields
    /// </summary>
    public void DestroyOrbitShield()
    {
        ClearOrbitShields();
        Debug.Log("All orbit shields destroyed during reset");
    }
    
    /// <summary>
    /// Called when a new round starts - respawn orbit shields
    /// </summary>
    private void OnRoundStart(int roundNumber)
    {
        Debug.Log($"OnRoundStart called for round {roundNumber}, orbitShieldCount: {orbitShieldCount}");
        SpawnOrbitShields();
    }
    
    /// <summary>
    /// Returns the current number of orbit shields
    /// </summary>
    public int GetOrbitShieldCount()
    {
        return orbitShieldCount;
    }
    
    /// <summary>
    /// Returns whether the player can have more orbit shields
    /// </summary>
    public bool CanHaveMoreOrbitShields()
    {
        return orbitShieldCount < MAX_ORBIT_SHIELDS;
    }
    
    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied -= HandleUpgrade;
        }
        
        // Unsubscribe from round start events
        try
        {
            GameFlowManager gameFlowManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameFlowManager>();
            if (gameFlowManager != null)
            {
                gameFlowManager.OnRoundStart -= OnRoundStart;
            }
        }
        catch (System.Exception e)
        {
            // GameFlowManager might already be destroyed, which is fine
            Debug.Log($"Could not unsubscribe from GameFlowManager events: {e.Message}");
        }
    }
} 