using System.Collections;
using DG.Tweening;
using Lofelt.NiceVibrations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PongBoard : MonoBehaviour
{
    public Camera monitorCamera;
    public SpriteRenderer glowBorderRenderer;
    public SpriteRenderer gridRenderer;

    [Space(20)] 
    public Transform gameTransform;
    public UIKnob leftController;
    public UIKnob rightController;
    public Ball ballPrefab;

    #region Score Data
    [Space(20), Header("UI Data")] 
    [SerializeField] private Text scoreText;
    [SerializeField] private Text monitorScoreText;
    [SerializeField] private TextMeshProUGUI gameEndScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI newText;
    [SerializeField] private CanvasGroup scoreCanvasGrp;
    [SerializeField] private Text countDownText;

    private Ball currentBall;
    private int currentScore;
    private readonly string[] scrambledScoreVariants = new string[]
    {
        "88", "EE", "7F", "3C", "0F", "1E", "9A", "C7", "D3", "4B",
        "5E", "6D", "FA", "3E", "8B", "F7", "AC", "DF", "BA", "0C",
        "2F", "AF", "5B", "6C", "4E", "CE", "A9", "1C", "DE", "3B",
        "@@", "##", "$$", "!!", "%%", "&&", "**", "::", "//", "00",
        "O0", "0O", "I1", "1I", "S5", "5S", "Z2", "2Z", "B8", "8B",
    };
    
    #endregion

    #region Sound Data

    [Space(20), Header("Sound Data")] 
    [SerializeField] private AudioClip[] bgMusicClip;
    [SerializeField] private AudioClip[] textGlitchClip;
    [SerializeField] private AudioClip countDownClip;
    [SerializeField] private AudioClip ballHitClip;
    [SerializeField] public AudioClip ballDestroyClip;
    
    #endregion

    public int CurrentScore
    {
        get => currentScore;
        set => currentScore = value;
    }
    
    private readonly float shakeOffset = 0.08f;
    private bool isReady;
    public static PongBoard instance;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
        isReady = false;
        
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Init()
    {
        gameEndScoreText.gameObject.SetActive(false);
        gameTransform.gameObject.SetActive(true);

        currentBall = Instantiate(ballPrefab,gameTransform);
        currentBall.OnCollidedWithPaddle += OnBallCollideWithPaddle;
        currentBall.OnDestroyed += OnBallDestroyed;
        currentBall.gameObject.SetActive(false);
        
        StartCoroutine(StartCountDownBeforeInit());
    }

    private void StartGame()
    {
        CurrentScore = 0;
        UpdateScore(CurrentScore);
        currentBall.gameObject.SetActive(true);
        
        leftController.Init();
        rightController.Init();
        currentBall.Init();

        // StartCoroutine(FlickerBorderCoroutine());
        GameManager.instance.StartGameMusic(bgMusicClip[Random.Range(0,bgMusicClip.Length - 1)],0.5f);
        isReady = true;
    }

    void Update()
    {
        if(!isReady) return;
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        leftController.IUpdate();
        rightController.IUpdate();
    }

    private void OnBallCollideWithPaddle(PaddleType paddleType, Vector2 collisionVelocity)
    {
        CurrentScore++;
        UpdateScore(currentScore);

        if (Random.Range(0f, 1f) <= 0.36f)
            monitorCamera.transform.DOPunchPosition(collisionVelocity.normalized * shakeOffset, 0.16f);
        
        GameManager.instance.PlayOneShot(ballHitClip,0.5f);
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        if (Random.Range(0f, 1f) <= 0.36f)
        {
            StartCoroutine(FlickerTextCoroutine());
        }
    }

    private void OnBallDestroyed()
    {
        GameManager.instance.uiManager.PauseGame(() =>
        {
            monitorScoreText.gameObject.SetActive(false);

            int bestScore = PlayerPrefs.GetInt(StringID.HighScore, 0);
            if (CurrentScore > bestScore)
            {
                newText.gameObject.SetActive(true);
                bestScore = CurrentScore;
                PlayerPrefs.SetInt(StringID.HighScore,CurrentScore);
            }
            else
                newText.gameObject.SetActive(false);

            gameEndScoreText.text = $"Score:{monitorScoreText.text}";
            string bestScoreString = bestScore < 10 ? bestScore.ToString().PadLeft(2, '0') : bestScore.ToString();
            bestScoreText.text = $"Best:{bestScoreString}";
            gameEndScoreText.gameObject.SetActive(true);
            gameEndScoreText.transform.DOLocalMoveY(1.8f, 0.4f).From(1.4f);
            scoreCanvasGrp.DOFade(1f, 0.75f).From(0);

            gameTransform.gameObject.SetActive(false);
        });
    }

    public void DisableScoreText()
    {
        scoreCanvasGrp.DOFade(0f, 0.3f);
    }
    
    public void ReloadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void UpdateScore(int score)
    {
        monitorScoreText.text = score < 10 ? score.ToString().PadLeft(2, '0') : score.ToString();
        if(scoreText != null) scoreText.text = monitorScoreText.text;
    }

    private IEnumerator StartCountDownBeforeInit()
    {
        GameManager.instance.StartGameMusic(countDownClip,0.3f,false);
        countDownText.gameObject.SetActive(true);
        monitorScoreText.gameObject.SetActive(false);
        for (int i = 3; i >= 0; i--)
        {
            countDownText.text = i.ToString();
            yield return new WaitForSeconds(0.92f);
        }
        yield return null;
        monitorScoreText.gameObject.SetActive(true);
        countDownText.gameObject.SetActive(false);
        StartGame();
    }

    private IEnumerator FlickerTextCoroutine()
    {
        float dx = 0;
        float duration = Random.Range(0.24f, 0.36f);
        Color tempColor = monitorScoreText.color;
        monitorScoreText.text = scrambledScoreVariants[Random.Range(0, scrambledScoreVariants.Length - 1)];
        GameManager.instance.PlayOneShot(textGlitchClip[Random.Range(0,textGlitchClip.Length - 1)]);
        
        while (dx <= duration)
        {
            dx += Time.deltaTime;
            float t = Hash(Time.time);
            tempColor.a = t;
            monitorScoreText.color = tempColor;
            yield return null;
        }

        tempColor.a = 1;
        monitorScoreText.color = tempColor;
        monitorScoreText.text = CurrentScore < 10 ? CurrentScore.ToString().PadLeft(2, '0') : CurrentScore.ToString();
    }

    public void FlickerBorder()
    {
        StartCoroutine(FlickerBorderCoroutine());
    }
    
    private IEnumerator FlickerBorderCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(2.8f, 4.5f));
        
        float dx = 0;
        float duration = Random.Range(0.24f, 0.36f);
        Color tempBorderColor = glowBorderRenderer.color;
        Color tempGridColor = gridRenderer.color;
        float borderAlpha = tempBorderColor.a;
        float gridAlpha = tempGridColor.a;
        
        while (dx <= duration)
        {
            dx += Time.deltaTime;
            float t = Hash(Time.time);
            
            float borderAlphaOffset = Mathf.Lerp(borderAlpha - 0.15f, borderAlpha + 0.18f, t);
            float gridAlphaOffset = Mathf.Lerp(gridAlpha - 0.07f, gridAlpha + 0.08f, t);
            
            tempGridColor.a = gridAlphaOffset;
            tempBorderColor.a = borderAlphaOffset;
            
            glowBorderRenderer.color = tempBorderColor;
            gridRenderer.color = tempGridColor;
            yield return null;
        }
        tempBorderColor.a = borderAlpha;
        tempGridColor.a = gridAlpha;
        glowBorderRenderer.color = tempBorderColor;
        gridRenderer.color = tempGridColor;

        yield return FlickerBorderCoroutine();
    }
    
    public static float Hash(float x)
    {
        int n = Mathf.FloorToInt(x * 1000f);
        n = (n << 13) ^ n;
        return 1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f;
    }
}