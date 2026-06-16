using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "ScriptableObjects/Sound", order = 1)]
public class Sound : ScriptableObject
{
    [SerializeField] public AudioClip clip;
    [SerializeField] public float volumeRatio = 1;
    [SerializeField] public bool bypassMute = false;
    private void Reset()
    {
        volumeRatio = 1;
        bypassMute = false;
    }
}