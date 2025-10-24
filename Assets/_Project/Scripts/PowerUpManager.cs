using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;
    public List<PowerUp> powerUps;
    public float spawnInterval = 3f;
    public Transform spawnArea; // Define area for spawning
    public float spawnRadius = 4; // Spawn range

    private bool canSpawn = true;
    private PowerUp activePowerUp;
    private Coroutine spawnCoroutine;
    
    private readonly float defaultPaddleLength = 1.4f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Init()
    {
        PongBoard.instance.OnGameStarted += StartPowerUpSpawn;
        PongBoard.instance.OnGameEnded += StopPowerUpSpawn;
    }

    public void StartPowerUpSpawn()
    {
        if(spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnPowerUps());

    }

    public void StopPowerUpSpawn()
    {
        if(spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        ClearRemainingPowerUpsAndItsEffects();
    }

    private void ClearRemainingPowerUpsAndItsEffects()
    {
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            activePowerUp = null;
        }
        Ball.EnableMagnet(false);
        Time.timeScale = 1;
        PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(defaultPaddleLength);
        PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(defaultPaddleLength);
    }

    private IEnumerator SpawnPowerUps()
    {
        while (true)
        {
            if (canSpawn && activePowerUp == null)
            {
                yield return new WaitForSeconds(spawnInterval);

                PowerUp randomPowerUp = powerUps[Random.Range(0, powerUps.Count)];
                Vector2 spawnPos = (Vector2)spawnArea.position + Random.insideUnitCircle * spawnRadius;
                activePowerUp = Instantiate(randomPowerUp, spawnPos, Quaternion.identity);

                activePowerUp.transform.localScale = Vector3.one * 0.2f;
                Tween scaleUpTween = activePowerUp.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack,3f);
                yield return scaleUpTween.WaitForCompletion();
                
                activePowerUp.Init();
            }
            yield return null;
        }
    }

    public void DestroyPowerUpAfterDuration(PowerUp powerUp)
    {
        if (activePowerUp != null)
        {
            Destroy(activePowerUp.gameObject);
            activePowerUp = null;
        }
    }

    public void ActivatePowerUp(PowerUpData data)
    {
        switch (data.type)
        {
            case PowerUpType.SlowTime:
                StartCoroutine(SlowTime(data.duration));
                break;
            case PowerUpType.LongPaddle:
                StartCoroutine(LongPaddle(data.duration));
                break;
            case PowerUpType.Magnet:
                StartCoroutine(MagnetEffect(data.duration));
                break;
            case PowerUpType.MultipleBalls:
                StartCoroutine(MultiplePongBalls(data.duration));
                break;
        }
    }

    private IEnumerator LongPaddle(float duration)
    {
        float currLength = 0;
        float powerUpValue = 3f;
        Tween scaleUpTween = DOTween.To(x => currLength = x,defaultPaddleLength,powerUpValue, 1f).SetEase(Ease.OutQuad);
        scaleUpTween.onUpdate += () =>
        {
            PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
            PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
        };
        yield return scaleUpTween.WaitForCompletion();
        yield return new WaitForSeconds(duration);
        
        Tween scaleDownTween = DOTween.To(x => currLength = x,powerUpValue,defaultPaddleLength, 0.8f).SetEase(Ease.OutQuad);
        scaleDownTween.onUpdate += () =>
        {
            PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
            PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(currLength);
        };
    }

    private IEnumerator SlowTime(float duration)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    private IEnumerator MagnetEffect(float duration)
    {
        Ball.EnableMagnet(true);
        yield return new WaitForSeconds(duration);
        Ball.EnableMagnet(false);
    }
    
    private IEnumerator MultiplePongBalls(float duration)
    {
        // Ball[] balls = FindObjectsOfType<Ball>();
        // foreach (var ball in balls) ball.EnableMagnet(true);
        Vector2 currVelocity = PongBoard.instance.currentBall.currVelocity.normalized;
        Vector3 spawnPos = PongBoard.instance.currentBall.transform.position;
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(0.2f);
            Vector2 rndVelocity = Utilities.GetRandomizedVelocity(currVelocity,30);
            PongBoard.instance.SpawnNewBallWithVelocity(rndVelocity,spawnPos);
        }
        yield return new WaitForSeconds(duration);
        // foreach (var ball in balls) ball.EnableMagnet(false);
    }
}