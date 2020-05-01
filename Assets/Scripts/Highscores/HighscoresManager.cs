using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;

public class HighscoresManager : MonoSingleton<HighscoresManager>
{
    public string baseUrl = "http://127.0.0.1:8000/";

    public string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player #" + Random.Range(0, 999));
        set => PlayerPrefs.SetString("PlayerName", value);
    }

    public int Highscore
    {
        get => PlayerPrefs.GetInt("Highscore", 0);
        set => PlayerPrefs.SetInt("Highscore", value);
    }

    public bool NewScore(int score)
    {
        if(score > Highscore)
        {
            Highscore = score;
            AddHigscore(score);
            return true;
        }
        else
        {
            return false;
        }
    }

    void AddHigscore(int score)
    {

    }

    [NaughtyAttributes.Button]
    public void LoadHighscores()
    {
        UIController.Instance.LoadingHighscores = true;
        StartCoroutine(GetScores_Routine(OnHighscoresLoaded));
    }

    void OnHighscoresLoaded(ScoreEntries scoreEntries)
    {
        UIController.Instance.SetHighscores(scoreEntries);
    }

    IEnumerator GetScores_Routine(System.Action<ScoreEntries> onDone = null)
    {
        var url = baseUrl + "highscores";
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if(request.IsSuccess())
            {
                onDone?.Invoke(ProcessResponse<ScoreEntries>(request.downloadHandler.text));
            }
            else
            {
                onDone?.Invoke(null);
            }
        }
    }

    private T ProcessResponse<T>(string text)
    {
        try
        {
            var result = JsonUtility.FromJson<T>(text);
            return result;
        } 
        catch (System.Exception e)
        {
            Debug.LogError($"Error while processing response");
            Debug.LogException(e);
            return default(T);
        }
    }
}

[System.Serializable]
public class ScoreEntries
{
    public List<ScoreEntry> entries;
}

[System.Serializable]
public class ScoreEntry
{
    public string player;
    public string message;
    public int score;
}
