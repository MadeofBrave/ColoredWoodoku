using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scores : MonoBehaviour
{
    [System.Serializable]
    private class BestScoreData
    {
        public int Score;
    }

    private BestScoreData bestScores_ = new BestScoreData();
    private int currentScores_;
    private string bestScoreKey_ = "bsdat";

    public Text scoreText;

    private void Awake()
    {
        if (BinaryDataSystem.Exist(bestScoreKey_))
        {
            StartCoroutine(ReadDataFile());
        }
    }

    private IEnumerator ReadDataFile()
    {
        bestScores_ = BinaryDataSystem.Read<BestScoreData>(bestScoreKey_);
        yield return new WaitForEndOfFrame();
        Debug.Log("Read Best Scores= " + bestScores_.Score);
    }

    void Start()
    {
        currentScores_ = 0;
        UpdateScoreText();
    }

    private void OnEnable()
    {
        GameEvents.AddScores += AddScores;
        GameEvents.GameOver += SaveBestScore;

    }

    private void OnDisable()
    {
        GameEvents.AddScores -= AddScores;
        GameEvents.GameOver -= SaveBestScore;
    }

    private void AddScores(int score)
    {
        currentScores_ += score;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = currentScores_.ToString();
    }

    private void SaveBestScore(bool newBestScore)
    {
        BinaryDataSystem.Save<BestScoreData>(bestScores_, bestScoreKey_);
    }
}