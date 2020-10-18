using System.Collections.Generic;

[System.Serializable]
public class ScoreEntries
{
    public List<ScoreEntry> entries;
    public ScoreEntry playerScore;
}

[System.Serializable]
public class ScoreEntry
{
    public int place;
    public string player;
    public string message;
    public long score;
    public string guid;

    public ScoreEntry(string player, string message, long score)
    {
        this.player = player;
        this.message = message;
        this.score = score;
    }

    public ScoreEntry(string playerName, string playerMessage, long highscore, string guid
    ) :
        this(playerName, playerMessage, highscore)
    {
        this.guid = guid;
    }
}

[System.Serializable]
public class PlayGuid
{
    public string guid;
}