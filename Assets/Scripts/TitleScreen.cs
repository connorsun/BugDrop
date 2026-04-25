using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Arena");
        op.allowSceneActivation = true;
        while (!op.isDone)
        {
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
