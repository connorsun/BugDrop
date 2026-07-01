using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private TMP_Text seedInputText;
    private bool load;
    private string seed = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.uiHandler.EnterTitleScreen();
        GameHandler.MuteSound = true;

    }

    public void SeededButtonClicked()
    {
        uiHandler.EnterTitleSeededScreen();
    }

    public void SeededBackButtonClicked()
    {
        uiHandler.EnterTitleScreen();
    }
    public void OnSeedUpdated()
    {
        seed = seedInputText.text;
    }

    public void OnStartButtonClicked()
    {
        GameHandler.Seed = Guid.NewGuid().GetHashCode();
        print("set random seed: " + GameHandler.Seed);
        UnityEngine.Random.InitState(GameHandler.Seed);
        load = true;
    }

    public void OnStartButtonClickedSeeded()
    {
        if (seed == null || seed == "")
        {
            GameHandler.Seed = Guid.NewGuid().GetHashCode();
            print("set rrrrrandom seed: " + GameHandler.Seed);
        } else if (int.TryParse(seed, out int seedNum)) {
            GameHandler.Seed = seedNum;
            print("set int seed: " + GameHandler.Seed);
        } else
        {
            GameHandler.Seed = seed.GetHashCode();
            print("set set seed: " + GameHandler.Seed);
        }
        UnityEngine.Random.InitState(GameHandler.Seed);
        load = true;

    }

    // IEnumerator LoadScene()
    // {
    //     AsyncOperation op = SceneManager.LoadScene("Arena");
    //     op.allowSceneActivation = true;
    //     while (!op.isDone)
    //     {
    //         yield return null;
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        if (load)
        {
            GameHandler.MuteSound = false;
            SceneManager.LoadScene("Arena");
        }
    }
}
