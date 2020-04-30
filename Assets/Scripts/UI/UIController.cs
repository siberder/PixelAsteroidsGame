using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
    [Header("References")]
    public TMP_Text scoreText;
    public RectTransform livesImagesRoot;
    public Image liveImagePrefab;

    List<Image> livesImages = new List<Image>();

    public void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString();
    }

    public void UpdateLivesCount(int lives)
    {
        if(livesImages.Count < lives)
        {
            for (int i = livesImages.Count; i < lives; i++)
            {
                var img = Instantiate(liveImagePrefab, livesImagesRoot);
                livesImages.Add(img);
            }
        }

        for (int i = 0; i < livesImages.Count; i++)
        {
            livesImages[i].enabled = i < lives;
        }
    }

}
