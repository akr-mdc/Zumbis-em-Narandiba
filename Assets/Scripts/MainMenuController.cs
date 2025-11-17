using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("IntroScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Jogo encerrado (só aparece no editor).");
    }
}
