using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public GameObject victoryScreen;
    public GameObject defeatScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        victoryScreen.SetActive(false);
        defeatScreen.SetActive(false);
    }

    // =====================
    // TELAS
    // =====================

    public void ShowVictory()
    {
        victoryScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowDefeat()
    {
        defeatScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    // =====================
    // BOTÕES
    // =====================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
