using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;
public class MusicHandler : MonoBehaviour
{
    [SerializeField] private AudioSource[] musicPlayers;
    [SerializeField] private AudioClip music;
    private int nextMusicPlayer;
    private const float preloadDelay = 1f;
    private const string startScene = "Title Screen";
    private bool notFirstLoad;
    private bool losing;
    private const float LOSE_VOLUME = 0f;
    private const float LOSE_VOLUME_DIFF = 0.2f;
    private const float LOSE_PITCH = 0.7f;
    private const float LOSE_PITCH_DIFF = 0.1f;

    public void Awake()
    {
        if (Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name == "MusicPlayer" && obj != gameObject)
                .ToArray().Length > 0)
        {
            // duplicate music handlers
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        nextMusicPlayer = 0;
        musicPlayers[nextMusicPlayer].clip = music;
        losing = false;
    }

    public async Task Lose()
    {
        losing = true;
        while (losing)
        {
            foreach (AudioSource source in musicPlayers)
            {
                source.volume = Mathf.Max(LOSE_VOLUME, source.volume - LOSE_VOLUME_DIFF * Time.unscaledDeltaTime);
                source.pitch = Mathf.Max(LOSE_PITCH, source.pitch - LOSE_PITCH_DIFF * Time.unscaledDeltaTime);
            }
            await Task.Yield();
        }
    }

    public async Task Unlose()
    {
        losing = false;
        foreach (AudioSource source in musicPlayers)
        {
            source.Stop();
            source.pitch = 1f;
            source.volume = 1f;
        }
        musicPlayers[nextMusicPlayer].Play();
        QueueNextSong();
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == startScene && !notFirstLoad) {
            notFirstLoad = true;
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