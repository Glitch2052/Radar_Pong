using System;
using System.Collections;
using Lofelt.NiceVibrations;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float moveSpeed;
    private Rigidbody2D rigidbody2D;
    [SerializeField] private SpriteRenderer graphicRenderer;
    [SerializeField] private ParticleSystem collideVfx;

    private static int ballCount;
    private readonly float maxSpeed = 7.2f;

    public event Action<PaddleType, Vector2> OnCollidedWithPaddle;
    public event Action<Ball> OnDestroyed;

    public Vector2 currVelocity => rigidbody2D.linearVelocity;

    private Paddle currentPaddle;
    readonly float detectionAngle = 90f;
    private static bool isMagneticMovement;

    public void Init()
    {
        TryGetComponent(out rigidbody2D);
        ballCount++;
    }

    private void Update()
    {
        if(!isMagneticMovement) return;
        
        foreach (var paddle in PongBoard.instance.allPaddles)
        {
            Vector2 velocity = rigidbody2D.linearVelocity.normalized;
            Vector2 toPaddle = (paddle.meshTransform.transform.position - transform.position);
            float angle = Vector2.Angle(velocity, toPaddle);

            if (angle <= detectionAngle * 0.5f/* && toPaddle.sqrMagnitude <= detectionDistance*/)
            {
                currentPaddle = paddle;
                break;
            }
        }

        if (currentPaddle == null) return;
        Vector2 desiredDir = (currentPaddle.meshTransform.transform.position - transform.position).normalized;
        rigidbody2D.linearVelocity = Vector2.Lerp(rigidbody2D.linearVelocity.normalized, desiredDir, 5f * Time.deltaTime).normalized * moveSpeed;
    }

    public void StartRandomBallMovement()
    {
        StartCoroutine(StartMovement());
    }
    
    IEnumerator StartMovement()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        yield return new WaitForSeconds(1f);
        Vector2 velocity = new Vector2(Random.Range(0f, 1f) <= 0.5f ? 1f : -1f, Random.Range(-0.32f, 0.32f)).normalized;
        rigidbody2D.linearVelocity = velocity * moveSpeed;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void SetBallVelocity(Vector2 newVelocity)
    {
        rigidbody2D.linearVelocity = newVelocity * moveSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Paddle") && other.gameObject.transform.parent.TryGetComponent(out Paddle paddle))
        {
            currentPaddle = null;
            paddle.OnCollidedWithBall();
            moveSpeed += 0.01f;
            moveSpeed = Mathf.Min(maxSpeed, moveSpeed);
            rigidbody2D.linearVelocity = GetPaddleAffectedBallVelocity(paddle, other); //rigidbody2D.linearVelocity.normalized * moveSpeed;
            OnCollidedWithPaddle?.Invoke(paddle.paddleType,other.relativeVelocity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectible"))
        {
            if(collision.gameObject.TryGetComponent(out PowerUp powerUp))
                PongBoard.instance.powerUpManager.CollectAndActivatePowerUp( powerUp,this);
            if(collision.gameObject.TryGetComponent(out Collectible collectible))
                PongBoard.instance.collectibleManager.OnCollectibleTriggeredByBall(collectible, this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.gameObject.CompareTag("FailBoundary")) return;
        Instantiate(collideVfx,transform.position,Quaternion.Euler(0,0,Vector2.SignedAngle(Vector2.right,-rigidbody2D.linearVelocity)));
        PongBoard.instance.ShakeMonitorCamera();
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
        GameManager.instance.PlayOneShot(PongBoard.instance.ballDestroyClip);
        
        ballCount--;
        ballCount = Mathf.Max(0, ballCount);
        
        if(ballCount == 0)
            PongBoard.instance.EndGame();

        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    private Vector2 GetPaddleAffectedBallVelocity(Paddle paddle,Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        Vector2 incoming = rigidbody2D.linearVelocity;

        // Normal reflection
        Vector2 reflected = Vector2.Reflect(incoming, normal);

        // Get paddle velocity
        Vector2 paddleVel = paddle.tangentialVelocity;

        // Tangent along paddle surface (perpendicular to normal)
        Vector2 tangent = new Vector2(-normal.y, normal.x);
        float tangentialSpeed = Vector2.Dot(paddleVel, tangent);
        Vector2 tangentialComponent = tangent * tangentialSpeed;
        Vector2 modifiedDir = reflected + (tangentialComponent * 0.15f);

        if (modifiedDir.sqrMagnitude < 0.001f)
            modifiedDir = reflected;
        
        modifiedDir.Normalize();
        
        float dot = Vector2.Dot(modifiedDir, normal);

        // if dot <= 0, we’re moving inward / along the surface → fix it
        if (dot <= 0f)
        {
            // reflect across the normal so we are definitely going outward
            modifiedDir = Vector2.Reflect(modifiedDir, normal);
        }
        
        return modifiedDir * moveSpeed;
    }

    public static void ResetBallCount()
    {
        ballCount = 0;
    }

    public static void EnableMagnet(bool value)
    {
        isMagneticMovement = value;
    }
}
