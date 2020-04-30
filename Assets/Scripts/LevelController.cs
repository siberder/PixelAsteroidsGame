using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class LevelController : MonoSingleton<LevelController>
{
    public static LayerMask PlayerLayerMask { get => LayerMask.GetMask("Player", "PlayerProjectile"); }

    [Header("Settings")]
    public int maxPlayerLives = 3;
    public float enemySpawnCooldown = 2f;
    public float playerInvincibilityTime = 3f;
    public float playerRespawnDelay = 2f;
    public int playerDeathReward = 100;
    
    [Header("Prefabs")]
    public List<Asteroid> asteroidPrefabs = new List<Asteroid>();

    float enemySpawnCooldownLeft;
    public Vector3 TopRightBoundCorner { get; private set; }
    public Vector3 BotLeftBoundCorner { get; private set; }
    public Camera MainCam { get; private set; }

    bool gameOver;

    public SpaceshipController Player { get; private set; }

    int score;
    public int Score
    {
        get => score; 
        set
        {
            score = value;
            UIController.Instance.UpdateScoreText(score);
        }
    }

    int lives;
    public int Lives
    {
        get => lives; 
        set
        {
            lives = value;
            UIController.Instance.UpdateLivesCount(lives);
        }
    }

    List<Entity> spawnedEntities = new List<Entity>();

    private void Awake()
    {
        MainCam = Camera.main;
        Player = FindObjectOfType<SpaceshipController>();

        var playAreaRect = MainCam.OrthographicBounds();
        TopRightBoundCorner = playAreaRect.center + playAreaRect.extents;
        BotLeftBoundCorner = playAreaRect.center - playAreaRect.extents;
    }

    private void Start()
    {
        StartNewGame();
    }

    public void Update()
    {
        if(enemySpawnCooldownLeft <= 0)
        {
            SpawnAsteroid();
            ResetEnemySpawnCooldown();
        }
        else
        {
            enemySpawnCooldownLeft -= Time.deltaTime;
        }
    }

    [NaughtyAttributes.Button]
    public void StartNewGame()
    {
        DestroyEntities();
        ResetEnemySpawnCooldown();
        Player.Respawn();
        Score = 0;
        Lives = maxPlayerLives;
        gameOver = false;
    }

    public void SetGameOver()
    {
        print("Gamover");
        gameOver = true;
    }

    void ResetEnemySpawnCooldown()
    {
        enemySpawnCooldownLeft = enemySpawnCooldown;
    }

    void DestroyEntities()
    {
        foreach (var entity in spawnedEntities)
        {
            Destroy(entity.gameObject);
        }

        spawnedEntities.Clear();
    }

    internal void OnPlayerDied()
    {
        Lives--;
        RewardPlayer(playerDeathReward);

        if(Lives <= 0)
        {
            SetGameOver();
        }
        else
        {
            Invoke(nameof(RespawnPlayer), playerRespawnDelay);
        }
    }

    void RespawnPlayer()
    {
        Player.Respawn();
    }

    void SpawnAsteroid()
    {
        Asteroid asteroidPrefab = asteroidPrefabs[UnityEngine.Random.Range(0, asteroidPrefabs.Count)];
        var asteroid = Instantiate(asteroidPrefab, GetRandomOffscreenPoint(), Quaternion.identity);
        asteroid.SetRandomSkin();
        asteroid.SetForceTowardPoint(Player.transform.position);
        asteroid.OnDestroy += (a) => spawnedEntities.Remove(a);

        spawnedEntities.Add(asteroid);
    }

    public void RewardPlayer(int points)
    {
        Score += points;
    }

    #region Helper Methods

    public Vector3 GetRandomOffscreenPoint()
    {
        Vector2 rndPos = Vector2.zero;
        float offset = 1f;

        if (UnityEngine.Random.value > 0.5f)
        {
            rndPos.x = UnityEngine.Random.Range(BotLeftBoundCorner.x, TopRightBoundCorner.x);

            if (UnityEngine.Random.value > 0.5f)
            {
                rndPos.y = TopRightBoundCorner.y + offset;
            }
            else
            {
                rndPos.y = BotLeftBoundCorner.y - offset;
            }
        }
        else
        {
            rndPos.y = UnityEngine.Random.Range(BotLeftBoundCorner.y, TopRightBoundCorner.y);

            if (UnityEngine.Random.value > 0.5f)
            {
                rndPos.x = TopRightBoundCorner.x + offset;
            }
            else
            {
                rndPos.x = BotLeftBoundCorner.x - offset;
            }
        }

        return rndPos;
    }

    #endregion
}
