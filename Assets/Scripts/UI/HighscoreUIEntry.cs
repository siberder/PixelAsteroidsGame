using TMPro;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class HighscoreUIEntry : MonoBehaviour
{
    public TMP_Text placeText;
    public TMP_Text playerNameText;
    public TMP_Text scoreText;
    public TMP_Text messageText;

    public void Init(ScoreEntry scoreEntry, bool isPlayer = false)
    {
        placeText.text = $"{scoreEntry.place}.";
        playerNameText.text = scoreEntry.player;
        scoreText.text = scoreEntry.score.ToString();
        messageText.text = (!string.IsNullOrEmpty(scoreEntry.message))
            ? $"\"{scoreEntry.message}\""
            : string.Empty;

        if (isPlayer)
        {
            playerNameText.color = Color.magenta;
        }
    }
}
