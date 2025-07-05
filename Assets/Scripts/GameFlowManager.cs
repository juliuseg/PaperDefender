using UnityEngine;
using System;

public class GameFlowManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject winMenu;
    public GameObject winMenu2;

    [Header("Upgrade System")]
    public UpgradeUIController upgradeUIController;

    [Header("Game Systems")]
    public Player_Controller playerController;
    public PointSystem pointSystem;
    public UIController uiController;
    public GoblinSpawner goblinSpawner;

    public int round = 1;
    
    // Events
    public event Action<int> OnRoundStart; // Event triggered when a new round starts

    void Start()
    {
        //HideAllMenus();
        pointSystem.ResetPoints(GetRoundMaxPoints()); // Assuming 10 is the initial max points
    }

    public int GetRoundMaxPoints()
    {
        return 90 + Mathf.RoundToInt(Mathf.Pow(round,2f))*10;
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        IsPaused = true;
        ShowPauseMenu();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        HidePauseMenu();
    }

    public void ShowPauseMenu()
    {
        if (AnyBlockingMenuActive()) return;
        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (winMenu != null) winMenu.SetActive(false);
        if (winMenu2 != null) winMenu2.SetActive(false);
    }

    public void HidePauseMenu()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        
    }

    public void GameOver(){
        Time.timeScale = 0f;
        IsPaused = false;
        ShowGameOverMenu();

    }

    public void WinLevel(){
        Time.timeScale = 0f;
        IsPaused = false;
        ShowWinMenu1();
    }

    public void ShowGameOverMenu()
    {
        HideAllMenus();
        if (gameOverMenu != null) gameOverMenu.SetActive(true);
        
    }

    public void ShowWinMenu1()
    {
        HideAllMenus();
        if (winMenu != null) winMenu.SetActive(true);
        
    }

    public void ShowWinMenu2()
    {
        if (winMenu2 != null) winMenu2.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = false;
        
        // Tell the upgrade UI controller to show upgrade selection
        if (upgradeUIController != null)
        {
            upgradeUIController.ShowUpgradeSelection();
        }
    }

    public void CloseWinMenusAndRestart()
    {
        if (winMenu != null) winMenu.SetActive(false);
        if (winMenu2 != null) winMenu2.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        
        // Restart the game after upgrade is applied
        RestartGame();
    }

    public void CloseGameOverAndRestart()
    {
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        RestartGame();
    }

    public void HideAllMenus()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverMenu != null) gameOverMenu.SetActive(false);
        if (winMenu != null) winMenu.SetActive(false);
        if (winMenu2 != null) winMenu2.SetActive(false);
    }

    private bool AnyBlockingMenuActive()
    {
        bool gameOverActive = gameOverMenu != null && gameOverMenu.activeSelf;
        bool win1Active = winMenu != null && winMenu.activeSelf;
        bool win2Active = winMenu2 != null && winMenu2.activeSelf;
        return gameOverActive || win1Active || win2Active;
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pauseMenu != null && pauseMenu.activeSelf && !AnyBlockingMenuActive())
            {
                Resume();
            }
            else if (!AnyBlockingMenuActive())
            {
                Pause();
            }
        }
        
        // Restart level with R key
        if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame && Time.timeScale > 0f)
        {
            RestartGame();
        }

        // print ("Time.timeScale: " + Time.timeScale);
    }

    public void OnPauseButtonPressed()
    {
        if (!AnyBlockingMenuActive())
            Pause();
    }

    public void OnResumeButtonPressed()
    {
        if (pauseMenu != null && pauseMenu.activeSelf)
            Resume();
    }

    public void OnWinMenu1NextButton()
    {
        if (winMenu != null) winMenu.SetActive(false);
        ShowWinMenu2();
    }

    // public void OnWinMenu2CloseButton(int upgrade_index)
    // {
    //     print("OnWinMenu2CloseButton: " + upgrade_index);
        
    //     // Tell the upgrade UI controller that an upgrade was selected
    //     if (upgradeUIController != null)
    //     {
    //         upgradeUIController.OnUpgradeSelected(upgrade_index);
    //     }
        
    //     CloseWinMenusAndRestart();
    // }

    public void OnGameOverRestartButton()
    {
        CloseGameOverAndRestart();
    }

    private void RestartGame()
    {
        // Instead of reloading the scene, reset all game systems
        ResetAllGameSystems();
        
        // Trigger round start event
        OnRoundStart?.Invoke(round);
    }

    private void ResetAllGameSystems()
    {
        // Debug.Log("Resetting all game systems...");
        
        // 1. Reset Game State
        Time.timeScale = 1f;
        IsPaused = false;
        HideAllMenus();
        
        // 2. Reset Player
        if (playerController != null)
        {
            ResetPlayer();
        }
        
        // 3. Reset Points
        if (pointSystem != null)
        {
            ResetPoints();
        }
        
        // 4. Reset UI
        if (uiController != null)
        {
            uiController.UpdateHearts();
        }
        
        // 5. Reset Enemies
        if (goblinSpawner != null)
        {
            ResetEnemies();
        }
        
        // 6. Reset Upgrade System (optional - you can choose to keep upgrades)
        ResetUpgradeSystem();
        
        // Debug.Log("Game reset complete!");
    }

    private void ResetPlayer()
    {
        // Use the player's own reset method
        playerController.ResetPlayerState();
        
        // Clean up orbit shield
        PlayerUpgradeHandler playerUpgradeHandler = playerController.GetComponent<PlayerUpgradeHandler>();
        if (playerUpgradeHandler != null)
        {
            playerUpgradeHandler.DestroyOrbitShield();
        }
        
        // Debug.Log("Player reset complete");
    }

    private void ResetPoints()
    {
        // Reset to initial values (you might want to make these configurable)
        pointSystem.ResetPoints(GetRoundMaxPoints()); // Assuming 10 is the initial max points
        // Debug.Log("Points reset complete");
    }

    private void ResetEnemies()
    {
        // Destroy all existing enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        // Destroy all player bullets
        BulletCont[] bullets = FindObjectsByType<BulletCont>(FindObjectsSortMode.None);
        foreach (BulletCont bullet in bullets)
        {
            if (bullet != null && bullet.gameObject != null)
            {
                Destroy(bullet.gameObject);
            }
        }
        
        // Clear all angel shields
        AngelUpgradeHandler angelHandler = GetComponent<AngelUpgradeHandler>();
        if (angelHandler != null)
        {
            angelHandler.ClearAllAngels();
        }
        
        // Restart the spawner
        goblinSpawner.RestartSpawner();
        
        // Debug.Log("Enemies reset complete");
    }

    private void ResetUpgradeSystem()
    {
        // Option 1: Keep upgrades (recommended for roguelike progression)
        // Do nothing - upgrades persist between levels
        
        
        // Debug.Log("Upgrade system reset complete (upgrades preserved)");
    }
}
