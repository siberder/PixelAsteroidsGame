using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;

public class HighscoresManager : MonoSingleton<HighscoresManager>
{
    string BaseUrl => GameManager.Instance.serverBaseUrl;

    public string PlayerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player #" + Random.Range(0, 999));
        set => PlayerPrefs.SetString("PlayerName", value);
    }

    public string PlayerMessage
    {
        get => PlayerPrefs.GetString("PlayerMessage", "I am the best");
        set => PlayerPrefs.SetString("PlayerMessage", value);
    }

    public int Highscore
    {
        get => PlayerPrefs.GetInt("Highscore", 0);
        set => PlayerPrefs.SetInt("Highscore", value);
    }

    public bool AddNewScore(int score)
    {
        if(score > Highscore)
        {
            Highscore = score;
            UploadScore(new ScoreEntry(PlayerName, PlayerMessage, Highscore));
            return true;
        }
        else
        {
            LoadHighscores();
            return false;
        }
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

    void UploadScore(ScoreEntry scoreEntry)
    {
        UIController.Instance.LoadingHighscores = true;
        StartCoroutine(AddScore_Routine(scoreEntry, OnHighscoresLoaded));
    }

    IEnumerator GetScores_Routine(System.Action<ScoreEntries> onDone = null)
    {
        var url = BaseUrl + "highscores";
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

    IEnumerator AddScore_Routine(ScoreEntry scoreEntry, System.Action<ScoreEntries> onDone = null)
    {
        var url = BaseUrl + "add_score";
        var data = JsonUtility.ToJson(scoreEntry);
        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.IsSuccess())
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
