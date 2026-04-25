using UnityEngine;

[CreateAssetMenu(fileName = "Sound", menuName = "ScriptableObjects/Sound", order = 1)]
public class Sound : ScriptableObject
{
    [SerializeField] public AudioClip clip;
    [SerializeField] public float volumeRatio = 1;
    private void Reset()
    {
        volumeRatio = 1;
    }
}