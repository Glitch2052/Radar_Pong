using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "Pong Power-Ups/PowerUps")]
public class PowerUpData : ScriptableObject
{
    public PowerUpType type;
    public float duration = 5f; // Default duration
}

public enum PowerUpType
{
    SlowTime,
    LongPaddle,
    Magnet,
    MultipleBalls,
}
