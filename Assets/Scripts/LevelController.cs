using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Timeline;
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
    public float offscreenOffset = 1f;
    public float asteroidRandomDistanceFromPlayer = 2f;

    [Header("Prefabs")]
    public List<Asteroid> asteroidPrefabs = new List<Asteroid>();    

    bool gameOver = true;
    float enemySpawnCooldownLeft;

    public Vector3 TopRightBoundCorner { get; private set; }
    public Vector3 BotLeftBoundCorner { get; private set; }

    public Camera MainCam { get; private set; }


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
        ShowMenuIntro();
    }

    public void Update()
    {
        if (!gameOver)
        {
            if (enemySpawnCooldownLeft <= 0)
            {
                SpawnAsteroid();
                ResetEnemySpawnCooldown();
            }
            else
            {
                enemySpawnCooldownLeft -= Time.deltaTime;
            }
        }
    }

    public void ShowMenuIntro()
    {
        IntroAnimator.Instance.ShowIntro();
    }

    public void ResetGame()
    {
        Score = 0;
        Lives = maxPlayerLives;
    }

    [NaughtyAttributes.Button]
    public void StartNewGame()
    {
        Debug.Log("Starting new game");
        ResetGame();
        DestroyEntities();
        ResetEnemySpawnCooldown();
        Player.Respawn();        
        gameOver = false;
        UIController.Instance.ShowGameUI();
        IntroAnimator.Instance.ShowingIntro = false;
    }

    public void SetGameOver()
    {
        print("Gamover");
        gameOver = true;
        UIController.Instance.ShowGameOverScreen();
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
        asteroid.SetForceTowardPoint(Player.transform.position + Random.insideUnitSphere * asteroidRandomDistanceFromPlayer);
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

        if (UnityEngine.Random.value > 0.5f)
        {
            rndPos.x = UnityEngine.Random.Range(BotLeftBoundCorner.x, TopRightBoundCorner.x);

            if (UnityEngine.Random.value > 0.5f)
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
            rndPos.y = UnityEngine.Random.Range(BotLeftBoundCorner.y, TopRightBoundCorner.y);

            if (UnityEngine.Random.value > 0.5f)
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

    public Vector2 GetOffscreenPoint(Vector2 normalizedPosition)
    {
        var nrm = (normalizedPosition + Vector2.one) * 0.5f;
        return new Vector2
        {
            x = Mathf.Lerp(BotLeftBoundCorner.x - offscreenOffset, TopRightBoundCorner.x + offscreenOffset, nrm.x),
            y = Mathf.Lerp(BotLeftBoundCorner.y - offscreenOffset, TopRightBoundCorner.y + offscreenOffset, nrm.y),
        };
    }

    #endregion

    #region Editor Help Buttons

    [NaughtyAttributes.Button]
    public void DestroyPlayer()
    {
        Player.DestroyEntity();
    }

    [NaughtyAttributes.Button]
    public void EndGame()
    {
        Lives = 0;
        Player.DestroyEntity();
    }

    [NaughtyAttributes.Button]
    public void IncrementScoreBy100()
    {
        RewardPlayer(100);
    }

    #endregion
}
