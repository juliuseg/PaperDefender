using UnityEngine;
using System.Collections;

public class GoblinSpawner : MonoBehaviour
{
    [Header("Goblin Spawning")]
    public GameObject goblinPrefab;
    public Transform player;
    public float minRadius = 5f;
    public float maxRadius = 10f;
    public float spawnOuterRadius = 15f; // New: spawn outside screen
    public float initialSpawnInterval = 10f;
    [Range(0f, 1f)]
    public float spawnIntervalDecreasePercent = 0.1f; // 10% decrease per spawn

    [Header("Enemy Counting")]
    public int enemiesOnScreen = 0;

    public bool spawnedFirst = false;
    public int maxEnemiesOnScreen = 20; // Maximum enemies allowed on screen

    private float currentSpawnInterval;

    public int initialspawnamount;

    void Start()
    {
        spawnedFirst = false;
        // Spawn initial goblins
        for (int i = 0; i < initialspawnamount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float destRadius = Random.Range(minRadius, maxRadius);
            SpawnGoblinAtAngle(angle, spawnOuterRadius, destRadius);
        }
        currentSpawnInterval = GetRoundSpawnInterval();
        StartCoroutine(SpawnGoblinLoop());
    }

    float GetRoundSpawnInterval()
    {
        float spawnInterval = initialSpawnInterval-Mathf.Pow(GetComponent<GameFlowManager>().round,0.5f);
        // print ("Spawn interval: " + spawnInterval);
        return spawnInterval;
    }

    void SpawnGoblinAtAngle(float angle, float spawnRadius, float destRadius)
    {
        if (goblinPrefab == null || player == null) return;
        spawnedFirst = true;
        Vector3 spawnOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spawnRadius;
        Vector3 spawnPos = player.position + spawnOffset;
        Vector3 destOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * destRadius;
        Vector3 destPos = player.position + destOffset;
        GameObject goblin = Instantiate(goblinPrefab, spawnPos, Quaternion.identity);
        
        // Increment enemy count
        enemiesOnScreen++;
        
        EnemyController ec = goblin.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.SetDestination(destPos);
            // Subscribe to enemy death events
            ec.OnEnemyDeath += OnEnemyDeath;
            ec.OnEnemyDeathWithPosition += OnEnemyDeathWithPosition;
        }
    }

    public void OnEnemyDeath()
    {
        enemiesOnScreen--;
        if (enemiesOnScreen < 0) {
            enemiesOnScreen = 0; // Prevent negative count
            Debug.LogError("Enemies on screen is negative, this should not happen");
        }
    }
    
    public void OnEnemyDeathWithPosition(Vector3 enemyPosition)
    {
        // Notify AngelUpgradeHandler if it exists
        AngelUpgradeHandler angelHandler = GetComponent<AngelUpgradeHandler>();
        if (angelHandler != null)
        {
            angelHandler.OnEnemyDeath(enemyPosition);
        }
    }

    public int GetEnemiesOnScreen()
    {
        return enemiesOnScreen;
    }


    IEnumerator SpawnGoblinLoop()
    {
        while (true)
        {
            // Add ±30% randomness to interval
            float perc = 0.3f;
            float randomFactor = Random.Range(1-perc, 1+perc);
            if (spawnedFirst)
            {
                yield return new WaitForSeconds(currentSpawnInterval * randomFactor);
            } 
            
            // Only spawn if we haven't reached the maximum
            
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float destRadius = Random.Range(minRadius, maxRadius);
            SpawnGoblinAtAngle(angle, spawnOuterRadius, destRadius);
            currentSpawnInterval *= (1f - spawnIntervalDecreasePercent);
            if (currentSpawnInterval < 1f) currentSpawnInterval = 1f; // Clamp to minimum 1 second
            
        }
    }

    // Public method for game reset
    public void RestartSpawner()
    {
        // Reset spawn interval to initial value // 7-(4^(0,5))
        currentSpawnInterval = GetRoundSpawnInterval();
        
        // Reset enemy count
        enemiesOnScreen = 0;
        spawnedFirst = false;
        
        // Stop any existing spawn coroutine
        StopAllCoroutines();
        
        // Spawn initial goblins
        for (int i = 0; i < initialspawnamount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float destRadius = Random.Range(minRadius, maxRadius);
            SpawnGoblinAtAngle(angle, spawnOuterRadius, destRadius);
        }
        
        // Restart the spawn loop
        StartCoroutine(SpawnGoblinLoop());
    }
}
