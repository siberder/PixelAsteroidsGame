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
    public List<Animator> screens = new List<Animator>();


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

    public Animator GetScreenByName(string screenName) => screens.Find(x => x.name == screenName);

    public void ShowScreen(string screenName) => ShowScreen(GetScreenByName(screenName));

    public void ShowScreen(Animator screen)
    {
        foreach (var s in screens)
        {
            SetScreenVisible(s, screen == s);
        }
    }

    public void ShowMenuScreen() => ShowScreen("Menu");
    public void ShowGameUI() => ShowScreen("Game");
    public void ShowHighscores() => ShowScreen("Highscores");
    public void ShowGameOverScreen() => ShowScreen("GameOver");

    void SetScreenVisible(Animator animator, bool visible)
    {
        animator.SetBool("Visible", visible);
    }

    public void UI_StartButton()
    {
        LevelController.Instance.SkipIntro = true;
    }

    public void UI_GameOver_OK()
    {
        ShowHighscores();
    }

    public void UI_HighScores_NewGame()
    {
        LevelController.Instance.ShowMenuIntro();
    }

    public void UI_Menu_Highscores()
    {
        ShowHighscores();
    }
}
