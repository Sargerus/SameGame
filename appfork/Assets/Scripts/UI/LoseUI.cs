using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseUI : MonoBehaviour
{
    [SerializeField]
    private Button _restartButton;
    [SerializeField]
    private Text _loseText;

    private void Awake()
    {
        if (_restartButton)
        {
            _restartButton.gameObject.SetActive(true);
            _restartButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); });
            _restartButton.gameObject.SetActive(false);
        }

        _loseText.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_restartButton)
            _restartButton.gameObject.SetActive(true);
        if (_loseText)
            _loseText.gameObject.SetActive(true);
    }
}
