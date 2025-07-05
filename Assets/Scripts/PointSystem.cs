using UnityEngine;
using TMPro;


public class PointSystem : MonoBehaviour
{

    public int points = 0;

    public int points_max = 0;

    public float points_multiplier; // Has to be x.x for the points to be integers

    public TextMeshProUGUI pointsText;

    public TextMeshProUGUI pointsText_max;

    public TextMeshProUGUI pointsText_multiplier;
    public GameObject pointsText_multiplier_object;

    public GameObject pointFloaterPrefab;

    private Canvas canvas;

    private void Start()
    {
        points = 0;
        pointsText.text = points.ToString();
        pointsText_max.text = points_max.ToString();
        pointsText_multiplier.text = GetPointsMultiplierText();
        canvas = FindFirstObjectByType<Canvas>();
        pointsText_multiplier_object.SetActive(false);
    }

    private string GetPointsMultiplierText()
    {
        return (points_multiplier*100).ToString();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
        {
            AddPoints(10, Vector3.zero);
        }

        int goblins_on_screen = GetComponent<GoblinSpawner>().GetEnemiesOnScreen();

        if (UpgradeManager.Instance.HasUpgrade(UpgradeEffect.BonusPerEnemyOnScreen)){
            points_multiplier = goblins_on_screen*0.1f;
            pointsText_multiplier.text = GetPointsMultiplierText();
            pointsText_multiplier_object.SetActive(true);
        } else {
            pointsText_multiplier_object.SetActive(false);
        }




    }

    public void ResetPoints(int max)
    {
        points = 0;
        points_max = max;
        pointsText_max.text = points_max.ToString();
        pointsText.text = points.ToString();
    }

    public void AddPoints(int p, Vector3 pos)
    {
        points += (int)(p * (1+points_multiplier));
        pointsText.text = points.ToString();

        GameObject floater = Instantiate(pointFloaterPrefab, canvas.transform);
        floater.GetComponent<PointFloater>().Setup(p, pos);
    

        if (points >= points_max){
            GameFlowManager gmf = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameFlowManager>();
            gmf.WinLevel();
        }
    }
}
