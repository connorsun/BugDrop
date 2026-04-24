using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private UIHandler uiHandler;
    public record DialogueLine(string Dialogue, string Speaker);
    private DialogueLine[] lines = new DialogueLine[]{
        new("...and that's the whole apartment! Still want to sign the lease?", "Landlord"),
        new("Definitely! Honestly, I can't afford not to.", "You"),
        new("Perfect! There's one last thing though... this is a bug-powered establishment.", "Landlord"),
        new("Sorry?", "You"),
        new("See that bug terrarium? It powers the whole place.", "Landlord"),
        new("To keep the lights on, you'll have to collect bugs.", "Landlord"),
        new("What are you talking about? How is that even possible?", "You"),
        new("Think of it less as a bug problem... and more as a feature.", "Landlord"),
    };
    int dialogueIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Start()
    {
        dialogueIndex = 0;
        this.uiHandler.SetIntroCutsceneLine(lines[dialogueIndex].Dialogue, lines[dialogueIndex].Speaker);
        this.uiHandler.EnterTitleScreen();
    }

    public void NextLine()
    {
        if (dialogueIndex >= lines.Length - 1)
        {
            NextScene();
            return;
        }

        dialogueIndex++;

        if (dialogueIndex == 1)
        {
            this.uiHandler.ShowSkipButton();
        }
        this.uiHandler.NextIntroCutsceneLine(lines[dialogueIndex].Dialogue, lines[dialogueIndex].Speaker);
    }

    public void NextScene()
    {
        SceneManager.LoadScene("Title Screen");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
