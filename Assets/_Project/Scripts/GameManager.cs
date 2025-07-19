using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    [SerializeField] private AudioSource sfxAudioPlayer;
    [SerializeField] private AudioSource bgmAudioPlayer;
    
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    IEnumerator Start()
    {
        uiManager.OnBgmToggleAction += (value) => ToggleMuteGameMusic(!value);
        uiManager.Init();
        
        yield return null;
        
        PongBoard.instance.FlickerBorder();
        yield return uiManager.PlaySplashScreen();
        
        uiManager.ShowMainPanel();
    }

    public void StartGame()
    {
        PongBoard.instance.Init();
    }

    public void PauseGame()
    {
        // uiManager.PauseGameToMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PauseGame();
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1)
    {
        if(uiManager.IsSfxEnabled)
            sfxAudioPlayer.PlayOneShot(clip, volume);
    }

    public void StartGameMusic(AudioClip clip, float volume = 1, bool isLoop = false)
    {
        bgmAudioPlayer.clip = clip;
        bgmAudioPlayer.volume = volume;
        bgmAudioPlayer.loop = isLoop;

        bgmAudioPlayer.Play();
    }

    public void StopGameMusic()
    {
        if(bgmAudioPlayer)
            bgmAudioPlayer.Stop();
    }

    private void ToggleMuteGameMusic(bool value)
    {
        if (bgmAudioPlayer) bgmAudioPlayer.mute = value;
    }
}