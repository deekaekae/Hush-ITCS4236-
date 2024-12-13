/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject instructionsPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    public enum GameState { Title, Playing, Success, Failure }
    private GameState currentState;

    private PriestController priestController;
    private SurvivorController[] survivors;
    private PlayerController playerController;

    private float priestTouchTime = 0f;
    private const float PriestTouchThreshold = 5f;

    void Start()
    {
        SetGameState(GameState.Title);

        // Get references to game components
        priestController = FindObjectOfType<PriestController>();
        playerController = FindObjectOfType<PlayerController>();
        survivors = FindObjectsOfType<SurvivorController>();
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            CheckGameConditions();
        }
    }

    public void SetGameState(GameState state)
    {
        currentState = state;

        // Hide all panels initially
        titlePanel.SetActive(false);
        instructionsPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        switch (state)
        {
            case GameState.Title:
                titlePanel.SetActive(true);
                Time.timeScale = 0; // Pause the game
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true; // Make cursor visible
                break;
            case GameState.Playing:
                Time.timeScale = 1; // Resume the game
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
                Cursor.visible = false; // Hide the cursor
                break;
            case GameState.Success:
                winPanel.SetActive(true);
                Time.timeScale = 0; // Pause the game
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true; // Make cursor visible
                break;
            case GameState.Failure:
                losePanel.SetActive(true);
                Time.timeScale = 0; // Pause the game
                Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                Cursor.visible = true; // Make cursor visible
                break;
        }
    }

    public void StartGame()
    {
        SetGameState(GameState.Playing);
    }

    public void ShowInstructions()
    {
        instructionsPanel.SetActive(true);
        Time.timeScale = 0; // Pause the game while showing instructions
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make cursor visible
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void CheckGameConditions()
    {
        bool allHealed = true;
        foreach (var survivor in survivors)
        {
            if (!survivor.IsHealed())
            {
                allHealed = false;
                break;
            }
        }
        if (allHealed)
        {
            SetGameState(GameState.Success);
            return;
        }

        if (priestController.IsTouchingPlayer(playerController.transform))
        {
            priestTouchTime += Time.deltaTime;
            if (priestTouchTime >= PriestTouchThreshold)
            {
                SetGameState(GameState.Failure);
            }
        }
        else
        {
            priestTouchTime = 0f;
        }
    }
}
*/