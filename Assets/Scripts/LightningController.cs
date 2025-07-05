using UnityEngine;
using System.Collections.Generic;

public class LightningController : MonoBehaviour
{
    [Header("Lightning Settings")]
    public GameObject particleSystemPrefab;
    public float maxChainDistance = 5f; // Maximum distance to find next enemy
    public int maxChainCount = 4; // Maximum number of enemies in the chain
    public float emissionRatePerUnit = 1f;
    [Header("Particle System Settings")]
    public float delayPerUnit = 0.1f;
    public float minDelay = 0.05f;
    public float maxDelay = 1.0f;


    
    private ParticleSystemSpawner particleSpawner;
    
    void Awake()
    {
        particleSpawner = gameObject.AddComponent<ParticleSystemSpawner>();
        particleSpawner.emissionRatePerUnit = emissionRatePerUnit;
        particleSpawner.particleSystemPrefab = particleSystemPrefab;
        particleSpawner.delayPerUnit = delayPerUnit;
        particleSpawner.minDelay = minDelay;
        particleSpawner.maxDelay = maxDelay;
        particleSpawner.autoSpawn = false; // We'll control spawning manually
    }
    
    /// <summary>
    /// Casts lightning from the player to the nearest enemy and chains to nearby enemies
    /// </summary>
    /// <param name="playerTransform">The player's transform</param>
    /// <param name="targetPosition">The target position (usually mouse position)</param>
    public void CastLightning(Transform playerTransform, Vector3 targetPosition)
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player transform is null!");
            return;
        }
        
        // Ensure particle spawner is initialized
        if (particleSpawner == null)
        {
            Debug.LogError("ParticleSpawner is null! Initializing now...");
            Awake(); // Force initialization
        }
        
        // Build the chain of transforms
        List<Transform> lightningChain = BuildLightningChain(playerTransform, targetPosition);
        
        if (lightningChain.Count < 2)
        {
            Debug.LogWarning("Lightning chain has less than 2 points, cannot cast lightning!");
            return;
        }
        
        // Set the transform list and spawn the particle systems
        particleSpawner.transformPoints = lightningChain;
        particleSpawner.SpawnParticleSystemsBetweenTransforms();
        
        Debug.Log($"Lightning cast with {lightningChain.Count} points in chain");
    }
    
    /// <summary>
    /// Builds a chain of transforms from player to enemies
    /// </summary>
    /// <param name="playerTransform">The player's transform</param>
    /// <param name="targetPosition">The target position to start searching from</param>
    /// <returns>List of transforms in the lightning chain</returns>
    private List<Transform> BuildLightningChain(Transform playerTransform, Vector3 targetPosition)
    {
        List<Transform> chain = new List<Transform>();
        List<Transform> usedEnemies = new List<Transform>();
        
        // Start with the player
        chain.Add(playerTransform);
        
        // Find the first enemy (nearest to target position)
        Transform currentEnemy = FindNearestEnemy(targetPosition, usedEnemies);
        if (currentEnemy == null)
        {
            Debug.Log("No enemies found for lightning chain!");
            return chain;
        }
        
        // Add enemies to the chain
        for (int i = 0; i < maxChainCount; i++) 
        {
            if (currentEnemy == null)
            {
                break;
            }
            
            chain.Add(currentEnemy);
            usedEnemies.Add(currentEnemy);
            
            // Find the next enemy (nearest to current enemy)
            Transform nextEnemy = FindNearestEnemy(currentEnemy.position, usedEnemies);
            
            // Check if the next enemy is too far away
            if (nextEnemy == null)
            {
                Debug.Log($"No more enemies found within range. Chain length: {chain.Count}");
                break;
            }
            
            float distance = Vector3.Distance(currentEnemy.position, nextEnemy.position);
            if (distance > maxChainDistance)
            {
                Debug.Log($"Next enemy too far away ({distance:F2} > {maxChainDistance}). Chain length: {chain.Count}");
                break;
            }
            
            currentEnemy = nextEnemy;
        }
        
        return chain;
    }
    
    /// <summary>
    /// Finds the nearest enemy to a position, excluding already used enemies and checking bounds
    /// </summary>
    /// <param name="position">The position to search from</param>
    /// <param name="excludeEnemies">List of enemies to exclude from search</param>
    /// <returns>The nearest enemy transform, or null if none found</returns>
    private Transform FindNearestEnemy(Vector3 position, List<Transform> excludeEnemies)
    {
        // Find all enemies in the scene
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject enemy in allEnemies)
        {
            // Skip if enemy is null or in the exclude list
            if (enemy == null || excludeEnemies.Contains(enemy.transform))
            {
                continue;
            }
            
            // Check if enemy has EnemyController component
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                continue;
            }
            
            // Check if enemy is within its own bounds
            if (!enemyController.IsWithinBounds())
            {
                continue; // Skip enemies outside their bounds
            }
            
            // Calculate distance
            float distance = Vector3.Distance(position, enemy.transform.position);
            
            // Check if this is the nearest enemy so far
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
        
        if (nearestEnemy != null)
        {
            Debug.Log($"Found nearest enemy in bounds: {nearestEnemy.name} at distance {nearestDistance:F2}");
        }
        else
        {
            Debug.Log("No enemies found within their bounds");
        }
        
        return nearestEnemy;
    }
    

    
    /// <summary>
    /// Gets the current number of spawned particle systems
    /// </summary>
    /// <returns>Number of active particle systems</returns>
    public int GetActiveParticleSystemCount()
    {
        if (particleSpawner != null)
        {
            return particleSpawner.GetSpawnedParticleSystemCount();
        }
        return 0;
    }


} 