using UnityEngine;
using System.Collections.Generic;

// Attach this to the GameController GameObject to handle angel spawning
public class AngelUpgradeHandler : MonoBehaviour
{
    [Header("Angel Settings")]
    public GameObject angelPrefab;
    public float spawnRadius = 2f; // Distance from enemy death position
    public int maxAngelsOnScreen = 5; // Maximum angels allowed at once
    
    private List<GameObject> activeAngels = new List<GameObject>();
    private GoblinSpawner goblinSpawner;
    
    void Start()
    {
        goblinSpawner = GetComponent<GoblinSpawner>();
        if (goblinSpawner == null)
        {
            Debug.LogError($"AngelUpgradeHandler on {gameObject.name} requires a GoblinSpawner component!");
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
        if (effect == UpgradeEffect.Angel)
        {
            Debug.Log("Angel upgrade applied - will spawn angel shields on enemy death");
            
            // Subscribe to enemy death events from the spawner
            if (goblinSpawner != null)
            {
                // We need to subscribe to each enemy's death event when they spawn
                // This is handled in the GoblinSpawner.OnEnemyDeath method
            }
        }
    }
    
    /// <summary>
    /// Called when an enemy dies - spawns an angel shield if the upgrade is active
    /// </summary>
    public void OnEnemyDeath(Vector3 enemyPosition)
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(UpgradeEffect.Angel))
        {
            SpawnAngel(enemyPosition);
        }
    }
    
    private void SpawnAngel(Vector3 enemyPosition)
    {
        if (angelPrefab == null)
        {
            Debug.LogError("Angel prefab is not assigned!");
            return;
        }
        
        // Check if we've reached the maximum number of angels
        if (activeAngels.Count >= maxAngelsOnScreen)
        {
            // Remove the oldest angel
            if (activeAngels.Count > 0)
            {
                GameObject oldestAngel = activeAngels[0];
                activeAngels.RemoveAt(0);
                if (oldestAngel != null)
                {
                    Destroy(oldestAngel);
                }
            }
        }
        
        // Calculate spawn position (random position around enemy death location)
        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPosition = enemyPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
        
        // Spawn the angel
        GameObject angel = Instantiate(angelPrefab, spawnPosition, Quaternion.identity);
        activeAngels.Add(angel);
        
        Debug.Log($"Angel shield spawned at {spawnPosition} (total angels: {activeAngels.Count})");
    }
    
    /// <summary>
    /// Called when the game resets to clean up all angels
    /// </summary>
    public void ClearAllAngels()
    {
        foreach (GameObject angel in activeAngels)
        {
            if (angel != null)
            {
                Destroy(angel);
            }
        }
        activeAngels.Clear();
        Debug.Log("All angel shields cleared during game reset");
    }
    
    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeApplied -= HandleUpgrade;
        }
    }
} 