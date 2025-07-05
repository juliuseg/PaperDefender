using UnityEngine;

public class BlastVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Blast Settings")]
    public float blastDuration = 1f;
    public float maxscale = 5f;
    public float colorPow = 2f;
    public float scalePow = 1f;

    private float blastTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        blastTimer += Time.deltaTime;
        float progress = blastTimer / blastDuration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Update scale and alpha based on progress
        float scaleValue = Mathf.Pow(progress, scalePow) * maxscale;
        transform.localScale = new Vector3(scaleValue, scaleValue, 1);

        Color currentColor = sr.color;
        currentColor.a = 1f - Mathf.Pow(progress, colorPow);
        sr.color = currentColor;

        sr.enabled = true;
    }
}
