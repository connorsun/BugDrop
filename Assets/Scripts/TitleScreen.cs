using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private UIHandler uiHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.uiHandler.EnterTitleScreen();
    }

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("Arena");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
