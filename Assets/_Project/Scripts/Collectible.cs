using DG.Tweening;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private readonly float collectDistance = 0.25f;
    private bool isGettingCollected;
    private bool isAcquired = false;
    private Transform target;
    private Vector3 targetPos;

    private float moveSpeed;
    private float initialDistance;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    
    public Vector3 LocalScale
    {
        get => transform.localScale;
        set => transform.localScale = value;
    }
    
    public void Init()
    {
        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).From(Vector3.zero);
    }

    private void Update()
    {
        if (!isGettingCollected) return;

        if (target != null) targetPos = target.position;
        
        Vector3 direction = targetPos - Position;
        Vector3 normalizedDirection = direction.normalized;
        float currentDistance = direction.magnitude;
        Position += normalizedDirection * (moveSpeed * Time.deltaTime);
        float scaleFactor = Mathf.Clamp(currentDistance / initialDistance,0.2f,1f);
        LocalScale = new Vector3(scaleFactor, scaleFactor, 1);

        if (currentDistance < collectDistance && !isAcquired)
        {
            Collect();
        }
    }

    public void ActivateCollectBehaviour(Ball ball)
    {
        target = ball.transform;
        moveSpeed = ball.moveSpeed * 1.2f;
        initialDistance = (target.position - Position).magnitude;
        isGettingCollected = true;
    }

    private void Collect()
    {
        isAcquired = true;
        GameManager.instance.coinManagerSo.AddCoins(1);
        Tween tween = transform.DOScale(Vector3.zero, 0.24f);
        tween.onComplete += () =>
        {
            Destroy(gameObject);
        };
    }
}
