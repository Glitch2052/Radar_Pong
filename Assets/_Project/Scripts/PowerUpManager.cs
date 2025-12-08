using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PowerUpManager : MonoBehaviour
{
    public List<PowerUp> powerUps;
    public float startDelay = 0f;
    public float baseInterval = 15f;
    public Vector2 spawnIntervalRange;
    
    public Transform spawnArea; // Define area for spawning
    public float spawnRadius = 4; // Spawn range
    [SerializeField] private Transform uiDisplayParentTransform;
    [SerializeField] private PowerUpActivator powerUpActivatorPrefab;

    private Dictionary<PowerUpType, PowerUpActivator> powerUpDisplayTimerDictionary = new ();
    private Dictionary<PowerUpType, float> activePowerUps = new ();
    private PowerUp currentPowerUp;
    // private List<PowerUp> spawnedPowerUps = new();

    private bool canSpawn = true;
    private Coroutine spawnCoroutine;

    private readonly float defaultPaddleLength = 1.4f;

    [SerializeField] private bool testPowerUps;

    public void Init()
    {
        PongBoard.instance.OnGameStarted += StartPowerUpSpawn;
        PongBoard.instance.OnGameContinued += ResumePowerUpSpawn;
        PongBoard.instance.OnGameEnded += StopPowerUpSpawn;
        
        powerUpDisplayTimerDictionary.Clear();
        // spawnedPowerUps.Clear();
    }

    public void StartPowerUpSpawn()
    {
        if(spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnPowerUpLoop(true));

    }

    public void ResumePowerUpSpawn()
    {
        if(spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnPowerUpLoop(false));
    }

    public void StopPowerUpSpawn()
    {
        if(spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        ClearRemainingPowerUpsAndItsEffects();
    }

    private void ClearRemainingPowerUpsAndItsEffects()
    {
        foreach (KeyValuePair<PowerUpType,PowerUpActivator> keyValuePair in powerUpDisplayTimerDictionary)
        {
            if(keyValuePair.Key == PowerUpType.MultipleBalls) continue;
            Destroy(keyValuePair.Value.gameObject);
        }

        if (currentPowerUp != null)
        {
            Destroy(currentPowerUp.gameObject);
            currentPowerUp = null;
        }

        // foreach (var powerUp in spawnedPowerUps)
        // {
        //     Destroy(powerUp.gameObject);
        // }
        
        powerUpDisplayTimerDictionary.Clear();
        // spawnedPowerUps.Clear();
        
        Ball.EnableMagnet(false);
        Time.timeScale = 1;
        PongBoard.instance.leftController.controlledPaddle.paddleMesh.UpdatePaddleLength(defaultPaddleLength);
        PongBoard.instance.rightController.controlledPaddle.paddleMesh.UpdatePaddleLength(defaultPaddleLength);
    }

    private IEnumerator SpawnPowerUpLoop(bool waitForInitialDelay)
    {
        if(waitForInitialDelay)
            yield return Utilities.WaitGameplaySeconds(startDelay);
        
        while (true)
        {
            if (canSpawn && currentPowerUp == null)
            {
#if UNITY_EDITOR
                yield return Utilities.WaitGameplaySeconds(1f);
#else
                yield return Utilities.WaitGameplaySeconds(GetAdaptiveInterval());
#endif
                
                PowerUp randomPowerUp = GetWeightedPowerUp(); /*powerUps[Random.Range(0, powerUps.Count)]*/
                Vector2 clampedRandomPoint = Random.insideUnitCircle * spawnRadius;
                Vector2 spawnPos = (Vector2)spawnArea.position + clampedRandomPoint;
                currentPowerUp = Instantiate(randomPowerUp, spawnPos, Quaternion.identity);
                // spawnedPowerUps.Add(currentPowerUp);
                currentPowerUp.Init();

                currentPowerUp.transform.localScale = Vector3.one * 0.2f;
                Tween scaleUpTween = currentPowerUp.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBack,3f);
                yield return scaleUpTween.WaitForCompletion();
            }
            yield return null;
        }
    }

    private float GetAdaptiveInterval()
    {
        int score = PongBoard.instance.CurrentScore;
        float interval = baseInterval;
        interval *= Mathf.Clamp(1 - Mathf.Log10(1 + score) * 0.08f, 0.6f, 1f);
        interval = Mathf.Max(5f, interval);
        return interval;
    }

    private PowerUp GetWeightedPowerUp()
    {
        float total = 0f;
        foreach (var p in powerUps) total += p.data.rarityWeight;
        float r = Random.value * total;
        float acc = 0f;
        foreach (var powerUp in powerUps)
        {
            acc += powerUp.data.rarityWeight;
            if (r <= acc) return powerUp;
        }
        return null;
    }

    public void RemovePowerUpOnCollectOrExpire(PowerUp powerUp)
    {
        if(currentPowerUp != null && currentPowerUp == powerUp)
        {
            Destroy(powerUp.gameObject);
            currentPowerUp = null;
        }
    }

    public void CollectAndActivatePowerUp(PowerUp powerUp, Ball ball)
    {
        var data = powerUp.data;
        ActivatePowerUpWithUiTimer(data, ball);
        RemovePowerUpOnCollectOrExpire(powerUp);
    }

    public Sprite GetPowerUpSpriteIcon(PowerUpType powerUpType)
    {
        foreach (PowerUp powerUp in powerUps)
        {
            if (powerUp.data.type == powerUpType) return powerUp.data.sprite;
        }
        return null;
    }

    private void ActivatePowerUpWithUiTimer(PowerUpData data, Ball ball)
    {
        if (powerUpDisplayTimerDictionary.TryGetValue(data.type, out PowerUpActivator uiTimer))
        {
            uiTimer.AddDurationToTimer(data.baseDuration, ball);
            return;
        }

        uiTimer = Instantiate(powerUpActivatorPrefab, uiDisplayParentTransform);
        
        powerUpDisplayTimerDictionary.Add(data.type, uiTimer);
        
        uiTimer.Init(data.type);
        uiTimer.StartDisplayTimer(data.baseDuration,ball);
        uiTimer.OnTimerEndAction += () =>
        {
            powerUpDisplayTimerDictionary.Remove(data.type);
        };
    }
}