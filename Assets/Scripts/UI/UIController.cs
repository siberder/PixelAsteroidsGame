using System;
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

    [Header("Higscores")]
    public ScrollRect scoresScroll;
    public HighscoreEntry highscoreEntryPrefab;
    public TMP_Text highscoresLoadingText;
    public string loadingStr = "Loading...";
    public string errorStr = "failed loading :(";
    List<HighscoreEntry> spawnedHighscoreEntries = new List<HighscoreEntry>();

    [Header("Screens")]
    public List<Animator> screens = new List<Animator>();

    Dictionary<string, Animator> screensCache = new Dictionary<string, Animator>();

    List<Image> livesImages = new List<Image>();

    bool firstLaunch = true;

    public bool LoadingHighscores
    {
        get => GetScreenByName("Highscores").GetBool("LoadingHighscores");
        set
        {
            GetScreenByName("Highscores").SetBool("LoadingHighscores", value);
            if (value)
            {
                highscoresLoadingText.text = loadingStr;
            }
        }
    }

    private void Awake()
    {
        foreach (var screen in screens)
        {
            screensCache.Add(screen.name, screen);
        }
    }

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

    public Animator GetScreenByName(string screenName) => screensCache[screenName];    

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
    public void ShowHighscores()
    {
        ShowScreen("Highscores");
        HighscoresManager.Instance.LoadHighscores();
    }

    public void ShowGameOverScreen() => ShowScreen("GameOver");

    void SetScreenVisible(Animator animator, bool visible)
    {
        animator.SetBool("Visible", visible);
    }

    internal void SetHighscores(ScoreEntries scoreEntries)
    {
        LoadingHighscores = false;
        if (scoreEntries.entries != null)
        {
            foreach (var scoreEntry in spawnedHighscoreEntries)
            {
                Destroy(scoreEntry.gameObject);
            }

            spawnedHighscoreEntries.Clear();

            foreach (var entry in scoreEntries.entries)
            {
                var scoreEntry = Instantiate(highscoreEntryPrefab, scoresScroll.content);
                scoreEntry.Init(entry);
                spawnedHighscoreEntries.Add(scoreEntry);
            }
        }
        else
        {
            highscoresLoadingText.text = errorStr;
        }
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
