using UnityEngine;
using System.Collections.Generic;

public class BlastController : MonoBehaviour
{
    [Header("Blast Settings")]
    public float blastRadius;
    public float blastForce;
    
    private CircleCollider2D blastCollider;
    private List<Rigidbody2D> affectedEnemies = new List<Rigidbody2D>();
    
    void Start()
    {
        print ("BlastController started, blastRadius: " + blastRadius + ", blastForce: " + blastForce);
        // Create the blast circle
        CreateBlastCircle();
        
        // Apply force to enemies in range
        ApplyBlastForce();
    }
    
    
    void CreateBlastCircle()
    {
        // Add CircleCollider2D for blast detection
        blastCollider = gameObject.AddComponent<CircleCollider2D>();
        blastCollider.isTrigger = true;
        blastCollider.radius = blastRadius;
    }
  
    
    void ApplyBlastForce()
    {
        // Find all enemies within the blast radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, blastRadius);
        
        foreach (Collider2D collider in colliders)
        {
            print ("Enemy: " + collider.gameObject.name);
            // Check if it's an enemy
            if (collider.CompareTag("Enemy"))
            {
                Rigidbody2D enemyRb = collider.GetComponent<Rigidbody2D>();
                if (enemyRb != null && !affectedEnemies.Contains(enemyRb))
                {
                    // Calculate direction from blast center to enemy
                    Vector2 direction = (collider.transform.position - transform.position).normalized;
                    
                    // Calculate distance from blast center
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    
                    
                    float normalizedDistance = Mathf.Clamp01(distance / blastRadius);
                    if (normalizedDistance == 1f) continue;
                    
                    // Apply power of 2 to get stronger effect for closer enemies
                    // Closer enemies (lower normalizedDistance) get higher force multiplier
                    float forceMultiplier = 1f -Mathf.Pow(normalizedDistance, 2f);
                    
                    // Apply force to the enemy with distance-based multiplier
                    enemyRb.AddForce(direction * blastForce * forceMultiplier, ForceMode2D.Impulse);

                    collider.GetComponent<EnemyController>().Suprise();
                    
                    // Add to affected enemies list to avoid double application
                    affectedEnemies.Add(enemyRb);
                    
                    Debug.Log($"Blast applied force to enemy: {collider.name} with force: {direction * blastForce * forceMultiplier}, distance: {distance}, multiplier: {forceMultiplier}, normalizedDistance: {normalizedDistance}");
                }
            } else {
                print ("Not enemy: " + collider.gameObject.name);
            }
        }

        print ("Blast applied force to enemies: " + affectedEnemies.Count + " Out of " + colliders.Length);
    }

} 