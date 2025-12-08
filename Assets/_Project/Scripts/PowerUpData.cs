using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "Pong Power-Ups/PowerUps")]
public class PowerUpData : ScriptableObject
{
    public PowerUpType type;
    public Sprite sprite;
    public float baseDuration = 5f; // Default duration
    public float rarityWeight;
}

public enum PowerUpType
{
    SlowTime,
    LongPaddle,
    Magnet,
    MultipleBalls,
}
