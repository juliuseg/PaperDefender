using UnityEngine;

// Attach this to Enemy GameObjects to handle enemy-related upgrades
public class EnemyUpgradeHandler : MonoBehaviour
{
    private EnemyController enemyController;
    private SpriteRenderer spriteRenderer;
    
    [Header("Shiny Upgrade")]
    public float shinyChanceValue;

    public GameObject shinyLight;

    [HideInInspector]
    public bool isShiny = false;
    
    void Start()
    {
        ApplyEnemyUpgrades();
    }
    
    void ApplyEnemyUpgrades()
    {
        // Check for shiny chance upgrade
        if (UpgradeManager.Instance.HasUpgrade(UpgradeEffect.Shiny))
        {
            if (Random.Range(0f, 1f) < shinyChanceValue)
            {
                MakeEnemyShiny();
            }
        }
    }
    
    void MakeEnemyShiny()
    {
        if (shinyLight != null)
        {
            isShiny = true;
            shinyLight.SetActive(true);
            Debug.Log("Shiny enemy spawned!");
        }
        else
        {
            Debug.LogError("Shiny light is null");
        }
    }
} 