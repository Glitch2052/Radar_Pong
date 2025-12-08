using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PowerUpData data;

    private float timer;
    private readonly float blinkDuration = 2f;
    private static readonly float StayDuration = 8f;
    private bool isInitialized;
    private float elapsedBlinkingTimer;

    public void Init()
    {
        timer = StayDuration;
        isInitialized = true;
        elapsedBlinkingTimer = 0;
    }
    
    private void Update()
    {
        if(!isInitialized) return;
        
        timer -= Time.deltaTime;
        if (timer <= 0f)  // Start blinking phase
        {
            // How long we’ve been in blinking mode
            elapsedBlinkingTimer += Time.deltaTime;
            float blinkTime = Mathf.Abs(timer);
            if (blinkTime < blinkDuration)
            {
                // Blink frequency increases over time
                float blinkSpeed = Mathf.Lerp(3f, 9f, blinkTime / blinkDuration);
                float alpha = Mathf.PingPong(elapsedBlinkingTimer * blinkSpeed, 1f) > 0.5f ? 1f : 0f;
                spriteRenderer.enabled = alpha > 0f;
            }
            else
            {
                // Destroy after blink duration
                PongBoard.instance.powerUpManager.RemovePowerUpOnCollectOrExpire(this);
            }
        }
    }
}