using System;

[Serializable]
public class PlayersData
{
    public int _highScore { get; private set; }

    public void ChangeData(int newHighScore)
    {
        _highScore = newHighScore;
    }
}