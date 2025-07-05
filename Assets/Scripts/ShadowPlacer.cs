using UnityEngine;

public class ShadowPlacer : MonoBehaviour
{
    public SpriteRenderer sr_ref;
    private SpriteRenderer sr_own;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr_own = GetComponent<SpriteRenderer>();
        sr_own.color = new Color(0, 0, 0, 0.33f);

        transform.position = sr_ref.transform.position + new Vector3(-0.020f, -0.05f, 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        sr_own.sprite = sr_ref.sprite;
        sr_own.flipX = sr_ref.flipX;
        sr_own.flipY = sr_ref.flipY;
    }
}
