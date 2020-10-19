using System;
using System.Collections.Generic;
using Highscores;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
public class GameManager : MonoSingleton<GameManager>
{
    [BoxGroup("Highscores Settings")]
    public bool debugMode = false;
    
    [BoxGroup("Highscores Settings")]
    public bool forceProdServer = true;

    [BoxGroup("Highscores Settings")]
    public string testServerBaseUrl = "http://127.0.0.1:8000/";

    [BoxGroup("Highscores Settings")]
    public string serverBaseUrl = "http://127.0.0.1:8000/";
    
    [BoxGroup("Player Settings")]
    public int maxPlayerLives = 3;

    [BoxGroup("Player Settings")]
    public float playerInvincibilityTime = 3f;

    [BoxGroup("Player Settings")]
    public float playerRespawnDelay = 2f;

    [BoxGroup("Player Settings")]
    public int playerDeathReward = 100;

    [BoxGroup("Enemies Settings")]
    public float enemySpawnCooldown = 2f;

    [BoxGroup("Enemies Settings")]
    public float asteroidRandomDistanceFromTarget = 2f;

    [BoxGroup("Enemies Settings")]
    public float enemySpaceshipSpawnChance = 0.05f;

    [BoxGroup("Other Settings")]
    public float offscreenOffset = 1f;

    [Header("Prefabs")]
    public List<Asteroid> asteroidPrefabs = new List<Asteroid>();

    public List<EnemySpaceship> enemySpaceships = new List<EnemySpaceship>();

    private bool _gameOver = true;
    private float _enemySpawnCooldownLeft;
    private int _screenWidth;
    private int _screenHeight;

    [PublicAPI]
    public Bounds PlayAreaBounds { get; private set; }

    [PublicAPI]
    public Vector3 TopRightBoundCorner { get; private set; }

    [PublicAPI]
    public Vector3 BotLeftBoundCorner { get; private set; }

    public Camera MainCam { get; private set; }


    public SpaceshipController Player { get; private set; }

    private int _score;

    [PublicAPI]
    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            UIController.Instance.UpdateScoreText(_score);
        }
    }

    int _lives;

    [PublicAPI]
    public int Lives
    {
        get => _lives;
        set
        {
            _lives = value;
            UIController.Instance.UpdateLivesCount(_lives);
        }
    }

    private readonly List<Entity> _spawnedEntities = new List<Entity>();

    private void Awake()
    {
        MainCam = Camera.main;
        Player = FindObjectOfType<SpaceshipController>();

        CalculateBoundsIfNeeded();
    }

    private void Start()
    {
        ShowMenuIntro();
    }

    public void Update()
    {
        CalculateBoundsIfNeeded();

        if (!_gameOver)
        {
            if (_enemySpawnCooldownLeft <= 0)
            {
                if (Random.value > enemySpaceshipSpawnChance)
                {
                    SpawnAsteroid();
                }
                else
                {
                    SpawnEnemy();
                }

                ResetEnemySpawnCooldown();
            }
            else
            {
                _enemySpawnCooldownLeft -= Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                StartNewGame();
            }
        }
    }

    private void CalculateBoundsIfNeeded()
    {
        if (_screenWidth == Screen.width && _screenHeight == Screen.height)
            return;
        
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        PlayAreaBounds = MainCam.OrthographicBounds();
        TopRightBoundCorner = PlayAreaBounds.center + PlayAreaBounds.extents;
        BotLeftBoundCorner = PlayAreaBounds.center - PlayAreaBounds.extents;
    }

    public static void ShowMenuIntro()
    {
        IntroAnimator.Instance.ShowIntro();
    }

    public void ResetGame()
    {
        Score = 0;
        Lives = maxPlayerLives;
    }

    [Button]
    [PublicAPI]
    public void StartNewGame()
    {
        Debug.Log("Starting new game");
        ResetGame();
        DestroyEntities();
        ResetEnemySpawnCooldown();
        Player.Respawn();
        _gameOver = false;
        UIController.Instance.ShowGameUI();
        IntroAnimator.Instance.ShowingIntro = false;
        HighscoresManager.Instance.SendStartGame();
    }

    [PublicAPI]
    public void SetGameOver()
    {
        Debug.Log("Game Over");
        _gameOver = true;
        UIController.Instance.ShowGameOverScreen();

        Invoke(nameof(DestroyEntities), 0.5f);
    }

    private void ResetEnemySpawnCooldown()
    {
        _enemySpawnCooldownLeft = enemySpawnCooldown;
    }

    private void DestroyEntities()
    {
        while (_spawnedEntities.Count > 0)
        {
            var entity = _spawnedEntities[0];
            if (entity != null)
            {
                entity.DestroyEntity();
            }

            _spawnedEntities.Remove(entity);
        }
    }

    public void SetPlayerDied()
    {
        Lives--;
        RewardPlayer(playerDeathReward);

        if (Lives <= 0)
        {
            SetGameOver();
        }
        else
        {
            Invoke(nameof(RespawnPlayer), playerRespawnDelay);
        }
    }

    private void RespawnPlayer()
    {
        Player.Respawn();
    }

    [Button]
    private void SpawnAsteroid()
    {
        var asteroidPrefab =
            asteroidPrefabs[Random.Range(0, asteroidPrefabs.Count)];
        var asteroid = Instantiate(asteroidPrefab, GetRandomOffscreenPoint(), Quaternion.identity);
        asteroid.SetRandomSkin();
        asteroid.SetForceTowardPoint(Random.insideUnitSphere *
                                     asteroidRandomDistanceFromTarget);

        _spawnedEntities.Add(asteroid);
    }

    [Button]
    private void SpawnEnemy()
    {
        var enemy = Instantiate(enemySpaceships[Random.Range(0, enemySpaceships.Count)],
            GetRandomOffscreenPoint(), Quaternion.identity);
        enemy.Init();

        _spawnedEntities.Add(enemy);
    }

    public void AddSpawnedEntity(Entity entity)
    {
        _spawnedEntities.Add(entity);
    }

    public void RemoveDeadEntity(Entity entity)
    {
        if (_spawnedEntities.Contains(entity))
        {
            _spawnedEntities.Remove(entity);
        }
    }

    public void RewardPlayer(int points)
    {
        Score += points;
    }

    #region Helper Methods

    [PublicAPI]
    public Vector3 GetRandomOffscreenPoint()
    {
        var rndPos = Vector2.zero;

        if (Random.value > 0.5f)
        {
            rndPos.x = Random.Range(BotLeftBoundCorner.x, TopRightBoundCorner.x);

            if (Random.value > 0.5f)
            {
                rndPos.y = TopRightBoundCorner.y + offscreenOffset;
            }
            else
            {
                rndPos.y = BotLeftBoundCorner.y - offscreenOffset;
            }
        }
        else
        {
            rndPos.y = Random.Range(BotLeftBoundCorner.y, TopRightBoundCorner.y);

            if (Random.value > 0.5f)
            {
                rndPos.x = TopRightBoundCorner.x + offscreenOffset;
            }
            else
            {
                rndPos.x = BotLeftBoundCorner.x - offscreenOffset;
            }
        }

        return rndPos;
    }

    public Vector2 GetOffsetScreenPoint(Vector2 normalizedPosition)
    {
        var nrm = (normalizedPosition + Vector2.one) * 0.5f;
        return new Vector2
        {
            x = Mathf.Lerp(BotLeftBoundCorner.x - offscreenOffset,
                TopRightBoundCorner.x + offscreenOffset, nrm.x),
            y = Mathf.Lerp(BotLeftBoundCorner.y - offscreenOffset,
                TopRightBoundCorner.y + offscreenOffset, nrm.y),
        };
    }

    public Vector2 GetRandomPointInPlayArea()
    {
        var rnd = (Random.insideUnitCircle + Vector2.one) * 0.5f;
        return new Vector2
        {
            x = Mathf.Lerp(BotLeftBoundCorner.x, TopRightBoundCorner.x, rnd.x),
            y = Mathf.Lerp(BotLeftBoundCorner.y, TopRightBoundCorner.y, rnd.y),
        };
    }

    public bool IsPointInPlayArea(Vector3 position)
    {
        return position.x < TopRightBoundCorner.x
               && position.x > BotLeftBoundCorner.x
               && position.y < TopRightBoundCorner.y
               && position.y > BotLeftBoundCorner.y;
    }

    #endregion

    #region Editor Help Buttons

    [Button]
    [UsedImplicitly]
    public void DestroyPlayer()
    {
        Player.DestroyEntity();
    }

    [Button]
    [UsedImplicitly]
    public void EndGame()
    {
        Lives = 0;
        Player.DestroyEntity();
    }

    [Button]
    [UsedImplicitly]
    public void IncrementScoreBy100()
    {
        RewardPlayer(100);
    }

    [Button]
    [UsedImplicitly]
    public void ClearLocalScores()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log($"Scores cleared");
    }

    #endregion
}