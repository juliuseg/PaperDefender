using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Upgrade")]
public class Upgrade : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    
    [Header("Upgrade Data")]
    public UpgradeCategory category;
    public UpgradeEffect effect;
    public InteractionType interactionType;
    public int cost;
    
    [Header("Cooldown (for active upgrades)")]
    public float cooldownDuration = 5f; // Default 5 seconds
    
}

public enum UpgradeCategory
{
    Defensive,
    Offensive,
    Utility,
    Movement,
    Economy
}

public enum UpgradeEffect
{
    Blast = 0,
    IncreaseShieldSize = 1,
    IncreaseShieldMaxHealth = 2,
    SecondShield = 3,
    ThroughEnemies = 4,
    BonusPerEnemyOnScreen = 5,
    WormHole = 6,
    Angel = 7,
    Shiny = 8,
    ExtraLife = 9,
    Lightning = 10,
    OrbitShield = 11,
    Vengeance = 12,
    Teleport = 13,
    BreakShields = 14,
}

public enum InteractionType
{
    active, // Active upgrades are upgrades where you can use them in game
    passive, // Passive upgrades are upgrades that just work in the background
    one_time,
}