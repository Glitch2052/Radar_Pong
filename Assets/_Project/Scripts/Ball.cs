using System;
using System.Collections;
using DG.Tweening;
using Lofelt.NiceVibrations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rigidbody2D;
    [SerializeField] private SpriteRenderer graphicRenderer;
    [SerializeField] private ParticleSystem collideVfx;

    public event Action<PaddleType, Vector2> OnCollidedWithPaddle;
    public event Action OnDestroyed;

    public void Init()
    {
        TryGetComponent(out rigidbody2D);
        StartCoroutine(StartMovement());
    }
    
    IEnumerator StartMovement()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        yield return new WaitForSeconds(1f);
        Vector2 velocity = new Vector2(Random.Range(0f, 1f) <= 0.5f ? 1f : -1f, Random.Range(-0.32f, 0.32f)).normalized;
        rigidbody2D.linearVelocity = velocity * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Paddle") && other.gameObject.transform.parent.TryGetComponent(out Paddle paddle))
        {
            paddle.OnCollidedWithBall();
            moveSpeed += 0.025f;
            rigidbody2D.linearVelocity = rigidbody2D.linearVelocity.normalized * moveSpeed;
            OnCollidedWithPaddle?.Invoke(paddle.paddleType,other.relativeVelocity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Instantiate(collideVfx,transform.position,Quaternion.Euler(0,0,Vector2.SignedAngle(Vector2.right,-rigidbody2D.linearVelocity)));
        PongBoard.instance.monitorCamera.transform.DOPunchPosition(Random.insideUnitCircle * 0.5f, 0.7f);
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        GameManager.instance.StopGameMusic();
        GameManager.instance.PlayOneShot(PongBoard.instance.ballDestroyClip);
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
