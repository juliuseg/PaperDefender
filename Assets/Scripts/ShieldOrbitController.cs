using UnityEngine;

public class ShieldOrbitController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate around the Z-axis by the rotation speed
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
