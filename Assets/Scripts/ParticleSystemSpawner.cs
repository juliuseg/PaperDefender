using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSystemSpawner : MonoBehaviour
{
    [Header("Particle System Settings")]
    public GameObject particleSystemPrefab;
    public float emissionRatePerUnit = 1f; // Emission rate per unit of distance
    
    [Header("Transform List")]
    public List<Transform> transformPoints = new List<Transform>();
    
    [Header("Spawn Control")]
    public bool spawnParticleSystems = false;
    public bool autoSpawn = false;
    public float autoSpawnInterval = 2f;
    public float delayPerUnit = 0.1f; // Delay per unit of distance (seconds per unit)
    public float minDelay = 0.05f; // Minimum delay regardless of distance
    public float maxDelay = 1.0f; // Maximum delay regardless of distance
    
    private float lastAutoSpawnTime;
    private List<GameObject> spawnedParticleSystems = new List<GameObject>();
    private bool isSpawningInProgress = false;
    private int currentHitMultiplier = 1;
    
    void Start()
    {
        lastAutoSpawnTime = Time.time;
    }
    
    void Update()
    {
        // Check if manual spawn is requested
        if (spawnParticleSystems && !isSpawningInProgress)
        {
            StartCoroutine(SpawnParticleSystemsBetweenTransformsWithDelay());
            spawnParticleSystems = false; // Reset the bool
        }
        
        // Check if auto spawn is enabled
        if (autoSpawn && Time.time - lastAutoSpawnTime >= autoSpawnInterval && !isSpawningInProgress)
        {
            StartCoroutine(SpawnParticleSystemsBetweenTransformsWithDelay());
            lastAutoSpawnTime = Time.time;
        }
    }
    
    private IEnumerator SpawnParticleSystemsBetweenTransformsWithDelay()
    {
        isSpawningInProgress = true;
        currentHitMultiplier = 1; // Reset hit multiplier for new spawn sequence
        
        // Validate that all transforms (except first) have EnemyController components
        if (!ValidateEnemyControllers())
        {
            isSpawningInProgress = false;
            yield break;
        }
        
        // Clear any existing spawned particle systems
        ClearSpawnedParticleSystems();
        
        if (particleSystemPrefab == null)
        {
            Debug.LogWarning("Particle System Prefab is not assigned!");
            isSpawningInProgress = false;
            yield break;
        }
        
        if (transformPoints.Count < 2)
        {
            Debug.LogWarning("Need at least 2 transforms to spawn particle systems between them!");
            isSpawningInProgress = false;
            yield break;
        }
        
        // Spawn particle systems between each pair of transforms with delay
        for (int i = 0; i < transformPoints.Count - 1; i++)
        {
            Transform startTransform = transformPoints[i];
            Transform endTransform = transformPoints[i + 1];
            
            if (startTransform == null || endTransform == null)
            {
                Debug.LogWarning($"Transform at index {i} or {i + 1} is null, skipping...");
                continue;
            }
            
            // Calculate the distance for this segment
            float segmentDistance = Vector3.Distance(startTransform.position, endTransform.position);
            
            // Damage the enemy at the end of this segment (if it's not the first transform)
            
            DamageEnemyAtTransform(endTransform);
            
            
            SpawnParticleSystemBetweenTwoTransforms(startTransform, endTransform);
            
            // Wait for the specified delay before spawning the next particle system
            if (i < transformPoints.Count - 2) // Don't wait after the last spawn
            {
                // Calculate delay based on segment length
                float calculatedDelay = segmentDistance * delayPerUnit;
                
                // Clamp the delay between min and max values
                float finalDelay = Mathf.Clamp(calculatedDelay, minDelay, maxDelay);
                
                Debug.Log($"Segment {i}: Distance = {segmentDistance}, Delay = {finalDelay}, Hit Multiplier: {currentHitMultiplier}");
                yield return new WaitForSeconds(finalDelay);
            }
        }
        
        Debug.Log($"Spawned {spawnedParticleSystems.Count} particle systems between {transformPoints.Count} transforms with delays");
        isSpawningInProgress = false;
    }
    
    public void SpawnParticleSystemsBetweenTransforms()
    {
        // For backward compatibility - starts the coroutine
        if (!isSpawningInProgress)
        {
            StartCoroutine(SpawnParticleSystemsBetweenTransformsWithDelay());
        }
    }
    
    private void SpawnParticleSystemBetweenTwoTransforms(Transform startTransform, Transform endTransform)
    {
        // Calculate the midpoint between the two transforms
        Vector3 midpoint = (startTransform.position + endTransform.position) / 2f;
        
        // Calculate the direction vector from start to end
        Vector3 direction = endTransform.position - startTransform.position;
        
        // Calculate the distance between the transforms
        float distance = direction.magnitude;
        
        // Calculate the angle (rotation) needed to point from start to end
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Calculate emission rate based on distance
        float emissionRate = distance * emissionRatePerUnit;
        
        // Spawn the particle system at the midpoint
        GameObject spawnedParticleSystem = Instantiate(particleSystemPrefab, midpoint, Quaternion.Euler(0, 0, angle));
        
        // Configure the particle system
        ConfigureParticleSystem(spawnedParticleSystem.GetComponent<ParticleSystem>(), distance, emissionRate);
        
        // Add to the list for cleanup
        spawnedParticleSystems.Add(spawnedParticleSystem);
        
        Debug.Log($"Spawned particle system between {startTransform.name} and {endTransform.name} - Distance: {distance}, Angle: {angle}, Emission Rate: {emissionRate}");
    }
    
    private void ConfigureParticleSystem(ParticleSystem ps, float length, float emissionRate)
    {
        if (ps == null)
        {
            Debug.LogWarning("ParticleSystem component not found on spawned prefab!");
            return;
        }
        
        // Configure the shape module (for line length)
        var shape = ps.shape;
        if (shape.enabled)
        {
            Vector3 scale = shape.scale;
            scale.x = length; // Set the X scale to control line length
            shape.scale = scale;
        }
        
        // Configure the emission rate
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;
    }
    
    private void ClearSpawnedParticleSystems()
    {
        foreach (GameObject ps in spawnedParticleSystems)
        {
            if (ps != null)
            {
                Destroy(ps);
            }
        }
        spawnedParticleSystems.Clear();
    }
    
    // Public method to add a transform to the list
    public void AddTransformPoint(Transform newTransform)
    {
        if (newTransform != null && !transformPoints.Contains(newTransform))
        {
            transformPoints.Add(newTransform);
        }
    }
    
    // Public method to remove a transform from the list
    public void RemoveTransformPoint(Transform transformToRemove)
    {
        if (transformPoints.Contains(transformToRemove))
        {
            transformPoints.Remove(transformToRemove);
        }
    }
    
    // Public method to clear all transform points
    public void ClearTransformPoints()
    {
        transformPoints.Clear();
        ClearSpawnedParticleSystems();
    }
    
    // Public method to spawn with a custom list of transforms
    public void SpawnParticleSystemsWithTransformList(List<Transform> customTransformList)
    {
        transformPoints = new List<Transform>(customTransformList);
        SpawnParticleSystemsBetweenTransforms();
    }
    
    // Public method to get the number of particle systems currently spawned
    public int GetSpawnedParticleSystemCount()
    {
        return spawnedParticleSystems.Count;
    }
    
    // Validate that all transforms (except the first) have EnemyController components
    private bool ValidateEnemyControllers()
    {
        if (transformPoints.Count < 2)
        {
            Debug.LogError("Need at least 2 transforms to validate enemy controllers!");
            return false;
        }
        
        // Check all transforms except the first one (index 0)
        for (int i = 1; i < transformPoints.Count; i++)
        {
            if (transformPoints[i] == null)
            {
                Debug.LogError($"Transform at index {i} is null!");
                return false;
            }
            
            EnemyController enemyController = transformPoints[i].GetComponent<EnemyController>();
            if (enemyController == null)
            {
                Debug.LogError($"Transform at index {i} ({transformPoints[i].name}) does not have an EnemyController component!");
                return false;
            }
        }
        
        Debug.Log($"Successfully validated {transformPoints.Count - 1} enemy controllers");
        return true;
    }
    
    // Damage the enemy at the specified transform and update hit multiplier
    private void DamageEnemyAtTransform(Transform enemyTransform)
    {
        if (enemyTransform == null)
        {
            Debug.LogWarning("Enemy transform is null, cannot damage!");
            return;
        }
        
        EnemyController enemyController = enemyTransform.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogWarning($"Enemy at {enemyTransform.name} does not have EnemyController component!");
            return;
        }
        
        // Try to hit the enemy with current hit multiplier
        bool hitSuccessful = enemyController.Hit(currentHitMultiplier);
        
        if (hitSuccessful)
        {
            Debug.Log($"Successfully hit enemy {enemyTransform.name} with multiplier {currentHitMultiplier}");
            // Increase hit multiplier for next enemy
            currentHitMultiplier++;
        }
        else
        {
            // Push the enemy
            enemyController.Suprise();
            Debug.Log($"Failed to hit enemy {enemyTransform.name} with multiplier {currentHitMultiplier}, pushing instead");
        }
    }
} 