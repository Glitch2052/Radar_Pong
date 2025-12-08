using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public CoinManagerSO coinManagerSo;
    [SerializeField] private AudioSource sfxAudioPlayer;
    [SerializeField] private AudioSource bgmAudioPlayer;

    public bool IsPaused { get; private set; }
    private float prevTimeScale = 1f;
    
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

    public IEnumerator Init()
    {
        uiManager.OnBgmToggleAction += (value) => ToggleMuteGameMusic(!value);
        uiManager.Init();
        yield return null;
        
        coinManagerSo.Init();
        
        PongBoard.instance.FlickerBorder();
        yield return uiManager.PlaySplashScreen();
        
        uiManager.ShowMainPanel();
        
        PongBoard.instance.powerUpManager.Init();
        PongBoard.instance.collectibleManager.Init();
        
        AdManager.Instance.ShowBanner();
    }

    public void StartGame()
    {
        PongBoard.instance.Init();
    }

    public void ContinueGameFromLastState()
    {
        PongBoard.instance.ContinueGameFromLastState();
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

    public void ResumeGameMusic()
    {
        if (bgmAudioPlayer)
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

    public void PauseGameState()
    {
        IsPaused = true;
        if (IsPaused)
            prevTimeScale = Time.timeScale;
        Time.timeScale = IsPaused ? 0 : prevTimeScale;
    }
    
    public void ResumeGameState()
    {
        IsPaused = false;
        if (IsPaused)
            prevTimeScale = Time.timeScale;
        Time.timeScale = IsPaused ? 0 : prevTimeScale;
    }
}