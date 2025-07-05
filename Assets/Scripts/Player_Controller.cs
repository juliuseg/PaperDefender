using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Player_Controller : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject MagicBullet;
    public GameObject BlastPrefab;
    public GameObject WormHolePrefab;
    public GameObject LightningPrefab;

    public GameObject lightFlashGO;

    // Bullet force
    public float bulletForce = 10f;

    [HideInInspector] public int health;

    public int max_health;

    public GameObject shieldPrefab;
    public GameObject orbitShieldPrefab;

    public List<GameObject> shields;

    public int shieldMaxLife = 3;

    public float shieldSize = 90f; // How wide the arc is (in degrees)

    public int maxShields = 1;

    public Transform shieldContainer;
    
    [Header("Shield Colors")]
    public Color firstShieldColorMax = new Color(0.396f, 0.815f, 0.925f, 1f); // Blue
    public Color firstShieldColorMin = new Color(0.516f, 0.044f, 0.547f, 1f); // Purple
    public Color secondShieldColorMax = new Color(0.925f, 0.396f, 0.396f, 1f); // Red
    public Color secondShieldColorMin = new Color(0.547f, 0.044f, 0.516f, 1f); // Dark Purple


    private Animator animator;
    private Coroutine animRoutine;

    [Header("Animation Timing")]
    public float castTime = 0.5f;
    public float damageTime = 0.7f;

    private readonly string idleAnim = "Player_Idle";
    private readonly string castAnim = "Player_Cast";
    private readonly string damageAnim = "Player_Damage";

    private bool isInvincible;

    private bool casting = false;

    public TMP_Text testText;
    
    // Track shield types (0 = first shield, 1 = second shield)
    private Dictionary<GameObject, int> shieldTypes = new Dictionary<GameObject, int>();
    
    // Track if we have shields of each type active
    private bool hasFirstShield = false;
    private bool hasSecondShield = false;
    
    // Track the actual shield GameObjects for each type
    private GameObject currentFirstShield = null;
    private GameObject currentSecondShield = null;
    
    // Wormhole system
    private GameObject currentWormhole = null;
    private bool hasWormhole = false;

    // Teleport system
    private Vector3 originalPosition;
    private bool isTeleporting = false;
    private Coroutine teleportCoroutine;
    public float teleportDuration = 3f; // How long to stay at teleported position

    // Break shields system
    private bool shieldsBroken = false;
    private Coroutine breakShieldsCoroutine;
    public float breakShieldsDuration = 5f; // How long shields remain broken

    // Vengeance system
    private bool vengeanceActive = false;
    private Coroutine vengeanceCoroutine;
    public float vengeanceDuration = 3f; // How long vengeance invincibility lasts
    public int vengeanceBasePoints = 5; // Base points gained per hit during vengeance
    private int vengeanceHitCount = 0; // Number of hits during vengeance period



    // Get mouse position in world space using the new Input System
    public Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseScreenPos3 = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z - transform.position.z));
        return Camera.main.ScreenToWorldPoint(mouseScreenPos3);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        health = max_health;
        PlayIdleAnim();
    }

    // Update is called once per frame
    void Update()
    {
        testText.text = "Casting: " + casting;
        // Check for left mouse button click using the new Input System
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.timeScale > 0f && !casting)
        {
            print ("Current upgrade: " + UpgradeManager.Instance.GetCurrentUpgrade());
            if (UpgradeManager.Instance == null) {
                Debug.LogError("UpgradeManager is not initialized");
                return;
            }
            // Get mouse position in world space
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Handle upgrade usage based on control mode
            UpgradeEffect? currentUpgrade = UpgradeManager.Instance.GetCurrentUpgrade();
            
            if (currentUpgrade == null)
            {
                // No upgrade selected - spawn regular bullet
                Transform currentSpawnPoint = GetBulletSpawnPoint();
                GameObject bullet = Instantiate(MagicBullet, currentSpawnPoint.position, Quaternion.identity);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = (mouseWorldPos - currentSpawnPoint.position).normalized;
                    rb.AddForce(direction * bulletForce, ForceMode2D.Impulse);
                }
            }
            else
            {
                // Handle selected upgrade usage (only in select-then-click mode)
                if (!UpgradeManager.Instance.immediateActivationMode)
                {
                    HandleUpgradeUsage(currentUpgrade.Value, mouseWorldPos);
                }
            }

            // Spawn light flash at current spawn point
            if (lightFlashGO != null)
            {
                Transform currentSpawnPoint = GetBulletSpawnPoint();
                Instantiate(lightFlashGO, currentSpawnPoint.position, Quaternion.identity);
            }
            
            // Play cast animation
            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(PlayCastAnim());
        }

        // Handle shield creation and movement with different keys and colors
        // Don't allow shield creation/movement if shields are broken
        if (!shieldsBroken)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.timeScale > 0f)
            {
                if (!hasFirstShield)
                {
                    CreateShield(0); // Create first shield (blue)
                }
                else
                {
                    MoveShield(0); // Move existing first shield
                }
            }
            
            // Second shield on E key (only if second shield upgrade is active)
            if (Keyboard.current.eKey.wasPressedThisFrame && Time.timeScale > 0f && 
                UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(UpgradeEffect.SecondShield))
            {
                if (!hasSecondShield)
                {
                    CreateShield(1); // Create second shield (red)
                }
                else
                {
                    MoveShield(1); // Move existing second shield
                }
            }
        }
        
        // Remove wormhole on X key (only if wormhole exists and upgrade is active)
        if (Keyboard.current.xKey.wasPressedThisFrame && Time.timeScale > 0f && 
            UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(UpgradeEffect.WormHole) &&
            hasWormhole && currentWormhole != null)
        {
            RemoveWormhole();
        }
    }

    private void PlayIdleAnim()
    {
        animator.Play(idleAnim);
    }

    private IEnumerator PlayCastAnim()
    {
        isInvincible = false;
        casting = true;
        animator.Play(castAnim);
        yield return new WaitForSeconds(castTime);
        casting = false;
        PlayIdleAnim();
    }

    private IEnumerator PlayDamageAnim()
    {
        // print ("Setting player to invinvible");
        casting = false;
        isInvincible = true;
        animator.Play(damageAnim);
        yield return new WaitForSeconds(damageTime);
        isInvincible = false;
        // print ("Player no longer invincible");
        PlayIdleAnim();
    }

    

    public bool Hit()
    {
        // Check if Vengeance upgrade is active and we're in vengeance mode
        if (vengeanceActive && UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(UpgradeEffect.Vengeance))
        {
            // Gain points instead of taking damage
            vengeanceHitCount++;
            int pointsGained = vengeanceBasePoints * (int)Mathf.Pow(2, vengeanceHitCount - 1); // Exponential doubling
            
            PointSystem ps = GameObject.FindGameObjectWithTag("GameController").GetComponent<PointSystem>();
            if (ps != null)
            {
                ps.AddPoints(pointsGained, transform.position);
            }
            
            Debug.Log($"Vengeance hit #{vengeanceHitCount}: Gained {pointsGained} points!");
            return true;
        }
        
        if (isInvincible) {
            // print("Player is invincible");
            return true;
        }
        
        health--;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PlayDamageAnim());
        
        // Activate Vengeance if the upgrade is available
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(UpgradeEffect.Vengeance))
        {
            ActivateVengeance();
        }
        
        if (health <= 0)
        {
            // print("Player DEAD");
            GameFlowManager gmf = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameFlowManager>();
            gmf.GameOver();
        }
        return true;
    }

    

    

    // Public methods for game reset
    public void ResetPlayerState()
    {
        // Reset health
        health = max_health;
        casting = false;
        
        // Remove all shields from the list and destroy them
        foreach (GameObject shield in shields)
        {
            if (shield != null)
            {
                Destroy(shield);
            }
        }
        shields.Clear();
        shieldTypes.Clear(); // Clear shield type tracking
        hasFirstShield = false;
        hasSecondShield = false;
        currentFirstShield = null;
        currentSecondShield = null;
        
        // Reset position to spawn point (if you have one)
        
        // Clean up wormhole
        DestroyWormhole();
        
        // Clean up teleport
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
            teleportCoroutine = null;
        }
        isTeleporting = false;
        
        // Clean up break shields
        if (breakShieldsCoroutine != null)
        {
            StopCoroutine(breakShieldsCoroutine);
            breakShieldsCoroutine = null;
        }
        shieldsBroken = false;
        
        // Clean up vengeance
        if (vengeanceCoroutine != null)
        {
            StopCoroutine(vengeanceCoroutine);
            vengeanceCoroutine = null;
        }
        vengeanceActive = false;
        vengeanceHitCount = 0;
        
        // Reset animation state
        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }
        
        // Reset invincibility
        isInvincible = false;
        
        // Play idle animation
        if (animator != null)
        {
            animator.Play(idleAnim);
        }
    }

    // Public getters for reset system
    public Animator GetAnimator() => animator;
    public Coroutine GetAnimRoutine() => animRoutine;
    public void SetAnimRoutine(Coroutine routine) => animRoutine = routine;
    public List<GameObject> GetShields() => shields;
    
    /// <summary>
    /// Returns whether shields are currently broken (for BreakShields upgrade)
    /// </summary>
    /// <returns>True if shields are broken, false otherwise</returns>
    public bool AreShieldsBroken() => shieldsBroken;
    
    /// <summary>
    /// Returns whether vengeance is currently active (for Vengeance upgrade)
    /// </summary>
    /// <returns>True if vengeance is active, false otherwise</returns>
    public bool IsVengeanceActive() => vengeanceActive;
    
    /// <summary>
    /// Finds the orbit shield in the shield container
    /// </summary>
    /// <returns>The orbit shield GameObject, or null if not found</returns>
    private GameObject FindOrbitShield()
    {
        foreach (GameObject shield in shields)
        {
            if (shield != null)
            {
                ShieldDeflector shieldDeflector = shield.GetComponent<ShieldDeflector>();
                if (shieldDeflector != null && shieldDeflector.IsOrbitShield())
                {
                    return shield;
                }
            }
        }
        return null;
    }
    
    // Create a shield with specified type (0 = first shield, 1 = second shield)
    private void CreateShield(int shieldType)
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // Create new shield
        GameObject newShield = Instantiate(shieldPrefab, shieldContainer);
        newShield.SetActive(true);

        ShieldDeflector shieldDeflector = newShield.GetComponent<ShieldDeflector>();
        shieldDeflector.SetShieldMaxHealth(shieldMaxLife, shieldSize);
        
        // Calculate shield angle
        Vector3 shieldDir = (mouseWorldPos - newShield.transform.position).normalized;
        float shieldAngle = Mathf.Atan2(shieldDir.y, shieldDir.x) * Mathf.Rad2Deg - 180;
        if (shieldAngle < 0f) shieldAngle += 360f;
        newShield.transform.rotation = Quaternion.AngleAxis(shieldAngle, Vector3.forward);
        
        // Track shield type
        shieldTypes[newShield] = shieldType;
        
        // Set tracking flags and store references
        if (shieldType == 0)
        {
            hasFirstShield = true;
            currentFirstShield = newShield;
        }
        else if (shieldType == 1)
        {
            hasSecondShield = true;
            currentSecondShield = newShield;
        }
        
        // Add to shields list
        shields.Add(newShield);
        
        // Check if we exceed max shields
        if (shields.Count > maxShields)
        {
            // Remove the oldest shield (first in list)
            GameObject oldestShield = shields[0];
            int oldestShieldType = shieldTypes[oldestShield];
            shields.RemoveAt(0);
            shieldTypes.Remove(oldestShield); // Remove from tracking
            
            // Update tracking flags and clear references
            if (oldestShieldType == 0)
            {
                hasFirstShield = false;
                currentFirstShield = null;
            }
            else if (oldestShieldType == 1)
            {
                hasSecondShield = false;
                currentSecondShield = null;
            }
            
            Destroy(oldestShield);
        }
    }
    
    // Get shield type (0 = first shield, 1 = second shield)
    public int GetShieldType(GameObject shield)
    {
        return shieldTypes.ContainsKey(shield) ? shieldTypes[shield] : 0;
    }
    
    // Move an existing shield to a new position
    private void MoveShield(int shieldType)
    {
        GameObject shieldToMove = null;
        
        if (shieldType == 0)
        {
            shieldToMove = currentFirstShield;
        }
        else if (shieldType == 1)
        {
            shieldToMove = currentSecondShield;
        }
        
        if (shieldToMove != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            
            // Calculate new shield angle
            Vector3 shieldDir = (mouseWorldPos - shieldToMove.transform.position).normalized;
            float shieldAngle = Mathf.Atan2(shieldDir.y, shieldDir.x) * Mathf.Rad2Deg - 180;
            if (shieldAngle < 0f) shieldAngle += 360f;
            shieldToMove.transform.rotation = Quaternion.AngleAxis(shieldAngle, Vector3.forward);
            
            // Reset shield health to full when moved
            ShieldDeflector shieldDeflector = shieldToMove.GetComponent<ShieldDeflector>();
            if (shieldDeflector != null)
            {
                shieldDeflector.SetShieldMaxHealth(shieldMaxLife, shieldSize);
            }
        }
    }
    
    // Call this when a shield is destroyed to update tracking flags
    public void OnShieldDestroyed(GameObject shield)
    {
        if (shieldTypes.ContainsKey(shield))
        {
            int shieldType = shieldTypes[shield];
            shieldTypes.Remove(shield);
            
            // Update tracking flags and clear references
            if (shieldType == 0)
            {
                hasFirstShield = false;
                currentFirstShield = null;
            }
            else if (shieldType == 1)
            {
                hasSecondShield = false;
                currentSecondShield = null;
            }
        }
    }
    
    // Wormhole methods
    private void PlaceWormhole(Vector3 position)
    {
        if (WormHolePrefab == null)
        {
            Debug.LogError("WormHolePrefab is not assigned!");
            return;
        }
        
        // Create wormhole at mouse position
        currentWormhole = Instantiate(WormHolePrefab, position, Quaternion.identity);
        hasWormhole = true;
        
        Debug.Log($"Wormhole placed at position: {position}");
    }
    
    private void MoveWormhole(Vector3 newPosition)
    {
        if (currentWormhole != null)
        {
            currentWormhole.transform.position = newPosition;
            Debug.Log($"Wormhole moved to position: {newPosition}");
        }
    }
    
    private void RemoveWormhole()
    {
        if (currentWormhole != null)
        {
            WormHoleController wormholeController = currentWormhole.GetComponent<WormHoleController>();
            if (wormholeController != null)
            {
                wormholeController.DestroyWormhole();
            }
            else
            {
                Destroy(currentWormhole);
            }
        }
        
        currentWormhole = null;
        hasWormhole = false;
        Debug.Log("Wormhole removed by player (X key)");
    }
    
    /// <summary>
    /// Gets the current spawn point for bullets (either player spawn point or wormhole)
    /// </summary>
    public Transform GetBulletSpawnPoint()
    {
        // If we have a wormhole and the WormHole upgrade is active, use wormhole position
        if (hasWormhole && currentWormhole != null && 
            UpgradeManager.Instance != null && 
            UpgradeManager.Instance.HasUpgrade(UpgradeEffect.WormHole))
        {
            return currentWormhole.transform;
        }
        
        // Otherwise use the default spawn point
        return SpawnPoint;
    }
    
    /// <summary>
    /// Called when the game resets to clean up wormhole
    /// </summary>
    public void DestroyWormhole()
    {
        if (currentWormhole != null)
        {
            WormHoleController wormholeController = currentWormhole.GetComponent<WormHoleController>();
            if (wormholeController != null)
            {
                wormholeController.DestroyWormhole();
            }
            else
            {
                Destroy(currentWormhole);
            }
        }
        
        currentWormhole = null;
        hasWormhole = false;
        Debug.Log("Wormhole destroyed during reset");
    }
    
    /// <summary>
    /// Handles immediate wormhole placement (for immediate activation mode)
    /// </summary>
    public void HandleImmediateWormholePlacement()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        if (!hasWormhole)
        {
            PlaceWormhole(mouseWorldPos);
        }
        else
        {
            MoveWormhole(mouseWorldPos);
        }
    }
    
    /// <summary>
    /// Handles immediate lightning casting (for immediate activation mode)
    /// </summary>
    public void HandleImmediateLightningCast()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        CastLightning(mouseWorldPos);
    }
    
    /// <summary>
    /// Handles immediate teleport (for immediate activation mode)
    /// </summary>
    public void HandleImmediateTeleport()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        TeleportToPosition(mouseWorldPos);
    }
    
    /// <summary>
    /// Handles immediate break shields (for immediate activation mode)
    /// </summary>
    public void HandleImmediateBreakShields()
    {
        BreakAllShields();
    }
    
    /// <summary>
    /// Casts lightning at the specified position
    /// </summary>
    /// <param name="targetPosition">The target position to cast lightning at</param>
    private void CastLightning(Vector3 targetPosition)
    {
        if (LightningPrefab == null)
        {
            Debug.LogError("LightningPrefab is not assigned!");
            return;
        }
        
        // Create lightning controller at the target position
        GameObject lightningObject = Instantiate(LightningPrefab, targetPosition, Quaternion.identity);
        LightningController lightningController = lightningObject.GetComponent<LightningController>();
        
        if (lightningController == null)
        {
            Debug.LogError("LightningPrefab does not have LightningController component!");
            Destroy(lightningObject);
            return;
        }
        
        // Cast the lightning from player to target
        lightningController.CastLightning(transform, targetPosition);
        
        Debug.Log($"Lightning cast at position: {targetPosition}");
    }
    
    /// <summary>
    /// Teleports the player to the specified position and returns them after teleportDuration seconds
    /// </summary>
    /// <param name="targetPosition">The position to teleport to</param>
    private void TeleportToPosition(Vector3 targetPosition)
    {
        if (isTeleporting)
        {
            Debug.Log("Already teleporting, cannot teleport again!");
            return;
        }
        
        // Store original position
        originalPosition = transform.position;
        
        // Teleport to target position
        transform.position = targetPosition;
        isTeleporting = true;
        
        Debug.Log($"Teleported from {originalPosition} to {targetPosition}");
        
        // Start coroutine to return after teleportDuration seconds
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
        }
        teleportCoroutine = StartCoroutine(ReturnFromTeleport());
    }
    
    /// <summary>
    /// Coroutine that returns the player to their original position after teleportDuration seconds
    /// </summary>
    private IEnumerator ReturnFromTeleport()
    {
        yield return new WaitForSeconds(teleportDuration);
        
        // Return to original position
        transform.position = originalPosition;
        isTeleporting = false;
        teleportCoroutine = null;
        
        Debug.Log($"Returned from teleport to {originalPosition}");
    }
    
    /// <summary>
    /// Breaks all regular shields (not angel shields) for a duration
    /// </summary>
    private void BreakAllShields()
    {
        if (shieldsBroken)
        {
            Debug.Log("Shields are already broken!");
            return;
        }
        
        shieldsBroken = true;
        
        // Remove all non-orbit shields from the shield container
        List<GameObject> shieldsToRemove = new List<GameObject>();
        foreach (GameObject shield in shields)
        {
            if (shield != null)
            {
                ShieldDeflector shieldDeflector = shield.GetComponent<ShieldDeflector>();
                if (shieldDeflector != null && !shieldDeflector.IsOrbitShield())
                {
                    shieldsToRemove.Add(shield);
                }
            }
        }
        
        // Remove the non-orbit shields
        foreach (GameObject shield in shieldsToRemove)
        {
            shields.Remove(shield);
            shieldTypes.Remove(shield);
            Destroy(shield);
        }
        
        // Update tracking flags
        hasFirstShield = false;
        hasSecondShield = false;
        currentFirstShield = null;
        currentSecondShield = null;
        
        // Disable orbit shield if it exists
        GameObject orbitShield = FindOrbitShield();
        if (orbitShield != null)
        {
            Collider2D orbitCollider = orbitShield.GetComponent<Collider2D>();
            if (orbitCollider != null)
            {
                orbitCollider.enabled = false;
            }
        }
        
        // Note: Enemy shields are now handled in EnemyController.Hit() method
        // which checks if BreakShields upgrade is active via AreShieldsBroken()
        
        Debug.Log($"All shields broken for {breakShieldsDuration} seconds");
        
        // Start coroutine to restore shields after duration
        if (breakShieldsCoroutine != null)
        {
            StopCoroutine(breakShieldsCoroutine);
        }
        breakShieldsCoroutine = StartCoroutine(RestoreShields());
    }
    
    /// <summary>
    /// Restores all shields after the break duration
    /// </summary>
    private IEnumerator RestoreShields()
    {
        yield return new WaitForSeconds(breakShieldsDuration);
        
        // Re-enable orbit shield if it exists
        GameObject orbitShield = FindOrbitShield();
        if (orbitShield != null)
        {
            Collider2D orbitCollider = orbitShield.GetComponent<Collider2D>();
            if (orbitCollider != null)
            {
                orbitCollider.enabled = true;
            }
        }
        
        // Note: Enemy shields are automatically restored when BreakShields ends
        // since EnemyController.Hit() will no longer see AreShieldsBroken() as true
        
        shieldsBroken = false;
        breakShieldsCoroutine = null;
        
        Debug.Log("All shields restored");
    }
    
    /// <summary>
    /// Activates vengeance mode when player takes damage
    /// </summary>
    private void ActivateVengeance()
    {
        if (vengeanceActive)
        {
            Debug.Log("Vengeance already active!");
            return;
        }
        
        vengeanceActive = true;
        vengeanceHitCount = 0;
        
        Debug.Log($"Vengeance activated for {vengeanceDuration} seconds");
        
        // Start coroutine to deactivate vengeance after duration
        if (vengeanceCoroutine != null)
        {
            StopCoroutine(vengeanceCoroutine);
        }
        vengeanceCoroutine = StartCoroutine(DeactivateVengeance());
    }
    
    /// <summary>
    /// Deactivates vengeance mode after the duration
    /// </summary>
    private IEnumerator DeactivateVengeance()
    {
        yield return new WaitForSeconds(vengeanceDuration);
        
        vengeanceActive = false;
        vengeanceHitCount = 0;
        vengeanceCoroutine = null;
        
        Debug.Log("Vengeance deactivated");
    }
    
    /// <summary>
    /// Handles the usage of a selected upgrade (for select-then-click mode)
    /// </summary>
    /// <param name="upgradeEffect">The upgrade effect to use</param>
    /// <param name="mouseWorldPos">The mouse position in world space</param>
    private void HandleUpgradeUsage(UpgradeEffect upgradeEffect, Vector3 mouseWorldPos)
    {
        // Use the unified active upgrade system
        bool success = UpgradeManager.Instance.UseActiveUpgrade(upgradeEffect);
        
        if (success)
        {
            // Handle the specific upgrade effect
            switch (upgradeEffect)
            {
                case UpgradeEffect.Blast:
                    Instantiate(BlastPrefab, mouseWorldPos, Quaternion.identity);
                    break;
                    
                case UpgradeEffect.WormHole:
                    if (!hasWormhole)
                    {
                        PlaceWormhole(mouseWorldPos);
                    }
                    else
                    {
                        MoveWormhole(mouseWorldPos);
                    }
                    break;
                    
                case UpgradeEffect.Lightning:
                    CastLightning(mouseWorldPos);
                    break;
                    
                case UpgradeEffect.Teleport:
                    TeleportToPosition(mouseWorldPos);
                    break;
                    
                case UpgradeEffect.BreakShields:
                    BreakAllShields();
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled upgrade effect: {upgradeEffect}");
                    break;
            }
            
            // Deselect the upgrade after successful use
            UpgradeManager.Instance.SetCurrentUpgradeIndex(-1);
        }
        else
        {
            // Upgrade failed (likely on cooldown) - don't deselect
            Debug.Log($"Failed to use upgrade {upgradeEffect}");
        }
    }
}   

