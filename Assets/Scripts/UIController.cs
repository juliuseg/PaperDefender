using UnityEngine;
using TMPro;
using UnityEngine.UI; // Needed for Canvas

public class UIController : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform heartsParent;
    public float heartXOffset = 40f;
    public Player_Controller playerController;

    private GameObject[] hearts;
    private int lastHealth = -1; // Track previous health value

    public TextMeshProUGUI roundText;

    private Canvas rootCanvas;

    void Awake()
    {
        // Find the root canvas (assumes this UI is under a Canvas)
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void UpdateHearts()
    {
        if (playerController == null) return;
        
        int currentHealth = playerController.health;
        if (currentHealth < 0) currentHealth = 0;
        
        // Only update if health has changed
        if (currentHealth != lastHealth)
        {
            // Remove old hearts
            if (hearts != null)
            {
                foreach (var h in hearts)
                {
                    if (h != null) Destroy(h);
                }
            }

            // Spawn new hearts
            hearts = new GameObject[currentHealth];
            for (int i = 0; i < currentHealth; i++)
            {
                // Instantiate as child of heartsParent
                GameObject heart = Instantiate(heartPrefab, heartsParent);
                // Set anchoredPosition (not world position!)
                RectTransform rt = heart.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = new Vector2(i * heartXOffset, 0);
                hearts[i] = heart;
            }
            
            lastHealth = currentHealth;
        }
    }

    void Update()
    {
        UpdateHearts();

        GameFlowManager gmf = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameFlowManager>();
        roundText.text = gmf.round.ToString();
    }
} 