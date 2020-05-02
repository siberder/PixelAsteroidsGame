using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoSingleton<UIController>
{
    [Header("Cursor")]
    public Texture2D defaultCursor;
    public Texture2D aimCursor;

    [Header("References")]
    public TMP_Text scoreText;
    public RectTransform livesImagesRoot;
    public Image liveImagePrefab;

    [Header("Game Over")]
    public TMP_Text gameOverScoreText;
    public TMP_InputField playerNameText;
    public TMP_InputField playerMessage;

    [Header("Higscores")]
    public ScrollRect scoresScroll;
    public HighscoreUIEntry highscoreEntryPrefab;
    public TMP_Text highscoresLoadingText;
    public string loadingStr = "Loading...";
    public string errorStr = "failed loading :(";
    List<HighscoreUIEntry> spawnedHighscoreEntries = new List<HighscoreUIEntry>();

    [Header("Screens")]
    public List<Animator> screens = new List<Animator>();

    Dictionary<string, Animator> screensCache = new Dictionary<string, Animator>();

    List<Image> livesImages = new List<Image>();

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

    public Animator ShowScreen(string screenName) => ShowScreen(GetScreenByName(screenName));

    public Animator ShowScreen(Animator screen)
    {
        Animator screenAnimator = null;
        foreach (var s in screens)
        {
            SetScreenVisible(s, screen == s);
            if(screen == s)
            {
                screenAnimator = s;
            }
        }

        return screenAnimator;
    }

    public void ShowMenuScreen() => ShowScreen("Menu");
    public void ShowGameUI()
    {
        ShowScreen("Game");
        Cursor.SetCursor(aimCursor, new Vector2(0.5f, 0.5f) * new Vector2(aimCursor.width, aimCursor.height), CursorMode.ForceSoftware);
    }

    public void ShowHighscores()
    {
        ShowScreen("Highscores");
    }

    public void ShowGameOverScreen()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.ForceSoftware);

        var screen = ShowScreen("GameOver");
        var score = GameManager.Instance.Score;
        screen.SetBool("NewHighscore", score > HighscoresManager.Instance.Highscore);

        gameOverScoreText.text = score.ToString();
        playerNameText.text = HighscoresManager.Instance.PlayerName;
        playerMessage.text = HighscoresManager.Instance.PlayerMessage;
    }

    void SetScreenVisible(Animator animator, bool visible)
    {
        animator.SetBool("Visible", visible);
    }

    internal void SetHighscores(ScoreEntries scoreEntries)
    {
        if (scoreEntries != null && scoreEntries.entries != null)
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

            LoadingHighscores = false;
        }
        else
        {
            highscoresLoadingText.text = errorStr;
        }
    }

    public void UI_StartButton()
    {
        IntroAnimator.Instance.SkipIntro = true;
    }

    public void UI_GameOver_OK()
    {
        HighscoresManager.Instance.PlayerName = playerNameText.text;
        HighscoresManager.Instance.PlayerMessage = playerMessage.text;
        HighscoresManager.Instance.AddNewScore(GameManager.Instance.Score);
        ShowHighscores();
    }

    public void UI_HighScores_NewGame()
    {
        GameManager.Instance.ShowMenuIntro();
    }

    public void UI_Menu_Highscores()
    {
        HighscoresManager.Instance.LoadHighscores();
        ShowHighscores();
    }

    public void UI_RestartGame()
    {
        GameManager.Instance.StartNewGame();
    }
}
