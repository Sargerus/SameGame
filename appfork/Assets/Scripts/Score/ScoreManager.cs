using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private const string SCORE_TEXT = "Score: ";
    private const string HIGHSCORE_TEXT = "Best: ";

    public int ScoreFor1Block;
    public int DecreasingScore;
    public float DecreasingScoreDelay;

    [SerializeField]
    private Text _scoreText;
    private int _score;
    [SerializeField]
    private Text _highScoreText;
    private int _highScore;

    private bool _quiting;
    private Coroutine _decreasingScoreCoroutine;
    private SerializeManager _serializeManager;
    private PlayersData _data;

    private void Awake()
    {
        var levelBuilder = FindObjectOfType<LevelBuilder>();
        if (levelBuilder)
        {
            levelBuilder.OnPlayerLose += StopDecreasingScore;
            levelBuilder.OnPlayerWin += StopDecreasingScore;
        }
    }

    private void StopDecreasingScore()
    {
        if (_decreasingScoreCoroutine != null)
            StopCoroutine(_decreasingScoreCoroutine);
    }

    void Start()
    {
        _quiting = false;

        var levelBuilder = FindObjectOfType<LevelBuilder>();
        if (levelBuilder != null)
            levelBuilder.OnBlocksBurn += IncreaseScore;

        if (!_serializeManager)
            _serializeManager = FindObjectOfType<SerializeManager>();

        if(_serializeManager)
        {
            _data = _serializeManager.LoadProgress();
            _highScoreText.text = HIGHSCORE_TEXT + _data._highScore;
            _highScore = _data._highScore;
        }

        _decreasingScoreCoroutine = StartCoroutine(DecreaseScore());
    }

    private void IncreaseScore(int burntBlocks)
    {
        SetScore(_score += burntBlocks * ScoreFor1Block);
    }

    public void SetScore(int newScore)
    {
        _scoreText.text = SCORE_TEXT + newScore;
    }

    private IEnumerator DecreaseScore()
    {
        while (!_quiting)
        {
            SetScore(_score -= DecreasingScore);
            yield return new WaitForSeconds(DecreasingScoreDelay);
        }
    }

    private void OnApplicationQuit()
    {
        _quiting = true;
    }

    private void OnDestroy()
    {
        if (!_quiting)
        {
            var levelBuilder = FindObjectOfType<LevelBuilder>();
            if (levelBuilder != null)
            {
                levelBuilder.OnBlocksBurn -= IncreaseScore;
                levelBuilder.OnPlayerLose -= StopDecreasingScore;
                levelBuilder.OnPlayerWin -= StopDecreasingScore;
            }

        }

        if(_score > _highScore)
        {
            if (_serializeManager)
            {
                _data.ChangeData(_score);
                _serializeManager.SaveProgress(_data);
            }
        }
    }
}
