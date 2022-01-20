using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private WinUI _winUI;
    [SerializeField]
    private LoseUI _loseUI;

    private bool _quiting;

   private void Awake()
   {
       var builder = FindObjectOfType<LevelBuilder>();
       if (builder)
       {
           builder.OnPlayerWin += ShowWinUI;
           builder.OnPlayerLose += ShowLoseUI;
       }
   }

    public void ShowWinUI()
    {
        if (_winUI)
            _winUI.Show();
    }

    public void ShowLoseUI()
    {
        if (_loseUI)
            _loseUI.Show();
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
                levelBuilder.OnPlayerLose -= ShowLoseUI;
                levelBuilder.OnPlayerWin -= ShowWinUI;
            }
        }
    }
}
