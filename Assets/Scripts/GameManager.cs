using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Text scoreText;
    int m_score;

    public Board GetGameBoard() {
        GameObject boardObj = GameObject.FindGameObjectWithTag("Board");
        return boardObj == null ? null : boardObj.GetComponent<Board>();
    }
    public void AddScore(int score) {
        m_score += score;
        UpdateScoreText();
    }

    public void SetScore(int score) {
        m_score = score;
        UpdateScoreText();
    }
    void UpdateScoreText() {
        scoreText.text = "Score: " + m_score;
    }
    // Start is called before the first frame update
    void Start()
    {
        m_score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
