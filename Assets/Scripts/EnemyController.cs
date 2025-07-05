using UnityEngine;
using System.Collections;
using System;

public class EnemyController : MonoBehaviour
{
    // Event that gets triggered when enemy dies
    public event Action OnEnemyDeath;
    public event Action<Vector3> OnEnemyDeathWithPosition; // New event with position
    
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Points gotten")]
    public int points = 10;
    public int pointsShiny = 50;

    [Header("Animation Timing")]
    public float idleWaitMin = 1f;
    public float idleWaitMax = 5f;
    public float pickUpTime = 0.7f;
    public float throwTime = 0.5f;
    public float surprisedTime = 1f;
    public float deadTime = 0.5f;

    [Header("Throw Timing")]
    [Range(0f, 1f)]
    public float throwAttackPoint = 0.5f; // 0=start, 1=end, 0.5=halfway

    [Header("Attack Settings")]
    public GameObject stonePrefab;
    public float throwForce = 10f;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public Vector2 boundsMin = new Vector2(-4f, -4f);
    public Vector2 boundsMax = new Vector2(4f, 4f);
    public float minVelocityThreshold = 0.1f;
    public GameObject pointFloaterPrefab;
    private Vector3? destination = null;
    private bool arrived = false;

    private readonly string idleAnim = "Goblin_Idle";
    private readonly string pickUpAnim = "Goblin_PickingUp";
    private readonly string throwAnim = "Goblin_Throwing";
    private readonly string walkAnim = "Goblin_Walk";
    private readonly string surprisedAnim = "Goblin_Surprised";
    private readonly string deadAnim = "Goblin_Dead";

    private bool canTakeDamage = false;
    private Coroutine attackRoutine;
    private Coroutine surprisedRoutine;
    private bool isDead = false;
    private bool isSurprised = false;

    public float zOffset = 0.1f;
    private Canvas canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (pointFloaterPrefab == null)
        {
            Debug.LogError("PointFloater prefab not found!");
        }
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        
        // Only start attack loop if already at destination
        if (arrived || destination == null)
            attackRoutine = StartCoroutine(PlayAnimationsLoop());
    }

    public void SetDestination(Vector3 dest)
    {
        destination = dest;
        arrived = false;
        rb = GetComponent<Rigidbody2D>();

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        StartCoroutine(MoveToDestination());
    }

    IEnumerator MoveToDestination()
    {
        animator = GetComponent<Animator>();
        animator.Play(walkAnim); // Play walk while moving
        
        while (Vector2.Distance(transform.position, destination.Value) > 0.1f)
        {
            // Calculate direction to destination
            Vector2 direction = (destination.Value - transform.position).normalized;
            
            // Move using Rigidbody2D
            rb.linearVelocity = direction * walkSpeed;
            
            yield return null;
        }
        
        // Stop movement when arrived
        rb.linearVelocity = Vector2.zero;
        arrived = true;
        attackRoutine = StartCoroutine(PlayAnimationsLoop());
    }

    IEnumerator PlayAnimationsLoop()
    {
        while (true)
        {
            // Check if dead or surprised - if so, stop the loop
            if (isDead || isSurprised)
            {
                yield break;
            }
            
            // Idle
            animator.Play(idleAnim);
            float idleWait = UnityEngine.Random.Range(idleWaitMin, idleWaitMax);
            yield return new WaitForSeconds(idleWait);

            // Check again after idle
            if (isDead || isSurprised)
            {
                yield break;
            }

            // Pick up
            canTakeDamage = true;
            animator.Play(pickUpAnim);
            yield return new WaitForSeconds(pickUpTime);
            

            // Check again after pick up
            if (isDead || isSurprised)
            {
                yield break;
            }

            // Throw
            animator.Play(throwAnim);
            float attackTime = throwTime * throwAttackPoint;
            float afterAttackTime = throwTime - attackTime;
            yield return new WaitForSeconds(attackTime);
            
            
            // Only attack if not dead or surprised
            if (!isDead && !isSurprised)
            {
                Attack();
            }
            
            yield return new WaitForSeconds(afterAttackTime);
            canTakeDamage = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the enemy is on the right side of the screen, flip the sprite using SpriteRenderer.flipX
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = transform.position.x > 0;
        }

        // Make the z position be related to the y position. This is to avoid z-fighting.
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y*0.001f+zOffset);
    }

    public void Suprise(){
        arrived = true;
        destination = null;
        
        // Stop any ongoing coroutines
        StopAllCoroutines();
        
        // Start velocity monitoring
        StartCoroutine(MonitorVelocityAndBounds());
        
        // Play surprised animation
        if (!isDead)
        {
            isSurprised = true;
            surprisedRoutine = StartCoroutine(PlaySurprisedAnimation());
        }
    }

    IEnumerator PlaySurprisedAnimation()
    {
        canTakeDamage = true;
        animator.Play(surprisedAnim);
        yield return new WaitForSeconds(surprisedTime); // Adjust time as needed for surprised animation
        canTakeDamage = false;
        isSurprised = false;
        
        // Only continue if not dead
        if (!isDead)
        {
            attackRoutine = StartCoroutine(PlayAnimationsLoop());
        }
    }

        public bool Hit(int hitMultiplier = 1)
    {
        print("Enemy hit");

        // Check if BreakShields upgrade is active - if so, enemy can always take damage
        bool breakShieldsActive = false;
        if (UpgradeManager.Instance != null)
        {
            // We need to check if the player has the BreakShields upgrade and if it's currently active
            // Since the upgrade is on the player, we need to access the player's state
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Player_Controller playerController = player.GetComponent<Player_Controller>();
                if (playerController != null)
                {
                    // We'll need to add a public property to Player_Controller to check if shields are broken
                    breakShieldsActive = playerController.AreShieldsBroken();
                }
            }
        }

        if (canTakeDamage || breakShieldsActive){
            Die(hitMultiplier);
            return true;
        }
        return false;
        
    }

    public void Die(int hitMultiplier)
    {
        isDead = true;
        
        // Trigger death events
        OnEnemyDeath?.Invoke();
        OnEnemyDeathWithPosition?.Invoke(transform.position);
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Play dead animation
        animator.Play(deadAnim);
        
        // Wait for animation then destroy
        StartCoroutine(DestroyAfterAnimation(hitMultiplier));
    }
    
    IEnumerator DestroyAfterAnimation(int hitMultiplier)
    {
        PointSystem ps = GameObject.FindGameObjectWithTag("GameController").GetComponent<PointSystem>();
        EnemyUpgradeHandler euh = GetComponent<EnemyUpgradeHandler>();
        
        if (euh != null)
        {
            if (euh.isShiny)
            {
                ps.AddPoints(pointsShiny*hitMultiplier, transform.position);
                
            } else {
                ps.AddPoints(points*hitMultiplier, transform.position);
            }

            
        } else {
            Debug.Log("Can't find EnemyUpgradeHandler");
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float fadeTimer = 0f;
        
        while (fadeTimer < deadTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = 1f - Mathf.Pow(fadeTimer / deadTime,2);
            Color currentColor = sr.color;
            currentColor.a = alpha;
            sr.color = currentColor;
            yield return null;
        }
        
        Destroy(gameObject);
    }

    public void Attack()
    {
        //print("Enemy attacks!");
        // Spawn stone and throw towards player
        if (stonePrefab != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 spawnPos = transform.position;
                GameObject stone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);
                Rigidbody2D rb = stone.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (player.transform.position - spawnPos).normalized;
                    rb.AddForce(dir * throwForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    IEnumerator MonitorVelocityAndBounds()
    {
        while (rb.linearVelocity.magnitude > minVelocityThreshold)
        {
            // Check bounds and flip velocity if needed
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 newVelocity = currentVelocity;
            
            // Check X bounds
            if (transform.position.x < boundsMin.x && currentVelocity.x < 0)
            {
                newVelocity.x = Mathf.Abs(currentVelocity.x);
            }
            else if (transform.position.x > boundsMax.x && currentVelocity.x > 0)
            {
                newVelocity.x = -Mathf.Abs(currentVelocity.x);
            }
            
            // Check Y bounds
            if (transform.position.y < boundsMin.y && currentVelocity.y < 0)
            {
                newVelocity.y = Mathf.Abs(currentVelocity.y);
            }
            else if (transform.position.y > boundsMax.y && currentVelocity.y > 0)
            {
                newVelocity.y = -Mathf.Abs(currentVelocity.y);
            }
            
            // Apply new velocity if changed
            if (newVelocity != currentVelocity)
            {
                rb.linearVelocity = newVelocity;
            }
            
            yield return null;
        }
        
        // Stop movement when velocity is low enough
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Checks if the enemy is within its own bounds (boundsMin and boundsMax)
    /// </summary>
    /// <returns>True if the enemy is within bounds, false otherwise</returns>
    public bool IsWithinBounds()
    {
        Vector3 position = transform.position;
        return position.x >= boundsMin.x && position.x <= boundsMax.x &&
               position.y >= boundsMin.y && position.y <= boundsMax.y;
    }
}
