using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighscoresManager : MonoSingleton<HighscoresManager>
{
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

    List<ScoreEntry> GetHighscores()
    {
        return null;
    }
}

public class ScoreEntry
{
    public string name;
    public int score;
}
