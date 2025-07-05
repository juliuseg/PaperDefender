using UnityEngine;

public class UpgradeEffectExecutor : MonoBehaviour
{
    public void ExecuteEffect(UpgradeEffect effect)
    {
        Player_Controller playerController = FindFirstObjectByType<Player_Controller>();
        if (playerController == null)
        {
            Debug.LogError("Player_Controller not found for upgrade effect");
            return;
        }
        
        switch (effect)
        {
            case UpgradeEffect.Blast:
                HandleBlast(playerController);
                break;
                
            case UpgradeEffect.WormHole:
                HandleWormHole(playerController);
                break;
                
            case UpgradeEffect.Lightning:
                HandleLightning(playerController);
                break;
                
            case UpgradeEffect.Teleport:
                HandleTeleport(playerController);
                break;
                
            case UpgradeEffect.BreakShields:
                HandleBreakShields(playerController);
                break;
                
            default:
                Debug.LogWarning($"Unhandled upgrade effect: {effect}");
                break;
        }
    }
    
    private void HandleBlast(Player_Controller playerController)
    {
        Vector3 blastPosition = playerController.GetMouseWorldPosition();
        GameObject blastPrefab = playerController.BlastPrefab;
        if (blastPrefab != null)
        {
            Instantiate(blastPrefab, blastPosition, Quaternion.identity);
        }
    }
    
    private void HandleWormHole(Player_Controller playerController)
    {
        playerController.HandleImmediateWormholePlacement();
    }
    
    private void HandleLightning(Player_Controller playerController)
    {
        playerController.HandleImmediateLightningCast();
    }
    
    private void HandleTeleport(Player_Controller playerController)
    {
        playerController.HandleImmediateTeleport();
    }
    
    private void HandleBreakShields(Player_Controller playerController)
    {
        playerController.HandleImmediateBreakShields();
    }
} 