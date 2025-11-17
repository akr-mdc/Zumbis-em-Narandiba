using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    public void ContinueToGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
