using System.Collections;
using TMPro;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private float innerSpawnRadius;
    [SerializeField] private float outerSpawnRadius;
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private Collectible collectiblePrefab;
    [SerializeField] private TextMeshPro starCountText;
    [SerializeField] private Transform starIcon;

    private Collectible currentCollectible;
    private Coroutine collectibleCoroutine;
    
    public void Init()
    {
        PongBoard.instance.OnGameStarted += StartCollectibleSpawnerLoop;
        PongBoard.instance.OnGameContinued += ResumeCollectibleSpawnerLoop;
        PongBoard.instance.OnGameEnded += StopCollectibleSpawnerLoop;
    }

    public void StartCollectibleSpawnerLoop()
    {
        StopCollectibleSpawnerLoop();
        collectibleCoroutine = StartCoroutine(CollectibleSpawnerLoop(true));
    }

    public void ResumeCollectibleSpawnerLoop()
    {
        collectibleCoroutine = StartCoroutine(CollectibleSpawnerLoop(false));
    }
    
    public void StopCollectibleSpawnerLoop()
    {
        if(collectibleCoroutine != null)
            StopCoroutine(collectibleCoroutine);
        
        ClearUncollectedCollectibles();
    }

    private IEnumerator CollectibleSpawnerLoop(bool waitForInitialDelay)
    {
        if (waitForInitialDelay)
        {
            int startSpawnScore = Random.Range(4, 6);
            yield return new WaitUntil(() => PongBoard.instance.CurrentScore > startSpawnScore);
        }
        while (true)
        {
            if (currentCollectible == null)
            {
                yield return Utilities.WaitGameplaySeconds(Random.Range(1.25f, 2.5f));
                
                Vector2 spawnPos = (Vector2)spawnTransform.position + Utilities.GetRandomPointInRing(innerSpawnRadius,outerSpawnRadius);
                currentCollectible = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
                currentCollectible.Init();
            }
            yield return null;
        }
    }

    public void OnCollectibleTriggeredByBall(Collectible collectible, Ball ball)
    {
        collectible.ActivateCollectBehaviour(ball);
        currentCollectible = null;
    }

    public void ClearUncollectedCollectibles()
    {
        if (currentCollectible != null)
        {
            Destroy(currentCollectible.gameObject);
            currentCollectible = null;
        }
    }
}
