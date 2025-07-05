using UnityEngine;

public class BulletCont : MonoBehaviour
{

    public string tagToHit;

    private bool hasHit = false; 

    private int hitMultiplier = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 3f);

        // Give rotational force to the bullet
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddTorque(10f, ForceMode2D.Impulse);
        }

        hitMultiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check for regular shields
        if (other.gameObject.CompareTag("Shield") ){
            bool hit = other.GetComponent<ShieldDeflector>().Hit(transform.position, tagToHit);
            if (hit)
            {
                if (tagToHit == "Player"){
                    PointSystem ps = GameObject.FindGameObjectWithTag("GameController").GetComponent<PointSystem>();
                    ps.AddPoints(10, transform.position);
                }
                Destroy(gameObject);
                hasHit = true;
                return;
            }       
        }
        
        // Check for angel shields
        if (other.gameObject.CompareTag("Angel") ){
            AngelController angel = other.GetComponent<AngelController>();
            if (angel != null)
            {
                bool hit = angel.Hit(transform.position, tagToHit);
                if (hit)
                {
                    Destroy(gameObject);
                    hasHit = true;
                    return;
                }
            }       
        }
        
        if (other.gameObject.CompareTag(tagToHit))
        {

            if (tagToHit == "Enemy"){
                bool hit = other.gameObject.GetComponent<EnemyController>().Hit(hitMultiplier);
                if (!UpgradeManager.Instance.HasUpgrade(UpgradeEffect.ThroughEnemies)){
                    print ("ThroughEnemies upgrade not active");
                    Destroy(gameObject);
                    hasHit = true;
                } else {
                    if (hit){
                        hitMultiplier = hitMultiplier*2;
                    }
                }
            }
            else if (tagToHit == "Player"){
                if (other.gameObject.GetComponent<Player_Controller>().Hit()){
                    Destroy(gameObject);
                    hasHit = true;
                }
            }
        }
    }
}
