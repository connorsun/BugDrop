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
        // GameHandler.MuteSound = true;
        GameHandler.MuteSound = false;

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
        UnityEngine.Random.InitState(GameHandler.Seed);
        GameHandler.Seeded = true;
        load = true;
    }

    public void OnStartButtonClicked()
    {
        GameHandler.Seed = Guid.NewGuid().GetHashCode();
        GameHandler.VisualSeed = GameHandler.Seed + "";
        UnityEngine.Random.InitState(GameHandler.Seed);
        GameHandler.Seeded = false;
        load = true;
    }

    public void OnStartButtonClickedSeeded()
    {
        if (seed == null || seed == "")
        {
            GameHandler.Seed = Guid.NewGuid().GetHashCode();
            GameHandler.VisualSeed = GameHandler.Seed + "";
            GameHandler.Seeded = false;
        } else if (int.TryParse(seed, out int seedNum)) {
            GameHandler.Seed = seedNum;
            GameHandler.VisualSeed = GameHandler.Seed + "";
            GameHandler.Seeded = true;
        } else
        {
            GameHandler.Seed = seed.GetHashCode();
            GameHandler.VisualSeed = seed;
            GameHandler.Seeded = true;
        }
        UnityEngine.Random.InitState(GameHandler.Seed);
        load = true;

    }

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
