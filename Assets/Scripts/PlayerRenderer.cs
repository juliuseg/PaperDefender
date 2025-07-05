using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class PlayerRenderer : MonoBehaviour
{
    public float widthMultiplier;
    public GameObject wand;
    public float wandOffset;
    public float wandOffsetPositionX;

    // Arc cutout settings
    public Material MatCircleCutoutInner;
    public Material MatCircleCutoutOuter;
    public float circleAngleOffset = 0f;

    public Color shieldColorMax;
    public Color shieldColorMin;


    public float shieldLightIntensityMax = 1f;
    public float shieldLightIntensityMin = 0f;

    private Player_Controller playerController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
                
        if (wand != null && Time.timeScale > 0f)
        {
            // Get mouse position in world space using the new Input System
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseScreenPos3 = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z - transform.position.z));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3);

            Vector3 wandDir = (mouseWorldPos - transform.position).normalized;
            float wandAngle = Mathf.Atan2(wandDir.y, wandDir.x) * Mathf.Rad2Deg + wandOffset;
            if (wandAngle >= 360f) wandAngle -= 360f;
            if (wandAngle < 0f) wandAngle += 360f;
            wand.transform.rotation = Quaternion.AngleAxis(wandAngle, Vector3.forward);

            if (wandAngle > 250f || wandAngle < 70f)
            {
                wand.transform.position = new Vector3(transform.position.x + wandOffsetPositionX, wand.transform.position.y, wand.transform.position.z);
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                wand.transform.position = new Vector3(transform.position.x - wandOffsetPositionX, wand.transform.position.y, wand.transform.position.z);
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        // Set arc cutout to always be from 0 to arcCutoutAngle
        if (MatCircleCutoutInner != null && MatCircleCutoutOuter != null)
        {
            MatCircleCutoutInner.SetFloat("_StartAngle", 180f-playerController.shieldSize/2);
            MatCircleCutoutInner.SetFloat("_EndAngle", 180f+playerController.shieldSize/2);

            MatCircleCutoutOuter.SetFloat("_StartAngle", 180f-playerController.shieldSize/2-circleAngleOffset);
            MatCircleCutoutOuter.SetFloat("_EndAngle", 180f+playerController.shieldSize/2+circleAngleOffset);
        }

        // Set color and light intensity based on shield life
        
        // Get all the shields from the player controller
        List<GameObject> shields = playerController.GetShields();
        foreach (GameObject shield in shields)
        {
            ShieldDeflector shieldDeflector = shield.GetComponent<ShieldDeflector>();
            // Get percentage of shield life
            float shieldLifePercentage = shieldDeflector.GetShieldLifePercentage();
            
            // Get shield type and determine colors
            int shieldType = playerController.GetShieldType(shield);
            Color shieldColorMax, shieldColorMin;
            
            if (shieldType == 1) // Second shield (red)
            {
                shieldColorMax = playerController.secondShieldColorMax;
                shieldColorMin = playerController.secondShieldColorMin;
            }
            else // First shield (blue) or default
            {
                shieldColorMax = playerController.firstShieldColorMax;
                shieldColorMin = playerController.firstShieldColorMin;
            }
            
            // Set color based on shield life percentage
            Color shieldColor = Color.Lerp(shieldColorMin, shieldColorMax, shieldLifePercentage);
            Transform shieldInner = shield.transform.Find("ShieldInner");
            if (shieldInner != null)
            {
                shieldInner.GetComponent<SpriteRenderer>().color = shieldColor;
            } else {
                Debug.LogError("ShieldInner not found on shield");
            }
            
            // Check if shield size upgrade is active to determine which light to use
            
            bool isShieldSizeUpgradeActive = UpgradeManager.Instance.HasUpgrade(UpgradeEffect.IncreaseShieldSize);
            
            // print ("isShieldSizeUpgradeActive: " + isShieldSizeUpgradeActive);
            
            // Get the appropriate light based on shield size upgrade
            Light2D shieldLight = null;
            Transform lightSmall = shield.transform.Find("LightSmall");
            Transform lightBig = shield.transform.Find("LightBig");
            
            if (isShieldSizeUpgradeActive)
            {
                // Use big light if shield size upgrade is active
                if (lightBig != null)
                {
                    shieldLight = lightBig.GetComponent<Light2D>();
                    if (lightSmall != null)
                    {
                        lightSmall.GetComponent<Light2D>().enabled = false; // Disable small light
                    }
                    lightBig.GetComponent<Light2D>().enabled = true; // Enable big light
                } else {
                    Debug.LogError("LightBig not found on shield");
                }
            }
            else
            {
                // Use small light if shield size upgrade is not active
                if (lightSmall != null)
                {
                    shieldLight = lightSmall.GetComponent<Light2D>();
                    if (lightBig != null)
                    {
                        lightBig.GetComponent<Light2D>().enabled = false; // Disable big light
                    }
                    lightSmall.GetComponent<Light2D>().enabled = true; // Enable small light
                } else {
                    Debug.LogError("LightSmall not found on shield");
                }
            }

            // Set light intensity to be 0 if 0 and 1 if 1
            if (shieldLight != null)
            {
                // print("shieldLifePercentage: " + shieldLifePercentage + ", min: " + shieldLightIntensityMin + ", max: " + shieldLightIntensityMax);
                shieldLight.intensity = Mathf.Lerp(shieldLightIntensityMin, shieldLightIntensityMax, shieldLifePercentage);
                shieldLight.color = shieldColor;
                // print ("Setting values for shield:" + shieldLight.gameObject.name  + " intensity: " + shieldLight.intensity + " color: " + shieldColor); 

            } else {
                Debug.LogError("ShieldLight not found on shield");
            }
        }
        

    }
}
