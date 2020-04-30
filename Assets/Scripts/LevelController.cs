using System;
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

    [Header("Prefabs")]
    public List<Asteroid> asteroidPrefabs = new List<Asteroid>();

    [Header("References")]
    public Transform introPosition;
    public Transform startPosition;


    float enemySpawnCooldownLeft;
    public Vector3 TopRightBoundCorner { get; private set; }
    public Vector3 BotLeftBoundCorner { get; private set; }
    public bool SkipIntro { get; set; }

    public Camera MainCam { get; private set; }

    bool gameOver = true;

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
        //StartNewGame();
        StartCoroutine(AnimateSpaceShip_Intro());
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

    IEnumerator AnimateSpaceShip_Intro()
    {
        yield return new WaitForSeconds(1f);

        Player.AnimatingIntro = true;

        var curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        var startPos = GetOffscreenPoint(new Vector2(0, -1f));
        var animTime = 2f;
        for (float _animTime = 0; _animTime < animTime; _animTime += Time.deltaTime)
        {
            float t = _animTime / animTime;
            Player.transform.position = Vector3.Lerp(startPos, introPosition.position, curve.Evaluate(t));
            yield return null;
        }

        yield return new WaitUntil(() => SkipIntro);

        ResetGame();
        EnableGameUI(true);

        startPos = Player.transform.position;
        animTime = 0.5f;
        for (float _animTime = 0; _animTime < animTime; _animTime += Time.deltaTime)
        {
            float t = _animTime / animTime;
            Player.transform.position = Vector3.Lerp(startPos, startPosition.position, curve.Evaluate(t));
            yield return null;
        }

        Player.AnimatingIntro = false;
        StartNewGame();
    }

    void EnableGameUI(bool enable)
    {
        UIController.Instance.SetGameScreenVisible(enable);
        UIController.Instance.SetMenuVisible(!enable);
    }

    void ResetGame()
    {
        Score = 0;
        Lives = maxPlayerLives;
    }

    [NaughtyAttributes.Button]
    public void StartNewGame()
    {
        print("Starting new game");
        ResetGame();
        DestroyEntities();
        ResetEnemySpawnCooldown();
        Player.Respawn();        
        gameOver = false;
        EnableGameUI(true);
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
}
