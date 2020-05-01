using System.Collections.Generic;

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

    public ScoreEntry(string player, string message, int score)
    {
        this.player = player;
        this.message = message;
        this.score = score;
    }
}