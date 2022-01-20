using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    [SerializeField]
    private Button _nextLevelButton;
    [SerializeField]
    private Text _winText;

    private void Awake()
    {
        if (_nextLevelButton)
        {
            _nextLevelButton.gameObject.SetActive(true);
            _nextLevelButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); });
            _nextLevelButton.gameObject.SetActive(false);
        }

        _winText.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (_nextLevelButton)
            _nextLevelButton.gameObject.SetActive(true);
        if (_winText)
            _winText.gameObject.SetActive(true);
    }
}
