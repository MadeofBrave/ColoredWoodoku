using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverPopUp : MonoBehaviour
{
    public GameObject gameOverPopUp;
    public GameObject loosePopUp;
    public GameObject newBestScorePopUp;

    void Start()
    {
        gameOverPopUp.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.GameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.GameOver -= OnGameOver;
    }

    private void OnGameOver(bool newBestScore)
    {
        gameOverPopUp.SetActive(true);
        loosePopUp.SetActive(false);
        newBestScorePopUp.SetActive(true);

    }



}