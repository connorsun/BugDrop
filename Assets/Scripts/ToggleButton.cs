using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    
    // Set these in the Unity Inspector
    public Color activeColor = new Color(0.2f, 0.2f, 0.2f); // Dark
    public Color inactiveColor = Color.white;             // Light
    public Color hoverTint = new Color(0.6f, 0.6f, 0.6f); // Highlight effect
    public Color pressTint = new Color(0.4f, 0.4f, 0.4f); // Highlight effect

    void Start()
    {
        toggle.onValueChanged.AddListener(UpdateToggleColors);
        UpdateToggleColors(toggle.isOn);
    }

    void UpdateToggleColors(bool isOn)
    {
        ColorBlock cb = toggle.colors;
        if (isOn)
        {
            cb.normalColor = activeColor;
            cb.selectedColor = activeColor;
            cb.highlightedColor = activeColor * hoverTint;
            cb.pressedColor = activeColor * pressTint;
        }
        else
        {
            cb.normalColor = inactiveColor;
            cb.selectedColor = inactiveColor;
            cb.highlightedColor = inactiveColor * hoverTint;
            cb.pressedColor = inactiveColor * pressTint;
        }
        toggle.colors = cb;
    }
}