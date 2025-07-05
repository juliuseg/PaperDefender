using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlash : MonoBehaviour
{
    public float fadeDuration = 0.2f;
    private Light2D light2D;
    private float initialIntensity;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light2D = GetComponent<Light2D>();
        if (light2D != null)
        {
            initialIntensity = Mathf.Sqrt(light2D.intensity);
        }
        else
        {
            Debug.LogWarning("No Light2D component found on LightFlash GameObject.");
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (light2D == null) return;
        timer += Time.deltaTime;
        float t = timer / fadeDuration;
        light2D.intensity = Mathf.Lerp(Mathf.Pow(initialIntensity,2), 0f, t);
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
