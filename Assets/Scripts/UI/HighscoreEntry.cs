using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HighscoreEntry : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text scoreText;
    public TMP_Text messageText;

    public void Init(ScoreEntry scoreEntry)
    {
        playerNameText.text = scoreEntry.player;
        scoreText.text = scoreEntry.score.ToString();
        messageText.text = $"\"{scoreEntry.message}\"";
    }
}
