using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
    [Header("References")]
    public TMP_Text scoreText;
    public RectTransform livesImagesRoot;
    public Image liveImagePrefab;

    [Header("Screens")]
    public Animator menuScreen;
    public Animator gameScreen;


    List<Image> livesImages = new List<Image>();

    bool firstLaunch = true;

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

    public void SetMenuVisible(bool visible)
    {
        SetScreenVisible(menuScreen, visible);
    }

    public void SetGameScreenVisible(bool visible)
    {
        SetScreenVisible(gameScreen, visible);
    }

    void SetScreenVisible(Animator animator, bool visible)
    {
        animator.SetBool("Visible", visible);
    }

    public void UI_StartButton()
    {
        if(firstLaunch)
        {
            firstLaunch = false;
            LevelController.Instance.SkipIntro = true;
        }
        else
        {
            LevelController.Instance.StartNewGame();
        }
    }
}
