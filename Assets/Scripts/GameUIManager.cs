using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    public GameObject victoryScreen;
    public GameObject defeatScreen;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        victoryScreen.SetActive(false);
        defeatScreen.SetActive(false);
    }

    // Chamado pela condição de vitória
    public void ShowVictory()
    {
        victoryScreen.SetActive(true);
        Time.timeScale = 0f; // Pausa o jogo
    }

    // Chamado quando o player morre
    public void ShowDefeat()
    {
        defeatScreen.SetActive(true);
        Time.timeScale = 0f; // Pausa o jogo
    }

    // Botão de Reinício
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
