using DG.Tweening;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    public PaddleType paddleType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private static readonly int AlphaIntensityFade2 = Shader.PropertyToID("_AlphaIntensity_Fade_2");

    public void OnCollidedWithBall()
    {
        spriteRenderer.DOKill(true);
        spriteRenderer.material.SetFloat(AlphaIntensityFade2,Random.Range(1.28f,1.4f));
        spriteRenderer.material.DOFloat(1f, AlphaIntensityFade2, 0.6f).SetEase(Ease.InQuad);
    }
}
