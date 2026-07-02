using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using TMPro;
using System.Text;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private TMP_Text seedInputText;
    [SerializeField] private GameObject inputField;
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
        inputField.GetComponent<CanvasGroup>().blocksRaycasts = true;
        uiHandler.EnterTitleSeededScreen();
        
    }

    public void SeededBackButtonClicked()
    {
        uiHandler.EnterTitleScreen();
    }

    public void OnSeedUpdated()
    {
        if (seedInputText.text.Length > 0) {
            seed = seedInputText.text.Substring(0, seedInputText.text.Length - 1);
        } else
        {
            seed = seedInputText.text;
        }
        // for some reason the input field appends a ? to the end of your text???
        // so i have to get rid of it
    }

    public void OnDailyButtonClicked()
    {
        string dateString = DateTime.Today.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        GameHandler.Seed = dateString.GetHashCode();
        GameHandler.VisualSeed = GameHandler.Seed + "";
        print("set daily seed: " + GameHandler.Seed + " " + dateString);
        UnityEngine.Random.InitState(GameHandler.Seed);
        GameHandler.Seeded = true;
        load = true;
    }

    public void OnStartButtonClicked()
    {
        GameHandler.Seed = Guid.NewGuid().GetHashCode();
        GameHandler.VisualSeed = GameHandler.Seed + "";
        print("set random seed: " + GameHandler.Seed);
        UnityEngine.Random.InitState(GameHandler.Seed);
        GameHandler.Seeded = false;
        load = true;
    }

    public void OnStartButtonClickedSeeded()
    {
        print(seed);
        print(BitConverter.ToString(Encoding.ASCII.GetBytes(seed)));
        print(int.TryParse(seed, out int sn));
        print(int.TryParse("2041278274", out int bkjndls));
        if (seed == null || seed == "")
        {
            GameHandler.Seed = Guid.NewGuid().GetHashCode();
            GameHandler.VisualSeed = GameHandler.Seed + "";
            GameHandler.Seeded = false;
            print("set rrrrrandom seed: " + GameHandler.Seed);
        } else if (int.TryParse(seed, out int seedNum)) {
            GameHandler.Seed = seedNum;
            GameHandler.VisualSeed = GameHandler.Seed + "";
            GameHandler.Seeded = true;
            print("set int seed: " + GameHandler.Seed);
        } else
        {
            GameHandler.Seed = seed.GetHashCode();
            GameHandler.VisualSeed = seed;
            GameHandler.Seeded = true;
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
