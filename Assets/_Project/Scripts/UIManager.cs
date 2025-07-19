using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lofelt.NiceVibrations;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Space(20), Header("SplashScreen")]
    [SerializeField] private SpriteRenderer logoRenderer;
    [SerializeField] private TextMeshPro logoText;

    [Space(20), Header("Game UI Data")] 
    [SerializeField] private AudioClip uiTapClip;
    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private RectTransform monitorTransform;
    [SerializeField] private RectTransform leftPaddleTransform;
    [SerializeField] private RectTransform rightPaddleTransform;
    
    [Space(20),Header("Menu UI Data")] 
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private RectTransform settingsPanel;
    [SerializeField] private RectTransform quitPanel;
    [SerializeField] private RectTransform pausePanel;

    [SerializeField]private CanvasGroup menuCanvasGroup;
    [SerializeField]private CanvasGroup settingsCanvasGroup;
    [SerializeField]private CanvasGroup quitCanvasGroup;
    [SerializeField] private CanvasGroup pauseCanvasGroup;

    private readonly float moveDistance = 120f;
    private readonly float fadeOutDuration = 0.3f;
    private readonly float fadeInDuration = 0.5f;
    
    private Dictionary<string, CanvasGroup> panelMap;
    private readonly Stack<string> panelHistory = new Stack<string>();

    private Vector2 monitorPlayModeScreenPos;
    private Vector2 leftPaddlePlayModeScreenPos;
    private Vector2 rightPaddlePlayModeScreenPos;
    
    public bool IsSfxEnabled { get; private set; }
    public bool IsBgmEnabled { get; private set; }
    public event Action<bool> OnBgmToggleAction;
    
    public void Init()
    {
        panelMap = new Dictionary<string, CanvasGroup>()
        {
            {StringID.Main, menuCanvasGroup},
            {StringID.Settings, settingsCanvasGroup},
            {StringID.Pause, pauseCanvasGroup},
            {StringID.Quit, quitCanvasGroup}
        };

        foreach (var kvp in panelMap)
        {
            kvp.Value.alpha = 0;
            kvp.Value.interactable = false;
            kvp.Value.blocksRaycasts = false;
            kvp.Value.gameObject.SetActive(false);
        }

        SetDefaultToggleValue();
        
        SetMonitorSizeAndAnchorToCenter();
    }

    private void SetUpGameUIForStart()
    {
        HideMainPanelOnGameStart();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(monitorTransform.DOAnchorPosY(monitorPlayModeScreenPos.y, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.Join(leftPaddleTransform.DOAnchorPosX(leftPaddlePlayModeScreenPos.x, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.Join(rightPaddleTransform.DOAnchorPosX(rightPaddlePlayModeScreenPos.x, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.onComplete += () =>
        {
            GameManager.instance.StartGame();
        };
    }

    public void PauseGame(Action onFadeInStartAction)
    {
        pauseCanvasGroup.gameObject.SetActive(true);
        Tween tween = pauseCanvasGroup.DOFade(1, fadeInDuration).SetDelay(1f);
        tween.OnStart(() => onFadeInStartAction?.Invoke());
        tween.onComplete += () =>
        {
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        };
        // Vector2 paddleSize = leftPaddleTransform.rect.size;
        //
        // Sequence sequence = DOTween.Sequence();
        // sequence.Append(monitorTransform.DOAnchorPosY(0, fadeInDuration)
        //     .SetEase(Ease.InOutSine));
        // sequence.Join(leftPaddleTransform.DOAnchorPosX(leftPaddlePlayModeScreenPos.x - paddleSize.x, fadeInDuration)
        //     .SetEase(Ease.InOutSine));
        // sequence.Join(rightPaddleTransform.DOAnchorPosX(rightPaddlePlayModeScreenPos.x + paddleSize.x, fadeInDuration)
        //     .SetEase(Ease.InOutSine));
        //
        // ShowMainPanel();
    }
    
    private void SetMonitorSizeAndAnchorToCenter()
    {
        Vector2 screenSize = monitorTransform.rect.size;
        float minSize = screenSize.y;
        var axis = RectTransform.Axis.Horizontal;
        if (screenSize.x < minSize)
        {
            minSize = screenSize.x;
            axis = RectTransform.Axis.Vertical;
        }
        monitorTransform.SetSizeWithCurrentAnchors(axis,minSize);

        Vector2 paddleSize = leftPaddleTransform.rect.size;
        
        monitorPlayModeScreenPos = GetRectTransformCenter(monitorTransform);
        leftPaddlePlayModeScreenPos = GetRectTransformCenter(leftPaddleTransform);
        rightPaddlePlayModeScreenPos = GetRectTransformCenter(rightPaddleTransform);

        monitorTransform.anchorMin = monitorTransform.anchorMax = leftPaddleTransform.anchorMin = 
            leftPaddleTransform.anchorMax = rightPaddleTransform.anchorMin = rightPaddleTransform.anchorMax = Vector2.one * 0.5f;
        monitorTransform.anchoredPosition = Vector2.zero;
        monitorTransform.sizeDelta = Vector2.one * minSize;

        leftPaddleTransform.sizeDelta = rightPaddleTransform.sizeDelta = paddleSize;
        leftPaddleTransform.anchoredPosition = new Vector2(leftPaddlePlayModeScreenPos.x - paddleSize.x, 0);
        rightPaddleTransform.anchoredPosition = new Vector2(rightPaddlePlayModeScreenPos.x + paddleSize.x, 0);
    }

    public void ShowMainPanel()
    {
        panelHistory.Push(StringID.Main);
        menuCanvasGroup.gameObject.SetActive(true);
        menuCanvasGroup.DOFade(1, fadeInDuration).onComplete += () =>
        {
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        };
    }

    private void HideMainPanelOnGameStart()
    {
        panelHistory.Pop();
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
        menuCanvasGroup.DOFade(0, fadeOutDuration).onComplete += () =>
        {
            menuCanvasGroup.gameObject.SetActive(true);
        };
    }
    
    public IEnumerator PlaySplashScreen()
    {
        Sequence fadeInSequence = DOTween.Sequence();

        float startDelay = 0.14f;
        fadeInSequence.Append(logoRenderer.DOFade(1, 0.6f)).SetDelay(startDelay);
        fadeInSequence.Join(logoRenderer.transform.DOLocalMoveY(1.4f, 0.8f).SetDelay(startDelay).SetEase(Ease.OutQuad));
        yield return fadeInSequence.WaitForCompletion();
        
        yield return TypeSplashScreenText(startDelay,0.12f);
        
        Sequence fadeOutSequence = DOTween.Sequence();
        fadeOutSequence.Append(logoRenderer.DOFade(0, 0.6f)).SetDelay(startDelay);
        fadeOutSequence.Join(logoRenderer.transform.DOLocalMoveY(3f, 0.6f).SetDelay(startDelay).SetEase(Ease.InQuad));
        fadeOutSequence.Join(logoText.transform.DOLocalMoveY(-2.4f,0.6f).SetDelay(startDelay).SetEase(Ease.InQuad));
        fadeOutSequence.Join(logoText.DOFade(0f,0.4f).SetDelay(startDelay));
        yield return fadeOutSequence.WaitForCompletion();
    }
    
    IEnumerator TypeSplashScreenText(float startDelay,float delayPerChar)
    {
        string fullText = "AstroRock\nGames";
        logoText.text = "";

        WaitForSeconds waitForSeconds = new WaitForSeconds(delayPerChar);
        yield return new WaitForSeconds(startDelay);
        foreach (var letter in fullText)
        {
            logoText.text += letter;
            yield return waitForSeconds;
        }
    }
    
    public void OnMenuButtonClicked(int index)
    {
        GameManager.instance.PlayOneShot(uiTapClip);
        switch (index)
        {
            case 1:
                SetUpGameUIForStart();
                break;
            case 2:
                TransitionTo(StringID.Settings);
                break;
            case 3:
                break;
            case 4:
                TransitionTo(StringID.Quit);
                break;
        }
    }

    private void TransitionTo(string panelName)
    {
        string current = panelHistory.Count > 0 ? panelHistory.Peek() : StringID.Main;
        
        if(panelName == current) return;
        
        panelHistory.Push(panelName);
        StartCoroutine(AnimatePanels(panelMap[current], panelMap[panelName]));
    }

    private IEnumerator AnimatePanels(CanvasGroup from, CanvasGroup to, bool reverse = false)
    {
        float dir = reverse ? 1 : -1;

        from.interactable = false;
        from.blocksRaycasts = false;

        from.transform.DOLocalMoveX(moveDistance * dir, fadeOutDuration).SetEase(Ease.InOutQuad);
        from.DOFade(0, fadeOutDuration);

        to.transform.localPosition = new Vector3(-(moveDistance + 50) * dir , 0, 0);
        to.gameObject.SetActive(true);
        to.DOFade(1, fadeInDuration);
        to.transform.DOLocalMoveX(0, fadeInDuration).SetEase(Ease.InOutQuad).onComplete += (() =>
        {
            to.interactable = true;
            to.blocksRaycasts = true;
        });
        
        yield return null;
    }

    public void OnBack()
    {
        if(panelHistory.Count < 2) return;

        GameManager.instance.PlayOneShot(uiTapClip);
        string current = panelHistory.Pop();
        string previous = panelHistory.Peek();
        StartCoroutine(AnimatePanels(panelMap[current], panelMap[previous], true));
    }

    public void OpenRateUsPanel()
    {
        
    }

    public void OnRetryButtonClicked()
    {
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
        pauseCanvasGroup.DOFade(0, fadeOutDuration).onComplete += () =>
        {
            GameManager.instance.StartGame();
            pauseCanvasGroup.gameObject.SetActive(false);
        };
    }

    public void OnMainMenuButtonClicked()
    {
        pauseCanvasGroup.interactable = false;
        pauseCanvasGroup.blocksRaycasts = false;
        pauseCanvasGroup.DOFade(0, fadeOutDuration).onComplete += () =>
        {
            pauseCanvasGroup.gameObject.SetActive(false);
        };
        PongBoard.instance.DisableScoreText();
        
        Vector2 paddleSize = leftPaddleTransform.rect.size;

        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(monitorTransform.DOAnchorPosY(0, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.Join(leftPaddleTransform.DOAnchorPosX(leftPaddlePlayModeScreenPos.x - paddleSize.x, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.Join(rightPaddleTransform.DOAnchorPosX(rightPaddlePlayModeScreenPos.x + paddleSize.x, fadeInDuration)
            .SetEase(Ease.InOutSine));
        sequence.onComplete += ShowMainPanel;
    }

    private void SetDefaultToggleValue()
    {
        IsSfxEnabled = PlayerPrefs.GetInt(StringID.SfxEnabled, 1) == 1;
        IsBgmEnabled = PlayerPrefs.GetInt(StringID.BgmEnabled, 1) == 1;
        
        sfxText.text = IsSfxEnabled ? StringID.On : StringID.Off;
        bgmText.text = IsBgmEnabled ? StringID.On : StringID.Off;
        OnBgmToggleAction?.Invoke(IsBgmEnabled);
    }

    public void ToggleSfxSound()
    {
        GameManager.instance.PlayOneShot(uiTapClip);
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        IsSfxEnabled = !IsSfxEnabled;
        sfxText.text = IsSfxEnabled ? StringID.On : StringID.Off;
        PlayerPrefs.SetInt(StringID.SfxEnabled,IsSfxEnabled ? 1 : 0);
    }

    public void ToggleBgmSound()
    {
        GameManager.instance.PlayOneShot(uiTapClip);
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
        IsBgmEnabled = !IsBgmEnabled;
        bgmText.text = IsBgmEnabled ? StringID.On : StringID.Off;
        PlayerPrefs.SetInt(StringID.BgmEnabled,IsBgmEnabled ? 1 : 0);
        OnBgmToggleAction?.Invoke(IsBgmEnabled);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    #region Helper Methods

    private Vector2 GetRectTransformCenter(RectTransform rectTransform)
    {
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;
        
        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null) return rectTransform.localPosition;
        
        Vector2 parentSize = parent.rect.size;

        Vector2 anchorCenter = (anchorMin + anchorMax - Vector2.one) * 0.5f;
        Vector2 anchorPos = new Vector2(anchorCenter.x * parentSize.x, anchorCenter.y * parentSize.y);
        
        Vector2 size = rectTransform.rect.size;
        Vector2 pivotOffset = new Vector2(
            (0.5f - rectTransform.pivot.x) * size.x,
            (0.5f - rectTransform.pivot.y) * size.y
        );
        
        return anchorPos + pivotOffset + rectTransform.anchoredPosition;
    }

    #endregion
}
