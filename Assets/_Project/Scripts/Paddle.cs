using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Paddle : MonoBehaviour
{
    public PaddleType paddleType;
    public ProceduralPaddleGen paddleMesh;
    public Transform meshTransform;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private static readonly int AlphaIntensityFade2 = Shader.PropertyToID("_AlphaIntensity_Fade_2");
    public Vector2 tangentialVelocity { get; private set; }
    
    [NonSerialized] private UIKnob knob;

    public void Init(UIKnob uiKnob)
    {
        knob = uiKnob;
    }

    public void OnCollidedWithBall()
    {
        spriteRenderer.DOKill(true);
        spriteRenderer.material.SetFloat(AlphaIntensityFade2,Random.Range(1.28f,1.4f));
        spriteRenderer.material.DOFloat(1f, AlphaIntensityFade2, 0.6f).SetEase(Ease.InQuad);
        CalculateTangentialVelocityOnCollision();
    }

    private void CalculateTangentialVelocityOnCollision()
    {
        Vector2 center = PongBoard.instance.gameTransform.position;
        Vector2 paddlePos = meshTransform.position;

        float radius = Vector2.Distance(center, paddlePos);

        // angular speed (deg/sec) -> rad/sec
        float omega = knob.AngularVelocity * Mathf.Deg2Rad;

        // tangential speed = omega * radius
        float tangentialSpeed = omega * radius;

        // tangential direction is perpendicular to radius
        Vector2 radialDir = (paddlePos - center).normalized;
        Vector2 tangent = new Vector2(-radialDir.y, radialDir.x); // choose direction (CW/CCW)

        tangentialVelocity = tangent * tangentialSpeed;
        Debug.Log($"tangential velocity on collision is {tangentialVelocity}");
    }
}
