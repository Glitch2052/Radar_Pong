using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PowerUpData data;

    private float timer;
    private readonly float blinkDuration = 2f;
    private static readonly float StayDuration = 8f;
    private bool isInitialized;
    private float elapsedTImer;

    public void Init()
    {
        timer = StayDuration;
        isInitialized = true;
        elapsedTImer = 0;
    }
    
    private void Update()
    {
        if(!isInitialized) return;
        
        timer -= Time.deltaTime;
        if (timer <= 0f)  // Start blinking phase
        {
            // How long we’ve been in blinking mode
            elapsedTImer += Time.deltaTime;
            float blinkTime = Mathf.Abs(timer);
            if (blinkTime < blinkDuration)
            {
                // Blink frequency increases over time
                float blinkSpeed = Mathf.Lerp(3f, 9f, blinkTime / blinkDuration);
                float alpha = Mathf.PingPong(elapsedTImer * blinkSpeed, 1f) > 0.5f ? 1f : 0f;
                spriteRenderer.enabled = alpha > 0f;
            }
            else
            {
                // Destroy after blink duration
                PowerUpManager.Instance.DestroyPowerUpAfterDuration(this);
            }
        }
    }
}