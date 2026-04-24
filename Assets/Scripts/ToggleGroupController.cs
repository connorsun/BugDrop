using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleGroupController : MonoBehaviour
{
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private List<Toggle> toggles;
    private List<ToggleButton> toggleButtons;
    [SerializeField] private UIHandler uiHandler;

    public System.Action<int> OnOptionSelected;

    void Awake()
    {
        toggleButtons = new List<ToggleButton>();
        for (int i = 0; i < toggles.Count; i++)
        {
            toggleButtons.Add(toggles[i].GetComponent<ToggleButton>());
            int index = i;
            toggles[i].group = toggleGroup;
            toggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (!isOn) return;
                if (index == 0) uiHandler.OnPlacePaletteClicked();
                if (index == 1) uiHandler.OnMovePaletteClicked();
                if (index == 2) uiHandler.OnDeletePaletteClicked();
            });
        }
    }

    public void SetSelection(int index)
    {
        if (index < 0 || index >= toggles.Count) return;
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].SetIsOnWithoutNotify(i == index);
            toggleButtons[i].ForceRefresh();
        }
    }

    public int GetSelectedIndex()
    {
        for (int i = 0; i < toggles.Count; i++)
            if (toggles[i].isOn) return i;
        return -1;
    }
}