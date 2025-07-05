using Unity.VisualScripting;
using UnityEngine;

public class ShieldDeflector : MonoBehaviour
{
    private Player_Controller playerController; // Reference to PlayerRenderer for shield orientation

    private float shield_health;
    private float shield_max_health;
    private bool isOrbitShield = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void SetShieldMaxHealth(float maxHealth, float size)
    {
        shield_max_health = maxHealth;
        shield_health = shield_max_health;
    }

    public void SetOrbitShield(bool isOrbitShield)
    {
        this.isOrbitShield = isOrbitShield;
    }
    
    public bool IsOrbitShield()
    {
        return isOrbitShield;
    }

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Returns true if the hit is deflected by the shield
    public bool Hit(Vector2 hitWorldPos, string tagToHit)
    {
        Vector3 shieldCenter = transform.position;
        // Use the actual shield rotation for arc center
        float shieldAngle = transform.rotation.eulerAngles.z;
        float halfArc = playerController.shieldSize*1.2f / 2f;

        // Direction from shield to hit
        Vector2 hitDir = ((Vector2)hitWorldPos - (Vector2)shieldCenter).normalized;
        float hitAngle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg-180;
        if (hitAngle < 0f) hitAngle += 360f;

        float angleDiff = Mathf.DeltaAngle(shieldAngle, hitAngle);
        bool deflected = Mathf.Abs(angleDiff) <= halfArc;

        if (deflected && tagToHit == "Player"){
            Player_Controller player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Controller>();
            if (!isOrbitShield){
                shield_health--;
                if (shield_health <= 0){
                    player.OnShieldDestroyed(gameObject);
                    player.shields.Remove(gameObject);
                    Destroy(gameObject);
                }
            }
        }


        return deflected;
    }

    public float GetShieldLifePercentage()
    {
        if (shield_max_health == 0)
        {
            Debug.LogError("Shield max health is 0, avoid dividing by 0");
            return 0;
        }
        return (float)shield_health / (float)shield_max_health;
    }
}
