using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemiesDefeatedText;

    private int defeatedEnemies = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Atualiza a vida do Player
    public void UpdatePlayerHealth(int current, int max)
    {
        playerHealthText.text = $"Vida: {current}/{max}";
    }

    // Quando um inimigo morrer
    public void AddDefeatedEnemy()
    {
        defeatedEnemies++;
        enemiesDefeatedText.text = $"Inimigos derrotados: {defeatedEnemies}";
    }
}
