using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private UIHandler uiHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.uiHandler.EnterTitleScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
