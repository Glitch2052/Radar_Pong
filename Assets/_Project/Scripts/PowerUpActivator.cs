using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpActivator : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image timerImage;

    private PowerUpType powerUpType;
    private Coroutine coroutine;
    private float elapsedDuration = 0;
    private float totalDuration = 0;
    
    private readonly float defaultPaddleLength = 1.4f;


    public event Action OnTimerStartAction;
    public event Action OnTimerEndAction;


    public void Init(PowerUpType powerUpType)
    {
        this.powerUpType = powerUpType;
        Sprite sprite = PongBoard.instance.powerUpManager.GetPowerUpSpriteIcon(powerUpType);
        bgImage.sprite = timerImage.sprite = sprite;
    }
    
    public void StartDisplayTimer(float duration, Ball ball)
    {
        if(coroutine != null)
            StopCoroutine(coroutine);

        elapsedDuration = 0;
        totalDuration = duration;
        coroutine = StartCoroutine(UpdateTimerUI(totalDuration, ball));
    }

    public void AddDurationToTimer(float duration, Ball ball)
    {
        totalDuration += duration;
        elapsedDuration /= totalDuration;

        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(UpdateTimerUI(totalDuration, ball,true));
    }

    private IEnumerator UpdateTimerUI(float duration, Ball ball ,bool isPowerUpActive = false)
    {
        if (powerUpType == PowerUpType.MultipleBalls)
        {
            OnTimerStartAction?.Invoke();
            yield return SpawnMultiplePongBalls(ball);
            OnTimerEndAction?.Invoke();
            coroutine = null;
            Destroy(gameObject);
        }
        
        float scaleDuration = 0.3f;
        if(!isPowerUpActive)
        {
            transform.localScale = Vector3.zero;
            
            StartPowerUpEffect();
            OnTimerStartAction?.Invoke();
            
            yield return transform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack).SetUpdate(true)
            .WaitForCompletion();
            //here the scale up duration is reduced so that the whole duration is included with scaling animation
            duration -= scaleDuration;
        }

        //here the scale down duration is reduced so that the whole duration is included with scaling animation
        duration -= scaleDuration;

        while (elapsedDuration <= 1)
        {
            if(!GameManager.instance.IsPaused)
                elapsedDuration += Time.unscaledDeltaTime / duration;
            
            timerImage.fillAmount = 1 - elapsedDuration;
            yield return null;
        }

        yield return transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack).SetUpdate(true)
            .WaitForCompletion();
        EndPowerUpEffect();
        
        OnTimerEndAction?.Invoke();
        coroutine = null;
        Destroy(gameObject);
    }

    private void StartPowerUpEffect()
    {
        switch (powerUpType)
        {
            case PowerUpType.SlowTime:
                Time.timeScale = 0.5f;
                break;
            case PowerUpType.LongPaddle:
                float currLength = 0;
                float powerUpValue = 3f;
                Tween scaleUpTween = DOTween.To(x => currLength = x,defaultPaddleLength,powerUpValue, 1f).SetUpdate(true).SetEase(Ease.OutQuad);
                scaleUpTween.onUpdate += () =>
                {
                    PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
                    PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
                };
                break;
            case PowerUpType.Magnet:
                Ball.EnableMagnet(true);
                break;
            case PowerUpType.MultipleBalls:
                // StartCoroutine(SpawnMultiplePongBalls());
                break;
        }
    }

    private void EndPowerUpEffect()
    {
        switch (powerUpType)
        {
            case PowerUpType.SlowTime:
                Time.timeScale = 1;
                break;
            case PowerUpType.LongPaddle:
                float powerUpValue = 3f;
                float currLength = powerUpValue;
                Tween scaleDownTween = DOTween.To(x => currLength = x,powerUpValue,defaultPaddleLength, 0.8f).SetUpdate(true).SetEase(Ease.OutQuad);
                scaleDownTween.onUpdate += () =>
                {
                    PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
                    PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
                };
                break;
            case PowerUpType.Magnet:
                Ball.EnableMagnet(false);
                break;
            case PowerUpType.MultipleBalls:
                break;
        }
    }
    
    private IEnumerator SpawnMultiplePongBalls(Ball ball)
    {
        Vector2 currVelocity = ball.currVelocity.normalized;
        Vector3 spawnPos = ball.transform.position;
        float moveSpeed = ball.moveSpeed;
        for (int i = 0; i < 2; i++)
        {
            yield return Utilities.WaitGameplaySeconds(0.2f);
            Vector2 rndVelocity = Utilities.GetRandomizedVelocity(currVelocity,30);
            PongBoard.instance.SpawnNewBallWithVelocity(rndVelocity,spawnPos, moveSpeed);
        }
    }
}