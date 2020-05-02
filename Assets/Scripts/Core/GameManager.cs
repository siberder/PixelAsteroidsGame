using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [BoxGroup("Highscores Settings")] public string serverBaseUrl = "http://127.0.0.1:8000/";

    [BoxGroup("Player Settings")] public int maxPlayerLives = 3;
    [BoxGroup("Player Settings")] public float playerInvincibilityTime = 3f;
    [BoxGroup("Player Settings")] public float playerRespawnDelay = 2f;
    [BoxGroup("Player Settings")] public int playerDeathReward = 100;

    [BoxGroup("Enemies Settings")] public float enemySpawnCooldown = 2f;
    [BoxGroup("Enemies Settings")] public float asteroidRandomDistanceFromTarget = 2f;
    [BoxGroup("Enemies Settings")] public float enemySpaceshipSpawnChance = 0.05f;

    [BoxGroup("Other Settings")] public float offscreenOffset = 1f;

    [Header("Prefabs")] 
    public List<Asteroid> asteroidPrefabs = new List<Asteroid>();
    public List<EnemySpaceship> enemySpaceships = new List<EnemySpaceship>();

    bool gameOver = true;
    float enemySpawnCooldownLeft;

    public Bounds PlayAreaBounds { get; private set; }
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

        PlayAreaBounds = MainCam.OrthographicBounds();
        TopRightBoundCorner = PlayAreaBounds.center + PlayAreaBounds.extents;
        BotLeftBoundCorner = PlayAreaBounds.center - PlayAreaBounds.extents;
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
                enemySpawnCooldownLeft -= Time.deltaTime;
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                StartNewGame();
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
        Debug.Log("Game Over");
        gameOver = true;
        UIController.Instance.ShowGameOverScreen();

        Invoke(nameof(DestroyEntities), 0.5f);
    }

    void ResetEnemySpawnCooldown()
    {
        enemySpawnCooldownLeft = enemySpawnCooldown;
    }

    void DestroyEntities()
    {
        while(spawnedEntities.Count > 0)
        {
            var entity = spawnedEntities[0];
            if (entity != null)
            {
                entity.DestroyEntity();
            }

            spawnedEntities.Remove(entity);
        }
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

    [NaughtyAttributes.Button]
    void SpawnAsteroid()
    {
        Asteroid asteroidPrefab = asteroidPrefabs[UnityEngine.Random.Range(0, asteroidPrefabs.Count)];
        var asteroid = Instantiate(asteroidPrefab, GetRandomOffscreenPoint(), Quaternion.identity);
        asteroid.SetRandomSkin();
        asteroid.SetForceTowardPoint(UnityEngine.Random.insideUnitSphere * asteroidRandomDistanceFromTarget);

        spawnedEntities.Add(asteroid);
    }

    [NaughtyAttributes.Button]
    void SpawnEnemy()
    {
        var enemy = Instantiate(enemySpaceships[UnityEngine.Random.Range(0, enemySpaceships.Count)], GetRandomOffscreenPoint(), Quaternion.identity);
        enemy.Init();

        spawnedEntities.Add(enemy);
    }

    public void AddSpawnedEntity(Entity entity)
    {
        spawnedEntities.Add(entity);
    }

    public void RemoveDeadEntity(Entity entity)
    {
        if(spawnedEntities.Contains(entity))
        {
            spawnedEntities.Remove(entity);
        }
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

    public Vector2 GetOffsetScreenPoint(Vector2 normalizedPosition)
    {
        var nrm = (normalizedPosition + Vector2.one) * 0.5f;
        return new Vector2
        {
            x = Mathf.Lerp(BotLeftBoundCorner.x - offscreenOffset, TopRightBoundCorner.x + offscreenOffset, nrm.x),
            y = Mathf.Lerp(BotLeftBoundCorner.y - offscreenOffset, TopRightBoundCorner.y + offscreenOffset, nrm.y),
        };
    }

    public Vector2 GetRandomPointInPlayArea()
    {
        var rnd = (UnityEngine.Random.insideUnitCircle + Vector2.one) * 0.5f;
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
