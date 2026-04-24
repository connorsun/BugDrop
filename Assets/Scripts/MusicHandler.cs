using UnityEngine;
using UnityEngine.SceneManagement;
public class MusicHandler : MonoBehaviour
{
    [SerializeField] private AudioSource[] musicPlayers;
    [SerializeField] private AudioClip music;
    private int nextMusicPlayer;
    private const float preloadDelay = 1f;
    private const string startScene = "Title Screen";
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        nextMusicPlayer = 0;
        musicPlayers[nextMusicPlayer].clip = music;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == startScene) {
            musicPlayers[nextMusicPlayer].Play();
            QueueNextSong();
        }
    }
    
    async Awaitable QueueNextSong()
    {
        double nextStartTime = AudioSettings.dspTime + ((double) music.samples) / music.frequency;
        while (true) {
            while (AudioSettings.dspTime < nextStartTime - preloadDelay)
            {
                await Awaitable.NextFrameAsync();
            }
            nextMusicPlayer = (nextMusicPlayer + 1) % musicPlayers.Length;
            musicPlayers[nextMusicPlayer].clip = music;
            musicPlayers[nextMusicPlayer].PlayScheduled(nextStartTime);
            nextStartTime += ((double) music.samples) / music.frequency;
        }
    }
}